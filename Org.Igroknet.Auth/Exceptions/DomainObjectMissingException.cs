using System;

namespace Org.Igroknet.Auth.Exceptions
{
    public class DomainObjectMissingException: Exception
    {
        public DomainObjectMissingException(string objectName): base($"{objectName} does not exist in current data context.")
        {
        }
    }
}
