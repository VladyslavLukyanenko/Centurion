namespace Centurion.SeedWork.Web;

public static class BinarySizeExtensions
{
  public static int Gb(this int i) => i * (int) Math.Pow(1024, 3);
  public static int Mb(this int i) => i * (int) Math.Pow(1024, 2);
}