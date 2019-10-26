using Autofac.Extras.Moq;
using BusinessLayer.External;
using Models;
using Moq;
using System.Threading.Tasks;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests
{
    public class ExternalPaymentServiceTests
    {
        private PaymentTransaction GetPaymentTransaction()
        {
            return new PaymentTransaction
            {
                PaymentTransactionId = 12,
                IsApproved = false,
                AuthorizationNumber = "AUTH123",
                TransactionReference = "REF456",
                TransactionTypeId = 1,
                DateCaptureAttempted = null,
                DateCapturedFunds = null,
                OrderId = 500,
                Shipping = 5m,
                Tax = 2m,
                Subtotal = 10m
            };
        }

        private PaymentProfile GetPaymentProfile()
        {
            return new PaymentProfile
            {
                AccountBillingProfileId = 60,
                AccountId = 34
            };
        }

        [Fact]
        public void ItWillCheckIsAutomatedProcessingEnabled()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var paymentMethod = PaymentMethod.CreditCard;
                var mockExternalPaymentProcessingService = new MockExternalPaymentProcessingService();

                mock.Mock<IExternalPaymentFactory>().Setup(x => x.GetPaymentService(It.IsAny<PaymentMethod>()))
                    .Returns(mockExternalPaymentProcessingService);

                var externalPaymentService = mock.Create<ExternalPaymentService>();
                bool result = externalPaymentService.IsAutomatedProcessingEnabled(paymentMethod);

                Assert.True(result);
                mockExternalPaymentProcessingService.VerifyIsAutomatedProcessingEnabledCalled();

                mock.Mock<IExternalPaymentFactory>().Verify(x =>
                    x.GetPaymentService(It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);
                //mock.Mock<IExternalPaymentFactory>().Verify(x =>
                //    x.GetPaymentService(It.IsAny<PaymentMethod>()), Times.Once);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ItWillCaptureTransaction(bool captureSuccess)
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                var paymentProfile = GetPaymentProfile();
                var mockExternalPaymentProcessingService = new MockExternalPaymentProcessingService
                {
                    TestCallResponseType = captureSuccess
                        ? TestCallResponseType.Succcess
                        : TestCallResponseType.Failure
                };

                mock.Mock<IExternalPaymentFactory>().Setup(x => x.GetPaymentService(It.IsAny<PaymentMethod>()))
                    .Returns(mockExternalPaymentProcessingService);

                var externalPaymentService = mock.Create<ExternalPaymentService>();
                var result = await externalPaymentService.CaptureTransaction(paymentMethod, paymentTransaction, paymentProfile);

                Assert.NotNull(result);
                Assert.Equal(captureSuccess, result.IsApproved);

                mockExternalPaymentProcessingService.VerifyCaptureTransactionCalls(paymentTransaction, paymentProfile);

                mock.Mock<IExternalPaymentFactory>().Verify(x =>
                    x.GetPaymentService(It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);
            }
        }

        public async Task ItWillRefundTransaction()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                var paymentProfile = GetPaymentProfile();
                var mockExternalPaymentProcessingService = new MockExternalPaymentProcessingService();

                mock.Mock<IExternalPaymentFactory>().Setup(x => x.GetPaymentService(It.IsAny<PaymentMethod>()))
                    .Returns(mockExternalPaymentProcessingService);

                var externalPaymentService = mock.Create<ExternalPaymentService>();
                var result = await externalPaymentService.RefundTransaction(paymentMethod, paymentTransaction, paymentProfile);

                Assert.NotNull(result);
                Assert.True(result.IsApproved);

                mockExternalPaymentProcessingService.VerifyRefundTransactionCalls(paymentTransaction, paymentProfile);

                mock.Mock<IExternalPaymentFactory>().Verify(x =>
                    x.GetPaymentService(It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);
            }
        }

        [Fact]
        public async Task ItWillSendManualProcessingEmail()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                var paymentProfile = GetPaymentProfile();
                var mockExternalPaymentProcessingService = new MockExternalPaymentProcessingService();

                mock.Mock<IExternalPaymentFactory>().Setup(x => x.GetPaymentService(It.IsAny<PaymentMethod>()))
                    .Returns(mockExternalPaymentProcessingService);

                var externalPaymentService = mock.Create<ExternalPaymentService>();
                await externalPaymentService.SendManualProcessingEmail(paymentMethod, paymentTransaction, paymentProfile);

                mockExternalPaymentProcessingService.VerifySendManualProcessingEmailCalls(paymentTransaction, paymentProfile);

                mock.Mock<IExternalPaymentFactory>().Verify(x =>
                    x.GetPaymentService(It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);
            }
        }
    }
}