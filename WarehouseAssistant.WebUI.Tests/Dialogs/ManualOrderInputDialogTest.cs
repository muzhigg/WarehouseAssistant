using System;
using AngleSharp.Dom;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

[TestSubject(typeof(ManualOrderInputDialog<ICalculatedTableItem>))]
public sealed class ManualOrderInputDialogTest : MudBlazorTestContext
{
    // private Mock<IDialogService> _dialogService;
    
    public ManualOrderInputDialogTest()
    {
        // _dialogService = new Mock<IDialogService>();
        // Services.AddSingleton(_dialogService.Object);
        Services.AddMudBlazorDialog();
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
    }
    
    // [Fact, Trait("Category", "Unit")]
    // public async Task Should_ThrowException_When_ItemParameterIsMissing()
    // {
    //     // Arrange
    //     var cut           = RenderComponent<MudDialogProvider>();
    //     var dialogService = Services.GetService<IDialogService>() as DialogService;
    //     // Act & Assert
    //     await cut.InvokeAsync((async () =>
    //     {
    //         DialogParameters<ManualOrderInputDialog<CalculatedTableItemStub>> parameters = new()
    //         {
    //             { inputDialog => inputDialog.Text, "Text" }
    //         };
    //         await dialogService!.ShowAsync<ManualOrderInputDialog<CalculatedTableItemStub>>("ManualOrderInputDialog");
    //     }));
    //     
    //     var dialog = cut.FindComponent<ManualOrderInputDialog<CalculatedTableItemStub>>();
    //     
    //     var exception = Assert.Throws<ArgumentNullException>(() => dialog.Render());
    //     exception.ParamName.Should().Be("Item");
    
    // }
    
    [Fact, Trait("Category", "Unit")]
    public void Should_ThrowException_When_TextParameterIsMissing()
    {
        // Arrange
        var mockItem = new Mock<ICalculatedTableItem>();
        
        // Act
        Action act = () => RenderComponent<ManualOrderInputDialog<ICalculatedTableItem>>(parameters => parameters
            .Add(p => p.Item, mockItem.Object)); // Missing Text parameter
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Text*");
    }
    
    [Fact, Trait("Category", "Unit")]
    public void Should_ThrowException_When_ItemParameterIsMissing()
    {
        // Act
        Action act = () => RenderComponent<ManualOrderInputDialog<ICalculatedTableItem>>(parameters => parameters
            .Add(p => p.Text, "Введите количество")); // Missing Item parameter
        
        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Item is not set as parameter (Parameter 'Item')");
    }
    
    [Fact, Trait("Category", "Unit")]
    public void Should_InitializeValueFromItemQuantityToOrder()
    {
        // Arrange
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupGet(x => x.QuantityToOrder).Returns(10);
        
        // Act
        var component = RenderComponent<ManualOrderInputDialog<ICalculatedTableItem>>(parameters => parameters
            .Add(p => p.Item, mockItem.Object)
            .Add(p => p.Text, "Введите количество"));
        
        // Assert
        component.Instance.Value.Should().Be(10, "Value should be initialized with Item.QuantityToOrder");
    }
    
    [Fact]
    public void Should_DisplayText_When_TextParameterIsProvided()
    {
        // Arrange
        var dialogService = Services.GetService<IDialogService>() as DialogService;
        
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupGet(x => x.QuantityToOrder).Returns(5);
        
        var textToDisplay = "Введите количество для заказа";
        var cut           = RenderComponent<MudDialogProvider>();
        DialogParameters<ManualOrderInputDialog<ICalculatedTableItem>> parameters = new()
        {
            { inputDialog => inputDialog.Item, mockItem.Object },
            { inputDialog => inputDialog.Text, textToDisplay }
        };
        
        // Act
        cut.InvokeAsync((async () =>
            await dialogService!.ShowAsync<ManualOrderInputDialog<ICalculatedTableItem>>("ManualOrderInputDialog",
                parameters)));
        
        // Assert
        cut.Find(".manual-order-input-text").Text().Should().Be(textToDisplay);
    }
    
    [Fact]
    public void Should_Render_AdditionalRenderFragment_When_Provided()
    {
        // Arrange
        var cut = RenderComponent<MudDialogProvider>();
        
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupGet(x => x.QuantityToOrder).Returns(10);
        
        // Создаем дополнительный фрагмент для отображения
        RenderFragment additionalFragment = builder => builder.AddContent(0, "Additional content");
        
        // Act
        cut.InvokeAsync((async () => ManualOrderInputDialog<ICalculatedTableItem>.Show(
            Services.GetService<IDialogService>() as DialogService,
            mockItem.Object, "Введите количество", additionalFragment)));
        
        // Assert
        cut.Markup.Should().Contain("Additional content");
    }
    
