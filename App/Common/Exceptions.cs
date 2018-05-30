using System;

namespace Collector
{
    public class ServiceErrorException : Exception
    {
        public ServiceErrorException() { }
        public ServiceErrorException(string message) { }
    }

    public class ServiceDeniedException : Exception
    {
        public ServiceDeniedException() { }
        public ServiceDeniedException(string message) { }
    }
}
