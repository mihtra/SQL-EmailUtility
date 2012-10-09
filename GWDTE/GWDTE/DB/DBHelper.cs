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
    class DBHelper
    {
        public string GetCreationScript()
        {
            StringBuilder CreationScript = new StringBuilder();
            DataSet Script;
            //Ping Security Suite Database and get Creation script
            String connectionString = ConfigurationHelper.GetConnectionString("ConnectionString");

            using (SqlConnection SQLConn = new SqlConnection(connectionString))
            {
                //get command text from a DB
                Script = SqlHelper.ExecuteDataset(SQLConn, "ss_usp_GetProcScript", null);

                foreach (DataRow dr in Script.Tables[0].Rows)
                {
                    CreationScript.Append(dr.ItemArray[0] + Environment.NewLine);
                }

            }
            return CreationScript.ToString();
        }

        public bool CreateStoredProcedure(string ConnectionString, string Server, string Database, string CreationScript)
        {

            bool isCreated = false;

            using (SqlConnection SQLConn = new SqlConnection(ConnectionString))
            {
                try
                {
                    Regex r = new Regex(@"^(\s|\t)*go(\s\t)?.*", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    foreach (string s in r.Split(CreationScript))
                    {
                        //Skip empty statements, in case of a GO and trailing blanks or something
                        string thisStatement = s.Trim();
                        if (String.IsNullOrEmpty(thisStatement)) continue;
                        SqlHelper.ExecuteNonQuery(SQLConn, CommandType.Text, s);
                    }
                    isCreated = true;
                }

                catch (SqlException ex)
                {
                    //TODO: Mail back to user
                    // ReturnString = ex.Message;

                }

            }
            return isCreated;
        }

        public string CallStoredProcedure(string ConnectionString, string FromUser, string ccList, string EmailSubject)
        {
            string ReturnString = string.Empty;
            try
            {
                SqlParameter[] sqlParams = new SqlParameter[3];

                sqlParams[0] = new SqlParameter("@fromuser", SqlDbType.NVarChar, 100);
                sqlParams[0].Value = FromUser;

                sqlParams[1] = new SqlParameter("@cclist", SqlDbType.NVarChar, 100);
                sqlParams[1].Value = ccList;

                sqlParams[2] = new SqlParameter("@perm_str ", SqlDbType.NVarChar, 4000);
                sqlParams[2].Value = EmailSubject;



                ReturnString = (string)SqlHelper.ExecuteScalar(ConnectionString, CommandType.StoredProcedure, "ss_usp_AssignPermission", sqlParams);

            }
            catch (Exception ex)
            {
                //TODO: return
                ReturnString = ConfigurationHelper.GetConfigurationValue("DefaultErrorString");
            }
            return ReturnString;

        }

    }
}
