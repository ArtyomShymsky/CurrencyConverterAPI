using CurrencyConverterAPI.Controllers;
using CurrencyConverterAPI.DTOs;
using CurrencyConverterAPI.Intefaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyTestsProject
{
    public class CurrencyConversionControllerTests
    {
        private readonly Mock<ICurrencyConversionService> _mockConversionService;
        private readonly CurrencyConversionController _controller;

        public CurrencyConversionControllerTests()
        {
            _mockConversionService = new Mock<ICurrencyConversionService>();
            _controller = new CurrencyConversionController(_mockConversionService.Object);
        }

        [Fact]
        public async Task ConvertCurrency_ReturnsOkResult_WhenConversionIsSuccessful()
        {
            // Arrange
            var from = "USD";
            var to = "EUR";
            var expectedRate = 0.85m;
            var exchangeRateResponse = new ExchangeRateResponse
            {
                Rates = new Dictionary<string, decimal>()
                {
                    {"EUR", 0.85m}
                }
            };

            _mockConversionService.Setup(service => service.GetRatesAsync(from, to))
                .ReturnsAsync(exchangeRateResponse);

            // Act
            var result = await _controller.GetExchangeRate(from, to);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ExchangeRateResponse>(okResult.Value);
            //Assert.Equal(from, value.From);
            //Assert.Equal(to, value.To);
            Assert.Equal(expectedRate, value.Rates.GetValueOrDefault("EUR"));
        }

        [Fact]
        public async Task ConvertCurrency_ReturnsNotFound_WhenConversionFails()
        {
            // Arrange
            var from = "USDT";
            var to = "EUR";

            // Act
            var result = await _controller.GetExchangeRate(from, to);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
