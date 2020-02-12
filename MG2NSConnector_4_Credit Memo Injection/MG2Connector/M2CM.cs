using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class Item
    {

        [JsonProperty("base_cost")]
        public string base_cost { get; set; }

        [JsonProperty("base_discount_amount")]
        public double base_discount_amount { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public int base_discount_tax_compensation_amount { get; set; }

        [JsonProperty("base_price")]
        public double base_price { get; set; }

        [JsonProperty("base_price_incl_tax")]
        public double base_price_incl_tax { get; set; }

        [JsonProperty("base_row_total")]
        public double base_row_total { get; set; }

        [JsonProperty("base_row_total_incl_tax")]
        public double base_row_total_incl_tax { get; set; }

        [JsonProperty("base_tax_amount")]
        public double base_tax_amount { get; set; }

        [JsonProperty("base_weee_tax_applied_amount")]
        public int base_weee_tax_applied_amount { get; set; }

        [JsonProperty("base_weee_tax_applied_row_amnt")]
        public int base_weee_tax_applied_row_amnt { get; set; }

        [JsonProperty("base_weee_tax_disposition")]
        public int base_weee_tax_disposition { get; set; }

        [JsonProperty("base_weee_tax_row_disposition")]
        public int base_weee_tax_row_disposition { get; set; }

        [JsonProperty("discount_amount")]
        public double discount_amount { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public int discount_tax_compensation_amount { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("order_item_id")]
        public int order_item_id { get; set; }

        [JsonProperty("parent_id")]
        public int parent_id { get; set; }

        [JsonProperty("price")]
        public double price { get; set; }

        [JsonProperty("price_incl_tax")]
        public double price_incl_tax { get; set; }

        [JsonProperty("product_id")]
        public int product_id { get; set; }

        [JsonProperty("qty")]
        public int qty { get; set; }

        [JsonProperty("row_total")]
        public double row_total { get; set; }

        [JsonProperty("row_total_incl_tax")]
        public double row_total_incl_tax { get; set; }

        [JsonProperty("sku")]
        public string sku { get; set; }

        [JsonProperty("tax_amount")]
        public double tax_amount { get; set; }

        [JsonProperty("weee_tax_applied")]
        public string weee_tax_applied { get; set; }

        [JsonProperty("weee_tax_applied_amount")]
        public int weee_tax_applied_amount { get; set; }

        [JsonProperty("weee_tax_applied_row_amount")]
        public int weee_tax_applied_row_amount { get; set; }

        [JsonProperty("weee_tax_disposition")]
        public int weee_tax_disposition { get; set; }

        [JsonProperty("weee_tax_row_disposition")]
        public int weee_tax_row_disposition { get; set; }
    }

    public class Comment
    {

        [JsonProperty("comment")]
        public string comment { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("is_customer_notified")]
        public int is_customer_notified { get; set; }

        [JsonProperty("is_visible_on_front")]
        public int is_visible_on_front { get; set; }

        [JsonProperty("parent_id")]
        public int parent_id { get; set; }
    }

    public class M2CM
    {
        public M2GetOrder order { get; set; }

        [JsonProperty("adjustment")]
        public double adjustment { get; set; }

        [JsonProperty("adjustment_negative")]
        public int adjustment_negative { get; set; }

        [JsonProperty("adjustment_positive")]
        public double adjustment_positive { get; set; }

        [JsonProperty("base_adjustment")]
        public double base_adjustment { get; set; }

        [JsonProperty("base_adjustment_negative")]
        public int base_adjustment_negative { get; set; }

        [JsonProperty("base_adjustment_positive")]
        public double base_adjustment_positive { get; set; }

        [JsonProperty("base_currency_code")]
        public string base_currency_code { get; set; }

        [JsonProperty("base_discount_amount")]
        public double base_discount_amount { get; set; }

        [JsonProperty("base_grand_total")]
        public double base_grand_total { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public int base_discount_tax_compensation_amount { get; set; }

        [JsonProperty("base_shipping_amount")]
        public double base_shipping_amount { get; set; }

        [JsonProperty("base_shipping_incl_tax")]
        public double base_shipping_incl_tax { get; set; }

        [JsonProperty("base_shipping_tax_amount")]
        public int base_shipping_tax_amount { get; set; }

        [JsonProperty("base_subtotal")]
        public double base_subtotal { get; set; }

        [JsonProperty("base_subtotal_incl_tax")]
        public double base_subtotal_incl_tax { get; set; }

        [JsonProperty("base_tax_amount")]
        public double base_tax_amount { get; set; }

        [JsonProperty("base_to_global_rate")]
        public int base_to_global_rate { get; set; }

        [JsonProperty("base_to_order_rate")]
        public int base_to_order_rate { get; set; }

        [JsonProperty("billing_address_id")]
        public int billing_address_id { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("discount_amount")]
        public double discount_amount { get; set; }

        [JsonProperty("discount_description")]
        public string discount_description { get; set; }

        [JsonProperty("email_sent")]
        public int email_sent { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("global_currency_code")]
        public string global_currency_code { get; set; }

        [JsonProperty("grand_total")]
        public double grand_total { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public int discount_tax_compensation_amount { get; set; }

        [JsonProperty("increment_id")]
        public string increment_id { get; set; }

        [JsonProperty("invoice_id")]
        public int invoice_id { get; set; }

        [JsonProperty("order_currency_code")]
        public string order_currency_code { get; set; }

        [JsonProperty("order_id")]
        public string order_id { get; set; }

        [JsonProperty("shipping_address_id")]
        public int shipping_address_id { get; set; }

        [JsonProperty("shipping_amount")]
        public double shipping_amount { get; set; }

        [JsonProperty("shipping_incl_tax")]
        public double shipping_incl_tax { get; set; }

        [JsonProperty("shipping_tax_amount")]
        public int shipping_tax_amount { get; set; }

        [JsonProperty("state")]
        public int state { get; set; }

        [JsonProperty("store_currency_code")]
        public string store_currency_code { get; set; }

        [JsonProperty("store_id")]
        public int store_id { get; set; }

        [JsonProperty("store_to_base_rate")]
        public int store_to_base_rate { get; set; }

        [JsonProperty("store_to_order_rate")]
        public int store_to_order_rate { get; set; }

        [JsonProperty("subtotal")]
        public double subtotal { get; set; }

        [JsonProperty("subtotal_incl_tax")]
        public double subtotal_incl_tax { get; set; }

        [JsonProperty("tax_amount")]
        public double tax_amount { get; set; }

        [JsonProperty("transaction_id")]
        public string transaction_id { get; set; }

        [JsonProperty("updated_at")]
        public string updated_at { get; set; }

        [JsonProperty("items")]
        public IList<Item> items { get; set; }

        [JsonProperty("comments")]
        public IList<Comment> comments { get; set; }
    }

    public class Filter
    {

        [JsonProperty("field")]
        public string field { get; set; }

        [JsonProperty("value")]
        public string value { get; set; }

        [JsonProperty("condition_type")]
        public string condition_type { get; set; }
    }

    public class FilterGroup
    {

        [JsonProperty("filters")]
        public IList<Filter> filters { get; set; }
    }

    public class SearchCriteria
    {

        [JsonProperty("filter_groups")]
        public IList<FilterGroup> filter_groups { get; set; }
    }

    public class M2SearchCM
    {

        [JsonProperty("items")]
        public IList<M2CM> credit_memos { get; set; }

        [JsonProperty("search_criteria")]
        public SearchCriteria search_criteria { get; set; }

        [JsonProperty("total_count")]
        public int total_count { get; set; }
    }
}
