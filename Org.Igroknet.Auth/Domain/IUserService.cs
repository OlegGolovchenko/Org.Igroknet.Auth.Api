using System;

namespace Org.Igroknet.Auth.Domain
{
    public interface IUserService
    {
        void AddUser(string login, string password);

        void RenameUser(Guid userId,string firstName, string lastName);

        void SetUsersPassword(Guid userId, string password);

        void SetUserRole(Guid userId, Guid roleId);

        void ConfirmUser(Guid userId, int confirmationCode);

        void EnableDisableUser(Guid userId);
    }
}
