using Microsoft.AspNetCore.Mvc;
using Stripe;
using Data;
using Microsoft.AspNetCore.Authorization;
using Models;
using Models.DTOs;
using Stripe.Checkout;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const string StripeWebhookSecret = "whsec_e00cf0870ce25d56148f90592eb041ee5e3deb605821308fb24a370b1c25290e"; // Replace with actual secret

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Creates a checkout session for purchasing a digital resource using Stripe
        [Authorize]
        [HttpPost("create-checkout-session/{id}")]
        public async Task<IActionResult> CreateCheckoutSession(int id)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null || resource.IsFree)
                return BadRequest("Invalid or free resource.");

            if (resource.Price == null)
                return BadRequest("Price cannot be null.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var domain = "https://tekhnologia.co.uk/";

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
                            UnitAmount = (long)(resource.Price.Value * 100), // Convert to cents
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "resourceId", resource.Id.ToString() }
                }
            };

            var stripeClient = new StripeClient(StripeConfiguration.ApiKey);
            var service = new SessionService(stripeClient);
            var session = await service.CreateAsync(options);

            // Store session details in DB
            var purchase = new Purchase
            {
                UserId = userId,
                DigitalResourceId = resource.Id,
                StripeSessionId = session.Id,
                IsPaid = false
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok(new { url = session.Url });
        }

        // Retrieves a list of digital resources that the authenticated user has purchased
        [Authorize]
        [HttpGet("my-purchases")]
        public IActionResult GetUserPurchases()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var purchases = _context.Purchases
                .Where(p => p.UserId == userId && p.IsPaid)
                .Select(p => new PurchaseDTO
                {
                    Id = p.Id,
                    DigitalResourceId = p.DigitalResourceId,
                    ResourceTitle = p.DigitalResource.Title,
                    Price = p.DigitalResource.Price ?? 0,
                    PurchaseDate = p.PurchaseDate
                })
                .ToList();

            return Ok(purchases);
        }
        
        // returns paid and unpaid purchases
        [Authorize(Roles = "Admin")]
        [HttpGet("all-purchases")]
        public IActionResult GetAllPurchases()
        {
            var purchases = _context.Purchases
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

            return Ok(purchases);
        }

        // fetches only paid purchases
        [Authorize(Roles = "Admin")]
        [HttpGet("paid-purchases")]
        public IActionResult GetPaidPurchases()
        {
            var purchases = _context.Purchases
                .Where(p => p.IsPaid)
                .Select(p => new PurchaseDTO
                {
                    Id = p.Id,
                    DigitalResourceId = p.DigitalResourceId,
                    ResourceTitle = p.DigitalResource.Title,
                    Price = p.DigitalResource.Price ?? 0,
                    PurchaseDate = p.PurchaseDate
                })
                .ToList();

            return Ok(purchases);
        }

        // Deletes purchases
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-purchase/{id}")]
        public IActionResult DeletePurchase(int id)
        {
            var purchase = _context.Purchases.Find(id);
            if (purchase == null)
                return NotFound("Purchase not found.");

            _context.Purchases.Remove(purchase);
            _context.SaveChanges();

            return Ok($"Purchase ID {id} deleted successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("mark-paid/{id}")]
        public IActionResult MarkPurchaseAsPaid(int id)
        {
            var purchase = _context.Purchases.Find(id);
            if (purchase == null)
                return NotFound("Purchase not found.");

            if (purchase.IsPaid)
                return BadRequest("Purchase is already marked as paid.");

            purchase.IsPaid = true;
            _context.SaveChanges();

            return Ok($"Purchase ID {id} marked as paid.");
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, 
                    Request.Headers["Stripe-Signature"], StripeWebhookSecret);
            }
            catch (StripeException e)
            {
                return BadRequest($"Webhook error: {e.Message}");
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                
                if (session == null)
                {
                    return BadRequest("Stripe session is null.");
                }

                if (string.IsNullOrEmpty(session.Id))
                {
                    return BadRequest("Session ID is null or empty.");
                }
                var purchase = _context.Purchases
                    .FirstOrDefault(p => p.StripeSessionId == session.Id);

                if (purchase != null)
                {
                    purchase.IsPaid = true;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok();
        }        
    }

}
