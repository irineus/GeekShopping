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

    }
}