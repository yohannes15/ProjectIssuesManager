using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace IssueTracker.Security
{
    public class ManagerLevel : AuthorizationHandler<ManagerClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManagerClaimsRequirement requirement)
        {
            var ManagerProjects = new List<int>();

            if (Global.globalCurrentUserClaims == null)
            {
                var ManagerClaims = context.User.FindFirst(c => c.Type == "Manager Role");
                var AdminClaims = context.User.FindFirst(c => c.Type == "Admin Role");

                var claimProjects = ManagerClaims.Value.Split(" ").ToList();
                claimProjects.AddRange(AdminClaims.Value.Split(" ").ToList());

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        ManagerProjects.Add(Convert.ToInt32(projectId));
                    }
                }


                if (ManagerProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                var ManagerClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Manager Role");
                var AdminClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Admin Role");

                var claimProjects = ManagerClaims.Value.Split(" ").ToList();
                claimProjects.AddRange(AdminClaims.Value.Split(" ").ToList());

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        ManagerProjects.Add(Convert.ToInt32(projectId));
                    }
                }


                if (ManagerProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
