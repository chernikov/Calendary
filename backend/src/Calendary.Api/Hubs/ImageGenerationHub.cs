using Microsoft.AspNetCore.SignalR;

namespace Calendary.Api.Hubs;

/// <summary>
/// SignalR Hub для відстеження прогресу генерації зображень
/// </summary>
public class ImageGenerationHub : Hub
{
    /// <summary>
    /// Підключення клієнта до групи для отримання оновлень прогресу
    /// </summary>
    /// <param name="taskId">ID задачі генерації</param>
    public async Task JoinTaskGroup(string taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"task_{taskId}");
    }

    /// <summary>
    /// Відключення клієнта від групи
    /// </summary>
    /// <param name="taskId">ID задачі генерації</param>
    public async Task LeaveTaskGroup(string taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task_{taskId}");
    }
}
