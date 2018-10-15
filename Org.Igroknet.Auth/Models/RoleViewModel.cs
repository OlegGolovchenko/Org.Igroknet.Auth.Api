using System;

namespace Org.Igroknet.Auth.Models
{
    public class RoleViewModel
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public string Claims { get; set; }
    }
}
