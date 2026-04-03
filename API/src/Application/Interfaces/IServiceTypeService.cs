using Application.DTOs.ServiceTypesDTOs;

namespace Application.Interfaces
{
    public interface IServiceTypeService
    {
        Task<Result<ServiceTypeDTO>> AddTypeAsync(CreateServiceTypeRequest type, CancellationToken cancellationToken);
        Task<Result<ServiceTypeDTO>> DeleteTypeAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<List<ServiceTypeDTO>>> GetAllServiceTypesAsync(CancellationToken cancellationToken);
        Task<Result<ServiceTypeDTO>> GetServiceTypeByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<ServiceTypeDTO>> UpdateTypeAsync(Guid id, UpdateServiceTypeRequest type, CancellationToken cancellationToken);
    }
}