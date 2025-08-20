using System;
using System.Net;
using System.Net.Http;

namespace DynaFetch.Core
{
  /// <summary>
  /// Custom exception for DynaFetch-specific errors.
  /// This provides clear, helpful error messages that make sense to users
  /// working in Dynamo graphs.
  /// 
  /// Why custom exceptions?
  /// - Clear error messages specific to REST API operations
  /// - Consistent error handling across all DynaFetch components
  /// - Easy to catch and handle in Dynamo nodes
  /// - Better debugging experience for users
  /// </summary>
  public class DynaFetchException : Exception
  {
    /// <summary>
    /// The operation that failed (e.g., "HTTP Request", "JSON Parsing", etc.)
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Additional context about the error (URL, request details, etc.)
    /// </summary>
    public string? Context { get; }

    /// <summary>
    /// Creates a new DynaFetch exception
    /// </summary>
    /// <param name="operation">What operation failed</param>
    /// <param name="message">Description of what went wrong</param>
    /// <param name="context">Additional context (optional)</param>
    /// <param name="innerException">Original exception that caused this (optional)</param>
    public DynaFetchException(string operation, string message, string? context = null, Exception? innerException = null)
        : base(FormatMessage(operation, message, context), innerException)
    {
      Operation = operation;
      Context = context;
    }

    /// <summary>
    /// Formats the exception message in a consistent, helpful way
    /// </summary>
    private static string FormatMessage(string operation, string message, string? context)
    {
      var formattedMessage = $"DynaFetch {operation}: {message}";

      if (!string.IsNullOrWhiteSpace(context))
      {
        formattedMessage += $" (Context: {context})";
      }

      return formattedMessage;
    }
  }

  /// <summary>
  /// Exception thrown when HTTP requests fail.
  /// Provides detailed information about HTTP errors including status codes,
  /// response content, and request details.
  /// </summary>
  public class DynaFetchHttpException : DynaFetchException
  {
    /// <summary>
    /// HTTP status code from the failed request
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Response content (often contains error details from the API)
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// The URL that was requested
    /// </summary>
    public string? RequestUrl { get; }

    /// <summary>
    /// Creates a new HTTP exception
    /// </summary>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="message">Error description</param>
    /// <param name="requestUrl">URL that was requested</param>
    /// <param name="responseContent">Response content from server</param>
    /// <param name="innerException">Original exception</param>
    public DynaFetchHttpException(
        HttpStatusCode statusCode,
        string message,
        string? requestUrl = null,
        string? responseContent = null,
        Exception? innerException = null)
        : base("HTTP Request", message, requestUrl, innerException)
    {
      StatusCode = statusCode;
      ResponseContent = responseContent;
      RequestUrl = requestUrl;
    }

    /// <summary>
    /// Creates an HTTP exception from an HttpResponseMessage
    /// </summary>
    /// <param name="response">Failed HTTP response</param>
    /// <param name="responseContent">Response content (if available)</param>
    /// <returns>Formatted HTTP exception</returns>
    public static DynaFetchHttpException FromResponse(HttpResponseMessage response, string? responseContent = null)
    {
      var message = $"Request failed with status {(int)response.StatusCode} {response.ReasonPhrase}";
      var url = response.RequestMessage?.RequestUri?.ToString();

      return new DynaFetchHttpException(response.StatusCode, message, url, responseContent);
    }
  }

  /// <summary>
  /// Exception thrown when JSON operations fail.
  /// Provides clear information about JSON parsing, serialization, or validation errors.
  /// </summary>
  public class DynaFetchJsonException : DynaFetchException
  {
    /// <summary>
    /// The JSON content that failed to process (truncated for readability)
    /// </summary>
    public string? JsonContent { get; }

    /// <summary>
    /// Creates a new JSON exception
    /// </summary>
    /// <param name="message">Error description</param>
    /// <param name="jsonContent">JSON content that failed (optional)</param>
    /// <param name="innerException">Original exception</param>
    public DynaFetchJsonException(string message, string? jsonContent = null, Exception? innerException = null)
        : base("JSON Processing", message, TruncateJson(jsonContent), innerException)
    {
      JsonContent = jsonContent;
    }

