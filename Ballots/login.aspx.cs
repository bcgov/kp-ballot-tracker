using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using BallotsUtil;

namespace Ballots
{
  public partial class BallotLogin : System.Web.UI.Page
  {
    protected PlaceHolder logBox = new PlaceHolder();
    protected TextBox userNameBox = new TextBox();
    protected TextBox userPassBox = new TextBox();

    protected void Page_Load(object sender, EventArgs e)
    {
      string str = "";
      this.logBox.Controls.Add((Control)new Literal()
      {
        Text = str.ToString()
      });
    }

    protected void btLoginClick(object sender, EventArgs e)
    {
      string userName = this.userNameBox.Text.ToString();
      string userPass = this.userPassBox.Text.ToString();
      string str1 = checkUserCreds(userName, userPass);

      switch (str1)
      {
        case "authSucess":
          if (getUserType(userName) == "DEO")
          {
            string str2 = "Authentication Success: " + getUserED(userName);
            this.logBox.Controls.Add((Control)new Literal()
            {
              Text = str2.ToString()
            });
            this.Session.Add("authDEOUserED", (object)getUserED(userName));
          }
          else
          {
            string str3 = "Authentication Success";
            this.logBox.Controls.Add((Control)new Literal()
            {
              Text = str3.ToString()
            });
          }
          this.Session.Add("authUserType", getUserType(userName));
          this.Session.Add("authUserName", (object)userName);
          this.Session.Add("authUserID", (object)getUserID(userName));
          this.Response.Redirect("ballot-tracker.aspx");
          break;
        case "authFail":
          string str4 = "Authentication Error: Incorrect Password (Check Case) " + userName + " - " + userPass + " - " + str1;
          this.logBox.Controls.Add((Control)new Literal()
          {
            Text = str4.ToString()
          });
          break;
        case "NoUser":
          string str5 = "Authentication Error: User does not exist";
          this.logBox.Controls.Add((Control)new Literal()
          {
            Text = str5.ToString()
          });
          break;
        default:
          string str6 = "Authentication Error - " + str1;
          this.logBox.Controls.Add((Control)new Literal()
          {
            Text = str6.ToString()
          });
          break;
      }
    }

    private string getUserID(string userName) => Convert.ToString(getDatabaseRecords("SELECT * FROM userACM where userName='" + userName + "'").Rows[0]["id"]).Trim();

    private string getUserED(string userName) => Convert.ToString(getDatabaseRecords("SELECT * FROM userACM where userName='" + userName + "'").Rows[0]["userED"]).Trim();

    private string getUserType(string userName)
    {
      return Convert.ToString(getDatabaseRecords("SELECT * FROM userACM where userName='" + userName + "'").Rows[0]["userType"]).Trim();
    }

    private string checkUserCreds(string userName, string userPass)
    {
      DataTable databaseRecords = getDatabaseRecords("SELECT * FROM userACM where userName='" + userName + "'");
      if (databaseRecords.Rows.Count <= 0)
        return "NoUser";
      object obj = (object)databaseRecords.Rows[0][nameof(userPass)].ToString();
      // return Convert.ToString(userPass).Substring(0, 7) == Convert.ToString(obj).Substring(0, 7) ? "authSucess" : "authFail";
      //not sure if the above code was trying to enforce a char minimum pwd but doing it that way throws a .Net error when pwd doesn't meet minimum,
      ////so removing the substring part of the check
      return Convert.ToString(userPass) == Convert.ToString(obj) ? "authSucess" : "authFail";
    }

    private DataTable getDatabaseRecords(string SQLString)
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
        //System.Web.HttpContext.Current.Response.Write("ERROR:" + ex.Message);
        this.errorLogBox.Controls.Add((Control)new Literal()
        {
          Text = message
        });
      }
      finally
      {
        selectConnection.Close();
      }
      return dataTable;
    }
  }
}