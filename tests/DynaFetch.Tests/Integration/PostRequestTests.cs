using System.Collections.Generic;
using NUnit.Framework;
using DynaFetch.Core;
using DynaFetch.Nodes;

namespace DynaFetch.Tests.Integration
{
  [TestFixture]
  public class PostRequestTests
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

    [Test]
    public void POST_JSONPlaceholder_CreatePost_ReturnsNewPost()
    {
      // Arrange
      var url = "https://jsonplaceholder.typicode.com/posts";
      var postData = @"{
                ""title"": ""DynaFetch Test Post"",
                ""body"": ""This post was created by DynaFetch during testing"",
                ""userId"": 1
            }";

      // Act
      var response = ExecuteNodes.POST(_client!, url, postData);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "POST should succeed");
      Assert.That(result, Is.Not.Null);
      Assert.That(result.ContainsKey("id"), Is.True, "Response should contain new ID");
      Assert.That(result.ContainsKey("title"), Is.True);
      Assert.That(result.ContainsKey("body"), Is.True);
      Assert.That(result.ContainsKey("userId"), Is.True);

      // JSONPlaceholder returns ID 101 for new posts
      Assert.That(result["id"], Is.EqualTo(101));
      Assert.That(result["title"], Is.EqualTo("DynaFetch Test Post"));

      Console.WriteLine($"Created post with ID: {result["id"]}");
    }

    [Test]
    public void POST_HttpBin_EchoRequest_ReturnsRequestData()
    {
      // Arrange
      var url = "https://httpbin.org/post";
      var testData = @"{
                ""project"": ""DynaFetch"",
                ""version"": ""1.0"",
                ""testing"": true
            }";

      // Act
      var response = ExecuteNodes.POST(_client!, url, testData);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True, "POST should succeed");
      Assert.That(result, Is.Not.Null);
      Assert.That(result.ContainsKey("json"), Is.True, "httpbin should echo JSON data");

      // Check that our data was echoed back
      var jsonData = (Dictionary<string, object>)result["json"];
      Assert.That(jsonData["project"], Is.EqualTo("DynaFetch"));
      Assert.That(jsonData["version"], Is.EqualTo("1.0"));
      Assert.That(jsonData["testing"], Is.EqualTo(true));

      Console.WriteLine($"Echo test successful: {jsonData["project"]}");
    }

    [Test]
    public void POST_DictionaryToJson_WorksCorrectly()
    {
      // Arrange
      var url = "https://httpbin.org/post";
      var dataDict = new Dictionary<string, object>();
      dataDict["name"] = "DynaFetch Test";
      dataDict["type"] = "Integration Test";
      dataDict["timestamp"] = "2025-08-20";
      dataDict["success"] = true;
      dataDict["count"] = 42;

      // Convert dictionary to JSON using JsonNodes
      var jsonData = JsonNodes.DictionaryToJson(dataDict);

      // Act
      var response = ExecuteNodes.POST(_client!, url, jsonData);
      var result = JsonNodes.ToDictionary(response);

      // Assert
      Assert.That(response.IsSuccess, Is.True);
      Assert.That(result.ContainsKey("json"), Is.True);

      var echoed = (Dictionary<string, object>)result["json"];
      Assert.That(echoed["name"], Is.EqualTo("DynaFetch Test"));
      Assert.That(echoed["type"], Is.EqualTo("Integration Test"));
      Assert.That(echoed["success"], Is.EqualTo(true));
      Assert.That(echoed["count"], Is.EqualTo(42));

      Console.WriteLine("Dictionary POST conversion successful");
    }
  }
}