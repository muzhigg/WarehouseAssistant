using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using WarehouseAssistant.WebUI.Auth;

namespace WarehouseAssistant.WebUI.Tests.Services;

[TestSubject(typeof(CustomAuthenticationStateProvider))]
public class CustomAuthenticationStateProviderTest : MudBlazorTestContext
{
    private readonly Mock<ILocalStorageService>                       _localStorageMock;
    private readonly Mock<ILogger<CustomAuthenticationStateProvider>> _loggerMock;
    private readonly Mock<IGotrueClient<User, Session>>               _gotrueClientMock;
    
    private CustomAuthenticationStateProvider _authStateProvider;
    
    public CustomAuthenticationStateProviderTest()
    {
        _localStorageMock = new Mock<ILocalStorageService>();
        _loggerMock       = new Mock<ILogger<CustomAuthenticationStateProvider>>();
        _gotrueClientMock = new Mock<IGotrueClient<User, Session>>();
    }
    
    [Fact]
    public void AuthEventHandler_Should_CallLocalStorage_When_AuthStateIsTokenRefreshed()
    {
        // Arrange
        _gotrueClientMock.SetupGet(client => client.CurrentSession).Returns(new Session());
        IGotrueClient<User, Session>.AuthEventHandler authEventHandler = null;
        _gotrueClientMock.Setup(client =>
                client.AddStateChangedListener(
                    It.IsAny<IGotrueClient<User, Session>.AuthEventHandler>()))
            .Callback((IGotrueClient<User, Session>.AuthEventHandler x) => authEventHandler = x);
        
        _authStateProvider = new CustomAuthenticationStateProvider(_localStorageMock.Object,
            _loggerMock.Object, _gotrueClientMock.Object);
        
        // Act
        Debug.Assert(authEventHandler != null, nameof(authEventHandler) + " != null");
        authEventHandler(_gotrueClientMock.Object, Constants.AuthState.TokenRefreshed);
        
        // Assert
        VerifyLogInfo(_loggerMock, s => s.Contains(Constants.AuthState.TokenRefreshed.ToString()),
            Times.Once());
        VerifyLogInfo(_loggerMock, s => s.Contains("Token refreshed"), Times.Once());
        _localStorageMock.Verify(service =>
                service.SetItemAsync(
                    It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<CancellationToken>()),
            Times.Once());
    }
    
    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAnonymous_WhenSessionIsNull()
    {
        // Arrange
        _localStorageMock.Setup(ls => ls.GetItemAsync<Session>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session)null!);
        
        _authStateProvider = new(_localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        
        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        authState.User.Identity.IsAuthenticated.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAuthenticated_HappyPath()
    {
        // Arrange
        Session oldSession = new Session()
        {
            AccessToken =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            RefreshToken = "refresh_token",
            CreatedAt    = DateTime.Now,
        };
        Session newSession = new()
        {
            AccessToken =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            RefreshToken = "new_refresh_token",
            CreatedAt    = DateTime.Now.AddSeconds(10),
        };
        
        _localStorageMock.Setup(ls => ls.GetItemAsync<Session>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldSession);
        _gotrueClientMock.Setup(
                client =>
                    client.SetSession(
                        oldSession.AccessToken, oldSession.RefreshToken, It.IsAny<bool>()))
            .ReturnsAsync(newSession);
        
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        
        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        authState.User.Identity.IsAuthenticated.Should().BeTrue();
        _localStorageMock.Verify(ls => ls.GetItemAsync<Session>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
        _localStorageMock.Verify(service => service.SetItemAsync(It.IsAny<string>(),
                newSession,
                It.IsAny<CancellationToken>()),
            Times.Once());
    }
    
    [Fact]
    public async Task GetAuthenticationStateAsync_Should_ReturnAnonymous_WhenExceptionIsThrown()
    {
        // Arrange
        Session oldSession = new Session()
        {
            AccessToken =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            RefreshToken = "refresh_token",
            CreatedAt    = DateTime.Now,
        };
        _localStorageMock.Setup(ls => ls.GetItemAsync<Session>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldSession);
        
        _gotrueClientMock.Setup(client =>
                client.SetSession(
                    oldSession.AccessToken, oldSession.RefreshToken, It.IsAny<bool>()))
            .Throws<GotrueException>(() => new GotrueException("Invalid token"));
        
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        
        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        
        // Assert
        authState.User.Identity.IsAuthenticated.Should().BeFalse();
        _localStorageMock.Verify(ls => ls.GetItemAsync<Session>(It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
        _localStorageMock.Verify(service => service.RemoveItemAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never());
    }
    
    [Fact]
    public async Task SignIn_Should_ReturnFalse_When_SessionIsNull()
    {
        // Arrange
        _gotrueClientMock.Setup(
                client =>
                    client.SignIn(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Session)null!);
        
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        
        // Act
        var result = await _authStateProvider.SignIn("", "");
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task SignIn_Should_ReturnTrue_When_SignInIsSuccessful()
    {
        // Arrange
        _gotrueClientMock.Setup(
                client =>
                    client.SignIn(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Session()
            {
                AccessToken =
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            });
        
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        int                 authStateChangedCount = 0;
        AuthenticationState authenticationState   = null!;
        _authStateProvider.AuthenticationStateChanged += task =>
        {
            authStateChangedCount++;
            authenticationState = task.Result;
        };
        
        // Act
        var result = await _authStateProvider.SignIn("", "");
        
        // Assert
        result.Should().BeTrue();
        authStateChangedCount.Should().Be(1);
        authenticationState.Should().NotBeNull();
        authenticationState.User.Identity.IsAuthenticated.Should().BeTrue();
        _localStorageMock.Verify(service => service.SetItemAsync(It.IsAny<string>(),
            It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once());
    }
    
    [Fact]
    public async Task SignIn_Should_ReturnFalse_When_ExceptionIsThrown()
    {
        // Arrange
        _gotrueClientMock.Setup(
                client =>
                    client.SignIn(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Session());
        
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        
        // Act
        var result = await _authStateProvider.SignIn("", "");
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task SignOut_Should_SetAnon()
    {
        // Arrange
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        AuthenticationState authenticationState = null!;
        _authStateProvider.AuthenticationStateChanged += task => { authenticationState = task.Result; };
        
        // Act
        await _authStateProvider.SignOut();
        
        // Assert
        _localStorageMock.Verify(service => service.RemoveItemAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
        authenticationState.User.Identity.IsAuthenticated.Should().BeFalse();
    }
    
    [Fact]
    public async Task SignOut_Should_SetAnon_When_ExceptionIsThrown()
    {
        // Arrange
        _authStateProvider = new(
            _localStorageMock.Object, _loggerMock.Object, _gotrueClientMock.Object);
        AuthenticationState authenticationState = null!;
        _authStateProvider.AuthenticationStateChanged += task => { authenticationState = task.Result; };
        _gotrueClientMock.Setup(client =>
                client.SignOut(It.IsAny<Constants.SignOutScope>()))
            .Throws<Exception>();
        
        // Act
        await _authStateProvider.SignOut();
        
        // Assert
        _localStorageMock.Verify(service => service.RemoveItemAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
        authenticationState.User.Identity.IsAuthenticated.Should().BeFalse();
    }
}