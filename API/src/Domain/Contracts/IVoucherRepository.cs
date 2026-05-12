using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IVoucherRepository
    {
        Task<Voucher?> GetByCodeAsync(string code, CancellationToken ct);
        Task<IEnumerable<Voucher>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct);
        Task<bool> ExistsForReferrerAsync(Guid ownerId, string referralCode, CancellationToken ct);
        Task AddAsync(Voucher voucher, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
