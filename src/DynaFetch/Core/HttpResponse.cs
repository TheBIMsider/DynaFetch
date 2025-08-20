using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DynaFetch.Utilities;

namespace DynaFetch.Core
{
  /// <summary>
  /// Wraps HttpResponseMessage to provide easy access to response data.
  /// This class makes it simple to get JSON data, check status codes,
  /// and handle errors in a Dynamo-friendly way.
  /// 
  /// Key features:
  /// - Easy JSON deserialization
  /// - Clear success/error checking
  /// - Automatic content reading
  /// - Helpful error messages
  /// </summary>
  public class HttpResponse : IDisposable
  {
    #region Private Fields

    private readonly HttpResponseMessage _response;
    private string? _content;
    private bool _contentRead = false;
    private bool _disposed = false;

    #endregion

    #region Public Properties

    /// <summary>
    /// HTTP status code (200, 404, 500, etc.)
    /// </summary>
    public HttpStatusCode StatusCode => _response.StatusCode;

    /// <summary>
    /// Numeric status code (200, 404, 500, etc.)
    /// Useful for Dynamo nodes that work better with numbers
    /// </summary>
    public int StatusCodeNumber => (int)_response.StatusCode;

    /// <summary>
    /// Status code description ("OK", "Not Found", "Internal Server Error", etc.)
    /// </summary>
    public string StatusDescription => _response.ReasonPhrase ?? StatusCode.ToString();

    /// <summary>
    /// True if the request was successful (status code 200-299)
    /// This is the main property to check if your API call worked
    /// </summary>
    public bool IsSuccess => _response.IsSuccessStatusCode;

    /// <summary>
    /// True if the request failed (status code outside 200-299)
    /// </summary>
    public bool IsError => !_response.IsSuccessStatusCode;

    /// <summary>
    /// Content-Type header from the response
    /// Tells you what kind of data you received (application/json, text/html, etc.)
    /// </summary>
    public string? ContentType => _response.Content.Headers.ContentType?.MediaType;

    /// <summary>
    /// Size of the response content in bytes
    /// Useful for checking if you got a reasonable response size
    /// </summary>
    public long? ContentLength => _response.Content.Headers.ContentLength;

    /// <summary>
    /// All response headers as a dictionary
    /// Useful for debugging or getting specific header values
    /// </summary>
    public Dictionary<string, string> Headers
    {
      get
      {
        var headers = new Dictionary<string, string>();

        // Add response headers
        foreach (var header in _response.Headers)
        {
          headers[header.Key] = string.Join(", ", header.Value);
        }

        // Add content headers
        foreach (var header in _response.Content.Headers)
        {
          headers[header.Key] = string.Join(", ", header.Value);
        }

        return headers;
      }
    }

