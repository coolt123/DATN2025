using AutoMapper;
using DATN.Dtos.CartDto;
using DATN.Dtos.CategoriesDto;
using DATN.Dtos.MaterialDto;
using DATN.Dtos.OrderDto;
using DATN.Dtos.ProductDto;
using DATN.Dtos.ReviewDto;
using DATN.Dtos.StyleDto;
using DATN.Entities;
using System.Management;

namespace DATN.Mapping
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(a=> a.Createdat , a => a.MapFrom(_ => DateTime.UtcNow));
            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(a => a.Createdat, a => a.MapFrom(a => a.UpdateAt));
            CreateMap<Product, ProductDto>()
               .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.NameCategory))
               .ForMember(dest => dest.Style, opt => opt.MapFrom(src => src.Style.Name))
               .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Material.Name))
               .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.ProductImages))
               .ForMember(dest=>dest.DescriptionDetails , opt=>opt.MapFrom(src=>src.DescriptionDetails))
               .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.SalePrice))
               .ForMember(dest => dest.PromotionStart, opt => opt.MapFrom(src => src.PromotionStart))
               .ForMember(dest => dest.PromotionEnd, opt => opt.MapFrom(src => src.PromotionEnd))
               .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.CurrentPrice))
               .ForMember(dest=>dest.CreatedDate,opt=>opt.MapFrom(src=>src.CreatedDate))
               .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src => src.IsOnSale))
               .ForMember(dest => dest.RemainingQuantity, opt => opt.MapFrom(src =>
                    src.StockQuantity -
                    (src.OrderDetails
                        .Where(od => od.Order.Status == "Complete")
                        .Sum(od => (int?)od.Quantity) ?? 0)
                ))
                .ForMember(dest => dest.SoldQuantity, opt => opt.MapFrom(src =>
                    src.OrderDetails
                       .Where(od => od.Order.Status == "Complete") 
                       .Sum(od => (int?)od.Quantity) ?? 0
                ));
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Createdat, opt => opt.MapFrom(_ => DateTime.UtcNow));
            
            CreateMap<UpdateProductWithFilesDto, Product>()
                .ForMember(dest => dest.Updatedat, opt => opt.MapFrom(_ => DateTime.UtcNow));
            CreateMap<CreateProductWithFilesDto, Product>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));


            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<ProductDescriptionDetail, ProductDescriptionDetailDto>();
            CreateMap<Cart, CartDto>()
               .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameProduct))
               .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Product.CurrentPrice))
                .ForMember(dest => dest.Saleprice, opt => opt.MapFrom(src => src.Product.SalePrice))
                .ForMember(Dest=>Dest.Price,opt=>opt.MapFrom(src=>src.Product.Price));
            CreateMap<Order, OrderItemDto>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
           .ForMember(o=>o.phonenumber,opt=>opt.MapFrom(src=>src.User.PhoneNumber))
           .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.OrderCode))
           .ForMember(a=>a.shippingAddress,o=>o.MapFrom(src=>src.ShippingAddress))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(src => src.Payments.FirstOrDefault() != null ? src.Payments.First().PaymentStatus : null))
            .ForMember(d=>d.PaymentMethod,p => p.MapFrom(src => src.Payments.FirstOrDefault() != null ? src.Payments.First().PaymentMethod : null));
            CreateMap<Order,OrderDetailUser>()
                .ForMember(d=>d.OrderCode,o=>o.MapFrom(src=>src.OrderCode))
                .ForMember(d=>d.userName,o=>o.MapFrom(src=>src.User.FullName))
                .ForMember(d=>d.phonenumber,o=>o.MapFrom(src=>src.User.PhoneNumber))
                .ForMember(d => d.PaymentStatus, o => o.MapFrom(src => src.Payments.FirstOrDefault() != null ? src.Payments.First().PaymentStatus : null));
            CreateMap<Order, OrderFullDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.OrderCode))
                 .ForMember(o => o.phonenumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(d=>d.ShippingAddress, o=>o.MapFrom(src=>src.ShippingAddress))
                .ForMember(d => d.paymentStatus, o => o.MapFrom(src => src.Payments.FirstOrDefault() != null ? src.Payments.First().PaymentStatus : null));
            CreateMap<OrderDetail, OrderDetailItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameProduct));
            CreateMap<Material, MaterialDto>().ReverseMap();
            CreateMap<CreateMaterialDto, Material>()
                 .ForMember(a => a.Createdat, a => a.MapFrom(_ => DateTime.UtcNow)).ReverseMap(); 
            CreateMap<UpdateMaterialDto, Material>()
                .ForMember(a => a.Updatedat, a => a.MapFrom(_ => DateTime.UtcNow)).ReverseMap(); 

            CreateMap<Style, StyleDto>().ReverseMap();
            CreateMap<CreateStyleDto, Style>()
                .ForMember(a => a.Createdat, a => a.MapFrom(_ => DateTime.UtcNow)).ReverseMap(); 
            CreateMap<UpdateStyleDto, Style>()
                .ForMember(a => a.Updatedat, a => a.MapFrom(_ => DateTime.UtcNow)).ReverseMap();
            CreateMap<Review, ReviewDto>()
               .ForMember(dest => dest.UserName,
                          opt => opt.MapFrom(src => src.User.FullName))
               .ForMember(dest => dest.ProductName,
                          opt => opt.MapFrom(src => src.Product.NameProduct));
            CreateMap<ReviewCreateDto, Review>()
                .ForMember(a=>a.Createdat,a=>a.MapFrom(_=> DateTime.UtcNow)).ReverseMap();

        }   
    }
}
