 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Data.OleDb; 
using System.Text;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Outlook;
using Attachment = Microsoft.Office.Interop.Outlook.Attachment;
using System.Net;
using System.IO;
using Atlassian; 
using Atlassian.Jira;

namespace JiraConnections
{
    class Program
    {
        
        static void Main(string[] args)
        {
          // GetJiraData();

            List<Issue> issues = new List<Issue>();

            int totalIssues = 0, issuesCount = 0;

            Jira jira = Jira.CreateRestClient("https://hcljiraproject.atlassian.net/jira/software/c/projects/HJ/issues", "hclproject.jira@gmail.com", "yggQoGIxlPB6nmZmCQY8FD74");
            Atlassian.Jira.Remote.RemoteProject rp = new Atlassian.Jira.Remote.RemoteProject();
            rp.projectUrl = "https://hcljiraproject.atlassian.net/jira/software/c/projects/HJ/issues";
            rp.url = "https://hcljiraproject.atlassian.net/jira/software/c/projects/HJ/issues";
            rp.name = "Hcl-Jira";
            // string configString = Newtonsoft.Json.JsonConvert.SerializeObject(Project);

            //do
            //{
            Project pj = new Project(jira, rp);
            
            string configString = Newtonsoft.Json.JsonConvert.SerializeObject(pj.Name);
            //pj.Url = "https://hcljiraproject.atlassian.net/jira/software/c/projects/HJ/issues";
            var query = @"project = 'Hcl-Jira'";
            var issuesQueryResult =   jira.Issues.GetIssuesFromJqlAsync(configString, int.MaxValue, issuesCount).Result;

            

            issues.AddRange(issuesQueryResult);

            issuesCount += issuesQueryResult.Count();
            
            totalIssues = issuesQueryResult.TotalItems;

            //}
            //while (issuesCount < totalIssues);



            foreach (var issue in issues)
            {

            }
             
        }

        public static void GetJiraData()
        {
            ServiceLocator sl = new ServiceLocator();
            //  Jira jiraConn = new Jira(sl);
            Jira jiraConn = Jira.CreateRestClient("https://hcljiraproject.atlassian.net/jira/software/c/projects/HJ/issues", "hclproject.jira@gmail.com", "yggQoGIxlPB6nmZmCQY8FD74");
            string jqlString = PrepareJqlbyDates( );
            var jiraIssues = jiraConn.Issues.GetIssuesFromJqlAsync(jqlString, 999).Result;

            foreach (var issue in jiraIssues)
            {
                System.Console.WriteLine(issue.ToString() + " -- ");
            }
        }

        static string PrepareJqlbyDates( )
        {
            string jqlString = "project = Hcl-Jira'";
            return jqlString;
        } 
    }
}