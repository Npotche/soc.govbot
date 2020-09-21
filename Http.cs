using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace rf
{
    class request_response
    {
        public string _html = "";
        public int _status = -1;
        public bool _requestMade = false;
    };

    class Http
    {

        public CookieContainer _cookies = new CookieContainer();

        protected HttpWebRequest createRequest(string target)
        {
            try
            {
                Uri postTarget = new Uri(target);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postTarget);
                request.CookieContainer = this._cookies;
                request.Proxy = null;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.75 Safari/537.36";
                request.Host = postTarget.Host;
                request.Timeout = 15000;
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                return request;
            }
            catch (Exception exp) { }
            return null;
        }

        protected request_response getResponseFromRequest(HttpWebRequest request)
        {
            request_response resData = new request_response();
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader ResStream = new StreamReader(response.GetResponseStream()))
                    {
                        resData._html = ResStream.ReadToEnd();
                        resData._status = (int)response.StatusCode;
                        resData._requestMade = true;
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        resData._html = sr.ReadToEnd();
                        resData._status = (int)(resp as HttpWebResponse).StatusCode;
                        resData._requestMade = true;
                    }
                }
            }
            catch (Exception exp){}
            return resData;
        }

        public request_response POST(string target, Dictionary<string, string> postData, Dictionary<string, string> postHeaders)
        {
            request_response response = new request_response();
            HttpWebRequest request = this.createRequest(target);
            if(request != null)
            {
                try
                {
                    string data =  string.Join("&", postData.Select(x => x.Key + "=" + x.Value).ToArray());
                    byte[] rDataBytes = Encoding.UTF8.GetBytes(data);
                    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    request.ContentLength = rDataBytes.Length;
                    request.Method = "POST";
                    
                    foreach (KeyValuePair<string, string> kvPair in postHeaders)
                    {
                        request.Headers[kvPair.Key] = kvPair.Value;
                    }
                    using (Stream Swriter = request.GetRequestStream())
                    {
                        Swriter.Write(rDataBytes, 0, rDataBytes.Length);
                    }



                    response = this.getResponseFromRequest(request);
                    this._cookies = request.CookieContainer;
                }
                catch (Exception exp) {
                    string x= "";
                }
            }
            return response;
        }


        public request_response GET(string target, Dictionary<string, string> getData, Dictionary<string, string> postHeaders)
        {
            request_response response = new request_response();
            try
            {
                string data = string.Join("&", getData.Select(x => x.Key + "=" + x.Value).ToArray());
                target += "?" + data;
                HttpWebRequest request = this.createRequest(target);
                request.Method = "GET";
                if (request != null)
                {
                    foreach (KeyValuePair<string, string> kvPair in postHeaders)
                    {
                        request.Headers.Add(kvPair.Key, kvPair.Value);
                    }

                    response = this.getResponseFromRequest(request);
                    this._cookies = request.CookieContainer;
                }
            }
            catch (Exception exp) { }
            return response;
        }

        public bool tryAddCookies(Dictionary<string, string> newCookies, string targetDomain)
        {
            try
            {
                Uri target = new Uri(targetDomain);
                foreach (KeyValuePair<string, string> kvPair in newCookies)
                {
                    this._cookies.Add(new Cookie(kvPair.Key, kvPair.Value) { Domain = target.Host });
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool downloadFile(string path, string filename)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(path, filename);
                }
            }
            catch (Exception) { }
            return false;
        }
    }
}
