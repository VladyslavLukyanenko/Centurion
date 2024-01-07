using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Centurion.Accounts.Infra;

public static class StringExtensions
{
  public static string ToSnakeCase(this string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      return input;
    }

    var startUnderscores = Regex.Match(input, @"^_+");
    return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
  }

  public static string ToCamelCase(this string input)
  {
    if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
    {
      return input;
    }

    // ReSharper disable once RedundantToStringCallForValueType
    return char.ToLowerInvariant(input[0]).ToString() + input.Substring(1);
  }

  public static string NonSpacingMark(this string? stFormD)
  {
    if (stFormD == null)
    {
      return "";
    }

    stFormD = stFormD.Trim().ToLowerInvariant().Replace(" ", "");

    stFormD = stFormD.Normalize(NormalizationForm.FormD);
    StringBuilder sb = new();

    for (var ich = 0; ich < stFormD.Length; ich++)
    {
      var uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
      if (uc != UnicodeCategory.NonSpacingMark)
      {
        sb.Append(stFormD[ich]);
      }
    }

    return sb.ToString().Normalize(NormalizationForm.FormC);
  }
}