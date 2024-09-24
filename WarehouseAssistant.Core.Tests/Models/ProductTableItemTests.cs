using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Tests.Models;

[Trait("Category", "Unit")]
public sealed class ProductTableItemTests
{
    [Fact]
    public void HasValidName_ValidName_ReturnsTrue()
    {
        // Arrange
        var item = new ProductTableItem { Name = "Valid Product Name" };
        
        // Act
        var result = item.HasValidName();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void HasValidName_NullName_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Name = null };
        
        // Act
        var result = item.HasValidName();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void HasValidName_EmptyName_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Name = "" };
        
        // Act
        var result = item.HasValidName();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void HasValidName_NameContainsАкция_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Name = "Product Акция Name" };
        
        // Act
        var result = item.HasValidName();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void HasValidArticle_ValidArticle_ReturnsTrue()
    {
        // Arrange
        var item = new ProductTableItem { Article = "12345678" };
        
        // Act
        var result = item.HasValidArticle();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void HasValidArticle_NullArticle_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Article = null };
        
        // Act
        var result = item.HasValidArticle();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void HasValidArticle_EmptyArticle_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Article = "" };
        
        // Act
        var result = item.HasValidArticle();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void HasValidArticle_ArticleLengthNot8_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Article = "1234567" };
        
        // Act
        var result = item.HasValidArticle();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void HasValidArticle_ArticleNotAllDigits_ReturnsFalse()
    {
        // Arrange
        var item = new ProductTableItem { Article = "1234567A" };
        
        // Act
        var result = item.HasValidArticle();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void AvailableQuantity_SetNegative_ClampsToZero()
    {
        // Arrange
        var item = new ProductTableItem { AvailableQuantity = -1 };
        
        // Act
        var result = item.AvailableQuantity;
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public void AvailableQuantity_SetMaxValue_ClampsToMaxValue()
    {
        // Arrange
        var item = new ProductTableItem { AvailableQuantity = int.MaxValue };
        
        // Act
        var result = item.AvailableQuantity;
        
        // Assert
        Assert.Equal(int.MaxValue, result);
    }
    
    [Fact]
    public void CurrentQuantity_SetNegative_ClampsToZero()
    {
        // Arrange
        var item = new ProductTableItem { CurrentQuantity = -1 };
        
        // Act
        var result = item.CurrentQuantity;
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public void CurrentQuantity_SetMaxValue_ClampsToMaxValue()
    {
        // Arrange
        var item = new ProductTableItem { CurrentQuantity = int.MaxValue };
        
        // Act
        var result = item.CurrentQuantity;
        
        // Assert
        Assert.Equal(int.MaxValue, result);
    }
    
    [Fact]
    public void AverageTurnover_SetNegative_ClampsToZero()
    {
        // Arrange
        var item = new ProductTableItem { AverageTurnover = -1.0 };
        
        // Act
        var result = item.AverageTurnover;
        
        // Assert
        Assert.Equal(0.0, result);
    }
    
    [Fact]
    public void AverageTurnover_SetMaxValue_ClampsToMaxValue()
    {
        // Arrange
        var item = new ProductTableItem { AverageTurnover = double.MaxValue };
        
        // Act
        var result = item.AverageTurnover;
        
        // Assert
        Assert.Equal(double.MaxValue, result);
    }
    
    [Fact]
    public void StockDays_SetNegative_ClampsToZero()
    {
        // Arrange
        var item = new ProductTableItem { StockDays = -1.0 };
        
        // Act
        var result = item.StockDays;
        
        // Assert
        Assert.Equal(0.0, result);
    }
    
    [Fact]
    public void StockDays_SetMaxValue_ClampsToMaxValue()
    {
        // Arrange
        var item = new ProductTableItem { StockDays = double.MaxValue };
        
        // Act
        var result = item.StockDays;
        
        // Assert
        Assert.Equal(double.MaxValue, result);
    }
    
    [Fact]
    public void QuantityToOrder_SetGreaterThanMaxCanBeOrdered_ClampsToMaxCanBeOrdered()
    {
        // Arrange
        var item = new ProductTableItem { AvailableQuantity = 100, QuantityToOrder = 10 };
        
        // Act
        var result = item.QuantityToOrder;
        
        // Assert
        Assert.Equal(7, result); // 100 * 0.07 = 7
    }
    
    [Fact]
    public void QuantityToOrder_SetLessThanMaxCanBeOrdered_SetsToValue()
    {
        // Arrange
        var item = new ProductTableItem { AvailableQuantity = 100, QuantityToOrder = 5 };
        
        // Act
        var result = item.QuantityToOrder;
        
        // Assert
        Assert.Equal(5, result);
    }
}