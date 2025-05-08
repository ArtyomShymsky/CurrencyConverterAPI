namespace CurrencyTestsProject
{
    using Xunit;
    using Moq;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using CurrencyConverterAPI.Intefaces;
    using CurrencyConverterAPI.DTOs;
    using CurrencyConverterAPI.Controllers;
    using Polly.CircuitBreaker;

    public class CurrencyControllerTests
    {
        [Fact]
        public async Task GetLatestRates_ReturnsOkResult_WithRates()
        {
            // Arrange
            var mockService = new Mock<ICurrencyRateService>();
            mockService.Setup(s => s.GetLatestRatesAsync("USD"))
                .ReturnsAsync(new CurrencyRatesResponse
                {
                    Base = "USD",
                    Date = "2025-05-08",
                    Rates = new Dictionary<string, decimal>
                    {
                    { "EUR", 0.9185M },
                    { "JPY", 155.62M }
                    }
                });

            var controller = new CurrencyRatesController(mockService.Object);

            // Act
            var result = await controller.GetLatestRates("USD");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CurrencyRatesResponse>(okResult.Value);

            Assert.Equal("USD", response.Base);
            Assert.Equal(2, response.Rates.Count);
            Assert.True(response.Rates.ContainsKey("EUR"));
        }

        [Fact]
        public async Task GetLatestRates_ReturnsNotFound_WhenServiceReturnsNull()
        {
            // Arrange
            var mockService = new Mock<ICurrencyRateService>();
            mockService.Setup(s => s.GetLatestRatesAsync("USD"))
                .ReturnsAsync((CurrencyRatesResponse?)null);

            var controller = new CurrencyRatesController(mockService.Object);

            // Act
            var result = await controller.GetLatestRates("USD");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetLatestRates_ReturnsServiceUnavailable_WhenCircuitIsOpen()
        {
            // Arrange
            var mockService = new Mock<ICurrencyRateService>();
            mockService.Setup(s => s.GetLatestRatesAsync(It.IsAny<string>()))
                       .ThrowsAsync(new BrokenCircuitException("Circuit is open"));

            var controller = new CurrencyRatesController(mockService.Object);

            // Act
            var result = await controller.GetLatestRates("USD");

            // Assert
            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, status.StatusCode);
        }


    }

}