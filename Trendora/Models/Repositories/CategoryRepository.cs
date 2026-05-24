using Dapper;
using System.Data;
using Trendora.Models.Interfaces;
using Trendora.Services;

namespace Trendora.Models.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public CategoryRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #region Category Methods

        public void AddCategory(Category category)
        {
            using var conn = _dbFactory.CreateConnection();

            string query = @"INSERT INTO Categories 
                (Name, IsActive, ImagePath, IsNew, IsSale, Description, CreatedAt, UpdatedAt) 
                VALUES 
                (@Name, @IsActive, @ImagePath, @IsNew, @IsSale, @Description, GETDATE(), GETDATE())";

            conn.Execute(query, category);
        }

        public IEnumerable<Category> GetAllCategories()
        {
            using var con = _dbFactory.CreateConnection();

            string query = @"
                SELECT 
                    c.*, 
                    ISNULL((SELECT COUNT(*) FROM Products WHERE CategoryId = c.CategoryId), 0) as ProductQuantity,
                    ISNULL((SELECT COUNT(*) FROM Products WHERE CategoryId = c.CategoryId AND Quantity > 0), 0) as ActiveProductQuantity
                FROM Categories c 
                ORDER BY c.Name ASC";

            return con.Query<Category>(query);
        }

        public Category GetCategoryById(int id)
        {
            using var con = _dbFactory.CreateConnection();

            string query = @"SELECT c.*, 
                (SELECT COUNT(*) FROM Products WHERE CategoryId = c.CategoryId) as ProductQuantity 
                FROM Categories c 
                WHERE c.CategoryId = @id";

            return con.QuerySingleOrDefault<Category>(query, new { id });
        }

        public void UpdateCategory(Category category)
        {
            using var con = _dbFactory.CreateConnection();

            string query = @"UPDATE Categories SET 
                Name = @Name, 
                IsActive = @IsActive,
                ImagePath = COALESCE(@ImagePath, ImagePath),
                IsNew = @IsNew, 
                IsSale = @IsSale, 
                Description = @Description,
                UpdatedAt = GETDATE()
                WHERE CategoryId = @CategoryId";

            con.Execute(query, category);
        }

        public void DeleteCategory(int id)
        {
            using var con = _dbFactory.CreateConnection();

            con.Execute(
                "UPDATE Products SET CategoryId = NULL WHERE CategoryId = @id",
                new { id });

            con.Execute(
                "DELETE FROM Categories WHERE CategoryId = @id",
                new { id });
        }

        public IEnumerable<Category> GetActiveCategories()
        {
            using var con = _dbFactory.CreateConnection();
            return con.Query<Category>(
                "SELECT * FROM Categories WHERE IsActive = 1 ORDER BY Name");
        }

        public int GetCategoryProductCount(int categoryId)
        {
            using var con = _dbFactory.CreateConnection();

            return con.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Products WHERE CategoryId = @categoryId",
                new { categoryId });
        }

        #endregion
    }
}
