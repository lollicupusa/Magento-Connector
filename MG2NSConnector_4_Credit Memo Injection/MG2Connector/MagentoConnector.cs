using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class MagentoConnector
    {
        public const int WEB_SERVICES_TRY_TIMES_LIMIT = 3;

        private RestClient Client { get; set; }

        private string Token { get; set; }

        public MagentoConnector(string magentoUrl, string token)
        {
            Client = new RestClient(magentoUrl);
            Token = token;
        }

        private RestRequest CreateRequest(string endpoint, Method method)
        {
            var request = new RestRequest(endpoint, method);
            request.RequestFormat = DataFormat.Json;
            return request;
        }

        private RestRequest CreateRequest(string endpoint, Method method, string token)
        {
            var request = new RestRequest(endpoint, method);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Accept", "application/json");
            return request;
        }

        public IList<M2CM> SearchUnsyncCreditMemos(double duration)
        {
            Console.WriteLine("");
            Console.WriteLine("Magento Search Unsync Credit Memos"); //creditmemos

            var endTime = getDateTime("GMT Standard Time");
            var startTime = endTime.AddHours(-duration);

            var startTimeString = startTime.ToString("yyyy-M-dd HH:mm:ss");
            var endTimeString = endTime.ToString("yyyy-M-dd HH:mm:ss");

            var request = CreateRequest("/rest/V1/creditmemos", Method.GET, Token);

            request.AddQueryParameter("searchCriteria[filter_groups][0][filters][0][field]", "created_at");
            request.AddQueryParameter("searchCriteria[filter_groups][0][filters][0][value]", startTimeString);
            request.AddQueryParameter("searchCriteria[filter_groups][0][filters][0][condition_type]", "gt");
            request.AddQueryParameter("searchCriteria[filter_groups][1][filters][1][field]", "created_at");
            request.AddQueryParameter("searchCriteria[filter_groups][1][filters][1][value]", endTimeString);
            request.AddQueryParameter("searchCriteria[filter_groups][1][filters][1][condition_type]", "lt");

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Get Unsync Credit Memo Success");
                Console.WriteLine("");
                var m2SearchOrders = JsonConvert.DeserializeObject<M2SearchCM>(response.Content);
                return m2SearchOrders.credit_memos;
            }
            else
            {
                throw new Exception("Get Magento Credit Memo Error");
                return null;
            }
        }

        private DateTime TransPSTtoGMT(string pstTimeString)
        {
            DateTime pstTime = DateTime.Parse(pstTimeString);
            return pstTime.ToUniversalTime();
        }

        private DateTime getDateTime(string timeZoneId)
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo selectedZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, selectedZone);
        }

        public void InsertSONumbers(IList<M2CM> m2CMs)
        {
            Console.WriteLine("");
            Console.WriteLine("Insert SO"); 
            foreach (M2CM m2CM in m2CMs)
            {
                InsertSONumber(m2CM);
            }
            Console.WriteLine("");
        }

        private void InsertSONumber(M2CM m2CM)
        {
            Console.Write("Get order {0} of CM {1}", m2CM.order_id, m2CM.increment_id);

            var request = CreateRequest("/rest/V1/orders/" + m2CM.order_id, Method.GET, Token);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                m2CM.order = JsonConvert.DeserializeObject<M2GetOrder>(response.Content);

                Console.WriteLine("   Get Order {0} Success", m2CM.order.increment_id);
            }
            else
            {
                throw new Exception("Get Magento Sales Order Error");
            }
        }

        public IList<SearchItem> SearchAllProducts()
        {
            Console.WriteLine("");
            Console.WriteLine("Magento Search All Products");

            var request = CreateRequest("/rest/V1/products?searchCriteria=0", Method.GET, Token);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Get All Products Success");
                Console.WriteLine("");
                var m2SearchProducts = JsonConvert.DeserializeObject<M2SearchProducts>(response.Content);
                return m2SearchProducts.items;
            }
            else
            {
                Console.WriteLine("Get All Products Failed");
                Console.WriteLine(response.StatusCode);
                return null;
            }
        }

        public M2Attribute GetAttribute(string attribute)
        {
            Console.WriteLine("");
            Console.WriteLine("Magento Get Attribute: " + attribute);

            var request = CreateRequest("/rest/V1/products/attributes/" + attribute, Method.GET, Token);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Get Attribute Success");
                Console.WriteLine("");
                var m2Attribute = JsonConvert.DeserializeObject<M2Attribute>(response.Content);
                return m2Attribute;
            }
            else
            {
                Console.WriteLine("Get Attribute Failed");
                Console.WriteLine(response.StatusCode);
                return null;
            }
        }

        public IList<M2Attribute> GetAttributes(IList<string> attributes)
        {
            List<M2Attribute> m2attributes = new List<M2Attribute>();

            foreach (string attribute in attributes)
            {
                m2attributes.Add(GetAttribute(attribute));
            }

            return m2attributes;
        }
    }
}
