using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace DAMBuddy2
{
	public partial class SetupTicketForm : Form
	{
		public string m_Ticket;
		public string m_TicketJSON;

		public SetupTicketForm()
		{
			InitializeComponent();
		}

		private string BuildHTML( string json )
		{
			JObject jsonissue = JObject.Parse(json);

			string assignee = (string)jsonissue["fields"]["assignee"]["displayName"];
			string description = (string)jsonissue["fields"]["description"];
			string email = (string)jsonissue["fields"]["assignee"]["emailAddress"];
			string sTicketID = (string)jsonissue["key"];

			string html = $"<html><body style='font-family:verdana'><h2>{sTicketID}</h2><br><br>Assignee: </b>{assignee}<br><b>Description: </b>{description}</body></html>";
			return html;
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			string jsonTicket = "";
			string searchTicket = cbPrefix.SelectedItem + tbTicket.Text;


			jsonTicket = JiraService.GetJiraIssue(searchTicket);
			
			if (!String.IsNullOrEmpty(jsonTicket))
			{
				m_Ticket = searchTicket;
				m_TicketJSON = jsonTicket;
				webBrowser1.DocumentText = BuildHTML(jsonTicket);
			}

			
		}

		private void cbPrefix_SelectedIndexChanged(object sender, EventArgs e)
		{
			// save preference
//			MessageBox.Show("TODO: Save Preference");
		}

		private void SetupTicketForm_Load(object sender, EventArgs e)
		{
			cbPrefix.SelectedItem = "CSDFK-";
//			cbPrefix.SelectedIndex = 1;
		}
	}
}
