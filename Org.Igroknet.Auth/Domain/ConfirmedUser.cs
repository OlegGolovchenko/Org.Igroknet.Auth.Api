using System;

namespace Org.Igroknet.Auth.Domain
{
    public class ConfirmedUser
    {
        public Guid ConfirmedUserId { get; private set; }
        public Guid UserId { get; private set; }
        public int ConfirmationCode { get; private set; }

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
