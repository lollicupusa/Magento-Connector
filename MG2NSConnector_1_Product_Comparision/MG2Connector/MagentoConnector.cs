using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2Connector
{
    public class MagentoConnector
    {
        private RestClient Client { get; set; }

        private string Token { get; set; }

        public MagentoConnector(string magentoUrl)
        {
            Client = new RestClient(magentoUrl);
        }

        public MagentoConnector(string magentoUrl, string token)
        {
            Client = new RestClient(magentoUrl);
            Token = token;
        }

        public string GetAdminToken(string userName, string passWord)
        {
            var request = CreateRequest("/rest/V1/integration/admin/token", Method.POST);
            var user = new Credentials(userName, passWord);

            string json = JsonConvert.SerializeObject(user, Formatting.Indented);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = Client.Execute(request);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Content;
            }
            else
            {
                return "";
            }
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

        public M2Product GetSku(string token, string sku)
        {
            var request = CreateRequest("/rest/V1/products/" + sku, Method.GET, token);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //return response.Content;
                return JsonConvert.DeserializeObject<M2Product>(response.Content);
            }
            else
            {
                return null;
            }
        }

        public M2Product GetSku(string sku)
        {
            var request = CreateRequest("/rest/V1/products/" + sku, Method.GET, Token);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //return response.Content;
                return JsonConvert.DeserializeObject<M2Product>(response.Content);
            }
            else
            {
                return null;
            }
        }

        public string GetOrder(string orderId)
        {
            var request = CreateRequest("/rest/V1/orders/" + orderId, Method.GET, Token);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //return JsonConvert.DeserializeObject<M2Order>(response.Content);
                return response.Content;
            }
            else
            {
                return null;
            }
        }

        public string PutSku(string token, string sku)
        {
            var request = CreateRequest("/rest/V1/products/" + sku, Method.PUT, token);

            var m2product = new M2Product()
            {
                product = new Product()
                {
                    price = 777
                }
            };

            string json = JsonConvert.SerializeObject(m2product, Formatting.Indented);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Content;
            }
            else
            {
                return null;
            }
        }

        public IList<Order> SearchOrders()
        {
            // https://lollicupdev.com/index.php/rest/V1/orders?
            // searchCriteria[filterGroups][0][filters][0][field]=status&searchCriteria[filterGroups][0][filters][0][value]=at_warehouse

            var request = CreateRequest("/rest/V1/orders?searchCriteria[filterGroups][0][filters][0][field]=status&searchCriteria[filterGroups][0][filters][0][value]=at_warehouse", Method.GET, Token);

            //var m2search = new M2Search();

            //string json = JsonConvert.SerializeObject(m2search, Formatting.Indented);

            //request.AddParameter("application/json", json, ParameterType.RequestBody);

            var response = Client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var m2SearchOrders = JsonConvert.DeserializeObject<M2SearchOrder>(response.Content);
                return m2SearchOrders.orders;
            }
            else
            {
                return null;
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
