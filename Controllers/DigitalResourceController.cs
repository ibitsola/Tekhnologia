using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Controllers
{
    [Route("api/digitalresources")]
    [ApiController]
    public class DigitalResourceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DigitalResourceController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // List all resources with optional filters (category, free/paid)
        [HttpGet]
        public IActionResult GetAllResources([FromQuery] string? category, [FromQuery] bool? isFree)
        {
            var query = _context.DigitalResources.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(r => r.Category == category);

            if (isFree.HasValue)
                query = query.Where(r => r.IsFree == isFree.Value);

            var resources = query.Select(r => new DigitalResourceDTO
            {
                Id = r.Id,
                Title = r.Title,
                FileType = r.FileType,
                Category = r.Category,
                IsFree = r.IsFree,
                Price = r.Price,
                FilePath = r.FilePath,
                UploadDate = r.UploadDate
            }).ToList();

            return Ok(resources);
        }

        // Admin: Upload a new resource
        [Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadResource([FromForm] CreateDigitalResourceDTO resourceDTO)
        {
            if (resourceDTO.File == null || resourceDTO.File.Length == 0)
                return BadRequest("File is missing.");

            string uploadPath = Path.Combine(_env.WebRootPath, "digital-resources");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string uniqueFileName = $"{Guid.NewGuid()}_{resourceDTO.File.FileName}";
            string filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await resourceDTO.File.CopyToAsync(stream);

            var newResource = new DigitalResource
            {
                Title = resourceDTO.Title,
                FileName = uniqueFileName,
                FilePath = $"/digital-resources/{uniqueFileName}",
                FileType = Path.GetExtension(resourceDTO.File.FileName).Substring(1),
                IsFree = resourceDTO.IsFree,
                Price = resourceDTO.IsFree ? null : resourceDTO.Price,
                Category = resourceDTO.Category,
                UploadedBy = User.Identity?.Name ?? "Unknown"
            };

            _context.DigitalResources.Add(newResource);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resource uploaded successfully!", newResource });
        }

        // Users: Download a resource (Free or Purchased in the Future)
        [HttpGet("download/{id}")]
        public IActionResult DownloadResource(int id)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                return NotFound("Resource not found.");

            if (!resource.IsFree)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var purchase = _context.Purchases
                    .FirstOrDefault(p => p.DigitalResourceId == id && p.UserId == userId && p.IsPaid);

                if (purchase == null)
                    return BadRequest("Payment required to download this resource.");
            }

            string fullPath = Path.Combine(_env.WebRootPath, "digital-resources", resource.FileName);
            if (!System.IO.File.Exists(fullPath))
                return NotFound("File does not exist.");

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/octet-stream", resource.FileName);
        }


        // Admin: Delete a resource
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteResource(int id)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                return NotFound("Resource not found.");

            string fullPath = Path.Combine(_env.WebRootPath, "digital-resources", resource.FileName);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.DigitalResources.Remove(resource);
            _context.SaveChanges();

            return Ok("Resource deleted successfully.");
        }

        // Admin: Edit resource details (title, pricing, category)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult EditResource(int id, [FromBody] DigitalResourceDTO updatedResource)
        {
            var resource = _context.DigitalResources.Find(id);
            if (resource == null)
                return NotFound("Resource not found.");

            resource.Title = updatedResource.Title;
            resource.IsFree = updatedResource.IsFree;
            resource.Price = updatedResource.IsFree ? null : updatedResource.Price;
            resource.Category = updatedResource.Category;

            _context.SaveChanges();
            return Ok("Resource updated successfully.");
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
            Console.WriteLine($"[DEBUG] Created Checkout Session ID: {session.Id}");



            // Store session details in DB
            var purchase = new Purchase
            {
                UserId = userId,
                DigitalResourceId = resource.Id,
                StripeSessionId = session.Id,
                IsPaid = false
            };
            // Log just after session id is assigned for debugging purpuses
            Console.WriteLine($"Storing Stripe Session ID: {session.Id} for Purchase {resource.Id}");
            // Debugging before saving to the database
            Console.WriteLine($"[DEBUG] Purchase Created - User: {userId}, Resource: {resource.Id}, Stored Session ID: {session.Id}");

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

        // Retrieves a list of all completed purchases
        [Authorize(Roles = "Admin")]
        [HttpGet("purchases")]
        public IActionResult GetPurchases()
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
    }
}
