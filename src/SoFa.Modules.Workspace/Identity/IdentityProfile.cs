using System.Text.Json.Serialization;

namespace JuanGCarmona.SoFa.Modules.Workspace.Identity;

internal sealed class IdentityProfile
{
  public required Uri Issuer { get; init; }
  public required string ClientId { get; init; }
  [JsonIgnore]
  public string ClientSecret { get; init; } = string.Empty;
  public required string OwnerSubject { get; init; }

  public override string ToString() => "IdentityProfile (credentials withheld)";
}
