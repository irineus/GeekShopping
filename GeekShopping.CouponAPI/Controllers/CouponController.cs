using GeekShopping.CouponAPI.Data.ValueObjects;
using GeekShopping.CouponAPI.Model.Errors;
using GeekShopping.CouponAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CouponAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private ICouponRepository _repository;

        public CouponController(ICouponRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("{couponCode}")]
        [Authorize]
        public async Task<ActionResult<CouponVO>> FindById(string couponCode)
        {
            try
            {
                var coupon = await _repository.GetCouponByCouponCode(couponCode);
                return Ok(coupon);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o produto pelo Id = {couponCode}: {ex.Message}");
            }
        }
    }
}
