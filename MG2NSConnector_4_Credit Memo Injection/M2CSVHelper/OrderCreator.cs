using MG2Connector;
using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectorIntegration
{
    public class OrderCreator
    {
        public List<CreditMemoInfo> allCMInfos;
        public Dictionary<string, MyInvoiceInfo> invoiceInfoMap;

        public OrderCreator()
        {
            allCMInfos = new List<CreditMemoInfo>();
        }

        public void InsertMCMs(IList<M2CM> m2CMs)
        {
            foreach (var m2CM in m2CMs)
            {
                CreditMemoInfo creditMemoInfo = new CreditMemoInfo()
                {
                    magentoCreditMemo = m2CM,
                    increment_id = m2CM.increment_id, //+ "x"
                    po = m2CM.order.increment_id,
                    credit_date = TransformUTCToPST(DateTime.Parse(m2CM.created_at))
                };

                if (creditMemoInfo.credit_date <= new DateTime(2020, 1, 1))
                {
                    creditMemoInfo.credit_date = new DateTime(2020, 1, 2);
                }

                allCMInfos.Add(creditMemoInfo);
            }
        }

        private DateTime TransformUTCToPST(DateTime utcTime)
        {
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, pstZone);
        }

        public void ExtractItems(Dictionary<int, ItemComparator.ItemInfo> magentoProductInfoMap)
        {
            foreach (var cmInfo in allCMInfos)
            {
                cmInfo.ExtractItemList(magentoProductInfoMap);

                if (!cmInfo.isAllItemsValid())
                {
                    cmInfo.orderType = OrderType.Invalid;
                    cmInfo.invalid_reason += "Item(s) not valid; ";
                }
            }
        }

        public Tuple<DateTime, DateTime> GetCreditDateRange()
        {
            List<DateTime> dates = allCMInfos.Select(cm => TransUTCtoPST(DateTime.Parse(cm.magentoCreditMemo.order.created_at))).ToList();

            return Tuple.Create(dates.Min(), dates.Max());
        }

        private DateTime TransUTCtoPST(DateTime utcTime)
        {
            TimeZoneInfo selectedZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, selectedZone);
        }

        public void CreateInvoiceInfoMap(List<TransactionSearchRow> invoiceSearchRows)
        {
            invoiceInfoMap = new Dictionary<string, MyInvoiceInfo>();

            foreach (var transactionSearchRow in invoiceSearchRows)
            {
                if (transactionSearchRow.basic.createdFrom == null || transactionSearchRow.basic.otherRefNum == null) continue;

                string po_number = transactionSearchRow.basic.otherRefNum[0].searchValue;

                var invoiceInfo = new MyInvoiceInfo()
                {
                    po_number = po_number,
                    so_number = transactionSearchRow.createdFromJoin.tranId[0].searchValue,
                    so_status = transactionSearchRow.createdFromJoin.status[0].searchValue,
                    invoice_number = transactionSearchRow.basic.tranId[0].searchValue,
                    invoice_internal_id = transactionSearchRow.basic.internalId[0].searchValue.internalId,
                    status = transactionSearchRow.basic.status[0].searchValue,
                    warehouse_location = transactionSearchRow.basic.location[0].searchValue.internalId,
                };

                invoiceInfoMap[po_number] = invoiceInfo;
            }
        }

        public void InsertInvoiceInfo()
        {
            foreach (var creditMemoInfo in allCMInfos)
            {
                try
                {
                    creditMemoInfo.invoiceInfo = invoiceInfoMap[creditMemoInfo.po];
                    
                    Console.WriteLine("Credit Memo: {0}, Order: {1} inserted", creditMemoInfo.increment_id, creditMemoInfo.po);
                }
                catch (KeyNotFoundException exception)
                {
                    Console.WriteLine("Credit Memo: {0}, Order: {1} not found the invoice in NetSuite", creditMemoInfo.increment_id, creditMemoInfo.po);
                    creditMemoInfo.orderType = OrderType.Invalid;
                    creditMemoInfo.invalid_reason += "Not found the invoice in NetSuite";
                }
            }
        }

        public void FixServiceItems()
        {
            Console.WriteLine("");

            foreach (CreditMemoInfo creditMemoInfo in allCMInfos)
            {
                Console.WriteLine("CM: {0}, Order: {1} ", creditMemoInfo.increment_id, creditMemoInfo.po);

                if (creditMemoInfo.orderType == OrderType.Invalid) continue;

                var comment = (creditMemoInfo.magentoCreditMemo.comments.Count == 0) ? "" : creditMemoInfo.magentoCreditMemo.comments[0].comment;

                // SO closed || damaged (x3041) || Miss pick(x3048) -> never fulfilled -> service code
                if (creditMemoInfo.invoiceInfo.so_status == "closed" || creditMemoInfo.HasDamagedCode() || creditMemoInfo.HasMissCode())
                {
                    creditMemoInfo.creditItems.Clear();

                    var service_code = (HasMapCode(comment.ToUpper(), CreditMemoInfo.serviceItemMap)) ?
                        creditMemoInfo.ExtractCode(comment, CreditMemoInfo.serviceItemMap) : "X3045";

                    ItemInfo serviceItem = CreditMemoInfo.serviceItemMap[service_code].deep_clone();
                    serviceItem.price = creditMemoInfo.magentoCreditMemo.adjustment + creditMemoInfo.magentoCreditMemo.subtotal;
                    creditMemoInfo.creditItems.Add(serviceItem);
                    Console.WriteLine("{0,-8}: {1,5} ", service_code, serviceItem.price);
                }
                else if (creditMemoInfo.magentoCreditMemo.adjustment != 0)   // Adjustment
                {
                    foreach (string serviceSku in CreditMemoInfo.serviceItemMap.Keys)
                    {
                        if (comment.ToUpper().Contains(serviceSku))
                        {
                            ItemInfo serviceItem = CreditMemoInfo.serviceItemMap[serviceSku].deep_clone();
                            serviceItem.price = creditMemoInfo.magentoCreditMemo.adjustment;
                            creditMemoInfo.creditItems.Add(serviceItem);
                            Console.WriteLine("{0,-8}: {1,5} ", serviceSku, serviceItem.price);
                        }
                    }
                }

                // tax
                if (creditMemoInfo.magentoCreditMemo.tax_amount != 0 || HasMapCode(comment.ToUpper(), CreditMemoInfo.taxItemMap))
                {
                    ItemInfo taxItem;

                    if (creditMemoInfo.IsAmazonOrder() && creditMemoInfo.HasInvoiceAmazonTaxItem())
                    {
                        taxItem = new ItemInfo() { sku = "X3076", qty = 1, ns_internal_id = "51845", ns_item_type = "Service for Sale" };
                        taxItem.price = creditMemoInfo.magentoCreditMemo.tax_amount;
                    }
                    else
                    {
                        taxItem = CreditMemoInfo.locationTaxMap[creditMemoInfo.invoiceInfo.warehouse_location].deep_clone();
                        taxItem.price = (creditMemoInfo.magentoCreditMemo.tax_amount != 0) ? 
                            creditMemoInfo.magentoCreditMemo.tax_amount : creditMemoInfo.magentoCreditMemo.adjustment;
                    }

                    creditMemoInfo.creditItems.Add(taxItem);
                    Console.WriteLine("{0,-8}: {1,5} ", taxItem.sku, taxItem.price);
                }

                // shipping
                if (creditMemoInfo.magentoCreditMemo.shipping_amount != 0 || HasMapCode(comment.ToUpper(), CreditMemoInfo.shippingItemMap))
                {
                    var shipping_code = (HasMapCode(comment.ToUpper(), CreditMemoInfo.shippingItemMap)) ? 
                        creditMemoInfo.ExtractCode(comment, CreditMemoInfo.shippingItemMap) : "X3075";

                    ItemInfo shipItem = CreditMemoInfo.shippingItemMap[shipping_code].deep_clone();
                    shipItem.price = (creditMemoInfo.magentoCreditMemo.shipping_amount != 0) ? creditMemoInfo.magentoCreditMemo.shipping_amount : creditMemoInfo.magentoCreditMemo.adjustment;
                    creditMemoInfo.creditItems.Add(shipItem);
                    Console.WriteLine("{0,-8}: {1,5} ", shipping_code, shipItem.price);
                }

                // Discount
                if (comment.ToUpper().Contains("X3027"))
                {
                    ItemInfo discountItem = CreditMemoInfo.discountItemMap["X3027"].deep_clone();
                    discountItem.price = creditMemoInfo.magentoCreditMemo.adjustment;
                    creditMemoInfo.creditItems.Add(discountItem);
                    Console.WriteLine("{0,-8}: {1,5} ", "X3027", discountItem.price);
                }

                Console.WriteLine("");
            }
        }

        private bool HasMapCode(string comment, Dictionary<string, ItemInfo> itemMap)
        {
            foreach (string sku in itemMap.Keys)
            {
                if (comment.Contains(sku))
                {
                    return true;
                }
            }

            return false;
        }

        public void CreateCreditMemos()
        {
            foreach (CreditMemoInfo creditMemoInfo in allCMInfos)
            {
                if(creditMemoInfo.orderType == OrderType.Invalid) continue;

                CreditMemo creditMemo = new CreditMemo()
                {
                    externalId = "MagentoCM" + creditMemoInfo.increment_id,
                    otherRefNum = creditMemoInfo.po,
                    createdFrom = new RecordRef()
                    {
                        internalId = creditMemoInfo.created_from_invoice.internalId,
                        type = RecordType.invoice,
                        typeSpecified = true
                    },

                    tranDate = creditMemoInfo.credit_date,
                    tranDateSpecified = true,
                    discountItem = creditMemoInfo.created_from_invoice.discountItem,
                    discountRate = creditMemoInfo.magentoCreditMemo.discount_amount.ToString(),
                    itemList = CreateItemList(creditMemoInfo, creditMemoInfo.created_from_invoice.entity),
                    shippingCostSpecified = true,
                    shippingCost = 0,
                    isTaxable = false,
                    isTaxableSpecified = true,
                    toBePrinted = true,
                    toBePrintedSpecified = true,

                    //Sales Order #361988, Credit Memo #100000393
                    memo = "Sales Order #" + creditMemoInfo.invoiceInfo.so_number + ", Magento Credit Memo #" + creditMemoInfo.increment_id,

                };

                if (creditMemo.tranDate <= new DateTime(2020, 1, 1))
                {
                    creditMemo.tranDate = new DateTime(2020, 1, 2);
                }

                creditMemoInfo.creditMemo = creditMemo;
            }
        }

        private CreditMemoItemList CreateItemList(CreditMemoInfo creditMemoInfo, RecordRef entity)
        {
            List<CreditMemoItem> creditMemoItems = new List<CreditMemoItem>();

            foreach (ItemInfo cmItem in creditMemoInfo.creditItems)
            {
                RecordRef itemRef = new RecordRef()
                {
                    internalId = cmItem.ns_internal_id,
                };
                
                CreditMemoItem creditMemoItem = new CreditMemoItem()
                {
                    item = itemRef,
                    quantity = cmItem.qty,
                    quantitySpecified = true,
                    rate = cmItem.price.ToString(),
                    price = new RecordRef()
                    {
                        type = RecordType.account,
                        internalId = "-1",
                        name = "Custom",
                    },
                };

                if (cmItem.ns_item_type.Equals("Lot Numbered Assembly"))
                {
                    creditMemoItem.inventoryDetail = new InventoryDetail()
                    {
                        inventoryAssignmentList = new InventoryAssignmentList()
                        {
                            inventoryAssignment = new InventoryAssignment[]
                            {
                                new InventoryAssignment()
                                {
                                    quantitySpecified = true,
                                    quantity = cmItem.qty,
                                    receiptInventoryNumber = CreditMemoInfo.warehouseInventroy[creditMemoInfo.created_from_invoice.location.internalId],
                                }
                            }
                        }
                    };
                }

                creditMemoItems.Add(creditMemoItem);
            }

            // description item
            if (creditMemoInfo.magentoCreditMemo.comments.Count > 0)
            {
                CreditMemoItem descriptionItem = new CreditMemoItem()
                {
                    item = new RecordRef()
                    {
                        type = RecordType.descriptionItem,
                        typeSpecified = true,
                        internalId = "-3"
                    },
                    
                    description = creditMemoInfo.magentoCreditMemo.comments[0].comment
                };

                creditMemoItems.Add(descriptionItem);
            }

            CreditMemoItemList creditMemoItemList = new CreditMemoItemList()
            {
                item = creditMemoItems.ToArray()
            };

            return creditMemoItemList;
        }
    }
    
}
