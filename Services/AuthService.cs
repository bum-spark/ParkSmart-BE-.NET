using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace ParkSmart;

public class AuthService : IAuthService
{
    private readonly ParkSmartDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ParkSmartDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<UsuarioDTO> RegistrarUsuario(RegistroUsuarioDTO registro)
    {
        if (await EmailExiste(registro.email))
        {
            return null;
        }

        var passwordHash = HashPassword(registro.password);

        var nuevoUsuario = new Usuario
        {
            usuarioId = Guid.NewGuid(),
            nombreCompleto = registro.nombreCompleto,
            email = registro.email,
            passwordHash = passwordHash,
            rol = RolUsuario.Empleado.ToString(), // Siempre Empleado por seguridad
            fechaCreacion = FechaHelper.AhoraLocal()
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return new UsuarioDTO(nuevoUsuario);
    }

    public async Task<LoginRespuestaDTO> Login(LoginDTO login)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.email == login.email);
        if (usuario == null) return null;
        if (!VerificarPassword(login.password, usuario.passwordHash)) return null;
        var token = GenerarToken(usuario);
        var expiracion = FechaHelper.AhoraLocal().AddMinutes(
            int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")
        );
        return new LoginRespuestaDTO
        {
            token = token,
            usuario = new UsuarioDTO(usuario),
            expiracion = expiracion
        };
    }

    public async Task<bool> EmailExiste(string email)
    {
        return await _context.Usuarios.AnyAsync(u => u.email == email);
    }

    public async Task<Usuario> ObtenerUsuarioPorId(Guid usuarioId)
    {
        return await _context.Usuarios.FindAsync(usuarioId);
    }

    public async Task<List<UsuarioDTO>> ObtenerTodosLosUsuarios()
    {
        var usuarios = await _context.Usuarios
            .Select(u => new UsuarioDTO
            {
                usuarioId = u.usuarioId,
                nombreCompleto = u.nombreCompleto,
                email = u.email,
                rol = u.rol
            })
            .ToListAsync();

        return usuarios;
    }

    public async Task<Usuario> ActualizarUsuario(Guid usuarioId, ActualizarUsuarioDTO dto)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return null;

        usuario.nombreCompleto = dto.nombreCompleto;
        usuario.email = dto.email;

        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> CambiarPassword(Guid usuarioId, string passwordActual, string passwordNueva)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return false;

        // Verificar que la contraseña actual sea correcta
        if (!BCrypt.Net.BCrypt.Verify(passwordActual, usuario.passwordHash))
        {
            return false;
        }

        // Hashear la nueva contraseña
        usuario.passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva, workFactor: 12);
        await _context.SaveChangesAsync();

        return true;
    }

    public string GenerarToken(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, usuario.nombreCompleto),
            new Claim(JwtRegisteredClaimNames.Email, usuario.email),
            new Claim(ClaimTypes.Role, usuario.rol)
            //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        
    }

    private bool VerificarPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CambiarRol(Guid id, string rol)
    {
        if (!Enum.TryParse<RolUsuario>(rol, ignoreCase: true, out _)) return false;

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return false;
        
        usuario.rol = rol;
        await _context.SaveChangesAsync();
        
        return true;
    }
}
