using System;
using System.Runtime.Serialization;

namespace Neurotoxin.Godspeed.Shell.Exceptions
{
    public class EstablishmentFailedException : Exception
    {
        public EstablishmentFailedException()
        {
        }

        public EstablishmentFailedException(string message) : base(message)
        {
        }

        public EstablishmentFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EstablishmentFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}