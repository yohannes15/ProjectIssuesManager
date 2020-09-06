using System;
using System.Collections.Generic;
using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.ViewModels.Admin
{
    public class ListUsersViewModel
    {
        public List<IdentityUser> ProjectUsers { get; set; }
        public Project Project { get; set; }
        public int ProjectId { get; set; }
        public string OwnerId { get; set; }
    }
}
