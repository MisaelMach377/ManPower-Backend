using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManPower.Data;
using ManPower.Modelos;

namespace ManPower.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsignacionesApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AsignacionesApiController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTAR
        // ==========================================

        [HttpGet]
        public async Task<ActionResult> GetAsignaciones()
        {
            var lista = await _context.Asignaciones
                .Include(a => a.Usuario)
                .Include(a => a.Celular)
                .Include(a => a.Laptop)
                .Select(a => new
                {
                    a.Id,

                    UsuarioId = a.UsuarioId,

                    Usuario = a.Usuario.Nombre + " " + a.Usuario.Apellido,

                    Documento = a.Usuario.NumeroDocumento,

                    a.TipoHerramienta,

                    Celular = a.Celular != null
                        ? a.Celular.Marca + " " + a.Celular.Modelo
                        : null,

                    Laptop = a.Laptop != null
                        ? a.Laptop.Marca + " " + a.Laptop.Modelo
                        : null,

                    a.NumeroGuia,
                    a.Zona,
                    a.Estado,
                    a.FechaAsignacion,
                    a.Observaciones
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ==========================================
        // OBTENER POR ID
        // ==========================================

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAsignacion(int id)
        {
            var asignacion = await _context.Asignaciones
                .Include(a => a.Usuario)
                .Include(a => a.Celular)
                .Include(a => a.Laptop)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacion == null)
            {
                return NotFound(new
                {
                    message = "Asignación no encontrada"
                });
            }

            return Ok(asignacion);
        }

        // ==========================================
        // CREAR
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> CreateAsignacion([FromBody] Asignaciones asignacion)
        {
            Console.WriteLine("===== NUEVA ASIGNACION =====");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(asignacion));

            if (asignacion == null)
            {
                Console.WriteLine("ERROR: asignacion null");

                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos"
                });
            }

            // VALIDAR USUARIO
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.Id == asignacion.UsuarioId);

            Console.WriteLine($"UsuarioId recibido: {asignacion.UsuarioId}");
            Console.WriteLine($"Usuario existe: {usuarioExiste}");

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Usuario no existe"
                });
            }

            // VALIDACIÓN TIPO
            Console.WriteLine($"Tipo herramienta: {asignacion.TipoHerramienta}");

            if (asignacion.TipoHerramienta == "CELULAR")
            {
                Console.WriteLine($"CelularId: {asignacion.CelularId}");

                if (asignacion.CelularId == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe seleccionar un celular"
                    });
                }
            }

            if (asignacion.TipoHerramienta == "LAPTOP")
            {
                Console.WriteLine($"LaptopId: {asignacion.LaptopId}");

                if (asignacion.LaptopId == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe seleccionar una laptop"
                    });
                }
            }

            asignacion.FechaAsignacion = DateTime.Now;

            _context.Asignaciones.Add(asignacion);
            await _context.SaveChangesAsync();

            Console.WriteLine("Asignación creada OK");

            return Ok(new
            {
                success = true,
                message = "Asignación creada correctamente",
                data = asignacion
            });
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsignacion(int id, [FromBody] Asignaciones asignacion)
        {
            if (id != asignacion.Id)
            {
                return BadRequest(new
                {
                    message = "ID incorrecto"
                });
            }

            var asignacionExistente = await _context.Asignaciones.FindAsync(id);

            if (asignacionExistente == null)
            {
                return NotFound(new
                {
                    message = "Asignación no encontrada"
                });
            }

            asignacionExistente.UsuarioId = asignacion.UsuarioId;
            asignacionExistente.TipoHerramienta = asignacion.TipoHerramienta;
            asignacionExistente.CelularId = asignacion.CelularId;
            asignacionExistente.LaptopId = asignacion.LaptopId;
            asignacionExistente.NumeroGuia = asignacion.NumeroGuia;
            asignacionExistente.Zona = asignacion.Zona;
            asignacionExistente.Estado = asignacion.Estado;
            asignacionExistente.Observaciones = asignacion.Observaciones;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Asignación actualizada correctamente"
            });
        }

        // ==========================================
        // ELIMINAR
        // ==========================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsignacion(int id)
        {
            var asignacion = await _context.Asignaciones.FindAsync(id);

            if (asignacion == null)
            {
                return NotFound(new
                {
                    message = "Asignación no encontrada"
                });
            }

            _context.Asignaciones.Remove(asignacion);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Asignación eliminada correctamente"
            });
        }
    }
}