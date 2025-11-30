namespace ParkSmart;

public interface IAuthService
{
        Task<UsuarioDTO?> RegistrarUsuario(RegistroUsuarioDTO registro);
        Task<LoginRespuestaDTO?> Login(LoginDTO login);
        Task<bool> EmailExiste(string email);
        Task<Usuario?> ObtenerUsuarioPorId(Guid usuarioId);
        Task<List<UsuarioDTO>> ObtenerTodosLosUsuarios();
        Task<Usuario?> ActualizarUsuario(Guid usuarioId, ActualizarUsuarioDTO dto);
        Task<bool> CambiarPassword(Guid usuarioId, string passwordActual, string passwordNueva);
        string GenerarToken(Usuario usuario);
        Task<bool> CambiarRol(Guid id, string rol);
}
