using CsvHelper;
using MG2Connector;
using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConnectorIntegration
{
    public class ItemComparator
    {
        public List<ItemInfo> magento_item_list;
        public List<ItemInfo> effective_magento_item_list;
        public List<ItemInfo> netsuite_item_list;
        public Dictionary<string, Dictionary<string, string>> attributesMaps;
        public Dictionary<int, ItemInfo> magentoProductInfoMap;
        public Dictionary<string, ItemInfo> netsuiteItemInfoMap;

        public string outputFilePath;

        public ItemComparator() { }

        public void SetOutputFilePath(string outputDirPath)
        {
            outputFilePath = SetOutputFilePath(outputDirPath, @"\orders_" + GetDate() + ".csv");
        }

        private string SetOutputFilePath(string outputDirPath, string filename)
        {
            string filePath = outputDirPath + @"\" + filename;
            Console.WriteLine("Output File Path: {0}", filePath);
            Console.WriteLine("");

            return filePath;
        }

        private void WriteListToCsv(List<ItemInfo> itemList, string csvPath)
        {
            if (itemList.Count == 0) return;

            using (var csv = new CsvWriter(new StreamWriter(csvPath)))
            {
                csv.WriteRecords(itemList);
            }
        }

        public void InsertAttributeMaps(IList<M2Attribute> m2Attributes)
        {
            attributesMaps = new Dictionary<string, Dictionary<string, string>>();

            foreach (string attribute_code in magento_custom_attributes)
            {
                var m2Attribute = m2Attributes.Where(att => att.attribute_code == attribute_code).First();
                var attributeValueMap = m2Attribute.options.ToDictionary(option => option.value, option => option.label);
                attributesMaps[attribute_code] = attributeValueMap;
            }
        }

        public void InsertMProducts(IList<SearchItem> m2products)
        {
            magento_item_list = new List<ItemInfo>();

            foreach (SearchItem m2product in m2products)
            {
                ItemInfo itemInfo = new ItemInfo();
                itemInfo.m_product_id = m2product.id;
                itemInfo.sku = m2product.sku;
                itemInfo.weight = m2product.weight;
                itemInfo.status = m2product.status;
                itemInfo.brand = MapAttribute(GetCustomAttribute(m2product.custom_attributes, "brand"));

                var is_netsuite_item_att = GetCustomAttribute(m2product.custom_attributes, "is_netsuite_item");
                itemInfo.is_netsuite_item = (is_netsuite_item_att == null) ? "" : is_netsuite_item_att.value.ToString();

                var ns_internal_id_att = GetCustomAttribute(m2product.custom_attributes, "ns_internal_id");
                itemInfo.ns_internal_id = (ns_internal_id_att == null) ? "" : ns_internal_id_att.value.ToString();

                itemInfo.ns_item_type = MapAttribute(GetCustomAttribute(m2product.custom_attributes, "ns_item_type"));
                
                magento_item_list.Add(itemInfo);
            }

            magento_item_list = magento_item_list.OrderBy(item => item.sku).ToList();
            magentoProductInfoMap = magento_item_list.ToDictionary(item => item.m_product_id, item => item);

            Console.WriteLine("");
        }

        private CustomAttribute GetCustomAttribute(IList<CustomAttribute> customAttributes, string attribute_code)
        {
            var itemAttributes = customAttributes.Where(att => att.attribute_code == attribute_code);

            if (!itemAttributes.Any())
            {
                return null;
            }
            return itemAttributes.First();
        }

        private string MapAttribute(CustomAttribute customAttribute)
        {
            if (customAttribute == null) return "";
            var customAttributeValue = customAttribute.value.ToString();
            var m2AttributeMap = attributesMaps[customAttribute.attribute_code];
            
            string o;
            return m2AttributeMap.TryGetValue(customAttributeValue, out o) ? o : "null";
        }

        public void InsertNSItems(List<ItemSearchRow> netsuite_raw_search_items)
        {
            netsuiteItemInfoMap = new Dictionary<string, ItemInfo>();

            foreach (ItemSearchRow itemSearchRow in netsuite_raw_search_items)
            {
                string sku = itemSearchRow.basic.itemId[0].searchValue;
                string internal_id = itemSearchRow.basic.internalId[0].searchValue.internalId;

                if (sku.Contains(':'))
                {
                    sku = sku.Split(':').Last().Trim();
                }

                // Not in map -> new
                if (!netsuiteItemInfoMap.ContainsKey(internal_id))
                {
                    ItemInfo newItemInfo = new ItemInfo()
                    {
                        ns_internal_id = internal_id,
                        sku = sku,
                        product_type = itemSearchRow.basic.type[0].searchValue,
                        locationInv = new Dictionary<WarehouseLocation, double>()
                    };

                    netsuiteItemInfoMap[internal_id] = newItemInfo;
                }

                var inventoryLocation = (WarehouseLocation)Convert.ToInt32(itemSearchRow.basic.inventoryLocation[0].searchValue.internalId);

                if (sku.StartsWith("ATO-"))
                {
                    netsuiteItemInfoMap[internal_id].UpdateInv(inventoryLocation, 25);
                }
                else if (itemSearchRow.basic.locationQuantityAvailable != null)
                {
                    double qty_availabe = itemSearchRow.basic.locationQuantityAvailable[0].searchValue;
                    double qty_backordered = (itemSearchRow.basic.locationQuantityBackOrdered != null) ? itemSearchRow.basic.locationQuantityBackOrdered[0].searchValue : 0;

                    netsuiteItemInfoMap[internal_id].UpdateInv(inventoryLocation, qty_availabe - qty_backordered);
                }
            }
        }

        public static Dictionary<string, string> itemTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "_inventoryItem", "Inventory Item"},
            { "_assembly", "Lot Numbered Assembly"},
            { "_service", "Service for Sale"},
            { "_kit", "Kit/Package"},
            { "_nonInventoryItem", "Non-inventory Item"},
            { "", ""}
        };

        public static List<string> magento_custom_attributes = new List<string>()
        {
            "brand", "ns_item_type", //"display_on"
        };

        public class ItemInfo
        {
            public int m_product_id { get; set; }
            public string sku { get; set; }
            public string product_type { get; set; }
            public double weight { get; set; }
            public int status { get; set; }
            public string brand { get; set; }
            public string is_netsuite_item { get; set; }
            public string ns_internal_id { get; set; }
            public string ns_item_type { get; set; }
            public Dictionary<WarehouseLocation, double> locationInv { get; set; } // [location_id, qty]

            internal bool isFulfillableType()
            {
                if (ns_item_type == "Inventory Item" || ns_item_type == "Lot Numbered Assembly")
                {
                    return true;
                }
                return false;
            }

            internal double ReturnInv(WarehouseLocation warehouse)
            {
                return locationInv.ContainsKey(warehouse) ? locationInv[warehouse] : 0;
            }

            internal void UpdateInv(WarehouseLocation inventoryLocation, double qty)
            {
                qty = (qty < 0) ? 0 : qty;

                locationInv[inventoryLocation] = qty;
            }
        }

        private string GetDate()
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, pstZone);

            return pstTime.ToString("yyyyMMddHHMMSS");
        }
    }
}
