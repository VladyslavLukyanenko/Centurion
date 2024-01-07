using System.Collections.Concurrent;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

namespace Centurion.Cli.Core.Services;

public class AudioService : IAudioService
{
  private readonly ConcurrentDictionary<string, Lazy<Task<WasapiOut>>> _audioFilesCache = new();

  public async ValueTask Play(string fileName)
  {
    var soundOut = await _audioFilesCache.GetOrAdd(fileName, ReadFileContent).Value;

    soundOut.Stop();
    soundOut.WaveSource.Position = 0;
    soundOut.Play();
  }

  private static Lazy<Task<WasapiOut>> ReadFileContent(string path) => new(Task.Run(async () =>
  {
    using var mmdeviceEnumerator = new MMDeviceEnumerator();
    var device = mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    var waveSource =
      CodecFactory.Instance.GetCodec(path)
        .ToSampleSource()
        .ToWaveSource();
    var soundOut = new WasapiOut {Latency = 100, Device = device};
    soundOut.Initialize(waveSource);

    return soundOut;
  }));
}