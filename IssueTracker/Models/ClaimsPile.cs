using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace IssueTracker.Models
{
    public class ClaimsPile
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("Admin Role", "Admin Role"),
            new Claim("Manager Role", "Manager Role"),
            new Claim("Developer Role", "Developer Role"),
            new Claim("User Role", "User Role")
        };
    }
}
