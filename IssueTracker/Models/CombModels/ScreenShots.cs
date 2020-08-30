using System;
namespace IssueTracker.Models.CombModels
{
    public class ScreenShots
    {
        public int Id { get; set; }
        public int AssociatedIssue { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }

    }
}
