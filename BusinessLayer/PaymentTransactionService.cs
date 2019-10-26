using BusinessLayer.External;
using DataAccessLayer;
using Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public interface IPaymentTransactionService
    {
        Task<PaymentTransaction> CaptureTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);

        Task<PaymentTransaction> RefundTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile);
    }

    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IExternalPaymentService _externalPaymentService;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;

        public PaymentTransactionService(IExternalPaymentService externalPaymentService,
            IPaymentTransactionRepository paymentTransactionRepository)
        {
            _externalPaymentService = externalPaymentService;
            _paymentTransactionRepository = paymentTransactionRepository;
        }

        public async Task<PaymentTransaction> CaptureTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile)
        {
            return await ProcessPaymentTransaction(paymentMethod, paymentTransaction, paymentProfile,
                _externalPaymentService.CaptureTransaction);
        }

        public async Task<PaymentTransaction> RefundTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile)
        {
            return await ProcessPaymentTransaction(paymentMethod, paymentTransaction, paymentProfile,
                _externalPaymentService.RefundTransaction);
        }

        private async Task<PaymentTransaction> ProcessPaymentTransaction(PaymentMethod paymentMethod,
            PaymentTransaction paymentTransaction, PaymentProfile paymentProfile,
            Func<PaymentMethod, PaymentTransaction, PaymentProfile, Task<PaymentTransaction>> externalTransactionDelegate)
        {
            DateTime now = DateTime.Now;
            bool automatedProcessingEnabled = _externalPaymentService.IsAutomatedProcessingEnabled(paymentMethod);

            using (IDbTransaction dbTransaction = _paymentTransactionRepository.GetOpenDbTransaction())
            {
                if (paymentTransaction.PaymentTransactionId < 1)
                {
                    paymentTransaction = await _paymentTransactionRepository.InsertPaymentTransaction(
                        paymentTransaction, dbTransaction);
                }

                try
                {
                    if (automatedProcessingEnabled)
                    {
                        paymentTransaction = await externalTransactionDelegate(paymentMethod,
                            paymentTransaction, paymentProfile);
                    }
                    else
                    {
                        paymentTransaction.IsApproved = false;
                    }
                }
                catch (Exception e)
                {
                    paymentTransaction.IsApproved = false;
                    //possibly log something
                }
                finally
                {
                    paymentTransaction.DateCaptureAttempted = now;
                }

                if (automatedProcessingEnabled && paymentTransaction.IsApproved)
                {
                    paymentTransaction.DateCapturedFunds = now;
                }
                await _paymentTransactionRepository.UpdatePaymentTransaction(paymentTransaction, dbTransaction);

                dbTransaction.Commit();
            }

            if (!automatedProcessingEnabled)
            {
                await _externalPaymentService.SendManualProcessingEmail(paymentMethod, paymentTransaction, paymentProfile);
            }

            return paymentTransaction;
        }
    }
}