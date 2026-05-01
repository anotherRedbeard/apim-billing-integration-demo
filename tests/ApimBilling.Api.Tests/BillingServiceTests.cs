using ApimBilling.Api.Models;
using ApimBilling.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApimBilling.Api.Tests;

public class BillingServiceTests
{
    private readonly Mock<IApimSubscriptionClient> _mockApimClient;
    private readonly BillingService _billingService;

    public BillingServiceTests()
    {
        _mockApimClient = new Mock<IApimSubscriptionClient>();
        var mockLogger = new Mock<ILogger<BillingService>>();
        _billingService = new BillingService(_mockApimClient.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetProductsAsync_FiltersOutProductsWithoutLegalTerms()
    {
        // Arrange
        var apimProducts = new ApimProductListResponse
        {
            Value = new[]
            {
                new ApimProductResponse
                {
                    Name = "bronze",
                    Properties = new ProductProperties
                    {
                        DisplayName = "Bronze",
                        Description = "Bronze tier",
                        State = "published",
                        Terms = "Legal terms for Bronze"
                    }
                },
                new ApimProductResponse
                {
                    Name = "starter",
                    Properties = new ProductProperties
                    {
                        DisplayName = "Starter",
                        Description = "Starter product with no terms",
                        State = "published",
                        Terms = null
                    }
                },
                new ApimProductResponse
                {
                    Name = "unlimited",
                    Properties = new ProductProperties
                    {
                        DisplayName = "Unlimited",
                        Description = "Unlimited product with empty terms",
                        State = "published",
                        Terms = ""
                    }
                }
            }
        };

        _mockApimClient.Setup(c => c.ListApimProductsAsync())
            .ReturnsAsync(apimProducts);

        // Act
        var products = await _billingService.GetProductsAsync();

        // Assert
        Assert.Single(products);
        Assert.Equal("bronze", products[0].ProductId);
    }

    [Fact]
    public async Task GetProductsAsync_FiltersOutUnpublishedProducts()
    {
        // Arrange
        var apimProducts = new ApimProductListResponse
        {
            Value = new[]
            {
                new ApimProductResponse
                {
                    Name = "gold",
                    Properties = new ProductProperties
                    {
                        DisplayName = "Gold",
                        Description = "Gold tier",
                        State = "notPublished",
                        Terms = "Legal terms for Gold"
                    }
                },
                new ApimProductResponse
                {
                    Name = "silver",
                    Properties = new ProductProperties
                    {
                        DisplayName = "Silver",
                        Description = "Silver tier",
                        State = "published",
                        Terms = "Legal terms for Silver"
                    }
                }
            }
        };

        _mockApimClient.Setup(c => c.ListApimProductsAsync())
            .ReturnsAsync(apimProducts);

        // Act
        var products = await _billingService.GetProductsAsync();

        // Assert
        Assert.Single(products);
        Assert.Equal("silver", products[0].ProductId);
    }
}