    /// <summary>
    /// The original request URL
    /// Helpful for debugging which URL was actually called
    /// </summary>
    public string? RequestUrl => _response.RequestMessage?.RequestUri?.ToString();

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new HttpResponse wrapper
    /// This is typically called internally by HttpClientWrapper
    /// </summary>
    /// <param name="response">The HttpResponseMessage to wrap</param>
    public HttpResponse(HttpResponseMessage response)
    {
      _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    #endregion

    #region Content Reading Methods

    /// <summary>
    /// Gets the response content as a string
    /// This reads the raw response body - could be JSON, HTML, plain text, etc.
    /// </summary>
    /// <returns>Response content as string</returns>
    public async Task<string> GetContentAsync()
    {
      if (!_contentRead)
      {
        _content = await _response.Content.ReadAsStringAsync();
        _contentRead = true;
      }
      return _content ?? string.Empty;
    }

    /// <summary>
    /// Gets the response content as a string (synchronous version)
    /// Use this in Dynamo nodes where async isn't possible
    /// </summary>
    /// <returns>Response content as string</returns>
    public string GetContent()
    {
      return GetContentAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the response content as a byte array
    /// Useful for downloading files or binary data
    /// </summary>
    /// <returns>Response content as byte array</returns>
    public async Task<byte[]> GetBytesAsync()
    {
      return await _response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Gets the response content as a stream
    /// Useful for processing large responses without loading everything into memory
    /// </summary>
    /// <returns>Response content as stream</returns>
    public async Task<Stream> GetStreamAsync()
    {
      return await _response.Content.ReadAsStreamAsync();
    }

    #endregion

    #region JSON Methods

    /// <summary>
    /// Deserializes the JSON response to a specified type
    /// This is the main method for getting typed data from API responses
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <returns>Deserialized object of type T</returns>
    /// <exception cref="InvalidOperationException">Thrown if response is not JSON or deserialization fails</exception>
    public async Task<T> GetJsonAsync<T>()
    {
      var content = await GetContentAsync();

      if (string.IsNullOrWhiteSpace(content))
      {
        throw new DynaFetchJsonException("Response content is empty, cannot deserialize JSON", content);
      }

      try
      {
        return JsonHelper.DeserializeSmart<T>(content);
      }
      catch (DynaFetchJsonException)
      {
        // Re-throw DynaFetch exceptions as-is
        throw;
      }
      catch (Exception ex)
      {
        throw new DynaFetchJsonException($"Failed to deserialize JSON response: {ex.Message}", content, ex);
      }
    }

    /// <summary>
    /// Tries to deserialize JSON response to a specified type
    /// Returns success/failure instead of throwing exceptions
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <returns>Tuple with success flag and result (or default if failed)</returns>
    public async Task<(bool Success, T? Result, string Error)> TryGetJsonAsync<T>()
    {
      try
      {
        var content = await GetContentAsync();
        var (success, result, error) = JsonHelper.TryDeserialize<T>(content);
        return (success, result, error);
      }
      catch (Exception ex)
      {
        return (false, default(T), ex.Message);
      }
    }

    /// <summary>
    /// Gets the JSON response as a JsonDocument for flexible parsing
    /// Useful when you don't know the exact structure of the JSON
    /// </summary>
    /// <returns>JsonDocument for manual parsing</returns>
    public async Task<JsonDocument> GetJsonDocumentAsync()
    {
      var content = await GetContentAsync();

      if (string.IsNullOrWhiteSpace(content))
      {
        throw new InvalidOperationException("Response content is empty, cannot parse JSON");
      }

      try
      {
        return JsonDocument.Parse(content);
      }
      catch (JsonException ex)
      {
        throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Checks if the response content is valid JSON
    /// Useful for validating API responses before trying to parse them
    /// </summary>
    /// <returns>True if content is valid JSON</returns>
    public async Task<bool> IsJsonAsync()
    {
      try
      {
        var content = await GetContentAsync();
        if (string.IsNullOrWhiteSpace(content)) return false;

        using var document = JsonDocument.Parse(content);
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Converts JSON response to Dynamo-friendly Dictionary&lt;string, object&gt;.
    /// Perfect for Dynamo graphs that work with key-value pairs.
    /// Objects become dictionaries, arrays become lists, primitives stay as-is.
    /// </summary>
    /// <returns>Dictionary representation of JSON, or null if conversion fails</returns>
    public async Task<Dictionary<string, object>?> GetJsonAsDictionaryAsync()
    {
      try
      {
        var content = await GetContentAsync();
        return JsonHelper.JsonToDictionary(content);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    /// Converts JSON array response to Dynamo-friendly List&lt;object&gt;.
    /// Perfect for processing arrays of data in Dynamo.
    /// Objects become dictionaries, arrays become lists, primitives stay as-is.
    /// </summary>
    /// <returns>List representation of JSON array, or null if conversion fails</returns>
    public async Task<List<object>?> GetJsonAsListAsync()
    {
      try
      {
        var content = await GetContentAsync();
        return JsonHelper.JsonToList(content);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    /// Converts JSON response to the most appropriate Dynamo-friendly type.
    /// - Objects become Dictionary&lt;string, object&gt;
    /// - Arrays become List&lt;object&gt;
    /// - Primitives (string, number, bool) stay as-is
    /// This is the "smart" method that figures out what you probably want.
    /// </summary>
    /// <returns>Dynamo-friendly representation of the JSON</returns>
    public async Task<object> GetJsonAsObjectAsync()
    {
      try
      {
        var content = await GetContentAsync();
        return JsonHelper.JsonToObject(content);
      }
      catch (Exception)
      {
        return await GetContentAsync(); // Return raw content if JSON parsing fails
      }
    }

    /// <summary>
    /// Safely gets JSON as Dictionary with success/failure information.
    /// Perfect for Dynamo graphs that need to handle errors gracefully.
    /// </summary>
    /// <returns>Tuple with success flag, dictionary result, and error message</returns>
    public async Task<(bool Success, Dictionary<string, object>? Result, string Error)> TryGetJsonAsDictionaryAsync()
    {
      try
      {
        var content = await GetContentAsync();
        var result = JsonHelper.JsonToDictionary(content);

        if (result == null)
        {
          return (false, null, "Failed to convert JSON to dictionary - content may not be a JSON object");
        }

        return (true, result, string.Empty);
      }
      catch (Exception ex)
      {
        return (false, null, ex.Message);
      }
    }

    /// <summary>
    /// Safely gets JSON as List with success/failure information.
    /// Perfect for Dynamo graphs that need to handle errors gracefully.
    /// </summary>
    /// <returns>Tuple with success flag, list result, and error message</returns>
    public async Task<(bool Success, List<object>? Result, string Error)> TryGetJsonAsListAsync()
    {
      try
      {
        var content = await GetContentAsync();
        var result = JsonHelper.JsonToList(content);

        if (result == null)
        {
          return (false, null, "Failed to convert JSON to list - content may not be a JSON array");
        }

        return (true, result, string.Empty);
      }
      catch (Exception ex)
      {
        return (false, null, ex.Message);
      }
    }

    /// <summary>
    /// Formats the JSON response with proper indentation for display.
    /// Great for making JSON readable in Dynamo outputs or debugging.
    /// </summary>
    /// <returns>Formatted JSON string, or original content if formatting fails</returns>
    public async Task<string> GetFormattedJsonAsync()
    {
      try
      {
        var content = await GetContentAsync();
        return JsonHelper.FormatJson(content);
      }
      catch (Exception)
      {
        return await GetContentAsync();
      }
    }

    /// <summary>
    /// Validates that the response content is valid JSON.
    /// Better than IsJsonAsync because it uses our comprehensive validation.
    /// </summary>
    /// <returns>True if content is valid JSON, false otherwise</returns>
    public async Task<bool> IsValidJsonAsync()
    {
      try
      {
        var content = await GetContentAsync();
        return JsonHelper.IsValidJson(content);
      }
      catch (Exception)
      {
        return false;
      }
    }

    #endregion

    #region Error Handling Methods

    /// <summary>
    /// Throws an exception if the response indicates an error
    /// Use this to fail fast when API calls don't succeed
    /// </summary>
    /// <exception cref="HttpRequestException">Thrown if response indicates error</exception>
    public async Task EnsureSuccessAsync()
    {
      if (IsSuccess) return;

      var content = await GetContentAsync();
      var errorMessage = $"HTTP {StatusCodeNumber} {StatusDescription}";

      if (!string.IsNullOrWhiteSpace(content))
      {
        errorMessage += $": {content}";
      }

      throw new HttpRequestException(errorMessage);
    }

    /// <summary>
    /// Gets error details if the response indicates an error
    /// Returns null if the response was successful
    /// </summary>
    /// <returns>Error details or null if successful</returns>
    public async Task<ErrorDetails?> GetErrorDetailsAsync()
    {
      if (IsSuccess) return null;

      var content = await GetContentAsync();

      return new ErrorDetails
      {
        StatusCode = StatusCodeNumber,
        StatusDescription = StatusDescription,
        Content = content,
        RequestUrl = RequestUrl
      };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets a specific header value
    /// Returns null if the header doesn't exist
    /// </summary>
    /// <param name="headerName">Name of the header to get</param>
    /// <returns>Header value or null</returns>
    public string? GetHeader(string headerName)
    {
      if (string.IsNullOrWhiteSpace(headerName)) return null;

      var headers = Headers;
      return headers.TryGetValue(headerName, out var value) ? value : null;
    }

    /// <summary>
    /// Checks if a specific header exists
    /// </summary>
    /// <param name="headerName">Name of the header to check</param>
    /// <returns>True if header exists</returns>
    public bool HasHeader(string headerName)
    {
      return !string.IsNullOrWhiteSpace(GetHeader(headerName));
    }

    #endregion

    #region Display Methods

    /// <summary>
    /// Returns a summary of the response for debugging/logging
    /// Shows status, content type, size, and URL
    /// </summary>
    /// <returns>Human-readable response summary</returns>
    public override string ToString()
    {
      var summary = $"{StatusCodeNumber} {StatusDescription}";

      if (!string.IsNullOrWhiteSpace(ContentType))
      {
        summary += $" | {ContentType}";
      }

      if (ContentLength.HasValue)
      {
        summary += $" | {ContentLength} bytes";
      }

      if (!string.IsNullOrWhiteSpace(RequestUrl))
      {
        summary += $" | {RequestUrl}";
      }

      return summary;
    }

    /// <summary>
    /// Gets detailed response information for debugging
    /// Includes headers, content preview, etc.
    /// </summary>
    /// <returns>Detailed response information</returns>
    public async Task<string> GetDetailedInfoAsync()
    {
      var info = new System.Text.StringBuilder();
      info.AppendLine($"Status: {StatusCodeNumber} {StatusDescription}");
      info.AppendLine($"Success: {IsSuccess}");
      info.AppendLine($"Content-Type: {ContentType ?? "Unknown"}");
      info.AppendLine($"Content-Length: {ContentLength?.ToString() ?? "Unknown"}");
      info.AppendLine($"Request URL: {RequestUrl ?? "Unknown"}");

      info.AppendLine("\nHeaders:");
      foreach (var header in Headers)
      {
        info.AppendLine($"  {header.Key}: {header.Value}");
      }

      var content = await GetContentAsync();
      if (!string.IsNullOrWhiteSpace(content))
      {
        info.AppendLine("\nContent Preview:");
        var preview = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
        info.AppendLine(preview);
      }

      return info.ToString();
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Disposes of the underlying HttpResponseMessage
    /// Important for freeing up resources properly
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method for proper cleanup
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed && disposing)
      {
        _response?.Dispose();
        _disposed = true;
      }
    }

    #endregion
  }

  /// <summary>
  /// Contains error details for failed HTTP responses
  /// Makes it easy to understand what went wrong with an API call
  /// </summary>
  public class ErrorDetails
  {
    /// <summary>
    /// HTTP status code number (404, 500, etc.)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Status code description ("Not Found", "Internal Server Error", etc.)
    /// </summary>
    public string StatusDescription { get; set; } = string.Empty;

    /// <summary>
    /// Response content (often contains error message from the API)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The URL that was requested
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// Returns a formatted error message
    /// </summary>
    public override string ToString()
    {
      var message = $"HTTP {StatusCode} {StatusDescription}";

      if (!string.IsNullOrWhiteSpace(Content))
      {
        message += $": {Content}";
      }

      if (!string.IsNullOrWhiteSpace(RequestUrl))
      {
        message += $" (URL: {RequestUrl})";
      }

      return message;
    }
  }
}