using Application.DTOs.CategoryDTOs;
using Application.DTOs.ProductDTOs;
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
            #region Category
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<CategoryDTO, Category>().ReverseMap();
            CreateMap<Category,CategoryDTO>().ReverseMap();
            #endregion





            #region User
            CreateMap<UserDTO, ApplicationUser>();
            CreateMap<CreateUserRequest, ApplicationUser>();
            #endregion

            #region ServiceType
            CreateMap<CreateServiceTypeRequest, ServiceType>().ReverseMap();
            CreateMap<ServiceTypeDTO, ServiceType>().ReverseMap();
            #endregion




            #region Product
            // Entity → Read DTO
            CreateMap<Product, ProductDTO>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
                .ForMember(d => d.VendorName, o => o.MapFrom(s => s.Vendor.BusinessName))
                .ForMember(d => d.ServiceTypeName, o => o.MapFrom(s => s.ServiceType.Name));

            // Create DTO → Entity
            CreateMap<CreateProductRequest, Product>();

            // Update DTO → Entity
            CreateMap<UpdateProductDTO, Product>();
            #endregion

            #region Vendor
            CreateMap<VendorDetailsDTO, Vendor>().ReverseMap();
            CreateMap<CreateVendorRequest, Vendor>().ReverseMap();
            CreateMap<VendorListDTO, Vendor>().ReverseMap();
            //CreateMap<Vendor, VendorListDTO>().ForMember(d => d.UserId, o => o.MapFrom(s => s.User.Id));
            CreateMap<VendorRating, VendorRatingDTO>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor.BusinessName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));
                
            // Remove .ReverseMap() — mapping back from DTO to Entity doesn't make sense here
            #endregion
        }
    }
}

