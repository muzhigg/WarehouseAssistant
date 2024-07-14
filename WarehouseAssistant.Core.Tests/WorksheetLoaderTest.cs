using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests
{
    public class WorksheetLoaderTest
    {
        [Fact]
        public void GetColumns_ShouldReturnCorrectCollection()
        {
            // Arrange
            using WorksheetLoader worksheetLoader = new(@"D:\Downloads\���� 29.03.xlsx");

            // Act
            Dictionary<string, string?> columns = worksheetLoader.GetColumns();

            // Assert
            Assert.NotNull(columns);
            Assert.Equal(10, columns.Count);

            Assert.True(columns.ContainsKey("A"));
            Assert.Equal("������������", columns["A"]);

            Assert.True(columns.ContainsKey("J"));
            Assert.Equal("����� �� ���� ���", columns["J"]);
        }
    }
}