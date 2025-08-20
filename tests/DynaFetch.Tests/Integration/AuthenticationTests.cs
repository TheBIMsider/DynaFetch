using System;
using System.Collections.Generic;
using NUnit.Framework;
using DynaFetch.Core;
using DynaFetch.Nodes;

namespace DynaFetch.Tests.Integration
{
  /// <summary>
  /// Tests for authentication scenarios including Bearer tokens, custom headers, and API keys
  /// These tests validate DynaFetch's ability to handle various authentication patterns
  /// using the client-level default headers approach
  /// </summary>
  [TestFixture]
  public class AuthenticationTests
  {
    private HttpClientWrapper? _client;

    [SetUp]
    public void Setup()
    {
      _client = ClientNodes.Create();
    }

    [TearDown]
    public void TearDown()
    {
      _client?.Dispose();
    }

    #region Bearer Token Authentication Tests

    [Test]
    public void ClientDefaultHeader_BearerToken_IncludesInRequest()
    {
      // Arrange
      var testToken = "test-bearer-token-12345";
      var url = "https://httpbin.org/bearer";

      // Add Bearer token as default header to client
      ClientNodes.AddDefaultHeader(_client!, "Authorization", $"Bearer {testToken}");

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "Bearer token request should succeed");
      Assert.That(result, Is.Not.Null);
      Assert.That(result.ContainsKey("authenticated"), Is.True);
      Assert.That(result["authenticated"], Is.EqualTo(true));
      Assert.That(result.ContainsKey("token"), Is.True);
      Assert.That(result["token"], Is.EqualTo(testToken));

      Console.WriteLine($"Bearer token authentication successful with token: {testToken}");
    }

    [Test]
    public void ClientDefaultHeader_InvalidBearerToken_ReturnsUnauthorized()
    {
      // Arrange - httpbin.org/bearer rejects requests without proper Authorization header
      var url = "https://httpbin.org/bearer";

      // Don't add any authorization header (or add an invalid one)
      // httpbin.org/bearer requires Authorization header

      // Act
      var response = ExecuteNodes.GET(_client!, url);

      // Assert
      // Request without Bearer token should result in 401 Unauthorized
      Assert.That(response, Is.Not.Null);
      Assert.That(response.StatusCode.ToString(), Is.EqualTo("Unauthorized"));
      Assert.That(response.IsSuccess, Is.False);

      Console.WriteLine($"Missing bearer token correctly rejected: {response.StatusCode}");
    }

    #endregion

    #region Custom Header Authentication Tests

    [Test]
    public void ClientDefaultHeader_CustomApiKey_IncludesInRequest()
    {
      // Arrange
      var apiKey = "test-api-key-67890";
      var url = "https://httpbin.org/headers";

      // Add custom API key header as default header
      ClientNodes.AddDefaultHeader(_client!, "X-API-Key", apiKey);

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True);
      Assert.That(result, Is.Not.Null);
      Assert.That(result.ContainsKey("headers"), Is.True);

      var headers = (Dictionary<string, object>)result["headers"];
      Assert.That(headers.ContainsKey("X-Api-Key"), Is.True, "Custom header should be present");
      Assert.That(headers["X-Api-Key"], Is.EqualTo(apiKey));

