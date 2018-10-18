using org.igrok.validator;
using Org.Igroknet.Auth.Domain;
using Org.Igroknet.Auth.Exceptions;
using Org.Igroknet.Auth.Models;
using SQLite;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Org.Igroknet.Auth.Data
{
    public class UserService : IUserService
    {
        private SQLiteConnection _connection;
        private SmtpClient _mailer;

        public UserService(SQLiteConnection connection, SmtpClient mailer)
        {
            _connection = connection;
            _mailer = mailer;
            InitTable();
        }

        private void InitTable()
        {
            try
            {
                _connection.BeginTransaction();

                _connection.CreateTable<User>();

                _connection.CreateTable<Role>();

                _connection.CreateTable<ConfirmedUser>();

                var admin = new User("admin@local.site", "Admin");
                admin.SetName("Main", "Administrator");
                admin.ConfirmAccount();
                var role = new Role("Admin");

                var userRole = new Role("User");

                admin.SetRole(role.RoleId);

                var confirmedAdmin = new ConfirmedUser(admin.UserId, 123456);

                if (_connection.Table<User>().Count() == 0)
                {

                    _connection.InsertOrReplace(admin);

                    _connection.InsertOrReplace(role);

                    _connection.InsertOrReplace(userRole);

                    _connection.InsertOrReplace(confirmedAdmin);
                }
                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
            }
        }

        public void AddUser(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            var hashedPassword = User.HashPassword(password);
            if (_connection.Table<User>().Any(x => x.FirstName == login && x.Password == hashedPassword))
            {
                throw new InvalidOperationException("User already exists");
            }

            try
            {
                _connection.BeginTransaction();

                var user = new User(login, password);

                _connection.Insert(user);

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void RenameUser(Guid userId, string firstName, string lastName)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                user.SetName(firstName, lastName);

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void SetUsersPassword(Guid userId, string password)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                user.ChangePassword(password);

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void SetUserRole(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                var role = _connection.Table<Role>().SingleOrDefault(rl => rl.RoleId == roleId);

                if (role == null)
                {
                    throw new DomainObjectMissingException(nameof(role));
                }

                user.SetRole(roleId);

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void ConfirmUser(Guid userId, int confirmationCode)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                if (_connection.Table<ConfirmedUser>().Any(cu => cu.UserId == userId && cu.ConfirmationCode == confirmationCode) && !user.IsActive)
                {
                    user.ConfirmAccount();
                    var confirmation = _connection.Table<ConfirmedUser>().SingleOrDefault(cu => cu.UserId == userId && cu.ConfirmationCode == confirmationCode);
                    if (confirmation != null)
                    {
                        _connection.Delete(confirmation);
                    }
                }

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void EnableDisableUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                if (_connection.Table<ConfirmedUser>().Any(cu => cu.UserId == userId))
                {
                    user.SwitchActiveStatus();
                }

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void SendConfirmationCode(Guid userId, string from)
        {
            EmailValidator.ValidateEmail(from);
            if(userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();
                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                if (!_connection.Table<ConfirmedUser>().Any(cu => cu.UserId == userId))
                {
                    var random = new Random();
                    var confCode = random.Next();
                    var ConfirmedUser = new ConfirmedUser(userId, confCode);
                    var message = new MailMessage(from, user.Email, "Please confirm your account.", $"Your confirmation code is: {confCode}.");
                    _mailer.Send(message);
                    _connection.Insert(ConfirmedUser);
                }

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public void RemoveUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.UserId == userId);

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                _connection.Delete(user);

                _connection.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public UserModel LoginUser(string login, string password)
        {
            try
            {
                _connection.BeginTransaction();

                var user = _connection.Table<User>().SingleOrDefault(usr => usr.Email == login && usr.Password == User.HashPassword(password));

                if (user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                var role = _connection.Table<Role>().SingleOrDefault(rl => rl.RoleId == user.RoleId);

                if(role == null)
                {
                    throw new DomainObjectMissingException(nameof(role));
                }

                var model = new UserModel
                {
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    UserId = user.UserId,
                    Claim = role.Name,
                    IsActive = user.IsActive
                };

                _connection.Commit();

                return model;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }

        public Guid AddRole(AddRoleViewModel model)
        {
            try
            {
                _connection.BeginTransaction();

                var role = new Role(model.Name);

                _connection.Insert(role);

                _connection.Commit();

                return role.RoleId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _connection.Rollback();
                throw;
            }
        }
    }
}
