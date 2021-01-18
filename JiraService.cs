using java.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DAMBuddy2
{
	public class JiraIssue
	{

		public String id = "";
		public String key = "";
		public String status = "";
		public String summary = "";
		public String description = "";
		public String assignee = "";
		public String assigneeemail = "";
		public String creator = "";
		public String creatoremail = "";
		public String url = "";
		public String implementationnotes = "";
		public String percentagecompleted = "";
		public Date targetstartdate = null;
		public Date targetenddate = null;
		public Date targetrepositoryenddate = null;
	}


	public class JiraService
	{
		//TODO - move some to config, and we need jira user form.
		readonly static string JIRA_USER = "jonbeeby";
		readonly static string JIRA_PW = "Up2Tkj2PqxPCkt";
		readonly static string JIRA_BASE_URL = "http://wsatsapp01.healthy.bewell.ca:8080/";

		readonly public static String search = "/rest/api/2/search";
		readonly public static String jqlCKCMTickets = "jql=assignee=ckcmservice+order+by+duedate";

		/*private static CredentialCache GetCredential(string url)
		{
			//string url = @"https://telematicoprova.agenziadogane.it/TelematicoServiziDiUtilitaWeb/ServiziDiUtilitaAutServlet?UC=22&SC=1&ST=2";
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
			CredentialCache credentialCache = new CredentialCache();
			credentialCache.Add(new System.Uri(url), "Basic", new NetworkCredential(JIRA_USER, JIRA_PW));
			return credentialCache;
		}*/

		public static List<JiraIssue> getItems(String jql)
		{
			List<JiraIssue> jiraIssues = new List<JiraIssue>();
			String finalUrl = "";

			try
			{
				String u = JIRA_USER;
				String p = JIRA_PW;
				String jiraBaseUrl = JIRA_BASE_URL;

				finalUrl = jiraBaseUrl + search + "?" + jql;

				var client = new RestClient(JIRA_BASE_URL);
				client.Authenticator = new HttpBasicAuthenticator(JIRA_USER, JIRA_PW);

				var request = new RestRequest(search + "?" + jql, DataFormat.Json);

				var response = client.Get(request);


				//	HttpWebRequest request = (HttpWebRequest)WebRequest.Create(finalUrl);
				//request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				//request.Credentials = GetCredential(finalUrl);
				//request.PreAuthenticate = true;

				//using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				//using (Stream stream = response.GetResponseStream())
				//using (StreamReader reader = new StreamReader(stream))
				//	{
				//	string jsonStatus = reader.ReadToEnd();
				//CallbackScheduleState?.Invoke(jsonStatus);
				//	}

				string content = response.Content;


				/* replace with .net json parsing */

				/*JSONObject obj = new JSONObject("" + response.getBody());
				JSONArray issues = obj.getJSONArray("issues");

				for (int i = 0; i < issues.length(); i++)
				{
					JiraIssue jiraIssue = new JiraIssue();
					jiraIssues.add(jiraIssue);
					JSONObject issueObj = issues.getJSONObject(i);
					String id = issueObj.getString("id");
					jiraIssue.setId(id);

					JSONObject fieldsObj = issueObj.getJSONObject("fields");
					String summary = fieldsObj.getString("summary");
					jiraIssue.setSummary(summary);

					String description = fieldsObj.get("description").toString();
					jiraIssue.setDescription(description);

					JSONObject assObj = fieldsObj.getJSONObject("assignee");
					String assigneeName = assObj.get("name").toString();
					jiraIssue.setAssignee(assigneeName); ;

					JSONObject creatorObj = fieldsObj.getJSONObject("creator");
					String creatorName = assObj.get("name").toString();
					jiraIssue.setCreator(creatorName);
				}    	    


				//Once we have completed the first pass, let's try to get the keys now

				for (int i = 0; i < jiraIssues.size(); i++)
				{
					JiraIssue issue = jiraIssues.get(i);
					JiraIssue issueWithKey = JiraService.getJiraIssue(issue.getId());
					String key = issueWithKey.getKey();
					issue.setKey(key);
					//URl http://compplayer.crha-health.ab.ca:8080/browse/CKCMFK-269
					String url = jiraBaseUrl + "/browse/" + issue.getKey();
					issue.setUrl(url);
				}
				mes += " 18 ";
					*/


			}
			catch (Exception e)
			{
				System.Windows.Forms.MessageBox.Show(e.Message);
				//e.printStackTrace();
				//throw new Exception(mes);
			}



			return jiraIssues;
		}

		public static string GetJiraIssue(String issueIdorKey)
		{
			//JiraIssue ji = null;

			//String u = JIRA_USER;
			//String p = JIRA_PW;
			//String jiraBaseUrl = JIRA_BASE_URL;
			//String finalUrl = jiraBaseUrl + "/rest/api/2/issue/" + issueIdorKey;

			//			finalUrl = jiraBaseUrl + search + "?" + jql;
			var client = new RestClient(JIRA_BASE_URL);
			client.Authenticator = new HttpBasicAuthenticator(JIRA_USER, JIRA_PW);

			var request = new RestRequest("/rest/api/2/issue/" + issueIdorKey, DataFormat.Json);

			var response = client.Get(request);
			
			if ( response.StatusCode == HttpStatusCode.OK)
			{
				return response.Content;
			}

			return "";
			/*
			JiraIssue issue = System.Text.Json.JsonSerializer.Deserialize<JiraIssue>(response.Content);

			JObject jsonissue = JObject.Parse(response.Content);

			string description = (string)jsonissue["fields"]["description"];


			MessageBox.Show(description);
			/*

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(finalUrl);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.Credentials = GetCredential(finalUrl);
			request.PreAuthenticate = true;

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				string jsonStatus = reader.ReadToEnd();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					try
					{
						ji = new JiraIssue();

						/* convert to .net json */

			/*
			JSONObject obj = new JSONObject("" + response.getBody());
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

		}
		catch (Exception exc)
		{
			Console.WriteLine(exc.Message);
		}

	}
	//CallbackScheduleState?.Invoke(jsonStatus);
}


return ji;

}
			*/


		//	return null;

		}


	}
}


/*
public static void main(String[] args)
{

	//Documentation
	//https://docs.atlassian.com/jira/REST/7.1.9/
	try
	{

		Console.WriteLine("Creating a ticket");
		Date now = new Date();
		String jiraIssue = "CKCMFK-1308";
		Console.WriteLine("Looking for JiraIssue=" + jiraIssue);
		JiraIssue issue = JiraService.getJiraIssue(jiraIssue);
		Console.WriteLine("JIRAISSUE=" + issue);

	}
	catch (Exception exc)
	{
		exc.printStackTrace();
	}

	Console.WriteLine("done");




}
	*/




