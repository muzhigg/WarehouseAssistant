using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using JetBrains.Annotations;
using Moq;
using WarehouseAssistant.WebUI.Auth;

namespace WarehouseAssistant.WebUI.Tests.Services;

[TestSubject(typeof(CustomAuthenticationStateProvider))]
public class CustomAuthenticationStateProviderTest
{
    private readonly Mock<ILocalStorageService> _localStorageServiceMock;
    
    public CustomAuthenticationStateProviderTest()
    {
        _localStorageServiceMock = new Mock<ILocalStorageService>();
    }
    
    [Fact]
    public async Task Should_Consider_User_NotAuthenticated_When_No_Token_In_LocalStorage()
    {
        // Arrange
        _localStorageServiceMock.Setup(ls => ls.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null);
        var httpClient        = new HttpClient();
        var authStateProvider = new CustomAuthenticationStateProvider(_localStorageServiceMock.Object, httpClient);
        
        // Act
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        Assert.False(authState.User.Identity.IsAuthenticated);
    }
    
    [Fact]
    public async Task Should_Consider_User_Authenticated_When_Token_In_LocalStorage_And_HttpClient_Has_Token_Header()
    {
        // Arrange
        var token =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJUZXN0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVmlld2VyIiwianRpIjoiNzRiNzJhYzktNWFlOC00YTI2LTkzOGYtNGE1MjA2YmY5ZWYxIiwiZXhwIjoxNzMyNjEwNzIyLCJpc3MiOiJodHRwczovL3dhcmVob3VzZWFzc2lzdGFudGRiYXBpLm9ucmVuZGVyLmNvbSIsImF1ZCI6Imh0dHBzOi8vd2FyZWhvdXNlYXNzaXN0YW50ZGJhcGkub25yZW5kZXIuY29tIn0.sht6-n9XuXjFudjI2IrmNcaict1M0lL1NgiKFl9pEro";
        _localStorageServiceMock.Setup(ls => ls.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        var httpClient        = new HttpClient();
        var authStateProvider = new CustomAuthenticationStateProvider(_localStorageServiceMock.Object, httpClient);
        
        // Act
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        Assert.True(authState.User.Identity.IsAuthenticated);
        Assert.Equal(new AuthenticationHeaderValue("Bearer", token), httpClient.DefaultRequestHeaders.Authorization);
    }
    
    [Fact]
    public async Task Should_Consider_User_NotAuthenticated_When_Token_Expired()
    {
        // Arrange
        var expiredToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJUZXN0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVmlld2VyIiwianRpIjoiNzRiNzJhYzktNWFlOC00YTI2LTkzOGYtNGE1MjA2YmY5ZWYxIiwiZXhwIjoxNjA5MTU4MDAwLCJpc3MiOiJodHRwczovL3dhcmVob3VzZWFzc2lzdGFudGRiYXBpLm9ucmVuZGVyLmNvbSIsImF1ZCI6Imh0dHBzOi8vd2FyZWhvdXNlYXNzaXN0YW50ZGJhcGkub25yZW5kZXIuY29tIn0.sht6-n9XuXjFudjI2IrmNcaict1M0lL1NgiKFl9pEro";
        _localStorageServiceMock.Setup(ls => ls.GetItemAsync<string>("authToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);
        var httpClient        = new HttpClient();
        var authStateProvider = new CustomAuthenticationStateProvider(_localStorageServiceMock.Object, httpClient);
        
        // Act
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        Assert.False(authState.User.Identity.IsAuthenticated);
    }
    
    [Fact]
    public async Task Should_Remove_Token_From_LocalStorage_When_User_LoggedOut()
    {
        // Arrange
        _localStorageServiceMock.Setup(ls => ls.RemoveItemAsync("authToken", It.IsAny<CancellationToken>()))
            .Verifiable();
        var httpClient        = new HttpClient();
        var authStateProvider = new CustomAuthenticationStateProvider(_localStorageServiceMock.Object, httpClient);
        
        // Act
        authStateProvider.MarkUserAsLoggedOut();
        
        // Assert
        _localStorageServiceMock.Verify(ls => ls.RemoveItemAsync("authToken", It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Null(httpClient.DefaultRequestHeaders.Authorization);
    }
}