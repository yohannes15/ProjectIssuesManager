using System;
using System.Collections.Generic;
using System.Linq;
using IssueTracker.Models.CombModels;
using IssueTracker.Storage;

namespace IssueTracker.Models
{
    public class SQLIssueRepository : IIssueRepository
    {
        private readonly AppDbContext _context;
        private readonly IFirebaseStorage fireBaseStorage;

        public SQLIssueRepository(AppDbContext Context, IFirebaseStorage fireBaseStorage)
        {
            _context = Context;
            this.fireBaseStorage = fireBaseStorage;
        }
        
        public Issue AddIssue(Issue issue)
        {
            _context.Issues.Add(issue);
            _context.SaveChanges();
            return issue;
        }

        public Issue GetIssue(int id)
        {
            return _context.Issues.Find(id);
        }

        public Issue Update(Issue issueChanges)
        {
            var issue = _context.Issues.Attach(issueChanges);
            issue.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return issueChanges;
        }

        public Issue Delete(int id)
        {
            var issue = _context.Issues.Find(id);
            if (issue != null)
            {
                var issueScreenshots = _context.ScreenShots.Where(s => s.AssociatedIssue == id);
                foreach (var screenshot in issueScreenshots)
                {
                    fireBaseStorage.Delete(screenshot.FileName);
                    _context.ScreenShots.Remove(screenshot);
                }

                var issueComments = _context.Comments.Where(c => c.AssociatedIssueId == id);
                if (issueComments != null && issueComments.Count() > 0)
                {
                    _context.Comments.RemoveRange(issueComments);
                }

                var issueHistories = _context.IssueHistory.Where(i => i.AssociatedIssueId == id);
                if (issueHistories != null && issueHistories.Count() > 0)
                {
                    _context.IssueHistory.RemoveRange(issueHistories);
                }

                var projectIssue = _context.ProjectIssues.FirstOrDefault(i => i.IssueId == id);

                _context.ProjectIssues.Remove(projectIssue);
                _context.Remove(issue);
                _context.SaveChanges();
            }

            return issue;
        }

        public IssueHistory AddIssueHistory(IssueHistory changes)
        {
            _context.IssueHistory.Add(changes);
            _context.SaveChanges();
            return changes;
        }

        public List<Issue> GetAllAssigneeIssues(string userId)
        {
            return _context.Issues.Where(i => i.AssigneeUserId == userId).ToList();
        }

        public IEnumerable<Issue> GetAllProjectIssues(int projectId)
        {
            return _context.Issues.Where(i => i.AssociatedProject == projectId);
        }

        public List<IssueHistory> GetIssueHistories(int id)
        {
            return _context.IssueHistory.Where(i => i.AssociatedIssueId == id).ToList();
        }

        public List<ScreenShots> ScreenShots(int issue)
        {
            return _context.ScreenShots.Where(s => s.AssociatedIssue == issue).ToList();
        }

        public List<ScreenShots> AddScreenShots(List<ScreenShots> addedScreenShots)
        {
            _context.ScreenShots.AddRange(addedScreenShots);
            _context.SaveChanges();
            return addedScreenShots;
        }

        public ScreenShots DeleteScreenShots(int screenShotId)
        {
            var screenshot = _context.ScreenShots.Find(screenShotId);

            fireBaseStorage.Delete(screenshot.FileName);

            _context.Remove(screenshot);
            _context.SaveChanges();
            return screenshot;
        }

        public List<Comment> Comments(int issue)
        {
            return _context.Comments.Where(c => c.AssociatedIssueId == issue).ToList();
        }

        public Comment AddComment(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return comment;
        }

        public Comment DeleteComment(int id)
        {
            var comment = _context.Comments.Find(id);
            _context.Remove(comment);
            _context.SaveChanges();
            return comment;
        }

        public Comment UpdateComment(Comment updatedComment)
        {
            var comment = _context.Comments.Attach(updatedComment);
            comment.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return updatedComment;
        }
    }
}
