using BallotsUtil;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using WinSCP;

namespace Ballots
{
  public partial class BallotTracker : Page
  {
    protected PlaceHolder BallotsTable = new PlaceHolder();
    protected PlaceHolder ProgressStats = new PlaceHolder();
    protected PlaceHolder percComplete = new PlaceHolder();
    protected PlaceHolder currUser = new PlaceHolder();
    protected PlaceHolder currUserEDName = new PlaceHolder();

    protected void Page_Load(object sender, EventArgs e)
    {
      BallotFileUploadControl.Attributes["onchange"] = "uploadBallotFiles(this)";
      MailInBallotFileUploadControl.Attributes["onchange"] = "uploadMailInBallotFiles(this)";
      if (this.Session["authUserName"] == null)
      {
        this.Response.Redirect("Login.aspx");
      }
      else
      {
        string str = "<form id=\"formUserInfo\" runat=\"server\"><input type=\"hidden\" id=\"userid\" name=\"userid\" value=\"" + this.Session["authUserID"].ToString() + "\"></form>" + BallotTracker.getUserName(Convert.ToString(this.Session["authUserID"].ToString()));
        this.currUser.Controls.Add((Control)new Literal()
        {
          Text = str
        });
      }

      if (GetUserType() == "DEO")
      {
        StatsAndTabsPanel.Visible = false;
        DeoViewPanel.Visible = true;

        this.currUserEDName.Controls.Add((Control)new Literal()
        {
          Text = BallotTracker.getUserEDName(this.Session["authDEOUserED"].ToString())
        });
        this.getBallotsTable(0);
      }
      else if (GetUserType() == "RFO")
      {
        StatsAndTabsPanel.Visible = false;
        DeoViewPanel.Visible = false;
        this.getBallotsTable(5);
      }
      else
      {
        StatsAndTabsPanel.Visible = true;
        DeoViewPanel.Visible = false;

        if (GetUserType() == "DVS")
        {
          StatsAndTabsPanel.Visible = false;
          this.getBallotsTable(7);
        }
        else
        {
          int ballotsTable1 = this.getBallotsTable(1);
          int ballotsTable2 = this.getBallotsTable(2);
          int ballotsTable3 = this.getBallotsTable(3);
          int ballotsTable4 = this.getBallotsTable(4);
          int ballotsTable7 = this.getBallotsTable(7); //even though we don't use the ballotsTable7 value for statistics anymore need to call the getBallotsTable function to populate the Overview tab
          this.ProgressStats.Controls.Add((Control)new Literal()
          {
            Text = BallotTracker.createStatusDash(ballotsTable1, ballotsTable2, ballotsTable3 - GetBallotRejectCount(), ballotsTable4)
          });
        }
      }

      userType.Value = GetUserType();
    }



    private int GetBallotRejectCount()
    {
      string sql = "SELECT ebt.*, ei.edName FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.proofPath is NOT NULL AND ebt.ballotApproveUser is NULL AND ebt.ballotRejectDT IS NOT NULL ORDER BY ebt.ED";
      DataTable databaseRecords = BallotTracker.getDatabaseRecords(sql);
      return databaseRecords.Rows.Count;
    }

