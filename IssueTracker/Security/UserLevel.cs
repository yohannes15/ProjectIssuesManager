using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace IssueTracker.Security
{
    public class UserLevel : AuthorizationHandler<UserClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserClaimsRequirement requirement)
        {
            var UserProjects = new List<int>();

            if (Global.globalCurrentUserClaims == null)
            {
                var UserClaims = context.User.FindFirst(c => c.Type == "User Role");
                var DeveloperClaims = context.User.FindFirst(c => c.Type == "Developer Role");
                var ManagerClaims = context.User.FindFirst(c => c.Type == "Manager Role");
                var AdminClaims = context.User.FindFirst(c => c.Type == "Admin Role");

                var claimProjects = UserClaims.Value.Split(" ").ToList();
                claimProjects.AddRange(DeveloperClaims.Value.Split(" ").ToList());
                claimProjects.AddRange(ManagerClaims.Value.Split(" ").ToList());
                claimProjects.AddRange(AdminClaims.Value.Split(" ").ToList());

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        UserProjects.Add(Convert.ToInt32(projectId));
                    }
                }


                if (UserProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }

            }
            else
            {
                var UserClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "User Role");
                var DeveloperClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Developer Role");
                var ManagerClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Manager Role");
                var AdminClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Admin Role");


                var claimProjects = UserClaims.Value.Split(" ").ToList();
                claimProjects.AddRange(DeveloperClaims.Value.Split(" ").ToList());
                claimProjects.AddRange(ManagerClaims.Value.Split(" ").ToList());
                claimProjects.AddRange(AdminClaims.Value.Split(" ").ToList());

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        UserProjects.Add(Convert.ToInt32(projectId));
                    }
                }


                if (UserProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
