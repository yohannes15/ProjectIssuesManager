//if (IsUserLevel == false) ==> NEEDS TO BE IMPLMENETED LATER
//{
//    return RedirectToAction("AccessDenied", "Account");
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using IssueTracker.Models;
using IssueTracker.Models.CombModels;
using IssueTracker.Security;
using IssueTracker.ViewModels.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ILogger _logger;
        private readonly IIssueRepository _issueRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<IdentityUser> userManager;


        public ProjectController(ILogger<ProjectController> logger,
            IProjectRepository projectRepository, IIssueRepository issueRepository,
            UserManager<IdentityUser> userManager
            )
        {
            _logger = logger;
            _issueRepository = issueRepository;
            _projectRepository = projectRepository;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult AddProject(string userId)
        {
            var viewModel = new AddProjectViewModel
            {
                UserId = userId
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(Project project)
        {
            if (ModelState.IsValid)
            {
                var userId = userManager.GetUserId(User);
                var currentUser = await userManager.FindByIdAsync(userId);
                var newProject = new Project();

                newProject.OwnerId = userId;
                newProject.OwnerUserName = currentUser.UserName;
                newProject.ProjectName = project.ProjectName;
                newProject.ProjectDescription = project.ProjectDescription;
                newProject.ProjectStatus = project.ProjectStatus;
                newProject.StartDate = project.StartDate;
                newProject.EndDate = project.EndDate;

                _projectRepository.Add(newProject);

                var claims = await userManager.GetClaimsAsync(currentUser);

                Console.WriteLine(claims.Count);
                var result = await userManager.RemoveClaimsAsync(currentUser, claims);

                var claimList = new List<Claim>();

                for(var i =0; i < ClaimsPile.AllClaims.Count; i++)
                {
                    if (claims.Count == 4 && claims[i].Value != null)
                    {
                        claimList.Add(new Claim(ClaimsPile.AllClaims[i].Type, claims[i].Value + " " + newProject.ProjectId.ToString()));
                    }
                    else
                    {
                        claimList.Add(new Claim(ClaimsPile.AllClaims[i].Type, newProject.ProjectId.ToString()));
                    }
                }

                foreach (var claim in claimList)
                {
                    Console.WriteLine(claim.Issuer);
                    Console.WriteLine(claim.Type);
                    Console.WriteLine(claim.Value);
                    Console.WriteLine("----------");
                }


                Global.globalCurrentUserClaims = claimList;

                result = await userManager.AddClaimsAsync(currentUser, claimList);

                Global.ProjectId = newProject.ProjectId;

                return RedirectToAction("ProjectDetails", new { projectId = newProject.ProjectId });
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult SetGlobal(int projectId)
        {
            Global.ProjectId = projectId;
            return RedirectToAction("ProjectDetails", new { projectId = projectId });
        }

        [HttpGet]
        [Authorize(Policy = "ManagerPolicy")]
        public async Task<IActionResult> ProjectDetails(int projectId)
        {
            var project = new Project();

            if(projectId > 0)
            {
                project = _projectRepository.GetProject(projectId);
                Global.ProjectId = projectId;
            }
            else
            {
                project = _projectRepository.GetProject(Global.ProjectId);
            }

            Global.Project = project;

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var claims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = claims.ToList();

            var IsUserLevel = ClaimsLevel.IsUser(claims.ToList(), projectId);

            //if (IsUserLevel == false)
            //{
            //    return RedirectToAction("AccessDenied", "Account");
            //}

            var projectIssues = _issueRepository.GetAllProjectIssues(projectId);
            var projectHistory = _projectRepository.GetAllProjectHistories(projectId);

            if (project != null)
            {
                Console.WriteLine(project.ProjectId);
            }

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                projectHistories = projectHistory,
                ProjectId = projectId,
                ProjectIssues = projectIssues
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProjectDetails(ProjectDetailsViewModel projectUpdates)
        {
            var originalProject = _projectRepository.GetProject(projectUpdates.Project.ProjectId);

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var claims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = claims.ToList();

            foreach(var property in originalProject.GetType().GetProperties())
            {
                var oldValue = "";
                var newValue = "";

                if (property.GetValue(projectUpdates.Project) != null)
                {
                    newValue = property.GetValue(projectUpdates.Project).ToString();
                }
                if (property.GetValue(originalProject) != null)
                {
                    oldValue = property.GetValue(originalProject).ToString();
                }

                if (oldValue != newValue)
                {
                    var changes = new ProjectHistory
                    {
                        AssociatedProjectId = originalProject.ProjectId,
                        Property = property.Name,
                        OldValue = oldValue,
                        NewValue = newValue,
                        DateModified = DateTime.Now
                    };
                    _projectRepository.AddHistory(changes);
                 }
            }

            var projectIssues = _issueRepository.GetAllProjectIssues(projectUpdates.Project.ProjectId);
            var updatedProject = _projectRepository.Update(projectUpdates.Project);
            var projectHistory = _projectRepository.GetAllProjectHistories(projectUpdates.Project.ProjectId);

            var viewModel = new ProjectDetailsViewModel
            {
                projectHistories = projectHistory,
                Project = updatedProject,
                ProjectIssues = projectIssues,
                ProjectId = Global.ProjectId
            };
            return View(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> ProjectIssues(int projectId)
        {
            Global.ProjectId = projectId;
            var project = _projectRepository.GetProject(projectId);
            Global.Project = project;

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var UserIsAdminLevel = ClaimsLevel.IsAdmin(userClaims.ToList(), projectId);

            if (UserIsAdminLevel == false)
            {
                RedirectToAction("AccessDenied", "Account");
            }

            var projectIssues = _issueRepository.GetAllProjectIssues(projectId);
            var viewModel = new ProjectDetailsViewModel
            {
                ProjectIssues = projectIssues,
                Project = project,
                ProjectId = projectId
            };

            return View(viewModel);

        }

        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var project = _projectRepository.Delete(projectId);
            return RedirectToAction("Index", "Home");
        }




    }
}
