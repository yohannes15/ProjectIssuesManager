using System;
using IssueTracker.Models;

namespace IssueTracker.ViewModels.Projects
{
    public class AddProjectViewModel
    {
        public string UserId { get; set; }
        public Project Project { get; set; }
    }
}
