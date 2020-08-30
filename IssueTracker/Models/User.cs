using System;
namespace IssueTracker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DateOfBirth { get; set; }
        public string ProfilePicUrl { get; set; }
        public Role Role { get; set; }
        public int AssignedProject { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
