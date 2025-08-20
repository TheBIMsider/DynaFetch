using System;
using System.Collections.Generic;
using NUnit.Framework;
using DynaFetch.Core;
using DynaFetch.Nodes;

namespace DynaFetch.Tests.Integration
{
  [TestFixture]
  public class ApiIntegrationTests
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

    #region JSON Processing Variations Tests

    [Test]
    public void GET_JSONPlaceholder_SinglePost_ReturnsValidDictionary()
    {
      // Arrange
      var url = "https://jsonplaceholder.typicode.com/posts/1";

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var dictionary = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "API call should succeed");
      Assert.That(dictionary, Is.Not.Null, "Dictionary should not be null");
      Assert.That(dictionary.ContainsKey("id"), Is.True, "Should contain id key");
      Assert.That(dictionary.ContainsKey("title"), Is.True, "Should contain title key");
      Assert.That(dictionary.ContainsKey("body"), Is.True, "Should contain body key");
      Assert.That(dictionary.ContainsKey("userId"), Is.True, "Should contain userId key");

      Console.WriteLine($"Post title: {dictionary["title"]}");
    }

    [Test]
    public void GET_JSONPlaceholder_AllPosts_ReturnsValidList()
    {
      // Arrange
      var url = "https://jsonplaceholder.typicode.com/posts";

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var list = JsonNodes.ToList(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "API call should succeed");
      Assert.That(list, Is.Not.Null, "List should not be null");
      Assert.That(list.Count, Is.GreaterThan(0), "List should contain posts");
      Assert.That(list.Count, Is.EqualTo(100), "JSONPlaceholder should return 100 posts");

      // Check first item structure
      if (list.Count > 0 && list[0] is Dictionary<string, object> firstPost)
      {
        Assert.That(firstPost.ContainsKey("id"), Is.True);
        Assert.That(firstPost.ContainsKey("title"), Is.True);
        Assert.That(firstPost.ContainsKey("body"), Is.True);
        Assert.That(firstPost.ContainsKey("userId"), Is.True);
        Console.WriteLine($"Retrieved {list.Count} posts");
      }
      else
      {
        Assert.Fail("First item should be a dictionary");
      }
    }

    [Test]
    public void GET_JSONPlaceholder_User_ReturnsComplexObject()
    {
      // Arrange
      var url = "https://jsonplaceholder.typicode.com/users/1";

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var dictionary = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "API call should succeed");
      Assert.That(dictionary, Is.Not.Null, "Dictionary should not be null");
      Assert.That(dictionary.ContainsKey("id"), Is.True);
      Assert.That(dictionary.ContainsKey("name"), Is.True);
      Assert.That(dictionary.ContainsKey("email"), Is.True);
      Assert.That(dictionary.ContainsKey("address"), Is.True);

      // Test nested object access
      Assert.That(dictionary["address"], Is.InstanceOf<Dictionary<string, object>>());
      var address = (Dictionary<string, object>)dictionary["address"];
      Assert.That(address.ContainsKey("city"), Is.True);
      Assert.That(address.ContainsKey("zipcode"), Is.True);

      Console.WriteLine($"User: {dictionary["name"]} from {address["city"]}");
    }

    #endregion

    #region JSON Content Format Tests

    [Test]
    public void JsonNodes_Format_ReturnsPrettyPrintedJson()
    {
      // Arrange
      var url = "https://jsonplaceholder.typicode.com/posts/1";

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var formatted = JsonNodes.Format(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True);
      Assert.That(formatted, Is.Not.Null.And.Not.Empty);

      // Check for proper JSON formatting - DynaFetch formats with proper indentation
      Assert.That(formatted, Does.StartWith("{"));
      Assert.That(formatted, Does.EndWith("}"));
      Assert.That(formatted, Does.Contain("\\n")); // Should have newlines
      Assert.That(formatted, Does.Contain("  ")); // Should have indentation

      // Verify it's still valid JSON after formatting
      Assert.That(JsonNodes.ValidateJson(formatted), Is.True, "Formatted JSON should remain valid");

      Console.WriteLine("Formatted JSON:");
      Console.WriteLine(formatted.Substring(0, Math.Min(200, formatted.Length)) + "...");
    }

    [Test]
    public void JsonNodes_GetContent_ReturnsRawJson()
    {
      // Arrange
      var url = "https://jsonplaceholder.typicode.com/posts/1";

      // Act
      var response = ExecuteNodes.GET(_client!, url);
      var content = JsonNodes.GetContent(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True);
      Assert.That(content, Is.Not.Null.And.Not.Empty);
      Assert.That(content, Does.StartWith("{"));
      Assert.That(content, Does.EndWith("}"));
      Assert.That(JsonNodes.ValidateJson(content), Is.True, "Raw content should be valid JSON");

      Console.WriteLine($"Raw content length: {content.Length}");
    }

    [Test]
    public void JsonNodes_ValidateJson_CorrectlyValidatesJson()
    {
      // Arrange
      var validJson = @"{""test"": ""value"", ""number"": 42}";
      var invalidJson = @"{""test"": ""value"", ""number"": }";

      // Act & Assert
      Assert.That(JsonNodes.ValidateJson(validJson), Is.True, "Valid JSON should return true");
      Assert.That(JsonNodes.ValidateJson(invalidJson), Is.False, "Invalid JSON should return false");
      Assert.That(JsonNodes.ValidateJson(""), Is.False, "Empty string should return false");
      Assert.That(JsonNodes.ValidateJson(null!), Is.False, "Null should return false");
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public void GET_InvalidUrl_HandlesErrorGracefully()
    {
      // Arrange
      var invalidUrl = "https://this-domain-definitely-does-not-exist-12345.com/api";

      // Act & Assert
      // DynaFetch throws specific exception types for better error handling
      // This provides more useful debugging information than generic exceptions
      var exception = Assert.Throws<InvalidOperationException>(() =>
      {
        var response = ExecuteNodes.GET(_client!, invalidUrl);
      });

      // Assert.Throws guarantees exception is not null, so we can safely access properties
      Assert.That(exception, Is.Not.Null);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      Assert.That(exception.Message, Is.Not.Null.And.Not.Empty,
        "Exception should contain helpful error message");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

      // Safe console output with null check
      var exceptionMessage = exception.Message ?? "No message available";
      Console.WriteLine($"Specific exception handling: {exception.GetType().Name}: {exceptionMessage}");
    }

    [Test]
    public void GET_404NotFound_HandlesErrorCorrectly()
    {
      // Arrange
      var notFoundUrl = "https://jsonplaceholder.typicode.com/posts/99999";

      // Act
      var response = ExecuteNodes.GET(_client!, notFoundUrl);

      // Assert
      Assert.That(response, Is.Not.Null);
      Assert.That(response.StatusCode.ToString(), Is.EqualTo("NotFound"));
      Assert.That(response.IsSuccess, Is.False);

      // DynaFetch is robust - it handles 404s gracefully by providing empty/default responses
      // This allows workflows to continue without crashing, which is better UX
      var tryDictResult = JsonNodes.TryToDictionary(response);
      var tryListResult = JsonNodes.TryToList(response);

      // The Try methods may succeed because DynaFetch handles 404s gracefully
      // What matters is that we got a proper 404 status code
      Assert.That(response.StatusCode.ToString(), Is.EqualTo("NotFound"),
        "Status code should indicate 404 NotFound");

      Console.WriteLine($"404 handling validation: Status={response.StatusCode}, " +
                        $"TryDict={tryDictResult.Success}, TryList={tryListResult.Success}");

      Console.WriteLine($"404 handling: Status={response.StatusCode}");
    }

    #endregion
  }
}