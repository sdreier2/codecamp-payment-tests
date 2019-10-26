using Autofac.Extras.Moq;
using BusinessLayer.External;
using BusinessLayer.PaymentProcessing;
using Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class ExternalPaymentFactoryTests
    {
        public static IEnumerable<object[]> GetExternalPaymentFactoryTestData()
        {
            yield return new object[] { PaymentMethod.CreditCard, typeof(ICreditCardProcessingService) };
            yield return new object[] { PaymentMethod.GiftCard, typeof(IGiftCardProcessingService) };
            yield return new object[] { PaymentMethod.Net30Terms, typeof(ITermsProcessingService) };
        }

        [Theory]
        [MemberData(nameof(GetExternalPaymentFactoryTestData))]
        public void ItWillGetPaymentService(PaymentMethod paymentMethod, Type expectedType)
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var externalPaymentFactory = mock.Create<ExternalPaymentFactory>();
                var result = externalPaymentFactory.GetPaymentService(paymentMethod);

                Assert.NotNull(result);
                Assert.IsAssignableFrom(expectedType, result);
            }
        }

        [Fact]
        public void ItWillThrowExceptionOnUnhandledPaymentMethod()
        {
            using (AutoMock mock = AutoMock.GetLoose())
            {
                var paymentMethod = PaymentMethod.Paypal;
                var externalPaymentFactory = mock.Create<ExternalPaymentFactory>();

                var ex = Assert.Throws<ArgumentException>(() => externalPaymentFactory.GetPaymentService(paymentMethod));

                Assert.Equal($"Invalid payment type {paymentMethod.ToString()}", ex.Message);
            }
        }
    }
}