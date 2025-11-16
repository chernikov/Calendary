using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IUserPhotoService
{
    Task<IReadOnlyCollection<UserPhoto>> GetByUserIdAsync(int userId, bool includeDeleted = false);
    Task<UserPhoto?> GetByIdAsync(int id);
    Task<UserPhoto?> GetByIdAndUserIdAsync(int id, int userId);
    Task<int> GetCountByUserIdAsync(int userId);
    Task<UserPhoto> CreateAsync(UserPhoto photo);
    Task<UserPhoto> UpdateAsync(UserPhoto photo);
    Task DeleteAsync(int id, int userId);
    Task SoftDeleteAsync(int id, int userId);
}

public class UserPhotoService : IUserPhotoService
{
    private readonly IUserPhotoRepository _photoRepository;

    public UserPhotoService(IUserPhotoRepository photoRepository)
    {
        _photoRepository = photoRepository;
    }

    public Task<IReadOnlyCollection<UserPhoto>> GetByUserIdAsync(int userId, bool includeDeleted = false)
        => _photoRepository.GetByUserIdAsync(userId, includeDeleted);

    public Task<UserPhoto?> GetByIdAsync(int id)
        => _photoRepository.GetByIdAsync(id);

    public Task<UserPhoto?> GetByIdAndUserIdAsync(int id, int userId)
        => _photoRepository.GetByIdAndUserIdAsync(id, userId);

    public Task<int> GetCountByUserIdAsync(int userId)
        => _photoRepository.GetCountByUserIdAsync(userId);

    public async Task<UserPhoto> CreateAsync(UserPhoto photo)
    {
        photo.CreatedAt = DateTime.UtcNow;
        photo.IsDeleted = false;
        await _photoRepository.AddAsync(photo);
        return photo;
    }

    public async Task<UserPhoto> UpdateAsync(UserPhoto photo)
    {
        photo.UpdatedAt = DateTime.UtcNow;
        await _photoRepository.UpdateAsync(photo);
        return photo;
    }

    public Task DeleteAsync(int id, int userId)
        => _photoRepository.DeleteAsync(id);

    public Task SoftDeleteAsync(int id, int userId)
        => _photoRepository.SoftDeleteAsync(id, userId);
}
