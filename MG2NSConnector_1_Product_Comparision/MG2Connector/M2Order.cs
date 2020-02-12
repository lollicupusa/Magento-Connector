using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class BillingAddress
    {

        [JsonProperty("address_type")]
        public string address_type { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

        [JsonProperty("country_id")]
        public string country_id { get; set; }

        [JsonProperty("customer_address_id")]
        public int customer_address_id { get; set; }

        [JsonProperty("customer_id")]
        public int customer_id { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("firstname")]
        public string firstname { get; set; }

        [JsonProperty("lastname")]
        public string lastname { get; set; }

        [JsonProperty("parent_id")]
        public int parent_id { get; set; }

        [JsonProperty("postcode")]
        public string postcode { get; set; }

        [JsonProperty("region")]
        public string region { get; set; }

        [JsonProperty("region_code")]
        public string region_code { get; set; }

        [JsonProperty("region_id")]
        public int region_id { get; set; }

        [JsonProperty("street")]
        public IList<string> street { get; set; }

        [JsonProperty("telephone")]
        public string telephone { get; set; }

        [JsonProperty("middlename")]
        public string middlename { get; set; }
    }

    public class Payment
    {

        [JsonProperty("account_status")]
        public object account_status { get; set; }

        [JsonProperty("additional_data")]
        public string additional_data { get; set; }

        [JsonProperty("additional_information")]
        public IList<object> additional_information { get; set; }

        [JsonProperty("amount_ordered")]
        public double amount_ordered { get; set; }

        [JsonProperty("amount_paid")]
        public double amount_paid { get; set; }

        [JsonProperty("amount_refunded")]
        public double amount_refunded { get; set; }

        [JsonProperty("base_amount_authorized")]
        public double base_amount_authorized { get; set; }

        [JsonProperty("base_amount_ordered")]
        public double base_amount_ordered { get; set; }

        [JsonProperty("base_amount_paid")]
        public double base_amount_paid { get; set; }

        [JsonProperty("base_amount_paid_online")]
        public double base_amount_paid_online { get; set; }

        [JsonProperty("base_amount_refunded")]
        public double base_amount_refunded { get; set; }

        [JsonProperty("base_shipping_amount")]
        public double base_shipping_amount { get; set; }

        [JsonProperty("base_shipping_captured")]
        public double base_shipping_captured { get; set; }

        [JsonProperty("base_shipping_refunded")]
        public double base_shipping_refunded { get; set; }

        [JsonProperty("cc_approval")]
        public string cc_approval { get; set; }

        [JsonProperty("cc_avs_status")]
        public string cc_avs_status { get; set; }

        [JsonProperty("cc_cid_status")]
        public string cc_cid_status { get; set; }

        [JsonProperty("cc_exp_month")]
        public string cc_exp_month { get; set; }

        [JsonProperty("cc_exp_year")]
        public string cc_exp_year { get; set; }

        [JsonProperty("cc_last4")]
        public string cc_last4 { get; set; }

        [JsonProperty("cc_trans_id")]
        public string cc_trans_id { get; set; }

        [JsonProperty("cc_type")]
        public string cc_type { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("last_trans_id")]
        public string last_trans_id { get; set; }

        [JsonProperty("method")]
        public string method { get; set; }

        [JsonProperty("parent_id")]
        public int parent_id { get; set; }

        [JsonProperty("shipping_amount")]
        public double shipping_amount { get; set; }

        [JsonProperty("shipping_captured")]
        public double shipping_captured { get; set; }

        [JsonProperty("shipping_refunded")]
        public double shipping_refunded { get; set; }
    }

    public class StatusHistory
    {

        [JsonProperty("comment")]
        public string comment { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("entity_name")]
        public string entity_name { get; set; }

        [JsonProperty("is_customer_notified")]
        public string is_customer_notified { get; set; }

        [JsonProperty("is_visible_on_front")]
        public int is_visible_on_front { get; set; }

        [JsonProperty("parent_id")]
        public int parent_id { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }
    }

    public class Address
    {

        [JsonProperty("address_type")]
        public string address_type { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

        [JsonProperty("country_id")]
        public string country_id { get; set; }

        [JsonProperty("customer_address_id")]
        public int customer_address_id { get; set; }

        [JsonProperty("customer_id")]
        public int customer_id { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("firstname")]
        public string firstname { get; set; }

        [JsonProperty("lastname")]
        public string lastname { get; set; }

        [JsonProperty("parent_id")]
        public int parent_id { get; set; }

        [JsonProperty("postcode")]
        public string postcode { get; set; }

        [JsonProperty("region")]
        public string region { get; set; }

        [JsonProperty("region_code")]
        public string region_code { get; set; }

        [JsonProperty("region_id")]
        public int region_id { get; set; }

        [JsonProperty("street")]
        public IList<string> street { get; set; }

        [JsonProperty("telephone")]
        public string telephone { get; set; }

        [JsonProperty("middlename")]
        public string middlename { get; set; }
    }

    public class Total
    {

        [JsonProperty("base_shipping_amount")]
        public double base_shipping_amount { get; set; }

        [JsonProperty("base_shipping_discount_amount")]
        public double base_shipping_discount_amount { get; set; }

        [JsonProperty("base_shipping_discount_tax_compensation_amnt")]
        public double base_shipping_discount_tax_compensation_amnt { get; set; }

        [JsonProperty("base_shipping_incl_tax")]
        public double base_shipping_incl_tax { get; set; }

        [JsonProperty("base_shipping_invoiced")]
        public double base_shipping_invoiced { get; set; }

        [JsonProperty("base_shipping_refunded")]
        public double base_shipping_refunded { get; set; }

        [JsonProperty("base_shipping_tax_amount")]
        public double base_shipping_tax_amount { get; set; }

        [JsonProperty("base_shipping_tax_refunded")]
        public double base_shipping_tax_refunded { get; set; }

        [JsonProperty("shipping_amount")]
        public double shipping_amount { get; set; }

        [JsonProperty("shipping_discount_amount")]
        public double shipping_discount_amount { get; set; }

        [JsonProperty("shipping_discount_tax_compensation_amount")]
        public double shipping_discount_tax_compensation_amount { get; set; }

        [JsonProperty("shipping_incl_tax")]
        public double shipping_incl_tax { get; set; }

        [JsonProperty("shipping_invoiced")]
        public double shipping_invoiced { get; set; }

        [JsonProperty("shipping_refunded")]
        public double shipping_refunded { get; set; }

        [JsonProperty("shipping_tax_amount")]
        public double shipping_tax_amount { get; set; }

        [JsonProperty("shipping_tax_refunded")]
        public double shipping_tax_refunded { get; set; }
    }

    public class Shipping
    {

        [JsonProperty("address")]
        public Address address { get; set; }

        [JsonProperty("method")]
        public string method { get; set; }

        [JsonProperty("total")]
        public Total total { get; set; }
    }

    public class Item
    {
        [JsonProperty("amount_refunded")]
        public double amount_refunded { get; set; }

        [JsonProperty("applied_rule_ids")]
        public string applied_rule_ids { get; set; }

        [JsonProperty("base_amount_refunded")]
        public double base_amount_refunded { get; set; }

        [JsonProperty("base_cost")]
        public double base_cost { get; set; }

        [JsonProperty("base_discount_amount")]
        public double base_discount_amount { get; set; }

        [JsonProperty("base_discount_invoiced")]
        public double base_discount_invoiced { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public double base_discount_tax_compensation_amount { get; set; }

        [JsonProperty("base_discount_tax_compensation_invoiced")]
        public double base_discount_tax_compensation_invoiced { get; set; }

        [JsonProperty("base_original_price")]
        public double base_original_price { get; set; }

        [JsonProperty("base_price")]
        public double base_price { get; set; }

        [JsonProperty("base_price_incl_tax")]
        public double base_price_incl_tax { get; set; }

        [JsonProperty("base_row_invoiced")]
        public double base_row_invoiced { get; set; }

        [JsonProperty("base_row_total")]
        public double base_row_total { get; set; }

        [JsonProperty("base_row_total_incl_tax")]
        public double base_row_total_incl_tax { get; set; }

        [JsonProperty("base_tax_amount")]
        public double base_tax_amount { get; set; }

        [JsonProperty("base_tax_invoiced")]
        public double base_tax_invoiced { get; set; }

        [JsonProperty("base_weee_tax_applied_amount")]
        public double base_weee_tax_applied_amount { get; set; }

        [JsonProperty("base_weee_tax_applied_row_amnt")]
        public double base_weee_tax_applied_row_amnt { get; set; }

        [JsonProperty("base_weee_tax_disposition")]
        public double base_weee_tax_disposition { get; set; }

        [JsonProperty("base_weee_tax_row_disposition")]
        public double base_weee_tax_row_disposition { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("discount_amount")]
        public double discount_amount { get; set; }

        [JsonProperty("discount_invoiced")]
        public double discount_invoiced { get; set; }

        [JsonProperty("discount_percent")]
        public double discount_percent { get; set; }

        [JsonProperty("free_shipping")]
        public int free_shipping { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public double discount_tax_compensation_amount { get; set; }

        [JsonProperty("discount_tax_compensation_invoiced")]
        public double discount_tax_compensation_invoiced { get; set; }

        [JsonProperty("is_qty_decimal")]
        public int is_qty_decimal { get; set; }

        [JsonProperty("is_virtual")]
        public int is_virtual { get; set; }

        [JsonProperty("item_id")]
        public int item_id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("no_discount")]
        public int no_discount { get; set; }

        [JsonProperty("order_id")]
        public int order_id { get; set; }

        [JsonProperty("original_price")]
        public double original_price { get; set; }

        [JsonProperty("price")]
        public double price { get; set; }

        [JsonProperty("price_incl_tax")]
        public double price_incl_tax { get; set; }

        [JsonProperty("product_id")]
        public int product_id { get; set; }

        [JsonProperty("product_type")]
        public string product_type { get; set; }

        [JsonProperty("qty_canceled")]
        public int qty_canceled { get; set; }

        [JsonProperty("qty_invoiced")]
        public int qty_invoiced { get; set; }

        [JsonProperty("qty_ordered")]
        public int qty_ordered { get; set; }

        [JsonProperty("qty_refunded")]
        public int qty_refunded { get; set; }

        [JsonProperty("qty_shipped")]
        public int qty_shipped { get; set; }

        [JsonProperty("quote_item_id")]
        public int quote_item_id { get; set; }

        [JsonProperty("row_invoiced")]
        public double row_invoiced { get; set; }

        [JsonProperty("row_total")]
        public double row_total { get; set; }

        [JsonProperty("row_total_incl_tax")]
        public double row_total_incl_tax { get; set; }

        [JsonProperty("row_weight")]
        public double row_weight { get; set; }

        [JsonProperty("sku")]
        public string sku { get; set; }

        [JsonProperty("store_id")]
        public int store_id { get; set; }

        [JsonProperty("tax_amount")]
        public double tax_amount { get; set; }

        [JsonProperty("tax_invoiced")]
        public double tax_invoiced { get; set; }

        [JsonProperty("tax_percent")]
        public double tax_percent { get; set; }

        [JsonProperty("updated_at")]
        public string updated_at { get; set; }

        [JsonProperty("weee_tax_applied")]
        public string weee_tax_applied { get; set; }

        [JsonProperty("weee_tax_applied_amount")]
        public double weee_tax_applied_amount { get; set; }

        [JsonProperty("weee_tax_applied_row_amount")]
        public double weee_tax_applied_row_amount { get; set; }

        [JsonProperty("weee_tax_disposition")]
        public double weee_tax_disposition { get; set; }

        [JsonProperty("weee_tax_row_disposition")]
        public double weee_tax_row_disposition { get; set; }

        [JsonProperty("weight")]
        public double weight { get; set; }

        [JsonProperty("base_discount_refunded")]
        public double base_discount_refunded { get; set; }

        [JsonProperty("base_discount_tax_compensation_refunded")]
        public double base_discount_tax_compensation_refunded { get; set; }

        [JsonProperty("base_tax_refunded")]
        public double? base_tax_refunded { get; set; }

        [JsonProperty("discount_refunded")]
        public double discount_refunded { get; set; }

        [JsonProperty("discount_tax_compensation_refunded")]
        public double discount_tax_compensation_refunded { get; set; }

        [JsonProperty("tax_refunded")]
        public double? tax_refunded { get; set; }
    }

    public class ShippingAssignment
    {

        [JsonProperty("shipping")]
        public Shipping shipping { get; set; }

        [JsonProperty("items")]
        public IList<Item> items { get; set; }
    }

    public class OrderExtensionAttributes
    {
        [JsonProperty("shipping_assignments")]
        public IList<ShippingAssignment> shipping_assignments { get; set; }
    }

    public class Order
    {
        [JsonProperty("adjustment_negative")]
        public double adjustment_negative { get; set; }

        [JsonProperty("adjustment_positive")]
        public double adjustment_positive { get; set; }

        [JsonProperty("applied_rule_ids")]
        public string applied_rule_ids { get; set; }

        [JsonProperty("base_adjustment_negative")]
        public double base_adjustment_negative { get; set; }

        [JsonProperty("base_adjustment_positive")]
        public double base_adjustment_positive { get; set; }

        [JsonProperty("base_currency_code")]
        public string base_currency_code { get; set; }

        [JsonProperty("base_discount_amount")]
        public double base_discount_amount { get; set; }

        [JsonProperty("base_discount_invoiced")]
        public double base_discount_invoiced { get; set; }

        [JsonProperty("base_discount_refunded")]
        public double base_discount_refunded { get; set; }

        [JsonProperty("base_grand_total")]
        public double base_grand_total { get; set; }

        [JsonProperty("base_discount_tax_compensation_amount")]
        public double base_discount_tax_compensation_amount { get; set; }

        [JsonProperty("base_discount_tax_compensation_invoiced")]
        public double base_discount_tax_compensation_invoiced { get; set; }

        [JsonProperty("base_discount_tax_compensation_refunded")]
        public double base_discount_tax_compensation_refunded { get; set; }

        [JsonProperty("base_shipping_amount")]
        public double base_shipping_amount { get; set; }

        [JsonProperty("base_shipping_discount_amount")]
        public double base_shipping_discount_amount { get; set; }

        [JsonProperty("base_shipping_discount_tax_compensation_amnt")]
        public double base_shipping_discount_tax_compensation_amnt { get; set; }

        [JsonProperty("base_shipping_incl_tax")]
        public double base_shipping_incl_tax { get; set; }

        [JsonProperty("base_shipping_invoiced")]
        public double base_shipping_invoiced { get; set; }

        [JsonProperty("base_shipping_refunded")]
        public double base_shipping_refunded { get; set; }

        [JsonProperty("base_shipping_tax_amount")]
        public double base_shipping_tax_amount { get; set; }

        [JsonProperty("base_shipping_tax_refunded")]
        public double base_shipping_tax_refunded { get; set; }

        [JsonProperty("base_subtotal")]
        public double base_subtotal { get; set; }

        [JsonProperty("base_subtotal_incl_tax")]
        public double base_subtotal_incl_tax { get; set; }

        [JsonProperty("base_subtotal_invoiced")]
        public double base_subtotal_invoiced { get; set; }

        [JsonProperty("base_subtotal_refunded")]
        public double base_subtotal_refunded { get; set; }

        [JsonProperty("base_tax_amount")]
        public double base_tax_amount { get; set; }

        [JsonProperty("base_tax_invoiced")]
        public double base_tax_invoiced { get; set; }

        [JsonProperty("base_tax_refunded")]
        public double base_tax_refunded { get; set; }

        [JsonProperty("base_total_due")]
        public double base_total_due { get; set; }

        [JsonProperty("base_total_invoiced")]
        public double base_total_invoiced { get; set; }

        [JsonProperty("base_total_invoiced_cost")]
        public double base_total_invoiced_cost { get; set; }

        [JsonProperty("base_total_offline_refunded")]
        public double base_total_offline_refunded { get; set; }

        [JsonProperty("base_total_paid")]
        public double base_total_paid { get; set; }

        [JsonProperty("base_total_refunded")]
        public double base_total_refunded { get; set; }

        [JsonProperty("base_to_global_rate")]
        public double base_to_global_rate { get; set; }

        [JsonProperty("base_to_order_rate")]
        public double base_to_order_rate { get; set; }

        [JsonProperty("billing_address_id")]
        public int billing_address_id { get; set; }

        [JsonProperty("coupon_code")]
        public string coupon_code { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("customer_email")]
        public string customer_email { get; set; }

        [JsonProperty("customer_firstname")]
        public string customer_firstname { get; set; }

        [JsonProperty("customer_group_id")]
        public int customer_group_id { get; set; }

        [JsonProperty("customer_id")]
        public int customer_id { get; set; }

        [JsonProperty("customer_is_guest")]
        public int customer_is_guest { get; set; }

        [JsonProperty("customer_lastname")]
        public string customer_lastname { get; set; }

        [JsonProperty("customer_note_notify")]
        public int customer_note_notify { get; set; }

        [JsonProperty("discount_amount")]
        public double discount_amount { get; set; }

        [JsonProperty("discount_description")]
        public string discount_description { get; set; }

        [JsonProperty("discount_invoiced")]
        public double discount_invoiced { get; set; }

        [JsonProperty("discount_refunded")]
        public double discount_refunded { get; set; }

        [JsonProperty("email_sent")]
        public int email_sent { get; set; }

        [JsonProperty("entity_id")]
        public int entity_id { get; set; }

        [JsonProperty("global_currency_code")]
        public string global_currency_code { get; set; }

        [JsonProperty("grand_total")]
        public double grand_total { get; set; }

        [JsonProperty("discount_tax_compensation_amount")]
        public double discount_tax_compensation_amount { get; set; }

        [JsonProperty("discount_tax_compensation_invoiced")]
        public double discount_tax_compensation_invoiced { get; set; }

        [JsonProperty("discount_tax_compensation_refunded")]
        public double discount_tax_compensation_refunded { get; set; }

        [JsonProperty("increment_id")]
        public string increment_id { get; set; }

        [JsonProperty("is_virtual")]
        public int is_virtual { get; set; }

        [JsonProperty("order_currency_code")]
        public string order_currency_code { get; set; }

        [JsonProperty("protect_code")]
        public string protect_code { get; set; }

        [JsonProperty("quote_id")]
        public int quote_id { get; set; }

        [JsonProperty("remote_ip")]
        public string remote_ip { get; set; }

        [JsonProperty("shipping_amount")]
        public double shipping_amount { get; set; }

        [JsonProperty("shipping_description")]
        public string shipping_description { get; set; }

        [JsonProperty("shipping_discount_amount")]
        public double shipping_discount_amount { get; set; }

        [JsonProperty("shipping_discount_tax_compensation_amount")]
        public double shipping_discount_tax_compensation_amount { get; set; }

        [JsonProperty("shipping_incl_tax")]
        public double shipping_incl_tax { get; set; }

        [JsonProperty("shipping_invoiced")]
        public double shipping_invoiced { get; set; }

        [JsonProperty("shipping_refunded")]
        public double shipping_refunded { get; set; }

        [JsonProperty("shipping_tax_amount")]
        public double shipping_tax_amount { get; set; }

        [JsonProperty("shipping_tax_refunded")]
        public double shipping_tax_refunded { get; set; }

        [JsonProperty("state")]
        public string state { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("store_currency_code")]
        public string store_currency_code { get; set; }

        [JsonProperty("store_id")]
        public int store_id { get; set; }

        [JsonProperty("store_name")]
        public string store_name { get; set; }

        [JsonProperty("store_to_base_rate")]
        public double store_to_base_rate { get; set; }

        [JsonProperty("store_to_order_rate")]
        public double store_to_order_rate { get; set; }

        [JsonProperty("subtotal")]
        public double subtotal { get; set; }

        [JsonProperty("subtotal_incl_tax")]
        public double subtotal_incl_tax { get; set; }

        [JsonProperty("subtotal_invoiced")]
        public double subtotal_invoiced { get; set; }

        [JsonProperty("subtotal_refunded")]
        public double subtotal_refunded { get; set; }

        [JsonProperty("tax_amount")]
        public double tax_amount { get; set; }

        [JsonProperty("tax_invoiced")]
        public double tax_invoiced { get; set; }

        [JsonProperty("tax_refunded")]
        public double tax_refunded { get; set; }

        [JsonProperty("total_due")]
        public double total_due { get; set; }

        [JsonProperty("total_invoiced")]
        public double total_invoiced { get; set; }

        [JsonProperty("total_item_count")]
        public int total_item_count { get; set; }

        [JsonProperty("total_offline_refunded")]
        public double total_offline_refunded { get; set; }

        [JsonProperty("total_paid")]
        public double total_paid { get; set; }

        [JsonProperty("total_qty_ordered")]
        public int total_qty_ordered { get; set; }

        [JsonProperty("total_refunded")]
        public double total_refunded { get; set; }

        [JsonProperty("updated_at")]
        public string updated_at { get; set; }

        [JsonProperty("weight")]
        public double weight { get; set; }

        [JsonProperty("x_forwarded_for")]
        public string x_forwarded_for { get; set; }

        [JsonProperty("items")]
        public IList<Item> items { get; set; }

        [JsonProperty("billing_address")]
        public BillingAddress billing_address { get; set; }

        [JsonProperty("payment")]
        public Payment payment { get; set; }

        [JsonProperty("status_histories")]
        public IList<StatusHistory> status_histories { get; set; }

        [JsonProperty("extension_attributes")]
        public OrderExtensionAttributes extension_attributes { get; set; }

        [JsonProperty("customer_middlename")]
        public string customer_middlename { get; set; }
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

    public class M2SearchOrder
    {
        [JsonProperty("items")]
        public IList<Order> orders { get; set; }

        [JsonProperty("search_criteria")]
        public SearchCriteria search_criteria { get; set; }

        [JsonProperty("total_count")]
        public int total_count { get; set; }
    }

}