      Console.WriteLine($"Custom API key header successfully sent: {apiKey}");
    }

    [Test]
    public void ClientDefaultHeaders_MultipleAuthHeaders_AllIncluded()
    {
      // Arrange
      var url = "https://httpbin.org/headers";
      var authHeaders = new Dictionary<string, string>
            {
                { "Authorization", "Bearer multi-test-token" },
                { "X-API-Key", "multi-test-key" },
                { "X-Client-ID", "dynafetch-test-client" },
                { "X-Request-ID", "test-request-123" }
            };

      // Add multiple headers as default headers
      ClientNodes.AddDefaultHeaders(_client!, authHeaders);

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True);
      Assert.That(result.ContainsKey("headers"), Is.True);

      var headers = (Dictionary<string, object>)result["headers"];

      // Check each header (httpbin normalizes header names)
      Assert.That(headers.ContainsKey("Authorization"), Is.True);
      Assert.That(headers["Authorization"], Is.EqualTo("Bearer multi-test-token"));

      Assert.That(headers.ContainsKey("X-Api-Key"), Is.True);
      Assert.That(headers["X-Api-Key"], Is.EqualTo("multi-test-key"));

      Assert.That(headers.ContainsKey("X-Client-Id"), Is.True);
      Assert.That(headers["X-Client-Id"], Is.EqualTo("dynafetch-test-client"));

      Console.WriteLine($"Multiple authentication headers successfully sent: {authHeaders.Count} headers");
    }

    #endregion

    #region Client Header Management Tests

    [Test]
    public void GetDefaultHeaders_ReturnsCurrentHeaders()
    {
      // Arrange
      var apiKey = "test-get-headers-key";
      ClientNodes.AddDefaultHeader(_client!, "X-Test-API-Key", apiKey);
      ClientNodes.AddDefaultHeader(_client!, "User-Agent", "DynaFetch-Test/1.0");

      // Act
      var headers = ClientNodes.GetDefaultHeaders(_client!);

      // Assert
      Assert.That(headers, Is.Not.Null);
      Assert.That(headers.ContainsKey("X-Test-API-Key"), Is.True);
      Assert.That(headers["X-Test-API-Key"], Is.EqualTo(apiKey));
      Assert.That(headers.ContainsKey("User-Agent"), Is.True);

      Console.WriteLine($"Retrieved {headers.Count} default headers from client");
    }

    [Test]
    public void RemoveDefaultHeader_RemovesSpecificHeader()
    {
      // Arrange
      var tempKey = "temp-header-key";
      var permanentKey = "permanent-header-key";

      ClientNodes.AddDefaultHeader(_client!, "X-Temp-Header", tempKey);
      ClientNodes.AddDefaultHeader(_client!, "X-Permanent-Header", permanentKey);

      // Verify both headers are present
      var headersBefore = ClientNodes.GetDefaultHeaders(_client!);
      Assert.That(headersBefore.ContainsKey("X-Temp-Header"), Is.True);
      Assert.That(headersBefore.ContainsKey("X-Permanent-Header"), Is.True);

      // Act
      ClientNodes.RemoveDefaultHeader(_client!, "X-Temp-Header");

      // Assert
      var headersAfter = ClientNodes.GetDefaultHeaders(_client!);
      Assert.That(headersAfter.ContainsKey("X-Temp-Header"), Is.False, "Temp header should be removed");
      Assert.That(headersAfter.ContainsKey("X-Permanent-Header"), Is.True, "Permanent header should remain");

      Console.WriteLine("Header removal successful");
    }

    [Test]
    public void DefaultHeaders_PersistAcrossRequests()
    {
      // Arrange
      var defaultHeaders = new Dictionary<string, string>
            {
                { "Authorization", "Bearer persistent-token" },
                { "X-Client-Version", "DynaFetch-1.0" },
                { "Accept", "application/json" }
            };

      ClientNodes.AddDefaultHeaders(_client!, defaultHeaders);

      // Test multiple URLs to verify persistence
      var urls = new[]
      {
                "https://httpbin.org/headers",
                "https://httpbin.org/get"
            };

      foreach (var url in urls)
      {
        // Act
        var response = ExecuteNodes.GET(_client!, url);
        var result = JsonNodes.ToDictionary(response);

        // Assert
        Assert.That(response.IsSuccess, Is.True, $"Request to {url} should succeed");
        Assert.That(result.ContainsKey("headers"), Is.True);

        var headers = (Dictionary<string, object>)result["headers"];
        Assert.That(headers.ContainsKey("Authorization"), Is.True);
        Assert.That(headers["Authorization"], Is.EqualTo("Bearer persistent-token"));

        Console.WriteLine($"Persistent auth verified for: {url}");
      }
    }

    #endregion

    #region POST Request Authentication Tests

    [Test]
    public void POST_WithAuthentication_IncludesHeaders()
    {
      // Arrange
      var url = "https://httpbin.org/post";
      var apiKey = "post-test-api-key";
      var testData = @"{
                ""project"": ""DynaFetch"",
                ""test"": ""authentication"",
                ""timestamp"": ""2025-08-20""
            }";

      // Add authentication header
      ClientNodes.AddDefaultHeader(_client!, "X-API-Key", apiKey);
      ClientNodes.AddDefaultHeader(_client!, "Authorization", "Bearer post-test-token");

      // Act
      var response = ExecuteNodes.POST(_client!, url, testData);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "POST with authentication should succeed");
      Assert.That(result.ContainsKey("headers"), Is.True);

      var headers = (Dictionary<string, object>)result["headers"];
      Assert.That(headers.ContainsKey("X-Api-Key"), Is.True);
      Assert.That(headers["X-Api-Key"], Is.EqualTo(apiKey));
      Assert.That(headers.ContainsKey("Authorization"), Is.True);
      Assert.That(headers["Authorization"], Is.EqualTo("Bearer post-test-token"));

      // Verify the JSON data was also sent correctly
      Assert.That(result.ContainsKey("json"), Is.True);
      var jsonData = (Dictionary<string, object>)result["json"];
      Assert.That(jsonData["project"], Is.EqualTo("DynaFetch"));

      Console.WriteLine("POST request with authentication successful");
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Authentication_NetworkTimeout_HandlesGracefully()
    {
      // Arrange - Create a client with very short timeout
      var shortTimeoutClient = ClientNodes.CreateWithSettings(1, "DynaFetch-Timeout-Test/1.0");
      ClientNodes.AddDefaultHeader(shortTimeoutClient, "Authorization", "Bearer timeout-test-token");

      try
      {
        // Act & Assert - Request that takes longer than timeout
        Assert.Throws<InvalidOperationException>(() =>
        {
          var response = ExecuteNodes.GET(shortTimeoutClient, "https://httpbin.org/delay/5"); // 5 second delay
        });

        Console.WriteLine("Timeout scenario handled correctly with specific exception");
      }
      finally
      {
        shortTimeoutClient?.Dispose();
      }
    }

    [Test]
    public void Authentication_InvalidHeaderValue_HandlesGracefully()
    {
      // Arrange & Act - Test with various potentially problematic header values
      var testValues = new[]
      {
                "valid-header-value", // Should work
                "", // Empty value - should work but might be ignored
                "   ", // Whitespace only - should work
            };

      foreach (var headerValue in testValues)
      {
        try
        {
          // Create fresh client for each test
          using var testClient = ClientNodes.Create();
          ClientNodes.AddDefaultHeader(testClient, "X-Test-Header", headerValue);

          // Try to make a request - should either work or fail gracefully
          var response = ExecuteNodes.GET(testClient, "https://httpbin.org/headers");

          // If it succeeds, verify the response is valid
          Assert.That(response, Is.Not.Null);
          Console.WriteLine($"Header value '{headerValue}' handled successfully");
        }
        catch (Exception ex)
        {
          // If it fails, should be a specific exception type with helpful message
          Assert.That(ex, Is.InstanceOf<ArgumentException>().Or.InstanceOf<InvalidOperationException>(),
              "Should throw specific exception for problematic headers");
          Console.WriteLine($"Header value '{headerValue}' correctly rejected: {ex.GetType().Name}");
        }
      }
    }

    #endregion

    #region Real-World Authentication Workflow

    [Test]
    public void CompleteAuthenticatedWorkflow_MultipleOperations()
    {
      // Simulate a complete API workflow with authentication
      // This tests the most common pattern: API key + multiple operations

      // Arrange
      var apiKey = "workflow-test-key-2025";
      var baseUrl = "https://httpbin.org";

      // Set up client with base URL and default authentication
      ClientNodes.SetBaseUrl(_client!, baseUrl);
      ClientNodes.AddDefaultHeader(_client!, "X-API-Key", apiKey);
      ClientNodes.AddDefaultHeader(_client!, "User-Agent", "DynaFetch-Workflow-Test/1.0");
      // Note: Content-Type is automatically set by ExecuteNodes.POST() when sending JSON

      // Step 1: Test GET with authentication
      var getResponse = ExecuteNodes.GET(_client!, "/get");

      Assert.That(getResponse.IsSuccess, Is.True, "Authenticated GET should succeed");

      var getResult = JsonNodes.ToDictionary(getResponse);
      var getHeaders = (Dictionary<string, object>)getResult["headers"];
      Assert.That(getHeaders["X-Api-Key"], Is.EqualTo(apiKey));

      // Step 2: Test POST with authentication and data
      var postData = @"{
                ""action"": ""create_resource"",
                ""data"": ""test data for authenticated endpoint"",
                ""timestamp"": ""2025-08-20 Session 7"",
                ""workflow"": true
            }";

      var postResponse = ExecuteNodes.POST(_client!, "/post", postData);

      Assert.That(postResponse.IsSuccess, Is.True, "Authenticated POST should succeed");

      var postResult = JsonNodes.ToDictionary(postResponse);
      var postHeaders = (Dictionary<string, object>)postResult["headers"];
      Assert.That(postHeaders["X-Api-Key"], Is.EqualTo(apiKey));

      // Verify the posted data was received correctly
      var postedJson = (Dictionary<string, object>)postResult["json"];
      Assert.That(postedJson["action"], Is.EqualTo("create_resource"));
      Assert.That(postedJson["workflow"], Is.EqualTo(true));

      Console.WriteLine("Complete authenticated workflow successful - GET and POST with persistent headers");
    }

    #endregion
  }
}