<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ballot-tracker.aspx.cs" Inherits="Ballots.BallotTracker" %>

<!DOCTYPE html>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>Election Ballots Preparation and Delivery Tracking</title>

  <link rel="icon" type="image/png" href="assets/images/ballot-tracker-favicon.png" />

  <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
  <meta http-equiv="X-UA-Compatible" content="IE=edge" />

  <link type="text/css" href="assets/css/forms.css" rel="stylesheet" />

  <link type="text/css" href="assets/css/jquery-ui.css" rel="stylesheet" />
  <link type="text/css" href="assets/css/ballot-tracking.css" rel="stylesheet" />
  <link type="text/css" href="assets/css/tempChart.css" rel="stylesheet" />

  <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>

  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.5.0/css/font-awesome.min.css" />

  <script type="text/javascript" src="./assets/js/jquery-3.5.1.js"></script>
  <script type="text/javascript" src="./assets/js/jquery-ui.min.js"></script>

  <script src="https://cdn.rawgit.com/kimmobrunfeldt/progressbar.js/0.9.0/dist/progressbar.js"></script>

  <script type="text/javascript" src="./assets/js/jquery-validate/jquery.validate.min.js"></script>
  <script type="text/javascript" src="./assets/js/jquery-validate/additional-methods.min.js"></script>

  <script type="text/javascript" src="jquery.uploadifive.js"></script>
  <link rel="stylesheet" type="text/css" href="uploadifive.css" />

  <script type="text/javascript" src="ballot-tracking.js"></script>

    <script type="text/javascript">

        function triggerFileUpload() {
            // Get the client ID of the ASP.NET control
            var fileUpload = document.getElementById('<%= BallotFileUploadControl.ClientID %>');
            if (fileUpload) {
                fileUpload.click();
            }
        }

        function uploadBallotFiles(fileUpload) {
            if (fileUpload.value != '') {
                document.getElementById("<%=btnUploadToServer.ClientID %>").click();
            }
        }

        function triggerMailInBallotFileUpload() {
            // Get the client ID of the ASP.NET control
            var fileUpload = document.getElementById('<%= MailInBallotFileUploadControl.ClientID %>');
            if (fileUpload) {
                fileUpload.click();
            }
        }

        function uploadMailInBallotFiles(fileUpload) {
            if (fileUpload.value != '') {
                document.getElementById("<%=MailInBallotUploadToServer.ClientID %>").click();
            }
        }

        window.addEventListener("beforeunload", function (event) {
            document.getElementById('proof-iframe').src = "FileHandler.ashx?fileAction=delete&userid=" + document.getElementById("userid").value;
        });
    </script>

</head>
<body>
  <form id="ballotsForm" runat="server">

    <div id="header">
        <img src="assets/images/elections-bc-logo.gif" />
        <h1>Election Ballots Preparation and Delivery Tracking</h1>
        <div id="user">
            <span class="username">
            <asp:PlaceHolder ID="currUser" runat="server" />
            </span>
            <asp:Button ID="btLogout" runat="server" class="logout" Text="Log out" OnClick="btLogoutClick"></asp:Button>
            <asp:HiddenField runat="server" ID="userType" />
            <br />
        </div>
        <br />
        <br />
        <asp:FileUpload ID="BallotFileUploadControl" runat="server" AllowMultiple="true" style="display:none;"   />
        <asp:Button ID="btnCustomUpload" runat="server" class="audioProofUpload" Text="Batch Upload Ballot Proofs" OnClientClick="triggerFileUpload(); return false;" />
        <asp:Button ID="btnUploadToServer" runat="server" style="display:none;" OnClick="btnUploadToServer_Click" />
        &#160;&#160;
        <asp:FileUpload ID="MailInBallotFileUploadControl" runat="server" AllowMultiple="true" style="display:none;"   />
        <asp:Button ID="MailInBallotCustomUpload" runat="server" class="audioProofUpload" Text="Batch Upload Mail-In Ballots" OnClientClick="triggerMailInBallotFileUpload(); return false;" />
        <asp:Button ID="MailInBallotUploadToServer" runat="server" style="display:none;" OnClick="MailInBallotUploadToServer_Click" />
    </div>
    <div id="button_header">
      <div id="buttons">
          <br />
          &#160;&#160;&#160;&#160;
  
            
      </div>
    </div>
    <asp:Panel runat="server" ID="StatsAndTabsPanel">
      <div id="stats">
        <div class="progress" id="progress">
          <asp:PlaceHolder ID="percComplete" runat="server" />
        </div>
        <asp:PlaceHolder ID="ProgressStats" runat="server" />
      </div>

      <div id="main">
        <div id="tabs">
          <ul id="stepTabs">
            <li id="upload-tab">Waiting for <br />Extract Upload</li>
            <li id="qp-tab">With DVS</li>
            <li id="ebc-tab">EBC Proofing</li>
            <li id="printer-tab">Approved Ballot Files</li>
            <li id="all-tab">Overview</li>
          </ul>
          <asp:Button ID="AudioProofUpload" runat="server" class="audioProofUpload" Text="Upload Audio Files" OnClientClick="Javascript:window.open('https://drive.kp.gov.bc.ca/WebInterface/login.html', 'blank');"></asp:Button>
        </div>
      </div>
    </asp:Panel>

    <asp:Panel runat="server" ID="DeoViewPanel">
      <div id="deo_main" class="single-ed">
        <div id="deo-welcome">
          <p class="ed-stats">
            <span class="ed-name">
              <asp:PlaceHolder ID="currUserEDName" runat="server" />
            </span>
          </p>

          <div id="ed-progress-bar">
            <div id="current-progress" class="ninety"></div>
          </div>
        </div>
      </div>
    </asp:Panel>

    <div id="tracking-table">
      <asp:PlaceHolder ID="BallotsTable" runat="server" />
    </div>
    <!--   END tracking-table -->

    <div id="proof-dialog" title="Ballot Proof">
      <iframe id="proof-iframe" width="1280px" height="700px" scrolling="no" frameborder="0"></iframe>
      <input type="hidden" id="currED" value="" />
    </div>

    <div id="mail-ballot-dialog" title="Mail-in Ballot">
      <iframe id="mail-ballot-iframe" width="1280px" height="700px" scrolling="no" frameborder="0"></iframe>
      <input type="hidden" id="mailBallotED" value="" />
    </div>

    <div id="ballot-dialog" title="Approved Ballot">
      <iframe id="ballot-iframe" width="1280px" height="700px" scrolling="no" frameborder="0"></iframe>
      <input type="hidden" id="viewBallotED" value="" />
    </div>
  </form>
</body>

</html>