    [Fact]
    public void Should_DisableSubmitButton_When_ValueIsZeroOrNegative()
    {
        // Arrange
        var cut      = RenderComponent<MudDialogProvider>();
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupProperty(x => x.QuantityToOrder, 0);
        mockItem.SetupGet(x => x.MaxCanBeOrdered).Returns(100);
        
        // Act
        cut.InvokeAsync((async () => ManualOrderInputDialog<ICalculatedTableItem>.Show(
            Services.GetService<IDialogService>() as DialogService,
            mockItem.Object, "Введите количество")));
        
        // Assert initial state
        // cut.Render();
        var submitButton = cut.Find(".manual-input-submit");
        submitButton.HasAttribute("disabled").Should()
            .BeTrue("Submit button should be disabled when QuantityToOrder is 0");
        
        // Act - Simulate entering a negative value
        cut.Find(".manual-input-value-field input").Change(-5);
        cut.Find(".manual-input-value-field input").Blur();
        
        // Assert
        submitButton.HasAttribute("disabled").Should()
            .BeTrue("Submit button should be disabled when QuantityToOrder is negative");
        mockItem.Object.QuantityToOrder.Should().Be(0);
        
        // Act - Simulate entering a positive value
        cut.Find(".manual-input-value-field input").Change(10);
        cut.Find(".manual-input-value-field input").Blur();
        
        // Assert
        mockItem.Object.QuantityToOrder.Should().Be(10, "QuantityToOrder should reflect the positive input value");
        submitButton.HasAttribute("disabled").Should()
            .BeFalse("Submit button should be enabled when QuantityToOrder is positive");
    }
    
    [Fact]
    public void Should_NotAllowNegativeValues()
    {
        // Arrange
        var cut      = RenderComponent<MudDialogProvider>();
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupProperty(x => x.QuantityToOrder, 0);
        
        // Act
        cut.InvokeAsync((async () => ManualOrderInputDialog<ICalculatedTableItem>.Show(
            Services.GetService<IDialogService>() as DialogService,
            mockItem.Object, "Введите количество")));
        
        // Act - Simulate entering a negative value
        var numericField = cut.Find(".manual-input-value-field input");
        numericField.Change(-1);
        
        // Assert
        mockItem.Object.QuantityToOrder.Should().Be(0);
    }
    
    [Fact]
    public void ManualInputSubmit_ShouldCloseDialogWithOkResult()
    {
        // Arrange
        var cut = RenderComponent<MudDialogProvider>();
        
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupProperty(x => x.QuantityToOrder, 5);
        
        // var mockDialogInstance = new Mock<MudDialogInstance>();
        IDialogReference dialogReference = null;
        
        // Act
        cut.InvokeAsync((async () => dialogReference = ManualOrderInputDialog<ICalculatedTableItem>.Show(
            Services.GetService<IDialogService>() as DialogService,
            mockItem.Object, "Введите количество")));
        
        IRenderedComponent<ManualOrderInputDialog<ICalculatedTableItem>> component =
            cut.FindComponent<ManualOrderInputDialog<ICalculatedTableItem>>();
        component.Instance.Value = 10; // Simulate user changing the value
        
        // Act
        component.Find(".manual-input-submit").Click();
        
        // Assert
        mockItem.Object.QuantityToOrder.Should().Be(10, "QuantityToOrder should be updated with the new value");
        dialogReference.Result.Result.Canceled.Should().BeFalse();
        dialogReference.Result.Result.Data.Should().Be(10);
    }
    
    [Fact]
    public void Should_ResetValueOnCancel()
    {
        // Arrange
        var cut = RenderComponent<MudDialogProvider>();
        
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupProperty(x => x.QuantityToOrder, 10);
        
        // Act
        cut.InvokeAsync((async () => ManualOrderInputDialog<ICalculatedTableItem>.Show(
            Services.GetService<IDialogService>() as DialogService,
            mockItem.Object, "Введите количество")));
        
        IRenderedComponent<ManualOrderInputDialog<ICalculatedTableItem>> component =
            cut.FindComponent<ManualOrderInputDialog<ICalculatedTableItem>>();
        
        // Simulate user changing the value
        component.Instance.Value = 15;
        
        // Act: User clicks "Пропустить"
        component.Find(".manual-input-cancel").Click();
        
        // Assert
        mockItem.Object.QuantityToOrder.Should().Be(10,
            because: "the value should be reset to the initial value after canceling the dialog");
    }
    
    [Fact]
    public void Should_ClampValueToMaxCanBeOrdered()
    {
        // Arrange
        var cut = RenderComponent<MudDialogProvider>();
        
        var mockItem = new Mock<ICalculatedTableItem>();
        mockItem.SetupProperty(x => x.QuantityToOrder, 0);
        mockItem.SetupGet(x => x.MaxCanBeOrdered).Returns(5);
        
        cut.InvokeAsync((async () => ManualOrderInputDialog<ICalculatedTableItem>.Show(
            Services.GetService<IDialogService>() as DialogService,
            mockItem.Object, "Введите количество")));
        
        IRenderedComponent<ManualOrderInputDialog<ICalculatedTableItem>> component =
            cut.FindComponent<ManualOrderInputDialog<ICalculatedTableItem>>();
        
        // Act - Simulate entering a value greater than MaxCanBeOrdered
        component.Find(".manual-input-value-field input").Change(10);
        component.Find(".manual-input-value-field input").Blur();
        
        // Assert
        component.Instance.Value.Should().Be(5, "Value should be clamped to MaxCanBeOrdered");
    }
}