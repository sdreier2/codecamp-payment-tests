using BusinessLayer.PaymentProcessing;
using Models;
using System;

namespace BusinessLayer.External
{
    public interface IExternalPaymentFactory
    {
        IExternalPaymentProcessingService GetPaymentService(PaymentMethod paymentMethod);
    }

    /// <summary>
    /// Factory class to return the appropriate external payment service based on the payment method
    /// </summary>
    public class ExternalPaymentFactory : IExternalPaymentFactory
    {
        private readonly IExternalPaymentProcessingService _creditCardProcessingService;
        private readonly IExternalPaymentProcessingService _termsProcessingService;
        private readonly IExternalPaymentProcessingService _giftCardProcessingService;

        public ExternalPaymentFactory(ICreditCardProcessingService creditCardProcessingService,
            ITermsProcessingService termsProcessingService,
            IGiftCardProcessingService giftCardProcessingService)
        {
            _creditCardProcessingService = creditCardProcessingService;
            _termsProcessingService = termsProcessingService;
            _giftCardProcessingService = giftCardProcessingService;
        }

        public IExternalPaymentProcessingService GetPaymentService(PaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case PaymentMethod.CreditCard:
                    return _creditCardProcessingService;
                case PaymentMethod.Net30Terms:
                    return _termsProcessingService;
                case PaymentMethod.GiftCard:
                    return _giftCardProcessingService;
                default:
                    throw new ArgumentException($"Invalid payment type {paymentMethod.ToString()}");;
            }
        }
    }
}