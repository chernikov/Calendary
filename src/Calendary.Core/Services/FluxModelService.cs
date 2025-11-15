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

    Task<IEnumerable<FluxModel>> GetByCategoryIdAsync(int categoryId);
    Task<FluxModel?> GetUserFluxModelAsync(int userId, int fluxModelId);

    Task<IList<FluxModel>> GetListByUserIdAsync(int userId);

    Task SoftDeleteAsync(int id);

    Task ChangeNameAsync(FluxModel fluxModel);

    Task SetActiveAsync(int userId, int fluxModelId);
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
        => fluxModelRepository.GetFullAsync(id);

    public Task<IEnumerable<FluxModel>> GetByCategoryIdAsync(int categoryId)
        => fluxModelRepository.GetByCategoryIdAsync(categoryId);

    public Task<FluxModel?> GetUserFluxModelAsync(int userId, int fluxModelId)
              => fluxModelRepository.GetUserFluxModelAsync(userId, fluxModelId);


    public async Task<IList<FluxModel>> GetListByUserIdAsync(int userId)
    {
        var list = await fluxModelRepository.GetListByUserIdAsync(userId);
        return list.Where(p => !p.IsDeleted).ToList();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entityDb = await fluxModelRepository.GetByIdAsync(id);
        if (entityDb is null)
        {
            return;
        }
        entityDb.IsDeleted = true;
        await fluxModelRepository.UpdateAsync(entityDb);
    }

    public async Task ChangeNameAsync(FluxModel fluxModel)
    {
        // Отримати flux модель з репозиторію за її Id
        var existingEntry = await fluxModelRepository.GetByIdAsync(fluxModel.Id);
        if (existingEntry == null)
        {
            throw new Exception("Flux модель не знайдена");
        }

        // Оновити лише поле Name
        existingEntry.Name = fluxModel.Name;

        // Зберегти зміни в базі даних
        await fluxModelRepository.UpdateAsync(existingEntry);
    }

    public async Task SetActiveAsync(int userId, int fluxModelId)
    {
        // Отримати модель, яку потрібно зробити активною
        var fluxModel = await fluxModelRepository.GetUserFluxModelAsync(userId, fluxModelId);
        if (fluxModel == null)
        {
            throw new Exception("Flux модель не знайдена або не належить користувачу");
        }

        // Деактивувати всі інші моделі користувача
        var allUserModels = await fluxModelRepository.GetListByUserIdAsync(userId);
        foreach (var model in allUserModels)
        {
            if (model.IsActive && model.Id != fluxModelId)
            {
                model.IsActive = false;
                await fluxModelRepository.UpdateAsync(model);
            }
        }

        // Активувати вибрану модель
        if (!fluxModel.IsActive)
        {
            fluxModel.IsActive = true;
            await fluxModelRepository.UpdateAsync(fluxModel);
        }
    }
}