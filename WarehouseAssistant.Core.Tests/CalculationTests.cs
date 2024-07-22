using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Collections;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests
{
    public class CalculationTests
    {
        [Fact]
        public void ProductCalculator_StockDays_ShouldReturnCorrectNumbers()
        {
            ProductTableItem product1 = new ProductTableItem()
            {
                Name              = "Test",
                Article = 4,
                AvailableQuantity = 1770,
                CurrentQuantity   = 10,
                AverageTurnover   = 0.11,
                StockDays         = 90.91
            };
            ProductTableItem product2 = new ProductTableItem()
            {
                Name              = "Test",
                Article           = 4,
                AvailableQuantity = 764,
                CurrentQuantity   = 60,
                AverageTurnover   = 0.0,
                StockDays         = 0.0
            };
            ProductTableItem product3 = new ProductTableItem()
            {
                Name              = "Test",
                Article           = 4,
                AvailableQuantity = 1256,
                CurrentQuantity   = 8,
                AverageTurnover   = 0.28,
                StockDays         = 28.57
            };

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60
            };

            ForNumberDaysCalculation          method = new ForNumberDaysCalculation();
            OrderCalculator<ProductTableItem> calc   = new OrderCalculator<ProductTableItem>(method, options);

            Assert.Equal(6, calc.CalculateOrderQuantity(product1));
            Assert.Equal(0, calc.CalculateOrderQuantity(product2));
            Assert.Equal(16, calc.CalculateOrderQuantity(product3));

            options.ConsiderCurrentQuantity = true;
            Assert.Equal(8, calc.CalculateOrderQuantity(product3));
        }

        [Fact]
        public void ProductCalculator_BGLCNull_ShouldReturnCorrectNumber()
        {
            ProductTableItem product1 = new ProductTableItem()
            {
                Name              = "Test",
                Article           = 4,
                AvailableQuantity = 0,
                CurrentQuantity   = 0,
                AverageTurnover   = 0.4,
                StockDays         = 0
            };

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60
            };
            ForNumberDaysCalculation          method = new ForNumberDaysCalculation();
            OrderCalculator<ProductTableItem> calc   = new OrderCalculator<ProductTableItem>(method, options);

            Assert.Equal(0, calc.CalculateOrderQuantity(product1));
        }

        private List<ProductTableItem> GetTableItems()
        {
            using WorksheetLoader worksheetLoader = new(@"D:\Downloads\товары 17.07.xlsx");
            ColumnMapping         columnMapping   = new ColumnMapping();
            columnMapping.AddMapping(ColumnMapping.NameKey, "A");
            columnMapping.AddMapping(ColumnMapping.ArticleKey, "B");
            columnMapping.AddMapping(ColumnMapping.AvailableQuantityKey, "C");
            columnMapping.AddMapping(ColumnMapping.CurrentQuantityKey, "D");
            columnMapping.AddMapping(ColumnMapping.AverageTurnoverKey, "E");
            columnMapping.AddMapping(ColumnMapping.StockDaysKey, "F");
            columnMapping.AddMapping(ColumnMapping.OrderCalculationKey, "G");
            return worksheetLoader.ParseItems(columnMapping);
        }

        [Fact]
        public void ForNumberDaysCalculation_First30Raw()
        {
            List<ProductTableItem> list = GetTableItems();

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60
            };

            ForNumberDaysCalculation          method = new ForNumberDaysCalculation();
            OrderCalculator<ProductTableItem> calc   = new OrderCalculator<ProductTableItem>(method, options);

            int[] expectedValues = new[]
            {
                4,
                0,
                1,
                34,
                28,
                0,
                0,
                0,
                3,
                0,
                28,
                0,
                0,
                0,
                0,
                0,
                28,
                0,
                4,
                0,
                14,
                0,
                0,
                0,
                1,
                0,
                12,
                5,
                20,
                25
            };

            for (var i = 0; i < 30; i++) Assert.Equal(expectedValues[i], calc.CalculateOrderQuantity(list[i]));
        }

        [Fact]
        public void ForNumberDaysCalculation_FirstAvailable30()
        {
            List<ProductTableItem> list = GetTableItems().Where(item => item.AvailableQuantity > 0).ToList();

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60
            };

            ForNumberDaysCalculation          method = new ForNumberDaysCalculation();
            OrderCalculator<ProductTableItem> calc   = new OrderCalculator<ProductTableItem>(method, options);

            int[] expectedValues = new[]
            {
                4,
                1,
                34,
                28,
                3,
                28,
                0,
                28,
                4,
                14,
                0,
                0,
                1,
                12,
                5,
                20,
                25,
                8,
                15,
                27,
                12,
                0,
                0,
                0,
                0,
                3,
                16,
                69,
                63,
                34
            };

            for (var i = 0; i < 30; i++) Assert.Equal(expectedValues[i], calc.CalculateOrderQuantity(list[i]));
        }

        [Fact]
        public void ForNumberDaysCalculation_FirstAvailable30WithCurrent()
        {
            List<ProductTableItem> list = GetTableItems().Where(item => item.AvailableQuantity > 0).ToList();

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60,
                ConsiderCurrentQuantity = true
            };

            ICalculationStrategy<ProductTableItem> method = new ForNumberDaysCalculation();
            OrderCalculator<ProductTableItem>      calc   = new OrderCalculator<ProductTableItem>(method, options);

            int[] expectedValues = new[]
            {
                1,
                0,
                0,
                7,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                69,
                11,
                0
            };

            for (var i = 0; i < 30; i++) Assert.Equal(expectedValues[i], calc.CalculateOrderQuantity(list[i]));
        }

        [Fact]
        public void ByRecommendedCalculation_First30Raw()
        {
            List<ProductTableItem> list = GetTableItems();

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60
            };

            ICalculationStrategy<ProductTableItem>            method = new ByRecommendedCalculation();
            OrderCalculator<ProductTableItem> calc   = new OrderCalculator<ProductTableItem>(method, options);

            int[] expectedValues = new[]
            {
                0,
                0,
                0,
                0,
                2,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
            };

            for (var i = 0; i < 30; i++) Assert.Equal(expectedValues[i], calc.CalculateOrderQuantity(list[i]));
        }

        [Fact]
        public void ByRecommendedCalculation_FirstAvailable30()
        {
            List<ProductTableItem> list = GetTableItems().Where(item => item.AvailableQuantity > 0).ToList();

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60
            };

            ICalculationStrategy<ProductTableItem> method = new ByRecommendedCalculation();
            OrderCalculator<ProductTableItem>      calc   = new OrderCalculator<ProductTableItem>(method, options);

            int[] expectedValues = new[]
            {
                0,
                0,
                0,
                2,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                59,
                0,
                0,
            };

            for (var i = 0; i < 30; i++) Assert.Equal(expectedValues[i], calc.CalculateOrderQuantity(list[i]));
        }

        [Fact]
        public void ByRecommendedCalculation_FirstAvailable30WithCurrent()
        {
            List<ProductTableItem> list = GetTableItems().Where(item => item.AvailableQuantity > 0).ToList();

            CalculationOptions options = new CalculationOptions()
            {
                DaysCount = 60,
                ConsiderCurrentQuantity = true
            };

            ICalculationStrategy<ProductTableItem> method = new ByRecommendedCalculation();
            OrderCalculator<ProductTableItem>      calc   = new OrderCalculator<ProductTableItem>(method, options);

            int[] expectedValues = new[]
            {
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                17,
                0,
                0,
            };

            for (var i = 0; i < 30; i++) Assert.Equal(expectedValues[i], calc.CalculateOrderQuantity(list[i]));
        }
    }
}
