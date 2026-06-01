using Application.DTOs.PackageDTOs;
using Application.Interfaces;
using Application.Services.ManualMapper;
using Domain.Contracts;
using Domain.Entities;
using Shared;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PackageService(IPackageRepository packageRepository) : IPackageService
    {
        public async Task<Result<PaginatedResponse<PackageDTO>>> GetAllAsync(PaginatedRequest request, bool isAdmin, bool isVendor, Guid? userId, CancellationToken cancellationToken)
        {
            Expression<Func<Package, bool>> visibilityFilter = p => true;

            if (isVendor && !isAdmin && userId.HasValue)
            {
                visibilityFilter = p => p.VendorId == userId.Value;
            }

            var packages = await packageRepository.GetAllAsync(request, visibilityFilter, cancellationToken);
            
            var dtos = packages.Items.Select(p => p.MapToDTO()).ToList();
            
            var response = new PaginatedResponse<PackageDTO>(
                dtos,
                packages.TotalCount,
                packages.PageNumber,
                packages.PageSize
            );

            return Result<PaginatedResponse<PackageDTO>>.Success(response);
        }

        public async Task<Result<PackageDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var package = await packageRepository.GetByIdAsync(id, cancellationToken);
            if (package == null)
            {
                return Result<PackageDTO>.NotFound(404, "Package not found.");
            }

            return Result<PackageDTO>.Success(package.MapToDTO());
        }

        public async Task<Result<PackageDTO>> CreateAsync(CreatePackageDTO dto, CancellationToken cancellationToken)
        {
            var package = new Package
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Discount = dto.Discount,
                Items = dto.Items,
                VendorId = dto.VendorId
            };

            var createdPackage = await packageRepository.CreateAsync(package, cancellationToken);

            return Result<PackageDTO>.Success(createdPackage.MapToDTO());
        }

        public async Task<Result<PackageDTO>> UpdateAsync(UpdatePackageDTO dto, CancellationToken cancellationToken)
        {
            var package = await packageRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (package == null)
            {
                return Result<PackageDTO>.NotFound(404, "Package not found.");
            }

            package.Name = dto.Name;
            package.Description = dto.Description;
            package.Price = dto.Price;
            package.Discount = dto.Discount;
            package.Items = dto.Items;
            package.VendorId = dto.VendorId;

            var updatedPackage = await packageRepository.UpdateAsync(package, cancellationToken);

            return Result<PackageDTO>.Success(updatedPackage.MapToDTO());
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var package = await packageRepository.GetByIdAsync(id, cancellationToken);
            if (package == null)
            {
                return Result<bool>.Failure(Error.NotFound(404, "Package not found."));
            }

            await packageRepository.DeleteAsync(id, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
