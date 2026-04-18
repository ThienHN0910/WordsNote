using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WordsNote.Desktop.Models;

namespace WordsNote.Desktop.Services;

public sealed class WordsNoteApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient = new();
    private string _baseUrl = "http://localhost:3000";

    public string BaseUrl => _baseUrl;

    public void SetBaseUrl(string baseUrl)
    {
        var normalized = (baseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("API Base URL is required.");
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var parsed))
        {
            throw new InvalidOperationException("API Base URL is invalid.");
        }

        _baseUrl = parsed.ToString().TrimEnd('/');
    }

    public void SetToken(string? jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken.Trim());
    }

    public async Task<string> LoginWithGoogleTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new InvalidOperationException("Google ID token is required.");
        }

        var payload = new Dictionary<string, string>
        {
            ["idToken"] = idToken.Trim(),
        };

        var response = await _httpClient.PostAsJsonAsync(BuildUri("/api/auth/google"), payload, cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task<List<StudyDeck>> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<StudyDeck>>("/api/collections", cancellationToken) ?? [];
    }

    public async Task<StudyDeck> CreateCollectionAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        return await PostAsync<StudyDeck>("/api/collections", new { title, description }, cancellationToken);
    }

    public async Task<StudyDeck> UpdateCollectionAsync(string id, string title, string description, CancellationToken cancellationToken = default)
    {
        return await PutAsync<StudyDeck>($"/api/collections/{id}", new { title, description }, cancellationToken);
    }

    public async Task DeleteCollectionAsync(string id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"/api/collections/{id}", cancellationToken);
    }

    public async Task<List<StudyCard>> GetCardsAsync(string? collectionId = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(collectionId)
            ? "/api/cards"
            : $"/api/cards?collectionId={Uri.EscapeDataString(collectionId)}";
        return await GetAsync<List<StudyCard>>(path, cancellationToken) ?? [];
    }

    public async Task<StudyCard> CreateCardAsync(string collectionId, string front, string back, string hint, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        return await PostAsync<StudyCard>("/api/cards", new
        {
            collectionId,
            front,
            back,
            hint,
            tags,
        }, cancellationToken);
    }

    public async Task<StudyCard> UpdateCardAsync(string cardId, string collectionId, string front, string back, string hint, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        return await PutAsync<StudyCard>($"/api/cards/{cardId}", new
        {
            collectionId,
            front,
            back,
            hint,
            tags,
        }, cancellationToken);
    }

    public async Task DeleteCardAsync(string cardId, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"/api/cards/{cardId}", cancellationToken);
    }

    public async Task<ImportCardsResult> ImportCardsAsync(string collectionId, string rawText, CancellationToken cancellationToken = default)
    {
        return await PostAsync<ImportCardsResult>("/api/cards/import", new
        {
            collectionId,
            rawText,
        }, cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(BuildUri(path), cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(BuildUri(path), body, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task<T> PutAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync(BuildUri(path), body, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    private async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync(BuildUri(path), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(ExtractError(content, response.StatusCode));
    }

    private async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ExtractError(content, response.StatusCode));
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)content;
        }

        var value = JsonSerializer.Deserialize<T>(content, JsonOptions);
        if (value is null)
        {
            throw new InvalidOperationException("API returned empty response.");
        }

        return value;
    }

    private async Task<string> ReadTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ExtractError(content, response.StatusCode));
        }

        var token = ParseToken(content);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Authentication response did not contain a valid token.");
        }

        return token;
    }

    private static string ParseToken(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        try
        {
            if (content.TrimStart().StartsWith('{'))
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("token", out var tokenElement))
                {
                    return tokenElement.GetString() ?? string.Empty;
                }

                if (doc.RootElement.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.String)
                {
                    return dataElement.GetString() ?? string.Empty;
                }
            }

            var token = JsonSerializer.Deserialize<string>(content, JsonOptions);
            return token ?? string.Empty;
        }
        catch
        {
            return content.Trim('"', ' ', '\n', '\r', '\t');
        }
    }

    private static string ExtractError(string content, System.Net.HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"API request failed with status {(int)statusCode}.";
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                return errorElement.GetString() ?? $"API request failed with status {(int)statusCode}.";
            }

            if (doc.RootElement.TryGetProperty("Error", out var upperErrorElement))
            {
                return upperErrorElement.GetString() ?? $"API request failed with status {(int)statusCode}.";
            }
        }
        catch
        {
            // Ignore json parse and return raw content below.
        }

        var compact = content.Replace("\r", string.Empty).Replace("\n", " ").Trim();
        return string.IsNullOrWhiteSpace(compact)
            ? $"API request failed with status {(int)statusCode}."
            : compact;
    }

    private Uri BuildUri(string path)
    {
        return new($"{_baseUrl.TrimEnd('/')}{path}");
    }
}
