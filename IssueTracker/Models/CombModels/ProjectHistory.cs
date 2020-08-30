using System;
namespace IssueTracker.Models.CombModels
{
    public class ProjectHistory
    {
        public int Id { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime DateModified { get; set; }
        public int AssociatedProjectId { get; set; }
    }
}
