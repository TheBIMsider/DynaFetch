using System;
using System.Collections.Generic;
using DynaFetch.Core;

namespace DynaFetch.Nodes
{
  /// <summary>
  /// HTTP Client management nodes for DynaFetch
  /// These nodes handle creating and configuring HTTP clients for API connections
  /// </summary>
  public static class ClientNodes
  {
    /// <summary>
    /// Create a simple HTTP client with default settings
    /// Perfect for basic API requests - use other methods to configure further
    /// </summary>
    /// <returns>HTTP client ready for making requests</returns>
    public static HttpClientWrapper Create()
    {
      try
      {
        return new HttpClientWrapper();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to create HTTP client: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Create a new HTTP client with custom settings
    /// Use this when you need specific timeout or user agent settings
    /// </summary>
    /// <param name="timeoutSeconds">Request timeout in seconds</param>
    /// <param name="userAgent">Custom User-Agent header</param>
    /// <returns>HTTP client ready for making requests</returns>
    public static HttpClientWrapper CreateWithSettings(int timeoutSeconds, string userAgent)
    {
      if (timeoutSeconds <= 0)
        throw new ArgumentException("Timeout must be greater than 0 seconds", nameof(timeoutSeconds));

      if (string.IsNullOrWhiteSpace(userAgent))
        throw new ArgumentException("User-Agent cannot be empty", nameof(userAgent));

      try
      {
        var client = new HttpClientWrapper();
        client.TimeoutSeconds = timeoutSeconds;
        client.UserAgent = userAgent;
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to create HTTP client: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Create a new HTTP client with a base URL
    /// Convenient shortcut for creating a client with a base URL
    /// </summary>
    /// <param name="baseUrl">Base URL (e.g., "https://api.example.com")</param>
    /// <returns>HTTP client with base URL configured</returns>
    public static HttpClientWrapper CreateWithBaseUrl(string baseUrl)
    {
      if (string.IsNullOrWhiteSpace(baseUrl))
        throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

      try
      {
        return new HttpClientWrapper(baseUrl);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to create HTTP client with base URL: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Set the base URL for an existing HTTP client
    /// All requests will be relative to this URL
    /// </summary>
    /// <param name="client">HTTP client to configure</param>
    /// <param name="baseUrl">Base URL (e.g., "https://api.example.com")</param>
    /// <returns>Updated HTTP client</returns>
    public static HttpClientWrapper SetBaseUrl(HttpClientWrapper client, string baseUrl)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(baseUrl))
        throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

      try
      {
        client.BaseUrl = baseUrl;
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to set base URL: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Set the request timeout for an HTTP client
    /// </summary>
    /// <param name="client">HTTP client to configure</param>
    /// <param name="timeoutSeconds">Timeout in seconds (must be greater than 0)</param>
    /// <returns>Updated HTTP client</returns>
    public static HttpClientWrapper SetTimeout(HttpClientWrapper client, int timeoutSeconds)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (timeoutSeconds <= 0)
        throw new ArgumentException("Timeout must be greater than 0 seconds", nameof(timeoutSeconds));

      try
      {
        client.TimeoutSeconds = timeoutSeconds;
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to set timeout: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Set the User-Agent header for an HTTP client
    /// This identifies your application to the API server
    /// </summary>
    /// <param name="client">HTTP client to configure</param>
    /// <param name="userAgent">User-Agent string (e.g., "MyApp/1.0")</param>
    /// <returns>Updated HTTP client</returns>
    public static HttpClientWrapper SetUserAgent(HttpClientWrapper client, string userAgent)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(userAgent))
        throw new ArgumentException("User-Agent cannot be empty", nameof(userAgent));

      try
      {
        client.UserAgent = userAgent;
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to set User-Agent: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add a default header to all requests from an HTTP client
    /// </summary>
    /// <param name="client">HTTP client to configure</param>
    /// <param name="headerName">Header name (e.g., "Authorization")</param>
    /// <param name="headerValue">Header value (e.g., "Bearer your-token")</param>
    /// <returns>Updated HTTP client</returns>
    public static HttpClientWrapper AddDefaultHeader(HttpClientWrapper client, string headerName, string headerValue)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(headerName))
        throw new ArgumentException("Header name cannot be empty", nameof(headerName));

      if (string.IsNullOrWhiteSpace(headerValue))
        throw new ArgumentException("Header value cannot be empty", nameof(headerValue));

      try
      {
        client.SetDefaultHeader(headerName, headerValue);
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add header '{headerName}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add multiple default headers from a dictionary
    /// </summary>
    /// <param name="client">HTTP client to configure</param>
    /// <param name="headers">Dictionary of header names and values</param>
    /// <returns>Updated HTTP client</returns>
    public static HttpClientWrapper AddDefaultHeaders(HttpClientWrapper client, Dictionary<string, string> headers)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (headers == null)
        throw new ArgumentNullException(nameof(headers), "Headers dictionary cannot be null");

      try
      {
        client.SetDefaultHeaders(headers);
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add headers: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Get all current default headers from an HTTP client
    /// </summary>
    /// <param name="client">HTTP client to query</param>
    /// <returns>Dictionary of current default headers</returns>
    public static Dictionary<string, string> GetDefaultHeaders(HttpClientWrapper client)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      try
      {
        return client.GetDefaultHeaders();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to get headers: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Remove a default header from an HTTP client
    /// </summary>
    /// <param name="client">HTTP client to modify</param>
    /// <param name="headerName">Header name to remove</param>
    /// <returns>Updated HTTP client</returns>
    public static HttpClientWrapper RemoveDefaultHeader(HttpClientWrapper client, string headerName)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(headerName))
        throw new ArgumentException("Header name cannot be empty", nameof(headerName));

      try
      {
        client.RemoveDefaultHeader(headerName);
        return client;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to remove header '{headerName}': {ex.Message}", ex);
      }
    }
  }
}