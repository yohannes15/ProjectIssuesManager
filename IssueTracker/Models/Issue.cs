using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Models.CombModels;
using IssueTracker.Models.Enums;

namespace IssueTracker.Models
{
    public class Issue
    {
        [Key]
        public int IssueId { get; set; }
        public int AssociatedProject { get; set; }
        public string SubmitterId { get; set; } //Dif
        public string SubmitterUserName { get; set; } //Dif
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; } //Dif
        public DateTime DueDate { get; set; }
        public IssueStatus IssueStatus { get; set; }
        public string AssigneeUserId { get; set; }
        public string AssigneeUserName { get; set; }
        //public string UsersAssigned { get; set; } after authetnication
        public IssuePriority IssuePriority { get; set; }
        public Reproducible Reproducible { get; set; }
        public List<ScreenShots> ScreenShots { get; set; }
        public List<Comment> Comments { get; set; }

    }
}
