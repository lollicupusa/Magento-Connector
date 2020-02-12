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
        public List<ItemInfo> all_items;
        public List<ItemInfo> magento_item_list;
        public List<ItemInfo> effective_magento_item_list;
        public List<ItemInfo> netsuite_item_list;
        public List<ItemInfo> in_magento_not_ns_items;
        public List<ItemInfo> in_magento_ns_inactive_items;
        public List<ItemInfo> in_magento_ns_field_not_check_items;
        public List<ItemInfo> in_ns_not_exist_magento_items;
        public List<ItemInfo> in_ns_not_live_magento_items;
        public List<ItemInfo> weight_diff_items;
        public List<ItemInfo> type_diff_items;
        public List<ItemInfo> internal_id_diff_items;
        public List<ItemInfo> intersect_items;
        public Dictionary<string, Dictionary<string, string>> attributesMaps;

        static string skuIdMapFilePath;
        static string in_magento_ns_inactive_filepath;
        static string in_magento_ns_field_not_check_filepath;
        static string in_ns_not_exist_magento_filepath;
        static string in_ns_not_live_magento_filepath;
        static string weight_diff_filepath;
        static string item_type_diff_filepath;
        static string ns_internal_id_diff_filepath;
        public List<string> allPaths;

        public ItemComparator() { }

        public ItemComparator(string magentoFilePath)
        {
            var reader = new StreamReader(System.IO.File.OpenRead(magentoFilePath));
            var csv = new CsvReader(reader);
            csv.Configuration.HeaderValidated = null;
            csv.Configuration.MissingFieldFound = null;
            magento_item_list = csv.GetRecords<ItemInfo>().Where(row => row.sku != "").ToList();
        }

        public void SetOutFilesPath(string outputDirPath, string toWayneDirName)
        {
            allPaths = new List<string>();
            skuIdMapFilePath = SetOutputFilePath(outputDirPath, "Sku_internal_id_map.csv");
            in_magento_ns_inactive_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "in_magento_ns_inactive_items.csv");
            in_magento_ns_field_not_check_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "in_magento_ns_field_not_check_items.csv");
            in_ns_not_exist_magento_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "in_ns_not_exist_magento_items.csv");
            in_ns_not_live_magento_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "in_ns_not_live_magento_items.csv");
            weight_diff_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "Weight_Diff.csv");
            item_type_diff_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "Item_Type_Diff.csv");
            ns_internal_id_diff_filepath = SetOutputFilePath(outputDirPath + toWayneDirName, "NS_Internal_ID_Diff.csv");
            
        }

        public void CleanFiles()
        {
            foreach (string path in allPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    Console.WriteLine("Delete {0}", path);
                }
                else
                {
                    Console.WriteLine("No {0}", path);
                }
            }
        }

        private string SetOutputFilePath(string outputDirPath, string filename)
        {
            string filePath = outputDirPath + @"\" + filename;
            allPaths.Add(filePath);
            Console.WriteLine("Output File Path: {0}", filePath);
            Console.WriteLine("");

            return filePath;
        }

        public void WriteOutputCSVs()
        {
            WriteListToCsv(all_items, skuIdMapFilePath);
            WriteListToCsv(in_magento_ns_inactive_items, in_magento_ns_inactive_filepath);
            WriteListToCsv(in_magento_ns_field_not_check_items, in_magento_ns_field_not_check_filepath);
            WriteListToCsv(in_ns_not_exist_magento_items, in_ns_not_exist_magento_filepath);
            WriteListToCsv(in_ns_not_live_magento_items, in_ns_not_live_magento_filepath);
            WriteListToCsv(type_diff_items, item_type_diff_filepath);
            WriteListToCsv(internal_id_diff_items, ns_internal_id_diff_filepath);
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

        public void GenerateEffectiveMagentoList()
        {
            effective_magento_item_list = magento_item_list.Where(item => item.status == 1 && item.is_netsuite_item == "1").ToList();
        }

        public void InsertNSItems(List<ItemSearchRow> netsuite_raw_items)
        {
            var netsuite_items = ConvertNSSearchType(netsuite_raw_items);
            netsuite_item_list = new List<ItemInfo>();
            //netsuite_item_list = ConvertNSSearchType(netsuite_raw_items);

            in_magento_not_ns_items = new List<ItemInfo>();

            foreach (ItemInfo magento_item in effective_magento_item_list)
            {
                var found = netsuite_items.Find(i => i.sku == magento_item.sku);

                if (found == null)
                {
                    in_magento_not_ns_items.Add(magento_item);
                }
                else
                {
                    magento_item.InsertNSInfo(found);
                    netsuite_item_list.Add(magento_item);
                }
            }

            // In NS not M
            var in_ns_not_m = netsuite_items.Except(netsuite_item_list, new SkuComparer()).ToList();
            netsuite_item_list.AddRange(in_ns_not_m);
        }

        public class SkuComparer : IEqualityComparer<ItemInfo>
        {
            public int GetHashCode(ItemInfo item)
            {
                if (item == null)
                {
                    return 0;
                }
                return item.sku.ToUpper().GetHashCode();
            }

            public bool Equals(ItemInfo item1, ItemInfo item2)
            {
                if (object.ReferenceEquals(item1, item2))
                {
                    return true;
                }
                if (object.ReferenceEquals(item1, null) || object.ReferenceEquals(item2, null))
                {
                    return false;
                }
                return item1.sku.ToUpper() == item2.sku.ToUpper();
            }
        }

        private List<ItemInfo> ConvertNSSearchType(List<ItemSearchRow> netsuite_raw_items)
        {
            List<ItemInfo> itemInfoList = new List<ItemInfo>();

            foreach (ItemSearchRow itemSearchRow in netsuite_raw_items)
            {
                string sku = itemSearchRow.basic.itemId[0].searchValue;

                if (sku.Contains(':'))
                {
                    sku = sku.Split(':').Last().Trim();
                }

                ItemInfo item = new ItemInfo()
                {
                    internal_id = itemSearchRow.basic.internalId[0].searchValue.internalId,
                    sku = sku,
                    product_type = itemSearchRow.basic.type[0].searchValue,
                    inactive_in_ns = false,
                    get_success = true
                };

                if (itemSearchRow.basic.customFieldList != null)
                {
                    foreach (SearchColumnDoubleCustomField searchColumnDoubleCustomField in itemSearchRow.basic.customFieldList)
                    {
                        if (searchColumnDoubleCustomField.scriptId == "custitem_pack_weight_gross")
                        {
                            item.ns_weight = searchColumnDoubleCustomField.searchValue;
                        }
                    }
                }

                itemInfoList.Add(item);
            }

            return itemInfoList;
        }

        static public List<string> GetSkuList(List<ItemInfo> item_list)
        {
            return item_list.Select(item => item.sku).ToList();
        }

        public List<RecordRef> GetNSGetList(List<ItemInfo> in_magento_not_ns_items)
        {
            List<RecordRef> recordRefs = new List<RecordRef>();

            foreach (ItemInfo itemInfo in in_magento_not_ns_items)
            {
                RecordRef itemRef = new RecordRef
                {
                    name = itemInfo.sku,
                    internalId = itemInfo.ns_internal_id,
                    typeSpecified = true
                };

                switch (itemInfo.ns_item_type)
                {
                    case "Inventory Item":
                        itemRef.type = RecordType.inventoryItem;
                        break;
                    case "Lot Numbered Assembly":
                        itemRef.type = RecordType.lotNumberedAssemblyItem;
                        break;
                    case "Non-inventory Item":
                        itemRef.type = RecordType.nonInventorySaleItem;
                        break;
                    case "Kit/Package":
                        itemRef.type = RecordType.kitItem;
                        break;
                    case "Service for Sale":
                        itemRef.type = RecordType.serviceSaleItem;
                        break;
                    case "Undefined":
                        Console.WriteLine("{0, -10} Type Undefined, Skip!", itemInfo.sku);
                        continue;
                    default:
                        Console.WriteLine("Error item: {0}, type: {1}", itemInfo.sku, itemInfo.ns_item_type);
                        throw new Exception("itemType match error!");
                }

                recordRefs.Add(itemRef);
            }

            return recordRefs;
        }

        private List<ItemInfo> ConvertNSGetType(List<ReadResponse> nsGetList)
        {
            List<ItemInfo> itemInfoList = new List<ItemInfo>();

            foreach (ReadResponse readResponse in nsGetList)
            {
                Type type = readResponse.record.GetType();

                dynamic responseItem = Convert.ChangeType(readResponse.record, type);

                string sku = responseItem.itemId;
                if (sku.Contains(':'))
                {
                    sku = sku.Split(':').Last().Trim();
                }

                ItemInfo item = new ItemInfo()
                {
                    internal_id = responseItem.internalId,
                    sku = sku,
                    product_type = type.ToString().Split('.').Last(),
                    inactive_in_ns = responseItem.isInactive,
                    get_success = true
                };

                if (responseItem.customFieldList != null)
                {
                    foreach (var searchColumnCustomField in responseItem.customFieldList)
                    {
                        if (searchColumnCustomField.scriptId == "custitem_pack_weight_gross")
                        {
                            item.ns_weight = searchColumnCustomField.value;
                        }
                    }
                }

                itemInfoList.Add(item);
            }

            return itemInfoList;
        }

        public void InsertNSGetList(List<ReadResponse> nsGetList)
        {
            var nsGetItemInfos = ConvertNSGetType(nsGetList);

            foreach (ItemInfo itemInfo in in_magento_not_ns_items)
            {
                var found = nsGetItemInfos.Find(i => i.sku == itemInfo.sku);

                if (found == null)
                {
                    itemInfo.get_success = false;
                }
                else
                {
                    itemInfo.InsertNSInfo(found);
                }
            }
        }

        private ItemInfo GetItemBySKU(List<ItemInfo> in_magento_not_ns_items, string itemId)
        {
            var items = in_magento_not_ns_items.Where(item => item.sku == itemId);

            if (!items.Any())
            {
                return null;
            }
            return items.First();
        }

        public void GenerateOutputLists()
        {
            all_items = netsuite_item_list.Union(magento_item_list).ToList();
            intersect_items = netsuite_item_list.Intersect(magento_item_list).ToList();
            in_magento_ns_inactive_items = in_magento_not_ns_items.Where(item => item.inactive_in_ns).ToList();
            in_magento_ns_field_not_check_items = in_magento_not_ns_items.Where(item => !item.inactive_in_ns).ToList();
            in_ns_not_exist_magento_items = netsuite_item_list.Where(n => !magento_item_list.Any(m => n.sku.Replace(" ", string.Empty).ToLower() == m.sku.Replace(" ", string.Empty).ToLower())).ToList();
            in_ns_not_live_magento_items = netsuite_item_list.Where(n => !effective_magento_item_list.Any(m => n.sku.Replace(" ", string.Empty).ToLower() == m.sku.Replace(" ", string.Empty).ToLower())).ToList();
            type_diff_items = intersect_items.Where(item => item.status == 1 && itemTypeMap[item.product_type] != item.ns_item_type).ToList();
            internal_id_diff_items = intersect_items.Where(item => item.status == 1 && item.internal_id != item.ns_internal_id).ToList();
            weight_diff_items = GenerateWeightDiffList();

            Console.WriteLine("");
            Console.WriteLine("all_items items: {0}", all_items.Count);
            Console.WriteLine("in_magento_ns_inactive items: {0}", in_magento_ns_inactive_items.Count);
            Console.WriteLine("in_magento_ns_field_not_check items: {0}", in_magento_ns_field_not_check_items.Count);
            Console.WriteLine("in_ns_not_exist_magento items: {0}", in_ns_not_exist_magento_items.Count);
            Console.WriteLine("in_ns_not_live_magento items: {0}", in_ns_not_live_magento_items.Count);
            Console.WriteLine("weight_diff items: {0}", weight_diff_items.Count);
            Console.WriteLine("type_diff items: {0}", type_diff_items.Count);
            Console.WriteLine("internal_id_diff items: {0}", internal_id_diff_items.Count);
            Console.WriteLine("");
        }

        private List<ItemInfo> GenerateWeightDiffList()
        {
            List<ItemInfo> res = new List<ItemInfo>();

            foreach (ItemInfo itemInfo in intersect_items)
            {
                if (Math.Abs(itemInfo.ns_weight - itemInfo.weight) >= 0.01)
                {
                    res.Add(itemInfo);
                }
            }

            return res;
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
            public string internal_id { get; set; }
            public string sku { get; set; }
            public string product_type { get; set; }
            public bool get_success { get; set; }
            public bool inactive_in_ns { get; set; }
            public double ns_weight { get; set; }
            public double weight { get; set; }
            public int status { get; set; }
            public string brand { get; set; }
            public string is_netsuite_item { get; set; }
            public string ns_internal_id { get; set; }
            public string ns_item_type { get; set; }

            internal void InsertNSInfo(ItemInfo ns_item_info)
            {
                internal_id = ns_item_info.internal_id;
                product_type = ns_item_info.product_type;
                inactive_in_ns = ns_item_info.inactive_in_ns;
                ns_weight = ns_item_info.ns_weight;
            }

            internal bool isFulfillableType()
            {
                if (ns_item_type == "Inventory Item" || ns_item_type == "Lot Numbered Assembly")
                {
                    return true;
                }
                return false;
            }
        }
    }
}
