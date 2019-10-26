using Models;
using System.Threading.Tasks;

namespace BusinessLayer.External
{
    public interface IExternalPaymentService
    {
        bool IsAutomatedProcessingEnabled(PaymentMethod paymentMethod);

        //optionally add a method to authorize a transaction

        Task<PaymentTransaction> CaptureTransaction(PaymentMethod paymentMethod, 
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);

        Task<PaymentTransaction> RefundTransaction(PaymentMethod paymentMethod, 
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);

        Task SendManualProcessingEmail(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);
    }

    public class ExternalPaymentService : IExternalPaymentService
    {
        private readonly IExternalPaymentFactory _externalPaymentFactory;

        public ExternalPaymentService(IExternalPaymentFactory externalPaymentFactory)
        {
            _externalPaymentFactory = externalPaymentFactory;
        }

        public bool IsAutomatedProcessingEnabled(PaymentMethod paymentMethod)
        {
            return _externalPaymentFactory.GetPaymentService(paymentMethod)
                .IsAutomatedProcessingEnabled();
        }

        public async Task<PaymentTransaction> CaptureTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile)
        {
            return await _externalPaymentFactory.GetPaymentService(paymentMethod)
                .CaptureTransaction(paymentTransaction, paymentProfile);
        }

        public async Task<PaymentTransaction> RefundTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile)
        {
            return await _externalPaymentFactory.GetPaymentService(paymentMethod)
                .RefundTransaction(paymentTransaction, paymentProfile);
        }

        public async Task SendManualProcessingEmail(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile)
        {
            await _externalPaymentFactory.GetPaymentService(paymentMethod)
                .SendManualProcessingEmail(paymentTransaction, paymentProfile);
        }
    }
}