using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model.Errors;
using GeekShopping.ProductAPI.Repository;
using GeekShopping.ProductAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.ProductAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductVO>>> FindAll()
        {
            var products = await _repository.FindAll();
            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductVO>> FindById(long id)
        {
            try
            {
                var product = await _repository.FindById(id);
                return Ok(product);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao buscar o produto pelo Id = {id}: {ex.Message}");
            }            
        }
        
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductVO>> Create([FromBody] ProductVO product)
        {
            try
            {
                if (product is null)
                {
                    return BadRequest();
                }
                var newProduct = await _repository.Create(product);
                return CreatedAtAction(nameof(FindById), new { id = newProduct.Id }, newProduct);
        }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar o produto '{product.Name}': {ex.Message}");
    }
}

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<ProductVO>> Update([FromBody] ProductVO product)
        {
            try
            {
                if (product is null)
                {
                    return BadRequest();
                }
                var foundProduct = await _repository.FindByIdNoTrack(product.Id);
                var updatedProduct = await _repository.Update(product);
                return Ok(updatedProduct);
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar o produto '{product.Name}': {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                var product = await _repository.FindByIdNoTrack(id);
                await _repository.Delete(product.Id);
                //if (!status)
                //{
                //    return BadRequest();
                //}
                return Ok();
            }
            catch (RecordNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao deletar o produto Id='{id}': {ex.Message}");
            }
        }

    }
}
