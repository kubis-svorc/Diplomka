namespace Diplomka.Exceptions
{
    public abstract class ApplicationException : System.Exception
    {
        public ApplicationException(string message) : base(message)
        {

        }
    }
}
