using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Exceptions
{
    public class UnprocessableContentException(List<string> errors) : Exception(string.Join(", ", errors))
    {
    }
}
