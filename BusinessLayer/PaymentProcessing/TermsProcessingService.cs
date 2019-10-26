using Models;
using System.Threading.Tasks;

namespace BusinessLayer.PaymentProcessing
{
    public interface ITermsProcessingService : IExternalPaymentProcessingService
    {

    }

    public class TermsProcessingService : ITermsProcessingService
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