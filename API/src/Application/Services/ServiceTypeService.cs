using Application;
using Application.DTOs.ServiceTypesDTOs;
using Application.Interfaces;
using AutoMapper;
using Application.DTOs.ServiceTypesDTOs;
using Domain.Entities;
using Domain.Contracts;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class ServiceTypeService(IServiceTypeRepository repository, IMapper mapper) :IServiceTypeService
    {
        public async Task<Result<ServiceTypeDTO>> AddTypeAsync(CreateServiceTypeRequest type, CancellationToken cancellationToken)
        {
            var newType = mapper.Map<ServiceType>(type);
            await repository.AddTypeAsync(newType, cancellationToken);
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(newType));
        }

        public async Task<Result<ServiceTypeDTO>> DeleteTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            var type = await repository.GetServiceTypeByIdAsync(id, cancellationToken);
            if (type == null)
            {
                return Result<ServiceTypeDTO>.NotFound("Service type not found");
            }

            await repository.DeleteTypeAsync(id, cancellationToken);
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(type));

        }

        public async Task<Result<List<ServiceTypeDTO>>> GetAllServiceTypesAsync(CancellationToken cancellationToken)
        {
            var types = await repository.GetAllServiceTypesAsync(cancellationToken);
            return Result<List<ServiceTypeDTO>>.Success(mapper.Map<List<ServiceTypeDTO>>(types));

        }

        public async Task<Result<ServiceTypeDTO>> GetServiceTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var type = await repository.GetServiceTypeByIdAsync(id, cancellationToken);
            if (type == null)
            {
                return Result<ServiceTypeDTO>.NotFound("Service type not found");
            }
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(type));
        }

        public async Task<Result<ServiceTypeDTO>> UpdateTypeAsync(Guid id, UpdateServiceTypeRequest type, CancellationToken cancellationToken)
        {
            var existingType = await repository.GetServiceTypeByIdAsync(id, cancellationToken);
            if (existingType == null)
            {
                return Result<ServiceTypeDTO>.NotFound("Service type not found");
            }

            await repository.UpdateTypeAsync(mapper.Map<ServiceType>(type), cancellationToken);
            return Result<ServiceTypeDTO>.Success(mapper.Map<ServiceTypeDTO>(type));
        }
    }
}
