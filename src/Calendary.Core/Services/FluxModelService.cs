using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IFluxModelService
{
    Task<IReadOnlyCollection<FluxModel>> GetAllAsync(int page, int pageSize);

    Task<FluxModel?> GetCurrentByUserIdAsync(int id);

    Task<FluxModel?> GetByIdAsync(int id);

    Task<FluxModel?> GetByTrainingIdAsync(int trainingId);
    Task CreateAsync(FluxModel fluxModel);

    Task UpdateStatusAsync(FluxModel model);

    Task UpdateArchiveUrlAsync(FluxModel model);

    Task UpdateReplicateIdAsync(FluxModel model);
    
    Task UpdateVersionAsync(FluxModel fluxModel);
    
    Task ArchiveAsync(FluxModel fluxModel);

    Task<FluxModel?> GetFullAsync(int id);

    Task<IEnumerable<FluxModel>> GetByAgeGenderAsync(int ageGender);
}

public class FluxModelService : IFluxModelService
{
    private readonly IFluxModelRepository fluxModelRepository;
    private readonly ITrainingRepository trainingRepository;

    public FluxModelService(IFluxModelRepository fluxModelRepository, ITrainingRepository trainingRepository)
    {
        this.fluxModelRepository = fluxModelRepository;
        this.trainingRepository = trainingRepository;
    }

    public async Task<IReadOnlyCollection<FluxModel>> GetAllAsync(int page, int pageSize)
    {
        return await fluxModelRepository.GetAllAsync(page, pageSize);
    }

    public Task<FluxModel?> GetCurrentByUserIdAsync(int useId)
       => fluxModelRepository.GetCurrentByUserIdAsync(useId);

    public Task<FluxModel?> GetByIdAsync(int id)
        => fluxModelRepository.GetByIdAsync(id);


    public async Task<FluxModel?> GetByTrainingIdAsync(int trainingId)
    {

        var training = await trainingRepository.GetByIdAsync(trainingId);
        if (training is null)
        {
            return null;
        }
        return await fluxModelRepository.GetByIdAsync(training.FluxModelId);
    }

    public async Task CreateAsync(FluxModel model)
    {
        await fluxModelRepository.AddAsync(model);
    }

    public async Task UpdateStatusAsync(FluxModel model)
    {

        var entityDb = await fluxModelRepository.GetByIdAsync(model.Id);
        if (entityDb is null)
        {
            return;
        }
        entityDb.Status = model.Status;
        await fluxModelRepository.UpdateAsync(entityDb);
    }

    public async Task UpdateArchiveUrlAsync(FluxModel model)
    {

        var entityDb = await fluxModelRepository.GetByIdAsync(model.Id);
        if (entityDb is null)
        {
            return;
        }
        entityDb.ArchiveUrl = model.ArchiveUrl;
        await fluxModelRepository.UpdateAsync(entityDb);
    }


    public async Task UpdateReplicateIdAsync(FluxModel model)
    {
        var entityDb = await fluxModelRepository.GetByIdAsync(model.Id);
        if (entityDb is null)
        {
            return;
        }
        entityDb.ReplicateId = model.ReplicateId;
        await fluxModelRepository.UpdateAsync(entityDb);
    }

    public async Task UpdateVersionAsync(FluxModel model)
    {
        var entityDb = await fluxModelRepository.GetByIdAsync(model.Id);
        if (entityDb is null)
        {
            return;
        }
        entityDb.Version = model.Version;
        await fluxModelRepository.UpdateAsync(entityDb);
    }

    public async Task ArchiveAsync(FluxModel fluxModel)
    {
        var entityDb = await fluxModelRepository.GetByIdAsync(fluxModel.Id);
        if (entityDb is null)
        {
            return;
        }
        entityDb.IsArchive = true;
        await fluxModelRepository.UpdateAsync(entityDb);
    }

    public Task<FluxModel?> GetFullAsync(int id)
    {
        return fluxModelRepository.GetFullAsync(id);
    }

    public Task<IEnumerable<FluxModel>> GetByAgeGenderAsync(int ageGender)
        => fluxModelRepository.GetByAgeGenderAsync(ageGender);

 
}