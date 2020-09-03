using System;
using System.Collections.Generic;
using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.ViewModels.Issues
{
    public class AddNewIssueViewModel
    {
        public Issue NewIssue { get; set; }
        public int ProjectId { get; set; }
        public List<IdentityUser> ProjectUsers { get; set; }
    }
}
