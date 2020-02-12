﻿using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Xml;

namespace NetSuiteConnector
{
    public class SuiteTalkConnector
    {
        // All web service operations executed against the NetSuiteService
        public NetSuiteService _service;

        // Object containing preferences that get generated in the Soap header of a request
        private Preferences _prefs;

        // Object containing search specific preferences that get generated in the Soap header of a request
        private SearchPreferences _searchPreferences;

        public const Int32 WEB_SERVICES_TRY_TIMES_LIMIT = 3;

        public SuiteTalkConnector()
        {
            /*
             * Sets the application to accept all certificates coming from a secure server.
             * NOTE: This line is only required if the network or application needs to communicate over HTTPS (SSL).
             */
            ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            // All web service operations execute against the _service field
            _service = new NetSuiteService();

            // Added to make sure the application works regardless of the data center
            setDataCenterUrl("ACCT105060");

            //setPreferences();   // Set all preferences, search & regular
            setPassport();      // Set the Passport information for authentication
            //setTokenPassport(); // Comment out the setPassport() call and uncomment this to use tokens for logging in.
        }

        /// <summary>
        /// Set the Data Center Url based on where the account is stored.
        /// </summary>
        /// <param name="accountId"></param>
        private void setDataCenterUrl(String accountId)
        {
            DataCenterUrls dataCenterUrls = _service.getDataCenterUrls(accountId).dataCenterUrls;
            String webServiceUrl = dataCenterUrls.webservicesDomain;

            _service.Url = webServiceUrl + "/services/NetSuitePort_2019_2?c=" + accountId;
        }

        /// <summary>
        /// This provides an alternative way of authentication by using tokens instead of
        /// username and password combinations
        /// </summary>
        private void setTokenPassport()
        {
            /* Replace text with the information generated by the integration and token records */
            string accountId = "<NetSuite Account number>";
            string consumerKey = "<Consumer Key>";
            string consumerSecret = "<Consumer Secret>";
            string tokenId = "<Token Id>";
            string tokenSecret = "<Token Secret>";

            _service.tokenPassport = WsHelper.generateTokenPassport(accountId, consumerKey, consumerSecret, tokenId, tokenSecret);
        }


        /// <summary>
        /// Sets the authentication information needed to connect to NetSuite
        /// </summary>
        private void setPassport()
        {
            // Replace text in the next lines as noted
            _service.applicationInfo = new ApplicationInfo
            {
                applicationId = "<NetSuite Application ID>"
            };

            _service.passport = new Passport
            {
                email = "<NetSuite Email address>",
                password = "<NetSuite Password>",
                account = "<NetSuite Account number>",
                role = new RecordRef { internalId = "<NetSuite Role ID>" }
            };

            // Display the login information in the console
            Console.WriteLine("Login info...");
            Console.WriteLine("\tEmail           : {0}", _service.passport.email);
            Console.WriteLine("\tRole Internal ID: {0}", _service.passport.role.internalId);
            Console.WriteLine("\tAccount Number  : {0}", _service.passport.account);
            Console.WriteLine("\tApplication ID  : {0}\n", _service.applicationInfo.applicationId);
        }

        public static void displayError(StatusDetail[] statusDetails)
        {
            foreach (StatusDetail statusDetail in statusDetails)
            {
                Console.WriteLine("Type : {0}", statusDetail.type);
                Console.WriteLine("Code : {0}", statusDetail.code);
                Console.WriteLine("Message : {0} \n", statusDetail.message);
            }
        }

