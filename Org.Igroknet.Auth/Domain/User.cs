using SQLite;
using System;
using org.igrok.validator;
using System.Security.Cryptography;
using System.Text;

namespace Org.Igroknet.Auth.Domain
{
    internal class User
    {
        [PrimaryKey]
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public Guid RoleId { get; set; }
        public bool IsActive { get; set; }

        public User()
        {

        }

        public User(string email, string password)
        {
            UserId = Guid.NewGuid();
            EmailValidator.ValidateEmail(email);
            Email = email;
            Password = HashPassword(password);
            IsActive = false;
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            using (var sha256 = SHA256.Create())
            {
                var hashedPassword = "";
                var passHash = sha256.ComputeHash(Encoding.Unicode.GetBytes(hashedPassword));
                foreach (var passHashByte in passHash)
                {
                    hashedPassword += string.Format("{0:x2}", passHashByte);
                }
                return hashedPassword;
            }
        }

        public void SetName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                new ArgumentNullException(nameof(firstName));
            }

            FirstName = firstName;

            if (string.IsNullOrWhiteSpace(lastName))
            {
                new ArgumentNullException(nameof(lastName));
            }

            LastName = lastName;
        }

        public void SetRole(Guid roleId)
        {
            RoleId = roleId;
        }

        public void ChangePassword(string password)
        {
            Password = HashPassword(password);
        }

        public void SwitchActiveStatus()
        {
            IsActive = !IsActive;
        }

        public void ConfirmAccount()
        {
            IsActive = true;
        }
    }
}
