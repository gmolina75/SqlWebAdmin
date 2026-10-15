<%@ Page Language="c#" Inherits="SqlWebAdmin.Login" CodeFile="Login.aspx.cs" %>

<%@ Register TagPrefix="Toolbar" TagName="HelpLogout" Src="Toolbars/HelpLogoutToolbar.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>Web Data Administrator - Login</title>
    <link rel="shortcut icon" href="favicon.ico">
    <link rel="stylesheet" type="text/css" href="admin.css">
</head>

<script language="javascript" src="Global.js"></script>

<body bottommargin="0" leftmargin="0" topmargin="0" rightmargin="0">
    <form id="WebForm1" method="post" runat="server">
    <table width="100%" height="100%" cellspacing="0" cellpadding="0" border="0">
        <!-- FIRST ROW: HEADER -->
        <tr>
            <td colspan="3" valign="bottom" align="left" width="100%" height="36" bgcolor="#c0c0c0">
                <table cellspacing="0" cellpadding="0" width="100%" border="0">
                    <tr>
                        <!--BEGIN ONE LINE-->
                        <td valign="bottom" width="308">
                            <img src="images/logo_top.gif" width="308" height="36" alt="" border="0">
                        </td>
                        <!--END ONE LINE-->
                        <td valign="bottom" align="right" width="100%">
                            <Toolbar:HelpLogout runat="server" ID="HelpLogout" HelpTopic="login"></Toolbar:HelpLogout>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <!-- FIRST ROW: HEADER -->
        <!-- SECOND ROW: CRUMBS -->
        <tr>
            <!--BEGIN ONE LINE-->
            <td align="left" bgcolor="#99ccff" background="images/blue_back.gif" width="172"
                height="26">
                <img src="images/logo_bottom.gif" width="172" height="26" alt="" border="0">
            </td>
            <!--END ONE LINE-->
            <td align="left" bgcolor="#99ccff" background="images/blue_back.gif" width="100%"
                height="26">
                <table width="100%" height="26" cellspacing="0" cellpadding="0" border="0" style="table-layout: fixed">
                    <tr>
                        <td width="12">
                            &nbsp;
                        </td>
                        <td valign="middle" align="left" width="100%" height="26">
                        </td>
                    </tr>
                </table>
            </td>
            <!--BEGIN ONE LINE-->
            <td align="left" bgcolor="#99ccff" width="12" height="26">
                <img src="images/blue_back_right.gif" width="12" height="26" alt="" border="0">
            </td>
            <!--END ONE LINE-->
        </tr>
        <!-- SECOND ROW: CRUMBS -->
        <!-- THIRD ROW: BOTTOM SECTION -->
        <tr>
            <!-- START NAVIGATION SECTION -->
            <td bgcolor="#6699ff" valign="top" align="center" width="172" height="100%">
            </td>
            <!-- END NAVIGATION SECTION -->
            <!-- START CONTENT SECTION -->
            <td valign="top" align="left">
                <table cellspacing="0" cellpadding="0" border="0" width="100%">
                    <tr>
                        <!--BEGIN ONE LINE-->
                        <td valign="bottom" colspan="2" height="8" width="100%">
                            <img src="images/spacer.gif" width="1" height="8" alt="" border="0">
                        </td>
                        <!--END ONE LINE-->
                    </tr>
                    <tr>
                        <!--BEGIN ONE LINE-->
                        <td align="left" width="12">
                            <img src="images/spacer.gif" width="12" height="1" alt="" border="0">
                        </td>
                        <!--END ONE LINE-->
                        <td align="left" class="databaseListItem" width="100%">
                            <!-- PAGE CONTENT: START -->
                            <asp:Label ID="LogoutInfoLabel" runat="server" Visible="False" CssClass="banner banner--success">You are now logged out.</asp:Label>
                            <asp:Label ID="LoginInfoLabel" runat="server" Visible="False" CssClass="banner"><strong>Welcome</strong> to the Web Data Administrator.</asp:Label>
                            <asp:Label ID="lblCredMsg" runat="server" CssClass="subtitle" Style="display: block; margin-bottom: 12px;">Sign in with your SQL Server credentials</asp:Label>

                            <div class="loginCard">
                                <div class="loginField">
                                    <asp:Label ID="lblUser" AssociatedControlID="UsernameTextBox" runat="server" CssClass="fieldLabel">Username</asp:Label>
                                    <asp:TextBox ID="UsernameTextBox" runat="server" Columns="35" autocomplete="username" autofocus="autofocus"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="UsernameRequiredFieldValidator" runat="server" ErrorMessage="Enter a user name"
                                        ControlToValidate="UsernameTextBox" Display="Dynamic" CssClass="fieldError"></asp:RequiredFieldValidator>
                                </div>
                                <div class="loginField">
                                    <asp:Label ID="lblPassword" AssociatedControlID="PasswordTextBox" runat="server" CssClass="fieldLabel">Password</asp:Label>
                                    <asp:TextBox ID="PasswordTextBox" runat="server" Columns="35" TextMode="Password" autocomplete="current-password"></asp:TextBox>
                                </div>
                                <div class="loginField">
                                    <asp:Label ID="lblServer" AssociatedControlID="ServerTextBox" runat="server" CssClass="fieldLabel">Server</asp:Label>
                                    <asp:TextBox ID="ServerTextBox" runat="server" Columns="35" autocomplete="off"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="ServerRequiredFieldValidator" runat="server" ErrorMessage="Enter a server name"
                                        ControlToValidate="ServerTextBox" Display="Dynamic" CssClass="fieldError"></asp:RequiredFieldValidator>
                                    <span class="fieldHint">For a local instance use <code>.\SQLEXPRESS</code></span>
                                </div>
                                <div class="loginField">
                                    <asp:Label ID="lblAuth" AssociatedControlID="AuthRadioButtonList" runat="server" CssClass="fieldLabel">Authentication method</asp:Label>
                                    <asp:RadioButtonList ID="AuthRadioButtonList" runat="server" AutoPostBack="True"
                                        OnSelectedIndexChanged="AuthRadioButtonList_SelectedIndexChanged">
                                        <asp:ListItem Value="sql" Selected="True">SQL Login</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                                <div class="loginActions">
                                    <asp:Button ID="LoginButton" CssClass="button" runat="server" Text="Sign in" OnClick="LoginButton_Click"></asp:Button>
                                </div>
                            </div>

                            <br>
                            <asp:Label ID="ErrorLabel" runat="server" Visible="False" CssClass="banner banner--error" role="alert"></asp:Label>
                            <!-- PAGE CONTENT: END -->
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <!-- THIRD ROW: BOTTOM SECTION -->
    </table>
    </form>
</body>
</html>