    private int getBallotsTable(int tabID)
    {
      StringBuilder stringBuilder = new StringBuilder();
      string SQLString;
      switch (tabID)
      {
        case 0:
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.ED ='" + this.Session["authDEOUserED"].ToString() + "'";
          break;
        case 1:
          stringBuilder.Append("<div class=\"table view-upload\">");
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.xlsPath is NULL ORDER BY ED";
          break;
        case 2:
          stringBuilder.Append("<div class=\"table view-qp\">");
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.xlsPath IS NOT NULL AND (ebt.proofPath IS NULL OR ebt.ballotRejectDT IS NOT NULL) ORDER BY ebt.ED";
          break;
        case 3:
          stringBuilder.Append("<div class=\"table view-ebc\">");
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.proofPath IS NOT NULL AND ebt.ballotApproveUser IS NULL ORDER BY ebt.ED";
          break;
        case 4:
          stringBuilder.Append("<div class=\"table view-printer\">");
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.ballotApproveUser is NOT NULL ORDER BY ebt.ED";
          break;
        case 5:
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 AND ebt.ED IN (SELECT ED FROM User_ED WHERE userID = " + this.Session["authUserID"].ToString() + ") ORDER BY ebt.ED";
          break;
        default:
          stringBuilder.Append("<div class=\"table view-all\">");
          SQLString = "SELECT ebt.*, ei.edName, ei.region FROM ED_Ballot_Track ebt, ED_Info ei WHERE ebt.ed = ei.ed AND ebt.archive <> 1  AND ei.archive <> 1 ORDER BY ebt.ED";
          break;
      }
      DataTable databaseRecords = BallotTracker.getDatabaseRecords(SQLString);
      stringBuilder.Append("<table>");
      stringBuilder.Append(createTableHeader());
      stringBuilder.Append("<tbody>");
      stringBuilder.Append(Environment.NewLine);

      foreach (DataRow row in (InternalDataCollectionBase)databaseRecords.Rows)
      {
        stringBuilder.Append("<tr>");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("<td class=\"ED\" title=\"" + row["edName"] + "\">");
        stringBuilder.Append(row["ed"]);
        stringBuilder.Append("</td>");
        stringBuilder.Append(Environment.NewLine);

        if (ShowColumn("ED_NAME"))
        {
          stringBuilder.Append("<td>");
          stringBuilder.Append(row["edName"]);
          stringBuilder.Append("</td>");
          stringBuilder.Append(Environment.NewLine);
        }

        if (ShowColumn("EXTRACT_UPLOAD"))
        {
          if (row["xlsUpDT"] != DBNull.Value)
          {
            stringBuilder.Append("<td><span class=\"timestamp\" title=\"");
            stringBuilder.Append(row["xlsUpDT"]);
            stringBuilder.Append("\"><i class=\"fa fa-clock-o\"></i></span><span class=\"userstamp\" title=\"");
            stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["xlsUpUser"])));
            stringBuilder.Append("\"><i class=\"fa fa-user\"></i></span>");
            if (GetUserType() == "EBC" || GetUserType() == "QP")
            {
              if (tabID != 7 && tabID != 4)
              {
                stringBuilder.Append("<div><input type='button' style='width:145px' class='btnDashboard' value='Re-Upload Required' onclick=\"Javascript:reUploadExtract('" + row["ed"] + "');\" /></div>");
              }
            }
            stringBuilder.Append("</td>");
          }
          else
          {
            if (tabID == 7)
            {
              stringBuilder.Append("<td><button type='button' class='disabled_select_files' disabled>SELECT FILES</button><br/>");
              stringBuilder.Append("<button type='button' class='disabled_button' disabled>Upload</button></td>");
            }
            else
            {
              stringBuilder.Append("<td><input type='file' class='upload_queue " + row["ed"] + "' name='upload_queue_extract_" + row["ed"] + "'  id='upload_queue_extract_" + row["ed"] + "_" + row["region"] + "'/>");
              stringBuilder.Append("<button type='button' class='upload_button' name='upload_button_" + row["ed"] + "' jobID='extract_" + row["ed"] + "' id='upload_button_extract_" + row["ed"] + "_" + row["region"] + "'>Upload</button></td>");
            }
          }
          stringBuilder.Append(Environment.NewLine);
        }

