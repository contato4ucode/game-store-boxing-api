using AutoMapper;
using GameStore.API.Contracts.Reponses;
using GameStore.API.Contracts.Requests;
using GameStore.Domain.Common;
using GameStore.Domain.DTOs;
using GameStore.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace GameStore.API.Mappers;

[ExcludeFromCodeCoverage]
public class BoxMapper : Profile
{
    public BoxMapper()
    {
        CreateMap<Box, BoxDTO>().ReverseMap();
        CreateMap<Box, BoxResponse>().ReverseMap();
        CreateMap<BoxRequest, Box>();

        CreateMap<PaginatedResponse<Box>, PaginatedResponse<BoxDTO>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));
    }
}
