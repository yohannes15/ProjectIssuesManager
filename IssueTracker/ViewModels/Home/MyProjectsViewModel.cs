using System;
using System.Collections.Generic;
using IssueTracker.Models;

namespace IssueTracker.ViewModels.Home
{
    public class MyProjectsViewModel
    {
        public List<Project> ProjectList { get; set; }
        public string UserId { get; set; }
    }
}
