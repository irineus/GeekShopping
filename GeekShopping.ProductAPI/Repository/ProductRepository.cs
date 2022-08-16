using AutoMapper;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model;
using GeekShopping.ProductAPI.Model.Context;
using GeekShopping.ProductAPI.Model.Errors;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly MySQLContext _context;
        private IMapper _mapper;

        public ProductRepository(MySQLContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductVO>> FindAll()
        {
            List<Product> products = await _context.Products.ToListAsync();
            return _mapper.Map<List<ProductVO>>(products);
        }

        public async Task<ProductVO> FindById(long id)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) 
            {
                throw new RecordNotFoundException($"Produto id={id} não encontrado.");
            }
            return _mapper.Map<ProductVO>(product);
        }

        public async Task<ProductVO> FindByIdNoTrack(long id)
        {
            Product product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                throw new RecordNotFoundException($"Produto id={id} não encontrado.");
            }
            return _mapper.Map<ProductVO>(product);
        }

        public async Task<ProductVO> Create(ProductVO vo)
        {
            try
            {
                Product product = _mapper.Map<Product>(vo);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return _mapper.Map<ProductVO>(product);
            }
            catch (Exception ex)
            {                
                throw new Exception(ex.InnerException is null ? ex.Message : ex.InnerException!.Message, ex);
            }
        }

        public async Task<ProductVO> Update(ProductVO vo)
        {
            try
            {
                Product product = _mapper.Map<Product>(vo);
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return _mapper.Map<ProductVO>(product);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new DbUpdateConcurrencyException($"O produto id={vo.Id} não existe na base de dados.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException is null ? ex.Message : ex.InnerException!.Message, ex);
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (product is null)
                {
                    throw new RecordNotFoundException($"Produto id={id} não encontrado.");
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException is null ? ex.Message : ex.InnerException!.Message, ex);
            }
        }
    }
}