        if (ShowColumn("PROOF_UPLOAD"))
        {
          if ((row["xlsUpDT"] != DBNull.Value && row["ballotPath"] == DBNull.Value) || row["ballotRejectDT"] != DBNull.Value)
          {
            if (tabID == 7 || tabID == 3)
            {
              stringBuilder.Append("<td><button type='button' class='disabled_select_files' disabled>SELECT FILES</button><br/>");
              stringBuilder.Append("<button type='button' class='disabled_button' disabled>Upload</button></td>");
            }
            else
            {
              stringBuilder.Append("<td><input type='file' class='upload_queue " + row["ed"] + "' name='upload_queue_proof_" + row["ed"] + "'  id='upload_queue_proof_" + row["ed"] + "_" + row["region"] + "' />");
              stringBuilder.Append("<button type='button' class='upload_button' name='upload_button_" + row["ed"] + "' jobID='proof_" + row["ed"] + "' id='upload_button_proof_" + row["ed"] + "_" + row["region"] + "'>Upload</button></td>");
            }
          }
          else if (row["proofUploadDT"] != DBNull.Value && row["ballotRejectDT"] == DBNull.Value)
          {
            stringBuilder.Append("<td><span class=\"timestamp\" title=\"");
            stringBuilder.Append(row["proofUploadDT"]);
            stringBuilder.Append("\"><i class=\"fa fa-clock-o\"></i></span><span class=\"userstamp\" title=\"");
            stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["proofUploadUser"])));
            stringBuilder.Append("\"><i class=\"fa fa-user\"></i></span></td>");
          }
          else
          {
            stringBuilder.Append("<td></td>");
          }
          stringBuilder.Append(Environment.NewLine);
        }

        if (ShowColumn("MAIL_BALLOT_UPLOAD"))
        {
          if ((row["xlsUpDT"] != DBNull.Value && row["mailBallotPath"] == DBNull.Value) || row["mailBallotRejectDT"] != DBNull.Value)
          {
            if (tabID == 7)
            {
              stringBuilder.Append("<td><button type='button' class='disabled_select_files' disabled>SELECT FILES</button><br/>");
              stringBuilder.Append("<button type='button' class='disabled_button' disabled>Upload</button></td>");
            }
            else
            {
              stringBuilder.Append("<td><input type='file' class='upload_queue " + row["ed"] + "' name='upload_queue_mail_" + row["ed"] + "'  id='upload_queue_mail_ballot_" + row["ed"] + "' />");
              stringBuilder.Append("<button type='button' class='upload_button' name='upload_button_" + row["ed"] + "' jobID='mail_ballot_" + row["ed"] + "' id='upload_button_mail_ballot_" + row["ed"] + "'>Upload</button></td>");
            }
          }
          else if (row["mailBallotUploadDT"] != DBNull.Value && row["mailBallotRejectDT"] == DBNull.Value)
          {
            stringBuilder.Append("<td><span class=\"timestamp\" title=\"");
            stringBuilder.Append(row["mailBallotUploadDT"]);
            stringBuilder.Append("\"><i class=\"fa fa-clock-o\"></i></span><span class=\"userstamp\" title=\"");
            stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["mailBallotUploadUser"])));
            stringBuilder.Append("\"><i class=\"fa fa-user\"></i></span></td>");
          }
          else
          {
            stringBuilder.Append("<td></td>");
          }
          stringBuilder.Append(Environment.NewLine);
        }

        if (ShowColumn("BALLOT_PROOFS"))
        {
          if (row["ballotPath"] != DBNull.Value)
          {
            if (row["ballotApproveDT"] != DBNull.Value)
            {
              if (tabID == 7 || tabID == 2)
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:110px' class=\"btnDashboardDisabled\" value=\"View Proof\"/><br>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:110px' class=\"btnDashboard btn-proof\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["BallotFileName"] + "\" id=\"" + row["ed"] + "-ballot-proof-" + row["region"] + "\" value=\"View Proof\"/><br>");
              }
              stringBuilder.Append("<span class=\"qty-match-yes\" title=\"");
              stringBuilder.Append(row["ballotApproveDT"]);
              stringBuilder.Append(" ");
              stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["ballotApproveUser"])));
              stringBuilder.Append("\">Approved");
              stringBuilder.Append("</span></td>");
            }
            else if (row["ballotRejectDT"] != DBNull.Value)
            {
              if (tabID == 7 || tabID == 2)
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:110px' class=\"btnDashboardDisabled\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["BallotFileName"] + "\" id=\"" + row["ed"] + "-ballot-proof-" + row["region"] + "\" value=\"View proof\"/><br>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:110px' class=\"btnDashboard btn-proof\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["BallotFileName"] + "\" id=\"" + row["ed"] + "-ballot-proof-" + row["region"] + "\" value=\"View proof\"/><br>");
              }
              stringBuilder.Append("<span class=\"qty-match-no\" title=\"");
              stringBuilder.Append(row["ballotRejectDT"]);
              stringBuilder.Append(" ");
              stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["ballotRejectUser"])));
              stringBuilder.Append("\">Not Approved");
              stringBuilder.Append("</span></td>");
            }
            else
            {
              if (tabID == 7 || tabID == 2)
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:110px' class=\"btnDashboardDisabled\" value=\"View proof\"/><br>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:110px' class=\"btnDashboard btn-proof\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["BallotFileName"] + "\" id=\"" + row["ed"] + "-ballot-proof-" + row["region"] + "\" value=\"View proof\"/></td>");
              }
            }
          }
          else
          {
            stringBuilder.Append("<td> </td>");
            stringBuilder.Append(Environment.NewLine);
          }
        }

        if (ShowColumn("MAIL_BALLOT_PROOFS"))
        {
          if (tabID == 2)
          {
            stringBuilder.Append("<td></td>");
          } 
          else if (row["mailBallotPath"] != DBNull.Value)
          {
            if (row["mailBallotApproveDT"] != DBNull.Value)
            {
              if (tabID == 7)
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:135px' class=\"btnDashboardDisabled\" value=\"View Mail-in Ballot\"/><br>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:135px' class=\"btnDashboard btn-mail-ballot\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["MailBallotFileName"] + "\" id=\"" + row["ed"] + "-mail-ballot\" value=\"View Mail-in Ballot\"/><br>");
              }
              stringBuilder.Append("<span class=\"qty-match-yes\" title=\"");
              stringBuilder.Append(row["mailBallotApproveDT"]);
              stringBuilder.Append(" ");
              stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["mailBallotApproveUser"])));
              stringBuilder.Append("\">Approved");
              stringBuilder.Append("</span></td>");
            }
            else if (row["mailBallotRejectDT"] != DBNull.Value)
            {
              if (tabID == 7)
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:135px' class=\"btnDashboardDisabled\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["MailBallotFileName"] + "\" id=\"" + row["ed"] + "-mail-ballot\" value=\"View Mail-in Ballot\"/><br>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:135px' class=\"btnDashboard btn-mail-ballot\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["MailBallotFileName"] + "\" id=\"" + row["ed"] + "-mail-ballot\" value=\"View Mail-in Ballot\"/><br>");
              }
              stringBuilder.Append("<span class=\"qty-match-no\" title=\"");
              stringBuilder.Append(row["mailBallotRejectDT"]);
              stringBuilder.Append(" ");
              stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["mailBallotRejectUser"])));
              stringBuilder.Append("\">Not Approved");
              stringBuilder.Append("</span></td>");
            }
            else
            {
              if (tabID == 7)
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:135px' class=\"btnDashboardDisabled\" value=\"View Mail-in Ballot\"/><br>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" style='width:135px' class=\"btnDashboard btn-mail-ballot\" name=\"" + row["ed"] + ConfigurationManager.AppSettings["MailBallotFileName"] + "\" id=\"" + row["ed"] + "-mail-ballot\" value=\"View Mail-in Ballot\"/></td>");
              }
            }
          }
          else
          {
            stringBuilder.Append("<td> </td>");
            stringBuilder.Append(Environment.NewLine);
          }
        }

        if (ShowColumn("HARD_COPY_PROOF_AVAILABLE"))
        {
          if (row["ballotApproveDT"] != DBNull.Value)
          {
            if (row["hardCopyApproveDT"] != DBNull.Value)
            {
              stringBuilder.Append("<td>");
              stringBuilder.Append("<span class=\"qty-match-yes\" title=\"");
              stringBuilder.Append(row["hardCopyApproveDT"]);
              stringBuilder.Append(" ");
              stringBuilder.Append(BallotTracker.getUserName(Convert.ToString(row["hardCopyApproveUser"])));
              stringBuilder.Append("\">Approved");
              stringBuilder.Append("</span></td>");
            }
            else
            {
              if (tabID == 7)
              {
                stringBuilder.Append("<td><input type=\"button\" class=\"btnDashboardDisabled\" name=\"" + row["ed"] + "-HardCopyApproved\" id=\"" + row["ed"] + "-HardCopyApproved\" value=\"Approve\" disabled\"/></td>");
              }
              else
              {
                stringBuilder.Append("<td><input type=\"button\" class=\"btnDashboard\" name=\"" + row["ed"] + "-HardCopyApproved\" id=\"" + row["ed"] + "-HardCopyApproved\" value=\"Approve\" onclick=\"Javascript:approveHardCopy('" + row["ed"] + "');\"/></td>");
              }
            }
          }
          else
          {
            stringBuilder.Append("<td> </td>");
          }
          stringBuilder.Append(Environment.NewLine);
        }

        if (ShowColumn("DEO_REVIEW_BALLOT"))
        {
          stringBuilder.Append("<td>");
          if (row["ballotApproveDT"] != DBNull.Value)
          {
            stringBuilder.Append("<input type=\"button\" class=\"btnDashboard btn-ballot\" id=\"" + row["ed"].ToString() + "-ballot-approved-" + row["region"].ToString() + "\" name=\"" + row["ed"].ToString() + ConfigurationManager.AppSettings["BallotFileName"] + "\" value=\"View Ballot\"/>");
          }
          stringBuilder.Append("</td>");
          stringBuilder.Append(Environment.NewLine);
        }
      }
      stringBuilder.Append(Environment.NewLine);
      stringBuilder.Append("</tbody>");
      stringBuilder.Append("</table>");
      if (tabID != 0)
      {
        stringBuilder.Append("</div>");
      }
      stringBuilder.Append(Environment.NewLine);
      stringBuilder.Append(Environment.NewLine);
      this.BallotsTable.Controls.Add((Control)new Literal()
      {
        Text = stringBuilder.ToString()
      });
      return databaseRecords.Rows.Count;
    }

    private static string createStatusDash(
      int statUpload,
      int statQP,
      int statEBC,
      int statPrinter)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("<ul id=\"progressStats\">");
      stringBuilder.Append("<li class=\"stat\" id=\"upload-stat\"><span class=\"number\" id=\"step1\">" + (object)statUpload + "</span><span class=\"step-name\">waiting for extract upload</span></li>");
      stringBuilder.Append("<li class=\"stat\" id=\"layout-stat\"><span class=\"number\" id=\"step2\">" + (object)statQP + "</span><span class=\"step-name\">extract uploaded</span></li>");
      stringBuilder.Append("<li class=\"stat\" id=\"layout-stat\"><span class=\"number\" id=\"step3\">" + (object)statEBC + "</span><span class=\"step-name\">ballot proof<br />uploaded</span></li>");
      stringBuilder.Append("<li class=\"stat\" id=\"proofing-stat\"><span class=\"number\" id=\"step4\">" + (object)statPrinter + "</span><span class=\"step-name\">ballot proof<br />approved</span></li>");
      stringBuilder.Append("</ul>");
      return stringBuilder.ToString();
    }

    private string GetUserType()
    {
      string userType = "";
      if (this.Session["authUserType"] != null)
      {
        userType = this.Session["authUserType"].ToString();
      }
      return userType;
    }

    private bool ShowColumn(string column)
    {
      bool showColumn = true;
      switch (GetUserType())
      {
        case "DEO":
          switch (column)
          {
            case "EXTRACT_UPLOAD":
            case "PROOF_UPLOAD":
            case "MAIL_BALLOT_UPLOAD":
            case "BALLOT_PROOFS":
            case "MAIL_BALLOT_PROOFS":
            case "HARD_COPY_PROOF_AVAILABLE":
              showColumn = false;
              break;
            default:
              break;
          }
          break;
        case "RFO":
          switch (column)
          {
            case "EXTRACT_UPLOAD":
            case "PROOF_UPLOAD":
            case "MAIL_BALLOT_UPLOAD":
            case "BALLOT_PROOFS":
            case "MAIL_BALLOT_PROOFS":
            case "HARD_COPY_PROOF_AVAILABLE":
              showColumn = false;
              break;
            default:
              break;
          }
          break;
        default:
          switch (column)
          {
            case "ED_NAME":
              showColumn = false;
              break;
            default:
              break;
          }
          break;
      }

      return showColumn;
    }

    private void AddColumnHeader(ref StringBuilder stringBuilder, string column)
    {
      if (column == "ED")
      {
        stringBuilder.Append("<th rowspan=\"2\">ED</th>");
      }
      else if (column == "ED_NAME" && ShowColumn(column))
      {
        stringBuilder.Append("<th rowspan=\"2\">Electoral District Name</th>");
      }
      else if (column == "EXTRACT_UPLOAD" && ShowColumn(column))
      {
        stringBuilder.Append("<th rowspan=\"2\">Extract<br/>Upload</th>");
      }
      else if (column == "PROOF_UPLOAD" && ShowColumn(column))
      {
        stringBuilder.Append("<th rowspan=\"2\">Ballot Proof<br/>Upload</th>");
      }
      else if (column == "MAIL_BALLOT_UPLOAD" && ShowColumn(column))
      {
        stringBuilder.Append("<th rowspan=\"2\">Mail-In Ballot<br/>Upload</th>");
      }
      else if (column == "BALLOT_PROOFS" && ShowColumn(column))
      {
        stringBuilder.Append("<th>Ballot Proofs</th>");
      }
      else if (column == "MAIL_BALLOT_PROOFS" && ShowColumn(column))
      {
        stringBuilder.Append("<th>Mail-In Ballots</th>");
      }
      else if (column == "HARD_COPY_PROOF_AVAILABLE" && ShowColumn(column))
      {
        stringBuilder.Append("<th colspan=\"1\">Hard Copy Proof<br/>DEO Approved</th>");
      }
      else if (column == "DEO_REVIEW_BALLOT" && ShowColumn(column))
      {
        stringBuilder.Append("<th rowspan=\"2\">DEO/DDEO<br />View Ballot</th>");
      }
    }

    private string createTableHeader()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("<thead>");
      stringBuilder.Append("<tr>");

      AddColumnHeader(ref stringBuilder, "ED");
      AddColumnHeader(ref stringBuilder, "ED_NAME");
      AddColumnHeader(ref stringBuilder, "EXTRACT_UPLOAD");
      AddColumnHeader(ref stringBuilder, "PROOF_UPLOAD");
      AddColumnHeader(ref stringBuilder, "MAIL_BALLOT_UPLOAD");
      AddColumnHeader(ref stringBuilder, "BALLOT_PROOFS");
      AddColumnHeader(ref stringBuilder, "MAIL_BALLOT_PROOFS");
      AddColumnHeader(ref stringBuilder, "HARD_COPY_PROOF_AVAILABLE");
      AddColumnHeader(ref stringBuilder, "DEO_REVIEW_BALLOT");

      stringBuilder.Append("</tr>");
      stringBuilder.Append("</thead>");
      stringBuilder.Append(Environment.NewLine);
      stringBuilder.Append(Environment.NewLine);
      return stringBuilder.ToString();
    }

    private static string getUserName(string userID)
    {
      DataTable databaseRecords = BallotTracker.getDatabaseRecords("SELECT * FROM userACM where id=" + userID);
      return databaseRecords.Rows.Count > 0 ? Convert.ToString(databaseRecords.Rows[0]["displayName"]) : "NoUser";
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

    [WebMethod]
    public static bool ApproveProof(string ed, string userid)
    {
      if (CheckSession())
      {
        string SQLString = "UPDATE ED_Ballot_Track SET ballotApproveDT=GETDATE(), ballotApproveUser='" + userid + "', ballotRejectDT=NULL, ballotRejectUser=NULL WHERE ed='" + ed + "' AND archive <> 1";

        try
        {
          BallotTracker.setDatabaseRecords(SQLString);
        }
        catch
        {
          Console.Write("Error writing to database");
          return false;
        }
      }

      return true;
    }


    [WebMethod]
    public static bool RejectProof(string ed, string userid)
    {
      if (CheckSession())
      {
        string SQLString = "UPDATE ED_Ballot_Track SET ballotApproveDT=NULL, ballotApproveUser=NULL, ballotRejectDT=GETDATE(), ballotRejectUser='" + userid + "' WHERE ed='" + ed + "' AND archive <> 1";

        try
        {
          BallotTracker.setDatabaseRecords(SQLString);
        }
        catch
        {
          Console.Write("Error writing to database");
          return false;
        }

        try
        {
          BallotTracker.SendEmail(ed, userid, "rejected");
        }
        catch
        {
          Console.Write("Error sending email");
          return false;
        }
      }

      return true;
    }

    [WebMethod]
    public static bool ApproveMailBallot(string ed, string userid)
    {
      if (CheckSession())
      {
        string SQLString = "UPDATE ED_Ballot_Track SET mailBallotApproveDT=GETDATE(), mailBallotApproveUser='" + userid + "', mailBallotRejectDT=NULL, mailBallotRejectUser=NULL WHERE ed='" + ed + "' AND archive <> 1";

        try
        {
          BallotTracker.setDatabaseRecords(SQLString);
        }
        catch
        {
          Console.Write("Error writing to database");
          return false;
        }
      }

      return true;
    }


    [WebMethod]
    public static bool RejectMailBallot(string ed, string userid)
    {
      if (CheckSession())
      {
        string SQLString = "UPDATE ED_Ballot_Track SET mailBallotApproveDT=NULL, mailBallotApproveUser=NULL, mailBallotRejectDT=GETDATE(), mailBallotRejectUser='" + userid + "' WHERE ed='" + ed + "' AND archive <> 1";

        try
        {
          BallotTracker.setDatabaseRecords(SQLString);
        }
        catch
        {
          Console.Write("Error writing to database");
          return false;
        }

        try
        {
          BallotTracker.SendEmail(ed, userid, "mail_ballot_rejected");
        }
        catch
        {
          Console.Write("Error sending email");
          return false;
        }
      }

      return true;
    }

    [WebMethod]
    public static bool ApproveHardCopy(string ed, string userid)
    {
      if (CheckSession())
      {
        string SQLString = "UPDATE ED_Ballot_Track SET hardCopyApproveDT=GETDATE(), hardCopyApproveUser=" + userid + " WHERE ed='" + ed + "' AND archive <> 1";

        try
        {
          BallotTracker.setDatabaseRecords(SQLString);
        }
        catch
        {
          Console.Write("Error writing to database");
          return false;
        }
      }

      return true;
    }

    [WebMethod]
    public static bool ReUploadExtract(string ed)
    {
      if (CheckSession())
      {
        string SQLString = "UPDATE ED_Ballot_Track SET xlsUpDT=NULL, xlsUpUser=NULL, xlsPath=NULL, proofPath=NULL, ballotPath=NULL, ballotApproveDT=NULL, ballotApproveUser=NULL, proofUploadDT=NULL, proofUploadUser=NULL, hardCopyApproveDT=NULL, hardCopyApproveUser=NULL, ballotRejectDT=NULL, ballotRejectUser=NULL WHERE ed='" + ed + "' AND archive <> 1";

        try
        {
          BallotTracker.setDatabaseRecords(SQLString);
        }
        catch
        {
          Console.Write("Error writing to database");
          return false;
        }
      }

      return true;
    }

    private static bool setDatabaseRecords(string SQLString)
    {
      SqlConnection selectConnection = new SqlConnection(ConfigurationManager.AppSettings["BallotsConnectionString"]);
      SqlCommand sqlCommand = new SqlCommand(SQLString);
      SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(SQLString, selectConnection);
      try
      {
        sqlCommand.Connection = selectConnection;
        selectConnection.Open();
        sqlCommand.ExecuteNonQuery();
        selectConnection.Close();
        return true;
      }
      catch (SqlException ex)
      {
        string message = ex.Message;
        return false;
      }
      finally
      {
        selectConnection.Close();
      }
    }

    [WebMethod]
    public static bool setEBCUpload(string id, string user, string path, string uploadType)
    {
      if (CheckSession())
      {
        try
        {
          if (uploadType == "proof")
          {
            path = id + ConfigurationManager.AppSettings["BallotFileName"];

            BallotTracker.setDatabaseRecords("UPDATE ED_Ballot_Track SET proofUploadDT=GETDATE(), proofUploadUser = '" + user + "', proofPath = '" + path + "', ballotPath='y', ballotRejectDT=NULL, ballotRejectUser=NULL WHERE ed='" + id + "' AND archive <> 1");
          }
          else if (uploadType == "mail")
          {
            BallotTracker.setDatabaseRecords("UPDATE ED_Ballot_Track SET mailBallotUploadDT=GETDATE(), mailBallotUploadUser = '" + user + "', mailBallotPath = '" + path + "', mailBallotRejectDT=NULL, mailBallotRejectUser=NULL WHERE ed='" + id + "' AND archive <> 1");
          }
          else
          {
            BallotTracker.setDatabaseRecords("UPDATE ED_Ballot_Track SET xlsUpDT=GETDATE(), xlsUpUser = '" + user + "', xlsPath = '" + path + "' WHERE ed='" + id + "' AND archive <> 1");
          }
        }
        catch
        {
          return false;
        }
      }
      return true;
    }

    private static void SendEmail(string ed, string userID, string emailType)
    {
      DataTable databaseRecords1 = BallotTracker.getDatabaseRecords("SELECT * FROM ED_Ballot_Track WHERE ed='" + ed + "' AND archive <> 1");
      DataTable databaseRecords2 = BallotTracker.getDatabaseRecords("SELECT * FROM ED_Info WHERE ed='" + ed + "' AND archive <> 1");
      DataTable userRecords = BallotTracker.getDatabaseRecords("SELECT * FROM userACM WHERE id=" + userID);
      string appSetting = ConfigurationManager.AppSettings["emailTo"];
      MailDefinition mailDefinition = new MailDefinition();
      switch (emailType)
      {
        case "deo":
          mailDefinition.BodyFileName = "mail-template-deo.html";
          break;
        case "printer":
          mailDefinition.BodyFileName = "mail-template.html";
          break;
        case "rejected":
          mailDefinition.BodyFileName = "mail-template-rejected.html";
          break;
        case "mail_ballot_rejected":
          mailDefinition.BodyFileName = "mail-template-mail-in-rejected.html";
          break;
        default:
          mailDefinition.BodyFileName = "mail-template.html";
          break;
      }

      mailDefinition.From = ConfigurationManager.AppSettings["emailFrom"];
      ListDictionary replacements = new ListDictionary();
      DateTime today = DateTime.Today;
      replacements.Add((object)"<%ed-name%>", (object)databaseRecords2.Rows[0]["edName"]);
      replacements.Add((object)"<%ed-code%>", (object)databaseRecords2.Rows[0]["ed"]);
      replacements.Add((object)"<%submission-date%>", (object)today.ToString("D"));
      replacements.Add((object)"<%updating-user%>", (object)userRecords.Rows[0]["displayName"]);
      replacements.Add((object)"<%ballot-site%>", (object)ConfigurationManager.AppSettings["BallotSite"]);
      replacements.Add((object)"<%ftp-site%>", (object)ConfigurationManager.AppSettings["FtpSite"]);
      MailMessage mailMessage = mailDefinition.CreateMailMessage(appSetting, (IDictionary)replacements, (Control)new LiteralControl());
      mailMessage.IsBodyHtml = true;

      switch (emailType)
      {
        case "deo":
          DataTable deoEmails = getDeoEmails(ed);
          foreach (DataRow row in deoEmails.Rows)
          {
            if (!string.IsNullOrWhiteSpace(Convert.ToString(row["userEmail"])))
            {
              mailMessage.To.Add(new MailAddress(Convert.ToString(row["userEmail"])));
            }
          }
          mailMessage.Subject = "Ballot Proof Approved for " + ed;
          break;
        case "printer":
          string printerEmail = Convert.ToString(databaseRecords2.Rows[0]["printerEmail"]);
          if (!string.IsNullOrEmpty(printerEmail))
          {
            mailMessage.To.Add(new MailAddress(printerEmail));
          }
          mailMessage.Subject = "Ballot Proof Approved for " + ed;
          break;
        case "rejected":
          mailMessage.Subject = "Ballot Proof Rejected for " + ed;
          foreach (string emailAddress in ConfigurationManager.AppSettings["emailBallotRejected"].ToString().Split(','))
          {
            if (!string.IsNullOrEmpty(emailAddress))
            {
              mailMessage.To.Add(new MailAddress(emailAddress));
            }
          }
          break;
        case "mail_ballot_rejected":
          mailMessage.Subject = "Mail-In Ballot Rejected for " + ed;
          foreach (string emailAddress in ConfigurationManager.AppSettings["emailBallotRejected"].ToString().Split(','))
          {
            if (!string.IsNullOrEmpty(emailAddress))
            {
              mailMessage.To.Add(new MailAddress(emailAddress));
            }
          }
          break;
        default:
          break;
      }

      new SmtpClient(ConfigurationManager.AppSettings["smtpserver"].ToString()).Send(mailMessage);
    }

    protected void Timer1_Tick(object sender, EventArgs e) => this.Response.Redirect("ballot-tracker.aspx");

    protected void btLogoutClick(object sender, EventArgs e)
    {
      this.Session.Remove("authUserName");
      this.Session.Remove("authUserID");
      this.Session.Remove("authDEOUserED");
      this.Response.Redirect("ballot-tracker.aspx");
    }

    private static string getUserEDName(string EDName) => Convert.ToString(BallotTracker.getDatabaseRecords("SELECT * FROM ED_Info where ed='" + EDName + "' AND archive <> 1").Rows[0]["edName"]).Trim();

    public void RegisterDOMReadyScript(string key, string script)
    {
      string script1 = this.EncloseOnDOMReadyEvent(script);
      System.Web.UI.ScriptManager.RegisterClientScriptBlock((Page)this, this.GetType(), key, script1, true);
    }

    private string EncloseOnDOMReadyEvent(string str)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("function r(f){/in/.test(document.readyState)?setTimeout('r('+f+')',9):f()} r(function(){").Append(str).Append("});");
      return stringBuilder.ToString();
    }

    private static DataTable getDeoEmails(string ed)
    {
      SqlConnection selectConnection = new SqlConnection(ConfigurationManager.AppSettings["BallotsConnectionString"]);
      SqlCommand cmd = new SqlCommand();
      SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
      DataTable dataTable = new DataTable();
      try
      {
        cmd = new SqlCommand("GetDEOEmailAddresses", selectConnection);
        cmd.Parameters.Add(new SqlParameter("@ed", ed));
        cmd.CommandType = CommandType.StoredProcedure;
        sqlDataAdapter.SelectCommand = cmd;
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

    private static bool CheckSession()
    {
      return System.Web.HttpContext.Current.Session["authUserName"] != null;
    }

     protected void btnUploadToServer_Click(object sender, EventArgs e)
    {
      BulkUploadHttpPostedFiles("proof");
    }

    private void BulkUploadHttpPostedFiles(string uploadType)
    {
      FileUpload fileUploadControl = null;
      if (uploadType == "mail")
      {
        fileUploadControl = MailInBallotFileUploadControl;
      }
      else
      {
        fileUploadControl = BallotFileUploadControl;
      }

      if (fileUploadControl.HasFiles)
      {
        using (WinSCP.Session session = new WinSCP.Session())
        {

          string remotePath = System.Configuration.ConfigurationManager.AppSettings["RemotePath"];
          session.Open(GetWinSCPSessionOptions());
          
          foreach (HttpPostedFile postfile in fileUploadControl.PostedFiles)
          {
            Stream inputStream = postfile.InputStream;
            string ed = "";
            string fileName = "";
            string uploadFolder = "";
            if (uploadType == "mail")
            {
              ed = postfile.FileName.Replace("_Mail.pdf", "");
              uploadFolder = System.Configuration.ConfigurationManager.AppSettings["MailBallotFolder"];
              fileName = ed + ConfigurationManager.AppSettings["MailBallotFileName"];
            }
            else
            {
              ed = postfile.FileName.Replace(".pdf", "");
              fileName = ed + ConfigurationManager.AppSettings["BallotFileName"];
              uploadFolder = "Region" + GetRegion(ed);
            }

            if (AllowUpload(ed, uploadType))
            {
              setEBCUpload(ed, this.Session["authUserID"].ToString(), fileName, uploadType);

              // Use PutFile to upload
              session.PutFile(inputStream, remotePath + "/" + uploadFolder + "/" + fileName);
            }
          }
        }
        Response.Redirect(Request.RawUrl);
      }
    }

    private SessionOptions GetWinSCPSessionOptions()
    {
      string sql = "SELECT sftp_host_name,winscp_user_name,ssh_host_key_fingerprint,private_key_path,private_key_passphrase FROM global_config";
      DataTable databaseRecords = BallotTracker.getDatabaseRecords(sql);

      SessionOptions sessionOptions = new SessionOptions
        {
          Protocol = Protocol.Sftp,
          HostName = (string)databaseRecords.Rows[0]["sftp_host_name"], 
          UserName = (string)databaseRecords.Rows[0]["winscp_user_name"],
          SshHostKeyFingerprint = (string)databaseRecords.Rows[0]["ssh_host_key_fingerprint"],
          SshPrivateKeyPath = (string)databaseRecords.Rows[0]["private_key_path"],
          PrivateKeyPassphrase = (string)databaseRecords.Rows[0]["private_key_passphrase"]
      };

      return sessionOptions;
    }

    private void GetBallotFile(string ed)
    {
      using (WinSCP.Session session = new WinSCP.Session())
      {
        session.Open(GetWinSCPSessionOptions());
        string remotePath = System.Configuration.ConfigurationManager.AppSettings["RemotePath"];

        string filename = ed + ConfigurationManager.AppSettings["BallotFileName"]; 
        string uploadFolder = "Region" + GetRegion(ed);
        TransferOptions transferOptions = new TransferOptions();
        transferOptions.TransferMode = TransferMode.Binary; // Use binary for most files
                                                            // Download file to a memory stream for browser display

        // Get the file as a stream
        // Use 'using' to ensure the stream is disposed properly, freeing up the session
        using (Stream remoteFileStream = session.GetFile(remotePath + "/" + uploadFolder + "/" + filename))
        {
          // Example: Copy the stream to a MemoryStream or upload directly to Azure Blob
          using (MemoryStream memoryStream = new MemoryStream())
          {
            remoteFileStream.CopyTo(memoryStream);
            // Process the memoryStream (e.g., memoryStream.ToArray() )
            byte[] fileBytes = memoryStream.ToArray();
            Response.Clear();
            Response.ContentType = "application/pdf"; // Set correct MIME type
            Response.AddHeader("Content-Disposition", "inline; filename=" + filename);
            Response.BinaryWrite(fileBytes);
            Response.End();
          }
        }
      }
    }

    private bool AllowUpload(string ed, string uploadType)
    {
      //when bulk uploading ballot proof files, need to only allow uploading files for electoral districts which are in the "With DVS" tab
      string sql = "SELECT ebt.ed FROM ED_Ballot_Track ebt WHERE ebt.archive <> 1 AND ebt.xlsPath IS NOT NULL AND (ebt.proofPath IS NULL OR ebt.ballotRejectDT IS NOT NULL) AND ebt.ed = '" + ed + "'";
      if (uploadType == "mail")
      {
        sql = "SELECT ebt.ed FROM ED_Ballot_Track ebt WHERE ebt.archive <> 1 AND ebt.xlsPath IS NOT NULL AND (ebt.mailBallotPath IS NULL OR ebt.mailBallotRejectDT IS NOT NULL) AND ebt.ed = '" + ed + "'";
      }
      DataTable databaseRecords = BallotTracker.getDatabaseRecords(sql);
      return databaseRecords.Rows.Count > 0;
    }

    private string GetRegion(string ed)
    {
      string sql = "SELECT region FROM ED_Info WHERE ed = '" + ed + "' AND archive <> 1";
      DataTable databaseRecords = BallotTracker.getDatabaseRecords(sql);
      
      return databaseRecords.Rows.Count > 0 ? Convert.ToString(databaseRecords.Rows[0]["region"]) : "";
    }

    protected void MailInBallotUploadToServer_Click(object sender, EventArgs e)
    {
      BulkUploadHttpPostedFiles("mail");
    }
  }
}