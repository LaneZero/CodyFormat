using System.Text;
using System.Text.RegularExpressions;

namespace CodyFormat.Services;

public static class GlobMatcher
{
    public static bool IsMatch(string relativePath, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var path = Normalize(relativePath);
        var glob = Normalize(pattern).TrimStart('/');

        if (!glob.Contains('/'))
            path = Path.GetFileName(path);

        var options = OperatingSystem.IsWindows()
            ? RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
            : RegexOptions.CultureInvariant;

        return Regex.IsMatch(path, BuildRegex(glob), options, TimeSpan.FromSeconds(1));
    }

    private static string BuildRegex(string glob)
    {
        var sb = new StringBuilder("^");

        for (var i = 0; i < glob.Length; i++)
        {
            var c = glob[i];
            if (c == '*')
            {
                if (i + 1 < glob.Length && glob[i + 1] == '*')
                {
                    i++;
                    if (i + 1 < glob.Length && glob[i + 1] == '/')
                    {
                        i++;
                        sb.Append("(?:.*/)?");
                    }
                    else
                    {
                        sb.Append(".*");
                    }
                }
                else
                {
                    sb.Append("[^/]*");
                }
            }
            else if (c == '?')
            {
                sb.Append("[^/]");
            }
            else
            {
                sb.Append(Regex.Escape(c.ToString()));
            }
        }

        sb.Append('$');
        return sb.ToString();
    }

    private static string Normalize(string value) => value.Replace('\\', '/');
}
