using System;
using System.Collections.Generic;
using System.Linq;
using IssueTracker.Models.CombModels;
using IssueTracker.Storage;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Models
{
    public class SQLProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;
        private readonly IFirebaseStorage firebaseStorage;

        public SQLProjectRepository(AppDbContext dbContext, IFirebaseStorage firebaseStorage)
        {
            _context = dbContext;
            this.firebaseStorage = firebaseStorage;
        }
        public Project Add(Project project)
        {
            _context.Projects.Add(project);
            _context.SaveChanges();
            return project;
        }

        public ProjectHistory AddHistory(ProjectHistory historyEntry)
        {
            _context.ProjectHistory.Add(historyEntry);
            _context.SaveChanges();
            return historyEntry;
        }

        public ProjectIssue AddProjectIssues(ProjectIssue issue)
        {
            _context.ProjectIssues.Add(issue);
            _context.SaveChanges();
            return issue;
        }

        public Project Delete(int id)
        {
            var project = _context.Projects.Find(id);
            var projectIssues = _context.ProjectIssues.Where(p => p.ProjectId == id).ToList();

            foreach (var issue in projectIssues)
            {
                if (issue != null)
                {
                    var projectScreenshots = _context.ScreenShots.Where(s => s.AssociatedIssue == issue.IssueId);
                    foreach (var screenshot in projectScreenshots)
                    {
                        firebaseStorage.Delete(screenshot.FileName);
                        _context.ScreenShots.Remove(screenshot);
                    }

                    var issueToDelete = _context.Issues.Find(issue.Id);
                    _context.Issues.Remove(issueToDelete);
                    _context.ProjectIssues.Remove(issue);
                }
            }

            project.ProjectIssues = projectIssues;
            if (project != null)
            {
                var projectHistory = _context.ProjectHistory.Where(p => p.AssociatedProjectId == id);
                _context.ProjectHistory.RemoveRange(projectHistory);
                _context.Projects.Remove(project);
                _context.SaveChanges();
            }

            return project;

        }

        public List<Project> GetAllOwnedProjects(string userId)
        {
            throw new NotImplementedException();
        }

        public List<ProjectHistory> GetAllProjectHistories(int projectId)
        {
            return _context.ProjectHistory.Where(p => p.AssociatedProjectId == projectId).ToList();
        }

        public Project GetProject(int id)
        {
            return _context.Projects.AsNoTracking().First(p => p.ProjectId == id);
        }

        public Project Update(Project updatedProject)
        {
            var project = _context.Projects.Attach(updatedProject);
            project.State = EntityState.Modified;
            _context.SaveChanges();
            return updatedProject;
        }
    }
}
