using System.Text;
using Unidecode.NET;

namespace Centurion.Accounts.Core.Primitives;

public class UriSafeString
  : ValueObject
{
  protected UriSafeString(string value)
  {
    if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(value));

    Value = value;
  }

  public string Value { get; }

  /// <summary>
  ///   Produces optional, URL-friendly version of a title, "like-this-one".
  ///   hand-tuned for speed, reflects performance refactoring contributed
  ///   by John Gietzen (user otac0n)
  /// </summary>
  public static UriSafeString Create(params string[] tokens)
  {
    return Create(0, 80, tokens);
  }

  /// <summary>
  ///   Produces optional, URL-friendly version of a title, "like-this-one".
  ///   hand-tuned for speed, reflects performance refactoring contributed
  ///   by John Gietzen (user otac0n)
  /// </summary>
  // ReSharper disable once CognitiveComplexity
  public static UriSafeString Create(int minLength = 0, int maxLength = 80, params string[] tokens)
  {
    var title = string.Join("-", tokens.Where(t => !string.IsNullOrEmpty(t)));
    if (title.Length < minLength)
    {
      throw new ArgumentException($"Length should be at least {minLength} characters long", nameof(title));
    }

    if (string.IsNullOrWhiteSpace(title))
    {
      throw new ArgumentException();
    }

    var len = title.Length;
    var prevdash = false;
    var sb = new StringBuilder(len);
    char c;

    for (var i = 0; i < len; i++)
    {
      c = title[i];
      if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9')
      {
        sb.Append(c);
        prevdash = false;
      }
      else if (c >= 'A' && c <= 'Z')
      {
        // tricky way to convert to lowercase
        sb.Append((char) (c | 32));
        prevdash = false;
      }
      else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
               c == '\\' || c == '-' || c == '_' || c == '=')
      {
        if (!prevdash && sb.Length > 0)
        {
          sb.Append('-');
          prevdash = true;
        }
      }
      else if (c >= 128)
      {
        var prevlen = sb.Length;
        sb.Append(RemapInternationalCharToAscii(c));
        if (prevlen != sb.Length) prevdash = false;
      }

      if (i == maxLength) break;
    }

    string slugValue;

    if (prevdash)
      slugValue = sb.ToString().Substring(0, sb.Length - 1);
    else
      slugValue = sb.ToString();

    if (string.IsNullOrEmpty(slugValue) || slugValue.Length < minLength)
      return Create(minLength, maxLength, title.Unidecode());

    return new UriSafeString(slugValue);
  }

  // ReSharper disable once CognitiveComplexity
  public static string RemapInternationalCharToAscii(char c)
  {
    var s = c.ToString().ToLowerInvariant();
    if ("àåáâäãåą".Contains(s))
      return "a";
    if ("èéêëę".Contains(s))
      return "e";
    if ("ìíîïı".Contains(s))
      return "i";
    if ("òóôõöøőð".Contains(s))
      return "o";
    if ("ùúûüŭů".Contains(s))
      return "u";
    if ("çćčĉ".Contains(s))
      return "c";
    if ("żźž".Contains(s))
      return "z";
    if ("śşšŝ".Contains(s))
      return "s";
    if ("ñń".Contains(s))
      return "n";
    if ("ýÿ".Contains(s))
      return "y";
    if ("ğĝ".Contains(s))
      return "g";
    if (c == 'ř')
      return "r";
    if (c == 'ł')
      return "l";
    if (c == 'đ')
      return "d";
    if (c == 'ß')
      return "ss";
    if (c == 'Þ')
      return "th";
    if (c == 'ĥ')
      return "h";
    if (c == 'ĵ')
      return "j";
    return "";
  }

  protected override IEnumerable<object> GetAtomicValues()
  {
    yield return Value;
  }

  public static UriSafeString FromSource(string source)
  {
    return new UriSafeString(source.ToLowerInvariant());
  }
}