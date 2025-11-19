using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DynaFetch.Core
{
  /// <summary>
  /// Builds HTTP requests with URL, parameters, headers, and body content.
  /// This class uses the "builder pattern" - you chain method calls together
  /// to configure your request step by step.
  /// 
  /// Example usage:
  /// var request = HttpRequest.Create("https://api.example.com/users")
  ///     .AddParameter("page", "1")
  ///     .AddHeader("Authorization", "Bearer token")
  ///     .AddJsonBody(userData);
  /// </summary>
  public class HttpRequest
  {
    #region Private Fields

    private string _url;
    private readonly Dictionary<string, string> _parameters;
    private readonly Dictionary<string, string> _headers;
    private HttpContent? _content;
    private string _httpMethod;

    // File upload support
    private List<FileUploadInfo>? _files;

    // Internal class to store file upload details
    private class FileUploadInfo
    {
      public string FilePath { get; set; } = string.Empty;
      public string FieldName { get; set; } = string.Empty;
      public string FileName { get; set; } = string.Empty;
      public string ContentType { get; set; } = string.Empty;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// The complete URL for this request
    /// Includes the base URL plus any query parameters
    /// </summary>
    public string Url => BuildFinalUrl();

    /// <summary>
    /// HTTP method for this request (GET, POST, PUT, DELETE, PATCH)
    /// </summary>
    public string HttpMethod => _httpMethod;

    /// <summary>
    /// Headers that will be sent with this specific request
    /// These are in addition to any default headers on the HttpClient
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers => _headers;

    /// <summary>
    /// Query parameters that will be added to the URL
    /// Example: ?page=1&limit=10
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    /// <summary>
    /// The content/body of the request (for POST, PUT, PATCH)
    /// </summary>
    public HttpContent? Content => _content;

    /// <summary>
    /// Indicates whether this request has file uploads
    /// </summary>
    public bool HasFiles => _files != null && _files.Count > 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor - use Create() method instead
    /// This enforces the builder pattern and ensures we always have a valid URL
    /// </summary>
    /// <param name="url">Base URL for the request</param>

    private HttpRequest(string url)
    {
      _url = url ?? throw new ArgumentException("URL cannot be null", nameof(url));
      _parameters = new Dictionary<string, string>();
      _headers = new Dictionary<string, string>();
      _httpMethod = "GET"; // Default to GET method
      _files = null; // Initialize as null, only create when needed
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a new HttpRequest builder with the specified URL
    /// This is the starting point for building any request
    /// </summary>
    /// <param name="url">URL to request (can be relative if using with a base URL)</param>
    /// <returns>New HttpRequest builder instance</returns>
    public static HttpRequest Create(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be null or empty", nameof(url));

      return new HttpRequest(url);
    }

    /// <summary>
    /// Creates a new HttpRequest builder by combining a base URL with an endpoint
    /// Useful when you have a base API URL and want to call different endpoints
    /// </summary>
    /// <param name="baseUrl">Base URL like "https://api.example.com"</param>
    /// <param name="endpoint">Endpoint like "/users" or "users/123"</param>
    /// <returns>New HttpRequest builder instance</returns>
    public static HttpRequest Create(string baseUrl, string endpoint)
    {
      if (string.IsNullOrWhiteSpace(baseUrl))
        throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

      if (string.IsNullOrWhiteSpace(endpoint))
        throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

      // Ensure proper URL combining (handle slashes correctly)
      var combinedUrl = CombineUrls(baseUrl, endpoint);
      return new HttpRequest(combinedUrl);
    }

    #endregion

    #region Builder Methods - Parameters

    /// <summary>
    /// Adds a query parameter to the URL
    /// Example: AddParameter("page", "1") adds "?page=1" to the URL
    /// </summary>
    /// <param name="name">Parameter name</param>
    /// <param name="value">Parameter value</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddParameter(string name, string value)
    {
      if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));

      _parameters[name] = value ?? string.Empty;
      return this;
    }

    /// <summary>
    /// Adds multiple query parameters from a dictionary
    /// Useful when you have many parameters to add at once
    /// </summary>
    /// <param name="parameters">Dictionary of parameter name/value pairs</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddParameters(Dictionary<string, string> parameters)
    {
      if (parameters == null) return this;

      foreach (var param in parameters)
      {
        AddParameter(param.Key, param.Value);
      }
      return this;
    }

    /// <summary>
    /// Removes a query parameter
    /// </summary>
    /// <param name="name">Parameter name to remove</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest RemoveParameter(string name)
    {
      if (!string.IsNullOrWhiteSpace(name))
      {
        _parameters.Remove(name);
      }
      return this;
    }

    #endregion

    #region Builder Methods - Headers

    /// <summary>
    /// Adds a header to this specific request
    /// Common headers: Authorization, Accept, Content-Type
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddHeader(string name, string value)
    {
      if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Header name cannot be null or empty", nameof(name));

      _headers[name] = value ?? string.Empty;
      return this;
    }

    /// <summary>
    /// Adds multiple headers from a dictionary
    /// </summary>
    /// <param name="headers">Dictionary of header name/value pairs</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddHeaders(Dictionary<string, string> headers)
    {
      if (headers == null) return this;

      foreach (var header in headers)
      {
        AddHeader(header.Key, header.Value);
      }
      return this;
    }

    /// <summary>
    /// Adds an Authorization header with Bearer token
    /// Common for API authentication
    /// </summary>
    /// <param name="token">Bearer token (without "Bearer " prefix)</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddBearerToken(string token)
    {
      if (!string.IsNullOrWhiteSpace(token))
      {
        AddHeader("Authorization", $"Bearer {token}");
      }
      return this;
    }

    /// <summary>
    /// Removes a header from this request
    /// </summary>
    /// <param name="name">Header name to remove</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest RemoveHeader(string name)
    {
      if (!string.IsNullOrWhiteSpace(name))
      {
        _headers.Remove(name);
      }
      return this;
    }

    #endregion

    #region Builder Methods - Content/Body

    /// <summary>
    /// Sets the request content to JSON
    /// Automatically sets Content-Type to application/json
    /// </summary>
    /// <param name="jsonContent">JSON string</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddJsonContent(string jsonContent)
    {
      if (string.IsNullOrEmpty(jsonContent))
      {
        _content = null;
        return this;
      }

      _content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
      return this;
    }

    /// <summary>
    /// Serializes an object to JSON and sets it as the request content
    /// This is the most common way to send data in API requests
    /// </summary>
    /// <param name="obj">Object to serialize to JSON</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddJsonBody(object obj)
    {
      if (obj == null)
      {
        _content = null;
        return this;
      }

      try
      {
        // Use System.Text.Json for modern, fast serialization
        var jsonOptions = new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
          WriteIndented = false
        };

        var json = JsonSerializer.Serialize(obj, jsonOptions);
        return AddJsonContent(json);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to serialize object to JSON: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Sets plain text content for the request body
    /// </summary>
    /// <param name="text">Text content</param>
    /// <param name="contentType">Content type (default: text/plain)</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddTextContent(string text, string contentType = "text/plain")
    {
      if (string.IsNullOrEmpty(text))
      {
        _content = null;
        return this;
      }

      _content = new StringContent(text, Encoding.UTF8, contentType);
      return this;
    }

    /// <summary>
    /// Sets custom HttpContent for the request
    /// Use this for advanced scenarios like file uploads
    /// </summary>
    /// <param name="content">HttpContent instance</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddContent(HttpContent content)
    {
      _content = content;
      return this;
    }

    #endregion

    /// <summary>
    /// Adds a file to this request for upload (multipart form-data)
    /// Follows DynaWeb's AddFile pattern for compatibility
    /// </summary>
    /// <param name="fieldName">Form field name for the file</param>
    /// <param name="filePath">Full path to the file to upload</param>
    /// <param name="contentType">MIME type (e.g., "image/png", "application/pdf")</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest AddFile(string fieldName, string filePath, string contentType)
    {
      // Validate inputs
      if (string.IsNullOrWhiteSpace(filePath))
        throw new ArgumentException("File path cannot be empty", nameof(filePath));

      if (!System.IO.File.Exists(filePath))
        throw new System.IO.FileNotFoundException($"File not found: {filePath}");

      // Initialize files list on first use
      if (_files == null)
        _files = new List<FileUploadInfo>();

      // Store file info for later processing
      _files.Add(new FileUploadInfo
      {
        FilePath = filePath,
        FieldName = fieldName ?? string.Empty,
        FileName = System.IO.Path.GetFileName(filePath),
        ContentType = contentType ?? "application/octet-stream"
      });

      return this;
    }

    /// <summary>
    /// Builds multipart form-data content from stored file information
    /// Internal method used by ExecuteNodes
    /// </summary>
    internal MultipartFormDataContent? BuildMultipartContent()
    {
      if (_files == null || _files.Count == 0)
        return null;

      var formData = new MultipartFormDataContent();

      foreach (var file in _files)
      {
        // Read file content
        byte[] fileBytes = System.IO.File.ReadAllBytes(file.FilePath);
        var fileContent = new ByteArrayContent(fileBytes);

        // Set content type
        fileContent.Headers.ContentType =
          new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        // Ensure field name is not empty (required by .NET API)
        string fieldName = string.IsNullOrWhiteSpace(file.FieldName) ? "file" : file.FieldName;
        formData.Add(fileContent, fieldName, file.FileName);
      }

      return formData;
    }

    #region Builder Methods - HTTP Method

    /// <summary>
    /// Sets the HTTP method for this request
    /// </summary>
    /// <param name="method">HTTP method (GET, POST, PUT, DELETE, PATCH)</param>
    /// <returns>This HttpRequest instance for method chaining</returns>
    public HttpRequest SetMethod(string method)
    {
      if (string.IsNullOrWhiteSpace(method))
        throw new ArgumentException("HTTP method cannot be null or empty", nameof(method));

      _httpMethod = method.ToUpperInvariant();
      return this;
    }

    /// <summary>
    /// Sets this request to use GET method (default)
    /// </summary>
    public HttpRequest AsGet() => SetMethod("GET");

    /// <summary>
    /// Sets this request to use POST method
    /// </summary>
    public HttpRequest AsPost() => SetMethod("POST");

    /// <summary>
    /// Sets this request to use PUT method
    /// </summary>
    public HttpRequest AsPut() => SetMethod("PUT");

    /// <summary>
    /// Sets this request to use DELETE method
    /// </summary>
    public HttpRequest AsDelete() => SetMethod("DELETE");

    /// <summary>
    /// Sets this request to use PATCH method
    /// </summary>
    public HttpRequest AsPatch() => SetMethod("PATCH");

    #endregion

    #region Helper Methods

    /// <summary>
    /// Builds the final URL with all query parameters
    /// </summary>
    /// <returns>Complete URL with query string</returns>
    private string BuildFinalUrl()
    {
      if (_parameters.Count == 0)
        return _url;

      var urlBuilder = new StringBuilder(_url);
      var isFirstParameter = !_url.Contains('?');

      foreach (var param in _parameters)
      {
        urlBuilder.Append(isFirstParameter ? '?' : '&');
        urlBuilder.Append(Uri.EscapeDataString(param.Key));
        urlBuilder.Append('=');
        urlBuilder.Append(Uri.EscapeDataString(param.Value ?? string.Empty));
        isFirstParameter = false;
      }

      return urlBuilder.ToString();
    }

    /// <summary>
    /// Properly combines a base URL with an endpoint
    /// Handles slashes correctly to avoid double slashes or missing slashes
    /// </summary>
    /// <param name="baseUrl">Base URL</param>
    /// <param name="endpoint">Endpoint path</param>
    /// <returns>Combined URL</returns>
    private static string CombineUrls(string baseUrl, string endpoint)
    {
      // Remove trailing slash from base URL
      var cleanBaseUrl = baseUrl.TrimEnd('/');

      // Remove leading slash from endpoint
      var cleanEndpoint = endpoint.TrimStart('/');

      // Combine with exactly one slash
      return $"{cleanBaseUrl}/{cleanEndpoint}";
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Converts this HttpRequest to an HttpRequestMessage
    /// This is used internally when sending the request
    /// </summary>
    /// <returns>HttpRequestMessage ready to send</returns>
    public HttpRequestMessage ToHttpRequestMessage()
    {
      var request = new HttpRequestMessage(new HttpMethod(_httpMethod), Url)
      {
        Content = _content
      };

      // Add headers to the request
      foreach (var header in _headers)
      {
        // Some headers need to go on the content, others on the request
        if (_content != null && IsContentHeader(header.Key))
        {
          _content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        else
        {
          request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
      }

      return request;
    }

    /// <summary>
    /// Determines if a header should be added to content vs request headers
    /// </summary>
    /// <param name="headerName">Name of the header</param>
    /// <returns>True if it's a content header</returns>
    private static bool IsContentHeader(string headerName)
    {
      // These headers need to be set on HttpContent, not HttpRequestMessage
      var contentHeaders = new[]
      {
                "Content-Type", "Content-Length", "Content-Encoding",
                "Content-Language", "Content-Location", "Content-MD5",
                "Content-Range", "Expires", "Last-Modified"
            };

      return Array.Exists(contentHeaders, h =>
          string.Equals(h, headerName, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Debug/Display Methods

    /// <summary>
    /// Returns a string representation of this request for debugging
    /// Shows the method, URL, headers, and whether there's content
    /// </summary>
    /// <returns>Human-readable description of the request</returns>
    public override string ToString()
    {
      var result = new StringBuilder();
      result.AppendLine($"{_httpMethod} {Url}");

      if (_headers.Count > 0)
      {
        result.AppendLine("Headers:");
        foreach (var header in _headers)
        {
          result.AppendLine($"  {header.Key}: {header.Value}");
        }
      }

      if (_content != null)
      {
        result.AppendLine($"Content: {_content.GetType().Name}");
      }

      return result.ToString();
    }

    #endregion
  }
}