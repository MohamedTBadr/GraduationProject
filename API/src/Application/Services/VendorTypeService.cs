using Application.DTOs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class VendorTypeService(IVendorTypeRepository repository) : IVendorTypeService
    {
        public async Task<Result<VendorTypeDetailsDTO>> AddVendorTypeAsync(CreateOrUpdateVendorTypeRequest request, CancellationToken cancellationToken)
        {
            var vendorType= new VendorType
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };
            await repository.AddVendorTypeAsync(vendorType, cancellationToken);
            return Result<VendorTypeDetailsDTO>.Success(new VendorTypeDetailsDTO
            {
                Id = vendorType.Id,
                Name = vendorType.Name
            });
        }

      

        public async Task<Result<bool>> DeleteVendorTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            await repository.DeleteVendorTypeAsync(id,cancellationToken);
            return Result<bool>.Success(true);
        }

        public async Task<Result<VendorTypeDetailsDTO>> GetVendorTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
               var vendorType = await repository.GetVendorTypeByIdAsync(id, cancellationToken);
               if (vendorType == null)
               {
                   return Result<VendorTypeDetailsDTO>.NotFound(404,"Vendor type not found");
               }
               return Result<VendorTypeDetailsDTO>.Success(new VendorTypeDetailsDTO
               {
                   Id = vendorType.Id,
                   Name = vendorType.Name
               });
        }

        public async Task<Result<IReadOnlyList<VendorTypeDetailsDTO>>> GetVendorTypesAsync(CancellationToken cancellationToken)
        {
            var vendorTypes = await repository.GetVendorTypesAsync(cancellationToken);
            var vendorTypeDTOs = vendorTypes.Select(v => new VendorTypeDetailsDTO
            {
                Id = v.Id,
                Name = v.Name
            }).ToList();
            return Result<IReadOnlyList<VendorTypeDetailsDTO>>.Success(vendorTypeDTOs);
        }

        public async Task<Result<VendorTypeDetailsDTO>> UpdateVendorTypeAsync(Guid id, CreateOrUpdateVendorTypeRequest request, CancellationToken cancellationToken)
        {
            var vendorType = await repository.GetVendorTypeByIdAsync(id, cancellationToken);
            if (vendorType == null)
            {
                return Result<VendorTypeDetailsDTO>.NotFound(404, "Vendor type not found");
            }

            vendorType.Name = request.Name;
            await repository.UpdateVendorTypeAsync(vendorType, cancellationToken);

            return Result<VendorTypeDetailsDTO>.Success(new VendorTypeDetailsDTO
            {
                Id = vendorType.Id,
                Name = vendorType.Name
            });
        }
    }
}
