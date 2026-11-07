using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using JuanGCarmona.SoFa.AppHost;
using Microsoft.Extensions.Configuration;

namespace SoFa.Web.Tests;

public sealed class WorkspaceCompositionTests
{
  [Fact]
  public void ExternalProfileComposesOnlyTheWorkspaceWithoutReadingDevelopmentProviderFiles()
  {
    var builder = Builder("External");
    WorkspaceComposition.Add(builder, Path.Combine(Path.GetTempPath(), "unprovisioned-identity", "external.json"));
    var resource = Assert.Single(builder.Resources.OfType<ProjectResource>());
    Assert.Equal("workspace", resource.Name);
    Assert.Equal(2, builder.Resources.Count);
    Assert.Equal(resource, Assert.Single(builder.Resources.OfType<IResourceWithParent>()).Parent);
    Assert.Empty(builder.Resources.OfType<ContainerResource>());
  }

  [Fact]
  public void DevelopmentProfileComposesThePinnedIsolatedProviderAndWorkspaceWithoutExecutionResources()
  {
    var directory = Path.Combine(Path.GetTempPath(), "sofa-composition-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    Directory.CreateDirectory(Path.Combine(directory, "tls"));
    Directory.CreateDirectory(Path.Combine(directory, "import"));
    File.WriteAllText(Path.Combine(directory, "provider.env"), "KC_BOOTSTRAP_ADMIN_USERNAME=development-admin\nKC_BOOTSTRAP_ADMIN_PASSWORD=isolated-test-admin-value\n");
    try
    {
      var builder = Builder("Development");
      WorkspaceComposition.Add(builder, Path.Combine(directory, "development.json"));
      Assert.Equal(4, builder.Resources.Count);
      var workspace = Assert.Single(builder.Resources.OfType<ProjectResource>());
      Assert.Equal(workspace, Assert.Single(builder.Resources.OfType<IResourceWithParent>()).Parent);
      var identity = Assert.Single(builder.Resources.OfType<ContainerResource>());
      Assert.Equal("development-identity", identity.Name);
      var image = Assert.Single(identity.Annotations.OfType<ContainerImageAnnotation>());
      // Aspire selects the immutable digest instead of retaining a mutable tag selector.
      Assert.Null(image.Tag);
      Assert.Equal("a78d6b79572168cb25886b80a8b5b8d4ed4390743e3eecb810538683e209dd6b", image.SHA256);
      Assert.True(Assert.Single(builder.Resources.OfType<ParameterResource>()).Secret);
    }
    finally { Directory.Delete(directory, recursive: true); }
  }

  [Theory]
  [InlineData("0:0")]
  [InlineData("1001:1001")]
  [InlineData("4294967295:0")]
  [InlineData("1001:0 --privileged")]
  public void UnsafeProviderUsersFailBeforeResourceComposition(string user)
  {
    var builder = Builder("Development");
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    { ["SoFa:Identity:Development:ProviderUser"] = user });
    Assert.Throws<InvalidOperationException>(() => WorkspaceComposition.Add(builder, "/unused/private/development.json"));
    Assert.Empty(builder.Resources);
  }

  [Fact]
  public void MixedProfilesFailBeforeAnyResourceIsComposed()
  {
    var builder = Builder("Development");
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["SoFa:Identity:External:Issuer"] = "https://other.invalid" });
    Assert.Throws<InvalidOperationException>(() => WorkspaceComposition.Add(builder, "/unused/private/development.json"));
    Assert.Empty(builder.Resources);
  }

  private static IDistributedApplicationBuilder Builder(string mode)
  {
    var builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
    { Args = [], DisableDashboard = true, AssemblyName = typeof(JuanGCarmona.SoFa.AppHost.Program).Assembly.GetName().Name });
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["SoFa:Origin"] = "https://workspace.invalid",
      ["SoFa:Identity:Mode"] = mode,
      ["SoFa:Identity:" + mode + ":Issuer"] = "https://identity.invalid/realms/dev",
      ["SoFa:Identity:" + mode + ":ClientId"] = "sofa-development",
      ["SoFa:Identity:" + mode + ":ClientSecret"] = "isolated-client-value",
      ["SoFa:Identity:" + mode + ":OwnerSubject"] = "owner-subject",
      ["SoFa:Repositories:0"] = "example/repository",
    });
    return builder;
  }
}
