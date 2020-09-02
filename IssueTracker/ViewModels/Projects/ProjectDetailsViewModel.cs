using System;
using System.Collections.Generic;
using IssueTracker.Models;
using IssueTracker.Models.CombModels;

namespace IssueTracker.ViewModels.Projects
{
    public class ProjectDetailsViewModel
    {
        public ProjectDetailsViewModel()
        {
            projectHistories = new List<ProjectHistory>();
        }

        public IEnumerable<Issue> ProjectIssues { get; set; }
        public List<ProjectHistory> projectHistories { get; set; }
        public Project Project { get; set; }
        public int ProjectId { get; set; }
    }
}
