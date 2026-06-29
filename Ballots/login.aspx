<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="Ballots.BallotLogin" %>

<html lang="en" xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title>Election Ballots Preparation and Delivery Tracking</title>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
		<meta http-equiv="X-UA-Compatible" content="IE=edge">

        <link type="text/css" href="assets/css/ballot-tracking.css" rel="stylesheet" /> 
        <link type="text/css" href="assets/css/jquery-ui.css" rel="stylesheet">
		
    	<%--<script type="text/javascript" src="./assets/js/jquery-1.11.3.js"></script>--%>
        <script type="text/javascript" src="./assets/js/jquery-3.5.1.js"></script>
        <script type="text/javascript" src="./assets/js/form-script-2.js"></script>
        <script type="text/javascript" src="./assets/js/jquery-ui.min.js"></script>

        <script type="text/javascript" src="./assets/js/jquery-validate/jquery.validate.min.js"></script>
        <script type="text/javascript" src="./assets/js/jquery-validate/additional-methods.min.js"></script>

        <link rel="stylesheet" type="text/css" href="./assets/css/jquery.realperson.css"> 
        <script type="text/javascript" src="./assets/js/jquery-realperson/jquery.plugin.min.js"></script> 
        <script type="text/javascript" src="./assets/js/jquery-realperson/jquery.realperson.min.js"></script>

<!--Resources required for uploadifive file upload controls.  Part of the brokerage Submit Print web application (form around line 1400).  NOTE: jquery also required but loaded previously-->
<script type="text/javascript" src="jquery.uploadifive.js"></script>
<link rel="stylesheet" type="text/css" href="uploadifive.css">
<!--end of Submit Print controls-->
</head>
<body class="login-page">
    <div id="header">
    <img src="assets/images/elections-bc-logo.gif">
    <h1>Election Ballots Preparation and Delivery Tracking</h1>

    <form id="loginUser" runat="server">
        <label>Username</label>
         <asp:TextBox id="userNameBox" runat="server"></asp:TextBox>
        <label>Password</label>
        <asp:TextBox id="userPassBox" TextMode="Password" runat="server"></asp:TextBox>

        <asp:Button id="btLogin" runat="server" class="btn btn-login" Text="Log in" OnClick="btLoginClick" ></asp:Button>
 
     </form>        
    </div>
    
    <span style="color:red;font-weight:bold">
        <asp:PlaceHolder id="logBox" runat="server" />
    </span>
    <br />
    <span style="color:red;font-weight:bold">
    <asp:PlaceHolder id="errorLogBox" runat="server" />
</span>
</body>
</html>