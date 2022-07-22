using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using System.Net.Http.Headers;

namespace GeekShopping.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _client;
        public const string BasePath = "api/v1/product";

        public ProductService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IEnumerable<ProductViewModel>> FindAllProducts(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync(BasePath);
            return await response.ReadContentAs<IEnumerable<ProductViewModel>>();
        }

        public async Task<ProductViewModel> FindProductById(long id, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync($"{BasePath}/{id}");
            return await response.ReadContentAs<ProductViewModel>();
        }
        
        public async Task<ProductViewModel> CreateProduct(ProductViewModel model, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJsonAsync(BasePath, model);
            if (response.IsSuccessStatusCode)
            {
                return await response.ReadContentAs<ProductViewModel>();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
        
        public async Task<ProductViewModel?> UpdateProduct(ProductViewModel model, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PutAsJsonAsync(BasePath, model);
            if (response.IsSuccessStatusCode)
            {
                return await response.ReadContentAs<ProductViewModel>();
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
        
        public async Task DeleteProduct(long id, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.DeleteAsync($"{BasePath}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}
