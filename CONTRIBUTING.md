# Contributing to DynaFetch

Thank you for your interest in contributing to DynaFetch! This document provides guidelines for contributing to the project and helps ensure a smooth collaboration process.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Process](#contributing-process)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Documentation Guidelines](#documentation-guidelines)
- [Issue Guidelines](#issue-guidelines)
- [Pull Request Process](#pull-request-process)
- [Community Support](#community-support)

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please be respectful in all interactions and help us maintain a positive community.

### Expected Behavior

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

### Unacceptable Behavior

- Harassment, discrimination, or offensive comments
- Personal attacks or insults
- Spam or irrelevant content
- Publishing private information without permission

## Getting Started

### Prerequisites

- **Development Environment**: Visual Studio Code with C# Dev Kit extension
- **.NET 8 SDK**: Required for compilation and development
- **Dynamo 3.0+**: For testing (Sandbox or Revit/Civil 3D versions)
- **Git**: For version control and collaboration

### First-Time Contributors

1. **Explore the Documentation**: Review all docs/ files to understand DynaFetch capabilities
2. **Run Sample Workflows**: Test the 6 included sample .dyn files in Dynamo
3. **Review Existing Issues**: Check GitHub Issues for beginner-friendly tasks
4. **Join Discussions**: Participate in GitHub Discussions for questions and ideas

## Development Setup

### 1. Fork and Clone

```bash
# Fork the repository on GitHub
# Clone your fork locally
git clone https://github.com/YOUR_USERNAME/DynaFetch.git
cd DynaFetch
```

### 2. Environment Configuration

```bash
# Open in VS Code
code .

# Verify .NET 8 SDK
dotnet --version

# Restore dependencies
dotnet restore
```

### 3. Build and Test

```bash
# Build the project
dotnet build

# Run tests
cd tests/DynaFetch.Tests
dotnet test
```

### 4. Dynamo Testing Setup

- Install DynaFetch in Dynamo packages folder
- Test sample workflows in samples/ directory
- Verify all nodes load correctly in Dynamo Data category

## Contributing Process

### 1. Discussion First

For significant changes, start with an issue or discussion:

- **Bug Reports**: Use the Bug Report template
- **Feature Requests**: Use the Feature Request template
- **Questions**: Use the Support Question template
- **Major Changes**: Create a GitHub Discussion

### 2. Development Workflow

1. **Create Branch**: `git checkout -b feature/your-feature-name`
2. **Make Changes**: Follow coding standards and test thoroughly
3. **Test Locally**: Ensure all tests pass and Dynamo integration works
4. **Commit Changes**: Use clear, descriptive commit messages
5. **Push Branch**: `git push origin feature/your-feature-name`
6. **Create Pull Request**: Use the PR template and link related issues

## Coding Standards

### C# Code Style

- **Modern C#**: Use .NET 8 features and async/await patterns
- **Clear Naming**: Descriptive variable and method names
- **Documentation**: XML comments for public methods and classes
- **Error Handling**: Comprehensive validation and custom exceptions
- **Resource Management**: Proper IDisposable implementation

### Example Code Structure

```csharp
/// <summary>
/// Executes an HTTP GET request and returns the response
/// </summary>
/// <param name="client">The HTTP client wrapper</param>
/// <param name="url">The target URL</param>
/// <returns>HTTP response with parsed data</returns>
[IsVisibleInDynamoLibrary(true)]
public static HttpResponse GET(HttpClientWrapper client, string url)
{
    // Input validation
    ErrorHandling.ValidateHttpClient(client);
    ErrorHandling.ValidateUrl(url);

    try
    {
        // Implementation with proper error handling
        var response = client.GetAsync(url).GetAwaiter().GetResult();
        return new HttpResponse(response);
    }
    catch (Exception ex)
    {
        throw new DynaFetchException($"GET request failed: {ex.Message}", ex);
    }
}
```

### Node Design Principles

- **Single Responsibility**: Each node should do one thing well
- **Dynamo Compatible**: Return types that work natively in Dynamo
- **Error Resilient**: Handle failures gracefully without crashing graphs
- **Beginner Friendly**: Clear parameter names and helpful error messages

## Testing Requirements

### Unit Tests

- **NUnit Framework**: Use existing test structure in tests/DynaFetch.Tests/
- **Real API Testing**: Include tests with live APIs (JSONPlaceholder, httpbin.org)
- **Error Scenarios**: Test invalid inputs and network failures
- **Performance**: Verify response times within acceptable ranges

### Test Categories

```csharp
[Test]
[Category("BasicFunctionality")]
public void Method_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var client = ClientNodes.Create();

    // Act
    var result = ExecuteNodes.GET(client, "https://jsonplaceholder.typicode.com/posts/1");

    // Assert
    Assert.That(result.IsSuccessful, Is.True);
    Assert.That(result.StatusCode, Is.EqualTo(200));
}
```

### Integration Testing

- **Dynamo Compatibility**: Verify nodes work in actual Dynamo environment
- **Sample Workflows**: Ensure all sample .dyn files execute successfully
- **Multiple API Services**: Test against various real APIs

## Documentation Guidelines

### Code Documentation

- **XML Comments**: All public methods require documentation
- **Parameter Descriptions**: Clear explanation of inputs and outputs
- **Usage Examples**: Include code examples for complex methods
- **Error Information**: Document possible exceptions and their causes

### User Documentation

- **API Reference**: Update docs/API-Documentation.md for new features
- **Best Practices**: Add guidance to docs/Best-Practices.md
- **Troubleshooting**: Include common issues in docs/Troubleshooting.md
- **Migration Guide**: Update docs/Migration-Guide.md for breaking changes

### Sample Workflows

- **Real-world Examples**: Use actual APIs in sample .dyn files
- **Progressive Complexity**: Simple examples for beginners, advanced for experts
- **Documentation**: Include comments explaining workflow steps
- **Testing**: Verify all samples work in both Sandbox and Revit

## Issue Guidelines

### Bug Reports

- **Reproduction Steps**: Detailed steps to reproduce the issue
- **Environment Information**: Dynamo version, .NET version, OS
- **Expected vs Actual**: Clear description of what should happen vs what does
- **Error Messages**: Complete error text and stack traces

### Feature Requests

- **Problem Statement**: What current limitation does this address?
- **Proposed Solution**: How should the feature work?
- **Use Cases**: Specific scenarios where this would be valuable
- **Implementation Ideas**: Technical approach suggestions (optional)

### Support Questions

- **Context**: What are you trying to accomplish?
- **Current Approach**: What have you tried so far?
- **Specific Problem**: Where exactly are you stuck?
- **Documentation Review**: Have you checked existing documentation?

## Pull Request Process

### Before Submitting

- [ ] All tests pass locally
- [ ] Code follows style guidelines
- [ ] Documentation updated (if applicable)
- [ ] Sample workflows tested (if applicable)
- [ ] No breaking changes (or clearly documented)

### PR Template Requirements

- **Description**: Clear explanation of changes
- **Issue Reference**: Link to related issue (if applicable)
- **Testing**: How have you tested these changes?
- **Breaking Changes**: Any compatibility impacts
- **Documentation**: What documentation needs updating?

### Review Process

1. **Automated Checks**: CI/CD pipeline must pass
2. **Code Review**: At least one maintainer review required
3. **Testing**: Manual testing in Dynamo environment
4. **Documentation**: Verify documentation is complete and accurate
5. **Merge**: Squash and merge with clear commit message

### Post-Merge

- **Version Planning**: Determine if change requires version bump
- **Release Notes**: Update for next release
- **Documentation Deployment**: Ensure docs are current
- **Community Communication**: Announce significant changes

## Community Support

### Getting Help

- **Documentation**: Check docs/ folder for comprehensive guides
- **GitHub Issues**: Use Support Question template for help
- **GitHub Discussions**: For open-ended questions and ideas
- **Sample Workflows**: Review samples/ for working examples

### Helping Others

- **Answer Questions**: Respond to GitHub Issues and Discussions
- **Share Examples**: Contribute sample workflows and use cases
- **Improve Documentation**: Fix typos, add clarity, include examples
- **Test Features**: Help validate new features and bug fixes

### Recognition

Contributors are recognized through:

- **GitHub Contributors Graph**: Automatic recognition for commits
- **Release Notes**: Credit for significant contributions
- **Documentation**: Attribution in relevant docs
- **Community Shoutouts**: Recognition in announcements and discussions

## Development Priorities

### Current Focus Areas

1. **Community Adoption**: Support new users and gather feedback
2. **Performance Optimization**: Monitor and improve response times
3. **Authentication Enhancements**: Additional enterprise auth methods
4. **Error Handling**: More detailed error messages and recovery options

### Future Roadmap

- **Additional HTTP Features**: Advanced request/response handling
- **Integration Examples**: Industry-specific workflow samples
- **Performance Monitoring**: Usage analytics and optimization
- **Educational Content**: Video tutorials and training materials

## Attribution and License

By contributing to DynaFetch, you agree that your contributions will be licensed under the same BSD-3-Clause license that covers the project. You retain copyright to your contributions while granting the project rights to use and distribute them.

### DynaWeb Attribution

DynaFetch is inspired by and builds upon the excellent work of Radu Gidei and the DynaWeb package. We maintain this attribution in our documentation and encourage contributors to respect this foundation.

## Questions?

If you have questions about contributing that aren't covered here:

- **Create an Issue**: Use the Support Question template
- **Start a Discussion**: For open-ended questions
- **Check Documentation**: Review all docs/ files
- **Review Examples**: Look at existing code and samples

Thank you for contributing to DynaFetch and helping make REST API integration better for the entire Dynamo community!

---

_Contributing Guidelines - DynaFetch v1.0.0_  
_Last Updated: August 2025_
