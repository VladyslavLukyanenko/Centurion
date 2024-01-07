namespace Centurion.Monitor.Domain.Services;

public interface IJsonSerializer
{
  ValueTask<T?> DeserializeAsync<T>(string json, CancellationToken ct = default);
  ValueTask<T?> DeserializeAsync<T>(Stream jsonStream, CancellationToken ct = default);
  ValueTask<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> inMemoryJson, CancellationToken ct = default);
  T? Deserialize<T>(string json);

  ValueTask<string> SerializeAsync<T>(T obj, CancellationToken ct = default);
  string Serialize<T>(T obj);
  ValueTask SerializeAsync<T>(Stream utf8Stream, T obj, CancellationToken ct = default);
}