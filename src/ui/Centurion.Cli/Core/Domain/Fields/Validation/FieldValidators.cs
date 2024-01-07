using System.Text.RegularExpressions;

namespace Centurion.Cli.Core.Domain.Fields.Validation;

public static class FieldValidators
{
  public static Func<string, Task<bool>> IsMatches(string pattern) => IsMatches(new Regex(pattern,
    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));

  public static Func<string, Task<bool>> IsMatches(Regex regex)
  {
    return value =>
    {
      var isMatch = value != null && regex.IsMatch(value);
      return Task.FromResult(isMatch);
    };
  }

  public static Func<T, Task<bool>> AlwaysValid<T>() => _ => Task.FromResult(true);
  public static Func<T, Task<bool>> NonDefaultValue<T>() => v => Task.FromResult(!Equals(v, default));
}