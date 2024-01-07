namespace Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;

public class ImageSize
{
  public ImageSize()
  {
  }

  public ImageSize(int width, int height)
  {
    Width = width;
    Height = height;
  }

  public int Width { get; set; }

  public int Height { get; set; }
}