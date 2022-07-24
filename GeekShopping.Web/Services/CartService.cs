using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using System.Net.Http.Headers;

namespace GeekShopping.Web.Services
{
    public class CartService : ICartService
    {
        private readonly HttpClient _client;
        public const string BasePath = "api/v1/cart";

        public CartService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<CartViewModel> FindCartByUserId(string userId, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync($"{BasePath}/find-cart/{userId}");
            return await response.ReadContentAs<CartViewModel>();
        }
        public async Task<CartViewModel> AddItemToCart(CartViewModel model, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJson($"{BasePath}/add-cart", model);
            return response.IsSuccessStatusCode ?
                await response.ReadContentAs<CartViewModel>() : throw new HttpRequestException(response.ReasonPhrase);
        }

        public async Task<CartViewModel> UpdateCart(CartViewModel model, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PutAsJson($"{BasePath}/update-cart", model);
            return response.IsSuccessStatusCode ?
                await response.ReadContentAs<CartViewModel>() : throw new HttpRequestException(response.ReasonPhrase);
        }

        public async Task<bool> RemoveFromCart(long cartId, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.DeleteAsync($"{BasePath}/remove-cart/{cartId}");
            return response.IsSuccessStatusCode ?
                await response.ReadContentAs<bool>() : throw new HttpRequestException(response.ReasonPhrase);
        }
        
        public async Task<bool> ApplyCoupon(CartViewModel model, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJson($"{BasePath}/apply-coupon", model);

            return response.IsSuccessStatusCode ?
                await response.ReadContentAs<bool>() : throw new HttpRequestException(response.ReasonPhrase);
        }

        public async Task<bool> RemoveCoupon(string userId, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.DeleteAsync($"{BasePath}/remove-coupon/{userId}");
            return response.IsSuccessStatusCode ?
                await response.ReadContentAs<bool>() : throw new HttpRequestException(response.ReasonPhrase);
        }

        public async Task<object> Checkout(CartHeaderViewModel model, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.PostAsJson($"{BasePath}/checkout", model);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode.ToString().Equals("PreconditionFailed"))
                {
                    return "Coupon Price has changed, please confirm!";
                }
                throw new HttpRequestException(response.ReasonPhrase);
            }
            return await response.ReadContentAs<CartHeaderViewModel>();
        }

        public async Task<bool> ClearCart(string userId, string token)
        {
            throw new NotImplementedException();
        }







    }
}
