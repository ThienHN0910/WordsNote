using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Options;
using WordsNote.Desktop.Models;
using WordsNote.Desktop.Services.Configuration;
using WordsNote.Desktop.Services.Serialization;

namespace WordsNote.Desktop.Services;

public sealed class WordsNoteApiClient
{
    private readonly HttpClient _httpClient;
    private string _baseUrl = string.Empty;

    public WordsNoteApiClient(HttpClient httpClient, IOptions<DesktopRuntimeOptions> runtimeOptions)
    {
        _httpClient = httpClient;
        SetBaseUrl(runtimeOptions.Value.ApiBaseUrl);
    }

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

        var payload = new GoogleAuthTokenRequest
        {
            IdToken = idToken.Trim(),
        };

        var response = await _httpClient.PostAsJsonAsync(
            BuildUri("/api/auth/google"),
            payload,
            WordsNoteJsonSerializerContext.Default.GoogleAuthTokenRequest,
            cancellationToken);
        return await ReadTokenResponseAsync(response, cancellationToken);
    }

    public async Task<List<StudyDeck>> GetCollectionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync("/api/collections", WordsNoteJsonSerializerContext.Default.ListStudyDeck, cancellationToken) ?? [];
    }

    public async Task<StudyDeck> CreateCollectionAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var payload = new CollectionUpsertRequest
        {
            Title = title,
            Description = description,
        };

        return await PostAsync(
            "/api/collections",
            payload,
            WordsNoteJsonSerializerContext.Default.CollectionUpsertRequest,
            WordsNoteJsonSerializerContext.Default.StudyDeck,
            cancellationToken);
    }

    public async Task<StudyDeck> UpdateCollectionAsync(string id, string title, string description, CancellationToken cancellationToken = default)
    {
        var payload = new CollectionUpsertRequest
        {
            Title = title,
            Description = description,
        };

        return await PutAsync(
            $"/api/collections/{id}",
            payload,
            WordsNoteJsonSerializerContext.Default.CollectionUpsertRequest,
            WordsNoteJsonSerializerContext.Default.StudyDeck,
            cancellationToken);
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
        return await GetAsync(path, WordsNoteJsonSerializerContext.Default.ListStudyCard, cancellationToken) ?? [];
    }

    public async Task<StudyCard> CreateCardAsync(string collectionId, string front, string back, string hint, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        var payload = new CardUpsertRequest
        {
            CollectionId = collectionId,
            Front = front,
            Back = back,
            Hint = hint,
            Tags = [.. tags],
        };

        return await PostAsync(
            "/api/cards",
            payload,
            WordsNoteJsonSerializerContext.Default.CardUpsertRequest,
            WordsNoteJsonSerializerContext.Default.StudyCard,
            cancellationToken);
    }

    public async Task<StudyCard> UpdateCardAsync(string cardId, string collectionId, string front, string back, string hint, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        var payload = new CardUpsertRequest
        {
            CollectionId = collectionId,
            Front = front,
            Back = back,
            Hint = hint,
            Tags = [.. tags],
        };

        return await PutAsync(
            $"/api/cards/{cardId}",
            payload,
            WordsNoteJsonSerializerContext.Default.CardUpsertRequest,
            WordsNoteJsonSerializerContext.Default.StudyCard,
            cancellationToken);
    }

    public async Task DeleteCardAsync(string cardId, CancellationToken cancellationToken = default)
    {
        await DeleteAsync($"/api/cards/{cardId}", cancellationToken);
    }

    public async Task<ImportCardsResult> ImportCardsAsync(string collectionId, string rawText, CancellationToken cancellationToken = default)
    {
        var payload = new CardsImportRequest
        {
            CollectionId = collectionId,
            RawText = rawText,
        };

        return await PostAsync(
            "/api/cards/import",
            payload,
            WordsNoteJsonSerializerContext.Default.CardsImportRequest,
            WordsNoteJsonSerializerContext.Default.ImportCardsResult,
            cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, JsonTypeInfo<T> responseTypeInfo, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(BuildUri(path), cancellationToken);
        return await ReadResponseAsync(response, responseTypeInfo, cancellationToken);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(BuildUri(path), body, requestTypeInfo, cancellationToken);
        return await ReadResponseAsync(response, responseTypeInfo, cancellationToken);
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(
        string path,
        TRequest body,
        JsonTypeInfo<TRequest> requestTypeInfo,
        JsonTypeInfo<TResponse> responseTypeInfo,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync(BuildUri(path), body, requestTypeInfo, cancellationToken);
        return await ReadResponseAsync(response, responseTypeInfo, cancellationToken);
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

    private async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response,
        JsonTypeInfo<T> responseTypeInfo,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ExtractError(content, response.StatusCode));
        }

        var value = JsonSerializer.Deserialize(content, responseTypeInfo);
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

            return content.Trim('"', ' ', '\n', '\r', '\t');
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

internal sealed class GoogleAuthTokenRequest
{
    public string IdToken { get; set; } = string.Empty;
}

internal sealed class CollectionUpsertRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

internal sealed class CardUpsertRequest
{
    public string CollectionId { get; set; } = string.Empty;

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string Hint { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];
}

internal sealed class CardsImportRequest
{
    public string CollectionId { get; set; } = string.Empty;

    public string RawText { get; set; } = string.Empty;
}
