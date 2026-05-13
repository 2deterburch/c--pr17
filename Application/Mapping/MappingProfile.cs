using AutoMapper;
using lab11.Application.DTOs;
using lab11.Domain.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lab11.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.Author.Name));
        }
    }
}