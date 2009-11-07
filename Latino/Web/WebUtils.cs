/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          WebUtils.cs
 *  Version:       1.0
 *  Desc:		   Fundamental Web-related routines
 *  Author:        Miha Grcar
 *  Created on:    Nov-2006
 *  Last modified: Jun-2008
 *  Revision:      Jun-2008
 *
 ***************************************************************************/

using System;
using System.Net;
using System.IO;

namespace Latino.Web
{
    /* .-----------------------------------------------------------------------
       |
       |  Static class WebUtils
       |
       '-----------------------------------------------------------------------
    */
    public static class WebUtils
    {
        private static IWebProxy m_web_proxy
            = WebRequest.DefaultWebProxy;

        public static void UseDefaultWebProxy()
        {
            m_web_proxy = WebRequest.DefaultWebProxy;
        }

        public static void SetWebProxy(string url)
        {
            if (url == null) { m_web_proxy = null; }
            else { m_web_proxy = new WebProxy(url); } // throws UriFormatException
        }

        public static string GetWebProxyUrl(string resource_url)
        {
            if (m_web_proxy == null) { return null; }
            else { return m_web_proxy.GetProxy(new Uri(resource_url)).ToString(); } // throws UriFormatException
        }

        public static string GetHttpProxyUrl()
        {
            string rnd_url = string.Format("http://{0}/", Guid.NewGuid().ToString("N"));
            string proxy_url = GetWebProxyUrl(rnd_url);
            if (proxy_url == rnd_url) { return null; }
            else { return proxy_url; }
        }

        public static string GetHttpsProxyUrl()
        {
            string rnd_url = string.Format("https://{0}/", Guid.NewGuid().ToString("N"));
            string proxy_url = GetWebProxyUrl(rnd_url);
            if (proxy_url == rnd_url) { return null; }
            else { return proxy_url; }
        }

        public static string GetWebPage(string url)
        {
            CookieContainer dummy = null;
            return GetWebPage(url, /*ref_url=*/null, ref dummy); // throws ArgumentNullException, ArgumentValueException, UriFormatException, WebException
        }

        public static string GetWebPage(string url, string ref_url)
        {
            CookieContainer dummy = null;
            return GetWebPage(url, ref_url, ref dummy); // throws ArgumentNullException, ArgumentValueException, UriFormatException, WebException
        }

        public static string GetWebPage(string url, string ref_url, ref CookieContainer cookies) 
        {
            Utils.ThrowException(url == null ? new ArgumentNullException("url") : null);
            Utils.ThrowException((!url.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase)) ? new ArgumentValueException("url") : null);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); // throws UriFormatException
            request.Proxy = m_web_proxy;
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US; rv:1.8.0.6) Gecko/20060728 Firefox/1.5.0.6";
            request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,*/*;q=0.5";
            request.Headers.Add("Accept-Language", "en-us,en;q=0.5");
            request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            if (cookies == null) { cookies = new CookieContainer(); }
            request.CookieContainer = cookies; 
            if (ref_url != null) { request.Referer = ref_url; }
            StreamReader response_reader;
            string page_html = (response_reader = new StreamReader(((HttpWebResponse)request.GetResponse()).GetResponseStream())).ReadToEnd(); // throws WebException
            response_reader.Close();
            return page_html;
        }
    }
}