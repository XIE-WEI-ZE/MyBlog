using System.Text.RegularExpressions;

namespace prjMyBlog.Helpers
{
    public static class HtmlUtility
    {
        public static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
