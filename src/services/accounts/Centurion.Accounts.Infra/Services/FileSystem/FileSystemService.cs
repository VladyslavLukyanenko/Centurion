namespace Centurion.Accounts.Infra.Services.FileSystem;

public class FileSystemService
  : IFileSystemService
{
  private readonly IMimeTypeResolver _mimeTypeProvider;

  public FileSystemService(IMimeTypeResolver mimeTypeProvider)
  {
    _mimeTypeProvider = mimeTypeProvider;
  }

  public async Task<string> SaveBinaryAsync(string storeRoot, string fileName, Stream fileInputStream,
    CancellationToken cancellationToken = default)
  {
    fileInputStream.Seek(0, SeekOrigin.Begin); // go to start of the file
    if (!Directory.Exists(storeRoot))
    {
      Directory.CreateDirectory(storeRoot);
    }

    var fullPath = Path.Combine(storeRoot, fileName);
    using (var file = File.Create(fullPath))
    {
      await fileInputStream.CopyToAsync(file);
    }

    return fullPath;
  }

  public async Task<Base64FileData> ReadAsBase64Async(string fullPath, CancellationToken token = default)
  {
    using (var file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
    {
      using (var memStream = new MemoryStream())
      {
        await file.CopyToAsync(memStream);
        var bytes = memStream.ToArray();
        if (!_mimeTypeProvider.TryGetMimeTypeByFilePath(fullPath, out var contentType))
        {
          contentType = "application/octet-stream";
        }

        string content = $"{contentType};base64,{Convert.ToBase64String(bytes)}";
        return new Base64FileData
        {
          Name = Path.GetFileName(fullPath),
          ContentType = contentType,
          Length = bytes.Length,
          Content = content
        };
      }
    }
  }

  public Task CopyFileAsync(string source, string destination, CancellationToken token = default)
  {
    File.Copy(source, destination, true);
    return Task.CompletedTask;
  }
}