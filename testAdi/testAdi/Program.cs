using System;
using System.Text;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;
using System.IO;

namespace test
{
    public enum State { Active, Complete };
    class Program
    {

		public static void Main(string[] args)
        {
           
            //tasks to enter - taken from cfg file
            string[] tasksToEnter = ReadCFG_File();
            //init driver and URL
            IWebDriver driver = initWebDriver();
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
            driver.Navigate().GoToUrl("http://todomvc.com/examples/canjs/");
            //init results file
            StreamWriter resultsFile = InitResultsFile();
            //init test cases with all essential
            TestCases TC = new TestCases(driver, wait, tasksToEnter, resultsFile);
            //start test cases and using of UIs
            TC.Test_InsertItems();
            TC.Test_ToggleAllItems(State.Complete);
            TC.Test_ToggleAllItems(State.Active);
            TC.Test_ClearCompleted();
            TC.Test_DeleteAllItems(State.Active);
            TC.Test_DeleteAllItems(State.Complete);
            //print final results to log:
            resultsFile.WriteLine("\n\n" + (TC.isPassed ? "ALL TEST CASES PASSED" : "ONE OR MORE OF THE TEST CASES HAS FAILED"));
			resultsFile.Flush();
            resultsFile.Close();
            driver.Close();
        }


        public static IWebDriver initWebDriver()
        {
            ChromeOptions options = new ChromeOptions();
            ChromeDriver driver = new ChromeDriver(@"C:\SeleniumDrivers", options);
            return driver;
        }

        public static StreamWriter InitResultsFile()
        {
            string resultsFileName = "resultsFile.txt";
            //in the beginning of every run, initialize the file for clean results
            if (File.Exists(resultsFileName))
                File.Delete(resultsFileName);
            return new StreamWriter(resultsFileName);
        }

        public static string[] ReadCFG_File()
        {
            string cfgFileName = "cfg_file.txt";
            StreamWriter cfgFile;
            //in case the file doesn't exists- create one with dummy writes
            if (!File.Exists(cfgFileName))
            {
                cfgFile = new StreamWriter(cfgFileName);
                cfgFile.WriteLine("one\ntwo\nthree");
                cfgFile.Flush();
                cfgFile.Close();
            }
      
           return File.ReadAllLines(cfgFileName);
           
        }
        
    }
}
