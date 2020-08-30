using System;
using System.Collections.Generic;
using IssueTracker.Models.CombModels;

namespace IssueTracker.Models
{
    public interface IIssueRepository
    {
        Issue AddIssue(Issue issue);
        Issue Delete(int id);
        IEnumerable<Issue> GetAllProjectIssues(int projectId);
        List<Issue> GetAllAssigneeIssues(string userId);
        Issue GetIssue(int id);
        Issue Update(Issue issueChanges);
        List<ScreenShots> AddScreenShots(List<ScreenShots> addedScreenShots);
        List<ScreenShots> ScreenShots(int issue);
        ScreenShots DeleteScreenShots(int screenShotId);
        List<Comment> Comments(int issue);
        Comment AddComment(Comment comment);
        Comment DeleteComment(int id);
        Comment UpdateComment(Comment updatedComment);
        IssueHistory AddIssueHistory(IssueHistory changes);
        List<IssueHistory> GetIssueHistories(int id);
    }
}
