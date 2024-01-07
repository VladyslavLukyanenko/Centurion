namespace Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;

public class ImageSizeUtil
{
  public static ImageSize Resize(ImageSize source, int dest, ImageResizeBy resizeBy)
  {
    int height, width;
    if (resizeBy == ImageResizeBy.Height)
    {
      var factor = dest / (double) source.Height;
      width = (int) (source.Width * factor);
      height = dest;
    }
    else
    {
      var factor = dest / (double) source.Width;
      height = (int) (source.Height * factor);
      width = dest;
    }


    return new ImageSize(width, height);
  }
}