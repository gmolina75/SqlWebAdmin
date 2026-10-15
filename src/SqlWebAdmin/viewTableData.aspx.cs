using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using SqlAdmin;

public partial class viewTableData : System.Web.UI.Page
{
    private const string controlPrefix1 = "Haiyan_Du_True_";//Dynamically add database field, if it is primary key, set id =controlPrefix1
    private const string controlPrefix2 = "Haiyan_Du_False_";//else use controlPrefix2

    /// <summary>
    /// Maximum number of rows loaded into the grid. Viewing a table must never
    /// try to pull an unbounded result set into memory.
    /// </summary>
    private const int MaxRows = 200;

    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Page.IsPostBack)
        //{
        //    //this.SqlDataSource1.Update();
        //    //this.SqlDataSource1.DataBind();
        //}
    }
   /// <summary>
   /// Page Initial Event
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
    protected void Page_PreInit(object sender, EventArgs e)
    {       
        ShowTableData();
    }

    /// <summary>
    /// Bind table data to DataGridView
    /// </summary>
    public void ShowTableData()
    {
        this.curTable.Text = "";
        this.MessageLabel.Text = "";

        try
        {
            string tableName = Request["table"];
            if (String.IsNullOrEmpty(tableName))
            {
                Response.Redirect("error.aspx?error=1002");
                return;
            }

            SqlServer server = SqlServer.CurrentServer;
            server.Connect();

            SqlDatabase database = SqlDatabase.CurrentDatabase(server);

            SqlTable table = database.Tables[HttpContext.Current.Server.HtmlDecode(tableName)];

            // The null check has to happen before anything touches the table:
            // asking a missing table for its primary keys used to throw.
            if (table == null)
            {
                server.Disconnect();

                // Table doesn't exist - break out and go to error page
                Response.Redirect(String.Format("error.aspx?error={0}", 1002));
                return;
            }

            if (table.PrimaryKeys == null || table.PrimaryKeys.Length < 1)
            {
                table.AddIDColumn();
            }

            DataTable data = table.GetTableData(MaxRows);
            this.GridView_ViewTableData.DataSource = data;
            this.GridView_ViewTableData.DataBind();

            this.curTable.Text = Server.HtmlEncode("Table - " + table.Name);

            if (data.Rows.Count >= MaxRows)
            {
                this.MessageLabel.Text = String.Format(
                    "Showing the first {0} rows. Use the Query page to see everything.",
                    MaxRows);
            }

            server.Disconnect();
        }
        catch (Exception ex)
        {
            // Never render a stack trace to the browser: log it and show a
            // readable message instead.
            Context.Trace.Warn("viewTableData", "Could not load table data", ex);
            this.MessageLabel.Text = Server.HtmlEncode("Could not load the table: " + ex.Message);
        }
    }
    /// <summary>
    /// Edit data record button clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GridView_ViewTableData_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int selectedIndex=-1;
        Int32.TryParse(e.CommandArgument.ToString(), out selectedIndex);
        GridViewRow gr = this.GridView_ViewTableData.Rows[selectedIndex];
        SqlTable curTable = SqlTable.CurrentTable;
        LiteralControl tblHeader = new LiteralControl("<table>");
        this.dbFields.Controls.Add(tblHeader);
        for (int i = 0; i < curTable.Columns.Count; i++)
        {
            TableCell c = gr.Cells[i + 1];//the first cell is edit column
            SqlColumn sc = curTable.Columns[i];
            Label lblTitle = new Label();
            lblTitle.Text = sc.ColumnInformation.Name;
           // lblTitle.EnableViewState = true;
            TextBox txtContent = new TextBox();
            txtContent.Text = c.Text;
            txtContent.EnableViewState = true;
            if(sc.ColumnInformation.Key)
                txtContent.ID = controlPrefix1+sc.ColumnInformation.Name;//make the control to be unique.
            else
                txtContent.ID = controlPrefix2+sc.ColumnInformation.Name;//make the control to be unique.
            

            if (sc.ColumnInformation.Key || sc.ColumnInformation.Identity 
                || sc.ColumnInformation.ColumnDbType==DbType.Object
                || sc.ColumnInformation.ColumnDbType==DbType.Binary)
                txtContent.Enabled = false;
            LiteralControl rowPart1=new LiteralControl("<tr><td>");

            this.dbFields.Controls.Add(rowPart1);
            this.dbFields.Controls.Add(lblTitle);
            LiteralControl rowPart2 = new LiteralControl("</td><td>");
            this.dbFields.Controls.Add(rowPart2);
            this.dbFields.Controls.Add(txtContent);
            LiteralControl rowPart3 = new LiteralControl("</td></tr>");
            this.dbFields.Controls.Add(rowPart3);           

            
        }
        
        LiteralControl tblFooter = new LiteralControl("</table>");    
        this.dbFields.EnableViewState = true;
        this.dbFields.Controls.Add(tblFooter);
        this.dbFields.DataBind();
        if (selectedIndex > -1)
        {
            programmaticModalPopup.Show();
        }

       
    }
    /// <summary>
    /// Update Data Record
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            SqlTable curTable = SqlTable.CurrentTable;
            if (curTable == null)
            {
                Response.Redirect("error.aspx?error=1002");
                return;
            }

            List<string> setClauses = new List<string>();
            List<string> whereClauses = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            int parameterIndex = 0;

            foreach (Control c in this.dbFields.Controls)
            {
                if (c.ID == null || !(c.ID.StartsWith(controlPrefix1) || c.ID.StartsWith(controlPrefix2)))
                    continue; // not a field text box

                TextBox curBox = (TextBox)c;
                bool isKey = curBox.ID.StartsWith(controlPrefix1);
                string colName = isKey
                    ? curBox.ID.Substring(controlPrefix1.Length)
                    : curBox.ID.Substring(controlPrefix2.Length);

                // Values travel as parameters and identifiers are bracket quoted,
                // so a cell containing "' or a column named "x]" can no longer
                // change the shape of the statement.
                string parameterName = "@p" + parameterIndex++;
                parameters.Add(new SqlParameter(parameterName, curBox.Text.Trim()));

                if (curBox.Enabled) //not key field, and not object/binary data type
                {
                    setClauses.Add(SqlIdentifier.Quote(colName) + "=" + parameterName);
                }
                else if (isKey) //key field
                {
                    whereClauses.Add(SqlIdentifier.Quote(colName) + "=" + parameterName);
                }
            }

            if (setClauses.Count == 0 || whereClauses.Count == 0)
            {
                this.programmaticModalPopup.Hide();
                this.MessageLabel.Text =
                    "This table has no editable key column, so rows cannot be updated safely.";
                return;
            }

            string sqlCommand = String.Format(
                "UPDATE {0} SET {1} WHERE {2}",
                SqlIdentifier.QuoteQualified(curTable.Owner, curTable.Name),
                String.Join(", ", setClauses.ToArray()),
                String.Join(" AND ", whereClauses.ToArray()));

            SqlServer server = SqlServer.CurrentServer;
            server.Connect();
            SqlDatabase database = SqlDatabase.CurrentDatabase(server);
            server.ExecuteNonQuery(sqlCommand, database.Name, parameters.ToArray());
            server.Disconnect();

            this.programmaticModalPopup.Hide();

            // Reload so the grid reflects the change instead of showing stale data.
            ShowTableData();
            this.MessageLabel.Text = "Row updated.";
        }
        catch (Exception ex)
        {
            Context.Trace.Warn("viewTableData", "Could not update row", ex);
            this.programmaticModalPopup.Hide();
            this.MessageLabel.Text = Server.HtmlEncode("Could not update the row: " + ex.Message);
        }
    }
    
    /// <summary>
    /// Cancel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        this.programmaticModalPopup.Hide();
    }
}
