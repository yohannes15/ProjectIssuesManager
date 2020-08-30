using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace IssueTracker.Security
{
    public class AdminLevel : AuthorizationHandler<AdminClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminClaimsRequirement requirement)
        {
            var AdminProjects = new List<int>();
            var things = context.User.Identity;


            if (Global.globalCurrentUserClaims == null)
            {
                var UserClaims = context.User.FindFirst(c => c.Type == "Admin Role");

                var claimProjects = UserClaims.Value.Split(" ").ToList();

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        AdminProjects.Add(Convert.ToInt32(projectId));
                    }
                }

                if (AdminProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }

            }
            else
            {
                var UserClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Admin Role");

                var claimProjects = UserClaims.Value.Split(" ").ToList();

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        AdminProjects.Add(Convert.ToInt32(projectId));
                    }
                }

                if (AdminProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;

        }

       
    }
}
