using Centurion.Accounts.Core.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Centurion.Accounts.Core.Audit;

public class ChangeSetEntry : AuditableEntity<Guid>
{
  private ChangeSetEntry()
  {
  }

  public ChangeSetEntry(string entityId, string entityType, ChangeType changeType,
    Dictionary<string, string?> payload)
  {
    Id = Guid.NewGuid();
    EntityId = entityId;
    EntityType = entityType;
    ChangeType = changeType;
    Payload = payload;
  }

  public string EntityId { get; private set; } = null!;
  public string EntityType { get; private set; } = null!;
  public ChangeType ChangeType { get; private set; }
  public Dictionary<string, string?> Payload { get; private set; } = null!;

  public bool SameChangesAs(ChangeSetEntry other) => EntityId == other.EntityId
                                                     && EntityType == other.EntityType
                                                     && ChangeType == other.ChangeType;

  private static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
  {
    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
    NullValueHandling = NullValueHandling.Ignore,
    DateFormatHandling = DateFormatHandling.IsoDateFormat,
    DateParseHandling = DateParseHandling.DateTimeOffset,
    ContractResolver = new CamelCasePropertyNamesContractResolver()
  };
}