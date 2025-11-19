using System;
using System.Collections.Generic;
using DynaFetch.Core;

namespace DynaFetch.Nodes
{
  /// <summary>
  /// HTTP Request building nodes for DynaFetch
  /// These nodes help you construct API requests with URLs, headers, parameters, and body data
  /// </summary>
  public static class RequestNodes
  {
    /// <summary>
    /// Create a request from a complete URL
    /// Use this when you have the full URL including protocol and domain
    /// </summary>
    /// <param name="url">Complete URL (e.g., "https://api.example.com/users")</param>
    /// <returns>HTTP request ready for configuration</returns>
    public static HttpRequest ByUrl(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));

      try
      {
        return HttpRequest.Create(url);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to create request for URL '{url}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Create a request from a base URL and endpoint
    /// Use this to combine a base URL with a specific endpoint path
    /// </summary>
    /// <param name="baseUrl">Base URL (e.g., "https://api.example.com")</param>
    /// <param name="endpoint">Endpoint path (e.g., "/users/123" or "posts")</param>
    /// <returns>HTTP request ready for configuration</returns>
    public static HttpRequest ByEndpoint(string baseUrl, string endpoint)
    {
      if (string.IsNullOrWhiteSpace(baseUrl))
        throw new ArgumentException("Base URL cannot be empty", nameof(baseUrl));

      if (string.IsNullOrWhiteSpace(endpoint))
        throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));

