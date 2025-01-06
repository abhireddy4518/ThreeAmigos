using System.Collections.Generic;
using System.Security.Policy;
using System.Text.Json;
using ThreeAmigos.Models;
using ThreeAmigos.Models.ViewModels;

namespace ThreeAmigos.Services;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private const string BASE_URL = "https://dummyjson.com/products";

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Product>> GetProductsAsync(int skip = 0, int limit = 10, string searchTerm = null)
    {
        // Append search query if searchTerm is provided
        string url = "";
        if (searchTerm != null)
        {
           url = BASE_URL + "/search?q=" + searchTerm;
        }
        else
        {
            url = $"{BASE_URL}?limit={limit}&skip={skip}&select=id,title,price,thumbnail,description,category,images";
        }
        

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ProductResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var products = apiResponse?.Products.Select(apiProduct => new Product
            {
                AddedProductId = apiProduct.Id,
                Name = apiProduct.Title,
                Price = apiProduct.Price,
                Description = apiProduct.Description,
                Category = apiProduct.Category,
                ImageUrl = apiProduct.Images != null && apiProduct.Images.Any() ? apiProduct.Images[0] : null,
                ThumbnailImageUrl = apiProduct.Thumbnail,
            }).ToList();

            return products ?? new List<Product>();
        }

        return new List<Product>();
    }

    public async Task<int> GetProductsCountAsync(string searchTerm = null)
    {
        string searchQuery = string.IsNullOrEmpty(searchTerm) ? "" : $"&q={searchTerm}";
        string url = $"{BASE_URL}?{searchQuery}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON
            var apiResponse = JsonSerializer.Deserialize<ProductResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Return total count from response
            return apiResponse?.Total ?? 0;
        }

        return 0;
    }

    public async Task<AddProductViewModel> GetProductByIdAsync(int id)
    {
        string url = $"{BASE_URL}/{id}";

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON response into API Model
            var apiProduct = JsonSerializer.Deserialize<ApiProduct>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            // Map API response to database entity model
            if (apiProduct != null)
            {
                return new AddProductViewModel
                {
                    AddedProductId = apiProduct.Id,
                    Name = apiProduct.Title,
                    Price = apiProduct.Price,
                    Description = apiProduct.Description,
                    Category = apiProduct.Category,

                    // Map the first image as ImageUrl
                    ImageUrl = apiProduct.Images != null && apiProduct.Images.Any() ? apiProduct.Images[0] : null,
                    ThumbnailImageUrl = apiProduct.Thumbnail
                };
            }
        }

        return new AddProductViewModel();
    }
}