    /// <summary>
    /// Truncates JSON content for error messages (prevents huge error messages)
    /// </summary>
    private static string? TruncateJson(string? json)
    {
      if (string.IsNullOrWhiteSpace(json)) return null;

      const int maxLength = 200;
      return json.Length > maxLength ? json.Substring(0, maxLength) + "..." : json;
    }
  }

  /// <summary>
  /// Provides validation methods for DynaFetch operations.
  /// These methods throw clear, helpful exceptions when validation fails.
  /// 
  /// Benefits:
  /// - Consistent validation across all components
  /// - Clear error messages that help users fix their Dynamo graphs
  /// - Prevents common mistakes and provides guidance
  /// </summary>
  public static class Validation
  {
    /// <summary>
    /// Validates that a URL is not null or empty
    /// </summary>
    /// <param name="url">URL to validate</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if URL is invalid</exception>
    public static void ValidateUrl(string? url, string parameterName = "url")
    {
      if (string.IsNullOrWhiteSpace(url))
      {
        throw new DynaFetchException(
            "Validation",
            $"URL cannot be null or empty. Please provide a valid URL like 'https://api.example.com/data'",
            $"Parameter: {parameterName}");
      }

      // Basic URL format validation
      if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
      {
        throw new DynaFetchException(
            "Validation",
            $"Invalid URL format: '{url}'. URLs should start with http:// or https://",
            $"Parameter: {parameterName}");
      }

      // Check for common mistakes
      if (uri.IsAbsoluteUri && uri.Scheme != "http" && uri.Scheme != "https")
      {
        throw new DynaFetchException(
            "Validation",
            $"Unsupported URL scheme: '{uri.Scheme}'. Only HTTP and HTTPS are supported",
            $"URL: {url}");
      }
    }

    /// <summary>
    /// Validates that a header name is valid
    /// </summary>
    /// <param name="headerName">Header name to validate</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if header name is invalid</exception>
    public static void ValidateHeaderName(string? headerName, string parameterName = "headerName")
    {
      if (string.IsNullOrWhiteSpace(headerName))
      {
        throw new DynaFetchException(
            "Validation",
            "Header name cannot be null or empty. Common headers include 'Authorization', 'Content-Type', 'Accept'",
            $"Parameter: {parameterName}");
      }

      // Check for invalid characters in header names
      if (headerName.Contains(':') || headerName.Contains('\r') || headerName.Contains('\n'))
      {
        throw new DynaFetchException(
            "Validation",
            $"Invalid characters in header name: '{headerName}'. Header names cannot contain colons, carriage returns, or line feeds",
            $"Parameter: {parameterName}");
      }
    }

    /// <summary>
    /// Validates that a timeout value is reasonable
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if timeout is invalid</exception>
    public static void ValidateTimeout(int timeoutSeconds, string parameterName = "timeoutSeconds")
    {
      if (timeoutSeconds <= 0)
      {
        throw new DynaFetchException(
            "Validation",
            $"Timeout must be greater than 0 seconds. Recommended values: 30-300 seconds",
            $"Parameter: {parameterName}, Value: {timeoutSeconds}");
      }

      if (timeoutSeconds > 3600) // 1 hour
      {
        throw new DynaFetchException(
            "Validation",
            $"Timeout of {timeoutSeconds} seconds seems unusually long. Consider using a shorter timeout (30-300 seconds)",
            $"Parameter: {parameterName}");
      }
    }

    /// <summary>
    /// Validates that an HTTP method is supported
    /// </summary>
    /// <param name="method">HTTP method to validate</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if method is invalid</exception>
    public static void ValidateHttpMethod(string? method, string parameterName = "method")
    {
      if (string.IsNullOrWhiteSpace(method))
      {
        throw new DynaFetchException(
            "Validation",
            "HTTP method cannot be null or empty. Supported methods: GET, POST, PUT, DELETE, PATCH",
            $"Parameter: {parameterName}");
      }

      var normalizedMethod = method.ToUpperInvariant();
      var supportedMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };

      if (Array.IndexOf(supportedMethods, normalizedMethod) == -1)
      {
        throw new DynaFetchException(
            "Validation",
            $"Unsupported HTTP method: '{method}'. Supported methods: {string.Join(", ", supportedMethods)}",
            $"Parameter: {parameterName}");
      }
    }

