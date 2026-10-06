using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using SqlAdmin;

namespace SqlWebAdmin
{
	public partial class CreateDatabaseRole : System.Web.UI.Page
	{
		protected void CreateRole_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				try
				{
					SqlServer server = SqlServer.CurrentServer;
					server.Connect();

					SqlDatabase database = SqlDatabase.CurrentDatabase(server);

					string roleName = RoleName.Text.Trim();
					bool applicationRole = RoleType.SelectedValue == "Application";
					string rolePassword = RolePassword.Text;

					if (applicationRole && rolePassword.Length == 0)
					{
						ErrorMessage.Text = "An application role requires a password.";
						server.Disconnect();
						return;
					}

					database.DatabaseRoles.Add(roleName, applicationRole, applicationRole ? rolePassword : null);

					server.Disconnect();

					Response.Redirect("DatabaseRoles.aspx?database=" + Server.UrlEncode(Request["database"]));
				}
				catch (Exception ex)
				{
					ErrorMessage.Text = Server.HtmlEncode(ex.Message).Replace("\n", "<br>");
				}
			}
		}
	}
}
