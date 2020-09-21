using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.IO;

namespace rf
{
    class socReport
    {
        public string _accessionNumber = "";
        public string _fileNumber = "";
        public string _filingDate = "";
        public string _formName = "";
        public string _formDesc = "";
        public string _act = "";
        public string _filingType = "";
        public string _filingHref = "";
    }

    class socRssFeed
    {
        public string _companyName = "";
        public Dictionary<string, socReport> _companyReports = new Dictionary<string, socReport>();
    }

    class socBot
    {
        public static Http _web = new Http();
        public static socRssFeed fetchCIK(string CIK)
        {
            socRssFeed feed = new socRssFeed();
            bool feedRemaining = true;
            int feedStart = 0;
            const int feedCount = 100;
            while (feedRemaining)
            {
                request_response response = _web.GET(
                    "https://www.sec.gov/cgi-bin/browse-edgar",
                    new Dictionary<string, string>()
                    {
                    { "action", "getcompany" },
                    { "CIK", CIK },
                    { "type", "" },
                    { "dateb", "" },
                    { "owner", "exclude" },
                    { "start", feedStart.ToString() },
                    { "count", feedCount.ToString() },
                    { "output", "atom" }
                    },
                    new Dictionary<string, string>()
                );

                if (response._status == 200)
                {
                    try
                    {
                        XmlDocument reportXml = new XmlDocument();
                        reportXml.LoadXml(response._html);
                        XmlNodeList reports = reportXml.GetElementsByTagName("entry");
                        if (feed._companyName == "")
                        {
                            XmlNodeList nameTags = reportXml.GetElementsByTagName("conformed-name");
                            if (nameTags.Count != 0)
                            {
                                feed._companyName = nameTags[0].InnerText;
                            }
                        }
                        if (reports.Count != 0)
                        {
                            for (int i = 0; i < reports.Count; ++i)
                            {
                                socReport r = parseReportXml(reports[i]);
                                feed._companyReports.Add(r._accessionNumber, r);
                            }
                            feedStart += feedCount;
                        }
                        else
                        {
                            feedRemaining = false;
                        }
                    }
                    catch (Exception) { feedRemaining = false; }
                }
            }
            return feed;
        }

        private static socReport parseReportXml(XmlNode rNode)
        {
            socReport socReport = new socReport();
            try
            {
                XmlNode cNode = searchNode(rNode, "content");

                socReport._accessionNumber = getNodeText(cNode, "accession-number");
                socReport._act = getNodeText(cNode, "act");
                socReport._fileNumber = getNodeText(cNode, "file-number");
                socReport._filingDate = getNodeText(cNode, "filing-date");
                socReport._formDesc = getNodeText(cNode, "items-desc");
                socReport._formName = getNodeText(cNode, "form-name");
                socReport._filingType = getNodeText(cNode, "filing-type");
                socReport._filingHref = getNodeText(cNode, "filing-href");
            }
            catch (Exception) { }
            return socReport;
        }

        private static string getNodeText(XmlNode node, string path)
        {
            try
            {
                XmlNode txtNode = searchNode(node, path);
                return txtNode.InnerText;
            }
            catch (Exception){}
            return "";
        }

        //Fuck xmlNodes!
        private static XmlNode searchNode(XmlNode node, string targetNodeName)
        {
            for(int i = 0; i <  node.ChildNodes.Count; ++i)
            {
                if(node.ChildNodes[i].Name == targetNodeName)
                {
                    return node.ChildNodes[i];
                }
            }
            return null;
        }

        public static bool downloadReport(string path, socReport report)
        {
            string subPath = path + "/" + dirNameNeutral(report._filingType);
            if (!Directory.Exists(subPath))
            {
                Directory.CreateDirectory(subPath);
            }

            string filename = subPath + "/" +
                dirNameNeutral(report._filingType) + "-" +
                report._filingDate + "-" +
                report._accessionNumber + 
                ".txt";

            string filingHref = report._filingHref;
            if (filingHref != "")
            {
                filingHref = filingHref.Replace("-index.htm", ".txt");
                return _web.downloadFile(filingHref, filename);
            }
            return false;
        }
        
        //lazy..
        public static string dirNameNeutral(string inp)
        {
            return inp.Replace("/", " Dash ")
            .Replace("\\", " Dash ")
            .Replace(":", " semicolon ")
            .Replace("?", " questionmark ")
            .Replace("\"", " Quatationmark ")
            .Replace(">", " morethan ")
            .Replace("<", " lessthan ")
            .Replace("|", " Dash ");
        }
    }
}
