using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.Model.Errors;
using GeekShopping.CartAPI.MessageSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IMessageSender _messageSender;

        public CartController(ICartRepository cartRepository, ICouponRepository couponRepository, IMessageSender messageSender)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _couponRepository = couponRepository ?? throw new ArgumentNullException(nameof(couponRepository));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartVO>> FindById(string id)
        {
            try
            {
                var cart = await _cartRepository.FindCartByUserId(id);
                return Ok(cart);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o carrinho pelo Id = {id}: {ex.Message}");
            }
        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart(CartVO vo)
        {
            try
            {
                var cart = await _cartRepository.SaveOrUpdateCart(vo);
                if (cart is null) return NotFound();
                return Ok(cart);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o carrinho pelo Id = {vo.CartHeader.Id}: {ex.Message}");
            }
        }
        
        [HttpPut("update-cart")]
        public async Task<ActionResult<CartVO>> UpdateCart(CartVO vo)
        {
            try
            {
                var cart = await _cartRepository.SaveOrUpdateCart(vo);
                if (cart is null) return NotFound();
                return Ok(cart);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o carrinho pelo Id = {vo.CartHeader.Id}: {ex.Message}");
            }
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<CartVO>> RemoveCart(int id)
        {
            try
            {
                var status = await _cartRepository.RemoveFromCart(id);
                if (!status) return BadRequest();
                return Ok(status);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o carrinho pelo Id = {id}: {ex.Message}");
            }
        }
        

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO vo)
        {
            try
            {
                var status = await _cartRepository.ApplyCoupon(vo.CartHeader.UserId, vo.CartHeader.CouponCode);
                if (!status) return NotFound();
                return Ok(status);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao aplicar cupom Cod = {vo.CartHeader.CouponCode} para o UserId = {vo.CartHeader.UserId}: {ex.Message}");
            }
        }
        
        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
        {
            try
            {
                var status = await _cartRepository.RemoveCoupon(userId);
                if (!status) return NotFound();
                return Ok(status);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao remover cupom para o UserId = {userId}: {ex.Message}");
            }
        }
        
        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO vo)
        {
            try
            {
                var token = await HttpContext.GetTokenAsync("access_token");
                if (vo?.UserId is null) return BadRequest();
                var cart = await _cartRepository.FindCartByUserId(vo.UserId);

                if (!string.IsNullOrEmpty(vo.CouponCode))
                {
                    CouponVO coupon = await _couponRepository.GetCoupon(vo.CouponCode, token);
                    if (vo.DiscountAmount != coupon.DiscountAmount)
                    {
                        // Sinaliza que as condições mudaram
                        return StatusCode(412);
                    }
                }
                
                vo.CartDetails = cart.CartDetails;
                vo.DateTime = DateTime.Now;

                _messageSender.SendMessageAsync(vo, "checkoutqueue");

                await _cartRepository.ClearCart(vo.UserId);

                return Ok(vo);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o carrinho pelo Id = {vo.UserId}: {ex.Message}");
            }
        }

    }
}