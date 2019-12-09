using System;
using System.Runtime.Serialization;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions
{
    internal class ManagerRootException : Exception
    {
        private const string DefaultMessage = "Manager root is null! In order to subscribe a contact to marketing categories you have to select a valid manager root!";

        public ManagerRootException(string message = DefaultMessage) : base(message)
        {
        }

        public ManagerRootException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ManagerRootException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
