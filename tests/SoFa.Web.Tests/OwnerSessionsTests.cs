using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace SoFa.Web.Tests;

public sealed class OwnerSessionsTests
{
  [Fact]
  public async Task PollingDoesNotRenewTheIdleDeadline()
  {
    var clock = new TestClock();
    var sessions = new OwnerSessions(OwnerAdmissionTests.Profile(), clock);
    var key = await sessions.StoreAsync(Ticket(clock));
    clock.Advance(TimeSpan.FromMinutes(14));
    Assert.NotNull(await sessions.RetrieveAsync(key));
    clock.Advance(TimeSpan.FromMinutes(1));
    Assert.Null(await sessions.RetrieveAsync(key));
  }

  [Fact]
  public async Task ExplicitActivityCannotExtendTheOriginalAbsoluteDeadline()
  {
    var clock = new TestClock();
    var sessions = new OwnerSessions(OwnerAdmissionTests.Profile(), clock);
    var key = await sessions.StoreAsync(Ticket(clock));
    for (var i = 0; i < 23; i++)
    {
      clock.Advance(TimeSpan.FromMinutes(10));
      Assert.True(sessions.TouchUserActivity(key));
    }
    Assert.Equal(clock.Start.AddHours(4), sessions.Get(key)!.IdleDeadline);
    clock.Advance(TimeSpan.FromMinutes(10));
    Assert.Null(await sessions.RetrieveAsync(key));
    Assert.False(sessions.TouchUserActivity(key));
  }

  [Fact]
  public async Task TicketRenewalCannotMoveSessionDeadlinesOrIdentity()
  {
    var clock = new TestClock();
    var sessions = new OwnerSessions(OwnerAdmissionTests.Profile(), clock);
    var key = await sessions.StoreAsync(Ticket(clock));
    var ticket = (await sessions.RetrieveAsync(key))!;
    clock.Advance(TimeSpan.FromMinutes(14));
    ticket.Properties.ExpiresUtc = clock.GetUtcNow().AddYears(1);
    await sessions.RenewAsync(key, ticket);
    clock.Advance(TimeSpan.FromMinutes(1));
    Assert.Null(await sessions.RetrieveAsync(key));

    key = await sessions.StoreAsync(Ticket(clock));
    await sessions.RenewAsync(key, new AuthenticationTicket(OwnerAdmissionTests.Principal(subject: "another-owner"), "owner"));
    Assert.Null(await sessions.RetrieveAsync(key));
  }

  [Fact]
  public async Task ReferencesAreLocallyGeneratedAndLogoutDestroysCredentials()
  {
    var clock = new TestClock();
    var sessions = new OwnerSessions(OwnerAdmissionTests.Profile(), clock);
    var ticket = Ticket(clock);
    ((ClaimsIdentity)ticket.Principal.Identity!).AddClaim(new Claim(OwnerSessions.SessionClaim, "provider-chosen-reference"));
    ticket.Properties.StoreTokens(new[] { new AuthenticationToken { Name = "refresh_token", Value = "provider-refresh-test-value" } });
    var key = await sessions.StoreAsync(ticket);
    Assert.NotEqual("provider-chosen-reference", key);
    Assert.Equal(key, sessions.Get(ticket.Principal)!.Id);
    var record = sessions.Get(key)!;
    await sessions.RemoveAsync(key);
    Assert.Null(await sessions.RetrieveAsync(key));
    Assert.False(record.Active);
    Assert.Null(record.Ticket.Properties.GetTokenValue("refresh_token"));
    Assert.Null(new OwnerSessions(OwnerAdmissionTests.Profile(), clock).Get(key));
  }

  [Fact]
  public async Task InvalidOwnersAndCapacityExhaustionFailClosed()
  {
    var clock = new TestClock();
    var sessions = new OwnerSessions(OwnerAdmissionTests.Profile(), clock);
    await Assert.ThrowsAsync<InvalidOperationException>(() => sessions.StoreAsync(new AuthenticationTicket(OwnerAdmissionTests.Principal(subject: "denied"), "owner")));
    for (var i = 0; i < 32; i++) await sessions.StoreAsync(Ticket(clock));
    await Assert.ThrowsAsync<InvalidOperationException>(() => sessions.StoreAsync(Ticket(clock)));
    clock.Advance(TimeSpan.FromMinutes(15));
    Assert.NotNull(await sessions.StoreAsync(Ticket(clock)));
  }

  private static AuthenticationTicket Ticket(TestClock clock) => new(OwnerAdmissionTests.Principal(
      authenticationTime: clock.GetUtcNow().ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)), "owner");
}

internal sealed class TestClock(DateTimeOffset? start = null) : TimeProvider
{
  public DateTimeOffset Start { get; } = start ?? DateTimeOffset.FromUnixTimeSeconds(1_800_000_000);
  private TimeSpan elapsed;
  public override DateTimeOffset GetUtcNow() => Start + elapsed;
  public void Advance(TimeSpan duration) => elapsed += duration;
}
