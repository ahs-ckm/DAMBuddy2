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
		public string m_Prefix;

		public SetupTicketForm()
		{
			InitializeComponent();
		}

		private string BuildHTML( string json )
		{
			JObject jsonissue = JObject.Parse(json);
			string assignee = "" ;
			string email = "";

			try
			{
				assignee = (string)jsonissue["fields"]["assignee"]["displayName"];
			}
			catch { }
			try
			{ 
				email = (string)jsonissue["fields"]["assignee"]["emailAddress"];
			}
			catch { }


			string description = (string)jsonissue["fields"]["description"];
			string sTicketID = (string)jsonissue["key"];

			string html = $"<html><body style='font-family:verdana'><h2>{sTicketID}</h2><br><br>Assignee: </b>{assignee}<br><b>Description: </b>{description}</body></html>";
			return html;
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{

			
		}

		private void cbPrefix_SelectedIndexChanged(object sender, EventArgs e)
		{
			// save preference
//			MessageBox.Show("TODO: Save Preference");
		}

		private void SetupTicketForm_Load(object sender, EventArgs e)
		{
			btnOK.Enabled = false;
			cbPrefix.SelectedItem = m_Prefix;
//			cbPrefix.SelectedIndex = 1;
		}

		private void timerSearch_Tick(object sender, EventArgs e)
		{
			btnOK.Enabled = false;
			timerSearch.Enabled = false;
			string jsonTicket = "";
			string searchTicket = cbPrefix.SelectedItem + tbTicket.Text;

			try
			{
				Application.UseWaitCursor = true;
				jsonTicket = JiraService.GetJiraIssue(searchTicket);

			}
			finally
			{
				Application.UseWaitCursor = false;
			}

			if (!String.IsNullOrEmpty(jsonTicket))
			{
				m_Ticket = searchTicket;
				m_TicketJSON = jsonTicket;
				webBrowser1.DocumentText = BuildHTML(jsonTicket);
				btnOK.Enabled = true;
			}


		}

		private void tbTicket_TextChanged(object sender, EventArgs e)
		{
			timerSearch.Enabled = true;
		}

		private void tbTicket_KeyUp(object sender, KeyEventArgs e)
		{
			timerSearch.Enabled = false;
		}

		private void tbTicket_KeyDown(object sender, KeyEventArgs e)
		{
			timerSearch.Enabled = true;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			m_Prefix = cbPrefix.Text;
		}
	}
}
