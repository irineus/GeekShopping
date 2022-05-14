using AutoMapper;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model;

namespace GeekShopping.ProductAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.CreateMap<ProductVO, Product>();
                mc.CreateMap<Product, ProductVO>();
            });

            return mappingConfig;
        }
    }
}
