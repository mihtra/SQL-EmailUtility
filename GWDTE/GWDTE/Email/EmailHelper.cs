using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Exchange.WebServices.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.DirectoryServices;

namespace SQLTechSuite
{
    class EmailHelper
    {
        //TODO List:
        ///Call stored procedure to grant permissions
        ///Pass Subject and CC list as parameters to Stored procedure
        ///TODO: Check for only un-read items 
        ///Check for DOS attacks - 
        ///Solution: Keep the last 10 entries in Hashtable and look for values in it before assigning values.
        //
        //First get stored procedure creation script from DB
        //Create the stored procedure to server listed in email
        //Call the newly created stored procedure and pass EmailFrom,Servername,DBName and Permission list.

        public void SendEmail(ExchangeService service, string Subject, string EmailBody, Item item)
        {
            var message = (EmailMessage)Item.Bind(service, item.Id, PropertySet.FirstClassProperties);
            var reply = message.CreateReply(true);
            reply.BodyPrefix = EmailBody;
            reply.SendAndSaveCopy();

        }

        public bool UserInDomain(string username, string domain)
        {
            string LDAPString = String.Empty;
            string[] domainComponents = domain.Split('.');
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < domainComponents.Length; i++)
            {
                builder.AppendFormat(",dc={0}", domainComponents[i]);
            }
            if (builder.Length > 0)
                LDAPString = builder.ToString(1, builder.Length - 1);

            DirectoryEntry entry = new DirectoryEntry("LDAP://" + LDAPString);

            DirectorySearcher searcher = new DirectorySearcher(entry);

            searcher.Filter = "sAMAccountName=" + username;

            SearchResult result = searcher.FindOne();

            return (result != null) ? true : false;
        }
    }


}
