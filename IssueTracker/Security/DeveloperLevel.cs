using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace IssueTracker.Security
{
    public class DeveloperLevel : AuthorizationHandler<DeveloperClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeveloperClaimsRequirement requirement)
        {

            var DeveloperProjects = new List<int>();

            if(Global.globalCurrentUserClaims == null)
            {
                var DeveloperClaims = context.User.FindFirst(c => c.Type == "Developer Role");
                var ManagerClaims = context.User.FindFirst(c => c.Type == "Manager Role");
                var AdminClaims = context.User.FindFirst(c => c.Type == "Admin Role");

                var claimProjects = DeveloperClaims.Value.Split(" ").ToList();
                claimProjects.AddRange(ManagerClaims.Value.Split(" ").ToList());
                claimProjects.AddRange(AdminClaims.Value.Split(" ").ToList());

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        DeveloperProjects.Add(Convert.ToInt32(projectId);
                    }
                }

                if (DeveloperProjects.Contains(Global.ProjectId))
                {
                    context.Succeed(requirement);
                }
            }

            else
            {
                var DeveloperClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Developer Role");
                var ManagerClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Manager Role");
                var AdminClaims = Global.globalCurrentUserClaims.Find(c => c.Type == "Admin Role");

                var claimProjects = DeveloperClaims.Value.Split(" ").ToList();
                claimProjects.AddRange(ManagerClaims.Value.Split(" ").ToList());
                claimProjects.AddRange(AdminClaims.Value.Split(" ").ToList());

                foreach (var projectId in claimProjects)
                {
                    if (projectId.Length > 0)
                    {
                        DeveloperProjects.Add(Convert.ToInt32(projectId));
                    }
                }


                if (DeveloperProjects.Contains(GlobalVar.ProjectId))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
