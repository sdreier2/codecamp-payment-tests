namespace Models
{
    public class PaymentProfile
    {
        public int AccountId { get; set; }

        //one or more identifiers linking this account to internal/external payment profiles or "cards"
        public int AccountBillingProfileId { get; set; }
    }
}