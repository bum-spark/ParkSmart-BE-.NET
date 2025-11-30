namespace ParkSmart;

public class UsuarioDTO
{
    public Guid usuarioId { get; set; }
    public string nombreCompleto { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string rol { get; set; } = string.Empty;

    public UsuarioDTO() { }

    public UsuarioDTO(Usuario usuario)
    {
        usuarioId = usuario.usuarioId;
        nombreCompleto = usuario.nombreCompleto;
        email = usuario.email;
        rol = usuario.rol;
    }
}

public class InfoUsuario
{
    public string email { get; set; } = string.Empty;

    public InfoUsuario(UsuarioDTO usuario)
    {
        email = usuario.email;
    }
}

public class LoginResponseDTO
{
    public Guid usuarioId { get; set; }
    public string nombreCompleto { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string rol { get; set; } = string.Empty;
    public LoginResponseDTO (UsuarioDTO usuario)
    {
        usuarioId = usuario.usuarioId;
        nombreCompleto = usuario.nombreCompleto;
        email = usuario.email;
        rol = usuario.rol;
    }
}

public class CambiarPasswordDTO
{
    public string passwordActual { get; set; } = string.Empty;
    public string passwordNueva { get; set; } = string.Empty;
}

public class CambiarRolDTO
{
    public string nuevoRol { get; set; } = string.Empty;
}

public class ActualizarUsuarioDTO
{
    public string nombreCompleto { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
}


