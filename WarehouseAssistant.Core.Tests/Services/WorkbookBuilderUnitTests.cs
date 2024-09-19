using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests.Services;

[Trait("Category", "Unit")]
public sealed class WorkbookBuilderUnitTests
{
    private sealed class TableItemStub
    {
        public string? Column1 { get; set; }
    }
    
    // Test the happy path for creating a sheet and adding data
    [Fact]
    public void CreateSheet_AddData_Success()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<string>();
        
        // Act
        var result = workbookBuilder.CreateSheet("TestSheet");
        workbookBuilder.AddToSheet("TestSheet", "TestData");
        
        // Assert
        Assert.True(result);
        List<string>? sheetData = workbookBuilder.GetSheetData("TestSheet");
        Assert.NotNull(sheetData);
        Assert.Single(sheetData);
    }
    
    // Test adding a range of data to a sheet
    [Fact]
    public void AddRangeToSheet_AddMultipleData_Success()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<int>();
        workbookBuilder.CreateSheet("TestSheet");
        var data = new List<int> { 1, 2, 3 };
        
        // Act
        workbookBuilder.AddRangeToSheet("TestSheet", data);
        
        // Assert
        List<int>? sheetData = workbookBuilder.GetSheetData("TestSheet");
        Assert.NotNull(sheetData);
        Assert.Equal(3, sheetData.Count);
    }
    
    // Test the edge case where creating a sheet with an existing name should fail
    [Fact]
    public void CreateSheet_ExistingSheetName_ReturnsFalse()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<string>();
        workbookBuilder.CreateSheet("TestSheet");
        
        // Act
        var result = workbookBuilder.CreateSheet("TestSheet");
        
        // Assert
        Assert.False(result);
    }
    
    // Test the edge case where adding to a non-existing sheet should throw an exception
    [Fact]
    public void AddToSheet_NonExistingSheet_ThrowsException()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<string>();
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => workbookBuilder.AddToSheet("NonExistingSheet", "TestData"));
    }
    
    // Test the edge case where adding a range to a non-existing sheet should throw an exception
    [Fact]
    public void AddRangeToSheet_NonExistingSheet_ThrowsException()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<int>();
        var data            = new List<int> { 1, 2, 3 };
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => workbookBuilder.AddRangeToSheet("NonExistingSheet", data));
    }
    
    // Test the happy path for generating a byte array from the workbook
    [Fact]
    public void AsByteArray_GeneratesByteArray_Success()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<TableItemStub>();
        workbookBuilder.CreateSheet("TestSheet");
        workbookBuilder.AddToSheet("TestSheet", new TableItemStub { Column1 = "TestData" });
        
        // Act
        var result = workbookBuilder.AsByteArray();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
    
    [Fact]
    public void AsByteArray_WithDynamicType()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<dynamic>();
        workbookBuilder.CreateSheet("TestSheet");
        workbookBuilder.AddToSheet("TestSheet", new { Column1 = "TestData" });
        
        // Act
        var result = workbookBuilder.AsByteArray();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
    
    // Test disposing of the workbook builder
    [Fact]
    public void Dispose_DisposesWorkbookStream_Success()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<TableItemStub>();
        
        // Act
        workbookBuilder.Dispose();
        // Assert
        Assert.Throws<ObjectDisposedException>(() => workbookBuilder.AsByteArray());
    }
    
    // Test asynchronously disposing of the workbook builder
    [Fact]
    public async Task DisposeAsync_DisposesWorkbookStream_Success()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<string>();
        
        // Act
        await workbookBuilder.DisposeAsync();
        
        // Assert
        Assert.Throws<ObjectDisposedException>(() => workbookBuilder.AsByteArray());
    }
}