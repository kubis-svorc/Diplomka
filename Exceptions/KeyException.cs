using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplomka.Exceptions
{
    public class KeyException : ApplicationException
    {
        public KeyException(string message) : base(message)
        {

        }
    }
}
