using System;
using System.Collections.Generic;
using IssueTracker.Models;

namespace IssueTracker.ViewModels.Home
{
    public class IssuesAndAssociatedProjects
    {
        public IEnumerable<Issue> MyIssues { get; set; }
        public IEnumerable<Project> AssociatedProjects { get; set; }

    }
}
