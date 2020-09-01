using System;
using System.Collections.Generic;
using IssueTracker.Models;

namespace IssueTracker.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<Issue> MyIssues { get; set; }
        public List<Project> MyProjects { get; set; }
        public string UserId { get; set; }
    }
}
