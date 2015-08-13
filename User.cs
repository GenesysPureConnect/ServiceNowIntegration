using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowConnector
{

    [DataContract]
    public class ServiceNowUser
    {
        [DataMember(Name = "result")]
        public List<UserResult> result { get; set; }
        public bool noResultData { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in result)
            {
                sb.AppendLine("ID: " + item.sys_id + " " + item.name);
            }

            return sb.ToString() + " " + result.Count;
        }
    }

    public class UserResult
    {
        public bool noResultData { get; set; }
        public string u_ldap_country_code { get; set; }
        public string phone { get; set; }
        public Manager manager { get; set; }
        public string auditor { get; set; }
        public Location location { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string vip { get; set; }
        public string first_name { get; set; }
        public string on_schedule { get; set; }
        public SysDomain sys_domain { get; set; }
        public string sys_tags { get; set; }
        public string gender { get; set; }
        public string sys_mod_count { get; set; }
        public string employee_number { get; set; }
        public string calendar_integration { get; set; }
        public string middle_name { get; set; }
        public string sys_updated_on { get; set; }
        public string last_login { get; set; }
        public string country { get; set; }
        public string time_zone { get; set; }
        public string user_name { get; set; }
        public string email { get; set; }
        public string failed_attempts { get; set; }
        public string last_name { get; set; }
        public string u_user_account_control { get; set; }
        public string active { get; set; }
        public string introduction { get; set; }
        public Department department { get; set; }
        public string state { get; set; }
        public string sys_created_on { get; set; }
        public string agent_status { get; set; }
        public string mobile_phone { get; set; }
        public string title { get; set; }
        public string sys_updated_by { get; set; }
        public string name { get; set; }
        public string date_format { get; set; }
        public string notification { get; set; }
        public string zip { get; set; }
        public string building { get; set; }
        public string last_login_time { get; set; }
        public string sys_id { get; set; }
        public string photo { get; set; }
        public string sys_created_by { get; set; }
        public string internal_integration_user { get; set; }
        public string schedule { get; set; }
        public string source { get; set; }
        public string sys_class_name { get; set; }
        public string home_phone { get; set; }
        public string time_format { get; set; }
        public string preferred_language { get; set; }
    }
}
