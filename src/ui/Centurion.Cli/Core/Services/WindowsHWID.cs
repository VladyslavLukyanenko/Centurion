using System.Management;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1416

namespace Centurion.Cli.Core.Services;

public class WindowsHWID
{
  private static string _fingerPrint = string.Empty;

  public static string Value()
  {
    if (string.IsNullOrEmpty(_fingerPrint))
    {
      _fingerPrint = GetHash("CPU >> " + CpuId() + "\nBIOS >> " + BiosId() + "\nBASE >> " + BaseId() +
                             "\nDISK >> " + DiskId() + "\nVIDEO >> " + VideoId() + "\nMAC >> " + MacId());
    }

    return _fingerPrint;
  }

  private static string GetHash(string s)
  {
    MD5 sec = new MD5CryptoServiceProvider();
    ASCIIEncoding enc = new();
    byte[] bt = enc.GetBytes(s);
    return GetHexString(sec.ComputeHash(bt));
  }

  private static string GetHexString(byte[] bt)
  {
    string s = string.Empty;
    for (int i = 0; i < bt.Length; i++)
    {
      byte b = bt[i];
      int n, n1, n2;
      n = b;
      n1 = n & 15;
      n2 = (n >> 4) & 15;
      if (n2 > 9)
        s += ((char) (n2 - 10 + 'A')).ToString();
      else
        s += n2.ToString();
      if (n1 > 9)
        s += ((char) (n1 - 10 + 'A')).ToString();
      else
        s += n1.ToString();
      if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
    }

    return s;
  }

  #region Original Device ID Getting Code

  private static string? Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
  {
    string? result = "";
    ManagementClass mc = new(wmiClass);
    ManagementObjectCollection moc = mc.GetInstances();
    foreach (ManagementObject mo in moc.OfType<ManagementObject>())
    {
      if (mo[wmiMustBeTrue].ToString() == "True")
      {
        if (result == "")
        {
          try
          {
            result = mo[wmiProperty]?.ToString();
            break;
          }
          catch
          {
            // expected
          }
        }
      }
    }

    return result;
  }

  private static string Identifier(string wmiClass, string wmiProperty)
  {
    // if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    // {
    // 	throw new PlatformNotSupportedException();
    // }

    var mc = new ManagementClass(wmiClass);
    ManagementObjectCollection moc = mc.GetInstances();

    return moc.OfType<ManagementObject>()
      .SelectMany(_ =>
        _.Properties.OfType<PropertyData>().Union(_.SystemProperties.OfType<PropertyData>()))
      .FirstOrDefault(_ => _.Name == wmiProperty)?.Value?.ToString() ?? string.Empty;
    // string result = "";
    // foreach (ManagementObject mo in moc)
    // {
    //     if (result == "")
    //     {
    //         try
    //         {
    //             result = mo[wmiProperty].ToString();
    //             break;
    //         }
    //         catch
    //         {
    //         }
    //     }
    // }
    // return result;
  }

  private static string CpuId()
  {
    try
    {
      string retVal = Identifier("Win32_Processor", "UniqueId");
      if (retVal == "")
      {
        retVal = Identifier("Win32_Processor", "ProcessorId");
        if (retVal == "")
        {
          retVal = Identifier("Win32_Processor", "Name");
          if (retVal == "")
          {
            retVal = Identifier("Win32_Processor", "Manufacturer");
          }

          retVal += Identifier("Win32_Processor", "MaxClockSpeed");
        }
      }

      return retVal;
    }
    catch
    {
      return "null";
    }
  }

  private static string BiosId()
  {
    try
    {
      return Identifier("Win32_BIOS", "Manufacturer") + Identifier("Win32_BIOS", "SMBIOSBIOSVersion") +
             Identifier("Win32_BIOS", "IdentificationCode") + Identifier("Win32_BIOS", "SerialNumber") +
             Identifier("Win32_BIOS", "ReleaseDate") + Identifier("Win32_BIOS", "Version");
    }
    catch
    {
      return "null";
    }
  }

  private static string DiskId()
  {
    try
    {
      return Identifier("Win32_DiskDrive", "Model") + Identifier("Win32_DiskDrive", "Manufacturer") +
             Identifier("Win32_DiskDrive", "Signature") + Identifier("Win32_DiskDrive", "TotalHeads");
    }
    catch
    {
      return "null";
    }
  }

  private static string BaseId()
  {
    try
    {
      return Identifier("Win32_BaseBoard", "Model") + Identifier("Win32_BaseBoard", "Manufacturer") +
             Identifier("Win32_BaseBoard", "Name") + Identifier("Win32_BaseBoard", "SerialNumber");
    }
    catch
    {
      return "null";
    }
  }

  private static string VideoId()
  {
    try
    {
      return Identifier("Win32_VideoController", "DriverVersion") +
             Identifier("Win32_VideoController", "Name");
    }
    catch
    {
      return "null";
    }
  }

  private static string MacId()
  {
    try
    {
      return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled")
             ?? throw new InvalidOperationException("Can't generate MacId");
    }
    catch
    {
      return "null";
    }
  }

  #endregion
}