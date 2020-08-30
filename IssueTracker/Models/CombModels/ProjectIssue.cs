using System;
namespace IssueTracker.Models.CombModels
{
    public class ProjectIssue
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int IssueId { get; set; }
    }
}
