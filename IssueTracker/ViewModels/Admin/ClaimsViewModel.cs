using System;
using System.Collections.Generic;
using IssueTracker.Models;

namespace IssueTracker.ViewModels.Admin
{
    public class ClaimsViewModel
    {
        public List<UserClaim> Claims { get; set; }
        public string UserId { get; set; }
        public int ProjectId { get; set; }

        public ClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }
    }
}
