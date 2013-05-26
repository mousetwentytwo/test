using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static int ToFatFileTime(this DateTime time)
        {
            var y = 0;
            y |= ((time.Year - 1980) & 0xEF) << 25;
            y |= (time.Month & 0xF) << 21;
            y |= (time.Day & 0x1F) << 16;
            y |= (time.Hour & 0x1F) << 11;
            y |= (time.Minute & 0x3F) << 5;
            y |= time.Second & 0x1F;
            return y;
        }
    }
}