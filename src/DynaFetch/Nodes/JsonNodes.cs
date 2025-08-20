using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynaFetch.Core;
using DynaFetch.Utilities;

namespace DynaFetch.Nodes
{
  /// <summary>
  /// JSON utility nodes for DynaFetch
  /// These nodes help you work with JSON data from API responses in Dynamo-friendly ways
  /// </summary>
  public static class JsonNodes
  {
    /// <summary>
    /// Convert HTTP response JSON to a Dynamo-friendly Dictionary
    /// Perfect for working with JSON objects in Dynamo
    /// </summary>
    /// <param name="response">HTTP response containing JSON</param>
    /// <returns>Dictionary&lt;string, object&gt; for use in Dynamo</returns>
    public static Dictionary<string, object> ToDictionary(HttpResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response), "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.GetJsonAsDictionaryAsync();
        return task.GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to convert response to Dictionary: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Convert HTTP response JSON array to a Dynamo-friendly List
    /// Perfect for working with JSON arrays in Dynamo
    /// </summary>
    /// <param name="response">HTTP response containing JSON array</param>
    /// <returns>List&lt;object&gt; for use in Dynamo</returns>
    public static List<object> ToList(HttpResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response), "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.GetJsonAsListAsync();
        return task.GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to convert response to List: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Smart conversion of HTTP response JSON to best Dynamo-friendly type
    /// Automatically detects if response is object, array, or primitive
    /// </summary>
    /// <param name="response">HTTP response containing JSON</param>
    /// <returns>Dictionary, List, or primitive type based on JSON structure</returns>
    public static object ToObject(HttpResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response), "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.GetJsonAsObjectAsync();
        return task.GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to convert response to Object: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Safely convert HTTP response JSON to Dictionary with error information
    /// Returns success/failure status along with result or error message
    /// </summary>
    /// <param name="response">HTTP response containing JSON</param>
    /// <returns>Tuple with success flag, result dictionary, and error message</returns>
    public static (bool Success, Dictionary<string, object>? Result, string ErrorMessage) TryToDictionary(HttpResponse response)
    {
      if (response == null)
        return (false, null, "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.TryGetJsonAsDictionaryAsync();
        var result = task.GetAwaiter().GetResult();
        return result;
      }
      catch (Exception ex)
      {
        return (false, null, $"Failed to convert response to Dictionary: {ex.Message}");
      }
    }

    /// <summary>
    /// Safely convert HTTP response JSON array to List with error information
    /// Returns success/failure status along with result or error message
    /// </summary>
    /// <param name="response">HTTP response containing JSON array</param>
    /// <returns>Tuple with success flag, result list, and error message</returns>
    public static (bool Success, List<object>? Result, string ErrorMessage) TryToList(HttpResponse response)
    {
      if (response == null)
        return (false, null, "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.TryGetJsonAsListAsync();
        var result = task.GetAwaiter().GetResult();
        return result;
      }
      catch (Exception ex)
      {
        return (false, null, $"Failed to convert response to List: {ex.Message}");
      }
    }

    /// <summary>
    /// Get pretty-formatted JSON string from HTTP response
    /// Perfect for debugging or displaying JSON in a readable format
    /// </summary>
    /// <param name="response">HTTP response containing JSON</param>
    /// <returns>Formatted JSON string with proper indentation</returns>
    public static string Format(HttpResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response), "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.GetFormattedJsonAsync();
        return task.GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to format response JSON: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Check if HTTP response contains valid JSON
    /// Returns true if response can be parsed as JSON, false otherwise
    /// </summary>
    /// <param name="response">HTTP response to validate</param>
    /// <returns>True if response contains valid JSON, false otherwise</returns>
    public static bool IsValid(HttpResponse response)
    {
      if (response == null)
        return false;

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.IsValidJsonAsync();
        return task.GetAwaiter().GetResult();
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Deserialize HTTP response JSON to specific type
    /// Use when you know the expected structure of your JSON response
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="response">HTTP response containing JSON</param>
    /// <returns>Deserialized object of type T</returns>
    public static T Deserialize<T>(HttpResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response), "Response cannot be null");

      try
      {
        // Get JSON content and deserialize synchronously
        var contentTask = response.GetContentAsync();
        var jsonContent = contentTask.GetAwaiter().GetResult();
        return JsonHelper.DeserializeSmart<T>(jsonContent);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to deserialize response to {typeof(T).Name}: {ex.Message}", ex);
      }
    }

