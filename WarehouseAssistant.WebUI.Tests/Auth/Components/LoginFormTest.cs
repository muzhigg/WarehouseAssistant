using System.Threading.Tasks;
using FluentAssertions.BUnit;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using WarehouseAssistant.WebUI.Auth;

namespace WarehouseAssistant.WebUI.Tests.Auth.Components;

[TestSubject(typeof(LoginForm))]
public class LoginFormTest : MudBlazorTestContext
{
    private readonly Mock<IAuthService>       _authServiceMock;
    private readonly Mock<ILogger<LoginForm>> _loggerMock;
    
    public LoginFormTest()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock      = new Mock<ILogger<LoginForm>>();
        
        Services.AddSingleton(_authServiceMock.Object)
            .AddSingleton(_loggerMock.Object);
    }
    
    [Fact]
    public void HandleLogin_Should_HappyPath()
    {
        // Arrange
        _authServiceMock.Setup(service => service.SignIn(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult<bool>(true));
        
        var cut      = RenderComponent<LoginForm>();
        var email    = "test@test.com";
        var password = "password";
        
        // Act
        cut.Find("input[type=email]").Change(email);
        cut.Find("input[type=password]").Change(password);
        cut.Find("button[type=submit]").Click();
        
        // Assert
        VerifyLogInfo(_loggerMock, s => s.Contains(email), Times.Once());
        VerifyLogError(_loggerMock, Times.Never());
        _authServiceMock.Verify(service =>
            service.SignIn(email, password), Times.Once);
    }
    
    [Fact]
    public void HandleLogin_Should_ShowError_When_LoginFails()
    {
        // Arrange
        _authServiceMock.Setup(service => service.SignIn(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult<bool>(false));
        
        var cut      = RenderComponent<LoginForm>();
        var email    = "test@test.com";
        var password = "password";
        
        // Act
        cut.Find("input[type=email]").Change(email);
        cut.Find("input[type=password]").Change(password);
        cut.Find("button[type=submit]").Click();
        
        // Assert
        VerifyLogInfo(_loggerMock, s => s.Contains(email), Times.Once());
        VerifyLogError(_loggerMock, s => s.Contains(email), Times.Once());
        _authServiceMock.Verify(service =>
            service.SignIn(email, password), Times.Once);
        cut.Render();
        cut.Find("#auth-error-message").Should().NotBeNull()
            .And.HaveChildMarkup("Неправильный логин или пароль");
    }
}