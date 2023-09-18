using System;

namespace Diplomka.Exceptions
{
    public class NameException : ApplicationException
    {
        public NameException(string messsage) : base(messsage)
        {
            ;
        }

    }
}
