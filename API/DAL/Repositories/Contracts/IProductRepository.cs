using DAL.Entities;

namespace DAL.Repositories.Contracts
{
    public interface IProductRepository
    {
        Task CreateProduct(Product product);
        Task DeleteProduct(Product product);
        Task<List<Product>> GetAllProducts();
        Task<Product> GetProductById(Guid id);
        Task UpdateProduct(Product product);
    }
}