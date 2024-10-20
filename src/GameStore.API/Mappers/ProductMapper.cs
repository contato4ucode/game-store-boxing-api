using AutoMapper;
using GameStore.Domain.Common;
using GameStore.Domain.DTOs;
using GameStore.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace GameStore.API.Mappers;

[ExcludeFromCodeCoverage]
public class ProductMapper : Profile
{
    public ProductMapper()
    {
        CreateMap<Product, ProductDTO>().ReverseMap();

        CreateMap<PaginatedResponse<Product>, PaginatedResponse<ProductDTO>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));
    }
}
