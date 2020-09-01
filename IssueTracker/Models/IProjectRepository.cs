using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IssueTracker.Models.CombModels;

namespace IssueTracker.Models
{
    public interface IProjectRepository
    {
        Project Add(Project project);
        Project Delete(int id);
        Task<List<Project>> GetAllOwnedProjects(string userId);
        Project GetProject(int id);
        Project Update(Project updatedProject);
        ProjectIssue AddProjectIssues(ProjectIssue issue);
        ProjectHistory AddHistory(ProjectHistory historyEntry);
        List<ProjectHistory> GetAllProjectHistories(int projectId);
    }
}
