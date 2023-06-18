using System.Text.RegularExpressions;

namespace BMOnline.Common
{
    public static class StringExtensions
    {
        private static readonly Regex richTextRegex = new Regex(@"<\/?(b|i|size|color|material|quad)(=.*?)?>", RegexOptions.IgnoreCase);

        public static string RemoveWhitespace(this string str)
            => str.Replace('\n', ' ')
                  .Replace('\r', ' ')
                  .Replace('\t', ' ')
                  .Replace('\f', ' ')
                  .Trim();

        public static string RemoveDoubleSpaces(this string str)
        {
            bool wasLastSpace = false;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                if (str[i] == ' ')
                {
                    if (wasLastSpace)
                        str = str.Substring(0, i) + str.Substring(i+1, str.Length - i - 1);
                    wasLastSpace = true;
                }
                else
                {
                    wasLastSpace = false;
                }
            }
            return str;
        }

        public static string RemoveRichText(this string str)
        {
            while (richTextRegex.IsMatch(str))
            {
                str = richTextRegex.Replace(str, "");
            }
            return str;
        }
    }
}
