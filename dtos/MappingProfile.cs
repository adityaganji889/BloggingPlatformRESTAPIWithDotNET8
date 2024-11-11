using AutoMapper;
using BloggingPlatform.models;

namespace BloggingPlatform.dtos
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Blog, BlogResponseDto>();
            CreateMap<User, RegisterResponseDto>()
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender ? "Male" : "Female"))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active ? "True" : "False"))
                .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Admin ? "True" : "False"));
        }
    }
}