      try
      {
        return HttpRequest.Create(baseUrl, endpoint);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to create request for '{baseUrl}' + '{endpoint}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add a custom header to a request
    /// Headers provide additional information with your request
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="headerName">Header name (e.g., "Authorization", "Content-Type")</param>
    /// <param name="headerValue">Header value (e.g., "Bearer token", "application/json")</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddHeader(HttpRequest request, string headerName, string headerValue)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (string.IsNullOrWhiteSpace(headerName))
        throw new ArgumentException("Header name cannot be empty", nameof(headerName));

      if (string.IsNullOrWhiteSpace(headerValue))
        throw new ArgumentException("Header value cannot be empty", nameof(headerValue));

      try
      {
        request.AddHeader(headerName, headerValue);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add header '{headerName}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add multiple headers from a dictionary
    /// Convenient for adding many headers at once
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="headers">Dictionary of header names and values</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddHeaders(HttpRequest request, Dictionary<string, string> headers)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (headers == null)
        throw new ArgumentNullException(nameof(headers), "Headers dictionary cannot be null");

      try
      {
        request.AddHeaders(headers);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add headers: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add a Bearer token for authorization
    /// Common authentication method for APIs
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="token">Bearer token (without "Bearer " prefix)</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddBearerToken(HttpRequest request, string token)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (string.IsNullOrWhiteSpace(token))
        throw new ArgumentException("Token cannot be empty", nameof(token));

      try
      {
        request.AddBearerToken(token);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add bearer token: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add a query parameter to a request
    /// Parameters are added to the URL as ?key=value&key2=value2
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="parameterName">Parameter name (e.g., "page", "limit")</param>
    /// <param name="parameterValue">Parameter value (e.g., "1", "100")</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddParameter(HttpRequest request, string parameterName, string parameterValue)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (string.IsNullOrWhiteSpace(parameterName))
        throw new ArgumentException("Parameter name cannot be empty", nameof(parameterName));

      if (parameterValue == null)
        throw new ArgumentNullException(nameof(parameterValue), "Parameter value cannot be null");

      try
      {
        request.AddParameter(parameterName, parameterValue);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add parameter '{parameterName}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add multiple query parameters from a dictionary
    /// Convenient for adding many parameters at once
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="parameters">Dictionary of parameter names and values</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddParameters(HttpRequest request, Dictionary<string, string> parameters)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (parameters == null)
        throw new ArgumentNullException(nameof(parameters), "Parameters dictionary cannot be null");

      try
      {
        request.AddParameters(parameters);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add parameters: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add JSON data as the request body
    /// This automatically sets the Content-Type header to application/json
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="jsonData">Data to serialize as JSON (object, dictionary, or JSON string)</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddJsonBody(HttpRequest request, object jsonData)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (jsonData == null)
        throw new ArgumentNullException(nameof(jsonData), "JSON data cannot be null");

      try
      {
        request.AddJsonBody(jsonData);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add JSON body: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add JSON content from a JSON string
    /// Use this when you already have a JSON string
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="jsonContent">JSON string</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddJsonContent(HttpRequest request, string jsonContent)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (string.IsNullOrWhiteSpace(jsonContent))
        throw new ArgumentException("JSON content cannot be empty", nameof(jsonContent));

      try
      {
        request.AddJsonContent(jsonContent);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add JSON content: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Add plain text content as the request body
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="content">Text content to send</param>
    /// <param name="contentType">Content type (default: "text/plain")</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AddTextContent(HttpRequest request, string content, string contentType = "text/plain")
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (content == null)
        throw new ArgumentNullException(nameof(content), "Content cannot be null");

      if (string.IsNullOrWhiteSpace(contentType))
        throw new ArgumentException("Content type cannot be empty", nameof(contentType));

      try
      {
        request.AddTextContent(content, contentType);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to add text content: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Set the HTTP method for a request
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <param name="method">HTTP method (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest SetMethod(HttpRequest request, string method)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      if (string.IsNullOrWhiteSpace(method))
        throw new ArgumentException("HTTP method cannot be empty", nameof(method));

      try
      {
        request.SetMethod(method);
        return request;
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to set method '{method}': {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Set request to use GET method
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AsGet(HttpRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      return request.AsGet();
    }

    /// <summary>
    /// Set request to use POST method
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AsPost(HttpRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      return request.AsPost();
    }

    /// <summary>
    /// Set request to use PUT method
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AsPut(HttpRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      return request.AsPut();
    }

    /// <summary>
    /// Set request to use DELETE method
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AsDelete(HttpRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      return request.AsDelete();
    }

    /// <summary>
    /// Set request to use PATCH method
    /// </summary>
    /// <param name="request">HTTP request to modify</param>
    /// <returns>Updated HTTP request</returns>
    public static HttpRequest AsPatch(HttpRequest request)
    {
      if (request == null)
        throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

      return request.AsPatch();
    }

    /// <summary>
    /// Creates multipart form-data content for file uploads.
    /// Returns a MultipartFormDataContent object that can be passed to POST/PUT methods.
    /// </summary>
    /// <param name="filePath">Full path to the file to upload</param>
    /// <param name="fieldName">Form field name (optional, defaults to empty string for unnamed fields)</param>
    /// <param name="fileName">Custom filename (optional, uses actual filename if not provided)</param>
    /// <returns>MultipartFormDataContent ready for upload</returns>
    public static object CreateFileUpload(string filePath, string fieldName = "", string fileName = "")
    {
      // Validate file exists
      if (!System.IO.File.Exists(filePath))
      {
        throw new System.IO.FileNotFoundException($"File not found: {filePath}");
      }

      // Read file content as byte array
      byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

      // Create ByteArrayContent from file bytes
      var fileContent = new ByteArrayContent(fileBytes);

      // Detect content type based on file extension
      string contentType = GetContentType(filePath);
      fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

      // Create multipart form data container
      var formData = new MultipartFormDataContent();

      // Use actual filename if custom one not provided
      string uploadFileName = string.IsNullOrEmpty(fileName)
          ? System.IO.Path.GetFileName(filePath)
          : fileName;

      // Add file content with field name (empty string for unnamed fields like BIMtrack)
      formData.Add(fileContent, fieldName, uploadFileName);

      return formData;
    }

    /// <summary>
    /// Adds additional form fields to existing multipart form data.
    /// Use this to add text fields alongside file uploads.
    /// </summary>
    /// <param name="formData">Existing MultipartFormDataContent from CreateFileUpload</param>
    /// <param name="fieldName">Form field name</param>
    /// <param name="value">Field value</param>
    /// <returns>Updated MultipartFormDataContent</returns>
    public static object AddFormField(object formData, string fieldName, string value)
    {
      if (formData is not MultipartFormDataContent content)
      {
        throw new ArgumentException("formData must be MultipartFormDataContent from CreateFileUpload");
      }

      content.Add(new StringContent(value), fieldName);
      return content;
    }

    /// <summary>
    /// Detects MIME content type based on file extension.
    /// </summary>
    private static string GetContentType(string filePath)
    {
      string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

      return extension switch
      {
        // Images
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".bmp" => "image/bmp",
        ".webp" => "image/webp",
        ".svg" => "image/svg+xml",
        ".ico" => "image/x-icon",

        // Documents
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".ppt" => "application/vnd.ms-powerpoint",
        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",

        // Text
        ".txt" => "text/plain",
        ".csv" => "text/csv",
        ".html" or ".htm" => "text/html",
        ".xml" => "application/xml",
        ".json" => "application/json",

        // Archives
        ".zip" => "application/zip",
        ".rar" => "application/x-rar-compressed",
        ".7z" => "application/x-7z-compressed",

        // Default
        _ => "application/octet-stream"
      };
    }
  }
}