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

            foreach (var uId in projectUsers)
            {
                var user = await userManager.FindByIdAsync(uId);
                if (user != null && users.Contains(user) == false)
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

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> AddIssue(AddNewIssueViewModel model)
        {
            if (ModelState.IsValid)
            {
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

                //var fileNames = new List<ScreenShots>();

                //if(Global.globalInitialScreenShots == true)
                //{
                //    fileNames = await UploadScreenShotsToStorage(issue.IssueId);
                //}

                //Global.InitialScreenShots = false;
                //_issueRepository.AddScreenShots(fileNames);
                return RedirectToAction("index", "home");
            }
            return View();
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

            //var maxFileSize = 3 * 1024 * 1024;

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
                    //else if (file.Length > maxFileSize)
                    //{

                    //    var filePaths = Directory.GetFiles(uploadsFolder).ToList();

                    //    if (filePaths.Count > 0)
                    //    {
                    //        foreach (var path in filePaths)
                    //        {
                    //            System.IO.File.Delete(path);
                    //        }
                    //    }

                    //    return Json(new { status = "fileTooLarge", message = "Please upload a smaller file" });
                    //}
                    else
                    {
                        var uniqueFileName = "";
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        //using (var stream = System.IO.File.Create(filePath))
                        //{
                        //    file.CopyTo(stream);

                        //    stream.Position = 0;



                        //}

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
                    var fileNameSplitLength = fileNameSplit.Length;

                    var placeholderFile = "JustHereToKeepTheFolderFromBeingDeleted.png";

                    if (fileNameSplit[fileNameSplitLength - 1] != placeholderFile)
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
