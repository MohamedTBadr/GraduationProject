using AutoMapper;
using BLL.DTOs.CategoryDTOs;
using BLL.DTOs.ProductDTOs;
using BLL.DTOs.ServiceTypesDTOs;
using BLL.DTOs.UserDTOs;
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
            #endregion


            #region Product
            CreateMap<ProductDTO, Product>().ReverseMap();
            CreateMap<CreateProductRequest, Product>().ReverseMap();
            #endregion


            #region User
            //  CreateMap<UserDTO, ApplicationUser>().
            #endregion

            #region ServiceType
            CreateMap<CreateServiceTypeRequest, ServiceType>().ReverseMap();
            CreateMap<ServiceTypeDTO, ServiceType>().ReverseMap();
            #endregion
        }
    }
}
