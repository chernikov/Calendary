using Calendary.Model;
using Calendary.Repos.Repositories;

namespace Calendary.Core.Services;

public interface IFluxModelService
{
    Task<FluxModel?> GetCurrentByUserIdAsync(int id);

    Task<FluxModel?> GetByIdAsync(int id);
    Task CreateAsync(FluxModel fluxModel);

    Task UpdateStatusAsync(FluxModel model);
    
    Task UpdateArchiveUrlAsync(FluxModel model);

    Task UpdateReplicateIdAsync(FluxModel model);
}

public class FluxModelService : IFluxModelService
{
    private readonly IFluxModelRepository fluxModelRepository;

    public FluxModelService(IFluxModelRepository fluxModelRepository)
    {
        this.fluxModelRepository = fluxModelRepository;
    }

    public Task<FluxModel?> GetCurrentByUserIdAsync(int useId)
       => fluxModelRepository.GetCurrentByUserIdAsync(useId);

    public Task<FluxModel?> GetByIdAsync(int id)
        => fluxModelRepository.GetByIdAsync(id);


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

}