using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using ParkSmart;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegistroUsuarioDTO> _registroValidator;
        private readonly IValidator<LoginDTO> _loginValidator;

        public AuthController(
            IAuthService authService,
            IValidator<RegistroUsuarioDTO> registroValidator,
            IValidator<LoginDTO> loginValidator)
        {
            _authService = authService;
            _registroValidator = registroValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroUsuarioDTO registro)
        {
            var validationResult = await _registroValidator.ValidateAsync(registro);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    error= true,
                    msg = "Errores de validación",
                    errors = validationResult.Errors.Select(e => new
                    {
                        campo = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            if (await _authService.EmailExiste(registro.email)) return BadRequest(new { 
                error= true, 
                msg = "El email ya está registrado" 
            });

            var usuarioCreado = await _authService.RegistrarUsuario(registro);

            if (usuarioCreado == null)
            {
                return BadRequest(new { 
                    error= true, 
                    msg = "Error al crear el usuario" 
                });
            }

            return Ok(new{
               error = false,
               msg= "El usuario se creó correctamente!",
               data= new InfoUsuario(usuarioCreado)
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var validationResult = await _loginValidator.ValidateAsync(login);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    error= true,
                    msg = "Errores de validación",
                    errors = validationResult.Errors.Select(e => new
                    {
                        campo = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            var resultado = await _authService.Login(login);

            if (resultado == null) return Unauthorized(new { error= true, msg = "Credenciales inválidas" });

            return Ok(new
            {
                error = false,
                msg = "Login exitoso",
                token = resultado.token,
                usuario = new LoginResponseDTO(resultado.usuario),
                expiracion = resultado.expiracion
            });
        }

        [HttpGet("usuario/{id}")]
        [Authorize]
        public async Task<IActionResult> ObtenerUsuario(Guid id)
        {
            var usuario = await _authService.ObtenerUsuarioPorId(id);

            if (usuario == null)
            {
                return NotFound(new { error = true, msg = "Usuario no encontrado" });
            }

            return Ok(new {
                error = false,
                msg = "Usuario encontrado",
                data = new UsuarioDTO(usuario)
            });
        }

        [HttpGet("usuarios")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> ObtenerTodosLosUsuarios()
        {
            var usuarios = await _authService.ObtenerTodosLosUsuarios();

            return Ok(new {
                error = false,
                msg = "Usuarios obtenidos correctamente",
                total = usuarios.Count,
                data = usuarios
            });
        }

        [HttpPut("usuario/{id}")]
        [Authorize]
        public async Task<IActionResult> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioDTO dto)
        {
            var usuarioActualizado = await _authService.ActualizarUsuario(id, dto);

            if (usuarioActualizado == null)
            {
                return NotFound(new { error = true, msg = "Usuario no encontrado" });
            }

            return Ok(new
            {
                error = false,
                msg = "Usuario actualizado exitosamente",
                data = new UsuarioDTO(usuarioActualizado)
            });
        }

        [HttpPatch("usuario/cambiar-password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDTO dto)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");
            
            if (userIdClaim == null)
            {
                return Unauthorized(new { error = true, msg = "No se pudo obtener el ID del usuario del token" });
            }

            var usuarioId = Guid.Parse(userIdClaim.Value);

            if (string.IsNullOrWhiteSpace(dto.passwordActual) || string.IsNullOrWhiteSpace(dto.passwordNueva))
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "Ambas contraseñas son requeridas"
                });
            }

            if (dto.passwordNueva.Length < 6)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "La nueva contraseña debe tener al menos 6 caracteres"
                });
            }

            var exito = await _authService.CambiarPassword(usuarioId, dto.passwordActual, dto.passwordNueva);

            if (!exito)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "La contraseña actual es incorrecta o el usuario no existe"
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Contraseña cambiada exitosamente"
            });
        }

        [HttpPatch("usuario/{id}/cambiar-rol")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> CambiarRol(Guid id, [FromBody] CambiarRolDTO dto)
        {
            // Verificar que el usuario objetivo existe
            var usuarioObjetivo = await _authService.ObtenerUsuarioPorId(id);
            if (usuarioObjetivo == null)
            {
                return NotFound(new { error = true, msg = "Usuario no encontrado" });
            }

            bool exito = await _authService.CambiarRol(id, dto.nuevoRol);
            
            if (!exito)
            {
                return BadRequest(new { error = true, msg = "No se pudo cambiar el rol. Verifique que el rol sea válido (Admin, Gerente, Empleado)" });
            }
            
            return Ok(new { 
                error = false, 
                msg = $"Se cambió correctamente el rol del usuario a '{dto.nuevoRol}'" 
            });
        }
    }
}
