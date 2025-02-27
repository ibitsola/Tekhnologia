using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Stripe;
using Stripe.Checkout;
using Tekhnologia.Services.Interfaces;


namespace Tekhnologia.Services
{
    /// <summary>
    /// Provides business logic for processing digital resource purchases using Stripe.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _domain;
        private readonly string _stripeWebhookSecret;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
            // You may want to move these settings to configuration.
            _domain = "https://tekhnologia.co.uk/";
            _stripeWebhookSecret = "whsec_e00cf0870ce25d56148f90592eb041ee5e3deb605821308fb24a370b1c25290e";
        }

        /// <summary>
        /// Creates a Stripe checkout session for purchasing a digital resource.
        /// </summary>
        public async Task<Session> CreateCheckoutSessionAsync(int resourceId, string userId)
        {
            var resource = await _context.DigitalResources.FindAsync(resourceId);
            if (resource == null || resource.IsFree)
                throw new Exception("Invalid or free resource.");

            if (resource.Price == null)
                throw new Exception("Price cannot be null.");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = resource.Title,
                            },
                            UnitAmount = (long)(resource.Price.Value * 100), // convert dollars to cents
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_domain}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_domain}/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "resourceId", resource.Id.ToString() }
                }
            };

            var stripeClient = new StripeClient(StripeConfiguration.ApiKey);
            var service = new SessionService(stripeClient);
            var session = await service.CreateAsync(options);

            // Create a purchase record with IsPaid false
            var purchase = new Purchase
            {
                UserId = userId,
                DigitalResourceId = resource.Id,
                StripeSessionId = session.Id,
                IsPaid = false,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return session;
        }

        /// <summary>
        /// Returns a list of purchases for the specified user that are paid.
        /// </summary>
        public List<PurchaseDTO> GetUserPurchases(string userId)
        {
            var purchases = _context.Purchases
                .Where(p => p.UserId == userId && p.IsPaid)
                .Select(p => new PurchaseDTO
                {
                    Id = p.Id,
                    DigitalResourceId = p.DigitalResourceId,
                    ResourceTitle = p.DigitalResource.Title,
                    Price = p.DigitalResource.Price ?? 0,
                    PurchaseDate = p.PurchaseDate,
                    IsPaid = p.IsPaid
                })
                .ToList();

            return purchases;
        }

        /// <summary>
        /// Returns all purchases (both paid and unpaid).
        /// </summary>
        public List<PurchaseDTO> GetAllPurchases()
        {
            return _context.Purchases
                .Select(p => new PurchaseDTO
                {
                    Id = p.Id,
                    DigitalResourceId = p.DigitalResourceId,
                    ResourceTitle = p.DigitalResource.Title,
                    Price = p.DigitalResource.Price ?? 0,
                    PurchaseDate = p.PurchaseDate,
                    IsPaid = p.IsPaid
                })
                .ToList();
        }

        /// <summary>
        /// Returns only paid purchases.
        /// </summary>
        public List<PurchaseDTO> GetPaidPurchases()
        {
            return _context.Purchases
                .Where(p => p.IsPaid)
                .Select(p => new PurchaseDTO
                {
                    Id = p.Id,
                    DigitalResourceId = p.DigitalResourceId,
                    ResourceTitle = p.DigitalResource.Title,
                    Price = p.DigitalResource.Price ?? 0,
                    PurchaseDate = p.PurchaseDate,
                    IsPaid = p.IsPaid
                })
                .ToList();
        }

        /// <summary>
        /// Deletes a purchase record.
        /// </summary>
        public async Task DeletePurchaseAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                throw new Exception("Purchase not found.");

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Marks a purchase as paid.
        /// </summary>
        public async Task MarkPurchaseAsPaidAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
                throw new Exception("Purchase not found.");

            if (purchase!.IsPaid)
                throw new Exception("Purchase is already marked as paid.");

            purchase.IsPaid = true;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Processes a Stripe webhook event and marks the associated purchase as paid.
        /// </summary>
        public async Task ProcessStripeWebhookAsync(string json, string stripeSignature)
        {
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeWebhookSecret);
            }
            catch (StripeException e)
            {
                throw new Exception($"Webhook error: {e.Message}");
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session == null || string.IsNullOrEmpty(session!.Id))
                    throw new Exception("Invalid Stripe session.");

                var purchase = _context.Purchases.FirstOrDefault(p => p.StripeSessionId == session.Id);
                if (purchase != null)
                {
                    purchase.IsPaid = true;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
