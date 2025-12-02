# Securing Your Stripe Webhook Secret

## ⚠️ IMPORTANT: Key Has Been Leaked

The Stripe webhook secret was accidentally committed to the repository. Follow these steps to secure it:

## Immediate Actions Required

### 1. **Revoke the Compromised Key** ✅ CRITICAL
   - Go to your [Stripe Dashboard](https://dashboard.stripe.com/)
   - Navigate to **Developers → Webhooks**
   - Find the webhook endpoint and **delete or regenerate** the signing secret
   - **Generate a new webhook secret**

### 2. **Configure the New Secret Locally**

#### Option A: Using appsettings.json (Local Development)
1. Copy `appsettings.example.json` to `appsettings.json`
2. Add your new webhook secret:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_actual_key",
    "PublishableKey": "pk_test_your_actual_key",
    "WebhookSecret": "whsec_your_new_webhook_secret"
  }
}
```

**Note:** `appsettings.json` is now in `.gitignore` and will NOT be committed.

#### Option B: Using Environment Variables
```powershell
# PowerShell
$env:Stripe__WebhookSecret="whsec_your_new_webhook_secret"

# Bash/Linux
export Stripe__WebhookSecret="whsec_your_new_webhook_secret"
```

#### Option C: Using User Secrets (Recommended for Development)
```powershell
cd Tekhnologia
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_your_new_webhook_secret"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_actual_key"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_your_actual_key"
```

### 3. **Production Deployment**
For production, use your hosting platform's secret management:

- **Azure**: Use Azure Key Vault or App Configuration
- **AWS**: Use AWS Secrets Manager or Parameter Store
- **Heroku**: Use Config Vars
- **Docker**: Use environment variables or Docker secrets

## What Changed

### Before (INSECURE ❌)
```csharp
_stripeWebhookSecret = "whsec_hardcoded_secret";
```

### After (SECURE ✅)
```csharp
_stripeWebhookSecret = configuration["Stripe:WebhookSecret"] 
    ?? throw new InvalidOperationException("Stripe webhook secret is not configured");
```

## Files Modified
- ✅ `PaymentService.cs` - Now reads from configuration
- ✅ `.gitignore` - Added appsettings.json to prevent future leaks
- ✅ `appsettings.example.json` - Added WebhookSecret placeholder
- ✅ `PaymentServiceTests.cs` - Updated to mock configuration

## Verification
Run tests to ensure everything works:
```powershell
dotnet test
```

All 82 tests should pass ✅

## Never Commit These Files
- ❌ `appsettings.json`
- ❌ `appsettings.Development.json`
- ❌ `appsettings.Production.json`
- ❌ `.env` files

## Questions?
If you need help configuring secrets in production, refer to your hosting provider's documentation on secret management.
