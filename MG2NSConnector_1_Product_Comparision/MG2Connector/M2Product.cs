using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class CategoryLink
    {

        [JsonProperty("position")]
        public int position { get; set; }

        [JsonProperty("category_id")]
        public string category_id { get; set; }
    }

    public class StockItem
    {
        [JsonProperty("item_id")]
        public int item_id { get; set; }

        [JsonProperty("product_id")]
        public int product_id { get; set; }

        [JsonProperty("stock_id")]
        public int stock_id { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("is_in_stock")]
        public bool is_in_stock { get; set; }

        [JsonProperty("is_qty_decimal")]
        public bool is_qty_decimal { get; set; }

        [JsonProperty("show_default_notification_message")]
        public bool show_default_notification_message { get; set; }

        [JsonProperty("use_config_min_qty")]
        public bool use_config_min_qty { get; set; }

        [JsonProperty("min_qty")]
        public int min_qty { get; set; }

        [JsonProperty("use_config_min_sale_qty")]
        public int use_config_min_sale_qty { get; set; }

        [JsonProperty("min_sale_qty")]
        public int min_sale_qty { get; set; }

        [JsonProperty("use_config_max_sale_qty")]
        public bool use_config_max_sale_qty { get; set; }

        [JsonProperty("max_sale_qty")]
        public int max_sale_qty { get; set; }

        [JsonProperty("use_config_backorders")]
        public bool use_config_backorders { get; set; }

        [JsonProperty("backorders")]
        public int backorders { get; set; }

        [JsonProperty("use_config_notify_stock_qty")]
        public bool use_config_notify_stock_qty { get; set; }

        [JsonProperty("notify_stock_qty")]
        public int notify_stock_qty { get; set; }

        [JsonProperty("use_config_qty_increments")]
        public bool use_config_qty_increments { get; set; }

        [JsonProperty("qty_increments")]
        public int qty_increments { get; set; }

        [JsonProperty("use_config_enable_qty_inc")]
        public bool use_config_enable_qty_inc { get; set; }

        [JsonProperty("enable_qty_increments")]
        public bool enable_qty_increments { get; set; }

        [JsonProperty("use_config_manage_stock")]
        public bool use_config_manage_stock { get; set; }

        [JsonProperty("manage_stock")]
        public bool manage_stock { get; set; }

        [JsonProperty("low_stock_date")]
        public object low_stock_date { get; set; }

        [JsonProperty("is_decimal_divided")]
        public bool is_decimal_divided { get; set; }

        [JsonProperty("stock_status_changed_auto")]
        public int stock_status_changed_auto { get; set; }
    }

    public class ExtensionAttributes
    {

        [JsonProperty("website_ids")]
        public IList<int> website_ids { get; set; }

        [JsonProperty("category_links")]
        public IList<CategoryLink> category_links { get; set; }

        [JsonProperty("stock_item")]
        public StockItem stock_item { get; set; }
    }

    public class ProductLink
    {

        [JsonProperty("sku")]
        public string sku { get; set; }

        [JsonProperty("link_type")]
        public string link_type { get; set; }

        [JsonProperty("linked_product_sku")]
        public string linked_product_sku { get; set; }

        [JsonProperty("linked_product_type")]
        public string linked_product_type { get; set; }

        [JsonProperty("position")]
        public int position { get; set; }
    }

    public class MediaGalleryEntry
    {

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("media_type")]
        public string media_type { get; set; }

        [JsonProperty("label")]
        public object label { get; set; }

        [JsonProperty("position")]
        public int position { get; set; }

        [JsonProperty("disabled")]
        public bool disabled { get; set; }

        [JsonProperty("types")]
        public IList<string> types { get; set; }

        [JsonProperty("file")]
        public string file { get; set; }
    }

    public class TierPrice
    {

        [JsonProperty("customer_group_id")]
        public int customer_group_id { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("value")]
        public double value { get; set; }

        [JsonProperty("extension_attributes")]
        public ExtensionAttributes extension_attributes { get; set; }
    }

    public class CustomAttribute
    {

        [JsonProperty("attribute_code")]
        public string attribute_code { get; set; }

        [JsonProperty("value")]
        public object value { get; set; }
    }

    public class M2Product
    {
        [JsonProperty("product")]
        public Product product { get; set; }
    }

    public class Product
    {
        [JsonProperty("price")]
        public double price { get; set; }
    }
}
