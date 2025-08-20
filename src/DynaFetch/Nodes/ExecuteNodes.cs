using System;
using DynaFetch.Core;

namespace DynaFetch.Nodes
{
  /// <summary>
  /// HTTP execution nodes for DynaFetch
  /// These nodes execute HTTP requests and return responses
  /// </summary>
  public static class ExecuteNodes
  {
    /// <summary>
    /// Execute a simple GET request with just a URL
    /// Perfect for basic API calls without complex configuration
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to request (e.g., "https://api.example.com/data")</param>
    /// <returns>HTTP response containing the result</returns>
    public static HttpResponse GET(HttpClientWrapper client, string url)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      try
      {
        // Execute GET request and wrap response
        var httpResponseTask = client.GetAsync(url);
        var httpResponseMessage = httpResponseTask.GetAwaiter().GetResult();
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"GET request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a POST request with URL and JSON data
    /// Perfect for sending JSON data to APIs
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to post to (e.g., "https://api.example.com/create")</param>
    /// <param name="jsonData">JSON data to send as string</param>
    /// <returns>HTTP response containing the result</returns>
    public static HttpResponse POST(HttpClientWrapper client, string url, string jsonData)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      if (string.IsNullOrWhiteSpace(jsonData))
        throw new ArgumentException("JSON data cannot be empty", nameof(jsonData));

      try
      {
        // Create JSON content and execute POST
        var content = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
        var httpResponseTask = client.PostAsync(url, content);
        var httpResponseMessage = httpResponseTask.GetAwaiter().GetResult();
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"POST request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a PUT request with URL and JSON data
    /// Perfect for updating resources with JSON data
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to put to (e.g., "https://api.example.com/update/123")</param>
    /// <param name="jsonData">JSON data to send as string</param>
    /// <returns>HTTP response containing the result</returns>
    public static HttpResponse PUT(HttpClientWrapper client, string url, string jsonData)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      if (string.IsNullOrWhiteSpace(jsonData))
        throw new ArgumentException("JSON data cannot be empty", nameof(jsonData));

      try
      {
        // Create JSON content and execute PUT
        var content = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
        var httpResponseTask = client.PutAsync(url, content);
        var httpResponseMessage = httpResponseTask.GetAwaiter().GetResult();
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"PUT request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a simple DELETE request with just a URL
    /// Perfect for deleting resources by URL
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to delete (e.g., "https://api.example.com/delete/123")</param>
    /// <returns>HTTP response containing the result</returns>
    public static HttpResponse DELETE(HttpClientWrapper client, string url)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      try
      {
        // Execute DELETE request and wrap response
        var httpResponseTask = client.DeleteAsync(url);
        var httpResponseMessage = httpResponseTask.GetAwaiter().GetResult();
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"DELETE request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a PATCH request with URL and JSON data
    /// Perfect for partial updates with JSON data
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to patch (e.g., "https://api.example.com/patch/123")</param>
    /// <param name="jsonData">JSON data to send as string</param>
    /// <returns>HTTP response containing the result</returns>
    public static HttpResponse PATCH(HttpClientWrapper client, string url, string jsonData)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      if (string.IsNullOrWhiteSpace(jsonData))
        throw new ArgumentException("JSON data cannot be empty", nameof(jsonData));

      try
      {
        // Create JSON content and execute PATCH
        var content = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
        var httpResponseTask = client.PatchAsync(url, content);
        var httpResponseMessage = httpResponseTask.GetAwaiter().GetResult();
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"PATCH request to '{url}' failed: {ex.Message}", ex);
      }
    }
  }
}