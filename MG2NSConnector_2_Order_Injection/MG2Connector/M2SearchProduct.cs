using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class SearchItem
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("sku")]
        public string sku { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("attribute_set_id")]
        public int attribute_set_id { get; set; }

        [JsonProperty("price")]
        public double price { get; set; }

        [JsonProperty("status")]
        public int status { get; set; }

        [JsonProperty("visibility")]
        public int visibility { get; set; }

        [JsonProperty("type_id")]
        public string type_id { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("updated_at")]
        public string updated_at { get; set; }

        [JsonProperty("weight")]
        public double weight { get; set; }

        [JsonProperty("product_links")]
        public IList<ProductLink> product_links { get; set; }

        [JsonProperty("tier_prices")]
        public IList<TierPrice> tier_prices { get; set; }

        [JsonProperty("custom_attributes")]
        public IList<CustomAttribute> custom_attributes { get; set; }
    }

    public class M2SearchProducts
    {
        [JsonProperty("items")]
        public IList<SearchItem> items { get; set; }

        [JsonProperty("search_criteria")]
        public SearchCriteria search_criteria { get; set; }

        [JsonProperty("total_count")]
        public int total_count { get; set; }
    }
}
