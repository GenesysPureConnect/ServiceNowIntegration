using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace ServiceNowConnector
{
    [ServiceContract]
    public interface IServiceNowConnector
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        UserResult UserExist(string phone);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        IncidentResult LookupIncident(string num);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        SNIncident LookupIncidentsByPhone(string phone);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        TicketResult LookupTicket(string num);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        Ticket LookupTicketsByPhone(string phone);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        Requests LookupRequestsByPhone(string phone);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        RequestedItems LookupRequestedItem(string reqID);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        Request LookupRequest(string num);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        newTicket SubmitTicket(string userId, string description);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        newIncident SubmitIncident(string userID, string description, int urgency);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        newRequest SubmitRequest(string userID, string[] itemIds);

        [OperationContract]
        [WebInvoke(Method = "GET",
             ResponseFormat = WebMessageFormat.Xml,
             RequestFormat = WebMessageFormat.Xml,
             BodyStyle = WebMessageBodyStyle.Wrapped)]
        RequestedItem SubmitRequestItem(string itemId, string requestId);
    }
}