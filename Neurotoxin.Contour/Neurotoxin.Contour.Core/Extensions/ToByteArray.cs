using System;

namespace Neurotoxin.Contour.Core.Extensions
{
    public static class StringToByteArray
    {
        public static byte[] ToByteArray(this string s)
        {
            return ToByteArray(s, s.Length);
        }

        public static byte[] ToByteArray(this string s, int length) 
        {
            return ExtensionHelper.BlockCopy(s.ToCharArray(), length);
        }
    }

    public static class UIntToByteArray 
    {
        public static byte[] ToByteArray(this uint v, int length)
        {
            return ExtensionHelper.BlockCopy(BitConverter.GetBytes(v), length);
        }
    }

    public static class ByteArrayToByteArray
    {
        public static byte[] ToByteArray(this byte[] a, int length)
        {
            return ExtensionHelper.BlockCopy(a, length);
        }
    }
}