using System;

namespace Models
{
    public class PaymentTransaction
    {
        public int PaymentTransactionId { get; set; }
        public int OrderId { get; set; }
        public int TransactionTypeId { get; set; }
        public string AuthorizationNumber { get; set; }
        public string TransactionReference { get; set; }
        public bool IsApproved { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Amount => Subtotal + Tax + Shipping;
        public DateTime? DateCaptureAttempted { get; set; }
        public DateTime? DateCapturedFunds { get; set; }
    }
}