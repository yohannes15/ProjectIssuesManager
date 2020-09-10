using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BugTrackerProject.Models;
using IssueTracker.Models;
using IssueTracker.Models.CombModels;
using IssueTracker.Security;
using IssueTracker.Storage;
using IssueTracker.ViewModels.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ImageMagick;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IssueTracker.Controllers
{
    public class IssueController : Controller
    {
        private readonly ILogger<IssueController> _logger;
        private readonly IIssueRepository _issueRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IFirebaseStorage firebaseStorage;

        public IssueController(ILogger<IssueController> logger,
            IIssueRepository issueRepository,
            IProjectRepository projectRepository,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment hostingEnvironment,
            IFirebaseStorage firebaseStorage
            )
        {
            _logger = logger;
            _issueRepository = issueRepository;
            _projectRepository = projectRepository;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
            this.firebaseStorage = firebaseStorage;
        }

        [HttpGet]
        public async Task<IActionResult> AddIssue(int projectId)
        { 
            Global.ProjectId = projectId;

            var initial = new Issue
            {
                AssociatedProject = projectId,
                DueDate = DateTime.Today
            };

            var project = _projectRepository.GetProject(projectId);

            Global.Project = project;

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var IsUserLevel = ClaimsLevel.IsUser(userClaims.ToList(), projectId);

            if (IsUserLevel == false)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = new List<IdentityUser>();
            var projectUsers = new List<String>();

            projectUsers.Add(project.OwnerId);

            if (project.UsersAssigned != null)
            {
                projectUsers.AddRange(project.UsersAssigned.Split(" ").ToList());
            }
            Console.WriteLine(projectUsers[0]);


            foreach (var uId in projectUsers)
            {
                var user = await userManager.FindByIdAsync(uId);
                Console.WriteLine($"user is {user}");
                if (user != null && !users.Contains(user))
                {
                    users.Add(user);
                }
            }

            var viewModel = new AddNewIssueViewModel
            {
                ProjectId = projectId,
                ProjectUsers = users,
                NewIssue = initial
            };

            foreach (var user in users)
            {
                Console.WriteLine("USEr EMAIL");
                Console.WriteLine(user.Email);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> AddIssue(AddNewIssueViewModel model)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("Testing");
                var userId = userManager.GetUserId(User);
                var currentUser = await userManager.FindByIdAsync(userId);
                var userClaims = await userManager.GetClaimsAsync(currentUser);

                Global.globalCurrentUserClaims = userClaims.ToList();

                var IsUserManagerLevel = ClaimsLevel.IsManager(userClaims.ToList(), model.NewIssue.AssociatedProject);

                if (IsUserManagerLevel && model.NewIssue.AssigneeUserId != null)
                {
                    var assignedUser = await userManager.FindByIdAsync(model.NewIssue.AssigneeUserId);
                    model.NewIssue.AssigneeUserName = assignedUser.UserName;
                }

                model.NewIssue.SubmitterId = userManager.GetUserId(User);
                model.NewIssue.SubmitterUserName = userManager.GetUserName(User);
                model.NewIssue.CreatedTime = DateTime.Now;

                var issue = _issueRepository.AddIssue(model.NewIssue);

                if (issue.Title == null)
                {
                    issue.Title = $"issue {issue.IssueId}";
                    _issueRepository.Update(issue);
                }

                var projectIssue = new ProjectIssue
                {
                    ProjectId = issue.AssociatedProject,
                    IssueId = issue.IssueId
                };
                _projectRepository.AddProjectIssues(projectIssue);
                Console.WriteLine("HERERERER");

                var fileNames = new List<ScreenShots>();

                if (Global.globalInitialScreenShots == true)
                {
                    Console.WriteLine("Globally");
                    fileNames = await UploadScreenShotsToStorage(issue.IssueId);
                }

                Global.InitialScreenShots = false;
                _issueRepository.AddScreenShots(fileNames);

                Console.WriteLine(_issueRepository.ScreenShots(issue.IssueId));



                return RedirectToAction("projectissues", "Project", new { projectId = model.NewIssue.AssociatedProject});
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> IssueDetails(int issueId)
        {
            var issue = _issueRepository.GetIssue(issueId);
            Global.ProjectId = issue.AssociatedProject;

            var project = _projectRepository.GetProject(issue.AssociatedProject);
            Global.Project = project;

            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var IsUserLevel = ClaimsLevel.IsUser(userClaims.ToList(), issue.AssociatedProject);

            if (IsUserLevel == false)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var screenshots = _issueRepository.ScreenShots(issueId);
            var projectName = project.ProjectName;
            var comments = _issueRepository.Comments(issueId);
            var issueHistory = _issueRepository.GetIssueHistories(issueId);
            issue.Comments = comments;

            var users = new List<IdentityUser>();
            var projectUsers = new List<string>();
            projectUsers.Add(project.OwnerId);

            if(project.UsersAssigned != null)
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

            var viewModel = new IssueDetailsViewModel
            {
                Issue = issue,
                IssueHistories = issueHistory,
                ProjectId = issue.AssociatedProject,
                CurrentUserName = User.Identity.Name,
                ProjectName = project.ProjectName,
                ProjectUsers = users,
                Source = screenshots,
                Updated = 0
            };

            return View(viewModel);


        }

        [HttpPost]
        public async Task<IActionResult> IssueDetails(IssueDetailsViewModel model)
        {
            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var IsManagerLevel = ClaimsLevel.IsManager(userClaims.ToList(), model.Issue.AssociatedProject);

            if (IsManagerLevel && model.Issue.AssigneeUserId != null)
            {
                var assignedUser = await userManager.FindByIdAsync(model.Issue.AssigneeUserId);
                model.Issue.AssigneeUserName = assignedUser.UserName;
            }

            var uniqueFileNames = new List<ScreenShots>();

            if(Global.InitialScreenShots == true)
            {
                uniqueFileNames = await UploadScreenShotsToStorage(model.Issue.IssueId);
            }

            Global.InitialScreenShots = false;
            _issueRepository.AddScreenShots(uniqueFileNames);


            var originalIssue = _issueRepository.GetIssue(model.Issue.IssueId);

            if(model.Issue.Title == null)
            {
                model.Issue.Title = originalIssue.Title;
            }

            var IsDeveloperLevel = ClaimsLevel.IsDeveloper(userClaims.ToList(), model.Issue.AssociatedProject);

            if (IsDeveloperLevel)
            {
                foreach(var property in originalIssue.GetType().GetProperties())
                {
                    if (property.Name == "AssigneeUserId")
                    {
                        continue;
                    }

                    var oldValue = "";
                    var newValue = "";

                    if (property.GetValue(model.Issue) != null)
                    {
                        newValue = property.GetValue(model.Issue).ToString();
                    }

                    if (property.GetValue(originalIssue) != null)
                    {
                        oldValue = property.GetValue(originalIssue).ToString();
                    }

                    if (newValue != oldValue)
                    {
                        var changes = new IssueHistory
                        {
                            AssociatedIssueId = originalIssue.IssueId,
                            DateModified = DateTime.Now,
                            NewValue = newValue,
                            OldValue = oldValue,
                            Property = property.Name
                        };

                        _issueRepository.AddIssueHistory(changes);
                    }

                }
            }

            var issue = new Issue();
            if (IsDeveloperLevel)
            {
                model.Issue.ScreenShots = uniqueFileNames;
                model.Issue.ScreenShots.AddRange(_issueRepository.ScreenShots(model.Issue.IssueId));
                issue = _issueRepository.Update(model.Issue);
            }
            else
            {
                issue = originalIssue;
                issue.ScreenShots = uniqueFileNames;
                issue.ScreenShots.AddRange(_issueRepository.ScreenShots(model.Issue.IssueId));
            }
            Console.WriteLine(issue.IssueId);
            var project = _projectRepository.GetProject(issue.AssociatedProject);
            var projectName = project.ProjectName;
            issue.Comments = _issueRepository.Comments(issue.IssueId);
            var issueHistory = _issueRepository.GetIssueHistories(issue.IssueId);

            var users = new List<IdentityUser>();
            var projectUsers = new List<string>();
            projectUsers.Add(project.OwnerId);

            if (project.UsersAssigned != null)
            {
                projectUsers.AddRange(project.UsersAssigned.Split(" ").ToList());
            }

            foreach (var uId in projectUsers)
            {
                var user = await userManager.FindByIdAsync(uId);
                if (user != null && !users.Contains(user))
                {
                    users.Add(user);
                }
            }

            var screenshots = _issueRepository.ScreenShots(model.Issue.IssueId);


            var viewModel = new IssueDetailsViewModel
            {
                Issue = issue,
                IssueHistories = issueHistory,
                Updated = 1,
                ProjectId = issue.AssociatedProject,
                Source = screenshots,
                ProjectUsers = users,
                ProjectName = projectName
            };

            return View(viewModel);
        }

        [Authorize(Policy="ManagerPolicy")]
        public async Task<IActionResult> DeleteIssue(int issueId)
        {
            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            var issue = _issueRepository.Delete(issueId);
            return RedirectToAction("ProjectIssues", "Project", new { projectId = issue.AssociatedProject});
        }

        [HttpPost]
        [Authorize(Policy="ManagerPolicy")]
        public async Task<IActionResult> DeleteScreenshot(int screenShotId)
        {
            var userId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(userId);
            var userClaims = await userManager.GetClaimsAsync(currentUser);

            Global.globalCurrentUserClaims = userClaims.ToList();

            try
            {
                _issueRepository.DeleteScreenShots(screenShotId);
                return Json(new { status = "success", message = "screenshot deleted" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> StoreInitialScreenShots(List<IFormFile> Attachments)
        {
            var currentUserId = userManager.GetUserId(HttpContext.User);
            var user = await userManager.FindByIdAsync(currentUserId);
            var claims = await userManager.GetClaimsAsync(user);

            Global.globalCurrentUserClaims = claims.ToList();

            var extensions = new List<string>() { ".tiff", ".pjp", ".pjpeg", ".jfif", ".tif",
            ".svg", ".bmp", ".png", ".jpeg", ".svgz", ".jpg", ".webp", ".ico", ".xbm", ".dib"};

            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "temporaryFileStorage");


            try
            {

                foreach (var file in Attachments)
                {
                    var extension = Path.GetExtension(file.FileName);

                    if (extensions.Contains(extension.ToLower()) == false)
                    {
                        var filePaths = Directory.GetFiles(uploadsFolder).ToList();

                        if (filePaths.Count > 0)
                        {
                            foreach (var path in filePaths)
                            {
                                System.IO.File.Delete(path);
                            }
                        }

                        return Json(new { status = "fileNotImage", message = "Please upload an image file" });


                    }

                    else
                    {
                        var uniqueFileName = "";
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);


                        using (var fileStream = file.OpenReadStream())
                        {
                            using (var image = new MagickImage(fileStream))
                            {
                                image.Quality = 50;
                                image.Write(filePath);
                            }
                        }
                    }
                }

                Global.InitialScreenShots = true;

                return Json(new { status = "success", message = "files temporarily stored" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }


        public async Task<List<ScreenShots>> UploadScreenShotsToStorage(int issueId)
        {
            Console.WriteLine("SCcuess");
            var currentUserId = userManager.GetUserId(HttpContext.User);
            var user = await userManager.FindByIdAsync(currentUserId);
            var claims = await userManager.GetClaimsAsync(user);

            Global.globalCurrentUserClaims = claims.ToList();

            List<ScreenShots> uniqueFileNames = new List<ScreenShots>();
            if (Global.InitialScreenShots == true)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "temporaryFileStorage");

                var filePaths = Directory.GetFiles(uploadsFolder).ToList();

                foreach (var file in filePaths)
                {

                    var fileNameSplit = file.Split("\\");
                    foreach(var element in fileNameSplit)
                    {
                        Console.WriteLine("--><---");
                        Console.WriteLine(element);
                        Console.WriteLine("--><---");
                    }
                    var fileNameSplitLength = fileNameSplit.Length;
                    Console.WriteLine(fileNameSplitLength);

                    Console.WriteLine("############");
                    Console.WriteLine(fileNameSplit[fileNameSplitLength - 1]);
                    Console.WriteLine("############");

                    var placeholderFile = "ToStopDeletion.png";

                    if (fileNameSplit[fileNameSplitLength - 1].ToLower().Contains(placeholderFile.ToLower()) == false)
                    {
                        using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {


                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileNameSplit[fileNameSplitLength - 1];

                            using (var ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                var downloadUrl = await firebaseStorage.Upload(fileBytes, uniqueFileName);
                                uniqueFileNames.Add(new ScreenShots
                                {
                                    Url = downloadUrl,
                                    AssociatedIssue = issueId,
                                    FileName = uniqueFileName
                                });

                            }
                        }

                        System.IO.File.Delete(file);

                    }

                }

            }

            return uniqueFileNames;
        }
    }
}
