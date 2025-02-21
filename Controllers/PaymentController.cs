using Microsoft.AspNetCore.Mvc;
using Stripe;
using Data;


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

        // [HttpPost("webhook")]
        // public async Task<IActionResult> StripeWebhook()
        // {
        //     var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        //     Event stripeEvent;

        //     try
        //     {
        //         stripeEvent = EventUtility.ConstructEvent(json, 
        //             Request.Headers["Stripe-Signature"], StripeWebhookSecret);
        //     }
        //     catch (StripeException e)
        //     {
        //         return BadRequest($"Webhook error: {e.Message}");
        //     }

        //     if (stripeEvent.Type == "checkout.session.completed")
        //     {
        //         var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                
        //         if (session == null)
        //         {
        //             return BadRequest("Stripe session is null.");
        //         }

        //         if (string.IsNullOrEmpty(session.Id))
        //         {
        //             return BadRequest("Session ID is null or empty.");
        //         }
        //         var purchase = _context.Purchases
        //             .FirstOrDefault(p => p.StripeSessionId == session.Id);

        //         if (purchase != null)
        //         {
        //             purchase.IsPaid = true;
        //             await _context.SaveChangesAsync();
        //         }
        //     }

        //     return Ok();
        // }


        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json, Request.Headers["Stripe-Signature"], StripeWebhookSecret
                );
            }
            catch (StripeException e)
            {
                Console.WriteLine($"[DEBUG] Webhook error: {e.Message}");
                return BadRequest($"Webhook error: {e.Message}");
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                if (session == null)
                {
                    Console.WriteLine("[DEBUG] Stripe session is null.");
                    return BadRequest("Stripe session is null.");
                }

                if (string.IsNullOrEmpty(session.Id))
                {
                    Console.WriteLine("[DEBUG] Session ID is null or empty.");
                    return BadRequest("Session ID is null or empty.");
                }

                Console.WriteLine($"[DEBUG] Webhook received session ID: {session.Id}");

                var purchase = _context.Purchases.FirstOrDefault(p => p.StripeSessionId == session.Id);

                if (purchase != null)
                {
                    Console.WriteLine($"[DEBUG] Found purchase with ID {purchase.Id}. Updating IsPaid to true.");
                    purchase.IsPaid = true;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"[DEBUG] No purchase found for session ID: {session.Id}");
                }
            }

            return Ok();
        }

        
    }

}
