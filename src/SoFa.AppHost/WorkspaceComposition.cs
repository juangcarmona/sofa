using JuanGCarmona.SoFa.Modules.Workspace;

namespace JuanGCarmona.SoFa.AppHost;

internal static class WorkspaceComposition
{
  internal static void Add(IDistributedApplicationBuilder builder, string configurationFile)
  {
    WorkspaceModule.ValidateConfiguration(builder.Configuration);
    var providerUser = builder.Configuration["SoFa:Identity:Development:ProviderUser"] ?? "1000:0";
    if (builder.Configuration["SoFa:Identity:Mode"] == "Development"
        && (!System.Text.RegularExpressions.Regex.IsMatch(providerUser, @"^[1-9][0-9]{0,9}:0$")
            || !uint.TryParse(providerUser.AsSpan(0, providerUser.Length - 2), out var uid) || uid == uint.MaxValue))
      throw new InvalidOperationException("Development provider requires a non-root private-file owner UID and image group 0.");
    var origin = new Uri(builder.Configuration["SoFa:Origin"]!);
    var workspace = builder.AddProject<Projects.SoFa_Web>("workspace", launchProfileName: null)
        .WithEnvironment("SOFA_CONFIG_FILE", configurationFile)
        .WithUrl(origin.AbsoluteUri, "SoFa");
    if (builder.Configuration["SoFa:Identity:Mode"] != "Development") return;

    var directory = Path.GetDirectoryName(configurationFile)!;
    var issuer = new Uri(builder.Configuration["SoFa:Identity:Development:Issuer"]!);
    var environment = File.ReadAllLines(Path.Combine(directory, "provider.env"))
        .Where(line => !string.IsNullOrWhiteSpace(line)).Select(line => line.Split('=', 2))
        .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);
    var adminPassword = builder.AddParameter("development-admin-password",
        () => environment["KC_BOOTSTRAP_ADMIN_PASSWORD"], secret: true);
    var identity = builder.AddContainer("development-identity", "quay.io/keycloak/keycloak", "26.8.1")
        .WithImageSHA256("a78d6b79572168cb25886b80a8b5b8d4ed4390743e3eecb810538683e209dd6b")
        .WithArgs("start-dev", "--import-realm", "--http-enabled=false", "--hostname-strict=true")
        .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", environment["KC_BOOTSTRAP_ADMIN_USERNAME"])
        .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", adminPassword)
        .WithEnvironment("KC_HOSTNAME", issuer.GetLeftPart(UriPartial.Authority))
        .WithEnvironment("KC_HTTP_ENABLED", "false")
        .WithEnvironment("KC_HTTPS_CERTIFICATE_FILE", "/run/sofa-tls/identity.crt")
        .WithEnvironment("KC_HTTPS_CERTIFICATE_KEY_FILE", "/run/sofa-tls/identity.key")
        .WithEnvironment("KC_LOG_LEVEL", "warn")
        .WithBindMount(Path.Combine(directory, "tls"), "/run/sofa-tls", isReadOnly: true)
        .WithBindMount(Path.Combine(directory, "import"), "/opt/keycloak/data/import", isReadOnly: true)
        .WithContainerRuntimeArgs("--user", providerUser, "--publish", issuer.Host + ":" + issuer.Port.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":8443",
                "--cpus", "2", "--memory", "1g", "--pids-limit", "512", "--cap-drop", "ALL", "--security-opt", "no-new-privileges",
                "--label", "sofa.scope=issue-49-development")
        .WithUrl(issuer.GetLeftPart(UriPartial.Authority), "Development identity");
    workspace.WithEnvironment("SSL_CERT_FILE", Path.Combine(directory, "trust-bundle.pem")).WaitFor(identity);
  }
}
