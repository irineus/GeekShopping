using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model.Errors;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CartController : ControllerBase
    {
        private ICartRepository _repository;

        public CartController(ICartRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartVO>> FindById(string id)
        {
            try
            {
                var cart = await _repository.FindCartByUserId(id);
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
                var cart = await _repository.SaveOrUpdateCart(vo);
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
                var cart = await _repository.SaveOrUpdateCart(vo);
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
                var status = await _repository.RemoveFromCart(id);
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
                var status = await _repository.ApplyCoupon(vo.CartHeader.UserId, vo.CartHeader.CouponCode);
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
                var status = await _repository.RemoveCoupon(userId);
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

    }
}