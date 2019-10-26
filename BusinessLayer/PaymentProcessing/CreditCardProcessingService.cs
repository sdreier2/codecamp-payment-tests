using Models;
using System.Threading.Tasks;

namespace BusinessLayer.PaymentProcessing
{
    public interface ICreditCardProcessingService : IExternalPaymentProcessingService
    {
        //you may optionally add any credit-card-specific methods here

        //may not be necessary as this service should be referenced as
        //IExternalPaymentProcessingService as returned from the factory
    }

    public class CreditCardProcessingService : ICreditCardProcessingService
    {
        //private readonly ICreditCardService
        //private readonly IHttpClient
        //...


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