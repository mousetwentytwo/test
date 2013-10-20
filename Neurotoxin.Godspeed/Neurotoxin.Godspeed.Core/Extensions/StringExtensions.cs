using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Hash(this string s)
        {
            var shA1Managed = new SHA1Managed();
            return shA1Managed.ComputeHash(Encoding.UTF8.GetBytes(s)).ToHex();
        }
    }
}
