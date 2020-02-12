using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class M2Search
    {
        [JsonProperty("search_criteria")]
        public SearchCriteria search_criteria { get; set; }
    }

    //public class SearchCriteria
    //{
    //    [JsonProperty("filter_groups")]
    //    public FilterGroups filter_groups { get; set; }
    //}

    public class FilterGroups
    {
        [JsonProperty("filters")]
        public IList<Filter> filters { get; set; }
    }

    //public class Filter
    //{
    //    [JsonProperty("status")]
    //    public string status { get; set; }
    //}
}
