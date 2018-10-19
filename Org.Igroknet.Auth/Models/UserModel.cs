using System;

namespace Org.Igroknet.Auth.Models
{
    public class UserModel
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string[] Claims { get; set; }
        public bool IsActive { get; set; }
    }
}
