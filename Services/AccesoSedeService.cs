using Microsoft.Extensions.Caching.Memory;
namespace ParkSmart;

public class AccesoSedeService : IAccesoSedeService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _duracionPorDefecto = TimeSpan.FromHours(8);

    public AccesoSedeService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void RegistrarAcceso(Guid usuarioId, Guid sedeId)
    {
        var key = GenerarClave(usuarioId, sedeId);
        var duracionFinal = _duracionPorDefecto;

        var opciones = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duracionFinal,
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(key, true, opciones);
    }

    public bool TieneAcceso(Guid usuarioId, Guid sedeId)
    {
        var key = GenerarClave(usuarioId, sedeId);
        return _cache.TryGetValue(key, out bool _);
    }

    private string GenerarClave(Guid usuarioId, Guid sedeId)
    {
        return $"AccesoSede:Usuario:{usuarioId}:Sede:{sedeId}";
    }
}
