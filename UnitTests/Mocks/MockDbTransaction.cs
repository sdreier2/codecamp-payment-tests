using System;
using System.Data;
using Xunit;

namespace UnitTests.Mocks
{
    public class MockDbTransaction : IDbTransaction
    {
        public bool WasCommited { get; private set; }
        public bool WasRolledBack { get; private set; }

        public void Dispose()
        {
            if (!WasCommited && !WasRolledBack)
            {
                Rollback();
            }
        }

        public void Commit()
        {
            if (WasRolledBack)
            {
                throw new InvalidOperationException("Transaction was already rolled back");
            }

            if (WasCommited)
            {
                throw new InvalidOperationException("Transaction was already committed");
            }

            WasCommited = true;
        }

        public void Rollback()
        {
            if (WasRolledBack)
            {
                throw new InvalidOperationException("Transaction was already rolled back");
            }

            if (WasCommited)
            {
                throw new InvalidOperationException("Transaction was already committed");
            }

            WasRolledBack = true;
        }

        /// <summary>
        /// Verifies the transaction was committed
        /// </summary>
        public void VerifyCommit()
        {
            Assert.True(WasCommited);
        }

        /// <summary>
        /// Verifies the transaction was rolled back
        /// </summary>
        public void VerifyRollBack()
        {
            Assert.True(WasRolledBack);
        }

        public IDbConnection Connection { get; set; }
        public IsolationLevel IsolationLevel { get; set; }
    }
}