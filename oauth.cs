using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowConnector
{
    class oauth
    {
        private string _accToken = "";
        private string accToken { get; set; }
        private int expires_in { get; set; }
        private string refreshToken { get; set; }
        private string url { get { return @"https://" + SNConfig.Instance.org + @".service-now.com/oauth_token.do"; } }
        private Stopwatch refreshTime { get; set; }

        public string getAccessToken()
        {
            return !validKey() ? refresh() : accToken;
        }

        private bool validKey()
        {
            if (String.IsNullOrEmpty(accToken)) return false;
            if (String.IsNullOrEmpty(refreshToken)) return false;
            if (String.IsNullOrEmpty(refreshToken)) return false;
            var diff = expires_in - refreshTime.Elapsed.Seconds;
            //Trace.oauth.note("Token valid time left : {}", diff);
            SNService.writeLog("Diff Time: " + diff);
            if (diff < 60) return false;
            return true;
        }

        private string refresh()
        {
            if (String.IsNullOrEmpty(refreshToken)) { updateToken(); return accToken; }
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                NameValueCollection body = prepareBody();
                body.Add("grant_type", "refresh_token");
                body.Add("refresh_token", refreshToken);
                var content = wc.UploadValues(new Uri(url), "POST", body);
                readMessage(content);
            }
            return accToken;
        }

        public void updateToken()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                NameValueCollection body = prepareBody();
                body.Add("grant_type", "password");
                body.Add("username", SNConfig.Instance.username);
                body.Add("password", SNConfig.Instance.password);
                var content = wc.UploadValues(new Uri(url), "POST", body);
                readMessage(content);
            }
        }

        private NameValueCollection prepareBody()
        {
            var body = new NameValueCollection();
            body.Add("client_id", SNConfig.Instance.client_id);
            body.Add("client_secret", SNConfig.Instance.client_secret);
            return body;
        }

        private void readMessage(byte[] content)
        {
            DataContractJsonSerializer serial = new DataContractJsonSerializer(typeof(Tokens));
            Tokens result = new Tokens();
            using (var ms = new MemoryStream(content))
            {
                result = (Tokens)serial.ReadObject(ms);
            }

            //Trace.oauth.note("Access Token: {}", result.access_token);
            accToken = result.access_token;
            refreshToken = result.refresh_token;
            expires_in = result.expires_in;
            refreshTime = new Stopwatch();
            refreshTime.Start();
            SNService.writeLog("renewed oauth");
            //Trace.oauth.note("Expires: {}", expires_in);
        }

        [DataContract]
        class Tokens
        {
            [DataMember]
            public string scope { get; set; }
            [DataMember]
            public int expires_in { get; set; }
            [DataMember]
            public string refresh_token { get; set; }
            [DataMember]
            public string access_token { get; set; }
        }

        private static volatile oauth instance;
        private static object syncRoot = new Object();

        private oauth()
        {
            refreshTime = new Stopwatch();
            updateToken();
        }

        public static oauth Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new oauth();
                    }
                }

                return instance;
            }
        }
    }
}
