using System;
namespace IssueTracker.Models.CombModels
{
    public class UserProjects
    {
       public int Id { get; set; }
       public int ProjectId { get; set; }
       public int AssignedUserId { get; set; }
    }
}
