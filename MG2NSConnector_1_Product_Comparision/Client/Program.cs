using System;
using MG2Connector;
using NetSuiteConnector;
using ConnectorIntegration;
using System.Diagnostics;

namespace Client
{
    static class Program
    {
        // Routine path
        static string routineDirPath = @"C:\Users\" + Environment.UserName + @"\Desktop\Github\routine";
        static string productCompareDirName = @"\1_product_comparison";
        static string inputDirName = @"\input";
        static string outputDirName = @"\output";
        static string toWayneDirName = @"\to_Wayne";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Initiation
            var itemComparator = new ItemComparator();
            string outputDirPath = routineDirPath + productCompareDirName + outputDirName;
            itemComparator.SetOutFilesPath(outputDirPath, toWayneDirName);

            // Clean old files
            //itemComparator.CleanFiles();

            // Get M2 products
            string M2Url = "<Magento Site URL>";
            string M2Token = "<Magento Token>";

            var m2 = new MagentoConnector(M2Url, M2Token);

            var m2products = m2.SearchAllProducts();
            var m2attributes = m2.GetAttributes(ItemComparator.magento_custom_attributes);
            itemComparator.InsertAttributeMaps(m2attributes);
            itemComparator.InsertMProducts(m2products);
            itemComparator.GenerateEffectiveMagentoList();

            // Get NetSuite items
            var ns = new SuiteTalkConnector();

            var netsuite_items = ns.executeSavedSearch("customsearch_magento_item_list");
            itemComparator.InsertNSItems(netsuite_items);
            var nsGetList = ns.GetItems(itemComparator.GetNSGetList(itemComparator.in_magento_not_ns_items));
            itemComparator.InsertNSGetList(nsGetList);

            // Generate all the output lists
            itemComparator.GenerateOutputLists();

            // Write Files
            itemComparator.WriteOutputCSVs();

            stopWatch.Stop();

            Console.WriteLine("");
            Console.WriteLine("*** Run Time: {0}s ***", stopWatch.Elapsed.ToString("hh\\:mm\\:ss"));
            Console.WriteLine("");

            Console.WriteLine("");
            Console.WriteLine("*** Product Compare Finish! ***");
            Console.WriteLine("");

            Console.ReadLine();
        }
    }
}
