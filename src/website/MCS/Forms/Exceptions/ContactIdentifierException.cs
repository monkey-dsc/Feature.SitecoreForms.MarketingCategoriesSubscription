using System;
using System.Runtime.Serialization;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions
{
    internal class ContactIdentifierException : Exception
    {
        private const string DefaultMessage = "A contact must have a valid email address in order to subscribe to marketing categories! Submitting will be aborted!";

        public ContactIdentifierException(string message = DefaultMessage) : base(message)
        {
        }

        public ContactIdentifierException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContactIdentifierException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
