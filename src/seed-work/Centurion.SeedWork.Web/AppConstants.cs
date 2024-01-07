using System.Reflection;

namespace Centurion.SeedWork.Web;

public static class AppConstants
{
  static AppConstants()
  {
    var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
    var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product;
    var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()!.Company;

    CurrentAppVersion = assembly.GetName().Version!;
    ProductName = product;
    DevelopmentTeam = company;
  }
    
  public static Version CurrentAppVersion { get; }
  public static string ProductName { get; }
  public static string DevelopmentTeam { get; }

  public static string InformationalVersion => GitVersionInformation.InformationalVersion;
  public static string ProductFullName { get; } = ProductName + " v" + CurrentAppVersion;
}