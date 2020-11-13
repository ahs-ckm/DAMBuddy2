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
			string description = (string)jsonissue["fields"]["description"];
			string html = $"<html><body><b>Description: </b>{description}</body></html>";
			return html;
		}

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string jsonTicket = "";
			string searchTicket = tbTicket.Text;

			jsonTicket = JiraService.GetJiraIssue(searchTicket);
			
			if (!String.IsNullOrEmpty(jsonTicket))
			{
				m_Ticket = searchTicket;
				m_TicketJSON = jsonTicket;
				webBrowser1.DocumentText = BuildHTML(jsonTicket);
			}


				/*
                 * 			JSONObject obj = new JSONObject("" + response.getBody());
			ji.setId(obj.getString("id"));

			ji.setKey(obj.getString("key"));

			JSONObject fieldsObj = obj.getJSONObject("fields");


			if (fieldsObj.isNull("summary") == false)
			{
				String summary = fieldsObj.getString("summary");
				ji.setSummary(summary);
			}


			if (fieldsObj.isNull("description") == false)
			{
				ji.setDescription(fieldsObj.getString("description"));
			}

			JSONObject creatorObj = fieldsObj.getJSONObject("creator");
			ji.setCreator(creatorObj.getString("name"));
			ji.setCreatoremail(creatorObj.getString("emailAddress"));


			if (fieldsObj.isNull("assignee") == false)
			{
				JSONObject assigneeObj = fieldsObj.getJSONObject("assignee");
				ji.setAssignee(assigneeObj.getString("name"));
				ji.setAssigneeemail(assigneeObj.getString("emailAddress"));
			}

			if (fieldsObj.isNull("status") == false)
			{
				JSONObject assigneeObj = fieldsObj.getJSONObject("status");
				ji.setStatus(assigneeObj.getString("name"));

			}

			//Percentage complete
			if (fieldsObj.isNull("customfield_12400") == false)
			{
				JSONObject percentageCompleteObj = fieldsObj.getJSONObject("customfield_12400");
				String percentageComplete = percentageCompleteObj.getString("value");
				ji.setPercentagecompleted(percentageComplete);
			}
			//targetStartDate
			if (fieldsObj.isNull("customfield_11903") == false)
			{
				String targetStartDate = fieldsObj.getString("customfield_11903");
				Date tsDate = Util.getDate(targetStartDate);
				ji.setTargetstartdate(tsDate);
			}
			//targetEndDate
			if (fieldsObj.isNull("customfield_11904") == false)
			{
				String targetEndDate = fieldsObj.getString("customfield_11904");
				//Log.log("Target Repository End Date STRING  = " + targetEndDate);
				Date teDate = Util.getDate(targetEndDate);
				ji.setTargetenddate(teDate);
			}

			//targetEpicEndDate
			if (fieldsObj.isNull("customfield_15716") == false)
			{
				String targetRepositoryEndDate = fieldsObj.getString("customfield_15716");
				Date treDate = Util.getDate(targetRepositoryEndDate);
				ji.setTargetrepositoryenddate(treDate);
			}

			//implementation notes
			if (fieldsObj.isNull("customfield_11920") == false)
			{
				String implementationNotes = fieldsObj.getString("customfield_11920");
				ji.setImplementationnotes(implementationNotes);
			}

			String url = jiraBaseUrl + "/browse/" + ji.getKey();
			ji.setUrl(url);

                 */
			
		}
    }
}
