using AutoMapper;
using BLL.DTOs.ServiceTypesDTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class ServiceTypeService(IServiceTypeRepository repository, IMapper mapper) :IServiceTypeService
    {
        public async Task<Result<ServiceTypeDTO>> AddTypeAsync(CreateServiceTypeRequest type)
        {
            var newType = mapper.Map<ServiceType>(type);
            await repository.AddTypeAsync(newType);
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(newType));
        }

        public async Task<Result<ServiceTypeDTO>> DeleteTypeAsync(Guid id)
        {
            var type = await repository.GetServiceTypeByIdAsync(id);
            if (type == null)
            {
                return Result<ServiceTypeDTO>.Failure(ErrorType.NotFound);
            }

            await repository.DeleteTypeAsync(id);
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(type));

        }

        public async Task<Result<List<ServiceTypeDTO>>> GetAllServiceTypesAsync()
        {
            var types = await repository.GetAllServiceTypesAsync();
            return Result<List<ServiceTypeDTO>>.Success(mapper.Map<List<ServiceTypeDTO>>(types));

        }

        public async Task<Result<ServiceTypeDTO>> GetServiceTypeByIdAsync(Guid id)
        {
            var type = repository.GetServiceTypeByIdAsync(id);
            if (type == null)
            {
                return Result<ServiceTypeDTO>.Failure(ErrorType.NotFound);
            }
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(type));
        }

        public async Task<Result<ServiceTypeDTO>> UpdateTypeAsync(Guid id, UpdateServiceTypeRequest type)
        {
            var existingType = repository.GetServiceTypeByIdAsync(id);
            if (existingType == null)
            {
                return Result<ServiceTypeDTO>.Failure(ErrorType.NotFound);
            }

            await repository.UpdateTypeAsync(mapper.Map<ServiceType>(type));
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(type));
        }
    }
}
