using Org.Igroknet.Auth.Models;
using System;

namespace Org.Igroknet.Auth.Domain
{
    public interface IUserService
    {
        void AddUser(string login, string password);

        void RenameUser(Guid userId,string firstName, string lastName);

        void SetUsersPassword(Guid userId, string password);

        void SetUserRole(Guid userId, Guid roleId);

        void SendConfirmationCode(Guid userId, string from);

        void ConfirmUser(Guid userId, int confirmationCode);

        void EnableDisableUser(Guid userId);

        void RemoveUser(Guid userId);

        UserModel LoginUser(string login, string password);
    }
}
