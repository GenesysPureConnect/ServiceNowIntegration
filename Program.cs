using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;

namespace ServiceNowConnector
{
    #region ServiceHost
    class SNServiceHost : ServiceBase
    {

        public ServiceHost serviceHost = null;
        public SNServiceHost()
        {
            // Name the Windows Service
            ServiceName = "ServiceNowConnector";
        }

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            try
            {
                //I3Trace.initialize_default_sinks();
                if (serviceHost != null)
                {
                    serviceHost.Close();
                }

                serviceHost = new ServiceHost(typeof(SNService));
                serviceHost.Open();
                SNService.writeLog("Started Service @ " + DateTime.Now.ToString());
                //Trace.main.note("Starting SN Connector");

            }
            catch (Exception ex)
            {
                //Trace.main.note(ex.Message);
                Console.WriteLine(ex.Message);
            }

        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
                SNService.writeLog("Service Stopped @ " + DateTime.Now.ToString());
                //Trace.main.note("Stopped SN Service");
            }
        }
    }
    #endregion

    #region Service Contracts
    class SNService : IServiceNowConnector
    {
        private string startURL { get { return @"https://" + SNConfig.Instance.org + ".service-now.com/api/now/table/"; } }

        static void Main(string[] args)
        {
            ServiceBase.Run(new SNServiceHost());
        }

        private Object callServiceNow(string fullUrl, Type t)
        {
            try
            {
                var client = createClient();
                var content = client.DownloadData(new Uri(fullUrl));
                DataContractJsonSerializer serial = new DataContractJsonSerializer(t);
                var result = new Object();

                using (var ms = new MemoryStream(content))
                {
                    result = serial.ReadObject(ms);
                }

                writeLog("Successful SN call of: " + t.Name);
                return result;
            }
            catch (Exception e)
            {
                writeLog(DateTime.Now + "   " + e.Message);
                if (e.Message.Contains("(401) Unauth"))
                {
                    oauth.Instance.updateToken();
                    var client = createClient();
                    var content = client.DownloadData(new Uri(fullUrl));
                    DataContractJsonSerializer serial = new DataContractJsonSerializer(t);
                    var result = new Object();

                    using (var ms = new MemoryStream(content))
                    {
                        result = serial.ReadObject(ms);
                    }

                    writeLog("Successful SN call of: " + t.Name);

                    return result;
                }
                else
                {
                    writeLog("Failed SN call of: " + t.Name);
                    throw new Exception();
                }
            }
        }

        private Object uploadServiceNow(string fullUrl, Type t, string body)
        {
            try
            {
                var client = createClient();
                var content = client.UploadData(new Uri(fullUrl), new ASCIIEncoding().GetBytes(body));
                DataContractJsonSerializer serial = new DataContractJsonSerializer(t);
                var result = new Object();

                using (var ms = new MemoryStream(content))
                {
                    result = serial.ReadObject(ms);
                }

                writeLog("Successful SN call of: " + t.Name);

                return result;
            }
            catch (Exception e)
            {
                writeLog(DateTime.Now + "   " + e.Message);
                if (e.Message.Contains("(401) Unauth"))
                {
                    oauth.Instance.updateToken();
                    var client = createClient();
                    var content = client.UploadData(new Uri(fullUrl), new ASCIIEncoding().GetBytes(body));
                    DataContractJsonSerializer serial = new DataContractJsonSerializer(t);
                    var result = new Object();

                    using (var ms = new MemoryStream(content))
                    {
                        result = serial.ReadObject(ms);
                    }

                    writeLog("Successful SN call of: " + t.Name);

                    return result;
                }
                else
                {
                    writeLog("Failed SN call of: " + t.Name);
                    throw new Exception();
                }
            }
        }

        public UserResult UserExist(string phone)
        {
            try
            {
                if (phone.Equals("")) return new UserResult();

                string full = startURL + @"sys_user?sysparm_limit=10&sysparm_query=phone%3D" + escapePhone(phone);

                var user = (ServiceNowUser)callServiceNow(full, typeof(ServiceNowUser));
                writeLog("User called in: " + user.result[0].name);
                return user.result[0];
            }
            catch
            {
                UserResult u = new UserResult();
                u.noResultData = true;
                return u;
            }
        }

        public SNIncident LookupIncidentsByPhone(string phone)
        {
            try
            {
                if (phone.Equals("")) return new SNIncident();
                string full = startURL + @"incident?sysparm_limit=10&sysparm_query=active%3Dtrue%5Ecaller_id.phone%3D" + escapePhone(phone);

                var incidents = (SNIncident)callServiceNow(full, typeof(SNIncident));
                foreach (var t in incidents.result)
                {
                    t.state = resolveState(t.state);
                }
                return incidents;
            }
            catch
            {
                SNIncident u = new SNIncident();
                u.noResultData = true;
                return u;
            }
        }

        public IncidentResult LookupIncident(string num)
        {
            try
            {
                if (num.Equals("")) return new IncidentResult();
                string full = startURL + @"incident?sysparm_limit=10&sysparm_query=active%3Dtrue%5Enumber%3DINC" + num;

                var result = (SNIncident)callServiceNow(full, typeof(SNIncident));
                result.result[0].state = resolveState(result.result[0].state);
                return result.result[0];
            }
            catch
            {
                IncidentResult u = new IncidentResult();
                u.noResultData = true;
                return u;
            }
        }

        public Ticket LookupTicketsByPhone(string phone)
        {
            try
            {
                if (phone.Equals("")) return new Ticket();
                string full = startURL + @"u_tech_sales_tickets?sysparm_limit=10&sysparm_query=active%3Dtrue%5Eu_caller.phone%3D" + escapePhone(phone);

                var tickets = (Ticket)callServiceNow(full, typeof(Ticket));
                foreach (var t in tickets.result)
                {
                    t.state = resolveState(t.state);
                }
                return tickets;
            }
            catch
            {
                Ticket u = new Ticket();
                u.noResultData = true;
                return u;
            }
        }

        public TicketResult LookupTicket(string num)
        {
            try
            {
                if (num.Equals("")) return new TicketResult();
                string full = startURL + @"u_tech_sales_tickets?sysparm_limit=10&sysparm_query=number%3DTKS" + num;

                var result = (Ticket)callServiceNow(full, typeof(Ticket));
                result.result[0].state = resolveState(result.result[0].state);
                return result.result[0];
            }
            catch
            {
                TicketResult u = new TicketResult();
                u.noResultData = true;
                return u;
            }
        }

        public Requests LookupRequestsByPhone(string phone)
        {
            try
            {
                if (phone.Equals("")) return new Requests();
                string full = startURL + @"sc_request?sysparm_limit=10&sysparm_query=active%3Dtrue%5Erequested_for.phone%3D" + escapePhone(phone);

                var reqs = (Requests)callServiceNow(full, typeof(Requests));
                foreach (var t in reqs.result)
                {
                    t.state = resolveState(t.state);
                }
                return reqs;
            }
            catch
            {
                Requests u = new Requests();
                u.noResultData = true;
                return u;
            }
        }

        public RequestedItems LookupRequestedItem(string reqID)
        {
            try
            {
                if (reqID.Equals("")) return new RequestedItems();
                string full = startURL + @"sc_req_item?sysparm_limit=10&sysparm_query=request.sys_id%3D" + reqID;

                var result = (RequestedItems)callServiceNow(full, typeof(RequestedItems));
                foreach (var t in result.result)
                {
                    t.state = resolveState(t.state);
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                writeLog(e.Message);
                RequestedItems u = new RequestedItems();
                u.noResultData = true;
                return u;
            }
        }

        public Request LookupRequest(string num)
        {
            try
            {
                if (num.Equals("")) return new Request();
                var client = createClient();
                string url = startURL + @"sc_request?sysparm_limit=10&sysparm_query=active%3Dtrue%5Enumber%3DREQ";
                string full = url + num;
                var result = (Requests)callServiceNow(full, typeof(Requests));
                result.result[0].state = resolveState(result.result[0].state);
                return result.result[0];
            }
            catch
            {
                Request u = new Request();
                u.noResultData = true;
                return u;
            }
        }

        public newTicket SubmitTicket(string userId, string description)
        {
            try
            {
                if (userId.Equals("") || description.Equals("")) return new newTicket();

                string url = startURL + @"u_tech_sales_tickets";
                string body = "{'short_description':'" + description + "','u_caller':'" + userId + "'}";
                var content = (newTicket)uploadServiceNow(url, typeof(newTicket), body);
                writeLog("Created Ticket: " + content.result.number);
                return content;
            }
            catch
            {
                newTicket u = new newTicket();
                u.noResultData = true;
                return u;
            }
        }

        public newIncident SubmitIncident(string userID, string description, int urgency)
        {
            try
            {
                if (userID.Equals("") || description.Equals("")) return new newIncident();
                string url = startURL + @"incident";
                string body = "{'short_description':'" + description + "','caller_id':'" + userID + "','urgency':'" + urgency + "'}";
                var result = (newIncident)uploadServiceNow(url, typeof(newIncident), body);

                writeLog("Created Ticket: " + result.result.number);
                return result;
            }
            catch
            {
                newIncident u = new newIncident();
                u.noResultData = true;
                return u;
            }
        }

        public newRequest SubmitRequest(string userID, string[] itemIds)
        {
            try
            {
                if (userID.Equals("") || itemIds.Length < 1) return new newRequest();

                var client = createClient();
                string url = startURL + @"sc_request";
                string body = "{'requested_for':'" + userID + "'}";

                var result = (newRequest)uploadServiceNow(url, typeof(newRequest), body);
                string requestID = result.result.sys_id;

                string itemUrl = startURL + @"sc_req_item";
                foreach (string id in itemIds)
                {
                    if (id.Equals("") || id.Equals("hold")) continue;
                    string itemBody = "{'quantity':'1','request':'" + requestID + "','cat_item':'" + id + "'}";

                    uploadServiceNow(url, typeof(RequestedItem), body);
                }
                writeLog("Created Request: " + result.result.number);

                return result;
            }
            catch
            {
                newRequest u = new newRequest();
                u.noResultData = true;
                return u;
            }
        }

        public RequestedItem SubmitRequestItem(string itemId, string requestId)
        {
            try
            {
                string itemUrl = startURL + @"sc_req_item";
                string itemBody = "{'request':'" + requestId + "', 'cat_item':'" + itemId + "'}";
                var result = (RequestedItems)uploadServiceNow(itemUrl, typeof(RequestedItems), itemBody);
                return result.result[0];
            }
            catch
            {
                RequestedItem u = new RequestedItem();
                u.noResultData = true;
                return u;
            }


        }

        private WebClient createClient()
        {
            var token = "Bearer " + oauth.Instance.getAccessToken();
            var client = new WebClient();
            client.Headers.Add("Accept", "application/json");
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Authorization", token);
            return client;
        }

        private static string escapePhone(string p)
        {
            //"3175551212" 
            p = p.Insert(0, "(").Insert(4, ") ").Insert(9, "-");
            p = "+1 " + p;
            Debug.WriteLine(p);
            p = p.Replace("-", "%2D")
            .Replace("(", "%28")
            .Replace(")", "%29")
            .Replace("+", "%2B")
            .Replace(" ", "%20");
            Debug.WriteLine(p);
            return p;
        }

        private static string resolveState(string state)
        {
            //these are technically object dependant, This is ok for now. 
            switch (state)
            {
                case "9":
                    return "Cancelled - Approval Denied";
                case "8":
                    return "Blocked";
                case "7":
                    return "Rejected";
                case "6":
                    return "Deferred";
                case "5":
                    return "Incomplete";
                case "4":
                    return "Open";
                case "3":
                    return "Complete";
                case "2":
                    return "Approved";
                case "1":
                    return "Open";
                case "0":
                    return "Scoping";
                case "-1":
                    return "Scoping";
                case "-2":
                    return "Scoping";
                case "-3":
                    return "Awaiting Approval";
                case "-4":
                    return "Scoping";
                case "-5":
                    return "Preview Pending";
                case "-6":
                    return "Draft";
                default:
                    return "Unknown";
            }
        }

        public static void writeLog(string msg)
        {
            try  {
                if (!File.Exists(SNConfig.Instance.logLocation)) return;

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(SNConfig.Instance.logLocation, true))
                {
                    file.WriteLine(DateTime.Now + "    " + msg);
                }
            } catch {
                return;
            }
        }

    }
    #endregion
}
