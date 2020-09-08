using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using IssueTracker.Models;
using IssueTracker.Security;
using IssueTracker.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IssueTracker.Controllers
{
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly ILogger<AdminController> logger;
        private readonly IProjectRepository _projectRepository;

        public AdminController(RoleManager<IdentityRole> RoleManager,
            UserManager<IdentityUser> UserManager, ILogger<AdminController> Logger,
            IProjectRepository projectRepository
            )
        {
            this.roleManager = RoleManager;
            this.userManager = UserManager;
            this.logger = Logger;
            this._projectRepository = projectRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers(int projectId)
        {
            Global.ProjectId = projectId;
            var project = _projectRepository.GetProject(projectId);
            Global.Project = project;

            var users = new List<IdentityUser>();

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var IsDeveloperLevel = ClaimsLevel.IsDeveloper(userClaims.ToList(), projectId);

            if (IsDeveloperLevel == false)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var projectUsers = new List<string>();
            projectUsers.Add(project.OwnerId);
            if (project.UsersAssigned != null)
            {
                projectUsers.AddRange(project.UsersAssigned.Split(" ").ToList());
            }

            foreach(var uId in projectUsers)
            {
                var user = await userManager.FindByIdAsync(uId);
                if (user != null && !users.Contains(user))
                {
                    users.Add(user);
                }
            }

            var viewModel = new ListUsersViewModel
            {
                OwnerId = project.OwnerId,
                Project = project,
                ProjectId = projectId,
                ProjectUsers = users
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddUserToProject(int projectId)
        {
            Global.ProjectId = projectId;
            var project = _projectRepository.GetProject(projectId);
            Global.Project = project;

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var IsDeveloperLevel = ClaimsLevel.IsDeveloper(userClaims.ToList(), projectId);

            if (IsDeveloperLevel == false)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new AddUserToProjectViewModel
            {
                ProjectId = projectId
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "DeveloperPolicy")]
        public async Task<IActionResult> AddUserToProject(int projectId, string users)
        {
            var usersToBeAdded = users.Split(" ").ToList();

            var project = _projectRepository.GetProject(projectId);

            var Id = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(Id);
            var claims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = claims.ToList();

            foreach(var userId in usersToBeAdded)
            {
                if(project.UsersAssigned == null)
                {
                    project.UsersAssigned = userId;
                    _projectRepository.Update(project);
                }
                else
                {
                    project.UsersAssigned += " " + userId;
                    _projectRepository.Update(project);
                }

                var user = await userManager.FindByIdAsync(userId);

                if (user!= null)
                {
                    var userClaims = await userManager.GetClaimsAsync(user);

                    if(userClaims.Count != 4)
                    {
                        var userClaimList = new List<Claim>();
                        foreach(var claim in ClaimsPile.AllClaims)
                        {
                            if(claim.Type == "User Role")
                            {
                                userClaimList.Add(new Claim(claim.Type, projectId.ToString()));
                            }
                            else
                            {
                                userClaimList.Add(new Claim(claim.Type, String.Empty));
                            }
                        };


                        var result = await userManager.AddClaimsAsync(user, userClaimList);
                        
                    }

                    else if (userClaims.Count == 4) //this means that they have claims and have been added to a project
                    {
                        var result = await userManager.RemoveClaimsAsync(user, userClaims);

                        var userClaimList = new List<Claim>();
                        foreach(var claim in userClaims)
                        {
                            if (claim.Type == "User Role")
                            {
                                userClaimList.Add(new Claim(claim.Type, claim.Value + " " + projectId.ToString()));
                            }
                            else
                            {
                                userClaimList.Add(new Claim(claim.Type, claim.Value));
                            }
                        }

                        result = await userManager.AddClaimsAsync(user, userClaimList);

                    }

                }
            }

            return Json(new
            {
                status = "successfully added users to project"
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromProject(string userId, int projectId)
        {
            var uId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(uId);
            var claims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = claims.ToList();

            var project = _projectRepository.GetProject(projectId);

            var usersAssignedToProject = project.UsersAssigned.Split(" ").ToList();

            for (var i = 0; i<usersAssignedToProject.Count; i++)
            {
                if (usersAssignedToProject[i] == userManager.GetUserId(User))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                if (usersAssignedToProject[i] == userId)
                {
                    usersAssignedToProject.Remove(usersAssignedToProject[i]);
                    i--;
                }
               
            }

            project.UsersAssigned = String.Join(" ", usersAssignedToProject);
            project = _projectRepository.Update(project);

            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var userClaims = await userManager.GetClaimsAsync(user);
                var result = await userManager.RemoveClaimsAsync(user, userClaims);

                var userClaimList = new List<Claim>();

                foreach (var claim in userClaims)
                {
                    var added = false;
                    if(claim.Value != null)
                    {
                        var userProjects = claim.Value.Split(" ").ToList();

                        foreach(var userProject in userProjects)
                        {
                            if (userProject == projectId.ToString())
                            {
                                userProjects.Remove(userProject);
                                userClaimList.Add(new Claim(claim.Type, String.Join(" ", userProjects)));
                                added = true;
                                break;
                            }
                        }
                        if (added == false)
                        {
                            userClaimList.Add(new Claim(claim.Type, claim.Value));
                        }
                    }
                    else
                    {
                        userClaimList.Add(new Claim(claim.Type, null));
                    }
                }

                result = await userManager.AddClaimsAsync(user, userClaimList);


            }

            return Json(new
            {
                status = "successfully removed user from project",
                userDiv = "userDiv_" + userId

            });
        }

        [HttpPost]
        public async Task<IActionResult> FindUsers(string input, int projectId)
        {
            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            try
            {
                var allUsers = from u in userManager.Users select u;
                var users = new List<IdentityUser>();

                if (!String.IsNullOrEmpty(input))
                {
                    users = userManager.Users.Where(user => user.UserName.Contains(input)).ToList();
                }

                var unassignedUsers = new List<IdentityUser>();
                var project = _projectRepository.GetProject(projectId);
                var projectUsers = project.OwnerId + " " + project.UsersAssigned;

                foreach(var user in users)
                {
                    if (projectUsers.Contains(user.Id) == false)
                    {
                        unassignedUsers.Add(user);
                    }
                    
                }

                return Json(new
                    {
                    status = "sucessfully found people",
                    users = unassignedUsers
                    });

            }
            catch(Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }

            
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string userId, int projectId)
        {
            Global.ProjectId = projectId;

            var project = _projectRepository.GetProject(projectId);

            Global.Project = project;

            var uId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(uId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var IsManagerLevel = ClaimsLevel.IsManager(userClaims.ToList(), projectId);

            if (IsManagerLevel == false)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} doesn't exist";
                return View("NotFound");
            }

            var viewModel = new EditUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                ProjectId = projectId
            };

            var allUserClaims = await userManager.GetClaimsAsync(user);

            foreach (var claim in allUserClaims)
            {
                var projectList = claim.Value.Split(" ").ToList();
                var claimString = "";

                for (var i=0; i<projectList.Count; i++)
                {
                    if (projectList[i] == projectId.ToString())
                    {
                        claimString = claim.Type + " : true";
                        viewModel.Claims.Add(claimString + " --> " + project.ProjectName);
                        break;
                    }
                }
                if(claimString == "")
                {
                    claimString = claim.Type + " : false";
                    viewModel.Claims.Add(claimString);
                }
            }




            return View(viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} doesn't exist";
                return View("NotFound");
            }

            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", new { projectId = model.ProjectId });
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId, int projectId)
        {
            Global.ProjectId = projectId;
            var project = _projectRepository.GetProject(projectId);
            Global.Project = project;

            var uId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(uId);
            var uClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = uClaims.ToList();

            var user = await userManager.FindByIdAsync(userId);

            var IsManagerLevel = ClaimsLevel.IsManager(User.Claims.ToList(), projectId);

            if (IsManagerLevel == false)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            else if (userId == userManager.GetUserId(User) || userId == project.OwnerId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {userId} doesn't exist";
                return View("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);

            //foreach(var claim in existingUserClaims)
            //{
            //    Console.WriteLine("------");
            //    Console.WriteLine(claim.Type);
            //    Console.WriteLine(claim.Value);
            //}

            var viewModel = new ClaimsViewModel
            {
                UserId = userId,
                ProjectId = projectId
            };


            for (var i = 0; i<ClaimsPile.AllClaims.Count; i++)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = ClaimsPile.AllClaims[i].Type
                };

                var projectList = new List<string>();

                if (existingUserClaims.Count == 4)
                {
                    projectList = existingUserClaims[i].Value.Split(" ").ToList();
                }

                for (int j=0; j<projectList.Count; j++)
                {
                    if (projectList[j] == projectId.ToString())
                    {
                        userClaim.IsSelected = true;
                        break;
                    }
                }
                viewModel.Claims.Add(userClaim);
            }

            foreach (var claim in viewModel.Claims)
            {
                Console.WriteLine(claim.ClaimType);
                Console.WriteLine(claim.IsSelected);
            }

            return View(viewModel);

        }

        [HttpPost]
        [Authorize(Policy = "ManagerPolicy")]
        public async Task<IActionResult> ManageUserClaims(ClaimsViewModel model)
        {
            var uId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(uId);
            var uClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = uClaims.ToList();

            var user = await userManager.FindByIdAsync(model.UserId);
            var project = _projectRepository.GetProject(model.ProjectId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id {model.UserId} doesn't exist";
                return View("NotFound");
            }

            if (model.UserId == userManager.GetUserId(User) || model.UserId == project.OwnerId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);
            var currentClaims = existingUserClaims.ToList();
            var result = await userManager.RemoveClaimsAsync(user, existingUserClaims);

            if(result.Succeeded == false)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            var claimsList = new List<Claim>();

            for(var i=0; i<model.Claims.Count; i++)
            {
                var projectList = new List<string>();
                var projectListString = "";

                if (currentClaims.Count == 4)
                {
                    projectList = currentClaims[i].Value.Split(" ").ToList();
                    var currentClaimContainsId = ClaimsHelper.ContainsId(model.ProjectId.ToString(), projectList);
                    if (currentClaimContainsId && model.Claims[i].IsSelected)
                    {
                        projectListString = String.Join(" ", projectList.ToArray());
                    }
                    else if (!currentClaimContainsId && model.Claims[i].IsSelected)
                    {
                        projectList.Add(model.ProjectId.ToString());
                        projectListString = String.Join(" ", projectList.ToArray());
                    }
                    else if (currentClaimContainsId && model.Claims[i].IsSelected == false)
                    {
                        var newListWithRemovedId = ClaimsHelper.RemoveProjectId(model.ProjectId.ToString(), projectList);
                        projectListString = String.Join(" ", newListWithRemovedId.ToArray());
                    }
                    else if (!currentClaimContainsId && model.Claims[i].IsSelected == false)
                    {
                        projectListString = String.Join(" ", projectList.ToArray());
                    }
                }

                claimsList.Add(new Claim (model.Claims[i].ClaimType, projectListString));

            }

            result = await userManager.AddClaimsAsync(user, claimsList);

            if (result.Succeeded == false)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { userId = model.UserId, projectId = model.ProjectId });


        }





    }
}
