using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Tekhnologia.Services.Interfaces; // use the interface namespace

namespace Tekhnologia.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Creates a checkout session for purchasing a digital resource using Stripe
        [Authorize]
        [HttpPost("create-checkout-session/{id}")]
        public async Task<IActionResult> CreateCheckoutSession(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found.");

                var session = await _paymentService.CreateCheckoutSessionAsync(id, userId);
                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Retrieves a list of digital resources that the authenticated user has purchased
        [Authorize]
        [HttpGet("my-purchases")]
        public IActionResult GetUserPurchases()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found.");

            var purchases = _paymentService.GetUserPurchases(userId);
            return Ok(purchases);
        }

        // Returns all purchases (both paid and unpaid).
        [Authorize(Roles = "Admin")]
        [HttpGet("all-purchases")]
        public IActionResult GetAllPurchases()
        {
            var purchases = _paymentService.GetAllPurchases();
            return Ok(purchases);
        }

        // Fetches only paid purchases (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("paid-purchases")]
        public IActionResult GetPaidPurchases()
        {
            var purchases = _paymentService.GetPaidPurchases();
            return Ok(purchases);
        }

        // Deletes a purchase (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-purchase/{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            try
            {
                await _paymentService.DeletePurchaseAsync(id);
                return Ok($"Purchase ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Marks a purchase as paid (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("mark-paid/{id}")]
        public async Task<IActionResult> MarkPurchaseAsPaid(int id)
        {
            try
            {
                await _paymentService.MarkPurchaseAsPaidAsync(id);
                return Ok($"Purchase ID {id} marked as paid.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Stripe webhook endpoint
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrEmpty(stripeSignature))
            {
                return BadRequest("Missing Stripe-Signature header.");
            }

            try
            {
                await _paymentService.ProcessStripeWebhookAsync(json, stripeSignature);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
