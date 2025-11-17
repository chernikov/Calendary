using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IPhotoService
{
    Task<IReadOnlyCollection<Photo>> GetByFluxModelIdAsync(int fluxModelId);
    Task SaveAsync(Photo photo);
}

public class PhotoService : IPhotoService
{
    private readonly IPhotoRepository photoRepository;

    public PhotoService(IPhotoRepository photoRepository)
    {
        this.photoRepository = photoRepository;
    }

    public Task<IReadOnlyCollection<Photo>> GetByFluxModelIdAsync(int fluxModelId)
        => photoRepository.GetByFluxModelIdAsync(fluxModelId);
    
    public async Task SaveAsync(Photo photo)
    {
        await photoRepository.AddAsync(photo);
    }
}
