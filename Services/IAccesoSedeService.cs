namespace ParkSmart;

public interface IAccesoSedeService
{
    void RegistrarAcceso(Guid usuarioId, Guid sedeId);
    bool TieneAcceso(Guid usuarioId, Guid sedeId);
}
