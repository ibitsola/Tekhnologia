using Tekhnologia.Models.DTOs;
using Stripe.Checkout;


namespace Tekhnologia.Services.Interfaces
{
    public interface IPaymentService
    {
        
        // Creates a Stripe checkout session for purchasing a digital resource.     
        Task<Session> CreateCheckoutSessionAsync(int resourceId, string userId);

        
        // Returns a list of purchases for the specified user that are paid.        
        List<PurchaseDTO> GetUserPurchases(string userId);

        // Returns all purchases (both paid and unpaid).
        List<PurchaseDTO> GetAllPurchases();

        
        // Returns only paid purchases.        
        List<PurchaseDTO> GetPaidPurchases();

        
        // Deletes a purchase record.
        Task DeletePurchaseAsync(int id);

        // Marks a purchase as paid.
        Task MarkPurchaseAsPaidAsync(int id);

        // Processes a Stripe webhook event and marks the associated purchase as paid.
        Task ProcessStripeWebhookAsync(string json, string stripeSignature);
    }
}
