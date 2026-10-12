using System.Text;
using System.Text.RegularExpressions;

namespace EduQuest.API.Services
{
    // ======================================================================
    // MathText
    //
    // Maths in EduQuest is stored as LaTeX between dollar signs, for example
    //     Find $\frac{x^3}{3}$ when $x = 2$.
    // The admin page (js/math-format.js) typesets anything written this way.
    //
    // AI replies reach us as JSON, and a backslash that was not escaped
    // properly in JSON turns into a control character:
    //     \frac -> form feed + "rac"      \to -> tab + "o"
    //     \neq  -> newline + "eq"         \beta -> backspace + "eta"
    // This class puts those backslashes back, but only inside $...$ so that
    // ordinary text is never touched.
    // ======================================================================
    public static class MathText
    {
        // A $...$ span. Same rules as the frontend: the opening $ is not followed by a
        // space, the closing $ is not preceded by one or followed by a digit, and
        // \$ is a literal dollar sign. Only a real space (not a tab, form feed or newline)
        // stops the opening $, because those characters are exactly what a swallowed
        // backslash looks like: "$\frac" arrives as "$" + form feed + "rac".
        private static readonly Regex DollarSpan = new(
            @"(?<!\\)\$(?![ \u00A0])((?:\\.|[^$\\])+?)(?<!\s)\$(?!\d)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        // "\\frac" (an over-escaped backslash) -> "\frac".
        private static readonly Regex DoubledBackslash = new(
            @"\\\\(?=[A-Za-z])",
            RegexOptions.Compiled);

        // Trims the text and repairs any LaTeX inside $...$.
        public static string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return DollarSpan
                .Replace(text, m => "$" + RepairLatex(m.Groups[1].Value) + "$")
                .Trim();
        }

        // Same as Normalize, but keeps "nothing" as null (used for the optional source extract).
        public static string? NormalizeOrNull(string? text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : Normalize(text);
        }

        private static string RepairLatex(string latex)
        {
            var sb = new StringBuilder(latex.Length + 8);

            for (var i = 0; i < latex.Length; i++)
            {
                var c = latex[i];
                var letterFollows = i + 1 < latex.Length && char.IsLetter(latex[i + 1]);

                // A control character straight before a letter is a swallowed backslash command.
                if (letterFollows && (c == '\t' || c == '\f' || c == '\b' || c == '\r' || c == '\n'))
                {
                    sb.Append('\\');
                    sb.Append(c switch
                    {
                        '\t' => 't',
                        '\f' => 'f',
                        '\b' => 'b',
                        '\r' => 'r',
                        _ => 'n'
                    });
                }
                else
                {
                    sb.Append(c);
                }
            }

            return DoubledBackslash.Replace(sb.ToString(), @"\");
        }
    }
}