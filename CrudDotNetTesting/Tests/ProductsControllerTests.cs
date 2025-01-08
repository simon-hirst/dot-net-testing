// Tests/ProductsControllerTests.cs

using AutoMapper;
using CrudDotNetTesting.Controllers;
using CrudDotNetTesting.Dtos;
using CrudDotNetTesting.Helpers;
using CrudDotNetTesting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // Add this for GetConnectionString()
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CrudDotNetTesting.Tests
{
    public class ProductsControllerTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            using (var context = CreateContext())
            {
                context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 10 });
                context.Products.Add(new Product { Id = 2, Name = "Product 2", Price = 20 });
                await context.SaveChangesAsync();

                var mockMapper = new Mock<IMapper>();
                mockMapper.Setup(m => m.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                    .Returns(new List<ProductDto>
                    {
                        new ProductDto { Id = 1, Name = "Product 1", Price = 10 },
                        new ProductDto { Id = 2, Name = "Product 2", Price = 20 }
                    });

                var controller = new ProductsController(context, mockMapper.Object);

                // Act
                var result = await controller.GetProducts();

                // Assert
                var okResult = Assert.IsType<ActionResult<IEnumerable<ProductDto>>>(result);
                var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
                Assert.Equal(2, products.Count());
            }
        }

        [Fact]
        public async Task GetProduct_WithExistingId_ReturnsOkResult_WithProduct()
        {
            // Arrange
            using (var context = CreateContext())
            {
                context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 10 });
                await context.SaveChangesAsync();

                var mockMapper = new Mock<IMapper>();
                mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                    .Returns(new ProductDto { Id = 1, Name = "Product 1", Price = 10 });

                var controller = new ProductsController(context, mockMapper.Object);

                // Act
                var result = await controller.GetProduct(1);

                // Assert
                var okResult = Assert.IsType<ActionResult<ProductDto>>(result);
                var product = Assert.IsType<ProductDto>(okResult.Value);
                Assert.Equal(1, product.Id);
                Assert.Equal("Product 1", product.Name);
            }
        }

        [Fact]
        public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            using (var context = CreateContext())
            {
                var mockMapper = new Mock<IMapper>();
                var controller = new ProductsController(context, mockMapper.Object);

                // Act
                var result = await controller.GetProduct(999);

                // Assert
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task PostProduct_ValidObject_ReturnsCreatedAtActionResult()
        {
            // Arrange
            using (var context = CreateContext())
            {
                var mockMapper = new Mock<IMapper>();
                mockMapper.Setup(m => m.Map<Product>(It.IsAny<ProductDto>()))
                    .Returns(new Product { Id = 1, Name = "New Product", Price = 50 });
                mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                    .Returns(new ProductDto { Id = 1, Name = "New Product", Price = 50 });

                var controller = new ProductsController(context, mockMapper.Object);

                var newProductDto = new ProductDto { Name = "New Product", Price = 50 };

                // Act
                var result = await controller.PostProduct(newProductDto);

                // Assert
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                Assert.Equal("GetProduct", createdAtActionResult.ActionName);
                Assert.IsType<ProductDto>(createdAtActionResult.Value);
            }
        }

        [Fact]
        public async Task PutProduct_ValidObject_ReturnsNoContent()
        {
            // Arrange
            using (var context = CreateContext())
            {
                context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 10 });
                await context.SaveChangesAsync();

                // Create a REAL mapper instance, don't mock it
                var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
                var mapper = config.CreateMapper();

                var controller = new ProductsController(context, mapper);
                var updatedProductDto = new ProductDto { Id = 1, Name = "Updated Product", Price = 25 };

                // Act
                var result = await controller.PutProduct(1, updatedProductDto);

                // Assert
                Assert.IsType<NoContentResult>(result);

                // Fetch the updated product using the SAME context
                var updatedProduct = await context.Products.FindAsync(1);
                Assert.Equal("Updated Product", updatedProduct.Name);
                Assert.Equal(25, updatedProduct.Price);
            }
        }

        [Fact]
        public async Task PutProduct_InvalidObject_ReturnsBadRequest()
        {
            // Arrange
            using (var context = CreateContext())
            {
                var mockMapper = new Mock<IMapper>();
                var controller = new ProductsController(context, mockMapper.Object);
                controller.ModelState.AddModelError("Name", "The Name field is required.");
                var invalidProductDto = new ProductDto { Id = 1, Price = 25 };

                // Act
                var result = await controller.PutProduct(1, invalidProductDto);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task DeleteProduct_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            using (var context = CreateContext())
            {
                context.Products.Add(new Product { Id = 1, Name = "Product 1", Price = 10 });
                await context.SaveChangesAsync();

                var mockMapper = new Mock<IMapper>();
                var controller = new ProductsController(context, mockMapper.Object);

                // Act
                var result = await controller.DeleteProduct(1);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }
        }

        [Fact]
        public async Task DeleteProduct_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            using (var context = CreateContext())
            {
                var mockMapper = new Mock<IMapper>();
                var controller = new ProductsController(context, mockMapper.Object);

                // Act
                var result = await controller.DeleteProduct(999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}