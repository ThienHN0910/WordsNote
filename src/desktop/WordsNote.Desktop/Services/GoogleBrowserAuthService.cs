using System.Diagnostics;
using System.IO;
using System.Net;

namespace WordsNote.Desktop.Services;

public sealed class GoogleBrowserAuthService
{
    private const string RedirectUri = "http://127.0.0.1:53682/oauth2/callback/";

    public async Task<string> AcquireIdTokenAsync(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Google Client ID is required for browser login.");
        }

        var state = Guid.NewGuid().ToString("N");
        var nonce = Guid.NewGuid().ToString("N");

        using var listener = new HttpListener();
        listener.Prefixes.Add(RedirectUri);
        listener.Start();

        var authUrl = BuildAuthorizeUrl(clientId.Trim(), RedirectUri, state, nonce);
        OpenBrowser(authUrl);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMinutes(3));

        while (true)
        {
            var context = await listener.GetContextAsync().WaitAsync(timeoutCts.Token);
            if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                await WriteHtmlResponseAsync(context.Response,
                    "Google login callback reached. You can close this tab and return to the desktop app.");
                continue;
            }

            var form = await ReadFormAsync(context.Request);
            var callbackState = form.TryGetValue("state", out var postedState) ? postedState : string.Empty;
            if (!string.Equals(state, callbackState, StringComparison.Ordinal))
            {
                await WriteHtmlResponseAsync(context.Response,
                    "State mismatch. Please close this tab and retry login.");
                throw new InvalidOperationException("Google login failed: state mismatch.");
            }

            if (!form.TryGetValue("id_token", out var idToken) || string.IsNullOrWhiteSpace(idToken))
            {
                await WriteHtmlResponseAsync(context.Response,
                    "ID token not found. Please close this tab and retry login.");
                throw new InvalidOperationException("Google login failed: id_token missing.");
            }

            await WriteHtmlResponseAsync(context.Response,
                "Login completed. You can close this tab and return to the desktop app.");
            return idToken;
        }
    }

    private static string BuildAuthorizeUrl(string clientId, string redirectUri, string state, string nonce)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "id_token",
            ["scope"] = "openid email profile",
            ["state"] = state,
            ["nonce"] = nonce,
            ["response_mode"] = "form_post",
            ["prompt"] = "select_account",
        };

        var queryString = string.Join("&", query.Select(item =>
            $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));

        return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
    }

    private static async Task<Dictionary<string, string>> ReadFormAsync(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = segment.Split('=', 2);
            var key = Uri.UnescapeDataString(pair[0].Replace('+', ' '));
            var value = pair.Length > 1
                ? Uri.UnescapeDataString(pair[1].Replace('+', ' '))
                : string.Empty;
            result[key] = value;
        }

        return result;
    }

    private static async Task WriteHtmlResponseAsync(HttpListenerResponse response, string message)
    {
        response.StatusCode = 200;
        response.ContentType = "text/html; charset=utf-8";
        var html = $"<html><body style='font-family:Segoe UI,Arial,sans-serif;padding:24px;'><h2>{WebUtility.HtmlEncode(message)}</h2></body></html>";
        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
        response.OutputStream.Close();
    }

    private static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
        });
    }
}
