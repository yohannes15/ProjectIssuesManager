using System;
namespace IssueTracker.Models.CombModels
{
    public class Comment
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int AssociatedIssueId { get; set; }
        public string CommentBody { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
