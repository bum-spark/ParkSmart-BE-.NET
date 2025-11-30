namespace ParkSmart;

public interface ISedeService
{
        Task<List<SedeDTO>> ObtenerTodasLasSedes();

        Task<SedeDTO?> ObtenerSedePorId(Guid sedeId);

        Task<SedeDTO?> CrearSede(CrearSedeDTO crearSede, Guid usuarioId);

        Task<bool> VerificarAccesoSede(Guid sedeId, string password);

        Task<bool> ActualizarEstadoSede(Guid sedeId, string nuevoEstado, string contraseñaCreador);

        Task<SedeDTO?> ActualizarSedeCompleta(Guid sedeId, ActualizarSedeCompletaDTO dto);


        Task<object> ObtenerEstadisticasSede(Guid sedeId);

        Task<List<NivelDTO>> ObtenerNivelesDeSede(Guid sedeId);

        Task<List<CajonDTO>?> ObtenerCajonesPorNivel(Guid sedeId, Guid nivelId);
}
