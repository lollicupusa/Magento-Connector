using System;
using MG2Connector;
using NetSuiteConnector;
using ConnectorIntegration;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace Client
{
    static class Program
    {
        // Routine path /* Set Output Path */
        static string routineDirPath = @"C:\Users\" + Environment.UserName + @"\Desktop\Github\routine";
        static string taskDirName = @"\6_credit_memo";
        static string inputDirName = @"\input";
        static string outputDirName = @"\output";
        static string outputFileName = "cm_" + GetDate() + ".csv";
        static string outputDirPath = routineDirPath + taskDirName + outputDirName;
        static string outputFilePath = SetOutputFilePath(outputDirPath, outputFileName);

        static string M2Url = "<Magento Site URL>";
        static string M2Token = "<Magento Token>";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            double cmDuration = 25;
#if DEBUG
            Console.Write("Please enter credit memos duration(hrs) from now: ");
            //cmDuration = Convert.ToDouble(Console.ReadLine());
            Console.WriteLine("");
#endif
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /* Initiation */
            var orderInjector = new OrderInjector();
            var itemComparator = new ItemComparator();
            var m2 = new MagentoConnector(M2Url, M2Token);
            var ns = new SuiteTalkConnector();

            /* Get all Magento products */
            var m2products = m2.SearchAllProducts();
            var m2attributes = m2.GetAttributes(ItemComparator.magento_custom_attributes);
            itemComparator.InsertAttributeMaps(m2attributes);
            itemComparator.InsertMProducts(m2products);

            /* Get all uninjected credit memos */
            var m2CMs = m2.SearchUnsyncCreditMemos(cmDuration);
            m2.InsertSONumbers(m2CMs);

            /* Insert CMs */
            orderInjector.orderCreator.InsertMCMs(m2CMs);
            orderInjector.orderCreator.ExtractItems(itemComparator.magentoProductInfoMap);
            Console.WriteLine("");
            Console.WriteLine("*** Credit Memos Inserted ***");
            Console.WriteLine("");

            /* Get Netsuite Invoice list */
            //string invoiceSavedSearchId = "customsearch3111";
            //var invoiceSearchAdvanced = SuiteTalkConnector.CreateTranSearchAdvanced(invoiceSavedSearchId);
            var invoiceDateRange = orderInjector.orderCreator.GetCreditDateRange();
            var invoiceSearchAdvanced = SuiteTalkConnector.CreateAdHocInvSearchAdv(invoiceDateRange);

            var invoiceSearchRows = ns.TransactionSavedSearch(invoiceSearchAdvanced);
            orderInjector.orderCreator.CreateInvoiceInfoMap(invoiceSearchRows);
            orderInjector.orderCreator.InsertInvoiceInfo();
            Console.WriteLine("");
            Console.WriteLine("*** Search Invoice Done ***");
            Console.WriteLine("");

            /* Get Invoices */
            orderInjector.SetNSConnector(ns);
            orderInjector.GetInvoices(orderInjector.orderCreator.allCMInfos);
            Console.WriteLine("");
            Console.WriteLine("*** Get Invoice Done ***");
            Console.WriteLine("");

            /* Pre-process CMs */
            orderInjector.orderCreator.FixServiceItems(); // Fix item info(damaged, tax)
            Console.WriteLine("");
            Console.WriteLine("*** Credit Memos Pre-processed ***");
            Console.WriteLine("");

            /* Create Credit Memos */
            orderInjector.orderCreator.CreateCreditMemos();

            /* Inject credit memos */
            orderInjector.InjectToNS();

            /* Get Upserted CMs */
            orderInjector.GetCMs();

            /* Compare and print */
            orderInjector.CompareCMs();

            /* Write Files */
            WriteListToCSV(orderInjector.cmComparisons, outputFilePath);

            /* Update synced to Magento */
            //var getSuccessOrders = orderInjector.GetSuccessOrders();
            //m2.UpdateSyncedOrders(getSuccessOrders);

            /* Report */
            orderInjector.ReportResult();

            stopWatch.Stop();

            Console.WriteLine("");
            Console.WriteLine("*** Run Time: {0}s ***", stopWatch.Elapsed.ToString("hh\\:mm\\:ss"));
            Console.WriteLine("");

            Console.WriteLine("");
            Console.WriteLine("*** Product Compare Finish! ***");
            Console.WriteLine("");
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void WriteListToCSV<T>(List<T> orderComparisons, string outputFilePath)
        {
            if (orderComparisons.Count == 0) return;

            using (var writer = new StreamWriter(outputFilePath))
            {
                var csv = new CsvWriter(writer);
                csv.WriteHeader<T>();
                csv.NextRecord();
                csv.WriteRecords(orderComparisons);
            }
        }

        private static string SetOutputFilePath(string outputDirPath, string filename)
        {
            string date = GetDate();

            Console.WriteLine("");
            string outputFilePath_o = outputDirPath + @"\" + outputFileName;
            string outputFilePath = outputFilePath_o;

            int count = 2;
            while (System.IO.File.Exists(outputFilePath))
            {
                int index_date = outputFilePath_o.IndexOf(date);
                outputFilePath = outputFilePath_o.Substring(0, index_date + date.Length) + " - " + count++ + ".csv";
            }

            Console.WriteLine("Output File Path: {0}", outputFilePath);
            Console.WriteLine("");

            return outputFilePath;
        }

        private static string GetDate()
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, pstZone);

            return pstTime.ToString("yyyyMMddHHmm");
        }
    }
}
