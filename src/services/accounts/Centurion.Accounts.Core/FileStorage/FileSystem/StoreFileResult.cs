namespace Centurion.Accounts.Core.FileStorage.FileSystem;

public class StoreFileResult
{
  public StoreFileResult(string fullPath, string relativePath, string hash, string fileName, string extension)
  {
    FullPath = fullPath;
    RelativeFilePath = relativePath;
    ComputedHash = hash;
    Extension = extension;
    FileName = fileName;
  }

  public string FullPath { get; }

  public string ComputedHash { get; }

  public string RelativeFilePath { get; }

  public string Extension { get; }

  public string FileName { get; }
}