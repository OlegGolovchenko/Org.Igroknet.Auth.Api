using SQLite;
using System;
using System.Linq;

namespace Org.Igroknet.Auth.Domain
{
    public class Role
    {
        [PrimaryKey]
        public Guid RoleId { get; private set; }
        public string Name { get; private set; }
        public string Claims { get; private set; }

        public Role()
        {

        }

        public Role(string name, string claims)
        {
            RoleId = Guid.NewGuid();
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
            Claims = claims;
        }

        public void AddClaim(string claim)
        {
            var claims = Claims.Split('|').ToList();
            if(!claims.Any(clm=>clm == claim))
            {
                claims.Add(claim);
            }
            Claims = string.Join("|", claims);
        }

        public void RemoveClaim(string claim)
        {
            var claims = Claims.Split('|').ToList();
            if (claims.Any(clm => clm == claim))
            {
                claims.Remove(claim);
            }
            Claims = string.Join("|", claims);
        }
    }
}
