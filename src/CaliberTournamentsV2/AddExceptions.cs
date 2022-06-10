namespace CaliberTournamentsV2
{
    internal class InitException : Exception
    {
        internal InitException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
