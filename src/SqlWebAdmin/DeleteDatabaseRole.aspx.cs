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
	public partial class DeleteDatabaseRole : System.Web.UI.Page
	{
		protected void Yes_Click(object sender, EventArgs e)
		{
			SqlServer server = SqlServer.CurrentServer;
			server.Connect();

			SqlDatabase database = SqlDatabase.CurrentDatabase(server);

			if (Request["role"] == null || database.DatabaseRoles[Request["role"]] == null)
			{
				server.Disconnect();
				Response.Redirect(String.Format("error.aspx?error={0}", 1004));
				return;
			}

			database.DatabaseRoles[Request["role"]].Remove();

			server.Disconnect();

			Response.Redirect("DatabaseRoles.aspx?database=" + Server.UrlEncode(Request["database"]));
		}

		protected void No_Click(object sender, EventArgs e)
		{
			Response.Redirect("DatabaseRoles.aspx?database=" + Server.UrlEncode(Request["database"]));
		}

		protected override void OnLoad(EventArgs e)
		{
			RoleName.Text = Request["role"];
			base.OnLoad(e);
		}
	}
}
