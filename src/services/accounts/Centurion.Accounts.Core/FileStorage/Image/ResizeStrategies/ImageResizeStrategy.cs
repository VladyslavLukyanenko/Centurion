namespace Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;

public enum ImageResizeStrategy
{
  /// <summary>
  ///   Image will be cropped to specified size
  /// </summary>
  Crop,

  /// <summary>
  ///   Image will be scaled proportionally by height
  /// </summary>
  // TODO: maybe better to have one option for height and width?
  ByHeight,

  /// <summary>
  ///   Image will be scaled proportionally by width
  /// </summary>
  ByWidth,

  /// <summary>
  ///   Image will be untouched
  /// </summary>
  None
}