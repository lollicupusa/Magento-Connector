using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorIntegration
{
    public class CreditMemoInfo
    {
        public MG2Connector.M2CM magentoCreditMemo { get; set; }
        public string increment_id { get; set; }
        public string po { get; set; }
        public DateTime credit_date { get; set; }

        public OrderType orderType { get; set; }
        public string invalid_reason { get; set; }
        //public string new_order_id { get; set; }
        //public string store_credit_used { get; set; }
        //public string credit_memo_comment { get; set; }

        public List<ItemInfo> creditItems { get; set; }
        //public List<string> cmComments { get; set; }
        public MyInvoiceInfo invoiceInfo { get; set; }
        public Invoice created_from_invoice { get; set; }
        public MyInvoiceInfo apply_invoiceInfo { get; set; }
        public Invoice apply_invoice { get; set; }
        public CreditMemo creditMemo { get; set; }
        public CreditMemo creditMemo_get { get; set; }

        internal bool IsAmazonOrder()
        {
            return created_from_invoice.entity.name == "Amazon Seller Central";
        }

        internal void ExtractItemList(Dictionary<int, ItemComparator.ItemInfo> magentoProductInfoMap)
        {
            creditItems = new List<ItemInfo>();

            foreach (var item in magentoCreditMemo.items)
            {
                var creditItem = new ItemInfo();
                creditItem.sku = item.sku;
                creditItem.ns_internal_id = magentoProductInfoMap[item.product_id].ns_internal_id;
                creditItem.ns_item_type = magentoProductInfoMap[item.product_id].ns_item_type;
                creditItem.qty = item.qty;
                creditItem.tax = item.tax_amount;
                creditItem.price = item.price;

                creditItems.Add(creditItem);
            }
        }

        internal bool IsToInject()
        {
            return orderType != OrderType.Invalid;
        }

        internal bool IsInvoiceOpen()
        {
            return created_from_invoice.status == "Open";
        }

        //internal void ExtractItemList()
        //{
        //    Console.WriteLine("");
        //    Console.WriteLine("CM  #{0,-33} Order #{1}", increment_id, order_id);

        //    cmItems = new List<ItemInfo>();

        //    List<string> itemList = products_ordered.Split('|').ToList<string>();
        //    itemList.RemoveAt(itemList.Count - 1);

        //    foreach (string item in itemList)
        //    {
        //        String[] itemInfoList = item.Split(':');
        //        string sku = itemInfoList[0].Trim();

        //        //C-KC24U:22687:Lot Numbered Assembly:49.9200:2.0000:0.0000|
        //        ItemInfo cmItem = new ItemInfo();
        //        cmItem.sku = itemInfoList[0].Trim();
        //        cmItem.ns_internal_id = itemInfoList[1];
        //        cmItem.ns_item_type = itemInfoList[2];
        //        cmItem.price = itemInfoList[3];
        //        cmItem.qty = (itemInfoList[4] != "") ? System.Convert.ToDouble(itemInfoList[4]) : 0;
        //        cmItem.tax = (itemInfoList[5] != "") ? System.Convert.ToDouble(itemInfoList[5]) : 0;

        //        //orderItem.qty = System.Convert.ToDouble(itemInfoList[1]);
        //        //orderItem.price = itemInfoList[2];
        //        //orderItem.tax = System.Convert.ToDouble(itemInfoList[3]);
        //        Console.WriteLine("sku: {0,-33} qty: {1,3}", sku, cmItem.qty);
        //        cmItems.Add(cmItem);
        //    }
        //}

        internal bool isAllItemsValid()
        {
            foreach (var orderItem in creditItems)
            {
                if (orderItem.isInvalidItem())
                {
                    this.invalid_reason = "Invalid Item: " + orderItem.sku;
                    return false;
                }
            }
            return true;
        }

        internal string ExtractCode(string comment, Dictionary<string, ItemInfo> serviceItemMap)
        {
            foreach (string sku in serviceItemMap.Keys)
            {
                if (comment.ToUpper().Contains(sku))
                {
                    return sku;
                }
            }

            return null;
        }

        internal bool HasDamagedCode()
        {
            if(magentoCreditMemo.comments.Count == 0) return false;

            if (magentoCreditMemo.comments[0].comment.ToUpper().Contains("X3041"))
            {
                return true;
            }

            return false;
        }

        internal bool HasMissCode()
        {
            if (magentoCreditMemo.comments.Count == 0) return false;

            if (magentoCreditMemo.comments[0].comment.ToUpper().Contains("X3048"))
            {
                return true;
            }

            return false;
        }

        //internal bool CanApplyCredit()
        //{
        //    //if (!IsNumber(grand_total) || !IsNumber(store_credit_used))
        //    //{
        //    //    return false;
        //    //}

        //    Console.WriteLine("{0} {1}", grand_total, store_credit_used);

        //    //if (Convert.ToDouble(grand_total) >= Convert.ToDouble(store_credit_used) && IsSameBillName())
        //    if (Convert.ToDouble(grand_total) >= Convert.ToDouble(store_credit_used) && (IsSameBillName() || IsSameEmail()))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private bool IsSameEmail()
        //{
        //    string created_from_invoice_email = getEmailFromCustomList(created_from_invoice.customFieldList);
        //    string apply_invoice_email = getEmailFromCustomList(apply_invoice.customFieldList);

        //    Console.WriteLine("{0} {1}", created_from_invoice_email, apply_invoice_email);

        //    if (created_from_invoice_email == apply_invoice_email)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        private string getEmailFromCustomList(CustomFieldRef[] customFieldList)
        {
            foreach (var customFieldRef in customFieldList)
            {
                if (customFieldRef.scriptId == "custbody111")
                {
                    return ((StringCustomFieldRef)customFieldRef).value;
                }
            }

            return null;
        }

        private bool IsNumber(string numString)
        {
            long number1 = 0;
            return long.TryParse(numString, out number1);
        }

        //private bool IsSameBillName()
        //{
        //    Console.WriteLine("{0} {1}", Regex.Replace(bill_to_name, " ", ""), Regex.Replace(invoice.billingAddress.addressee, " ", ""));

        //    if (Regex.Replace(bill_to_name, " ", "") == Regex.Replace(invoice.billingAddress.addressee, " ", ""))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        internal bool HasInvoiceAmazonTaxItem()
        {
            foreach (InvoiceItem invoiceItem in created_from_invoice.itemList.item)
            {
                if (invoiceItem.item.internalId == "51845")  //X3076
                {
                    return true;
                }
            }

            return false;
        }

        public static Dictionary<string, ItemInfo> serviceItemMap = new Dictionary<string, ItemInfo>()
        {
            { "X3011", new ItemInfo(){ qty = 1, ns_internal_id = "3824" , ns_item_type = "Service for Sale", sku = "X3011 - Service Fee - S&H for Sample Packages" } },
            { "X3041", new ItemInfo(){ qty = 1, ns_internal_id = "4834" , ns_item_type = "Service for Sale", sku = "X3041 - Damage/Defective Claim (Online)" } },
            { "X3043", new ItemInfo(){ qty = 1, ns_internal_id = "4851" , ns_item_type = "Service for Sale", sku = "X3043 - Miss-ship Claim (Online)" } },
            { "X3045", new ItemInfo(){ qty = 1, ns_internal_id = "11322", ns_item_type = "Service for Sale", sku = "X3045 - TOS (online use only)" } },
            { "X3047", new ItemInfo(){ qty = 1, ns_internal_id = "42790", ns_item_type = "Service for Sale", sku = "X3047 - Missing Claim (Online)" } },
            { "X3048", new ItemInfo(){ qty = 1, ns_internal_id = "64359", ns_item_type = "Service for Sale", sku = "X3048 - Missing Claim - Lost by Carrier (Online)" } },
            { "X3050", new ItemInfo(){ qty = 1, ns_internal_id = "2301" , ns_item_type = "Service for Sale", sku = "X3050 - Price Adjustment" } },
            { "X2000", new ItemInfo(){ qty = 1, ns_internal_id = "242"  , ns_item_type = "Service for Sale", sku = "X2000" } },
            { "X2020", new ItemInfo(){ qty = 1, ns_internal_id = "243"  , ns_item_type = "Service for Sale", sku = "X2020" } },
            { "X2021", new ItemInfo(){ qty = 1, ns_internal_id = "10980", ns_item_type = "Service for Sale", sku = "X2021" } },
            { "X2025", new ItemInfo(){ qty = 1, ns_internal_id = "10977", ns_item_type = "Service for Sale", sku = "X2025" } },
            { "X3010", new ItemInfo(){ qty = 1, ns_internal_id = "245"  , ns_item_type = "Service for Sale", sku = "X3010 - Service Fee" } },
            { "X3052", new ItemInfo(){ qty = 1, ns_internal_id = "51836", ns_item_type = "Service for Sale", sku = "X3052 - Restocking Fee (10%)" } },
        };

        public static Dictionary<string, ItemInfo> taxItemMap = new Dictionary<string, ItemInfo>()
        {
            { "S-1001", new ItemInfo(){ sku = "S-1001", qty = 1, ns_internal_id = "337"  , ns_item_type = "OtherChargeSaleItem" } },
            { "S-1002", new ItemInfo(){ sku = "S-1002", qty = 1, ns_internal_id = "40653", ns_item_type = "OtherChargeSaleItem" } },
            { "S-1003", new ItemInfo(){ sku = "S-1003", qty = 1, ns_internal_id = "45694", ns_item_type = "OtherChargeSaleItem" } },
            { "S-1004", new ItemInfo(){ sku = "S-1004", qty = 1, ns_internal_id = "45695", ns_item_type = "OtherChargeSaleItem" } },
            { "S-1005", new ItemInfo(){ sku = "S-1005", qty = 1, ns_internal_id = "49541", ns_item_type = "OtherChargeSaleItem" } },
        };

        public static Dictionary<string, ItemInfo> locationTaxMap = new Dictionary<string, ItemInfo>()
        {
            { "1", new ItemInfo(){ sku = "S-1001", qty = 1, ns_internal_id = "337"  , ns_item_type = "OtherChargeSaleItem" } },
            { "6", new ItemInfo(){ sku = "S-1002", qty = 1, ns_internal_id = "40653", ns_item_type = "OtherChargeSaleItem" } },
            { "4", new ItemInfo(){ sku = "S-1003", qty = 1, ns_internal_id = "45694", ns_item_type = "OtherChargeSaleItem" } },
            { "2", new ItemInfo(){ sku = "S-1004", qty = 1, ns_internal_id = "45695", ns_item_type = "OtherChargeSaleItem" } },
            { "9", new ItemInfo(){ sku = "S-1005", qty = 1, ns_internal_id = "49541", ns_item_type = "OtherChargeSaleItem" } },
        };

        public static Dictionary<string, ItemInfo> shippingItemMap = new Dictionary<string, ItemInfo>()
        {
            { "X3075", new ItemInfo(){ qty = 1, ns_internal_id = "4862" , ns_item_type = "Service for Sale", sku = "X3075 - Shipping Credit (FedEx/UPS)" } },
            { "X3074", new ItemInfo(){ qty = 1, ns_internal_id = "4861" , ns_item_type = "Service for Sale", sku = "X3074 - Shipping Credit (Trucking)" } },
        };

        public static Dictionary<string, ItemInfo> discountItemMap = new Dictionary<string, ItemInfo>()
        {
            { "X3027", new ItemInfo(){ qty = 1, ns_internal_id = "49026", ns_item_type = "Service for Sale", sku = "x3027 - Discount - Lollicup Online Promotion" } },
        };

        public static Dictionary<string, string> warehouseInventroy = new Dictionary<string, string>()
        {
            { "1", "CA-inv"},
            { "6", "TX-inv"},
            { "4", "SC-inv"},
            { "2", "WA-inv"},
            { "9", "NJ-inv"},
        };
    }

    //public class MyOrderItem
    //{
    //    public string sku { get; set; }
    //    public double qty { get; set; }
    //    public double weight { get; set; }
    //    public double salesPrice { get; set; }
    //    public double tax_amount { get; set; }
    //    public double tax_percent { get; set; }
    //    public double subtotal { get; set; }
    //    public string brand { get; set; }
    //    public string internal_id { get; set; }
    //    public string item_type { get; set; }

    //    public MyOrderItem deep_clone()
    //    {
    //        var itemInfo = new MyOrderItem()
    //        {
    //            sku = sku,
    //            qty = qty,
    //            salesPrice = salesPrice,
    //            tax_amount = tax_amount,
    //            internal_id = internal_id,
    //            item_type = item_type
    //        };

    //        return itemInfo;
    //    }

    //    internal bool isInvalidItem()
    //    {
    //        if (item_type == "" || internal_id == "")
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    internal bool IsInventory()
    //    {
    //        if (item_type == "Inventory Item" || item_type == "Lot Numbered Assembly")
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //}

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
        NULL, Normal, Invalid
    };

    public class ItemInfo
    {
        public string sku { get; set; }
        public int qty { get; set; }
        public double price { get; set; }
        public double tax { get; set; }
        public string ns_internal_id { get; set; }
        public string ns_item_type { get; set; }

        public ItemInfo deep_clone()
        {
            ItemInfo itemInfo = new ItemInfo()
            {
                sku = sku,
                qty = qty,
                price = price,
                tax = tax,
                ns_internal_id = ns_internal_id,
                ns_item_type = ns_item_type
            };

            return itemInfo;
        }

        internal bool isInvalidItem()
        {
            if (string.IsNullOrEmpty(sku) || string.IsNullOrEmpty(ns_internal_id) || string.IsNullOrEmpty(ns_item_type))
            {
                return true;
            }
            return false;
        }
    }

    public class MyInvoiceInfo
    {
        public string po_number { get; set; }
        public string so_number { get; set; }
        //public string so_internal_id { get; set; }
        public string so_status { get; set; }
        public string invoice_number { get; set; }
        public string invoice_internal_id { get; set; }
        //public string customer_id { get; set; }
        //public string salesRep { get; set; }
        public string status { get; set; }
        //public double amount { get; set; }
        public string warehouse_location { get; set; }
    }

    public class MySalesOrderInfo
    {
        public string po_number { get; set; }
        public string so_number { get; set; }
        public string so_internal_id { get; set; }
        public string customer_id { get; set; }
        public string salesRep { get; set; }
        public string status { get; set; }
        public double amount { get; set; }
    }

    public class CMComparison
    {
        public string Magento_Id { get; set; }
        public string Magento_CM_Id { get; set; }
        public string NS_CM_Id { get; set; }
        public string NS_CM_Internal_Id { get; set; }
        public string NS_CM_External_ID { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public string Entity { get; set; }
        public double M_Refunded { get; set; }
        public double NS_Refunded { get; set; }
        public string Discrepancy { get; set; }
        public string Comment { get; set; }
    }
}
