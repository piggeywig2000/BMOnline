using System;
using System.Collections.Generic;
using System.Text;

namespace BMOnline.Common
{
    public static class StringExtensions
    {
        public static string RemoveWhitespace(this string str)
            => str.Replace('\n', ' ')
                  .Replace('\r', ' ')
                  .Replace('\t', ' ')
                  .Replace('\f', ' ')
                  .Trim();
    }
}
