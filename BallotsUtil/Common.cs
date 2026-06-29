using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BallotsUtil
{
  public static class Common
  {

    public static void LogDebug(string logSource, string logMessage)
    {
      LogMessage("DEBUG", logSource, logMessage);
    }

    public static void LogError(string logSource, string logMessage)
    {
      LogMessage("ERROR", logSource, logMessage);
    }

    private static void LogMessage(string logLevel, string logSource, string logMessage)
    {
      SqlConnection selectConnection = new SqlConnection();
      try
      {
        string systemLogLevel = ConfigurationManager.AppSettings["LogLevel"].ToString();
        if ((systemLogLevel == "DEBUG" && (logLevel == "DEBUG" || logLevel == "ERROR")) || (logLevel == "ERROR" && (systemLogLevel == "ERROR" || systemLogLevel == "DEBUG")))
        {
          selectConnection = new SqlConnection(ConfigurationManager.AppSettings["BallotsConnectionString"]);
          using (SqlCommand sqlCommand = selectConnection.CreateCommand())
          {
            sqlCommand.CommandText = @"INSERT INTO common_log (log_level, log_source, log_message, log_date) VALUES (@log_level, @log_source, @log_message, @log_date)";
            sqlCommand.Parameters.Add(new SqlParameter("@log_level", logLevel));
            sqlCommand.Parameters.Add(new SqlParameter("@log_source", logSource));
            sqlCommand.Parameters.Add(new SqlParameter("@log_message", logMessage));
            sqlCommand.Parameters.Add(new SqlParameter("@log_date", DateTime.Now.ToString()));

            selectConnection.Open();
            sqlCommand.ExecuteNonQuery();
            selectConnection.Close();
          }
        }
      }
      catch (SqlException ex)
      {
        string message = ex.Message;
      }
      finally
      {
        selectConnection.Close();
      }
    }
  }
}
