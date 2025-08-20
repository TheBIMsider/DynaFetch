using NUnit.Framework;
using DynaFetch.Nodes;

namespace DynaFetch.Tests
{
  [TestFixture]
  public class BasicTests
  {
    [Test]
    public void ClientNodes_Create_ReturnsNonNullClient()
    {
      // Act
      var client = ClientNodes.Create();

      // Assert
      Assert.That(client, Is.Not.Null, "Client should not be null");

      // Cleanup
      client?.Dispose();
    }

    [Test]
    public void JsonNodes_ValidateJson_WorksWithSimpleJson()
    {
      // Arrange
      var validJson = @"{""test"": ""value""}";
      var invalidJson = @"{""test"": }";

      // Act & Assert
      Assert.That(JsonNodes.ValidateJson(validJson), Is.True);
      Assert.That(JsonNodes.ValidateJson(invalidJson), Is.False);
    }

    [Test]
    public void HttpResponse_Properties_WorkCorrectly()
    {
      // Create a client and make a simple request to test HttpResponse
      using var client = ClientNodes.Create();
      var response = ExecuteNodes.GET(client, "https://jsonplaceholder.typicode.com/posts/1");

      // Test the actual properties that exist
      Assert.That(response, Is.Not.Null);
      Assert.That(response.StatusCode, Is.Not.Null);
      Assert.That(response.IsSuccess, Is.True);
    }
  }
}