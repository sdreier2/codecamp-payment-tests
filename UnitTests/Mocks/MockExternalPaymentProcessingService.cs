using System;
using BusinessLayer.PaymentProcessing;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Mocks
{
    public class MockExternalPaymentProcessingService : IExternalPaymentProcessingService
    {
        private readonly IDictionary<string, List<object>> _calls = new Dictionary<string, List<object>>();
        public TestCallResponseType TestCallResponseType { get; set; } = TestCallResponseType.Succcess;

        public bool IsAutomatedProcessingEnabled()
        {
            _calls.Add(nameof(IsAutomatedProcessingEnabled), new List<object>());
            switch (TestCallResponseType)
            {
                case TestCallResponseType.Succcess:
                    return true;
                case TestCallResponseType.Failure:
                    return false;
                default:
                    throw new Exception();
            }
        }

        public void VerifyIsAutomatedProcessingEnabledCalled(int numberOfCalls = 1)
        {
            Assert.Equal(numberOfCalls, _calls.Count(c => c.Key == nameof(IsAutomatedProcessingEnabled)));
        }

        public async Task<PaymentTransaction> CaptureTransaction(PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            RegisterPaymentTransactionCall(nameof(CaptureTransaction), paymentTransaction, paymentProfile);
            switch (TestCallResponseType)
            {
                case TestCallResponseType.Succcess:
                    paymentTransaction.IsApproved = true;
                    break;
                case TestCallResponseType.Failure:
                    paymentTransaction.IsApproved = false;
                    break;
                default:
                    throw new Exception();
            }
            return paymentTransaction;
        }

        public void VerifyCaptureTransactionCalls(PaymentTransaction verificationPaymentTransaction,
            PaymentProfile verificationPaymentProfile, int numberOfCalls = 1)
        {
            VerifyPaymentTransactionCall(nameof(CaptureTransaction), verificationPaymentTransaction,
                verificationPaymentProfile, numberOfCalls);
        }

        public async Task<PaymentTransaction> RefundTransaction(PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            RegisterPaymentTransactionCall(nameof(RefundTransaction), paymentTransaction, paymentProfile);
            switch (TestCallResponseType)
            {
                case TestCallResponseType.Succcess:
                    paymentTransaction.IsApproved = true;
                    break;
                case TestCallResponseType.Failure:
                    paymentTransaction.IsApproved = false;
                    break;
                default:
                    throw new Exception();
            }
            return paymentTransaction;
        }

        public void VerifyRefundTransactionCalls(PaymentTransaction verificationPaymentTransaction,
            PaymentProfile verificationPaymentProfile, int numberOfCalls = 1)
        {
            VerifyPaymentTransactionCall(nameof(RefundTransaction), verificationPaymentTransaction,
                verificationPaymentProfile, numberOfCalls);
        }

        public async Task SendManualProcessingEmail(PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            RegisterPaymentTransactionCall(nameof(SendManualProcessingEmail), paymentTransaction, paymentProfile);
            if (TestCallResponseType == TestCallResponseType.Exception)
            {
                throw new Exception();
            }
        }

        public void VerifySendManualProcessingEmailCalls(PaymentTransaction verificationPaymentTransaction,
            PaymentProfile verificationPaymentProfile, int numberOfCalls = 1)
        {
            VerifyPaymentTransactionCall(nameof(SendManualProcessingEmail), verificationPaymentTransaction,
                verificationPaymentProfile, numberOfCalls);
        }

        private void RegisterPaymentTransactionCall(string methodName, PaymentTransaction paymentTransaction,
            PaymentProfile paymentProfile)
        {
            _calls.Add(methodName, new List<object>
            {
                paymentTransaction, paymentProfile
            });
        }

        private void VerifyPaymentTransactionCall(string methodName, PaymentTransaction verificationPaymentTransaction,
            PaymentProfile verificationPaymentProfile, int numberOfCalls)
        {
            //for simplicity, only check some fields on these objects
            Assert.Equal(numberOfCalls,
                _calls.Count(c => c.Key == methodName
                   && c.Value.Any(v => v is PaymentTransaction pt 
                                       && pt.PaymentTransactionId == verificationPaymentTransaction.PaymentTransactionId
                                       && pt.AuthorizationNumber == verificationPaymentTransaction.AuthorizationNumber)
                   && c.Value.Any(v => v is PaymentProfile pp && pp.AccountId == verificationPaymentProfile.AccountId)));
        }
    }
}