# DynaFetch {🐕}

<img width="512" height="512" alt="DynaFetch_Logo_O_Tag" src="https://github.com/user-attachments/assets/1b6ba9bc-b950-4a4e-a217-6d9f766eeba0" />

**Modern REST API integration for Dynamo 3.0**

> **🌐 [View DynaFetch Landing Page](https://thebimsider.github.io/DynaFetch/landing/)** - Interactive overview with examples and documentation links

DynaFetch is a clean, modern .NET 8 package that brings REST API capabilities to Dynamo 3.0. Built for performance and ease of use, it provides a comprehensive HTTP client with robust JSON processing and enterprise-ready authentication support.

## Development & Attribution

**DynaFetch** was developed through a collaborative "Vibe Coding" project between [**TheBIMsider**](https://bio.link/thebimsider) and **Claude** [(Anthropic AI)](https://www.anthropic.com/). This partnership combined domain expertise in BIM and construction technology with AI-assisted development to create a modern, efficient REST API package for the Dynamo community.

**Development Approach**: Iterative development with comprehensive testing, focusing on real-world usage patterns and professional-grade reliability.

**Inspiration**: DynaFetch is inspired by and builds upon the pioneering work of **Radu Gidei** and the **DynaWeb** package:

- **DynaWeb Repository**: https://github.com/radumg/DynaWeb
- **DynaWeb Website**: https://radumg.github.io/DynaWeb/
- **Radu Gidei's Profile**: https://github.com/radumg

DynaWeb established the foundation for REST API integration in Dynamo. DynaFetch continues this legacy with modern .NET 8 architecture, enhanced JSON processing, and streamlined authentication patterns. We're grateful for Radu's contribution to the Dynamo community.

## Quick Start

### Installation

1. Open Dynamo 3.0 or later
2. Go to **Packages** → **Search for a Package**
3. Search for "DynaFetch"
4. Click **Download** and restart Dynamo

### Your First API Call (3 nodes)

```
1. ClientNodes.Create → Creates HTTP client
2. ExecuteNodes.GET(client, "https://api.github.com/users/octocat") → Makes API call
3. JsonNodes.ToDictionary(response) → Converts JSON to Dynamo Dictionary
```

That's it! You now have GitHub user data as a Dynamo Dictionary.

## Basic Examples

### Simple GET Request

```
Create Client → GET Request → Process JSON
```

**Nodes**:

1. `ClientNodes.Create()` → client
2. `ExecuteNodes.GET(client, "https://jsonplaceholder.typicode.com/posts/1")` → response
3. `JsonNodes.ToDictionary(response)` → dictionary

**Result**: Post data as Dynamo Dictionary with keys like "userId", "title", "body"

### Authenticated API Access

```
Create Client → Add Authentication → GET Request → Process Response
```

**Nodes**:

1. `ClientNodes.Create()` → client
2. `ClientNodes.AddDefaultHeader(client, "Authorization", "Bearer YOUR_TOKEN")` → client
3. `ExecuteNodes.GET(client, "https://httpbin.org/bearer")` → response
4. `JsonNodes.ToDictionary(response)` → dictionary

**Authentication persists**: All subsequent requests from this client automatically include the Bearer token.

### POST Data to API

```
Create Client → Build JSON → POST Request → Process Response
```

**Nodes**:

1. `ClientNodes.Create()` → client
2. `JsonNodes.DictionaryToJson(your_dictionary)` → json_string
3. `ExecuteNodes.POST(client, "https://api.example.com/data", json_string)` → response
4. `JsonNodes.ToDictionary(response)` → result_dictionary

## Core Concepts

### Client Management

- **Create once, use everywhere**: Create a client and reuse it for multiple requests
- **Persistent authentication**: Add default headers (like API keys) that apply to all requests
- **Configuration**: Set timeouts, base URLs, and custom headers at the client level

### Authentication Patterns

DynaFetch supports modern API authentication:

**Bearer Tokens** (OAuth/JWT):

```
ClientNodes.AddDefaultHeader(client, "Authorization", "Bearer " + token)
```

**API Keys**:

```
ClientNodes.AddDefaultHeader(client, "X-API-Key", api_key)
```

**Custom Headers**:

```
ClientNodes.AddDefaultHeader(client, "Custom-Auth", auth_value)
```

**JWT Assertions** (Service Accounts):

```
ClientNodes.GenerateJwtAssertion(privateKeyPem, clientId, audience, scopes, 60)
```

#### JWT Assertion Workflow (Autodesk SSA Example)

For service account authentication like Autodesk Platform Services Secure Service Accounts (SSA):

```
1. ClientNodes.GenerateJwtAssertion(privateKeyPem, clientId, "https://developer.api.autodesk.com/", ["data:read", "data:write"], 60) → jwt_assertion

2. Build token exchange body:
   "grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion=" + jwt_assertion → request_body

3. ClientNodes.Create() → client

4. ExecuteNodes.POST(client, "https://developer.api.autodesk.com/authentication/v2/token", request_body) → token_response

5. JsonNodes.ToDictionary(token_response) → token_dict

6. Dictionary.ValueAtKey(token_dict, "access_token") → access_token

7. ClientNodes.AddDefaultHeader(client, "Authorization", "Bearer " + access_token) → authenticated_client

8. Use authenticated_client for subsequent API calls
```

**Supported JWT Use Cases**:

- Autodesk Platform Services Secure Service Accounts (SSA)
- Google Service Accounts
- Custom OAuth 2.0 JWT Bearer flows
- Any RFC 7523 compliant JWT assertion authentication

### JSON Processing

- **ToDictionary**: Convert API responses to Dynamo Dictionaries
- **ToList**: Convert JSON arrays to Dynamo Lists
- **DictionaryToJson**: Convert Dynamo data to JSON for API submission
- **Format**: Pretty-print JSON for debugging
- **Serialize/Deserialize**: Advanced JSON operations

## Samples

DynaFetch includes 22 comprehensive sample graphs organized into three categories for complete learning and reference.

### Quick Start Path

1. **Start with DynaFetch_Samples** - Learn core concepts and basic patterns
2. **Explore DynaFetched_DynaWeb_Samples** - See practical migration examples from DynaWeb
3. **Reference DynaFetch_Nodes_Groups** - Detailed visual documentation of all available nodes

### DynaFetch_Samples (7 Original Samples)

Core functionality demonstrations perfect for learning DynaFetch basics:

- **01_Basic-GET-Request.dyn** - Simplest HTTP request pattern using DynaFetch for beginners
- **02_Authenticated-API.dyn** - Bearer token authentication with client-level auth setup
- **03_POST-Data-Submission.dyn** - Complete structured data submission workflow
- **04_Multi-Step-Workflow.dyn** - Complex CRUD operations with authentication
- **05_Error-Handling-Pattern.dyn** - Graceful API error handling instead of workflow crashes
- **06_JSON-Processing-Demo.dyn** - Multiple JSON processing methods for different response types
- **07_Fun-GET-Random-Ron_Swanson_Quote.dyn** - Simple public API demonstration using Ron Swanson Quotes API

### DynaFetched_DynaWeb_Samples (7 Migration Examples)

Practical migration examples showing DynaWeb to DynaFetch transitions:

- **1 - First-Request-DynaFetch.dyn** - DynaWeb to DynaFetch migration of basic HTTP request pattern
- **2 - Simple-Request-DynaFetch.dyn** - Recreates DynaWeb's multi-endpoint JSON processing workflow
- **3 - Simple-Request-Benchmarking-DynaFetch.dyn** - Comprehensive response analysis with detailed metadata extraction
- **4 - REST-API-Example-DynaFetch.dyn** - Complex JSON deserialization with multiple data structure handling
- **5 - REST-API-Advanced-DynaFetch.dyn** - Multi-step authenticated API workflow with POST data submission
- **6 - Complex-POST-Request-DynaFetch.dyn** - Advanced POST operations with JSON body creation and response processing
- **7 - GET-ACC-Projects-and-Users-DynaFetch.dyn** - Modern Autodesk Platform Services integration (updated from legacy Forge)

### DynaFetch_Nodes-Groups (8 Node Reference Samples)

Complete node documentation organized by functional groups:

- **DynaFetch.dyn** - Overview of DynaFetch nodes
- **DynaFetch_Core.dyn** - HTTP client creation and configuration nodes
- **DynaFetch_Nodes.dyn** - Request execution nodes for GET, POST, PUT, DELETE operations
- **DynaFetch_Package_Nodes.dyn** - Complete DynaFetch node collection including DynaFetch and System nodes
- **DynaFetch_Utilities.dyn** - Advanced JSON manipulation and serialization tools
- **System.dyn** - Overview of System Nodes
- **System_Exception.dyn** - Exception handling
- **System_Net.dyn** .NET HttpStatusCode integration nodes

### Sample Usage

All samples are located in the `samples/` folder and can be opened directly in Dynamo. Each sample includes:

- Detailed annotations explaining the workflow
- Color-coded sections for easy navigation
- Real working endpoints for immediate testing
- Progressive complexity from basic to advanced patterns

**Recommended Learning Path**: Start with Basic-GET-Request, progress through the original samples, then explore migration examples to see practical real-world patterns.

## Node Categories

### Client Nodes (Connection Management)

- `Create()` - Create new HTTP client
- `CreateWithBaseUrl(baseUrl)` - Create client with base URL
- `SetTimeout(client, seconds)` - Set request timeout
- `AddDefaultHeader(client, name, value)` - Add persistent header
- `SetUserAgent(client, userAgent)` - Set client user agent
- `GenerateJwtAssertion(privateKeyPem, clientId, audience, scopes, expirationMinutes)` - Generate JWT assertion for service account authentication

### Execute Nodes (HTTP Operations)

- `GET(client, url)` - HTTP GET request
- `POST(client, url, jsonData)` - HTTP POST with JSON
- `PUT(client, url, jsonData)` - HTTP PUT with JSON
- `DELETE(client, url)` - HTTP DELETE request
- `PATCH(client, url, jsonData)` - HTTP PATCH with JSON

### JSON Nodes (Data Processing)

- `ToDictionary(response)` - Response to Dictionary
- `ToList(response)` - Response to List
- `DictionaryToJson(dictionary)` - Dictionary to JSON string
- `Format(json)` - Pretty-print JSON
- `IsValid(json)` - Validate JSON syntax
- `GetContent(response)` - Get raw response text

## Common Workflows

### Workflow 1: Public API Integration

**Use Case**: Get weather data, stock prices, public datasets

```
ClientNodes.Create()
↓
ExecuteNodes.GET(client, "https://api.openweathermap.org/data/2.5/weather?q=London&appid=YOUR_KEY")
↓
JsonNodes.ToDictionary(response)
↓
Extract data: dictionary["main"]["temp"]
```

### Workflow 2: Authenticated Data Submission

**Use Case**: Submit form data, create records, upload information

```
ClientNodes.Create()
↓
ClientNodes.AddDefaultHeader(client, "Authorization", "Bearer " + token)
↓
JsonNodes.DictionaryToJson(your_data)
↓
ExecuteNodes.POST(client, "https://api.example.com/records", json_data)
↓
JsonNodes.ToDictionary(response)
```

### Workflow 3: Multi-Step API Operations

**Use Case**: Login, then perform operations with session

```
1. POST login credentials → Get auth token
2. Add auth token to client headers
3. GET/POST/PUT operations with authenticated client
4. Process responses with JSON nodes
```

## Performance & Best Practices

### Client Reuse

- **Do**: Create one client, use for multiple requests
- **Don't**: Create new client for every request

### Authentication Management

- **Do**: Use `AddDefaultHeader` for persistent authentication
- **Don't**: Add auth headers to individual requests

### Error Handling

- Check `response.IsSuccessful` before processing JSON
- Use `JsonNodes.IsValid()` to validate JSON before parsing
- Handle network timeouts with appropriate client timeout settings

### JSON Processing

- Use `ToDictionary` for JSON objects
- Use `ToList` for JSON arrays
- Use `Format` for debugging JSON structure

## Documentation

### Quick References

- **[Node Library Reference](docs/Node-Library.md)** - Primary workflow nodes as they appear in Dynamo
- **[Advanced Node Library](docs/Advanced-Node-Library.md)** - Complete reference including Core, System, and all HTTP status codes
- **[API Documentation](docs/API-Documentation.md)** - Complete method reference with parameters and examples

### Guides & Best Practices

- **[Best Practices](docs/Best-Practices.md)** - Security, authentication, performance, and workflow organization
- **[Migration Guide](docs/Migration-Guide.md)** - Step-by-step transition from DynaWeb to DynaFetch
- **[Troubleshooting](docs/Troubleshooting.md)** - Problem resolution and common issues

### Getting Started

- **[Installation & Quick Start](#quick-start)** - Get up and running in 15 minutes
- **[Basic Examples](#basic-examples)** - Common workflow patterns
- **[Sample Graphs](samples/)** - Working Dynamo examples for download

## Troubleshooting

### Common Issues

**"Cannot connect to API"**

- Check internet connectivity
- Verify API URL is correct
- Check if API requires authentication

**"JSON parsing failed"**

- Use `JsonNodes.IsValid()` to check JSON syntax
- Use `JsonNodes.GetContent()` to see raw response
- Check if API returned error message instead of JSON

**"Authentication failed"**

- Verify API key/token is correct
- Check authentication header format
- Ensure token hasn't expired

For comprehensive troubleshooting help, see the [Troubleshooting Guide](docs/Troubleshooting.md).

## Contributing

DynaFetch is open source under the BSD-3-Clause license. Contributions are welcome!

### Development Setup

1. Clone repository
2. Open in VS Code with C# Dev Kit extension
3. Ensure .NET 8 SDK installed
4. Run tests: `dotnet test`

### Reporting Issues

- Use GitHub Issues for bug reports
- Include Dynamo version and sample graph when possible
- Provide API endpoint and expected vs actual behavior

## License & Attribution

**DynaFetch** is licensed under the BSD-3-Clause License.

**DynaWeb Attribution**: This project builds upon patterns and concepts pioneered by Radu Gidei's DynaWeb package. We acknowledge and appreciate the foundational work that made REST API integration possible in Dynamo.

## Support

- **Documentation**: See `docs/` folder for comprehensive guides
- **Examples**: Check `samples/` folder for Dynamo graph examples
- **Issues**: Report bugs and feature requests on GitHub
- **Community**: Join Dynamo community forums for usage questions

---

_DynaFetch - Modern REST APIs for Modern Dynamo_
