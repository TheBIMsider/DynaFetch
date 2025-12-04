using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using DynaFetch.Core;
using Microsoft.IdentityModel.Tokens;

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

    /// <summary>
    /// Generates a JWT assertion for service account authentication (e.g., Autodesk SSA).
    /// This creates a cryptographically signed JSON Web Token using an RSA private key.
    /// 
    /// USAGE PATTERN (Autodesk SSA 3-legged token exchange):
    /// 1. Generate JWT assertion with this method
    /// 2. Exchange assertion for access token using ExecuteNodes.POST to token endpoint
    /// 3. Use access token for subsequent API calls
    /// 
    /// Example token exchange body:
    /// "grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&amp;assertion={JWT}"
    /// </summary>
    /// <param name="privateKeyPem">RSA private key in PEM format (begins with "-----BEGIN RSA PRIVATE KEY-----")</param>
    /// <param name="clientId">OAuth client ID / application ID (also used as issuer and subject)</param>
    /// <param name="audience">Token audience URL (e.g., "https://developer.api.autodesk.com/")</param>
    /// <param name="scopes">List of scope strings (e.g., ["data:read", "data:write"])</param>
    /// <param name="expirationMinutes">Token validity period in minutes (default: 60, max: 60 for most providers)</param>
    /// <returns>Signed JWT assertion string ready for token exchange</returns>
    public static string GenerateJwtAssertion(
      string privateKeyPem,
      string clientId,
      string audience,
      List<string> scopes,
      int expirationMinutes = 60)
    {
      try
      {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(privateKeyPem))
          throw new ArgumentException("Private key PEM cannot be empty");

        if (string.IsNullOrWhiteSpace(clientId))
          throw new ArgumentException("Client ID cannot be empty");

        if (string.IsNullOrWhiteSpace(audience))
          throw new ArgumentException("Audience URL cannot be empty");

        if (scopes == null || !scopes.Any())
          throw new ArgumentException("At least one scope is required");

        if (expirationMinutes < 1 || expirationMinutes > 60)
          throw new ArgumentException("Expiration must be between 1 and 60 minutes");

        // Parse the RSA private key from PEM format
        var rsa = LoadRsaPrivateKeyFromPem(privateKeyPem);

        // Create signing credentials with RS256 algorithm
        var signingCredentials = new SigningCredentials(
          new RsaSecurityKey(rsa),
          SecurityAlgorithms.RsaSha256
        );

        // Build JWT claims according to RFC 7523 and Autodesk SSA requirements
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
          new Claim(JwtRegisteredClaimNames.Iss, clientId),    // Issuer = client ID
          new Claim(JwtRegisteredClaimNames.Sub, clientId),    // Subject = client ID
          new Claim(JwtRegisteredClaimNames.Aud, audience),    // Audience = token endpoint
          new Claim(JwtRegisteredClaimNames.Iat,
            new DateTimeOffset(now).ToUnixTimeSeconds().ToString()), // Issued at
          new Claim(JwtRegisteredClaimNames.Exp,
            new DateTimeOffset(now.AddMinutes(expirationMinutes)).ToUnixTimeSeconds().ToString()), // Expires
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
        };

        // Add scope as a single space-separated string claim (OAuth 2.0 standard)
        // Note: Some providers may expect array format, but space-separated is standard
        var scopeValue = string.Join(" ", scopes);
        claims.Add(new Claim("scope", scopeValue));

        // Create the JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
          Subject = new ClaimsIdentity(claims),
          SigningCredentials = signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        return jwtString;
      }
      catch (CryptographicException ex)
      {
        throw new InvalidOperationException(
          $"Failed to load RSA private key. Ensure the PEM format is correct " +
          $"and begins with '-----BEGIN RSA PRIVATE KEY-----': {ex.Message}", ex);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException($"Invalid JWT assertion parameter: {ex.Message}", ex);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(
          $"Failed to generate JWT assertion: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Loads an RSA private key from PEM format string.
    /// Supports both PKCS#1 (BEGIN RSA PRIVATE KEY) and PKCS#8 (BEGIN PRIVATE KEY) formats.
    /// </summary>
    private static RSA LoadRsaPrivateKeyFromPem(string privateKeyPem)
    {
      var rsa = RSA.Create();

      try
      {
        // Clean up the PEM string - remove headers, footers, and whitespace
        var cleanedPem = privateKeyPem
          .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
          .Replace("-----END RSA PRIVATE KEY-----", "")
          .Replace("-----BEGIN PRIVATE KEY-----", "")
          .Replace("-----END PRIVATE KEY-----", "")
          .Replace("\n", "")
          .Replace("\r", "")
          .Replace(" ", "");

        // Convert from Base64 to bytes
        var privateKeyBytes = Convert.FromBase64String(cleanedPem);

        // Try PKCS#1 format first (RSA PRIVATE KEY)
        try
        {
          rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        }
        catch
        {
          // Fall back to PKCS#8 format (PRIVATE KEY)
          rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        }

        return rsa;
      }
      catch (Exception ex)
      {
        rsa.Dispose();
        throw new CryptographicException(
          "Failed to parse RSA private key from PEM. Ensure the key is in valid " +
          "PKCS#1 or PKCS#8 format.", ex);
      }
    }
  }
}