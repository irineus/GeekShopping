using AutoMapper;
using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Model;

namespace GeekShopping.CouponAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.CreateMap<CouponVO, Coupon>().ReverseMap();
            });

            return mappingConfig;
        }
    }
}
