using Autofac.Extras.Moq;
using BusinessLayer;
using BusinessLayer.External;
using DataAccessLayer;
using Models;
using Moq;
using System;
using System.Data;
using System.Threading.Tasks;
using UnitTests.Mocks;
using Xunit;

namespace UnitTests
{
    public class PaymentTransactionServiceTests
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

        private readonly string _dateComparison;

        public PaymentTransactionServiceTests()
        {
            _dateComparison = DateTime.Today.ToShortDateString();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ItWillProcessCaptureWhenAutomatedProcessingEnabled(bool transactionApproved)
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var mockDbTransaction = new MockDbTransaction();
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                var paymentProfile = GetPaymentProfile();

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.GetOpenDbTransaction())
                    .Returns(mockDbTransaction);

                mock.Mock<IExternalPaymentService>()
                    .Setup(x => x.IsAutomatedProcessingEnabled(It.IsAny<PaymentMethod>()))
                    .Returns(true);

                mock.Mock<IExternalPaymentService>().Setup(x => x.CaptureTransaction(It.IsAny<PaymentMethod>(),
                        It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()))
                    .ReturnsAsync((PaymentMethod pm, PaymentTransaction pt, PaymentProfile pp) =>
                    {
                        pt.IsApproved = transactionApproved;
                        return pt;
                    });

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.UpdatePaymentTransaction(
                        It.IsAny<PaymentTransaction>(), It.IsAny<IDbTransaction>()))
                    .ReturnsAsync((PaymentTransaction pt, IDbTransaction dbt) => pt);

                var paymentTransactionService = mock.Create<PaymentTransactionService>();
                var result = await paymentTransactionService.CaptureTransaction(paymentMethod,
                    paymentTransaction, paymentProfile);

                Assert.NotNull(result);
                Assert.Equal(transactionApproved, result.IsApproved);
                Assert.Equal(_dateComparison, result.DateCaptureAttempted.Value.ToShortDateString());

                if (transactionApproved)
                {
                    Assert.Equal(_dateComparison, result.DateCapturedFunds.Value.ToShortDateString());
                }
                else
                {
                    Assert.Null(result.DateCapturedFunds);
                }

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.GetOpenDbTransaction(), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.IsAutomatedProcessingEnabled(
                    It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.CaptureTransaction(
                    It.Is<PaymentMethod>(p => p == paymentMethod),
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId),
                    It.Is<PaymentProfile>(pp => pp.AccountId == paymentProfile.AccountId)), Times.Once);

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.UpdatePaymentTransaction(
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId
                                                    && pt.DateCaptureAttempted.Value.ToShortDateString() == _dateComparison
                                                    && pt.IsApproved == transactionApproved
                                                    && (transactionApproved 
                                                        ? paymentTransaction.DateCapturedFunds.Value.ToShortDateString() == _dateComparison
                                                        : paymentTransaction.DateCapturedFunds == null)),
                    It.Is<IDbTransaction>(t => t != null)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.SendManualProcessingEmail(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mock.Mock<IExternalPaymentService>().Verify(x => x.RefundTransaction(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mockDbTransaction.VerifyCommit();
            }
        }

        [Fact]
        public async Task ItWillNotProcessCaptureWhenAutomatedProcessingDisabled()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var mockDbTransaction = new MockDbTransaction();
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                var paymentProfile = GetPaymentProfile();

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.GetOpenDbTransaction())
                    .Returns(mockDbTransaction);

                mock.Mock<IExternalPaymentService>()
                    .Setup(x => x.IsAutomatedProcessingEnabled(It.IsAny<PaymentMethod>()))
                    .Returns(false);

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.UpdatePaymentTransaction(
                        It.IsAny<PaymentTransaction>(), It.IsAny<IDbTransaction>()))
                    .ReturnsAsync((PaymentTransaction pt, IDbTransaction dbt) => pt);

                var paymentTransactionService = mock.Create<PaymentTransactionService>();
                var result = await paymentTransactionService.CaptureTransaction(paymentMethod,
                    paymentTransaction, paymentProfile);

                Assert.NotNull(result);
                Assert.False(result.IsApproved);
                Assert.Equal(_dateComparison, result.DateCaptureAttempted.Value.ToShortDateString());
                Assert.Null(result.DateCapturedFunds);

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.GetOpenDbTransaction(), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.IsAutomatedProcessingEnabled(
                    It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.CaptureTransaction(
                    It.IsAny<PaymentMethod>(), It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.UpdatePaymentTransaction(
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId
                                                    && !pt.IsApproved
                                                    && pt.DateCaptureAttempted.Value.ToShortDateString() == _dateComparison
                                                    && pt.DateCapturedFunds == null),
                    It.Is<IDbTransaction>(t => t != null)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.SendManualProcessingEmail(
                    It.Is<PaymentMethod>(p => p == paymentMethod),
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId),
                    It.Is<PaymentProfile>(pp => pp.AccountId == paymentProfile.AccountId)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.RefundTransaction(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mockDbTransaction.VerifyCommit();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task ItWillProcessCaptureAndOptionallyInsertPaymentTransaction(int initialPaymentTransactionId)
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var mockDbTransaction = new MockDbTransaction();
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                paymentTransaction.PaymentTransactionId = initialPaymentTransactionId;
                var paymentProfile = GetPaymentProfile();

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.GetOpenDbTransaction())
                    .Returns(mockDbTransaction);
                
                mock.Mock<IExternalPaymentService>()
                    .Setup(x => x.IsAutomatedProcessingEnabled(It.IsAny<PaymentMethod>()))
                    .Returns(true);

                mock.Mock<IExternalPaymentService>().Setup(x => x.CaptureTransaction(It.IsAny<PaymentMethod>(),
                        It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()))
                    .ReturnsAsync((PaymentMethod pm, PaymentTransaction pt, PaymentProfile pp) =>
                    {
                        pt.IsApproved = true;
                        return pt;
                    });

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.UpdatePaymentTransaction(
                        It.IsAny<PaymentTransaction>(), It.IsAny<IDbTransaction>()))
                    .ReturnsAsync((PaymentTransaction pt, IDbTransaction dbt) => pt);

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.InsertPaymentTransaction(
                        It.IsAny<PaymentTransaction>(), It.IsAny<IDbTransaction>()))
                    .ReturnsAsync((PaymentTransaction pt, IDbTransaction dbt) => 
                    {
                        pt.PaymentTransactionId++;
                        return pt;
                    });

                var paymentTransactionService = mock.Create<PaymentTransactionService>();
                var result = await paymentTransactionService.CaptureTransaction(paymentMethod,
                    paymentTransaction, paymentProfile);

                Assert.NotNull(result);
                Assert.True(result.PaymentTransactionId > 0);
                Assert.True(result.IsApproved);
                Assert.Equal(_dateComparison, result.DateCaptureAttempted.Value.ToShortDateString());
                Assert.Equal(_dateComparison, result.DateCapturedFunds.Value.ToShortDateString());

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.GetOpenDbTransaction(), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.IsAutomatedProcessingEnabled(
                    It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.CaptureTransaction(
                    It.Is<PaymentMethod>(p => p == paymentMethod),
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId),
                    It.Is<PaymentProfile>(pp => pp.AccountId == paymentProfile.AccountId)), Times.Once);

                if (initialPaymentTransactionId == 0)
                {
                    mock.Mock<IPaymentTransactionRepository>().Verify(x => x.InsertPaymentTransaction(
                        It.Is<PaymentTransaction>(pt => pt.TransactionReference == paymentTransaction.TransactionReference),
                        It.Is<IDbTransaction>(t => t != null)), Times.Once);
                }
                else
                {
                    mock.Mock<IPaymentTransactionRepository>().Verify(x => x.InsertPaymentTransaction(
                        It.IsAny<PaymentTransaction>(),
                        It.IsAny<IDbTransaction>()), Times.Never);
                }

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.UpdatePaymentTransaction(
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId > 0
                                                    && pt.DateCaptureAttempted.Value.ToShortDateString() == _dateComparison
                                                    && pt.IsApproved 
                                                    && paymentTransaction.DateCapturedFunds.Value.ToShortDateString() == _dateComparison),
                    It.Is<IDbTransaction>(t => t != null)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.SendManualProcessingEmail(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mock.Mock<IExternalPaymentService>().Verify(x => x.RefundTransaction(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mockDbTransaction.VerifyCommit();
            }
        }

        [Fact]
        public async Task ItWillHandleExceptionOnCapture()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var mockDbTransaction = new MockDbTransaction();
                var paymentMethod = PaymentMethod.CreditCard;
                var paymentTransaction = GetPaymentTransaction();
                var paymentProfile = GetPaymentProfile();

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.GetOpenDbTransaction())
                    .Returns(mockDbTransaction);

                mock.Mock<IExternalPaymentService>()
                    .Setup(x => x.IsAutomatedProcessingEnabled(It.IsAny<PaymentMethod>()))
                    .Returns(true);

                mock.Mock<IExternalPaymentService>().Setup(x => x.CaptureTransaction(It.IsAny<PaymentMethod>(),
                        It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()))
                    .ThrowsAsync(new Exception());

                mock.Mock<IPaymentTransactionRepository>().Setup(x => x.UpdatePaymentTransaction(
                        It.IsAny<PaymentTransaction>(), It.IsAny<IDbTransaction>()))
                    .ReturnsAsync((PaymentTransaction pt, IDbTransaction dbt) => pt);

                var paymentTransactionService = mock.Create<PaymentTransactionService>();
                var result = await paymentTransactionService.CaptureTransaction(paymentMethod,
                    paymentTransaction, paymentProfile);

                Assert.NotNull(result);
                Assert.False(result.IsApproved);
                Assert.Equal(_dateComparison, result.DateCaptureAttempted.Value.ToShortDateString());
                Assert.Null(result.DateCapturedFunds);

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.GetOpenDbTransaction(), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.IsAutomatedProcessingEnabled(
                    It.Is<PaymentMethod>(p => p == paymentMethod)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.CaptureTransaction(
                    It.Is<PaymentMethod>(p => p == paymentMethod),
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId),
                    It.Is<PaymentProfile>(pp => pp.AccountId == paymentProfile.AccountId)), Times.Once);

                mock.Mock<IPaymentTransactionRepository>().Verify(x => x.UpdatePaymentTransaction(
                    It.Is<PaymentTransaction>(pt => pt.PaymentTransactionId == paymentTransaction.PaymentTransactionId
                                                    && pt.DateCaptureAttempted.Value.ToShortDateString() == _dateComparison
                                                    && !pt.IsApproved
                                                    && paymentTransaction.DateCapturedFunds == null),
                    It.Is<IDbTransaction>(t => t != null)), Times.Once);

                mock.Mock<IExternalPaymentService>().Verify(x => x.SendManualProcessingEmail(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mock.Mock<IExternalPaymentService>().Verify(x => x.RefundTransaction(It.IsAny<PaymentMethod>(),
                    It.IsAny<PaymentTransaction>(), It.IsAny<PaymentProfile>()), Times.Never);

                mockDbTransaction.VerifyCommit();
            }
        }
    }
}