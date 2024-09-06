using System.Net;
using System.Text;
using Moq;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.Data.Tests;

public class ProductRepositoryTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly ProductRepository        _productRepository;
    
    public ProductRepositoryTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        HttpClient httpClient = _mockHttpMessageHandler.CreateClient();
        _productRepository = new ProductRepository(httpClient);
    }
    
    [Fact]
    public async Task ValidateAccessKeyAsync_ValidKey_ReturnsTrue()
    {
        // Arrange
        string accessKey = "valid-access-key";
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.OK);
        
        // Act
        var result = await _productRepository.ValidateAccessKeyAsync(accessKey);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ValidateAccessKeyAsync_InvalidKey_ReturnsFalse()
    {
        // Arrange
        var accessKey = "invalid-access-key";
        
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.Unauthorized);
        
        // Act
        var result = await _productRepository.ValidateAccessKeyAsync(accessKey);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ValidateAccessKeyAsync_ExceptionThrown_ReturnsFalse()
    {
        // Arrange
        var accessKey = "exception-access-key";
        
        _mockHttpMessageHandler.SetupAnyRequest()
            .ThrowsAsync(new HttpRequestException());
        
        // Act
        var result = await _productRepository.ValidateAccessKeyAsync(accessKey);
        
        // Assert
        Assert.False(result);
    }
    
    private const string Uri = "https://warehouseassistantdbapi.onrender.com/api/products";
    
    [Fact]
    public async Task GetByArticleAsync_ValidArticle_ReturnsProduct()
    {
        // Arrange
        var article = "valid-article";
        var product = new Product { Article = article };
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, $"{Uri}/{article}")
            .ReturnsResponse(HttpStatusCode.OK,
                new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json"));
        
        // Act
        var result = await _productRepository.GetByArticleAsync(article);
        
        // Assert
        Assert.Equivalent(product, result);
    }
    
    [Fact]
    public async Task GetByArticleAsync_InvalidArticle_ReturnsNull()
    {
        // Arrange
        var article = "invalid-article";
        _mockHttpMessageHandler.SetupAnyRequest()
            .ThrowsAsync(new HttpRequestException("", null, HttpStatusCode.NotFound));
        
        // Act
        var result = await _productRepository.GetByArticleAsync(article);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByArticleAsync_EmptyArticle_ReturnsNull()
    {
        // Arrange
        var article = string.Empty;
        
        // Act
        var result = await _productRepository.GetByArticleAsync(article);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsListOfProducts()
    {
        // Arrange
        var products = new List<Product> { new Product { Article = "article1" }, new Product { Article = "article2" } };
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, $"{Uri}")
            .ReturnsResponse(HttpStatusCode.OK,
                new StringContent(JsonConvert.SerializeObject(products), Encoding.UTF8, "application/json"));
        
        // Act
        var result = await _productRepository.GetAllAsync();
        
        // Assert
        Assert.Equivalent(products, result);
    }
    
    [Fact]
    public async Task AddAsync_ValidProduct_Success()
    {
        // Arrange
        var product = new Product { Article = "new-article" };
        _productRepository.SetAccessKey("valid-access-key");
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.OK);
        
        // Act
        await _productRepository.AddAsync(product);
        
        // Assert
        // No exception thrown
    }
    
    [Fact]
    public async Task AddAsync_InvalidAccessKey_ThrowsException()
    {
        // Arrange
        var product = new Product { Article = "new-article" };
        _productRepository.SetAccessKey("invalid-access-key");
        
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.Unauthorized);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _productRepository.AddAsync(product));
    }
    
    [Fact]
    public async Task UpdateAsync_ValidProduct_Success()
    {
        // Arrange
        var product = new Product { Article = "existing-article" };
        _productRepository.SetAccessKey("valid-access-key");
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.OK);
        
        // Act
        await _productRepository.UpdateAsync(product);
        
        // Assert
        // No exception thrown
    }
    
    [Fact]
    public async Task UpdateAsync_InvalidAccessKey_ThrowsException()
    {
        // Arrange
        var product = new Product { Article = "existing-article" };
        _productRepository.SetAccessKey("invalid-access-key");
        
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.Unauthorized);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _productRepository.UpdateAsync(product));
    }
    
    [Fact]
    public async Task DeleteAsync_ValidArticle_Success()
    {
        // Arrange
        var article = "existing-article";
        _productRepository.SetAccessKey("valid-access-key");
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.OK);
        
        // Act
        await _productRepository.DeleteAsync(article);
        
        // Assert
        // No exception thrown
    }
    
    [Fact]
    public async Task DeleteAsync_InvalidAccessKey_ThrowsException()
    {
        // Arrange
        var article = "existing-article";
        _productRepository.SetAccessKey("invalid-access-key");
        
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.Unauthorized);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _productRepository.DeleteAsync(article));
    }
    
    [Fact]
    public async Task DeleteAsync_NonExistentArticle_NoException()
    {
        // Arrange
        var article = "non-existent-article";
        _productRepository.SetAccessKey("valid-access-key");
        
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.NotFound);
        
        // Act
        await _productRepository.DeleteAsync(article);
        
        // Assert
        // No exception thrown
    }
    
    [Fact]
    public async Task ValidateAccessKeyAsync_ValidKeyShouldCacheResult()
    {
        // Arrange
        string accessKey = "valid-access-key";
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.OK);
        
        await _productRepository.ValidateAccessKeyAsync(accessKey);
        
        // Act
        await _productRepository.ValidateAccessKeyAsync(accessKey);
        
        // Assert
        _mockHttpMessageHandler.VerifyAnyRequest(Times.Once());
    }
    
    [Fact]
    public void DeleteAsync_ShouldThrowException_WithoutAccessKey()
    {
        // Arrange
        _mockHttpMessageHandler.SetupAnyRequest()
            .ReturnsResponse(HttpStatusCode.OK);
        
        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(() => _productRepository.DeleteAsync("article"));
    }
}