    /// <summary>
    /// Validates that JSON content is not null for operations that require it
    /// </summary>
    /// <param name="json">JSON content to validate</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if JSON is invalid</exception>
    public static void ValidateJsonContent(string? json, string parameterName = "json")
    {
      if (string.IsNullOrWhiteSpace(json))
      {
        throw new DynaFetchException(
            "Validation",
            "JSON content cannot be null or empty for this operation",
            $"Parameter: {parameterName}");
      }

      // Basic JSON format check (starts with { or [)
      var trimmed = json.Trim();
      if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
      {
        throw new DynaFetchException(
            "Validation",
            $"Content does not appear to be valid JSON. JSON should start with '{{' (object) or '[' (array)",
            $"Content preview: {(trimmed.Length > 50 ? trimmed.Substring(0, 50) + "..." : trimmed)}");
      }
    }

    /// <summary>
    /// Validates that an object is not null
    /// </summary>
    /// <param name="obj">Object to validate</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if object is null</exception>
    public static void ValidateNotNull(object? obj, string parameterName)
    {
      if (obj == null)
      {
        throw new DynaFetchException(
            "Validation",
            $"Parameter cannot be null",
            $"Parameter: {parameterName}");
      }
    }

    /// <summary>
    /// Validates that a string parameter is not null or empty
    /// </summary>
    /// <param name="value">String to validate</param>
    /// <param name="parameterName">Name of the parameter for error messages</param>
    /// <exception cref="DynaFetchException">Thrown if string is null or empty</exception>
    public static void ValidateNotEmpty(string? value, string parameterName)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        throw new DynaFetchException(
            "Validation",
            $"Parameter cannot be null or empty",
            $"Parameter: {parameterName}");
      }
    }

    /// <summary>
    /// Validates that an HttpClient is properly configured
    /// </summary>
    /// <param name="client">HttpClient to validate</param>
    /// <exception cref="DynaFetchException">Thrown if client is invalid</exception>
    public static void ValidateHttpClient(HttpClient? client)
    {
      if (client == null)
      {
        throw new DynaFetchException(
            "Validation",
            "HttpClient cannot be null. Create a client using DynaFetch.Client.Create()");
      }

      // Check if timeout is reasonable
      if (client.Timeout.TotalSeconds < 1)
      {
        throw new DynaFetchException(
            "Validation",
            $"HttpClient timeout is too short: {client.Timeout.TotalSeconds} seconds. Consider using 30-300 seconds");
      }
    }
  }

  /// <summary>
  /// Provides helper methods for safe operations that might fail.
  /// These methods return success/failure results instead of throwing exceptions,
  /// which is often more user-friendly in Dynamo graphs.
  /// </summary>
  public static class SafeOperations
  {
    /// <summary>
    /// Safely executes an operation and returns success/failure with result
    /// </summary>
    /// <typeparam name="T">Type of result</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Tuple with success flag and result</returns>
    public static (bool Success, T? Result, string? Error) TryExecute<T>(Func<T> operation)
    {
      try
      {
        var result = operation();
        return (true, result, null);
      }
      catch (Exception ex)
      {
        return (false, default(T), ex.Message);
      }
    }

    /// <summary>
    /// Safely parses a URL and returns success/failure
    /// </summary>
    /// <param name="url">URL to parse</param>
    /// <returns>Tuple with success flag and parsed URI</returns>
    public static (bool Success, Uri? Uri, string? Error) TryParseUrl(string? url)
    {
      if (string.IsNullOrWhiteSpace(url))
      {
        return (false, null, "URL cannot be null or empty");
      }

      if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
      {
        return (true, uri, null);
      }

      return (false, null, $"Invalid URL format: {url}");
    }

    /// <summary>
    /// Safely validates JSON format without throwing exceptions
    /// </summary>
    /// <param name="json">JSON to validate</param>
    /// <returns>Tuple with success flag and error message</returns>
    public static (bool IsValid, string? Error) ValidateJson(string? json)
    {
      if (string.IsNullOrWhiteSpace(json))
      {
        return (false, "JSON cannot be null or empty");
      }

      try
      {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return (true, null);
      }
      catch (System.Text.Json.JsonException ex)
      {
        return (false, $"Invalid JSON: {ex.Message}");
      }
    }
  }
}