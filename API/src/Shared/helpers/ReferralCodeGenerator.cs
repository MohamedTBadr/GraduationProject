using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers
{
    // Infrastructure/Helpers/ReferralCodeGenerator.cs
    public static class ReferralCodeGenerator
    {
        public static string Generate() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                   .Replace("=", "")
                   .Replace("+", "")
                   .Replace("/", "")[..8]
                   .ToUpper();
    }
}
