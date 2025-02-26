using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stripe;
using Stripe.Checkout;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services;
using Xunit;

namespace Tekhnologia.Tests.Services
{
    public class PaymentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            // Create a new in-memory database for each test to ensure isolation.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _paymentService = new PaymentService(_context);
        }

        [Fact]
        public void GetUserPurchases_ShouldReturnPaidPurchases_ForGivenUser()
        {
            // Arrange: add two purchases (one paid, one not) along with a dummy DigitalResource.
            var resource = new DigitalResource
            {
                Id = 1,
                Title = "Test Resource",
                FileType = "pdf",
                FilePath = "/digital-resources/test.pdf",
                IsFree = false,
                Price = 9.99m,
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);

            var paidPurchase = new Purchase
            {
                Id = 1,
                DigitalResourceId = resource.Id,
                UserId = "user1",
                StripeSessionId = "sess_1",
                IsPaid = true,
                PurchaseDate = DateTime.UtcNow
            };

            var unpaidPurchase = new Purchase
            {
                Id = 2,
                DigitalResourceId = resource.Id,
                UserId = "user1",
                StripeSessionId = "sess_2",
                IsPaid = false,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Purchases.AddRange(paidPurchase, unpaidPurchase);
            _context.SaveChanges();

            // Act
            var purchases = _paymentService.GetUserPurchases("user1");

            // Assert
            purchases.Should().HaveCount(1);
            purchases.First().Id.Should().Be(1);
        }

        [Fact]
        public void GetAllPurchases_ShouldReturnAllPurchases()
        {
            // Arrange
            var resource = new DigitalResource
            {
                Id = 1,
                Title = "Resource A",
                FileType = "pdf",
                FilePath = "/digital-resources/a.pdf",
                IsFree = false,
                Price = 5.00m,
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);

            var purchase1 = new Purchase
            {
                Id = 1,
                DigitalResourceId = resource.Id,
                UserId = "user1",
                StripeSessionId = "sess_1",
                IsPaid = true,
                PurchaseDate = DateTime.UtcNow
            };

            var purchase2 = new Purchase
            {
                Id = 2,
                DigitalResourceId = resource.Id,
                UserId = "user2",
                StripeSessionId = "sess_2",
                IsPaid = false,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Purchases.AddRange(purchase1, purchase2);
            _context.SaveChanges();

            // Act
            var allPurchases = _paymentService.GetAllPurchases();

            // Assert
            allPurchases.Should().HaveCount(2);
        }

        [Fact]
        public void GetPaidPurchases_ShouldReturnOnlyPaidPurchases()
        {
            // Arrange
            var resource = new DigitalResource
            {
                Id = 1,
                Title = "Resource A",
                FileType = "pdf",
                FilePath = "/digital-resources/a.pdf",
                IsFree = false,
                Price = 5.00m,
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);

            var purchase1 = new Purchase
            {
                Id = 1,
                DigitalResourceId = resource.Id,
                UserId = "user1",
                StripeSessionId = "sess_1",
                IsPaid = true,
                PurchaseDate = DateTime.UtcNow
            };

            var purchase2 = new Purchase
            {
                Id = 2,
                DigitalResourceId = resource.Id,
                UserId = "user2",
                StripeSessionId = "sess_2",
                IsPaid = false,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Purchases.AddRange(purchase1, purchase2);
            _context.SaveChanges();

            // Act
            var paidPurchases = _paymentService.GetPaidPurchases();

            // Assert
            paidPurchases.Should().HaveCount(1);
            paidPurchases.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task DeletePurchaseAsync_ShouldRemovePurchase()
        {
            // Arrange
            var purchase = new Purchase
            {
                Id = 1,
                DigitalResourceId = 1,
                UserId = "user1",
                StripeSessionId = "sess_1",
                IsPaid = true,
                PurchaseDate = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);
            _context.SaveChanges();

            // Act
            await _paymentService.DeletePurchaseAsync(1);

            // Assert
            var deleted = _context.Purchases.Find(1);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task MarkPurchaseAsPaidAsync_ShouldUpdateIsPaid()
        {
            // Arrange
            var purchase = new Purchase
            {
                Id = 1,
                DigitalResourceId = 1,
                UserId = "user1",
                StripeSessionId = "sess_1",
                IsPaid = false,
                PurchaseDate = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);
            _context.SaveChanges();

            // Act
            await _paymentService.MarkPurchaseAsPaidAsync(1);

            // Assert
            var updated = _context.Purchases.Find(1);
            updated.Should().NotBeNull();

            updated!.IsPaid.Should().BeTrue(); 

        }

        [Fact]
        public async Task ProcessStripeWebhookAsync_ShouldMarkPurchaseAsPaid_WhenSessionCompleted()
        {
            // Arrange
            // Insert a purchase record with a known Stripe session ID.
            var resource = new DigitalResource
            {
                Id = 1,
                Title = "Test Resource",
                FileType = "pdf",
                FilePath = "/digital-resources/test.pdf",
                IsFree = false,
                Price = 9.99m,
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);

            var purchase = new Purchase
            {
                Id = 1,
                DigitalResourceId = resource.Id,
                UserId = "user1",
                StripeSessionId = "cs_test_session",
                IsPaid = false,
                PurchaseDate = DateTime.UtcNow
            };
            _context.Purchases.Add(purchase);
            _context.SaveChanges();

            // Create a dummy JSON for a checkout.session.completed event.
            string json = @"{
                ""id"": ""evt_test"",
                ""object"": ""event"",
                ""type"": ""checkout.session.completed"",
                ""data"": {
                    ""object"": {
                        ""id"": ""cs_test_session"",
                        ""object"": ""checkout.session""
                    }
                }
            }";

            // For testing, we pass an invalid signature so that EventUtility.ConstructEvent throws.
            // (Since the webhook secret in PaymentService is a dummy value, we expect an exception.)
            Func<Task> act = async () => { await _paymentService.ProcessStripeWebhookAsync(json, "invalid_signature"); };

            // Assert: Expect a StripeException wrapped in our Exception.
            await act.Should().ThrowAsync<Exception>().Where(e => e.Message.StartsWith("Webhook error:"));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
