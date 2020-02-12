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
        static string taskDirName = @"\2_order_injection";
        static string inputDirName = @"\input";
        static string outputDirName = @"\output";
        static string outputFileName = "orders_" + GetDateTime().ToString("yyyyMMddHHmm") + ".csv";
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
            double orderDuration = 1.1;
#if DEBUG
            Console.Write("Please enter orders duration(hrs) from now: ");
            orderDuration = Convert.ToDouble(Console.ReadLine());
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

            /* Get all uninjected orders */
            var m2Orders = m2.SearchUnsyncOrders(orderDuration);

            /* Get NetSuite Inventory Info */
            var netsuite_items = ns.ExecuteSavedSearch("customsearch_magento_inv_qty");
            itemComparator.InsertNSItems(netsuite_items);
            Console.WriteLine("");
            Console.WriteLine("*** Inventory Inserted ***");
            Console.WriteLine("");

            /* Pre-process Orders */
            orderInjector.orderCreator.InsertMOrders(m2Orders);     
            orderInjector.orderCreator.PickToInjectOrders();
            orderInjector.orderCreator.FixTotal();
            orderInjector.orderCreator.FixItems(itemComparator.magentoProductInfoMap);
            orderInjector.orderCreator.DecideWarehouse(itemComparator.netsuiteItemInfoMap);
            Console.WriteLine("");
            Console.WriteLine("*** Orders Pre-processed ***");
            Console.WriteLine("");

            /* Create Sales Orders */
            orderInjector.orderCreator.CreateSalesOrders();
            //orderInjector.orderCreator.CreateApproveSalesOrders();
            orderInjector.orderCreator.CreateInvoices();

            /* Inject orders */
            orderInjector.SetNSConnector(ns);
            orderInjector.InjectToNS();

            /* Get Upserted SO */
            orderInjector.GetSOs();

            /* Compare and print */
            orderInjector.CompareSOs();

            /* Write Files */
            WriteListToCSV(orderInjector.orderComparisons, outputFilePath);

            /* Update synced to Magento */
            var getSuccessOrders = orderInjector.GetSuccessOrders();
            //m2.UpdateSyncedOrders(getSuccessOrders);

            /* Report */
            orderInjector.ReportResult();

            stopWatch.Stop();

            Console.WriteLine("");
            Console.WriteLine("*** Run Time: {0}s ***", stopWatch.Elapsed.ToString("hh\\:mm\\:ss"));
            Console.WriteLine("");

            Console.WriteLine("");
            Console.WriteLine("*** Orders Injection Finish! {0}***", GetDateTime());
            Console.WriteLine("");
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void WriteListToCSV<T>(List<T> lists, string outputFilePath)
        {
            if (lists.Count == 0) return;

            using (var writer = new StreamWriter(outputFilePath))
            {
                var csv = new CsvWriter(writer);
                csv.WriteHeader<T>();
                csv.NextRecord();
                csv.WriteRecords(lists);
            }
        }

        private static string SetOutputFilePath(string outputDirPath, string filename)
        {
            Console.WriteLine("");
            string outputFilePath_o = outputDirPath + @"\" + filename;
            string outputFilePath = outputFilePath_o;

            Console.WriteLine("Output File Path: {0}", outputFilePath);
            Console.WriteLine("");

            return outputFilePath;
        }

        private static DateTime GetDateTime()
        {
            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, pstZone);

            return pstTime;
        }
    }
}
