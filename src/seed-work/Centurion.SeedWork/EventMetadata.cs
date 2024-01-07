using Google.Protobuf.WellKnownTypes;

// ReSharper disable once CheckNamespace
namespace Centurion.Contracts.Integration;

public partial class EventMetadata
{
  partial void OnConstruction()
  {
    Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow);
  }
}