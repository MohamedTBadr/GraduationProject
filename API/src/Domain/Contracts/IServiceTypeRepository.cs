using Domain.Entities;

namespace Domain.Contracts
{
    public interface IServiceTypeRepository
    {
        Task AddTypeAsync(ServiceType ServiceType, CancellationToken cancellationToken);
        Task DeleteTypeAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateTypeAsync(ServiceType ServiceType, CancellationToken cancellationToken);
        Task<List<ServiceType>> GetAllServiceTypesAsync(CancellationToken cancellationToken);
        Task<ServiceType> GetServiceTypeByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}