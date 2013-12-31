using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Hash(this string s)
        {
            var shA1Managed = new SHA1Managed();
            return shA1Managed.ComputeHash(Encoding.UTF8.GetBytes(s)).ToHex();
        }

        public static string GetParentPath(this string path)
        {
            path = "/" + path.TrimEnd('/', '\\');
            var r = new Regex(@"^(.*[\\/]).*$");
            return r.Replace(path, "$1").Substring(1);
        }
    }
}
