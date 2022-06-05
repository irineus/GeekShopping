using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;

namespace GeekShopping.CartAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.CreateMap<ProductVO, Product>().ReverseMap();
                mc.CreateMap<CartHeaderVO, CartHeader>().ReverseMap();
                mc.CreateMap<CartDetailVO, CartDetail>().ReverseMap();
                mc.CreateMap<CartVO, Cart>().ReverseMap();
            });

            return mappingConfig;
        }
    }
}
