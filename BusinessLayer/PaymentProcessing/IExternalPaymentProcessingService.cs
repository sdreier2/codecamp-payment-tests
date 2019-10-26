using Models;
using System.Threading.Tasks;

namespace BusinessLayer.PaymentProcessing
{
    public interface IExternalPaymentProcessingService
    {
        bool IsAutomatedProcessingEnabled();

        Task<PaymentTransaction> CaptureTransaction(PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);

        Task<PaymentTransaction> RefundTransaction(PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);

        Task SendManualProcessingEmail(PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);
    }
}