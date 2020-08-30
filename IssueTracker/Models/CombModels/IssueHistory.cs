using System;
namespace IssueTracker.Models.CombModels
{
    public class IssueHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime DateModified { get; set; }
        public int AssociatedIssueId { get; set; }
    }
}
