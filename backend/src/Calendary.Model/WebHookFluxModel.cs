namespace Calendary.Model;

public class WebHookFluxModel
{
    public int Id { get; set; }

    public int FluxModelId { get; set; }

    public int WebHookId { get; set; }

    public WebHook WebHook { get; set; } = null!;

    public FluxModel FluxModel { get; set; } = null!;
}
