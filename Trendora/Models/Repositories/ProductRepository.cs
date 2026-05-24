using Microsoft.Data.SqlClient;
using Trendora.Models.Interfaces;
using Dapper;
using System.Data;
using Trendora.Services;


namespace Trendora.Models.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public ProductRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #region Product Methods
        public void AddProduct(Product product)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                
                if (product.CategoryId.HasValue && product.CategoryId > 0)
                {
                    var categoryExists = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Categories WHERE CategoryId = @CategoryId",
                        new { product.CategoryId });
                    
                    if (categoryExists == 0)
                    {
                        product.CategoryId = null;
                    }
                }

                string addQuery = @"INSERT INTO Products 
                      (Name, CategoryId, Brand, Price, OriginalPrice, Color, Gender,
                       Rating, Quantity, Size, IsNew, IsSale, Description, ImagePath, CreatedAt, UpdatedAt) 
                      VALUES 
                      (@Name, @CategoryId, @Brand, @Price, @OriginalPrice, @Color, @Gender,
                       @Rating, @Quantity, @Size, @IsNew, @IsSale, @Description, @ImagePath, GETDATE(), GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() as int)";

                var productId = conn.ExecuteScalar<int>(addQuery, new
                {
                    product.Name,
                    product.CategoryId,
                    product.Brand,
                    product.Price,
                    product.OriginalPrice,
                    product.Color,
                    product.Gender,
                    product.Rating,
                    product.Quantity,
                    product.Size,
                    product.IsNew,
                    product.IsSale,
                    product.Description,
                    product.ImagePath
                });
                
                product.ProductId = productId;
            }
        }

        public IEnumerable<Product> GetAllProducts()
        {
            using (var con = _dbFactory.CreateConnection())
            {
                Console.WriteLine("DEBUG: GetAllProducts called");

                string query = @"SELECT 
            p.*, 
            ISNULL(c.Name, 'Uncategorized') as CategoryName,
            ISNULL(c.CategoryId, 0) as CategoryId
        FROM Products p 
        LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
        ORDER BY p.CreatedAt DESC";

                var products = con.Query<Product>(query).ToList();
            
               

                return products;
            }
        }

        public Product GetProductById(int id)
        {
            using (var con = _dbFactory.CreateConnection())
            {
                string query = @"SELECT 
                    p.*, 
                    ISNULL(c.Name, 'Uncategorized') as CategoryName
                FROM Products p 
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId 
                WHERE p.ProductId = @id";

                var product = con.QuerySingleOrDefault<Product>(query, new { id });
                return product;
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var con = _dbFactory.CreateConnection())
            {
               
                if (product.CategoryId.HasValue && product.CategoryId > 0)
                {
                    var categoryExists = con.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM Categories WHERE CategoryId = @CategoryId",
                        new { product.CategoryId });
                    
                    if (categoryExists == 0)
                    {
                        product.CategoryId = null;
                    }
                }

                string query = @"UPDATE Products SET 
                    Name = @Name, 
                    CategoryId = @CategoryId,
                    Brand = @Brand, 
                    Price = @Price, 
                    OriginalPrice = @OriginalPrice, 
                    Color = @Color, 
                    Gender = @Gender,
                    Rating = @Rating, 
                    Size = @Size, 
                    Quantity = @Quantity,
                    IsNew = @IsNew, 
                    IsSale = @IsSale, 
                    Description = @Description, 
                    ImagePath = COALESCE(@ImagePath, ImagePath),
                    UpdatedAt = GETDATE()
                    WHERE ProductId = @ProductId";

                con.Execute(query, new
                {
                    product.Name,
                    product.CategoryId,
                    product.Brand,
                    product.Price,
                    product.OriginalPrice,
                    product.Color,
                    product.Gender,
                    product.Rating,
                    product.Size,
                    product.Quantity,
                    product.IsNew,
                    product.IsSale,
                    product.Description,
                    product.ImagePath,
                    product.ProductId
                });
            }
        }

        public void DeleteProduct(int id)
        {
            using (var con = _dbFactory.CreateConnection())
            {
               
                var orderCount = con.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM OrderItems WHERE ProductId = @id",
                    new { id });
                
                if (orderCount > 0)
                {
                    throw new InvalidOperationException("Cannot delete product that exists in orders.");
                }

                string query = "DELETE FROM Products WHERE ProductId = @id";
                con.Execute(query, new { id });
            }
        }

        public IEnumerable<Product> GetFilteredProducts(
     string searchTerm = null,
     string category = null,
     decimal maxPrice = 500,
     List<string> sizes = null,
     List<string> brands = null,
     List<string> colors = null,
     int minRating = 0,
     string sortBy = "featured")
        {
            using (var con = _dbFactory.CreateConnection())
            {
                Console.WriteLine($"=== REPOSITORY DEBUG GetFilteredProducts ===");
                Console.WriteLine($"Parameters: searchTerm={searchTerm}, category={category}, maxPrice={maxPrice}, minRating={minRating}");

                var testProducts = con.Query<Product>("SELECT * FROM Products").ToList();
           
                var baseQuery = @"SELECT 
            p.*, 
            ISNULL(c.Name, 'Uncategorized') as CategoryName
        FROM Products p 
        LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
        WHERE 1=1";

                var parameters = new DynamicParameters();

            
                baseQuery += " AND p.Price <= @maxPrice";
                parameters.Add("@maxPrice", maxPrice);


                if (minRating > 0)
                {
                    baseQuery += " AND (p.Rating >= @minRating OR p.Rating IS NULL)";
                    parameters.Add("@minRating", minRating);
                }
                else
                {
                    // When minRating is 0, don't filter by rating at all
                    baseQuery += " AND (p.Rating >= 0 OR p.Rating IS NULL)";
                }

                var conditions = new List<string>();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    conditions.Add("(p.Name LIKE @searchTerm OR p.Description LIKE @searchTerm OR p.Brand LIKE @searchTerm)");
                    parameters.Add("@searchTerm", $"%{searchTerm}%");
                }

                if (!string.IsNullOrEmpty(category) && category != "All")
                {
                    conditions.Add("c.Name = @category");
                    parameters.Add("@category", category);
                }

                if (sizes != null && sizes.Any())
                {
                    conditions.Add("p.Size IS NOT NULL AND EXISTS (SELECT 1 FROM STRING_SPLIT(p.Size, ',') s WHERE TRIM(s.value) IN @sizes)");
                    parameters.Add("@sizes", sizes.Select(s => s.Trim()).ToArray());
                }

                if (brands != null && brands.Any())
                {
                    conditions.Add("p.Brand IN @brands");
                    parameters.Add("@brands", brands);
                }

                if (colors != null && colors.Any())
                {
                    conditions.Add("p.Color IS NOT NULL AND EXISTS (SELECT 1 FROM STRING_SPLIT(p.Color, ',') col WHERE TRIM(col.value) IN @colors)");
                    parameters.Add("@colors", colors.Select(c => c.Trim()).ToArray());
                }

                if (conditions.Any())
                {
                    baseQuery += " AND " + string.Join(" AND ", conditions);
                }

                switch (sortBy.ToLower())
                {
                    case "price-low":
                        baseQuery += " ORDER BY p.Price ASC";
                        break;
                    case "price-high":
                        baseQuery += " ORDER BY p.Price DESC";
                        break;
                    case "rating":
                        baseQuery += " ORDER BY COALESCE(p.Rating, 0) DESC";
                        break;
                    case "newest":
                        baseQuery += " ORDER BY p.CreatedAt DESC";
                        break;
                    default: 
                        baseQuery += " ORDER BY CASE WHEN p.IsSale = 1 THEN 0 WHEN p.IsNew = 1 THEN 1 ELSE 2 END, COALESCE(p.Rating, 0) DESC";
                        break;
                }

               

                try
                {
                    var result = con.Query<Product>(baseQuery, parameters).ToList();

                  

                    

               

                    return result;
                }
                catch (Exception ex)
                {
                    Logger.LogExpception(ex);
                    
                    return new List<Product>();
                }
            }
        }

        public int GetTotalProductCount()
        {
            using (var con = _dbFactory.CreateConnection())
            {
                string query = "SELECT COUNT(*) FROM Products WHERE Quantity > 0";
                return con.ExecuteScalar<int>(query);
            }
        }

        public IEnumerable<Product> GetProductsByCategory(int categoryId)
        {
            using (var con = _dbFactory.CreateConnection())
            {
                string query = @"SELECT p.*, c.Name as CategoryName 
                               FROM Products p 
                               LEFT JOIN Categories c ON p.CategoryId = c.CategoryId 
                               WHERE p.CategoryId = @categoryId AND p.Quantity > 0
                               ORDER BY p.CreatedAt DESC";

                return con.Query<Product>(query, new { categoryId });
            }
        }

        public IEnumerable<Product> GetFeaturedProducts()
        {
            using (var con = _dbFactory.CreateConnection())
            {
                string query = @"
            SELECT * FROM (
                SELECT p.*, c.Name as CategoryName,
                       ROW_NUMBER() OVER (ORDER BY p.CreatedAt DESC) as RowNum
                FROM Products p 
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId 
                WHERE p.Quantity > 0 AND (p.IsNew = 1 OR p.IsSale = 1)
            ) AS NumberedProducts
            WHERE RowNum BETWEEN 5 AND 8"; // Gets rows 5-8 (second set of 4)

                return con.Query<Product>(query);
            }
        }

        public IEnumerable<Product> GetTrendingProducts()
        {
            using (var con = _dbFactory.CreateConnection())
            {
                string query = @"
            SELECT TOP 4 p.*, c.Name as CategoryName 
            FROM Products p 
            LEFT JOIN Categories c ON p.CategoryId = c.CategoryId 
            WHERE p.Quantity > 0 AND (p.IsNew = 1 OR p.IsSale = 1)
            ORDER BY p.CreatedAt DESC";

                return con.Query<Product>(query);
            }
        }

        public IEnumerable<Product> GetNewArrivals()
        {
            using (var con = _dbFactory.CreateConnection())
            {
                string query = @"SELECT TOP 6 p.*, c.Name as CategoryName 
                               FROM Products p 
                               LEFT JOIN Categories c ON p.CategoryId = c.CategoryId 
                               WHERE p.Quantity > 0 AND p.IsNew = 1
                               ORDER BY p.CreatedAt DESC";

                return con.Query<Product>(query);
            }
        }

        public IEnumerable<Product> GetSaleProducts(
     string searchTerm = null,
     string category = null,
     decimal maxPrice = 500,
     List<string> sizes = null,
     List<string> brands = null,
     List<string> colors = null,
     int minRating = 0,
     string sortBy = "featured")
        {
            using (var con = _dbFactory.CreateConnection())
            {
                Console.WriteLine($"=== REPOSITORY DEBUG GetSaleProducts ===");
                Console.WriteLine($"Parameters: searchTerm={searchTerm}, category={category}, maxPrice={maxPrice}, minRating={minRating}");

         
                var baseQuery = @"SELECT 
            p.*, 
            ISNULL(c.Name, 'Uncategorized') as CategoryName
        FROM Products p 
        LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
        WHERE p.IsSale = 1 AND 1=1";

                var parameters = new DynamicParameters();

             
                baseQuery += " AND p.Price <= @maxPrice";
                parameters.Add("@maxPrice", maxPrice);

          
                if (minRating > 0)
                {
                    baseQuery += " AND (p.Rating >= @minRating OR p.Rating IS NULL)";
                    parameters.Add("@minRating", minRating);
                }
                else
                {
                    baseQuery += " AND (p.Rating >= 0 OR p.Rating IS NULL)";
                }

                var conditions = new List<string>();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    conditions.Add("(p.Name LIKE @searchTerm OR p.Description LIKE @searchTerm OR p.Brand LIKE @searchTerm)");
                    parameters.Add("@searchTerm", $"%{searchTerm}%");
                }

                if (!string.IsNullOrEmpty(category) && category != "All")
                {
                    conditions.Add("c.Name = @category");
                    parameters.Add("@category", category);
                }

                if (sizes != null && sizes.Any())
                {
                    conditions.Add("p.Size IS NOT NULL AND EXISTS (SELECT 1 FROM STRING_SPLIT(p.Size, ',') s WHERE TRIM(s.value) IN @sizes)");
                    parameters.Add("@sizes", sizes.Select(s => s.Trim()).ToArray());
                }

                if (brands != null && brands.Any())
                {
                    conditions.Add("p.Brand IN @brands");
                    parameters.Add("@brands", brands);
                }

                if (colors != null && colors.Any())
                {
                    conditions.Add("p.Color IS NOT NULL AND EXISTS (SELECT 1 FROM STRING_SPLIT(p.Color, ',') col WHERE TRIM(col.value) IN @colors)");
                    parameters.Add("@colors", colors.Select(c => c.Trim()).ToArray());
                }

                if (conditions.Any())
                {
                    baseQuery += " AND " + string.Join(" AND ", conditions);
                }

                switch (sortBy.ToLower())
                {
                    case "price-low":
                        baseQuery += " ORDER BY p.Price ASC";
                        break;
                    case "price-high":
                        baseQuery += " ORDER BY p.Price DESC";
                        break;
                    case "rating":
                        baseQuery += " ORDER BY COALESCE(p.Rating, 0) DESC";
                        break;
                    case "newest":
                        baseQuery += " ORDER BY p.CreatedAt DESC";
                        break;
                    default: 
                        baseQuery += " ORDER BY ((p.OriginalPrice - p.Price) / p.OriginalPrice * 100) DESC, p.CreatedAt DESC";
                        break;
                }

             

                try
                {
                    var result = con.Query<Product>(baseQuery, parameters).ToList();

                   


                  

                    return result;
                }
                catch (Exception ex)
                {
                    Logger.LogExpception(ex);
                   
                    return new List<Product>();
                }
            }
        }
        #endregion
    }
}