using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class BadRequestException : Exception
    {
        public List<string> Errors { get; }

        public BadRequestException(List<string> errors)
: base($"Validation failed: {string.Join(", ", errors)}")
        {
            Errors = errors;
        }
    }
}

