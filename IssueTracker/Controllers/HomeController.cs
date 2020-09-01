﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using IssueTracker.Models;
using IssueTracker.Models.CombModels;
using IssueTracker.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIssueRepository _issueRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<IdentityUser> userManager;

        public HomeController(IIssueRepository issueRepository,
            IProjectRepository projectRepository,
            UserManager<IdentityUser> userManager)
        {
            _issueRepository = issueRepository;
            _projectRepository = projectRepository;
            this.userManager = userManager;
        }


        //[HttpGet]
        //[Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(HttpContext.User);
            
            var currentUser = await userManager.FindByIdAsync(userId);
            var currentUserClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = currentUserClaims.ToList();

            var viewModel = new HomeViewModel
            {
                MyIssues = _issueRepository.GetAllAssigneeIssues(userId),
                MyProjects = _projectRepository.GetAllOwnedProjects(userId).Result,
                UserId = userId
            };

            return View(viewModel);
        }

        public async Task<IActionResult> MyIssues()
        {
            var userId = userManager.GetUserId(HttpContext.User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var currentUserClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = currentUserClaims.ToList();

            var myIssues = _issueRepository.GetAllAssigneeIssues(userId);
            var projects = new List<Project>();

            foreach (var issue in myIssues)
            {
                var project = _projectRepository.GetProject(issue.AssociatedProject);
                projects.Add(project);
            }

            var viewModel = new IssuesAndAssociatedProjects
            {
                AssociatedProjects = projects,
                MyIssues = myIssues
            };

            return View(viewModel);

        }

        public async Task<IActionResult> MyProjects()
        {
            var userId = userManager.GetUserId(HttpContext.User);
            var userName = userManager.GetUserName(HttpContext.User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var currentUserClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = currentUserClaims.ToList();

            var projectList = _projectRepository.GetAllOwnedProjects(userId).Result;

            foreach(var project in projectList)
            {
                var projectIssueList = new List<ProjectIssue>();
                var projectIssues = _issueRepository.GetAllProjectIssues(project.ProjectId);

                foreach(var issue in projectIssues)
                {
                    var projectIssue = new ProjectIssue
                    {
                        IssueId = issue.IssueId,
                        ProjectId = issue.AssociatedProject
                    };
                    projectIssueList.Add(projectIssue);
                }
                project.ProjectIssues = projectIssueList;

            }

            return View(projectList);

        }

        public IActionResult Privacy()
        {
            return View();
        }


    }
}