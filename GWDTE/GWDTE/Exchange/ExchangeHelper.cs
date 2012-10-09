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
    class ExchangeHelper
    {
        #region init
        DBHelper DBObj = new DBHelper();
        EmailHelper EmailObj = new EmailHelper();
        #endregion

        public void CreateExchangeInstance()
        {
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
            string ExchangeUserName = ConfigurationHelper.GetConfigurationValue("username");
            service.Credentials = new NetworkCredential(ConfigurationHelper.GetConfigurationValue("username"), ConfigurationHelper.GetConfigurationValue("password"), ConfigurationHelper.GetConfigurationValue("domain"));
            //Auto discovery can be diabled later if there is an exception
            service.AutodiscoverUrl(ExchangeUserName + ConfigurationHelper.GetConfigurationValue("CompanyEmailAddress"));
            //For tracing - Enable it for detail logging
            //service.TraceEnabled = true;
            //service.TraceFlags = TraceFlags.All;
            GetExchangeItems(service);


        }

        private void GetExchangeItems(ExchangeService service)
        {
            int unreadEmailCount = 0;
            SearchFilter searchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
            //itemView upperbound value hardcoded because we need to do selective search on unread emails.
            ItemView view = new ItemView(999);
            FindItemsResults<Item> findResults = service.FindItems(WellKnownFolderName.Inbox, searchFilter, view);
            unreadEmailCount = findResults.Items.Count;
            if (findResults.Items.Count > 0)
            {
                service.LoadPropertiesForItems(findResults.Items, PropertySet.FirstClassProperties);
                //string Script = "";
                string Script = DBObj.GetCreationScript();
                //FindItemsResults<Item> findResults = service.FindItems(WellKnownFolderName.Inbox, new ItemView(10));
                foreach (Item item in findResults.Items)
                {

                    string EmailProcessedMessage = ProcessExchangeItems(item, Script);
                    //Send email only in case we have something to return back to user.
                    if (!string.IsNullOrEmpty(EmailProcessedMessage))
                        EmailObj.SendEmail(service, item.Subject, EmailProcessedMessage, item);
                }
            }
        }

        private string ProcessExchangeItems(Item item, string Script)
        {
            string EmailProcessedMessage = String.Empty;
            string sender = ((Microsoft.Exchange.WebServices.Data.EmailMessage)(item)).Sender.Address;
            //check if sender is Domain user, if yes then 


            //Inputstring : <UserName>,<ServerName>,<Database>,<Permissions>
            //Example InputString: Sumit.verma,L3s-dbdvl2,master,read
            //Stored Procedure Calling: 
            //Parmaeter1: SenderName
            //Parameter2: CCList
            //Parameter3: aristotledc\Sumit.verma,L3s-dbdvl2,master,read

            #region ParseEmailSubject
            if (!String.IsNullOrEmpty(item.Subject) && item.Subject.IndexOf(',') > 0)
            {
                //Remove RE:(Reply) string from Email subject,RE: is constant string and will not change.
                if (item.Subject.StartsWith("RE:"))
                    item.Subject = item.Subject.Replace("RE:", String.Empty).Trim();

                string[] EmailSubject = item.Subject.Split(new char[] { ',' });
                //First string contains Servername''
                //EmailSubject[] should conatin following index's
                //1. UserName
                //2. ServerName
                //3. Database Name
                //4. Permissions seperated by comma e.g. read,write

                //TODO : remove @aristotle.com(CompanyName) from UserName and append "AristotleDC" (Domain Name) to it.
                if (EmailSubject.Length > 2)
                {
                    string UserName = EmailSubject[0].Trim();
                    string ServerName = EmailSubject[1].Trim();
                    string DatabaseName = EmailSubject[2].Trim();
                    //Create Dynamic Connection String
                    string ConnectionString = String.Format("Data Source={0};Initial Catalog={1}; Integrated Security=true", ServerName, DatabaseName);
                    if (DBObj.CreateStoredProcedure(ConnectionString, ServerName, DatabaseName, Script))
                    {
                        EmailProcessedMessage = DBObj.CallStoredProcedure(ConnectionString, sender, item.DisplayCc, item.Subject);
                    }
                    else
                        EmailProcessedMessage =ConfigurationHelper.GetConfigurationValue("DefaultErrorString");
                }
                else
                {
                    //Send Email stating Parameters not valid.
                    EmailProcessedMessage = String.Empty;
                }
            #endregion
            }
            //Mark email as read
            ((Microsoft.Exchange.WebServices.Data.EmailMessage)(item)).IsRead = true;
            item.Update(ConflictResolutionMode.AutoResolve);
            return EmailProcessedMessage;
        }
    }
}