        public WriteResponse UpsertRecord(Record record)
        {
            WriteResponse response = null;

            int tryTimes = 0;

            while (tryTimes < WEB_SERVICES_TRY_TIMES_LIMIT)
            {
                try
                {
                    setTokenPassport();
                    response = _service.upsert(record);
                    break; // if working properly, break here.
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    // Get the fault type. It's the only child element of the detail element.
                    string fault = ex.Detail.FirstChild.Name;
                    string code = null;
                    string message = null;

                    // Get the list of child elements of the fault type element.
                    // It should include the code and message elements
                    System.Collections.IEnumerator ienum = ex.Detail.FirstChild.ChildNodes.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        XmlNode node = (XmlNode)ienum.Current;

                        if (node.Name == "code")
                        {
                            code = node.InnerText;
                        }
                        else if (node.Name == "message")
                        {
                            message = node.InnerText;
                        }
                    }
                    Console.WriteLine("\n***   SOAP FAULT: fault type={0} with code={1}. {2}", fault, code, message);
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine("[SOAP Fault Web Exception]: {0}", ex.Message);
                }
                catch (System.InvalidOperationException ex)
                {
                    Console.WriteLine("[SOAP Fault Invalid Operation Exception]: {0}", ex.Message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Error]: {0}", ex.Message);
                }
                finally
                {
                    tryTimes++; // ensure whether exception or not, retry time++ here

                    if (tryTimes >= 2)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return response;
        }

        public CreditMemo GetCrediMemo(string externalId)
        {
            RecordRef recordRef = new RecordRef()
            {
                externalId = externalId,
                type = RecordType.creditMemo,
                typeSpecified = true,
            };

            ReadResponse response = GetRecord(recordRef);

            if (response.record == null)
            {
                Console.WriteLine("Get Credit Memo {0} failed!!", externalId);
                return null;
            }

            Console.WriteLine("Get Credit Memo {0} Success!!", externalId);

            return (CreditMemo)response.record;
        }

        public Invoice GetInvoice(string invoice_internal_id)
        {
            RecordRef recordRef = new RecordRef()
            {
                internalId = invoice_internal_id,
                type = RecordType.invoice,
                typeSpecified = true,
            };

            ReadResponse response = GetRecord(recordRef);

            if (response.record == null)
            {
                Console.WriteLine("Get Invoice {0} failed!!", invoice_internal_id);
                return null;
            }

            Console.WriteLine("Get Invoice {0} Success!!", invoice_internal_id);

            return (Invoice)response.record;
        }

        public ReadResponse GetRecord(RecordRef recordRef)
        {
            ReadResponse response = null;

            int tryTimes = 0;

            while (tryTimes < WEB_SERVICES_TRY_TIMES_LIMIT)
            {
                try
                {
                    //setTokenPassport();
                    response = _service.get(recordRef);
                    break; // if working properly, break here.
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    // Get the fault type. It's the only child element of the detail element.
                    string fault = ex.Detail.FirstChild.Name;
                    string code = null;
                    string message = null;

                    // Get the list of child elements of the fault type element.
                    // It should include the code and message elements
                    System.Collections.IEnumerator ienum = ex.Detail.FirstChild.ChildNodes.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        XmlNode node = (XmlNode)ienum.Current;

                        if (node.Name == "code")
                        {
                            code = node.InnerText;
                        }
                        else if (node.Name == "message")
                        {
                            message = node.InnerText;
                        }
                    }
                    Console.WriteLine("\n***   SOAP FAULT: fault type={0} with code={1}. {2}", fault, code, message);
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine("[SOAP Fault Web Exception]: {0}", ex.Message);
                }
                catch (System.InvalidOperationException ex)
                {
                    Console.WriteLine("[SOAP Fault Invalid Operation Exception]: {0}", ex.Message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Error]: {0}", ex.Message);
                }
                finally
                {
                    tryTimes++; // ensure whether exception or not, retry time++ here

                    if (tryTimes >= 2)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return response;
        }

        public static TransactionSearchAdvanced CreateAdHocInvSearchAdv(Tuple<DateTime, DateTime> invoiceDateRange)
        {
            var tranSearchBasic = new TransactionSearchBasic
            {
                type = new SearchEnumMultiSelectField
                {
                    searchValue = new string[] { "_invoice" },
                    @operator = SearchEnumMultiSelectFieldOperator.anyOf,
                    operatorSpecified = true
                },
                mainLine = new SearchBooleanField
                {
                    searchValue = true,
                    searchValueSpecified = true
                },
                status = new SearchEnumMultiSelectField
                {
                    searchValue = new string[] { "_invoiceVoided", "_invoiceRejected", "_invoicePendingApproval" },
                    @operator = SearchEnumMultiSelectFieldOperator.noneOf,
                    operatorSpecified = true
                },
                salesRep = new SearchMultiSelectField
                {
                    searchValue = new RecordRef[]
                    {
                        new RecordRef
                        {
                            type = RecordType.salesRole,
                            typeSpecified = true,
                            internalId = "8"
                        },
                        new RecordRef
                        {
                            type = RecordType.salesRole,
                            typeSpecified = true,
                            internalId = "1670292"
                        }
                    },
                    @operator = SearchMultiSelectFieldOperator.anyOf,
                    operatorSpecified = true
                },
                customerSubOf = new SearchMultiSelectField
                {
                    searchValue = new RecordRef[]
                    {
                        new RecordRef
                        {
                            type = RecordType.customer,
                            typeSpecified = true,
                            internalId = "3183521"
                        },
                        new RecordRef
                        {
                            type = RecordType.customer,
                            typeSpecified = true,
                            internalId = "3114319"
                        }
                    },
                    @operator = SearchMultiSelectFieldOperator.anyOf,
                    operatorSpecified = true
                },
                tranDate = new SearchDateField
                {
                    searchValue = invoiceDateRange.Item1,
                    searchValueSpecified = true,
                    @operator = SearchDateFieldOperator.onOrAfter,
                    operatorSpecified = true
                },
            };

            var tranSearchRow = new TransactionSearchRowBasic()
            {
                internalId = new SearchColumnSelectField[]
                {
                    new SearchColumnSelectField()
                },
                tranId = new SearchColumnStringField[]
                {
                    new SearchColumnStringField()
                },
                otherRefNum = new SearchColumnTextNumberField[]
                {
                    new SearchColumnTextNumberField()
                },
                location = new SearchColumnSelectField[]
                {
                    new SearchColumnSelectField()
                },
                status = new SearchColumnEnumSelectField[]
                {
                    new SearchColumnEnumSelectField()
                },
                createdFrom = new SearchColumnSelectField[]
                {
                    new SearchColumnSelectField()
                }
            };

            var createdFromSearchRowJoin = new TransactionSearchRowBasic()
            {
                tranId = new SearchColumnStringField[]
                {
                    new SearchColumnStringField()
                },
                status = new SearchColumnEnumSelectField[]
                {
                    new SearchColumnEnumSelectField()
                }
            };

            return new TransactionSearchAdvanced
            {
                criteria = new TransactionSearch()
                {
                    basic = tranSearchBasic
                },
                columns = new TransactionSearchRow()
                {
                    basic = tranSearchRow,
                    createdFromJoin = createdFromSearchRowJoin
                }
            };
        }

        public static TransactionSearchAdvanced CreateTranSearchAdvanced(string transSavedSearchId)
        {
            return new TransactionSearchAdvanced
            {
                savedSearchScriptId = transSavedSearchId
            };
        }

        public List<TransactionSearchRow> TransactionSavedSearch(SearchRecord searchRecord)
        {
            Console.WriteLine("Start Transaction Saved Search");

            List<TransactionSearchRow> transactionSearchRows = new List<TransactionSearchRow>();

            SearchResult searchResult = getSearch(searchRecord);

            if (searchResult.status.isSuccess)
            {
                Console.WriteLine("");
                Console.WriteLine("Get Saved Transaction Search Success");
                Console.WriteLine("Total Records: {0}", searchResult.totalRecords);
                Console.WriteLine("Total Page(s): {0}", searchResult.totalPages);
                Console.WriteLine("");

                int totalPages = searchResult.totalPages;
                int pageIndex = searchResult.pageIndex;
                string searchId = searchResult.searchId;

                for (int i = pageIndex; i <= totalPages; i++)
                {
                    searchResult = getSearchMoreWithId(searchId, i);
                    Console.WriteLine("Reponse Page {0}", i);

                    if (searchResult.status.isSuccess)
                    {
                        foreach (TransactionSearchRow transactionSearchRow in searchResult.searchRowList)
                        {
                            transactionSearchRows.Add(transactionSearchRow);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Search More Item Failed");
                        displayError(searchResult.status.statusDetail);
                    }
                }//foreach page
            }
            else
            {
                Console.WriteLine("Get Saved Search Failed");
                displayError(searchResult.status.statusDetail);
            }
            Console.WriteLine("");

            return transactionSearchRows;
        }

        public List<ItemSearchRow> ExecuteSavedSearch(string savedSearchId)
        {
            List<ItemSearchRow> itemSearchRows = new List<ItemSearchRow>();

            ItemSearchAdvanced itemSearchAdvanced = new ItemSearchAdvanced
            {
                savedSearchScriptId = savedSearchId
            };
            
            SearchResult searchResult = getSearch(itemSearchAdvanced);

            if (searchResult.status.isSuccess)
            {
                Console.WriteLine("");
                Console.WriteLine("Get Saved Search Success");
                Console.WriteLine("Total Records: {0}", searchResult.totalRecords);
                Console.WriteLine("Total Page(s): {0}", searchResult.totalPages);
                Console.WriteLine("");

                int totalPages = searchResult.totalPages;
                int pageIndex = searchResult.pageIndex;
                string searchId = searchResult.searchId;

                for (int i = pageIndex; i <= totalPages; i++)
                {
                    searchResult = getSearchMoreWithId(searchId, i);
                    Console.WriteLine("Reponse Page {0}", i);

                    if (searchResult.status.isSuccess)
                    {
                        foreach (ItemSearchRow itemSearchRow in searchResult.searchRowList)
                        {
                            itemSearchRows.Add(itemSearchRow);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Search More Item Failed");
                        displayError(searchResult.status.statusDetail);
                    }
                }//foreach page
            }
            else
            {
                Console.WriteLine("Get Saved Search Failed");
                displayError(searchResult.status.statusDetail);
            }

            Console.WriteLine("");

            return itemSearchRows;
        }

        private SearchResult getSearch(SearchRecord searchAdvanced)
        {
            SearchResult searchResult = null;

            int tryTimes = 0;

            while (tryTimes < WEB_SERVICES_TRY_TIMES_LIMIT)
            {
                try
                {
                    //setTokenPassport();
                    searchResult = _service.search(searchAdvanced);
                    break; // if working properly, break here.
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    // Get the fault type. It's the only child element of the detail element.
                    string fault = ex.Detail.FirstChild.Name;
                    string code = null;
                    string message = null;

                    // Get the list of child elements of the fault type element.
                    // It should include the code and message elements
                    System.Collections.IEnumerator ienum = ex.Detail.FirstChild.ChildNodes.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        XmlNode node = (XmlNode)ienum.Current;

                        if (node.Name == "code")
                        {
                            code = node.InnerText;
                        }
                        else if (node.Name == "message")
                        {
                            message = node.InnerText;
                        }
                    }
                    Console.WriteLine("\n***   SOAP FAULT: fault type={0} with code={1}. {2}", fault, code, message);
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine("[SOAP Fault Web Exception]: {0}", ex.Message);
                }
                catch (System.InvalidOperationException ex)
                {
                    Console.WriteLine("[SOAP Fault Invalid Operation Exception]: {0}", ex.Message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Error]: {0}", ex.Message);
                }
                finally
                {
                    tryTimes++; // ensure whether exception or not, retry time++ here

                    if (tryTimes >= 2)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return searchResult;
        }

        private SearchResult getSearchMoreWithId(string searchId, int i)
        {
            SearchResult searchResult = null;

            int tryTimes = 0;

            while (tryTimes < WEB_SERVICES_TRY_TIMES_LIMIT)
            {
                try
                {
                    //setTokenPassport();
                    searchResult = _service.searchMoreWithId(searchId, i);
                    break; // if working properly, break here.
                }
                catch (System.Web.Services.Protocols.SoapException ex)
                {
                    // Get the fault type. It's the only child element of the detail element.
                    string fault = ex.Detail.FirstChild.Name;
                    string code = null;
                    string message = null;

                    // Get the list of child elements of the fault type element.
                    // It should include the code and message elements
                    System.Collections.IEnumerator ienum = ex.Detail.FirstChild.ChildNodes.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        XmlNode node = (XmlNode)ienum.Current;

                        if (node.Name == "code")
                        {
                            code = node.InnerText;
                        }
                        else if (node.Name == "message")
                        {
                            message = node.InnerText;
                        }
                    }
                    Console.WriteLine("\n***   SOAP FAULT: fault type={0} with code={1}. {2}", fault, code, message);
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine("[SOAP Fault Web Exception]: {0}", ex.Message);
                }
                catch (System.InvalidOperationException ex)
                {
                    Console.WriteLine("[SOAP Fault Invalid Operation Exception]: {0}", ex.Message);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[Error]: {0}", ex.Message);
                }
                finally
                {
                    tryTimes++; // ensure whether exception or not, retry time++ here

                    if (tryTimes >= 2)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return searchResult;
        }
    }
}
