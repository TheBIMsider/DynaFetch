using System;
using System.Threading.Tasks;
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
        // Execute GET request and wrap response (using Task.Run to avoid deadlocks in Revit)
        var httpResponseMessage = Task.Run(async () => await client.GetAsync(url)).Result;
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"GET request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a POST request
    /// Supports: (1) JSON string data, (2) HttpRequest with custom Content-Type headers, (3) HttpRequest with file uploads
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to post to</param>
    /// <param name="content">Optional: JSON string, HttpRequest with custom headers, or HttpRequest with files</param>
    /// <returns>HTTP response containing the result</returns>
    /// <remarks>
    /// When using HttpRequest with AddHeader to set custom Content-Type (e.g., application/merge-patch+json),
    /// the custom header is preserved instead of forcing application/json. This enables specialized POST
    /// operations with custom content-types while maintaining backward compatibility.
    /// </remarks>
    public static HttpResponse POST(HttpClientWrapper client, string url, object? content = null)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      try
      {
        System.Net.Http.HttpContent? httpContent = null;

        // Handle different content types
        if (content == null)
        {
          // No content - simple POST
          httpContent = null;
        }
        else if (content is HttpRequest httpRequest)
        {
          // HttpRequest - check for file uploads first
          httpContent = httpRequest.BuildMultipartContent();

          // If no files, check for custom Content-Type header
          if (httpContent == null)
          {
            // Check if user specified a custom Content-Type
            if (httpRequest.Headers.ContainsKey("Content-Type") && httpRequest.Content is StringContent)
            {
              // Extract the string content and recreate with custom Content-Type
              var contentString = httpRequest.Content.ReadAsStringAsync().Result;
              var customContentType = httpRequest.Headers["Content-Type"];
              httpContent = new System.Net.Http.StringContent(contentString, System.Text.Encoding.UTF8, customContentType);
            }
            else
            {
              // Use content as-is
              httpContent = httpRequest.Content;
            }
          }
        }
        else if (content is string jsonData)
        {
          // JSON string content - apply default application/json
          if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be empty", nameof(content));

          httpContent = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
        }
        else if (content is MultipartFormDataContent multipartContent)
        {
          // Direct multipart content
          httpContent = multipartContent;
        }
        else
        {
          // Unknown type - provide helpful error
          var typeName = content?.GetType().FullName ?? "null";
          throw new ArgumentException(
            $"Unsupported content type: {typeName}. " +
            $"POST accepts: (1) string for JSON, (2) HttpRequest with AddFile for uploads, (3) HttpRequest with custom Content-Type, or (4) MultipartFormDataContent.",
            nameof(content));
        }

        // Execute POST (using Task.Run to avoid deadlocks in Revit)
        var httpResponseMessage = Task.Run(async () => await client.PostAsync(url, httpContent!)).Result;
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"POST request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a PUT request
    /// Supports: (1) JSON string data, (2) HttpRequest with custom Content-Type headers, (3) HttpRequest with file uploads
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to put to</param>
    /// <param name="content">Optional: JSON string, HttpRequest with custom headers, or HttpRequest with files</param>
    /// <returns>HTTP response containing the result</returns>
    /// <remarks>
    /// When using HttpRequest with AddHeader to set custom Content-Type (e.g., application/merge-patch+json),
    /// the custom header is preserved instead of forcing application/json. This enables specialized PUT
    /// operations with custom content-types while maintaining backward compatibility.
    /// </remarks>
    public static HttpResponse PUT(HttpClientWrapper client, string url, object? content = null)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      try
      {
        System.Net.Http.HttpContent? httpContent = null;

        // Handle different content types
        if (content == null)
        {
          // No content - simple PUT
          httpContent = null;
        }
        else if (content is HttpRequest httpRequest)
        {
          // HttpRequest - check for file uploads first
          httpContent = httpRequest.BuildMultipartContent();

          // If no files, check for custom Content-Type header
          if (httpContent == null)
          {
            // Check if user specified a custom Content-Type
            if (httpRequest.Headers.ContainsKey("Content-Type") && httpRequest.Content is StringContent)
            {
              // Extract the string content and recreate with custom Content-Type
              var contentString = httpRequest.Content.ReadAsStringAsync().Result;
              var customContentType = httpRequest.Headers["Content-Type"];
              httpContent = new System.Net.Http.StringContent(contentString, System.Text.Encoding.UTF8, customContentType);
            }
            else
            {
              // Use content as-is
              httpContent = httpRequest.Content;
            }
          }
        }
        else if (content is string jsonData)
        {
          // JSON string content - apply default application/json
          if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be empty", nameof(content));

          httpContent = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
        }
        else if (content is MultipartFormDataContent multipartContent)
        {
          // Direct multipart content
          httpContent = multipartContent;
        }
        else
        {
          // Unknown type - provide helpful error
          var typeName = content?.GetType().FullName ?? "null";
          throw new ArgumentException(
            $"Unsupported content type: {typeName}. " +
            $"PUT accepts: (1) string for JSON, (2) HttpRequest with AddFile for uploads, (3) HttpRequest with custom Content-Type, or (4) MultipartFormDataContent.",
            nameof(content));
        }

        // Execute PUT (using Task.Run to avoid deadlocks in Revit)
        var httpResponseMessage = Task.Run(async () => await client.PutAsync(url, httpContent!)).Result;
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
        // Execute DELETE request and wrap response (using Task.Run to avoid deadlocks in Revit)
        var httpResponseMessage = Task.Run(async () => await client.DeleteAsync(url)).Result;
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"DELETE request to '{url}' failed: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Execute a PATCH request
    /// Supports: (1) JSON string data, (2) HttpRequest with custom Content-Type headers, (3) HttpRequest with file uploads
    /// </summary>
    /// <param name="client">HTTP client for making the request</param>
    /// <param name="url">URL to patch</param>
    /// <param name="content">Optional: JSON string, HttpRequest with custom headers, or HttpRequest with files</param>
    /// <returns>HTTP response containing the result</returns>
    /// <remarks>
    /// When using HttpRequest with AddHeader to set custom Content-Type (e.g., application/merge-patch+json),
    /// the custom header is preserved instead of forcing application/json. This enables RFC 7396 Merge Patch
    /// and other specialized PATCH operations.
    /// </remarks>
    public static HttpResponse PATCH(HttpClientWrapper client, string url, object? content = null)
    {
      if (client == null)
        throw new ArgumentNullException(nameof(client), "HTTP client cannot be null");

      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      try
      {
        System.Net.Http.HttpContent? httpContent = null;

        // Handle different content types
        if (content == null)
        {
          // No content - simple PATCH
          httpContent = null;
        }
        else if (content is HttpRequest httpRequest)
        {
          // HttpRequest - check for file uploads first
          httpContent = httpRequest.BuildMultipartContent();

          // If no files, check for custom Content-Type header
          if (httpContent == null)
          {
            // Check if user specified a custom Content-Type
            if (httpRequest.Headers.ContainsKey("Content-Type") && httpRequest.Content is StringContent)
            {
              // Extract the string content and recreate with custom Content-Type
              var contentString = httpRequest.Content.ReadAsStringAsync().Result;
              var customContentType = httpRequest.Headers["Content-Type"];
              httpContent = new System.Net.Http.StringContent(contentString, System.Text.Encoding.UTF8, customContentType);
            }
            else
            {
              // Use content as-is
              httpContent = httpRequest.Content;
            }
          }
        }
        else if (content is string jsonData)
        {
          // JSON string content - apply default application/json
          if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("JSON data cannot be empty", nameof(content));

          httpContent = new System.Net.Http.StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
        }
        else if (content is MultipartFormDataContent multipartContent)
        {
          // Direct multipart content
          httpContent = multipartContent;
        }
        else
        {
          // Unknown type - provide helpful error
          var typeName = content?.GetType().FullName ?? "null";
          throw new ArgumentException(
            $"Unsupported content type: {typeName}. " +
            $"PATCH accepts: (1) string for JSON, (2) HttpRequest with AddFile for uploads, (3) HttpRequest with custom Content-Type, or (4) MultipartFormDataContent.",
            nameof(content));
        }

        // Execute PATCH (using Task.Run to avoid deadlocks in Revit)
        var httpResponseMessage = Task.Run(async () => await client.PatchAsync(url, httpContent!)).Result;
        return new HttpResponse(httpResponseMessage);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"PATCH request to '{url}' failed: {ex.Message}", ex);
      }
    }
  }
}