using Application.DTOs;
using Application.DTOs.PackageDTOs;
using Domain.Entities;

namespace Application.Services.ManualMapper
{
    public static class PackageMapper
    {
        public static PackageDTO MapToDTO(this Package entity)
        {
            if (entity == null) return null;
            return new PackageDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Discount = entity.Discount,
                Items = entity.Items,
                VendorId = entity.VendorId,
            };
        }
    }
}