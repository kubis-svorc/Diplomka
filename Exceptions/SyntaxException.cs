namespace Diplomka.Exceptions
{
    class SyntaxException : System.Exception
    {
        public SyntaxException() : base()
        { 
        }

        public SyntaxException(string message) : base(message)
        {
        }
    }
}
