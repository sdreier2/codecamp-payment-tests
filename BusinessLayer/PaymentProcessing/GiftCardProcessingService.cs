using Models;
using System.Threading.Tasks;

namespace BusinessLayer.PaymentProcessing
{
    public interface IGiftCardProcessingService : IExternalPaymentProcessingService
    {

    }

    public class GiftCardProcessingService : IGiftCardProcessingService
    {
        public bool IsAutomatedProcessingEnabled()
        {
            return true;
        }

        public async Task<PaymentTransaction> CaptureTransaction(PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            return paymentTransaction;
        }

        public async Task<PaymentTransaction> RefundTransaction(PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            return paymentTransaction;
        }

        public async Task SendManualProcessingEmail(PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            
        }
    }
}