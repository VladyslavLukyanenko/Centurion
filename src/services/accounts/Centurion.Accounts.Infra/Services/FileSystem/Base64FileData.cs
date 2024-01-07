namespace Centurion.Accounts.Infra.Services.FileSystem;

public class Base64FileData
  : BaseBinaryData
{
  public string Content { get; set; } = null!;

  public override Stream OpenReadStream()
  {
    var endmimeIndex = Content.IndexOf(';');
    if (endmimeIndex != -1)
    {
      endmimeIndex += ";base64,".Length;
    }
    else
    {
      endmimeIndex = 0;
    }

    string onlyBase64 = Content.Substring(endmimeIndex).Trim();
    byte[] bytes = Convert.FromBase64String(onlyBase64);
    return new MemoryStream(bytes);
  }
}