namespace Trendora.Models.Interfaces
{
    public interface ICategoryRepository
    {
        void AddCategory(Category category);
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryById(int id);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);

        IEnumerable<Category> GetActiveCategories();
        int GetCategoryProductCount(int categoryId);
    }
}
