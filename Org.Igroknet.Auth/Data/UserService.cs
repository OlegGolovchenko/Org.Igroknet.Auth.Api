using Org.Igroknet.Auth.Domain;
using Org.Igroknet.Auth.Exceptions;
using SQLite;
using System;
using System.Linq;

namespace Org.Igroknet.Auth.Data
{
    public class UserService : IUserService
    {
        private SQLiteConnection _connection;

        public UserService(SQLiteConnection connection)
        {
            _connection = connection;
            InitTable();
        }

        private void InitTable()
        {
            try
            {
                _connection.BeginTransaction();

                _connection.CreateTable<User>();

                _connection.CreateTable<Role>();

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

                _connection.Insert(new User(login, password));

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

                if(user == null)
                {
                    throw new DomainObjectMissingException(nameof(user));
                }

                user.SetName(firstName, lastName);

                _connection.Commit();
            }catch (Exception e)
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

                if(role == null)
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
                    if(confirmation != null)
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
    }
}
