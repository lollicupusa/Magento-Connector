﻿using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;


namespace NetSuiteConnector
{
    public static class WsHelper
    {
        private static string _nonce;
        private static long _timestamp;

        /// <summary>
        /// This generates the TokenPassport object used by the integration client for authenticating
        /// an account without using username and password combination
        /// 
        /// </summary>
        /// <param name="account">ID of the NetSuite account you intend to connect to.</param>
        /// <param name="consumerKey">Generated by creating an integration record.</param>
        /// <param name="consumerSecret">Generated by creating an integration record.</param>
        /// <param name="tokenId">Generated when a token is created.</param>
        /// <param name="tokenSecret">Generated when a token is created.</param>
        /// <returns>Generated TokenPassport object</returns>
        public static TokenPassport generateTokenPassport(string account, string consumerKey, string consumerSecret, string tokenId, string tokenSecret)
        {
            generateNonce();
            generateTimestamp();

            TokenPassport tokenPassport = new TokenPassport
            {
                account = account,
                consumerKey = consumerKey,
                token = tokenId,
                nonce = _nonce,
                timestamp = _timestamp,
                signature = getPassportSignature(account, consumerKey, consumerSecret, tokenId, tokenSecret)
            };

            return tokenPassport;
        }


        /// <summary>
        /// Generate the passport signature needed for creating the TokenPassport Object
        /// 
        /// </summary>
        /// <param name="account">ID of the NetSuite accoutn you intend to connect to</param>
        /// <param name="consumerKey">Generated by creating an integration record</param>
        /// <param name="consumerSecret">Generated by creating an integration record</param>
        /// <param name="tokenId">Generated when a token is created.</param>
        /// <param name="tokenSecret">Generated when a token is created.</param>
        /// <returns>Generated TokenPassport Signature</returns>
        private static TokenPassportSignature getPassportSignature(string account, string consumerKey, string consumerSecret, string tokenId, string tokenSecret)
        {
            string signature;
            string algorithm = "HMAC-SHA1";

            string baseString = string.Format("{0}&{1}&{2}&{3}&{4}", account, consumerKey, tokenId, _nonce, _timestamp);
            string keyString = string.Format("{0}&{1}", consumerSecret, tokenSecret);

            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(keyString);
            byte[] messageBytes = encoding.GetBytes(baseString);

            using (var myhmacsha1 = new HMACSHA1(keyByte))
            {
                byte[] hashmessage = myhmacsha1.ComputeHash(messageBytes);
                signature = Convert.ToBase64String(hashmessage);
            }

            TokenPassportSignature passportSignature = new TokenPassportSignature
            {
                algorithm = algorithm,
                Value = signature
            };

            return passportSignature;
        }


        /// <summary>
        /// Generates the timestamp. This value is needed for creating tokens.
        /// 
        /// </summary>
        private static void generateTimestamp()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string timestampString = unixTimestamp.ToString();

            _timestamp = Convert.ToInt64(timestampString);
        }


        /// <summary>
        /// Generates a single use value for encryption. This value is needed for creating tokens.
        /// </summary>
        private static void generateNonce()
        {
            byte[] data = new byte[20];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(data);
            int value = Math.Abs(BitConverter.ToInt32(data, 0));

            _nonce = value.ToString();
        }


        /// <summary>
        /// Generate the header string necessary for authenticating RESTlet calls.
        /// 
        /// </summary>
        /// <param name="passport">The Passport object here is used to transport the necessary information to the method.</param>
        /// <returns>The NLAuth string that will be set in the header of the RESTlet call.</returns>
        public static string generateNLAuthHeader(Passport passport)
        {
            string header = String.Format("NLAuth nlauth_account={0}, nlauth_email={1}, nlauth_signature={2}, nlauth_role={3}",
                passport.account, passport.email, passport.password, passport.role.internalId);

            return header;
        }


