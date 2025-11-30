namespace ParkSmart;

public class LoginRespuestaDTO
{
    public string token { get; set; } = string.Empty;
    public UsuarioDTO usuario { get; set; } = null!;
    public DateTime expiracion { get; set; }
}
