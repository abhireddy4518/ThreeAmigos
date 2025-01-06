namespace ThreeAmigos.Models;

public class ApiProduct
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Thumbnail { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public List<string> Images { get; set; }
}

// Wrapper for API response
public class ProductResponse
{
    public List<ApiProduct> Products { get; set; }

    public ApiProduct Product { get; set; }
    public int Total { get; set; }                 // Add Total count property
    public int Skip { get; set; }                  // Add Skip for pagination
    public int Limit { get; set; }
}
