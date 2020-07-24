using System;
using System.Runtime.Serialization;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions
{
    internal class MarketingPreferencesException : Exception
    {
        private const string DefaultMessage = "The form doesn't contain a marketing preferences field! Submitting will be aborted!";

        public MarketingPreferencesException(string message = DefaultMessage) : base(message)
        {
        }

        public MarketingPreferencesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MarketingPreferencesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
