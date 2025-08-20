using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DynaFetch.Core
{
  /// <summary>
  /// Wraps HttpClient with DynaFetch-specific configuration and settings.
  /// This is the main class that handles all HTTP communication.
  /// 
  /// Why this class exists:
  /// - Provides a clean, simple interface for Dynamo nodes
  /// - Manages HttpClient lifecycle properly (avoids socket exhaustion)
  /// - Centralizes all HTTP configuration (timeouts, headers, etc.)
  /// - Makes testing and mocking easier
  /// </summary>
  public class HttpClientWrapper : IDisposable
  {
    #region Private Fields

    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    #endregion

    #region Public Properties

    /// <summary>
    /// The base URL for all requests (e.g., "https://api.example.com/")
    /// When set, all relative URLs will be combined with this base URL
    /// </summary>
    public string? BaseUrl
    {
      get => _httpClient.BaseAddress?.ToString();
      set => _httpClient.BaseAddress = string.IsNullOrEmpty(value) ? null : new Uri(value);
    }

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// Default is 30 seconds, which works for most API calls
    /// </summary>
    public int TimeoutSeconds
    {
      get => (int)_httpClient.Timeout.TotalSeconds;
      set => _httpClient.Timeout = TimeSpan.FromSeconds(value);
    }

    /// <summary>
    /// User-Agent string sent with requests
    /// Helps APIs identify your application
    /// </summary>
    public string UserAgent
    {
      get => _httpClient.DefaultRequestHeaders.UserAgent.ToString();
      set
      {
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        if (!string.IsNullOrEmpty(value))
        {
          _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(value);
        }
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new HttpClientWrapper with default settings
    /// 
    /// Default settings:
    /// - 30 second timeout
    /// - DynaFetch user agent
    /// - No base URL (must specify full URLs)
    /// </summary>
    public HttpClientWrapper()
    {
      _httpClient = new HttpClient();

      // Set sensible defaults
      TimeoutSeconds = 30;
      UserAgent = "DynaFetch/1.0";
    }

    /// <summary>
    /// Creates a new HttpClientWrapper with a base URL
    /// All requests will be relative to this base URL
    /// </summary>
    /// <param name="baseUrl">Base URL like "https://api.example.com/"</param>
    public HttpClientWrapper(string baseUrl) : this()
    {
      BaseUrl = baseUrl;
    }

    #endregion

    #region Header Management

    /// <summary>
    /// Adds or updates a default header that will be sent with every request
    /// Common headers: Authorization, Accept, Content-Type
    /// </summary>
    /// <param name="name">Header name (e.g., "Authorization")</param>
    /// <param name="value">Header value (e.g., "Bearer your-token-here")</param>
    public void SetDefaultHeader(string name, string value)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException("Header name cannot be empty", nameof(name));

      // Remove existing header if it exists
      _httpClient.DefaultRequestHeaders.Remove(name);

      // Add the new header
      _httpClient.DefaultRequestHeaders.Add(name, value);
    }

    /// <summary>
    /// Removes a default header
    /// </summary>
    /// <param name="name">Header name to remove</param>
    public void RemoveDefaultHeader(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        _httpClient.DefaultRequestHeaders.Remove(name);
      }
    }

    /// <summary>
    /// Sets multiple headers at once from a dictionary
    /// Useful for setting up authentication and other common headers
    /// </summary>
    /// <param name="headers">Dictionary of header name/value pairs</param>
    public void SetDefaultHeaders(Dictionary<string, string> headers)
    {
      if (headers == null) return;

      foreach (var header in headers)
      {
        SetDefaultHeader(header.Key, header.Value);
      }
    }

    /// <summary>
    /// Gets all current default headers as a dictionary
    /// Useful for debugging or displaying current configuration
    /// </summary>
    /// <returns>Dictionary of current headers</returns>
    public Dictionary<string, string> GetDefaultHeaders()
    {
      var headers = new Dictionary<string, string>();

      foreach (var header in _httpClient.DefaultRequestHeaders)
      {
        headers[header.Key] = string.Join(", ", header.Value);
      }

      return headers;
    }

    #endregion

    #region HTTP Methods

    /// <summary>
    /// Performs an HTTP GET request
    /// This is the most common HTTP method - used to retrieve data
    /// </summary>
    /// <param name="url">URL to request (can be relative if BaseUrl is set)</param>
    /// <returns>HttpResponseMessage with the server's response</returns>
    public async Task<HttpResponseMessage> GetAsync(string url)
    {
      ValidateUrl(url);
      return await _httpClient.GetAsync(url);
    }

    /// <summary>
    /// Performs an HTTP POST request with content
    /// Used to send data to the server (create new resources)
    /// </summary>
    /// <param name="url">URL to post to</param>
    /// <param name="content">Content to send in the request body</param>
    /// <returns>HttpResponseMessage with the server's response</returns>
    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
      ValidateUrl(url);
      return await _httpClient.PostAsync(url, content);
    }

    /// <summary>
    /// Performs an HTTP PUT request with content
    /// Used to update/replace existing resources
    /// </summary>
    /// <param name="url">URL to put to</param>
    /// <param name="content">Content to send in the request body</param>
    /// <returns>HttpResponseMessage with the server's response</returns>
    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
    {
      ValidateUrl(url);
      return await _httpClient.PutAsync(url, content);
    }

    /// <summary>
    /// Performs an HTTP DELETE request
    /// Used to delete resources from the server
    /// </summary>
    /// <param name="url">URL to delete</param>
    /// <returns>HttpResponseMessage with the server's response</returns>
    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
      ValidateUrl(url);
      return await _httpClient.DeleteAsync(url);
    }

    /// <summary>
    /// Performs an HTTP PATCH request with content
    /// Used to partially update existing resources
    /// </summary>
    /// <param name="url">URL to patch</param>
    /// <param name="content">Content to send in the request body</param>
    /// <returns>HttpResponseMessage with the server's response</returns>
    public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
    {
      ValidateUrl(url);

      var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
      {
        Content = content
      };

      return await _httpClient.SendAsync(request);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates that a URL is not null or empty
    /// Throws a clear exception if the URL is invalid
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <exception cref="ArgumentException">Thrown if URL is null or empty</exception>
    private static void ValidateUrl(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
      {
        throw new ArgumentException("URL cannot be null or empty", nameof(url));
      }
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Disposes of the HttpClient to free up resources
    /// This is important to prevent socket exhaustion
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
        _httpClient?.Dispose();
        _disposed = true;
      }
    }

    #endregion
  }
}