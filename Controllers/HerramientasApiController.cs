using ManPower.Data;
using ManPower.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace ManPower.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HerramientasApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HerramientasApiController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/HerramientasApi
        // =====================================================

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Herramientas>>> GetHerramientas()
        {
            return await _context.Herramientas.ToListAsync();
        }

        // =====================================================
        // GET: api/HerramientasApi/1
        // =====================================================

        [HttpGet("{id}")]
        public async Task<ActionResult<Herramientas>> GetHerramienta(int id)
        {
            var herramienta = await _context.Herramientas.FindAsync(id);

            if (herramienta == null)
            {
                return NotFound(new
                {
                    message = "Herramienta no encontrada"
                });
            }

            return herramienta;
        }

        // =====================================================
        // POST
        // =====================================================

        [HttpPost]
        public async Task<ActionResult<Herramientas>> CreateHerramienta([FromBody] Herramientas herramienta)
        {
            if (herramienta == null)
            {
                return BadRequest(new
                {
                    message = "Datos inválidos"
                });
            }

            // VALIDAR DESCRIPCION REPETIDA
            var existe = await _context.Herramientas
                .AnyAsync(h => h.Descripcion == herramienta.Descripcion);

            if (existe)
            {
                return BadRequest(new
                {
                    message = "La herramienta ya existe"
                });
            }

            herramienta.FechaCreacion = DateTime.Now;
            herramienta.Estado = true;

            _context.Herramientas.Add(herramienta);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Herramienta creada correctamente",
                data = herramienta
            });
        }

        // =====================================================
        // PUT
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHerramienta(int id, Herramientas herramienta)
        {
            if (id != herramienta.Id)
            {
                return BadRequest(new
                {
                    message = "El ID no coincide"
                });
            }

            var herramientaExistente = await _context.Herramientas.FindAsync(id);

            if (herramientaExistente == null)
            {
                return NotFound(new
                {
                    message = "Herramienta no encontrada"
                });
            }

            // VALIDAR DESCRIPCION DUPLICADA
            var existe = await _context.Herramientas
                .AnyAsync(h =>
                    h.Descripcion == herramienta.Descripcion &&
                    h.Id != id
                );

            if (existe)
            {
                return BadRequest(new
                {
                    message = "La herramienta ya existe"
                });
            }

            herramientaExistente.Descripcion = herramienta.Descripcion;
            herramientaExistente.UnidadMedida = herramienta.UnidadMedida;
            herramientaExistente.Familia = herramienta.Familia;
            herramientaExistente.Stock = herramienta.Stock;
            herramientaExistente.Precio = herramienta.Precio;
            herramientaExistente.Categoria = herramienta.Categoria;
            herramientaExistente.Estado = herramienta.Estado;

            herramientaExistente.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Herramienta actualizada correctamente"
            });
        }

        // =====================================================
        // DELETE
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHerramienta(int id)
        {
            var herramienta = await _context.Herramientas.FindAsync(id);

            if (herramienta == null)
            {
                return NotFound(new
                {
                    message = "Herramienta no encontrada"
                });
            }

            _context.Herramientas.Remove(herramienta);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Herramienta eliminada correctamente"
            });
        }

        // =====================================================
        // EXPORT EXCEL
        // =====================================================

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportarExcel()
        {
            var herramientas = await _context.Herramientas.ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Herramientas");

            // TITULO
            worksheet.Cell("A1").Value = "REPORTE DE HERRAMIENTAS";
            worksheet.Range("A1:G1").Merge();

            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            worksheet.Cell("A1").Style.Fill.BackgroundColor =
                XLColor.FromHtml("#2563EB");

            worksheet.Cell("A1").Style.Font.FontColor =
                XLColor.White;

            // HEADERS
            worksheet.Cell(3, 1).Value = "Descripción";
            worksheet.Cell(3, 2).Value = "Unidad Medida";
            worksheet.Cell(3, 3).Value = "Familia";
            worksheet.Cell(3, 4).Value = "Stock";
            worksheet.Cell(3, 5).Value = "Precio";
            worksheet.Cell(3, 6).Value = "Categoría";
            worksheet.Cell(3, 7).Value = "Estado";

            var headerRange = worksheet.Range("A3:G3");

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#0F172A");

            headerRange.Style.Font.FontColor = XLColor.White;

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // DATA
            int row = 4;

            foreach (var h in herramientas)
            {
                worksheet.Cell(row, 1).Value = h.Descripcion;
                worksheet.Cell(row, 2).Value = h.UnidadMedida;
                worksheet.Cell(row, 3).Value = h.Familia;
                worksheet.Cell(row, 4).Value = h.Stock;
                worksheet.Cell(row, 5).Value = h.Precio;
                worksheet.Cell(row, 6).Value = h.Categoria;
                worksheet.Cell(row, 7).Value =
                    h.Estado ? "Activo" : "Inactivo";

                row++;
            }

            // FORMATO PRECIO
            worksheet.Column(5).Style.NumberFormat.Format = "S/ #,##0.00";

            // BORDES
            var dataRange = worksheet.Range($"A3:G{row - 1}");

            dataRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            dataRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            // AUTO SIZE
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Herramientas_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }
    }
}