using SQLite;
using System;

namespace Org.Igroknet.Auth.Domain
{
    internal class Role
    {
        [PrimaryKey]
        public Guid RoleId { get; set; }
        public string Name { get; set; }

        public Role()
        {

        }

        public Role(string name)
        {
            RoleId = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }
    }
}
