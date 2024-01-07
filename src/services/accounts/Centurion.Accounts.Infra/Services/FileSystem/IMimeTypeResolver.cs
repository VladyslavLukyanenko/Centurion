namespace Centurion.Accounts.Infra.Services.FileSystem;

public interface IMimeTypeResolver
{
  bool TryGetFileExtension(string mimeType, out string extension);
  bool TryGetMimeTypeByFilePath(string fullPath, out string contentType);
  bool IsImage(string contentType);
}