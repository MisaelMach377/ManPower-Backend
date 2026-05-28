using ManPower.Data;
using ManPower.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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


        // ================================================
        // ================================================


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

        // ================================================
        // ================================================


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


        // ================================================
        // ================================================


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

            // 🔥 VALIDAR CORREO REPETIDO
            var correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == usuario.Correo && u.Id != id);

            if (correoExiste)
            {
                return BadRequest(new
                {
                    message = "Este correo ya está registrado"
                });
            }

            // 🔥 VALIDAR DOCUMENTO REPETIDO
            var dniExiste = await _context.Usuarios
                .AnyAsync(u => u.NumeroDocumento == usuario.NumeroDocumento && u.Id != id);

            if (dniExiste)
            {
                return BadRequest(new
                {
                    message = "Este DNI ya está registrado"
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


        // ================================================
        // ================================================


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



        // ================================================
        // ================================================

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportarExcel()
        {
            var usuarios = await _context.Usuarios.ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Usuarios");

            // TITULO
            worksheet.Cell("A1").Value = "REPORTE DE USUARIOS";
            worksheet.Range("A1:H1").Merge();

            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            worksheet.Cell("A1").Style.Font.FontColor = XLColor.White;

            // HEADERS
            worksheet.Cell(3, 1).Value = "Nombre";
            worksheet.Cell(3, 2).Value = "Apellido";
            worksheet.Cell(3, 3).Value = "Tipo Documento";
            worksheet.Cell(3, 4).Value = "N° Documento";
            worksheet.Cell(3, 5).Value = "Correo";
            worksheet.Cell(3, 6).Value = "Celular";
            worksheet.Cell(3, 7).Value = "Estado";
            worksheet.Cell(3, 8).Value = "Fecha Creación";

            var headerRange = worksheet.Range("A3:H3");

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // DATA
            int row = 4;

            foreach (var u in usuarios)
            {
                worksheet.Cell(row, 1).Value = u.Nombre;
                worksheet.Cell(row, 2).Value = u.Apellido;
                worksheet.Cell(row, 3).Value = u.TipoDocumento;
                worksheet.Cell(row, 4).Value = u.NumeroDocumento;
                worksheet.Cell(row, 5).Value = u.Correo;
                worksheet.Cell(row, 6).Value = u.Celular;
                worksheet.Cell(row, 7).Value = u.Activo ? "Activo" : "Inactivo";
                worksheet.Cell(row, 8).Value = u.FechaCreacion.ToString("dd/MM/yyyy");

                row++;
            }

            // BORDES
            var dataRange = worksheet.Range($"A3:H{row - 1}");

            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // AUTO SIZE
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }

    }
}