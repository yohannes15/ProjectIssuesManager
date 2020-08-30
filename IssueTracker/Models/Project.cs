using System;
using System.Collections.Generic;
using IssueTracker.Models.Enums;
using IssueTracker.Models.CombModels;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }
        [Required]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        [Required]
        [Display(Name = "Project Description")]
        public string ProjectDescription { get; set; }
        public string UsersAssigned { get; set; }
        public string OwnerId { get; set; }
        public string OwnerUserName { get; set; }
        [Required]
        public ProjectStatus ProjectStatus { get; set; }
        public List<ProjectIssue> ProjectIssues { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

    }
}