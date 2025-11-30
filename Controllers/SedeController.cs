using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ParkSmart;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SedeController : ControllerBase
    {
        private readonly ISedeService _sedeService;
        private readonly IValidator<CrearSedeDTO> _crearSedeValidator;
        private readonly IAccesoSedeService _accesoSedeService;

        public SedeController(
            ISedeService sedeService, 
            IValidator<CrearSedeDTO> crearSedeValidator,
            IAccesoSedeService accesoSedeService)
        {
            _sedeService = sedeService;
            _crearSedeValidator = crearSedeValidator;
            _accesoSedeService = accesoSedeService;
        }

        private IActionResult VerificarAccesoUsuario(Guid sedeId)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized(new { error = true, msg = "No se pudo obtener el ID del usuario del token" });
            }

            var usuarioId = Guid.Parse(userIdClaim.Value);

            if (!_accesoSedeService.TieneAcceso(usuarioId, sedeId))
            {
                return Unauthorized(new
                {
                    error = true,
                    msg = "No tienes acceso a esta sede. Por favor verifica la contraseña primero usando /api/sede/verificar-acceso"
                });
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var sedes = await _sedeService.ObtenerTodasLasSedes();
            return Ok(new{
                error= false,
                msg = "Se encontraron sedes",
                data = sedes
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(Guid id)
        {
            var sede = await _sedeService.ObtenerSedePorId(id);

            if (sede == null) return NotFound(new { error = true, msg = "Sede no encontrada" });

            return Ok(new {
                error = false, 
                msg = "Se encontró la sede",
                data = sede
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> Crear([FromBody] CrearSedeDTO crearSede)
        {
            var validationResult = await _crearSedeValidator.ValidateAsync(crearSede);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "Errores de validación",
                    errores = validationResult.Errors.Select(e => new
                    {
                        campo = e.PropertyName,
                        error = e.ErrorMessage
                    })
                });
            }

            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");
            
            if (userIdClaim == null)
            {
                return Unauthorized(new { error = true, msg = "No se pudo obtener el ID del usuario del token" });
            }

            var usuarioId = Guid.Parse(userIdClaim.Value);

            var sedeCreada = await _sedeService.CrearSede(crearSede, usuarioId);

            if (sedeCreada == null)
            {
                return BadRequest(new { 
                    error = true,
                    msg = "Error al crear la sede" });
            }

            return Ok(new { 
                error = false,
                msg = "Sede creada exitosamente", 
                sede = new SedeResponseDTO(sedeCreada)
            });
        }

        [HttpPost("verificar-acceso")]
        public async Task<IActionResult> VerificarAcceso([FromBody] VerificarAccesoSedeDTO verificar)
        {
            var accesoPermitido = await _sedeService.VerificarAccesoSede(verificar.sedeId, verificar.passwordAcceso);

            if (!accesoPermitido)
            {
                return Unauthorized(new
                {
                    error = true,
                    msg = "Contraseña de acceso incorrecta"
                });
            }

            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");
            
            if (userIdClaim == null)
            {
                return Unauthorized(new { error = true, msg = "No se pudo obtener el ID del usuario del token" });
            }

            var usuarioId = Guid.Parse(userIdClaim.Value);

            _accesoSedeService.RegistrarAcceso(usuarioId, verificar.sedeId);

            return Ok(new
            {
                error = false,
                msg = "Acceso permitido y registrado",
                sedeId = verificar.sedeId,
                usuarioId = usuarioId,
                validoHasta = FechaHelper.AhoraLocal().AddHours(8)
            });
        }

        [HttpGet("{id}/estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas(Guid id)
        {
            var errorAcceso = VerificarAccesoUsuario(id);
            if (errorAcceso != null) return Unauthorized(new
                {
                    error = true,
                    msg = "No tienes acceso a esta sede. Por favor verifica la contraseña primero usando /api/sede/verificar-acceso"
                });;

            var estadisticas = await _sedeService.ObtenerEstadisticasSede(id);

            if (estadisticas == null) return NotFound(new { 
                error = true, 
                msg = "Sede no encontrada" 
            });

            return Ok(new {
                error = false,
                msg = "Se calcularon estadisticas",
                data = estadisticas
            });
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoSedeDTO dto)
        {
            var exito = await _sedeService.ActualizarEstadoSede(id, dto.nuevoEstado, dto.contraseñaCreador);

            if (!exito)
            {
                return BadRequest(new { error = true, msg = "Verifique el estado o contraseña." });
            }

            return Ok(new { 
                error = false, 
                msg = "Estado actualizado correctamente" 
            });
        }

        [HttpGet("{id}/niveles")]
        public async Task<IActionResult> ObtenerNiveles(Guid id)
        {
            var errorAcceso = VerificarAccesoUsuario(id);
            if (errorAcceso != null) return Unauthorized(new
                {
                    error = true,
                    msg = "No tienes acceso a esta sede. Por favor verifica la contraseña primero usando /api/sede/verificar-acceso"
                });

            var niveles = await _sedeService.ObtenerNivelesDeSede(id);

            return Ok(new
            {
                error = false,
                msg = "Niveles obtenidos exitosamente",
                data = niveles
            });
        }

        [HttpPut("{id}/actualizar-completa")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> ActualizarSedeCompleta(Guid id, [FromBody] ActualizarSedeCompletaDTO dto)
        {
            try
            {
                var sedeActualizada = await _sedeService.ActualizarSedeCompleta(id, dto);

                if (sedeActualizada == null)
                {
                    return BadRequest(new
                    {
                        error = true,
                        msg = "Error: la sede no existe o la contraseña del creador es incorrecta"
                    });
                }

                return Ok(new
                {
                    error = false,
                    msg = "Sede actualizada exitosamente"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    error = true,
                    msg = "Error interno al actualizar la sede. Verifique los datos e intente nuevamente."
                });
            }
        }
    }
}
