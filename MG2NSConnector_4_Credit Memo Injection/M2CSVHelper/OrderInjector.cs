using MG2Connector;
using NetSuiteConnector;
using NetSuiteConnector.com.netsuite.webservices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectorIntegration
{
    public class OrderInjector
    {
        public OrderCreator orderCreator;
        public SuiteTalkConnector nsConnector;
        public List<CreditMemoInfo> failedUpsertCMs;
        public List<CreditMemoInfo> failedGetCMs;
        public List<CreditMemoInfo> differentAmountCMs;
        public List<CMComparison> cmComparisons;

        public OrderInjector()
        {
            orderCreator = new OrderCreator();
            failedUpsertCMs = new List<CreditMemoInfo>();
            failedGetCMs = new List<CreditMemoInfo>();
            cmComparisons = new List<CMComparison>();
            differentAmountCMs = new List<CreditMemoInfo>();
        }

        public void SetNSConnector(SuiteTalkConnector ns)
        {
            nsConnector = ns;
        }

        public void GetInvoices(List<CreditMemoInfo> allCMInfos)
        {
            foreach (CreditMemoInfo creditMemoInfo in allCMInfos)
            {
                if (creditMemoInfo.orderType == OrderType.Invalid) continue;

                creditMemoInfo.created_from_invoice = nsConnector.GetInvoice(creditMemoInfo.invoiceInfo.invoice_internal_id);
            }
        }

        public void InjectToNS()
        {
            foreach (CreditMemoInfo creditMemoInfo in orderCreator.allCMInfos)
            {
                if (!creditMemoInfo.IsToInject()) continue;

                Console.WriteLine("---------------");
                Console.WriteLine("Upsert Credit Memo #{0}", creditMemoInfo.po);

                WriteResponse cmResponse = nsConnector.UpsertRecord(creditMemoInfo.creditMemo);

                if (!IsWriteSucceed(cmResponse))
                {
                    creditMemoInfo.invalid_reason = "Upsert Failed: " + GetErrorMessage(cmResponse);
                    failedUpsertCMs.Add(creditMemoInfo);
                }
            }

            Console.WriteLine("\n");
        }

        private string GetErrorMessage(WriteResponse cmResponse)
        {
            return cmResponse.status.statusDetail[0].message;
        }

        private bool IsWriteSucceed(WriteResponse writeResponse)
        {
            if (writeResponse == null)
            {
                Console.WriteLine("Upsert Transaction failed!!");
                return false;
            }

            if (writeResponse.status.isSuccess)
            {
                Console.WriteLine("Upsert Transaction Succeed, External ID: {0}", ((RecordRef)writeResponse.baseRef).externalId);
                return true;
            }
            else
            {
                Console.WriteLine("Upsert Transaction failed!!");
                SuiteTalkConnector.displayError(writeResponse.status.statusDetail);
                return false;
            }
        }

        public void GetCMs()
        {
            foreach (CreditMemoInfo creditMemoInfo in orderCreator.allCMInfos)
            {
                if (!creditMemoInfo.IsToInject()) continue;

                creditMemoInfo.creditMemo_get = nsConnector.GetCrediMemo(creditMemoInfo.creditMemo.externalId);

                if (creditMemoInfo.creditMemo_get == null)
                {
                    failedGetCMs.Add(creditMemoInfo);
                }
            }
        }

        public void CompareCMs()
        {
            foreach (CreditMemoInfo creditMemoInfo in orderCreator.allCMInfos)
            {

                CMComparison cmComparison = new CMComparison()
                {
                    Magento_Id = creditMemoInfo.po,
                    Magento_CM_Id = creditMemoInfo.increment_id,
                    Comment = creditMemoInfo.invalid_reason
                };

                cmComparisons.Add(cmComparison);

                if (creditMemoInfo.creditMemo_get == null || creditMemoInfo.creditMemo_get.otherRefNum == null) continue;

                cmComparison.NS_CM_Id = creditMemoInfo.creditMemo_get.tranId;
                cmComparison.NS_CM_Internal_Id = creditMemoInfo.creditMemo_get.internalId;
                cmComparison.NS_CM_External_ID = creditMemoInfo.creditMemo_get.externalId;
                cmComparison.Location = creditMemoInfo.creditMemo_get.location.name;
                cmComparison.Date = creditMemoInfo.creditMemo_get.tranDate.ToString("yyyyMMdd");
                cmComparison.Entity = creditMemoInfo.creditMemo_get.entity.name;
                cmComparison.M_Refunded = creditMemoInfo.magentoCreditMemo.grand_total;
                cmComparison.NS_Refunded = creditMemoInfo.creditMemo_get.total;
                double diff_amount = cmComparison.M_Refunded - creditMemoInfo.creditMemo_get.total;
                cmComparison.Discrepancy = diff_amount.ToString("0.00");

                if (diff_amount >= 0.01)
                {
                    differentAmountCMs.Add(creditMemoInfo);
                }
            }
        }

        public void ReportResult()
        {
            var invalidOrders = orderCreator.allCMInfos.Where(order => order.orderType == OrderType.Invalid).ToList();

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} credit memo(s) are invalid ***", invalidOrders.Count);
            foreach (CreditMemoInfo invalidCM in invalidOrders)
            {
                var addressee = invalidCM.magentoCreditMemo.order.billing_address.firstname + " " + invalidCM.magentoCreditMemo.order.billing_address.lastname;
                Console.WriteLine("#{0,-20} by {1,-20} : {2}", invalidCM.po, addressee, invalidCM.invalid_reason);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} credit memo(s) Created! ***", orderCreator.allCMInfos.Count - invalidOrders.Count);

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} credit memo(s) Upsert Failed! ***", failedUpsertCMs.Count);
            foreach (CreditMemoInfo failedUpsertCM in failedUpsertCMs)
            {
                Console.WriteLine(failedUpsertCM.po);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} credit memo(s) Get Failed! ***", failedGetCMs.Count);
            foreach (CreditMemoInfo failedGetCM in failedGetCMs)
            {
                Console.WriteLine(failedGetCM.po);
            }

            Console.WriteLine("");
            Console.WriteLine("*** {0,3} credit memo(s) have different amount ***", differentAmountCMs.Count);
            foreach (CreditMemoInfo diffAmountCM in differentAmountCMs)
            {
                Console.WriteLine(diffAmountCM.po);
            }
            Console.WriteLine("");
        }

        public class OrderComparison
        {
            public string Entity { get; set; }
            public string Warehouse { get; set; }
            public string Default_Warehouse { get; set; }
            public string Magento_id { get; set; }
            public string NS_id { get; set; }
            public double M_total { get; set; }
            public double NS_Total { get; set; }
            public string Discrepancy { get; set; }
            public string Comment { get; set; }
        }
    }
}
