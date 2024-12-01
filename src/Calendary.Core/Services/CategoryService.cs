using Calendary.Model;
using Calendary.Repos.Repositories;



namespace Calendary.Core.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    public Task<Category?> GetByIdAsync(int id)
        => _categoryRepository.GetByIdAsync(id);

    public Task CreateAsync(Category category)
        => _categoryRepository.AddAsync(category);

    public async Task UpdateAsync(Category category)
    {
        var entity = await _categoryRepository.GetByIdAsync(category.Id);
        if (entity is null)
        {
            return;
        }
        entity.Name = category.Name;
        entity.IsAlive = category.IsAlive;
        await _categoryRepository.UpdateAsync(entity);
    }

    public Task DeleteAsync(int id)
        => _categoryRepository.DeleteAsync(id);
}
