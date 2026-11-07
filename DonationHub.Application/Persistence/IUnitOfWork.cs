using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application.Persistence
{
    public interface IUnitOfWork
    {
        Task<int> CompleteAsync();
        Task ExecuteInTransactionAsync(Func<Task> operation);

    }
}
