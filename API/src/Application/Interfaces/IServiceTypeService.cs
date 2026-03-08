using BLL.DTOs.ServiceTypesDTOs;

namespace Application.Interfaces
{
    public interface IServiceTypeService
    {
        Task<Result<ServiceTypeDTO>> AddTypeAsync(CreateServiceTypeRequest type);
        Task<Result<ServiceTypeDTO>> DeleteTypeAsync(Guid id);
        Task<Result<List<ServiceTypeDTO>>> GetAllServiceTypesAsync();
        Task<Result<ServiceTypeDTO>> GetServiceTypeByIdAsync(Guid id);
        Task<Result<ServiceTypeDTO>> UpdateTypeAsync(Guid id, UpdateServiceTypeRequest type);
    }
}