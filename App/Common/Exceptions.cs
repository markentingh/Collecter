using System;

namespace Collector
{
    public class LogicException : Exception
    {
        public string Method { get; set; } = "";
        public string Argument { get; set; } = "";
        public LogicErrorCode ErrorCode { get; set; } = LogicErrorCode.Unknown;

        public LogicException() { }
        /// <summary>
        /// Generates a human-readable error based on the failed business logic.
        /// </summary>
        /// <param name="errorCode">A code that can be used to identify specific errors, and is for internal logging & debugging only
        /// </param>
        /// <param name="argument"></param>
        /// <param name="method"></param>
        public LogicException(LogicErrorCode errorCode, string message = "", string argument = "", string method = "") : base(message)
        {
            ErrorCode = errorCode;
            Argument = argument;
            Method = method;
        }
    }

    public enum LogicErrorCode : int
    {
        Unknown = 0,

        //User (1001001 to 1001999)
        User_Missing_Encrypted_Password = 1001001
    }
}
