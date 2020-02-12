using MG2Connector;
using NetSuiteConnector;
using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectorIntegration
{
    public class OrderInjector
    {
        public OrderCreator orderCreator;
        public SuiteTalkConnector nsConnector;
        public List<OrderInfo> failedUpsertOrders;
        public List<OrderInfo> failedUpsertInvoices;
        public List<OrderInfo> failedCloseOrders;
        public List<OrderInfo> failedGetOrders;
        public List<OrderComparison> orderComparisons;
        public List<OrderInfo> differentAmountOrders;

        public OrderInjector()
        {
            orderCreator = new OrderCreator();
            failedUpsertOrders = new List<OrderInfo>();
            failedUpsertInvoices = new List<OrderInfo>();
            failedCloseOrders = new List<OrderInfo>();
            failedGetOrders = new List<OrderInfo>();
            orderComparisons = new List<OrderComparison>();
            differentAmountOrders = new List<OrderInfo>();
        }

        public void SetNSConnector(SuiteTalkConnector ns)
        {
            nsConnector = ns;
        }

        public void InjectToNS()
        {
            foreach (OrderInfo orderInfo in orderCreator.allOrderInfos)
            {
                if (!orderInfo.IsToInject()) continue;

                Console.WriteLine("---------------");
                Console.WriteLine("Upsert Order #{0}", orderInfo.order_id);

                WriteResponse salesOrderResponse = nsConnector.UpsertRecord(orderInfo.sales_order);

                if (!IsWriteSucceed(salesOrderResponse))
                {
                    orderInfo.invalid_reason += "Upsert SO failed; ";
                    failedUpsertOrders.Add(orderInfo);
                }

                if (orderInfo.invoice != null)
                {
                    WriteResponse invoiceResponse = nsConnector.UpsertRecord(orderInfo.invoice);

                    if (!IsWriteSucceed(invoiceResponse))
                    {
                        orderInfo.invalid_reason += "Upsert Invoice failed; ";
                        failedUpsertInvoices.Add(orderInfo);
                    }
                }

                if (orderInfo.IsToCloseOrder())  // Close invalid order
                {
                    // Get order and close line item
                    orderInfo.sales_order_get = nsConnector.GetSalesOrder(orderInfo.sales_order.externalId);
                    orderInfo.CreateClosedOrder();

                    salesOrderResponse = nsConnector.UpsertRecord(orderInfo.sales_order_close);

                    if (!IsWriteSucceed(salesOrderResponse))
                    {
                        orderInfo.invalid_reason += "Close SO failed; ";
                        failedCloseOrders.Add(orderInfo);
                    }
                }
            }

            Console.WriteLine("\n");
        }

        private bool IsWriteSucceed(WriteResponse writeResponse)
        {
            if (writeResponse == null)
            {
                Console.WriteLine("Upsert Transaction failed!!");
                return false;
            }

            if (writeResponse.status.isSuccess)
            {
                Console.WriteLine("Upsert Transaction Succeed, External ID: {0}", ((RecordRef)writeResponse.baseRef).externalId);
                return true;
            }
            else
            {
                Console.WriteLine("Upsert Transaction failed!!");
                SuiteTalkConnector.displayError(writeResponse.status.statusDetail);
                return false;
            }
        }

        public void GetSOs()
        {
            foreach (OrderInfo orderInfo in orderCreator.allOrderInfos)
            {
                if (!orderInfo.IsToInject()) continue;

                orderInfo.sales_order_get = nsConnector.GetSalesOrder(orderInfo.sales_order.externalId);

                if (orderInfo.sales_order_get == null)
                {
                    failedGetOrders.Add(orderInfo);
                }
            }
        }

        public void CompareSOs()
        {
            foreach (OrderInfo orderInfo in orderCreator.allOrderInfos)
            {
                OrderComparison orderComparison = new OrderComparison()
                {
                    Magento_id = orderInfo.order_id,
                    Comment = orderInfo.invalid_reason,
                    M_total = orderInfo.sum_total
                };

                orderComparisons.Add(orderComparison);

                if (orderInfo.sales_order_get == null || orderInfo.sales_order_get.otherRefNum == null)
                {
                    continue;
                }

                orderComparison.Entity = orderInfo.sales_order_get.entity.name;
                orderComparison.ShippingState = orderInfo.sales_order_get.shippingAddress.state;
                orderComparison.ShipVia = orderInfo.sales_order_get.shipMethod.name;
                orderComparison.TOS_Item = orderInfo.tos_sku;
                orderComparison.Warehouse = orderInfo.warehouse.ToString();
                orderComparison.Default_Warehouse = orderInfo.default_warehouse.ToString();
                orderComparison.NS_id = orderInfo.sales_order_get.tranId;
                orderComparison.NS_Total = orderInfo.sales_order_get.total;

                double diff_amount = orderComparison.M_total - orderComparison.NS_Total;
                orderComparison.Discrepancy = diff_amount.ToString("0.00");

                if (Math.Abs(diff_amount) >= 0.01)
                {
                    differentAmountOrders.Add(orderInfo);
                }
            }
        }

        public void ReportResult()
        {
            var testOrders = orderCreator.allOrderInfos.Where(order => order.orderType == OrderType.Testing).ToList();
            var testInjectOrders = orderCreator.allOrderInfos.Where(order => order.orderType == OrderType.TestInject).ToList();
            var invalidOrders = orderCreator.allOrderInfos.Where(order => order.orderType == OrderType.Invalid).ToList();
            var invalidInjectOrders = orderCreator.allOrderInfos.Where(order => order.orderType == OrderType.InvalidButInject).ToList();
            var ordersToInject = orderCreator.allOrderInfos.Where(order => order.orderType != OrderType.Testing && order.orderType != OrderType.Invalid).ToList();

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) are testing orders ***", testOrders.Count);
            foreach (OrderInfo testOrder in testOrders)
            {
                Console.WriteLine("#{0} by {1} {2}", testOrder.order_id, testOrder.magentoOrder.customer_firstname, testOrder.magentoOrder.customer_lastname);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) are invalid orders ***", invalidOrders.Count);
            foreach (OrderInfo invalidOrder in invalidOrders)
            {
                Console.WriteLine("#{0}  {1}", invalidOrder.order_id, invalidOrder.invalid_reason);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) are testing inject orders ***", testOrders.Count);
            foreach (OrderInfo testInjectOrder in testInjectOrders)
            {
                Console.WriteLine("#{0} by {1} {2}", testInjectOrder.order_id, testInjectOrder.magentoOrder.customer_firstname, testInjectOrder.magentoOrder.customer_lastname);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) are invalid orders but injected ***", invalidInjectOrders.Count);
            foreach (OrderInfo invalidOrder in invalidInjectOrders)
            {
                Console.WriteLine("#{0}  {1}", invalidOrder.order_id, invalidOrder.invalid_reason);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) Created! ***", ordersToInject.Count);

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) Upsert Failed! ***", failedUpsertOrders.Count);
            foreach (OrderInfo failedUpsertOrder in failedUpsertOrders)
            {
                Console.WriteLine(failedUpsertOrder.order_id);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} Invoice(s) Upsert Failed! ***", failedUpsertInvoices.Count);
            foreach (OrderInfo failedUpsertOrder in failedUpsertInvoices)
            {
                Console.WriteLine(failedUpsertOrder.order_id);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) Close Failed! ***", failedCloseOrders.Count);
            foreach (OrderInfo failedCloseOrder in failedCloseOrders)
            {
                Console.WriteLine(failedCloseOrder.order_id);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) Get Failed! ***", failedGetOrders.Count);
            foreach (OrderInfo failedGetOrder in failedGetOrders)
            {
                Console.WriteLine(failedGetOrder.order_id);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} sales order(s) have different amount ***", differentAmountOrders.Count);
            foreach (OrderInfo diffAmountOrder in differentAmountOrders)
            {
                Console.WriteLine(diffAmountOrder.order_id);
            }
            Console.WriteLine("");
        }

        public List<M2Order> GetSuccessOrders()
        {
            var successOrders = new List<M2Order>();

            foreach (OrderInfo orderInfo in orderCreator.allOrderInfos)
            {
                if(!failedGetOrders.Contains(orderInfo))
                {
                    successOrders.Add(orderInfo.magentoOrder);
                }
            }

            return successOrders;
        }

        public class OrderComparison
        {
            public string Entity { get; set; }
            public string ShippingState { get; set; }
            public string ShipVia { get; set; }
            public string TOS_Item { get; set; }
            public string Warehouse { get; set; }
            public string Default_Warehouse { get; set; }
            public string Magento_id { get; set; }
            public string NS_id { get; set; }
            public double M_total { get; set; }
            public double NS_Total { get; set; }
            public string Discrepancy { get; set; }
            public string Comment { get; set; }
        }
    }
}
