using FluentAssertions;
using JetBrains.Annotations;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Models;

namespace WarehouseAssistant.WebUI.Tests.Components;

[TestSubject(typeof(ReceivingInputForm))]
public class ReceivingInputFormTest : MudBlazorTestContext
{
    public ReceivingInputFormTest()
    {
        Services.AddMudServices();
    }
    
    [Fact]
    public void Should_Add_4000_To_Id_When_Length_Is_4()
    {
        // Arrange
        ReceivingInputData? receivingInputData = null;
        var component = RenderComponent<ReceivingInputForm>(builder => builder.Add(form =>
            form.OnInputSubmit, data => receivingInputData = data));
        var inputElement = component.Find("#receiving-input-form-id");
        
        // Act
        inputElement.Change("1234");
        component.Find("#receiving-input-form-submit-button").Click();
        
        // Assert
        receivingInputData.Should().NotBeNull();
        Assert.Equal("40001234", receivingInputData.Id);
    }
    
    [Fact]
    public void Should_Reset_Fields_After_Submit()
    {
        // Arrange
        var component       = RenderComponent<ReceivingInputForm>();
        var inputElement    = component.Find("input#receiving-input-form-id");
        var quantityElement = component.Find("#receiving-input-form-quantity");
        
        // Act
        inputElement.Change("5678");
        component.FindComponent<MudTextField<string>>().Instance.Value.Should().Be("5678");
        quantityElement.Change(5);
        component.Find("#receiving-input-form-submit-button").Click();
        
        // Assert
        Assert.Null(component.FindComponent<MudTextField<string>>().Instance.Value);
        Assert.Equal(1, component.FindComponent<MudNumericField<int>>().Instance.Value);
    }
    
    [Fact]
    public void Should_Invoke_OnInputSubmit_With_Expected_Values()
    {
        // Arrange
        var inputData = new ReceivingInputData();
        var component = RenderComponent<ReceivingInputForm>(
            parameters => parameters.Add(p => p.OnInputSubmit, data => inputData = data)
        );
        var inputElement    = component.Find("input#receiving-input-form-id");
        var quantityElement = component.Find("#receiving-input-form-quantity");
        
        // Act
        inputElement.Change("1234");
        quantityElement.Change(10);
        component.Find("#receiving-input-form-submit-button").Click();
        
        // Assert
        Assert.Equal("40001234", inputData.Id);
        Assert.Equal(10, inputData.Quantity);
    }
}