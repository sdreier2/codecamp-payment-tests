using Models;
using System.Data;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public interface IPaymentTransactionRepository
    {
        IDbTransaction GetOpenDbTransaction();

        Task<PaymentTransaction> InsertPaymentTransaction(PaymentTransaction paymentTransaction,
            IDbTransaction dbTransaction);

        Task<PaymentTransaction> UpdatePaymentTransaction(PaymentTransaction paymentTransaction,
            IDbTransaction dbTransaction);
    }

    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        public IDbTransaction GetOpenDbTransaction()
        {
            return null;
        }

        public async Task<PaymentTransaction> InsertPaymentTransaction(PaymentTransaction paymentTransaction,
            IDbTransaction dbTransaction)
        {
            return await Task.FromResult(paymentTransaction);
        }

        public async Task<PaymentTransaction> UpdatePaymentTransaction(PaymentTransaction paymentTransaction,
            IDbTransaction dbTransaction)
        {
            return await Task.FromResult(paymentTransaction);
        }
    }
}