        /// <summary>
        /// This is a convenience method that calls a RESTlet service via GET.
        /// 
        /// </summary>
        /// <param name="restletUrl">External URL of the RESTlet service that will be called.</param>
        /// <param name="header">NLAuth Header String. This will be used to authenticate the method call.</param>
        /// <param name="parameters">List of key/value pairs that will be passed to the RESTlet as parameters.</param>
        /// <returns>String response from the RESTlet</returns>
        public static string getByRestlet(string restletUrl, string header, List<KeyValuePair<string, string>> parameters)
        {
            // Process parameters for RESTlet
            var urlParams = "";
            foreach (var element in parameters)
            {
                urlParams = urlParams + String.Format("&{0}={1}", element.Key, element.Value);
            }
            restletUrl = restletUrl + urlParams;

            HttpWebRequest webRequest = createWebRequest(restletUrl, "GET", header);

            return callRestlet(webRequest);
        }


        /// <summary>
        /// This is a convenience method that calls a RESTlet service via POST.
        /// 
        /// </summary>
        /// <param name="restletUrl">External URL of the RESTlet service that will be called.</param>
        /// <param name="header">NLAuth Header String. This will be used to authenticate the method call.</param>
        /// <param name="record">Record that you want to add to NetSuite.</param>
        /// <returns>String response from the RESTlet</returns>
        public static string postByRestlet(string restletUrl, string header, Record record)
        {
            HttpWebRequest webRequest = createWebRequest(restletUrl, "POST", header);

            // Convert Record object to JSON
            string json = new JavaScriptSerializer().Serialize(record);

            // Add the record type to the JSON string. This will be used by the RESTlet
            string updatedJSON = json.Substring(0, json.Length - 1) + String.Format(", \"recType\" : \"{0}\"}}", record.GetType().Name);

            // Include the JSON string to the POST request
            var postData = Encoding.ASCII.GetBytes(updatedJSON);
            using (var requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(postData, 0, postData.Length);
            }

            return callRestlet(webRequest);
        }


        /// <summary>
        /// Creates the HttpWebRequest object that will be used for connecting to the RESTlet.
        /// 
        /// </summary>
        /// <param name="restletUrl">External URL of the RESTlet service that will be called</param>
        /// <param name="operation">"GET" or "POST". This determines the kind of operation used for the operation</param>
        /// <param name="header">NLAuth header that is required for authentication</param>
        /// <returns>Created HttpWebRequest. This will be used when the RESTlet is called</returns>
        private static HttpWebRequest createWebRequest(string restletUrl, string operation, string header)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(restletUrl);
            httpWebRequest.Method = operation;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("Authorization", header);
            return httpWebRequest;
        }


        /// <summary>
        /// This is the actual method that calls the RESTlet.
        /// 
        /// </summary>
        /// <param name="webRequest">HttpWebRequest object that contains the information about the request</param>
        /// <returns>String response from the RESTlet</returns>
        private static string callRestlet(HttpWebRequest webRequest)
        {
            // Execute Web Service
            WebResponse webResponse = webRequest.GetResponse();
            Stream stream = webResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);
            string httpBody = streamReader.ReadToEnd();

            // Close resources
            streamReader.Close();
            stream.Close();

            return httpBody;
        }


        /// <summary>
        /// Generates the connection string which configures the connection to the ODBC server
        /// 
        /// </summary>
        /// <param name="passport">The Passport object here is used to transport the necessary information to the method.</param>
        /// <returns>Generated connection string</returns>
        public static string generateAdoConnectionString(Passport passport)
        {
            string connectionString = "";

            // Connection settings
            string host = "odbcserver.na2.netsuite.com";
            string port = "1708";
            string dataSource = "NetSuite.com";

            connectionString = String.Format("Host={0};Port={1};ServerDataSource={2};UserID={3};Password={4};CustomProperties='AccountID={5};RoleID={6}';EncryptionMethod=SSL;",
                host, port, dataSource, passport.email, passport.password, passport.account, passport.role.internalId);

            return connectionString;
        }
    }
}
