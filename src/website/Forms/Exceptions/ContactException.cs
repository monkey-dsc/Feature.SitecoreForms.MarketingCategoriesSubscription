using System;
using System.Runtime.Serialization;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions
{
    internal class ContactException : Exception
    {
        private const string DefaultMessage = "Unable to retrieve the contact from XConnect! Submitting will be aborted!";

        public ContactException(string message = DefaultMessage) : base(message)
        {
        }

        public ContactException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContactException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
