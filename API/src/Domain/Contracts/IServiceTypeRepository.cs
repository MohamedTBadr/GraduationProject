using Domain.Entities;

namespace Domain.Contracts
{
    public interface IServiceTypeRepository
    {
        Task AddTypeAsync(ServiceType ServiceType);
        Task DeleteTypeAsync(Guid id);
        Task UpdateTypeAsync(ServiceType ServiceType);
        Task<List<ServiceType>> GetAllServiceTypesAsync();
        Task<ServiceType> GetServiceTypeByIdAsync(Guid id);
    }
}