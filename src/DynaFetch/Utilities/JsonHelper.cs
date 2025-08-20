using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DynaFetch.Core;

namespace DynaFetch.Utilities
{
  /// <summary>
  /// Comprehensive JSON handling utilities for DynaFetch.
  /// Provides serialization, deserialization, and data conversion for Dynamo.
  /// 
  /// Uses System.Text.Json as primary engine for performance, with Newtonsoft.Json
  /// as fallback for complex scenarios that require more flexible parsing.
  /// 
  /// Key Features:
  /// - Fast System.Text.Json serialization/deserialization
  /// - Newtonsoft.Json fallback for compatibility
  /// - Dynamo-friendly type conversions (objects to Lists/Dictionaries)
  /// - Safe parsing that won't crash Dynamo graphs
  /// - JSON querying and manipulation utilities
  /// </summary>
  public static class JsonHelper
  {
    #region Configuration

    /// <summary>
    /// Default System.Text.Json options for consistent parsing behavior.
    /// Configured for web API compatibility and case-insensitive properties.
    /// </summary>
    private static readonly JsonSerializerOptions DefaultSystemJsonOptions = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
      AllowTrailingCommas = true,
      ReadCommentHandling = JsonCommentHandling.Skip,
      NumberHandling = JsonNumberHandling.AllowReadingFromString,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Default Newtonsoft.Json settings for fallback scenarios.
    /// Configured for maximum compatibility and flexible parsing.
    /// </summary>
    private static readonly JsonSerializerSettings DefaultNewtonsoftSettings = new JsonSerializerSettings
    {
      NullValueHandling = NullValueHandling.Ignore,
      MissingMemberHandling = MissingMemberHandling.Ignore,
      DateParseHandling = DateParseHandling.DateTimeOffset,
      FloatParseHandling = FloatParseHandling.Double,
      Formatting = Formatting.Indented
    };

    #endregion

    #region Core Serialization Methods

    /// <summary>
    /// Serializes an object to JSON string using System.Text.Json.
    /// This is the fastest and most modern approach for simple objects.
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="options">Optional custom JsonSerializerOptions</param>
    /// <returns>JSON string representation</returns>
    /// <exception cref="DynaFetchJsonException">Thrown when serialization fails</exception>
    public static string Serialize(object obj, JsonSerializerOptions? options = null)
    {
      if (obj == null)
        return "null";

      try
      {
        var jsonOptions = options ?? DefaultSystemJsonOptions;
        return System.Text.Json.JsonSerializer.Serialize(obj, jsonOptions);
      }
      catch (Exception ex)
      {
        throw new Core.DynaFetchJsonException(
                            $"Failed to serialize object of type {obj.GetType().Name} to JSON",
                            null,
                            ex);
      }
    }

    /// <summary>
    /// Serializes an object to JSON using Newtonsoft.Json as fallback.
    /// Use this when System.Text.Json fails or for complex object structures.
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <param name="settings">Optional custom JsonSerializerSettings</param>
    /// <returns>JSON string representation</returns>
    public static string SerializeWithNewtonsoft(object obj, JsonSerializerSettings? settings = null)
    {
      if (obj == null)
        return "null";

      try
      {
        var jsonSettings = settings ?? DefaultNewtonsoftSettings;
        return JsonConvert.SerializeObject(obj, jsonSettings);
      }
      catch (Exception ex)
      {
        throw new Core.DynaFetchJsonException(
                            $"Failed to serialize object of type {obj.GetType().Name} to JSON using Newtonsoft.Json",
                            null,
                            ex);
      }
    }

    /// <summary>
    /// Smart serialization that tries System.Text.Json first, falls back to Newtonsoft.Json.
    /// This gives you the best of both worlds: speed when possible, compatibility when needed.
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>JSON string representation</returns>
    public static string SerializeSmart(object obj)
    {
      if (obj == null)
        return "null";

      try
      {
        // Try System.Text.Json first for performance
        return Serialize(obj);
      }
      catch (Exception)
      {
        try
        {
          // Fall back to Newtonsoft.Json for compatibility
          return SerializeWithNewtonsoft(obj);
        }
        catch (Exception ex)
        {
          throw new Core.DynaFetchJsonException(
                              $"Failed to serialize object of type {obj.GetType().Name} with both JSON libraries",
                              null,
                              ex);
        }
      }
    }

    #endregion

    #region Core Deserialization Methods

    /// <summary>
    /// Deserializes JSON string to specified type using System.Text.Json.
    /// Fast and efficient for simple types and DTOs.
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="options">Optional custom JsonSerializerOptions</param>
    /// <returns>Deserialized object of type T</returns>
    /// <exception cref="DynaFetchJsonException">Thrown when deserialization fails</exception>
    public static T Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new Core.DynaFetchJsonException("Cannot deserialize null or empty JSON string");

