using System;

namespace Org.Igroknet.Auth.Domain
{
    internal class ConfirmedUser
    {
        public Guid ConfirmedUserId { get; set; }
        public Guid UserId { get; set; }
        public int ConfirmationCode { get; set; }

        public ConfirmedUser()
        {

        }

        public ConfirmedUser(Guid userId, int confirmationCode)
        {
            if(userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            UserId = userId;

            ConfirmedUserId = Guid.NewGuid();

            ConfirmationCode = confirmationCode;
        }
    }
}
