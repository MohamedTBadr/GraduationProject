using Application.DTOs;
using Application.DTOs.PackageDTOs;
using Application.DTOs.ServiceDTOs;
using Application.DTOs.VendorDTOs;
using Domain.Entities;

namespace Application.Services.ManualMapper
{
    public static class PackageMapper
    {
        public static PackageDTO MapToDTO(this Package entity)
        {
            if (entity == null)
                return null;

            var serviceDTOs = entity.Services?
                .Select(service => new ServiceDTO
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,

                    VendorId = service.VendorId,
                    VendorName = service.Vendor?.BusinessName,

                    ServiceTypeId = service.ServiceTypeId,
                    ServiceTypeName = service.ServiceType?.Name,

                    SetupDuration = service.SetupDuration,
                    LeadTimeRequired = service.LeadTimeRequired,

                   

                    ServiceAreas = service.Vendor?.ServiceAreas?
                        .Select(a => new ServiceAreaDTO
                        {
                            Id = a.Id,
                            City = a.City,
                            Region = a.Region
                        })
                        .ToList() ?? new List<ServiceAreaDTO>()
                })
                .ToList();

            return new PackageDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Discount = entity.Discount,


                Services = serviceDTOs ?? new List<ServiceDTO>(),

                VendorId = entity.VendorId,
            };
        }
    }
}