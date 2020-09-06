using System;
using System.Collections.Generic;
using IssueTracker.Models;
using IssueTracker.Models.CombModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.ViewModels.Issues
{
    public class IssueDetailsViewModel
    {
        public Issue Issue { get; set; }
        public string CurrentUserName { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int Updated { get; set; }
        public List<ScreenShots> Source { get; set; }
        public List<IFormFile> ScreenShots { get; set; }
        public List<IssueHistory> IssueHistories { get; set; }
        public List<IdentityUser> ProjectUsers { get; set; }
    }
}
