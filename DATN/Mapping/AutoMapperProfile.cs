using AutoMapper;
using DATN.Dtos.CartDto;
using DATN.Dtos.CategoriesDto;
using DATN.Dtos.OrderDto;
using DATN.Dtos.ProductDto;
using DATN.Entities;

namespace DATN.Mapping
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Product, ProductDto>()
               .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.NameCategory))
               .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages));

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateProductDto, Product>();

            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<Cart, CartDto>()
               .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameProduct))
               .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));
            CreateMap<Order, OrderItemDto>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
           .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.OrderCode));

            CreateMap<Order, OrderFullDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.OrderCode));

            CreateMap<OrderDetail, OrderDetailItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameProduct));
        }   
    }
}
