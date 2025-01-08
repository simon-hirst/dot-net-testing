using AutoMapper;
using CrudDotNetTesting.Dtos;
using CrudDotNetTesting.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrudDotNetTesting.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
        }
    }
}