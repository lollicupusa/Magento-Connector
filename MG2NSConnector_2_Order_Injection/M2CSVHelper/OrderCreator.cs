using MG2Connector;
using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectorIntegration
{
    public class OrderCreator
    {
        public List<OrderInfo> allOrderInfos;

        public OrderCreator()
        {
            allOrderInfos = new List<OrderInfo>();
        }

        public void InsertMOrders(IList<M2Order> m2Orders)
        {
            foreach (var m2Order in m2Orders)
            {
                OrderInfo orderInfo = new OrderInfo()
                {
                    magentoOrder = m2Order,
                    order_id = m2Order.increment_id, //+ "x"
                    purchase_date = TransformUTCToPST(DateTime.Parse(m2Order.created_at))
                };

                if (orderInfo.purchase_date <= new DateTime(2020, 1, 1))
                {
                    orderInfo.purchase_date = new DateTime(2020, 1, 2);
                }

                allOrderInfos.Add(orderInfo);
            }
        }

        private DateTime TransformUTCToPST(DateTime utcTime)
        {
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, pstZone);
        }

        public void PickToInjectOrders()
        {
            foreach (var orderInfo in allOrderInfos)
            {
                // Skip testing orders
                if (orderInfo.IsTestNotForInject())
                {
                    orderInfo.invalid_reason += "Test Order by " + orderInfo.magentoOrder.customer_firstname + " " + orderInfo.magentoOrder.customer_lastname;
                    orderInfo.orderType = OrderType.Testing;
                }
                else if (orderInfo.IsTestInjectingOrder())
                {
                    orderInfo.invalid_reason += "Test Order by " + orderInfo.magentoOrder.customer_firstname + " " + orderInfo.magentoOrder.customer_lastname;
                    orderInfo.orderType = OrderType.TestInject;
                }
                else
                {
                    orderInfo.orderType = OrderType.Normal;
                }
            }
        }

        public static List<string> testNotInjectAccounts = new List<string>()
        {
            "aaron.liu@lollicup.com",
            "aaron.liu@karatpackaging.com",
            //"shurui91@yahoo.com",
            "wayne.liu@lollicup.com",
            "wayne.liu@karatpackaging.com",
            "chad.chen@lollicup.com",
            "disha.patel@lollicup.com",
            "test@simicart.com"
        };

        public static List<string> testInjectAccounts = new List<string>()
        {
            "shurui91@gmail.com",
        };

        public void FixTotal()
        {
            foreach (var orderInfo in allOrderInfos)
            {
                orderInfo.sum_total = orderInfo.magentoOrder.subtotal + orderInfo.magentoOrder.shipping_amount
                    + orderInfo.magentoOrder.discount_amount + orderInfo.magentoOrder.tax_amount;
            }
        }

        public void FixItems(Dictionary<int, ItemComparator.ItemInfo> magentoProductInfoMap)
        {
            foreach (var orderInfo in allOrderInfos)
            {
                if (orderInfo.orderType == OrderType.NULL || orderInfo.orderType == OrderType.Testing) continue;

                orderInfo.ExtractItemList(magentoProductInfoMap);

                if (!orderInfo.isAllItemsValid())
                {
                    orderInfo.orderType = OrderType.Invalid;
                    orderInfo.invalid_reason += "Item(s) not valid; ";
                    continue;
                }

                // Sort item list by sku alphabet
                orderInfo.SortItemListBySku();
            }
        }

        public void CreateApproveSalesOrders()
        {
            foreach (OrderInfo orderInfo in allOrderInfos)
            {
                if (orderInfo.sales_order == null) continue;

                orderInfo.sales_order_approve = new SalesOrder()
                {
                    externalId = "MagentoOneToOneSO" + orderInfo.order_id,
                    orderStatus = SalesOrderOrderStatus._pendingFulfillment,
                    orderStatusSpecified = true
                };
            }
        }

        public void CreateInvoices()
        {
            foreach (OrderInfo orderInfo in allOrderInfos)
            {
                //if (orderInfo.sales_order == null || orderInfo.notToBill) continue;
                if (orderInfo.sales_order == null) continue;

                orderInfo.invoice = new Invoice()
                {
                    externalId = "MagentoInvoice" + orderInfo.order_id,
                    createdFrom = new RecordRef
                    {
                        externalId = orderInfo.sales_order.externalId
                    },
                    tranDate = orderInfo.purchase_date,
                    tranDateSpecified = true
                };
            }
        }

        public void CreateSalesOrders()
        {
            // Create sales orders
            foreach (OrderInfo orderInfo in allOrderInfos)
            {
                if (orderInfo.orderType is OrderType.Testing || orderInfo.orderType is OrderType.Invalid) continue;

                SalesOrder salesOrder = new SalesOrder();

                salesOrder.externalId = "MagentoOneToOneSO" + orderInfo.order_id;
                salesOrder.otherRefNum = orderInfo.order_id;
                salesOrder.billingAddress = CreateBillAddress(orderInfo.magentoOrder.billing_address);
                salesOrder.shippingAddress = CreateShippinhAddress(orderInfo.magentoOrder.extension_attributes.shipping_assignments[0].shipping.address);
                salesOrder.entity = CreateCustomer(orderInfo.magentoOrder.customer_group_id);
                salesOrder.salesRep = CreateSalesRep(orderInfo.magentoOrder.customer_group_id);
                salesOrder.@class = CreateBusinessLine();
                salesOrder.tranDate = orderInfo.purchase_date;
                salesOrder.tranDateSpecified = true;

                if (orderInfo.magentoOrder.discount_amount != 0)
                {
                    salesOrder.discountItem = CreateDiscountItem();
                    salesOrder.discountRate = orderInfo.magentoOrder.discount_amount.ToString();
                }

                CreateTaxInfo(salesOrder, orderInfo);

                salesOrder.location = CreateLocation(orderInfo.warehouse);
                salesOrder.shipMethod = CreateShippingMethod(orderInfo);
                salesOrder.shippingCost = orderInfo.magentoOrder.shipping_amount;
                salesOrder.shippingCostSpecified = true;
                salesOrder.itemList = CreateItemList(orderInfo.orderItems);
                salesOrder.customFieldList = CreateCustomFieldList(orderInfo.magentoOrder);

                Console.WriteLine("Create SO: {0}", salesOrder.otherRefNum);

                if (orderInfo.IsTestInjectingOrder())
                {
                    salesOrder.memo = "Invalid order. Don't process it!";
                }

                orderInfo.sales_order = salesOrder;
            }

            Console.WriteLine("");
            Console.WriteLine("Create {0} Sales Orders.", allOrderInfos.Count(orderInfo => orderInfo.sales_order != null));
            Console.WriteLine("");
        }

        internal class TaxInfo
        {
            public RecordRef taxItem { get; set; }
            public double taxRate { get; set; }
            public bool taxRateSpecified { get; set; }
            public bool isTaxable { get; set; }
            public bool isTaxableSpecified { get; set; }
        }

        private void CreateTaxInfo(SalesOrder salesOrder, OrderInfo orderInfo)
        {
            var taxInfo = GetTaxInfo(salesOrder.shippingAddress.state, orderInfo);

            salesOrder.taxItem = taxInfo.taxItem;
            salesOrder.taxRate = taxInfo.taxRate;
            salesOrder.taxRateSpecified = taxInfo.taxRateSpecified;
            salesOrder.isTaxable = taxInfo.isTaxable;
            salesOrder.isTaxableSpecified = taxInfo.isTaxableSpecified;
        }

        private TaxInfo GetTaxInfo(string shippingState, OrderInfo orderInfo)
        {
            TaxInfo taxInfo = new TaxInfo();

            if (orderInfo.magentoOrder.customer_group_id == 3)
            {
                taxInfo.isTaxable = false;
                taxInfo.isTaxableSpecified = true;

                if (orderInfo.magentoOrder.tax_amount > 0)
                {
                    MyOrderItem amazon_sales_tax = new MyOrderItem()
                    {
                        item_type = "Service for Sale",
                        internal_id = "51845", // X3076 - Finance Fee - Amazon Sales Tax
                        qty = 1,
                        salesPrice = orderInfo.magentoOrder.tax_amount
                    };
                    orderInfo.orderItems.Add(amazon_sales_tax);
                }

                return taxInfo;
            }

            double taxableItemsAmount = orderInfo.GetTaxableItemAmount();
            double calculated_taxRate = (taxableItemsAmount > 0) ? 100 * orderInfo.magentoOrder.tax_amount / taxableItemsAmount : 0;
            orderInfo.calculated_taxRate = calculated_taxRate;

            if (calculated_taxRate <= 0)
            {
                taxInfo.isTaxable = false;
                taxInfo.isTaxableSpecified = true;
                return taxInfo; // no tax -> return initial
            }

            RecordRef taxItem = new RecordRef()
            {
                type = RecordType.salesTaxItem
            };

            if (orderInfo.magentoOrder.customer_group_id == 2)
            {
                taxInfo.isTaxable = true;
                taxInfo.isTaxableSpecified = true;

                if (shippingState == "CA")
                {
                    taxItem.internalId = "-4950";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                }
                else if (shippingState == "TX")
                {
                    taxItem.internalId = "-4577";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                }
                else if (shippingState == "NJ")
                {
                    taxItem.internalId = "-5316";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                }
                else if (shippingState == "SC")
                {
                    taxItem.internalId = "-679";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                }
                else if (shippingState == "WA")
                {
                    taxItem.internalId = "-1519";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                }
                // else: let NS find the tax item  
            }
            else  // Standard
            {
                if (shippingState == "CA")
                {
                    taxItem.internalId = "-4950";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                    taxInfo.isTaxable = true;
                    taxInfo.isTaxableSpecified = true;
                }
                else if (shippingState == "TX")
                {
                    taxItem.internalId = "-4577";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                    taxInfo.isTaxable = true;
                    taxInfo.isTaxableSpecified = true;
                }
                else if (shippingState == "NJ")
                {
                    taxItem.internalId = "-5316";
                    taxInfo.taxItem = taxItem;
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                    taxInfo.isTaxable = true;
                    taxInfo.isTaxableSpecified = true;
                }
                else if (shippingState == "WA" || shippingState == "SC")
                {
                    taxInfo.taxRate = calculated_taxRate;
                    taxInfo.taxRateSpecified = true;
                    taxInfo.isTaxable = true;
                    taxInfo.isTaxableSpecified = true;
                }
            }

            return taxInfo;
        }

        private SalesOrderItemList CreateItemList(List<MyOrderItem> orderItems)
        {
            List<SalesOrderItem> salesOrderItems = new List<SalesOrderItem>();

            foreach (MyOrderItem orderItem in orderItems)
            {
                SalesOrderItem salesOrderItem = new SalesOrderItem()
                {
                    item = new RecordRef()
                    {
                        internalId = orderItem.internal_id
                    },
                    quantity = orderItem.qty,
                    quantitySpecified = true,
                    commitInventory = SalesOrderItemCommitInventory._availableQty,
                    commitInventorySpecified = (orderItem.item_type == "Inventory Item" || orderItem.item_type == "Lot Numbered Assembly") ? true : false,
                    rate = orderItem.salesPrice.ToString(),
                    isTaxable = (orderItem.tax_amount > 0) ? true : false,
                    isTaxableSpecified = true,
                    price = new RecordRef()
                    {
                        type = RecordType.account,
                        internalId = "-1",
                        name = "Custom",
                    }
                };

                salesOrderItems.Add(salesOrderItem);
            }

            return new SalesOrderItemList()
            {
                item = salesOrderItems.ToArray()
            };
        }

        private CustomFieldRef[] CreateCustomFieldList(M2Order magentoOrder)
        {
            StringCustomFieldRef customer_email = new StringCustomFieldRef()
            {
                scriptId = "custbody111",
                value = magentoOrder.customer_email
            };

            StringCustomFieldRef receiving_phone = new StringCustomFieldRef()
            {
                scriptId = "custbody46",
                value = (magentoOrder.extension_attributes.shipping_assignments[0].shipping.address.telephone == null) ? "" : magentoOrder.extension_attributes.shipping_assignments[0].shipping.address.telephone
            };

            StringCustomFieldRef totalWeight = new StringCustomFieldRef()
            {
                scriptId = "custbody63",
                value = magentoOrder.weight.ToString()
            };

            CustomFieldRef[] customFieldList;

            if (magentoOrder.customer_group_id == 3)
            {
                StringCustomFieldRef amazon_id = new StringCustomFieldRef()
                {
                    scriptId = "custbody_celigo_amz_orderid",
                    value = magentoOrder.increment_id
                };

                customFieldList = new CustomFieldRef[] { receiving_phone, amazon_id, customer_email, totalWeight };
            }
            else
            {
                customFieldList = new CustomFieldRef[] { receiving_phone, customer_email, totalWeight };
            }

            return customFieldList;
        }

        private RecordRef CreateShippingMethod(OrderInfo orderInfo)
        {
            return new RecordRef()
            {
                internalId = getShipMethodInternalId(orderInfo)
            };
        }

        private string getShipMethodInternalId(OrderInfo orderInfo)
        {
            var shippingInfo = orderInfo.magentoOrder.extension_attributes.shipping_assignments[0].shipping;

            if (shippingInfo.method == "tablerate_bestway" || shippingInfo.method == "m2eproshipping_m2eproshipping")
            {
                if ((orderInfo.magentoOrder.weight >= 800 || orderInfo.sum_total >= 1000)
                    && (orderInfo.magentoOrder.customer_group_id == 1 || orderInfo.magentoOrder.customer_group_id == 2))
                {
                    orderInfo.AddPallet();
                    //orderInfo.notToBill = true;
                    return "5397";  // TK_other
                }

                return "39161";  // SP_UPS Ground (EBIZ ONLY)
            }
            else if (shippingInfo.method == "amstrates_amstrates2")  // Will call   // freeshipping_freeshipping
            {
                switch (orderInfo.warehouse)
                {
                    case WarehouseLocation.CA:
                        return "8419";
                    case WarehouseLocation.TX:
                        return "11029";
                    case WarehouseLocation.SC:
                        return "8450";
                    case WarehouseLocation.WA:
                        return "8451";
                    case WarehouseLocation.NJ:
                        return "49536";
                    default:
                        return "Found no warehouse";
                }
            }
            else if (shippingInfo.method == "amstrates_amstrates1") // Local Delivery
            {
                //orderInfo.notToBill = true;

                switch (orderInfo.warehouse)
                {
                    case WarehouseLocation.CA:  // 218 DEL_CA (Standard)
                        return "218";
                    case WarehouseLocation.TX:  // 11243 DEL_TX (Standard)
                        return "11243";
                    case WarehouseLocation.SC:
                        return "8450";
                    case WarehouseLocation.WA:
                        return "8451";
                    case WarehouseLocation.NJ:
                        return "49536";
                    default:
                        return "Found no warehouse";
                }
            }

            return "No shipment method";
        }

        private RecordRef CreateLocation(WarehouseLocation warehouse)
        {
            RecordRef location = new RecordRef()
            {
                type = RecordType.location,
                typeSpecified = true,
                internalId = warehouse.ToString("d")
            };

            return location;
        }

        private RecordRef CreateBusinessLine()
        {
            return new RecordRef()
            {
                type = RecordType.classification,
                typeSpecified = true,
                internalId = "19"
            };
        }

        private RecordRef CreateDiscountItem()
        {
            return new RecordRef()
            {
                type = RecordType.discountItem,
                internalId = "9262"
            };
        }

        private RecordRef CreateSalesRep(int customer_group_id)
        {
            RecordRef salesRep = new RecordRef()
            {
                type = RecordType.salesRole,
                typeSpecified = true
            };

            switch (customer_group_id)
            {
                case 1: salesRep.internalId = "8"; break;       // Standard : E-BIZ LLC
                case 2: salesRep.internalId = "8"; break;       // Reseller : E-BIZ LLC
                case 3: salesRep.internalId = "1670292"; break; // Amazon   : E-BIZ AMZ
                default: throw new Exception("group_id match error!");
            }

            return salesRep;
        }

        private RecordRef CreateCustomer(int customer_group_id)
        {
            RecordRef customer = new RecordRef()
            {
                type = RecordType.customer,
                typeSpecified = true
            };

            switch (customer_group_id)
            {
                case 1: customer.internalId = "3183521"; break; // Lollicupstore2
                case 2: customer.internalId = "3196989"; break; // Lollicupstore2 - Reseller
                case 3: customer.internalId = "3114319"; break; // Amazon Seller Central
                default: throw new Exception("group_id match error!");
            }

            return customer;
        }

        private NetSuiteConnector.com.netsuite.webservices.Address CreateShippinhAddress(MG2Connector.Address mShippingAddress)
        {
            StringCustomFieldRef receiving_phone = new StringCustomFieldRef()
            {
                scriptId = "custrecord14",
                internalId = "1691",
                value = (mShippingAddress.telephone == null) ? "" : mShippingAddress.telephone
            };
            StringCustomFieldRef receiving_name = new StringCustomFieldRef()
            {
                scriptId = "custrecord13",
                value = mShippingAddress.firstname + " " + mShippingAddress.lastname
            };

            var shippingAddress = new NetSuiteConnector.com.netsuite.webservices.Address()
            {
                country = Country._unitedStates,
                addressee = mShippingAddress.firstname + " " + mShippingAddress.middlename + " " + mShippingAddress.lastname,
                city = mShippingAddress.city,
                state = mShippingAddress.region_code,
                zip = mShippingAddress.postcode,
                customFieldList = new CustomFieldRef[] { receiving_phone, receiving_name },
            };

            if (mShippingAddress.street.Count > 0)
            {
                shippingAddress.addr1 = mShippingAddress.street[0];
            }
            if (mShippingAddress.street.Count > 1)
            {
                shippingAddress.addr2 = mShippingAddress.street[1];
            }
            if (mShippingAddress.street.Count > 2)
            {
                //shippingAddress.addr3 = mShippingAddress.street[2];
                shippingAddress.addr2 = mShippingAddress.street[1] + " " + mShippingAddress.street[2];
            }

            return shippingAddress;
        }

        private NetSuiteConnector.com.netsuite.webservices.Address CreateBillAddress(MG2Connector.Address billing_address)
        {
            var billAddress = new NetSuiteConnector.com.netsuite.webservices.Address()
            {
                country = Country._unitedStates,
                zip = billing_address.postcode,
                addrPhone = (billing_address.telephone == null) ? "" : billing_address.telephone,
                state = billing_address.region_code,
                city = billing_address.city,
                addressee = billing_address.firstname + " " + billing_address.middlename + " " + billing_address.lastname
            };

            if (billing_address.street.Count > 0)
            {
                billAddress.addr1 = billing_address.street[0];
            }
            if (billing_address.street.Count > 1)
            {
                billAddress.addr2 = billing_address.street[1];
            }
            if (billing_address.street.Count > 2)
            {
                //billAddress.addr3 = billing_address.street[2];
                billAddress.addr2 = billing_address.street[1] + " " + billing_address.street[2];
            }

            return billAddress;
        }

        public void DecideWarehouse(Dictionary<string, ItemComparator.ItemInfo> netsuiteItemInfoMap)
        {
            // Assign the warehouse to the order
            foreach (OrderInfo orderInfo in allOrderInfos)
            {
                if (orderInfo.orderType is OrderType.Testing || orderInfo.orderType is OrderType.Invalid) continue;

                Console.WriteLine("Order Id: {0}", orderInfo.order_id);

                var shippingInfo = orderInfo.magentoOrder.extension_attributes.shipping_assignments[0].shipping;

                // Find the warehouse that can fulfill the most
                WarehouseLocation default_warehouse = OrderCreator.warehouseStateMap[shippingInfo.address.region_code];
                orderInfo.default_warehouse = default_warehouse;

                // Check if warehouse valid
                if (default_warehouse == WarehouseLocation.NULL)
                {
                    Console.WriteLine("Not Shippable: {0}", shippingInfo.address.region);
                    orderInfo.orderType = OrderType.InvalidButInject;
                    orderInfo.warehouse = WarehouseLocation.CA;
                    orderInfo.invalid_reason = "No Warehouse: " + shippingInfo.address.region + "; ";
                    continue;
                }

                // 1. SKU: TOS_XX -> done in fixOrderInfo()
                // 2. Will Call -> Default Warehouse
                // 3. Local Delivery -> Default Warehouse
                // 4. Choose most fulfilled warehouse
                if (orderInfo.tos_sku != null)
                {
                    orderInfo.warehouse = OrderCreator.tosWarehouseMap[orderInfo.tos_sku];
                }
                else if (orderInfo.IsWillCall())
                {
                    if (default_warehouse == WarehouseLocation.SC)
                    {
                        //invalidButInjectOrders.Add(orderInfo);
                        orderInfo.orderType = OrderType.InvalidButInject;
                        orderInfo.invalid_reason = "SC Will Call Order; ";
                    }
                    else
                    {
                        orderInfo.warehouse = default_warehouse;
                    }
                }
                else if (shippingInfo.method == "amstrates_amstrates1") // Local delivery
                {
                    orderInfo.warehouse = default_warehouse;
                }
                else if (!orderInfo.IsAmazonOrder() && CanSCShip(shippingInfo.address.region_code) && IsSCGoods(orderInfo.orderItems)
                    && CanWarehouseFulfillAll(WarehouseLocation.SC, orderInfo.orderItems, netsuiteItemInfoMap))
                {
                    orderInfo.warehouse = WarehouseLocation.SC;
                }
                else if (orderInfo.IsAmazonOrder() && default_warehouse == WarehouseLocation.WA)
                {
                    orderInfo.warehouse = WarehouseLocation.CA;
                }
                //else if (orderInfo.HasCertainItem("C-KPP32U"))
                //{
                //    orderInfo.warehouse = "Chino CA Warehouse";
                //}
                else  // Find the warehouse that can fulfill the most
                {
                    orderInfo.warehouse = SelectWarehouse(default_warehouse, orderInfo.orderItems, 0.7, netsuiteItemInfoMap);
                }

                DeductInventory(orderInfo.orderItems, orderInfo.warehouse, netsuiteItemInfoMap);
            }
        }

        private void DeductInventory(List<MyOrderItem> orderItems, WarehouseLocation warehouse, Dictionary<string, ItemComparator.ItemInfo> netsuiteItemInfoMap)
        {
            foreach (MyOrderItem orderItem in orderItems)
            {
                if (netsuiteItemInfoMap.ContainsKey(orderItem.internal_id))
                {
                    double remaing = netsuiteItemInfoMap[orderItem.internal_id].ReturnInv(warehouse) - orderItem.qty;
                    netsuiteItemInfoMap[orderItem.internal_id].UpdateInv(warehouse, remaing);
                }
            }
        }

        private WarehouseLocation SelectWarehouse(WarehouseLocation default_warehouse, List<MyOrderItem> orderItems, double threshold, Dictionary<string, ItemComparator.ItemInfo> netsuiteItemInfoMap)
        {
            if(FulfillPercent(default_warehouse, orderItems, netsuiteItemInfoMap) > threshold)
            {
                return default_warehouse;
            }

            if (default_warehouse is WarehouseLocation.CA)
            {
                if (FulfillPercent(WarehouseLocation.TX, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change CA -> TX");
                    return WarehouseLocation.TX;
                }
                else if (FulfillPercent(WarehouseLocation.NJ, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change CA -> NJ");
                    return WarehouseLocation.NJ;
                }
                else
                {
                    return WarehouseLocation.CA;
                }
            }
            else if (default_warehouse is WarehouseLocation.TX)
            {
                if (FulfillPercent(WarehouseLocation.CA, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change TX -> CA");
                    return WarehouseLocation.CA;
                }
                else if (FulfillPercent(WarehouseLocation.NJ, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change TX -> NJ");
                    return WarehouseLocation.NJ;
                }
                else
                {
                    return WarehouseLocation.TX;
                }
            }
            else if (default_warehouse is WarehouseLocation.NJ)
            {
                if (FulfillPercent(WarehouseLocation.TX, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change NJ -> TX");
                    return WarehouseLocation.TX;
                }
                else if (FulfillPercent(WarehouseLocation.CA, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change NJ -> CA");
                    return WarehouseLocation.CA;
                }
                else
                {
                    return WarehouseLocation.NJ;
                }
            }
            else if (default_warehouse is WarehouseLocation.WA)
            {
                if (FulfillPercent(WarehouseLocation.CA, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change WA -> CA");
                    return WarehouseLocation.CA;
                }
                else if (FulfillPercent(WarehouseLocation.TX, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change WA -> TX");
                    return WarehouseLocation.TX;
                }
                else if (FulfillPercent(WarehouseLocation.NJ, orderItems, netsuiteItemInfoMap) > threshold)
                {
                    Console.WriteLine("Change WA -> NJ");
                    return WarehouseLocation.NJ;
                }
                else
                {
                    return WarehouseLocation.WA;
                }
            }

            return WarehouseLocation.NULL;
        }

        internal bool CanSCShip(string region_code)
        {
            if (statesSCCanShip.Contains(region_code))
            {
                return true;
            }
            return false;
        }

        public static List<string> statesSCCanShip = new List<string>()
        {
            "AL", "GA", "SC", "NC", "FL"
        };

        private static List<string> scGoodsCapitals = new List<string>()
        {
            "Af-", "C", "FP", "FW", "GS", "IM", "JS", "KE", "KN", "U"
        };

        private bool IsSCGoods(List<MyOrderItem> orderItems)
        {
            foreach (MyOrderItem orderItem in orderItems)
            {
                if (!IsSCGood(orderItem.sku))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsSCGood(string sku)
        {
            foreach (string scGoodsCapital in scGoodsCapitals)
            {
                if (sku.StartsWith(scGoodsCapital))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanWarehouseFulfillAll(WarehouseLocation warehouse, List<MyOrderItem> orderItems, Dictionary<string, ItemComparator.ItemInfo> netsuiteItemInfoMap)
        {
            if (FulfillPercent(warehouse, orderItems, netsuiteItemInfoMap) == 1)
            {
                return true;
            }
            return false;
        }

        private double FulfillPercent(WarehouseLocation warehouse, List<MyOrderItem> orderItems, Dictionary<string, ItemComparator.ItemInfo> netsuiteItemInfoMap)
        {
            double total_items = 0;
            double can_be_fulfilled = 0;

            foreach (MyOrderItem orderItem in orderItems)
            {
                // Skip non inv items
                if (!orderItem.IsInventory()) continue;

                total_items += orderItem.qty;

                try
                {
                    double inventory = netsuiteItemInfoMap[orderItem.internal_id].ReturnInv(warehouse);
                    can_be_fulfilled += Math.Min(inventory, orderItem.qty);
                }
                catch (KeyNotFoundException keyNotFoundException)
                {
                    continue;
                }
            }

            Console.WriteLine("-- Location: {0}, Fulfill Precent: {1} --", warehouse, can_be_fulfilled / total_items);
            return can_be_fulfilled / total_items;
        }

        public static Dictionary<string, WarehouseLocation> warehouseStateMap = new Dictionary<string, WarehouseLocation>(StringComparer.OrdinalIgnoreCase)
        {
            { "AL", WarehouseLocation.TX },
            { "AK", WarehouseLocation.NULL },
            { "AZ", WarehouseLocation.CA },
            { "AR", WarehouseLocation.TX },
            { "CA", WarehouseLocation.CA },
            { "CO", WarehouseLocation.CA },
            { "CT", WarehouseLocation.NJ },
            { "DE", WarehouseLocation.NJ },
            { "DC", WarehouseLocation.NJ },
            { "FL", WarehouseLocation.TX },
            { "GA", WarehouseLocation.TX },
            { "HI", WarehouseLocation.NULL },
            { "ID", WarehouseLocation.WA },
            { "IL", WarehouseLocation.TX },
            { "IN", WarehouseLocation.NJ },
            { "IA", WarehouseLocation.TX },
            { "KS", WarehouseLocation.TX },
            { "KY", WarehouseLocation.NJ },
            { "LA", WarehouseLocation.TX },
            { "ME", WarehouseLocation.NJ },
            { "MD", WarehouseLocation.NJ },
            { "MA", WarehouseLocation.NJ },
            { "MI", WarehouseLocation.NJ },
            { "MN", WarehouseLocation.TX },
            { "MS", WarehouseLocation.TX },
            { "MO", WarehouseLocation.TX },
            { "MT", WarehouseLocation.WA },
            { "NE", WarehouseLocation.TX },
            { "NV", WarehouseLocation.CA },
            { "NH", WarehouseLocation.NJ },
            { "NJ", WarehouseLocation.NJ },
            { "NM", WarehouseLocation.TX },
            { "NY", WarehouseLocation.NJ },
            { "NC", WarehouseLocation.TX },
            { "ND", WarehouseLocation.CA },
            { "OH", WarehouseLocation.NJ },
            { "OK", WarehouseLocation.TX },
            { "OR", WarehouseLocation.WA },
            { "PA", WarehouseLocation.NJ },
            { "RI", WarehouseLocation.NJ },
            { "SC", WarehouseLocation.TX },
            { "SD", WarehouseLocation.CA },
            { "TN", WarehouseLocation.TX },
            { "TX", WarehouseLocation.TX },
            { "UT", WarehouseLocation.CA },
            { "VT", WarehouseLocation.NJ },
            { "VA", WarehouseLocation.NJ },
            { "WA", WarehouseLocation.WA },
            { "WV", WarehouseLocation.NJ },
            { "WI", WarehouseLocation.TX },
            { "WY", WarehouseLocation.CA },
            { "PR", WarehouseLocation.NULL },
            { "AS", WarehouseLocation.NULL },
            { "GU", WarehouseLocation.NULL },
            { "PW", WarehouseLocation.NULL },
            { "FM", WarehouseLocation.NULL },
            { "MP", WarehouseLocation.NULL },
            { "MH", WarehouseLocation.NULL },
            { "VI", WarehouseLocation.NULL },
        };

        public static Dictionary<string, WarehouseLocation> tosWarehouseMap = new Dictionary<string, WarehouseLocation>(StringComparer.OrdinalIgnoreCase)
        {
            { "TOS_CA", WarehouseLocation.CA },
            { "TOS_TX", WarehouseLocation.TX },
            { "TOS_SC", WarehouseLocation.SC },
            { "TOS_WA", WarehouseLocation.WA },
            { "TOS_NJ", WarehouseLocation.NJ }
        };
    }



    public class MyOrderItem
    {
        public string sku { get; set; }
        public double qty { get; set; }
        public double weight { get; set; }
        public double salesPrice { get; set; }
        public double tax_amount { get; set; }
        public double tax_percent { get; set; }
        public double subtotal { get; set; }
        public string brand { get; set; }
        public string internal_id { get; set; }
        public string item_type { get; set; }

        public MyOrderItem deep_clone()
        {
            var itemInfo = new MyOrderItem()
            {
                sku = sku,
                qty = qty,
                salesPrice = salesPrice,
                tax_amount = tax_amount,
                internal_id = internal_id,
                item_type = item_type
            };

            return itemInfo;
        }

        internal bool isInvalidItem()
        {
            if (item_type == "" || internal_id == "")
            {
                return true;
            }
            return false;
        }

        internal bool IsInventory()
        {
            if (item_type == "Inventory Item" || item_type == "Lot Numbered Assembly")
            {
                return true;
            }
            return false;
        }
    }

    public enum WarehouseLocation
    {
        NULL = 0,
        CA = 1,
        WA = 2,
        SC = 4,
        TX = 6,
        NJ = 9
    };

    public enum OrderType
    {
        NULL, Normal, Testing, TestInject, Invalid, InvalidButInject
    };

    public class OrderInfo
    {
        public M2Order magentoOrder { get; set; }
        public WarehouseLocation warehouse { get; set; }
        public WarehouseLocation default_warehouse { get; set; }
        public OrderType orderType { get; set; }
       // public bool notToBill { get; set; }

        public DateTime purchase_date { get; set; }
        public string order_id { get; set; }
        public string tos_sku { get; set; }
        public double sum_total { get; set; }
        public double calculated_taxRate { get; set; }
        public string invalid_reason { get; set; }
        public SalesOrder sales_order { get; set; }
        public SalesOrder sales_order_approve { get; set; }
        public SalesOrder sales_order_get { get; set; }
        public SalesOrder sales_order_close { get; set; }
        public Invoice invoice { get; set; }

        public List<MyOrderItem> orderItems { get; set; }

        internal bool IsTestNotForInject()
        {
            return OrderCreator.testNotInjectAccounts.Any(s => s == magentoOrder.customer_email) || magentoOrder.customer_email.EndsWith("@lollicupchina.com");
        }

        internal bool IsTestInjectingOrder()
        {
            return OrderCreator.testInjectAccounts.Any(s => s == magentoOrder.customer_email) || order_id.EndsWith("x");
        }

        internal bool IsToInject()
        {
            return (orderType == OrderType.Normal || orderType == OrderType.InvalidButInject || orderType == OrderType.TestInject);
        }

        internal bool IsToCloseOrder()
        {
            return (orderType == OrderType.InvalidButInject || orderType == OrderType.TestInject);
        }

        internal void ExtractItemList(Dictionary<int, ItemComparator.ItemInfo> magentoProductInfoMap)
        {
            orderItems = new List<MyOrderItem>();

            foreach (var item in magentoOrder.items)
            {
                if (item.sku.StartsWith("TOS_", StringComparison.InvariantCultureIgnoreCase))
                {
                    tos_sku = item.sku;
                    continue;
                }

                var orderItem = new MyOrderItem();
                orderItem.sku = item.sku;
                orderItem.internal_id = magentoProductInfoMap[item.product_id].ns_internal_id;
                orderItem.item_type = magentoProductInfoMap[item.product_id].ns_item_type;
                orderItem.brand = magentoProductInfoMap[item.product_id].brand;
                orderItem.weight = item.weight;
                orderItem.salesPrice = item.price;
                orderItem.qty = item.qty_ordered;
                orderItem.subtotal = item.row_total;
                orderItem.tax_amount = item.tax_amount;
                orderItem.tax_percent = item.tax_percent;

                orderItems.Add(orderItem);
            }
        }

        internal bool IsWillCall()
        {
            return (magentoOrder.extension_attributes.shipping_assignments[0].shipping.method == "amstrates_amstrates2");
        }

        internal bool HasTeaZoneProduct()
        {
            foreach (MyOrderItem itemInfo in orderItems)
            {
                if (itemInfo.brand == "Tea Zone")
                {
                    return true;
                }
            }

            return false;
        }

        internal bool HasToraniProduct()
        {
            foreach (MyOrderItem itemInfo in orderItems)
            {
                if (itemInfo.brand == "Torani")
                {
                    return true;
                }
            }

            return false;
        }

        public double GetTaxableItemAmount()
        {
            double amount = 0;

            foreach (MyOrderItem orderItem in orderItems)
            {
                if (orderItem.tax_amount > 0)
                {
                    amount += System.Convert.ToDouble(orderItem.salesPrice) * orderItem.qty;
                }
            }

            return amount;
        }

        internal bool isAllItemsValid()
        {
            foreach (MyOrderItem orderItem in this.orderItems)
            {
                if (orderItem.isInvalidItem())
                {
                    this.invalid_reason = "Invalid Item: " + orderItem.sku;
                    return false;
                }
            }
            return true;
        }

        internal void SortItemListBySku()
        {
            orderItems = orderItems.OrderBy(item => item.sku).ToList();
        }

        internal bool HasCertainItem(string certain_sku)
        {
            foreach (MyOrderItem orderItem in this.orderItems)
            {
                if (orderItem.sku == certain_sku)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool IsAmazonOrder()
        {
            if (magentoOrder.customer_group_id == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void AddPallet()
        {
            orderItems.Add(serviceItemMap["Pallet B"].deep_clone());
        }

        internal void CreateClosedOrder()
        {
            sales_order_close = new SalesOrder()
            {
                externalId = sales_order_get.externalId,
                itemList = sales_order_get.itemList
            };

            CloseItems(sales_order_close.itemList.item);
        }

        internal void CloseItems(SalesOrderItem[] salesOrderItems)
        {
            foreach (SalesOrderItem salesOrderItem in salesOrderItems)
            {
                salesOrderItem.isClosed = true;
                salesOrderItem.isClosedSpecified = true;
            }
        }

        public static Dictionary<string, MyOrderItem> serviceItemMap = new Dictionary<string, MyOrderItem>()
        {
            { "Pallet B", new MyOrderItem(){ qty = 1, internal_id = "65717", item_type = "Inventory Item", sku = "Pallet B", salesPrice = 0 } },
            { "X3041", new MyOrderItem(){ qty = 1, internal_id = "4834" , item_type = "Service for Sale", sku = "X3041 - Damage/Defective Claim (Online)" } },
            { "X3043", new MyOrderItem(){ qty = 1, internal_id = "4851" , item_type = "Service for Sale", sku = "X3043 - Miss-ship Claim (Online)" } },
            { "X3045", new MyOrderItem(){ qty = 1, internal_id = "11322", item_type = "Service for Sale", sku = "X3045 - TOS (online use only)" } },
            { "X3047", new MyOrderItem(){ qty = 1, internal_id = "42790", item_type = "Service for Sale", sku = "X3047 - Missing Claim (Online)" } },
            { "X3048", new MyOrderItem(){ qty = 1, internal_id = "64359", item_type = "Service for Sale", sku = "X3048 - Missing Claim - Lost by Carrier (Online)" } },
            { "X3050", new MyOrderItem(){ qty = 1, internal_id = "2301" , item_type = "Service for Sale", sku = "X3050 - Price Adjustment" } },
        };
    }
}
