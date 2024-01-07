#pragma warning disable 8618

namespace Centurion.Accounts.App.Config;

public class CommonConfig
{
  public static CommonConfig Instance { get; private set; }


  public string WebsiteUrl { get; set; }
  public EmailNotificationsConfig EmailNotifications { get; set; }
  public GlobalUploadsConfig Uploads { get; set; }
  public HostingInfoConfiguration HostingInfo { get; set; }

  public CorsConfig Cors { get; set; }

  public static void SetInstance(CommonConfig configuration)
  {
    Instance = configuration;
  }

  public class GlobalUploadsConfig
  {
    public int FileSizeLimitBytes { get; set; }
    public string DirectoryName { get; set; }
    public bool IsPathRelative { get; set; }
  }

  public class CorsConfig
  {
    public bool UseCors { get; set; }
    public IReadOnlyList<string> AllowedHosts { get; set; }
  }

  public class EmailNotificationsConfig
  {
    public enum SecureConnectionOptions
    {
      None,
      SslOnConnect,
      StartTls,
      StartTlsWhenAvailable
    }

    public string SenderEmail { get; set; }

    public string SenderPassword { get; set; }

    public int SmtpPort { get; set; }

    public string SmtpHost { get; set; }

    public SecureConnectionOptions SmtpSecurity { get; set; }

    public string EmailTemplate { get; set; }
  }

  public class HostingInfoConfiguration
  {
    public string HostName
    {
      get
      {
        string hostName = Schema + Uri.SchemeDelimiter + DomainName;
        if (!string.IsNullOrWhiteSpace(Port))
        {
          hostName += ":" + Port;
        }

        return hostName;
      }
    }

    public string Schema { get; set; }
    public string DomainName { get; set; }
    public string Port { get; set; }

    public string ResolveAbsoluteUrl(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        throw new ArgumentNullException(nameof(path));
      }

      if (path.StartsWith("http") && path.Contains(Uri.SchemeDelimiter))
      {
        return path;
      }

      return HostName + path;
    }
  }
}