      try
      {
        var jsonOptions = options ?? DefaultSystemJsonOptions;
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(json, jsonOptions);

        if (result == null && !typeof(T).IsClass)
          throw new Core.DynaFetchJsonException($"Deserialization resulted in null for non-nullable type {typeof(T).Name}");

        return result!;
      }
      catch (System.Text.Json.JsonException ex)
      {
        throw new Core.DynaFetchJsonException(
    $"Failed to deserialize JSON to type {typeof(T).Name}: {ex.Message}", null,
    ex);
      }
      catch (Exception ex)
      {
        throw new Core.DynaFetchJsonException(
            $"Unexpected error deserializing JSON to type {typeof(T).Name}", null,
            ex);
      }
    }

    /// <summary>
    /// Deserializes JSON string using Newtonsoft.Json as fallback.
    /// Use this for complex objects or when System.Text.Json fails.
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="settings">Optional custom JsonSerializerSettings</param>
    /// <returns>Deserialized object of type T</returns>
    public static T DeserializeWithNewtonsoft<T>(string json, JsonSerializerSettings? settings = null)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new Core.DynaFetchJsonException("Cannot deserialize null or empty JSON string");

      try
      {
        var jsonSettings = settings ?? DefaultNewtonsoftSettings;
        var result = JsonConvert.DeserializeObject<T>(json, jsonSettings);

        if (result == null && !typeof(T).IsClass)
          throw new Core.DynaFetchJsonException($"Deserialization resulted in null for non-nullable type {typeof(T).Name}");

        return result!;
      }
      catch (Newtonsoft.Json.JsonException ex)
      {
        throw new Core.DynaFetchJsonException(
    $"Failed to deserialize JSON to type {typeof(T).Name} using Newtonsoft.Json: {ex.Message}", null,
    ex);
      }
      catch (Exception ex)
      {
        throw new Core.DynaFetchJsonException(
            $"Unexpected error deserializing JSON to type {typeof(T).Name} using Newtonsoft.Json", null,
            ex);
      }
    }

    /// <summary>
    /// Smart deserialization that tries System.Text.Json first, falls back to Newtonsoft.Json.
    /// Provides maximum compatibility while maintaining performance.
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized object of type T</returns>
    public static T DeserializeSmart<T>(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        throw new Core.DynaFetchJsonException("Cannot deserialize null or empty JSON string");

      try
      {
        // Try System.Text.Json first for performance
        return Deserialize<T>(json);
      }
      catch (Exception)
      {
        try
        {
          // Fall back to Newtonsoft.Json for compatibility
          return DeserializeWithNewtonsoft<T>(json);
        }
        catch (Exception ex)
        {
          throw new Core.DynaFetchJsonException(
              $"Failed to deserialize JSON to type {typeof(T).Name} with both JSON libraries", null,
              ex);
        }
      }
    }

    #endregion

    #region Safe Operations (Try Methods)

    /// <summary>
    /// Safely attempts to deserialize JSON without throwing exceptions.
    /// Returns tuple with success flag and result. Perfect for Dynamo graphs.
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Tuple with (success: bool, result: T, error: string)</returns>
    public static (bool success, T? result, string error) TryDeserialize<T>(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return (false, default(T), "JSON string is null or empty");

      try
      {
        var result = DeserializeSmart<T>(json);
        return (true, result, string.Empty);
      }
      catch (Exception ex)
      {
        return (false, default(T), ex.Message);
      }
    }

    /// <summary>
    /// Safely attempts to serialize object without throwing exceptions.
    /// Returns tuple with success flag and result. Perfect for Dynamo graphs.
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>Tuple with (success: bool, json: string, error: string)</returns>
    public static (bool success, string json, string error) TrySerialize(object obj)
    {
      if (obj == null)
        return (true, "null", string.Empty);

      try
      {
        var json = SerializeSmart(obj);
        return (true, json, string.Empty);
      }
      catch (Exception ex)
      {
        return (false, string.Empty, ex.Message);
      }
    }

    #endregion

    #region Dynamo-Friendly Conversions

    /// <summary>
    /// Converts JSON string to Dynamo-friendly Dictionary&lt;string, object&gt;.
    /// This is perfect for Dynamo graphs that work with key-value pairs.
    /// Handles nested objects by converting them to nested dictionaries.
    /// </summary>
    /// <param name="json">JSON string to convert</param>
    /// <returns>Dictionary representation of JSON, or null if conversion fails</returns>
    public static Dictionary<string, object>? JsonToDictionary(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return null;

      try
      {
        // Use Newtonsoft.Json for this because it handles dynamic objects better
        var jObject = JObject.Parse(json);
        return JObjectToDictionary(jObject);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    /// Converts JSON array string to List of Dynamo-friendly objects.
    /// Arrays become Lists, objects become Dictionaries, primitives stay as-is.
    /// </summary>
    /// <param name="json">JSON array string to convert</param>
    /// <returns>List representation of JSON array, or null if conversion fails</returns>
    public static List<object>? JsonToList(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return null;

      try
      {
        var jArray = JArray.Parse(json);
        return JArrayToList(jArray);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    /// Converts any JSON string to the most appropriate Dynamo-friendly type.
    /// - Objects become Dictionary&lt;string, object&gt;
    /// - Arrays become List&lt;object&gt;
    /// - Primitives (string, number, bool) stay as-is
    /// </summary>
    /// <param name="json">JSON string to convert</param>
    /// <returns>Dynamo-friendly representation, or original string if conversion fails</returns>
    public static object JsonToObject(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return json;

      try
      {
        var jToken = JToken.Parse(json);
        return JTokenToObject(jToken);
      }
      catch (Exception)
      {
        // If parsing fails, return the original string
        return json;
      }
    }

    /// <summary>
    /// Converts Dictionary&lt;string, object&gt; back to JSON string.
    /// Perfect for taking Dynamo dictionaries and converting to JSON for API calls.
    /// </summary>
    /// <param name="dictionary">Dictionary to convert to JSON</param>
    /// <returns>JSON string representation</returns>
    public static string DictionaryToJson(Dictionary<string, object> dictionary)
    {
      if (dictionary == null)
        return "null";

      try
      {
        return SerializeSmart(dictionary);
      }
      catch (Exception ex)
      {
        throw new Core.DynaFetchJsonException(
            "Failed to convert dictionary to JSON", null, ex);
      }
    }

    /// <summary>
    /// Converts List&lt;object&gt; back to JSON array string.
    /// Perfect for taking Dynamo lists and converting to JSON arrays for API calls.
    /// </summary>
    /// <param name="list">List to convert to JSON array</param>
    /// <returns>JSON array string representation</returns>
    public static string ListToJson(List<object> list)
    {
      if (list == null)
        return "null";

      try
      {
        return SerializeSmart(list);
      }
      catch (Exception ex)
      {
        throw new Core.DynaFetchJsonException(
            "Failed to convert list to JSON", null, ex); ;
      }
    }

    #endregion

    #region JSON Validation and Utilities

    /// <summary>
    /// Validates if a string is valid JSON without throwing exceptions.
    /// Perfect for checking data before processing in Dynamo graphs.
    /// </summary>
    /// <param name="json">String to validate</param>
    /// <returns>True if valid JSON, false otherwise</returns>
    public static bool IsValidJson(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return false;

      try
      {
        JToken.Parse(json);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    /// Formats/prettifies JSON string with proper indentation.
    /// Great for making JSON readable in Dynamo outputs.
    /// </summary>
    /// <param name="json">JSON string to format</param>
    /// <returns>Formatted JSON string, or original if formatting fails</returns>
    public static string FormatJson(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return json;

      try
      {
        var jToken = JToken.Parse(json);
        return jToken.ToString(Formatting.Indented);
      }
      catch (Exception)
      {
        return json;
      }
    }

    /// <summary>
    /// Minifies JSON string by removing unnecessary whitespace.
    /// Useful for reducing payload size for API calls.
    /// </summary>
    /// <param name="json">JSON string to minify</param>
    /// <returns>Minified JSON string, or original if minification fails</returns>
    public static string MinifyJson(string json)
    {
      if (string.IsNullOrWhiteSpace(json))
        return json;

      try
      {
        var jToken = JToken.Parse(json);
        return jToken.ToString(Formatting.None);
      }
      catch (Exception)
      {
        return json;
      }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Recursively converts JObject to Dictionary&lt;string, object&gt;.
    /// Handles nested objects and arrays properly.
    /// </summary>
    private static Dictionary<string, object> JObjectToDictionary(JObject jObject)
    {
      var dictionary = new Dictionary<string, object>();

      foreach (var property in jObject.Properties())
      {
        dictionary[property.Name] = JTokenToObject(property.Value);
      }

      return dictionary;
    }

    /// <summary>
    /// Recursively converts JArray to List&lt;object&gt;.
    /// Handles nested objects and arrays properly.
    /// </summary>
    private static List<object> JArrayToList(JArray jArray)
    {
      var list = new List<object>();

      foreach (var item in jArray)
      {
        list.Add(JTokenToObject(item));
      }

      return list;
    }

    /// <summary>
    /// Converts any JToken to appropriate .NET object.
    /// This is the core conversion logic for Dynamo-friendly types.
    /// </summary>
    private static object JTokenToObject(JToken jToken)
    {
      switch (jToken.Type)
      {
        case JTokenType.Object:
          return JObjectToDictionary((JObject)jToken);

        case JTokenType.Array:
          return JArrayToList((JArray)jToken);

        case JTokenType.String:
          return jToken.Value<string>() ?? string.Empty;

        case JTokenType.Integer:
          return jToken.Value<long>();

        case JTokenType.Float:
          return jToken.Value<double>();

        case JTokenType.Boolean:
          return jToken.Value<bool>();

        case JTokenType.Date:
          return jToken.Value<DateTime>();

        case JTokenType.Null:
        case JTokenType.Undefined:
          return null!;

        default:
          // For any other type, convert to string
          return jToken.ToString();
      }
    }

    #endregion
  }
}