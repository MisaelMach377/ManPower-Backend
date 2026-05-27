using ManPower.Data;
using ManPower.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManPower.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UsuariosApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/UsuariosApi/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    message = "Usuario no encontrado"
                });
            }

            return usuario;
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> CreateUsuario([FromBody] Usuario usuario)
        {
            if (usuario == null)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            // 🔥 VALIDAR CORREO DUPLICADO
            var correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == usuario.Correo);

            if (correoExiste)
            {
                return BadRequest(new
                {
                    message = "Este correo ya está registrado"
                });
            }

            // 🔥 VALIDAR DNI / DOCUMENTO DUPLICADO
            var dniExiste = await _context.Usuarios
                .AnyAsync(u => u.NumeroDocumento == usuario.NumeroDocumento);

            if (dniExiste)
            {
                return BadRequest(new
                {
                    message = "Este DNI ya está registrado"
                });
            }

            // 🔥 si todo ok, recién guardas
            usuario.FechaCreacion = DateTime.Now;
            usuario.Activo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Usuario creado correctamente",
                data = usuario
            });
        }


        // PUT: api/UsuariosApi/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest(new
                {
                    message = "El ID no coincide"
                });
            }

            var usuarioExistente = await _context.Usuarios.FindAsync(id);

            if (usuarioExistente == null)
            {
                return NotFound(new
                {
                    message = "Usuario no encontrado"
                });
            }

            usuarioExistente.Nombre = usuario.Nombre;
            usuarioExistente.Apellido = usuario.Apellido;
            usuarioExistente.TipoDocumento = usuario.TipoDocumento;
            usuarioExistente.NumeroDocumento = usuario.NumeroDocumento;
            usuarioExistente.Correo = usuario.Correo;
            usuarioExistente.Celular = usuario.Celular;
            usuarioExistente.Activo = usuario.Activo;
            usuarioExistente.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Usuario actualizado correctamente"
            });
        }

        // DELETE: api/UsuariosApi/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new
                {
                    message = "Usuario no encontrado"
                });
            }

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Usuario eliminado correctamente"
            });
        }
    }
}