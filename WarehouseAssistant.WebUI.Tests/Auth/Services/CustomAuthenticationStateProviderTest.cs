using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using Moq.Contrib.HttpClient;
using Moq.Language.Flow;
using WarehouseAssistant.WebUI.Auth;

namespace WarehouseAssistant.WebUI.Tests.Services;

[TestSubject(typeof(CustomAuthenticationStateProvider))]
public class CustomAuthenticationStateProviderTest
{
    private readonly Mock<ILocalStorageService> _localStorageMock;
    
    private const string ValidToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJUZXN0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVmlld2VyIiwianRpIjoiNzRiNzJhYzktNWFlOC00YTI2LTkzOGYtNGE1MjA2YmY5ZWYxIiwiZXhwIjoxNzMyNjEwNzIyLCJpc3MiOiJodHRwczovL3dhcmVob3VzZWFzc2lzdGFudGRiYXBpLm9ucmVuZGVyLmNvbSIsImF1ZCI6Imh0dHBzOi8vd2FyZWhvdXNlYXNzaXN0YW50ZGJhcGkub25yZW5kZXIuY29tIn0.sht6-n9XuXjFudjI2IrmNcaict1M0lL1NgiKFl9pEro";
    
    private const string ExpiredToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJUZXN0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVmlld2VyIiwianRpIjoiNzRiNzJhYzktNWFlOC00YTI2LTkzOGYtNGE1MjA2YmY5ZWYxIiwiZXhwIjoxNjA5MTU4MDAwLCJpc3MiOiJodHRwczovL3dhcmVob3VzZWFzc2lzdGFudGRiYXBpLm9ucmVuZGVyLmNvbSIsImF1ZCI6Imh0dHBzOi8vd2FyZWhvdXNlYXNzaXN0YW50ZGJhcGkub25yZW5kZXIuY29tIn0.sht6-n9XuXjFudjI2IrmNcaict1M0lL1NgiKFl9pEro";
    
    private readonly HttpClient                        _httpClientMock;
    private readonly Mock<HttpMessageHandler>          _httpMessageHandlerMock;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    
    public CustomAuthenticationStateProviderTest()
    {
        _localStorageMock       = new Mock<ILocalStorageService>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientMock         = _httpMessageHandlerMock.CreateClient();
        _authStateProvider      = new CustomAuthenticationStateProvider(_localStorageMock.Object, _httpClientMock);
    }
    
    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAnonymous_WhenTokenIsNull()
    {
        // Arrange
        _localStorageMock.Setup(ls => ls.GetItemAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);
        
        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        authState.User.Identity.IsAuthenticated.Should().BeFalse();
        _httpClientMock.DefaultRequestHeaders.Authorization.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAnonymous_WhenTokenIsExpired()
    {
        // Arrange
        _localStorageMock.Setup(ls => ls.GetItemAsync<string>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExpiredToken);
        
        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        authState.User.Identity.IsAuthenticated.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAuthenticated_WhenTokenIsValid()
    {
        // Arrange
        _localStorageMock.Setup(ls => ls.GetItemAsync<string>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidToken);
        
        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        authState.User.Identity.IsAuthenticated.Should().BeTrue();
        _httpClientMock.DefaultRequestHeaders.Authorization.Should().NotBeNull();
    }
    
    [Fact]
    public async Task MarkUserAsAuthenticated_ShouldSaveToken_WhenTokenIsValid()
    {
        // Arrange
        _localStorageMock.Setup(ls => ls.GetItemAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidToken);
        
        // Act
        _authStateProvider.MarkUserAsAuthenticated(ValidToken);
        
        // Assert
        AuthenticationState authState = await _authStateProvider.GetAuthenticationStateAsync();
        authState.User.Identity.IsAuthenticated.Should().BeTrue();
        _httpClientMock.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        _localStorageMock.Verify(ls => ls.SetItemAsync(It.IsAny<string>(), ValidToken, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task MarkUserAsLoggedOut_ShouldRemoveToken()
    {
        // Arrange
        ISetup<ILocalStorageService, ValueTask<string?>> setup = _localStorageMock.Setup(ls =>
            ls.GetItemAsync<string>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()));
        setup.ReturnsAsync(ValidToken);
        await _authStateProvider.GetAuthenticationStateAsync();
        
        // Act
        setup.ReturnsAsync(string.Empty);
        
        // Assert
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        _localStorageMock.Verify(ls => ls.RemoveItemAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        authState.User.Identity.IsAuthenticated.Should().BeFalse();
        _httpClientMock.DefaultRequestHeaders.Authorization.Should().BeNull();
    }
}