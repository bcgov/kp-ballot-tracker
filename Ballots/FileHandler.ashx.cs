using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using WinSCP;

namespace Ballots
{
  public class FileHandler : IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {
      context.Response.ContentType = "text/plain";
      context.Response.Expires = -1;

      if (context.Request["fileAction"] == "delete")
      {
        DirectoryInfo di = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["LocalPath"] + @"\" + System.Configuration.ConfigurationManager.AppSettings["TempFolder"] + @"\" + context.Request["userid"]);

        if (di.Exists)
        {
          foreach (FileInfo file in di.GetFiles())
          {
            file.Delete();
          }
        }
        context.Response.Write("");
      }
      else
      {
        try
        {
#if DEBUG
          //This line triggers the debugger as the ASHX file won't be debugged otherwise. AAP 2015
          //System.Diagnostics.Debugger.Break();
#endif
          HttpPostedFile postedFile = context.Request.Files["Filedata"];
          string ed = context.Request["ed"];
          string uploadType = context.Request["uploadType"];
          string region = context.Request["region"];
          string uploadFolder = System.Configuration.ConfigurationManager.AppSettings["BallotExtractFolder"];
          string remotePath = System.Configuration.ConfigurationManager.AppSettings["RemotePath"];

          string sql = "SELECT sftp_host_name,winscp_user_name,ssh_host_key_fingerprint,private_key_path,private_key_passphrase FROM global_config";
          DataTable databaseRecords = getDatabaseRecords(sql);

          SessionOptions sessionOptions = new SessionOptions
          {
            Protocol = Protocol.Sftp,
            HostName = (string)databaseRecords.Rows[0]["sftp_host_name"],
            UserName = (string)databaseRecords.Rows[0]["winscp_user_name"],
            SshHostKeyFingerprint = (string)databaseRecords.Rows[0]["ssh_host_key_fingerprint"],
            SshPrivateKeyPath = (string)databaseRecords.Rows[0]["private_key_path"],
            PrivateKeyPassphrase = (string)databaseRecords.Rows[0]["private_key_passphrase"]
          };

          if (uploadType == "view" || uploadType == "view_mail")
          {
            string fileName = ed + ConfigurationManager.AppSettings["BallotFileName"];
            if (uploadType == "view_mail")
            {
              uploadFolder = System.Configuration.ConfigurationManager.AppSettings["MailBallotFolder"];
              fileName = ed + ConfigurationManager.AppSettings["MailBallotFileName"];
            }
            else
            {
              uploadFolder = "Region" + region;
              fileName = ed + ConfigurationManager.AppSettings["BallotFileName"];
            }

            //copy file to a user specific local folder on web server to view it
            string localPath = System.Configuration.ConfigurationManager.AppSettings["LocalPath"] + @"\" + System.Configuration.ConfigurationManager.AppSettings["TempFolder"] + @"\" + context.Request["userid"];

            Directory.CreateDirectory(localPath);
            remotePath = remotePath + "/" + uploadFolder + "/" + fileName;

            using (Session session = new Session())
            {
              session.Open(sessionOptions);
              session.GetFileToDirectory(remotePath, localPath);
              context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
              context.Response.Cache.SetExpires(DateTime.Now.ToUniversalTime());
              context.Response.Cache.SetMaxAge(new TimeSpan(0, 0, 0, 0));
              context.Response.ContentType = "application/pdf";
              context.Response.BinaryWrite(File.ReadAllBytes(localPath + @"\" + fileName));
            }
          }
          else
          {
            using (WinSCP.Session session = new WinSCP.Session())
            {
              session.Open(sessionOptions);
              Stream inputStream = postedFile.InputStream;

              string filename = ed + ConfigurationManager.AppSettings["BallotFileName"];

              if (uploadType == "proof")
              {
                filename = ed + ConfigurationManager.AppSettings["BallotFileName"];
                uploadFolder = "Region" + region;
              }
              else if (uploadType == "mail")
              {
                filename = ed + ConfigurationManager.AppSettings["MailBallotFileName"];
                uploadFolder = System.Configuration.ConfigurationManager.AppSettings["MailBallotFolder"];
              }

              session.PutFile(inputStream, remotePath + "/" + uploadFolder + "/" + filename);
            }
          }
        }
        catch (Exception ex)
        {
          BallotsUtil.Common.LogError("FileHandler", "ex.Message=" + ex.Message);
          context.Response.Write("Error: " + ex.Message);
          context.Response.StatusCode = 500;
        }
      }
    }

    private static DataTable getDatabaseRecords(string SQLString)
    {
      SqlConnection selectConnection = new SqlConnection(ConfigurationManager.AppSettings["BallotsConnectionString"]);
      SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(SQLString, selectConnection);
      DataTable dataTable = new DataTable();
      try
      {
        selectConnection.Open();
        sqlDataAdapter.Fill(dataTable);
      }
      catch (SqlException ex)
      {
        string message = ex.Message;
      }
      finally
      {
        selectConnection.Close();
      }
      return dataTable;
    }

    public bool IsReusable
    {
      get
      {
        return false;
      }
    }

  }
}