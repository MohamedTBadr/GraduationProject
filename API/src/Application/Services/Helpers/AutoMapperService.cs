using Application.DTOs;
using Application.DTOs.CategoryDTOs;
using Application.DTOs.Orders;
using Application.DTOs.PackageDTOs;
using Application.DTOs.ServiceDTOs;
using Application.DTOs.ServiceTypesDTOs;
using Application.DTOs.UserDTOs;
using Application.DTOs.VendorDTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Helpers
{
    public class AutoMapperService:Profile
    {
        public AutoMapperService()
        {
 
          

            #region User
            CreateMap<UserDTO, ApplicationUser>();
            CreateMap<CreateUserRequest, ApplicationUser>();
            #endregion

            #region ServiceType
            CreateMap<CreateServiceTypeRequest, ServiceType>().ReverseMap();
            CreateMap<UpdateServiceTypeRequest, ServiceType>().ReverseMap();
            CreateMap<UpdateServiceTypeRequest, ServiceTypeDTO>().ReverseMap();
            CreateMap<ServiceTypeDTO, ServiceType>().ReverseMap();
            #endregion

            #region Service
            // Entity → Read DTO
            CreateMap<Service, ServiceDTO>()
                .ForMember(d => d.VendorName, o => o.MapFrom(s => s.Vendor.BusinessName))
                .ForMember(d=>d.ServiceAreas,o=>o.MapFrom(s=>s.Vendor.ServiceAreas))
                .ForMember(d => d.ServiceTypeName, o => o.MapFrom(s => s.ServiceType.Name))
                .ForMember(d => d.ServiceImages, o => o.MapFrom(s => s.ServiceImages.Select(i => i.ImagePath).ToList()));

            CreateMap<ServiceArea,ServiceAreaDTO>().ReverseMap();
            // Create DTO → Entity
            // In your mapping profile
            CreateMap<CreateServiceRequest, Service>()
                .ForMember(d => d.ServiceImages, o => o.Ignore());
            // Update DTO → Entity
            CreateMap<UpdateServiceDTO, Service>();
            #endregion

            #region Vendor
            CreateMap<Vendor, VendorDetailsDTO>()
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src =>
                    src.Services.SelectMany(s => s.ServiceRatings).Any()
                    ? src.Services.SelectMany(s => s.ServiceRatings).Average(r => r.Rating)
                    : 0)); 
            CreateMap<CreateVendorRequest, Vendor>()
                .ForMember(d => d.ProfilePicture, o => o.Ignore())
                .ForMember(d => d.Document, o => o.Ignore())
                .ForMember(d => d.PortfolioLink, o => o.Ignore())
                .ReverseMap();
            CreateMap<UpdateVendorRequest, Vendor>()
                .ForMember(d => d.ProfilePicture, o => o.Ignore())
                .ForMember(d => d.Document, o => o.Ignore())
                .ForMember(d => d.PortfolioLink, o => o.Ignore());
            CreateMap<Vendor, VendorListDTO>()
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => src.VendorType != null ? src.VendorType.Name : string.Empty))
                .ForMember(dest => dest.VendorType, opt => opt.MapFrom(src => src.VendorType != null ? src.VendorType.Name : string.Empty))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src =>
                    src.Services != null && src.Services.SelectMany(s => s.ServiceRatings).Any()
                    ? src.Services.SelectMany(s => s.ServiceRatings).Average(r => r.Rating)
                    : 0))
                .ForMember(dest => dest.StartingPrice, opt => opt.MapFrom(src =>
                    src.Services != null && src.Services.Any()
                    ? src.Services.Min(s => s.Price)
                    : 0));
            //CreateMap<Vendor, VendorListDTO>().ForMember(d => d.UserId, o => o.MapFrom(s => s.User.Id));
            CreateMap<ServiceRating, VendorRatingDTO>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Service.Vendor.BusinessName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            // Remove .ReverseMap() — mapping back from DTO to Entity doesn't make sense here
            #endregion

            CreateMap<DTOs.AddressDto, Address>();  // or whatever your owned type is called


            CreateMap<Package, PackageDTO>();  // ✅ this was missing

        }
    }
}

