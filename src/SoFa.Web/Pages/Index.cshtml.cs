using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoFa.Attention;

namespace SoFa.Web.Pages;

public sealed class IndexModel(AttentionService attention, ILogger<IndexModel> logger) : PageModel
{
  public AttentionSnapshot? Snapshot => attention.Current;
  public string? Error => attention.LastError;
  public bool Refreshing => attention.IsRefreshing;
  public void OnGet() { }
  public async Task<IActionResult> OnPostRefreshAsync()
  {
    try
    {
      var snapshot = await attention.RefreshAsync();
      logger.LogInformation("Attention refresh completed for {RepositoryCount} repositories", snapshot.Repositories.Count);
    }
    catch (Exception)
    {
      // Do not include transport exceptions or arbitrary source strings in logs.
      logger.LogWarning("Attention refresh did not complete");
    }
    return RedirectToPage();
  }
  public static string? SafeLink(string url) => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
      uri.Scheme == "https" && uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
      uri.UserInfo.Length == 0 && uri.Port == 443 ? uri.AbsoluteUri : null;
}
