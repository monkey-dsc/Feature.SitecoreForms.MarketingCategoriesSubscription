using System;
using System.Runtime.Serialization;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions
{
    internal class ContactListException : Exception
    {
        public const string DefaultMessage = "To subscribe a contact to a contact list using double-opt-in, a valid contact list is needed and the list should not be segmented!";

        public ContactListException(string message = DefaultMessage) : base(message)
        {
        }

        public ContactListException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContactListException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
