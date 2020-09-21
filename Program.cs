using System;
using System.IO;
using System.Collections.Generic;
namespace rf
{

    class Program
    {
        static string version = "0.10";
        static bool consoleOutOn = true;
        static void Main(string[] args)
        {
            consoleOutLine("CeRi's soc.gov Report Fetcher " + version);
            if(args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                consoleOutLine("------------------------------------------------------");
                consoleOutLine("Usage");
                consoleOutLine("Please input company CIK id as command line argument");
                consoleOutLine("------------------------------------------------------");
                
            }
            else
            {
                string cik = args[0];
                Console.ForegroundColor = ConsoleColor.Green;
                consoleOutLine("------------------------------------------------------");
                consoleOutLine("Fetching data for CIK " + cik);
                socRssFeed feed = socBot.fetchCIK(cik);
                consoleOutLine("Company found: " + feed._companyName);
                consoleOutLine("Entries found: " + feed._companyReports.Count);
                consoleOutLine("Beginning to download files");
                string downloadPath = socBot.dirNameNeutral(feed._companyName) + "/" + Guid.NewGuid();
                Directory.CreateDirectory(downloadPath);
                
                foreach (KeyValuePair<string, socReport> kvPair in feed._companyReports)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    consoleOut("\nDownloading report " + kvPair.Key + ": ... ");
                    bool success = socBot.downloadReport(downloadPath, kvPair.Value);
                    if (success)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        consoleOut("Failed");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        consoleOut("Success");
                    }
                }
                consoleOut("\n");
                consoleOutLine("------------------------------------------------------");

            }

            Console.ReadKey();
        }

        static void consoleOutLine(string msg)
        {
            if (consoleOutOn)
            {
                Console.WriteLine(msg);
            }
        }

        static void consoleOut(string msg)
        {
            if (consoleOutOn)
            {
                Console.Write(msg);
            }
        }
    }
}
