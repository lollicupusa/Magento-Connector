using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MG2Connector
{
    public class MagentoConnector
    {
        public const Int32 WEB_SERVICES_TRY_TIMES_LIMIT = 3;

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

        public IList<M2Order> SearchUnsyncOrders(double duration)
        {
            Console.WriteLine("");
            Console.WriteLine("Magento Search Unsync Orders");

            var endTime = GetDateTime("GMT Standard Time");
            var startTime = endTime.AddHours(-duration);

            var startTimeString = startTime.ToString("yyyy-M-dd HH:mm:ss");
            var endTimeString = endTime.ToString("yyyy-M-dd HH:mm:ss");

            var request = CreateRequest("/rest/V1/orders", Method.GET, Token);

            request.AddQueryParameter("searchCriteria[filter_groups][0][filters][0][field]"          , "created_at"    );
            request.AddQueryParameter("searchCriteria[filter_groups][0][filters][0][value]"          , startTimeString );
            request.AddQueryParameter("searchCriteria[filter_groups][0][filters][0][condition_type]" , "gt"            );
            request.AddQueryParameter("searchCriteria[filter_groups][1][filters][1][field]"          , "created_at"    );
            request.AddQueryParameter("searchCriteria[filter_groups][1][filters][1][value]"          , endTimeString   );
            request.AddQueryParameter("searchCriteria[filter_groups][1][filters][1][condition_type]" , "lt"            );
            
            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Get Unsync Orders Success");
                Console.WriteLine("");
                var m2SearchOrders = JsonConvert.DeserializeObject<M2SearchOrder>(response.Content);
                return m2SearchOrders.orders;
            }
            else
            {
                throw new Exception("Get Magento Order Error");
                return null;
            }
        }

        private DateTime GetDateTime(string timeZoneId)
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo selectedZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, selectedZone);
        }

        private DateTime TransPSTtoGMT(string pstTimeString)
        {
            DateTime pstTime = DateTime.Parse(pstTimeString);
            return pstTime.ToUniversalTime();
        }

        public IList<SearchItem> SearchAllProducts()
        {
            Console.WriteLine("");
            Console.WriteLine("Magento Search All Products");

            var request = CreateRequest("/rest/V1/products", Method.GET, Token);
            request.AddQueryParameter("searchCriteria", "0");

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

        public IList<M2Order> UpdateSyncedOrders(IList<M2Order> ordersToUpdate)
        {
            var failedUpdateOrders = new List<M2Order>();

            Console.WriteLine("");
            Console.WriteLine("-----Product Update: {0} orders-----", ordersToUpdate.Count);

            int count = 0;
            foreach (var m2order in ordersToUpdate)
            {
                Console.WriteLine("Order Update: {0, 4}. {1}", ++count, m2order.increment_id);

                var m2orderUpdateInfo = new M2Order()
                {
                    entity_id = m2order.entity_id,
                    // TO DO: synced field = true
                };

                var response = UpdateOrder(m2order);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Update Failed: {0}", m2order.increment_id);
                    Console.WriteLine("{0}", response.Content);
                    Console.WriteLine("");

                    failedUpdateOrders.Add(m2order);
                }
            }
            Console.WriteLine("");

            return failedUpdateOrders;
        }

        public IRestResponse UpdateOrder(M2Order m2order)
        {
            var request = CreateRequest("/rest/V1/orders/" + m2order.entity_id, Method.PUT, Token);

            string json = JsonConvert.SerializeObject(m2order, Formatting.Indented);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = Client.Execute(request);

            int tryTimes = 0;

            while (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout && tryTimes < WEB_SERVICES_TRY_TIMES_LIMIT)
            {
                Console.WriteLine("Order Update: {0} Again", m2order.entity_id);
                response = Client.Execute(request);
                tryTimes++; // ensure whether exception or not, retry time++ here
                Thread.Sleep(1000);
            }

            return response;
        }
    }
}