    /// <summary>
    /// Safely deserialize HTTP response JSON to specific type with error information
    /// Returns success/failure status along with result or error message
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="response">HTTP response containing JSON</param>
    /// <returns>Tuple with success flag, result object, and error message</returns>
    public static (bool Success, T? Result, string ErrorMessage) TryDeserialize<T>(HttpResponse response)
    {
      if (response == null)
        return (false, default, "Response cannot be null");

      try
      {
        // Get JSON content and try to deserialize
        var contentTask = response.GetContentAsync();
        var jsonContent = contentTask.GetAwaiter().GetResult();
        var result = JsonHelper.TryDeserialize<T>(jsonContent);
        return result;
      }
      catch (Exception ex)
      {
        return (false, default, $"Failed to deserialize response to {typeof(T).Name}: {ex.Message}");
      }
    }

    /// <summary>
    /// Get raw content from HTTP response as string
    /// Useful for debugging or working with non-JSON responses
    /// </summary>
    /// <param name="response">HTTP response</param>
    /// <returns>Raw response content as string</returns>
    public static string GetContent(HttpResponse response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response), "Response cannot be null");

      try
      {
        // Convert async call to sync for Dynamo compatibility
        var task = response.GetContentAsync();
        return task.GetAwaiter().GetResult();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed to get response content: {ex.Message}", ex);
      }
    }

    // Static JSON utility methods (for working with JSON strings directly)

    /// <summary>
    /// Convert JSON string to Dynamo-friendly Dictionary
    /// Use when you have JSON as a string and want to work with it in Dynamo
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>Dictionary&lt;string, object&gt; for use in Dynamo</returns>
    public static Dictionary<string, object> JsonToDictionary(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new ArgumentException("JSON cannot be null or empty", nameof(json));

      return JsonHelper.JsonToDictionary(json);
    }

    /// <summary>
    /// Convert JSON array string to Dynamo-friendly List
    /// Use when you have JSON array as a string and want to work with it in Dynamo
    /// </summary>
    /// <param name="json">JSON array string</param>
    /// <returns>List&lt;object&gt; for use in Dynamo</returns>
    public static List<object> JsonToList(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new ArgumentException("JSON cannot be null or empty", nameof(json));

      return JsonHelper.JsonToList(json);
    }

    /// <summary>
    /// Smart conversion of JSON string to best Dynamo-friendly type
    /// Automatically detects if JSON is object, array, or primitive
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>Dictionary, List, or primitive type based on JSON structure</returns>
    public static object JsonToObject(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new ArgumentException("JSON cannot be null or empty", nameof(json));

      return JsonHelper.JsonToObject(json);
    }

    /// <summary>
    /// Convert Dynamo Dictionary to JSON string
    /// Perfect for preparing data to send in POST/PUT requests
    /// </summary>
    /// <param name="dictionary">Dictionary to convert</param>
    /// <returns>JSON string representation</returns>
    public static string DictionaryToJson(Dictionary<string, object> dictionary)
    {
      if (dictionary == null)
        throw new ArgumentNullException(nameof(dictionary), "Dictionary cannot be null");

      return JsonHelper.DictionaryToJson(dictionary);
    }

    /// <summary>
    /// Convert Dynamo List to JSON array string
    /// Perfect for preparing array data to send in POST/PUT requests
    /// </summary>
    /// <param name="list">List to convert</param>
    /// <returns>JSON array string representation</returns>
    public static string ListToJson(List<object> list)
    {
      if (list == null)
        throw new ArgumentNullException(nameof(list), "List cannot be null");

      return JsonHelper.ListToJson(list);
    }

    /// <summary>
    /// Serialize any object to JSON string
    /// Use for converting complex objects to JSON for API requests
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>JSON string representation</returns>
    public static string Serialize(object obj)
    {
      if (obj == null)
        throw new ArgumentNullException(nameof(obj), "Object cannot be null");

      return JsonHelper.SerializeSmart(obj);
    }

    /// <summary>
    /// Pretty-format a JSON string with proper indentation
    /// Perfect for debugging or displaying JSON in a readable format
    /// </summary>
    /// <param name="json">JSON string to format</param>
    /// <returns>Formatted JSON string with proper indentation</returns>
    public static string FormatJson(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new ArgumentException("JSON cannot be null or empty", nameof(json));

      return JsonHelper.FormatJson(json);
    }

    /// <summary>
    /// Validate if a string contains valid JSON
    /// Returns true if string can be parsed as JSON, false otherwise
    /// </summary>
    /// <param name="json">String to validate</param>
    /// <returns>True if string contains valid JSON, false otherwise</returns>
    public static bool ValidateJson(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return false;

      return JsonHelper.IsValidJson(json);
    }

    /// <summary>
    /// Remove whitespace from JSON string to minimize size
    /// Useful for reducing payload size in API requests
    /// </summary>
    /// <param name="json">JSON string to minify</param>
    /// <returns>Minified JSON string without unnecessary whitespace</returns>
    public static string MinifyJson(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new ArgumentException("JSON cannot be null or empty", nameof(json));

      return JsonHelper.MinifyJson(json);
    }
  }
}