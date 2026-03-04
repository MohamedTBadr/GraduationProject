using AutoMapper;
using BLL.DTOs.CategoryDTOs;
using BLL.DTOs.ProductDTOs;
using BLL.DTOs.ServiceTypesDTOs;
using BLL.DTOs.UserDTOs;
using BLL.DTOs.VendorDTOs;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Helpers
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

            #endregion
        }
    }
}
