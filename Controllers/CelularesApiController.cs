using ClosedXML.Excel;
using ManPower.Data;
using ManPower.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManPower.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CelularesApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CelularesApiController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/CelularesApi
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Celulares>>> GetCelulares()
        {
            return await _context.Celulares.ToListAsync();
        }

        // =====================================================
        // GET: api/CelularesApi/1
        // =====================================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Celulares>> GetCelular(int id)
        {
            var celular = await _context.Celulares.FindAsync(id);

            if (celular == null)
            {
                return NotFound(new
                {
                    message = "Celular no encontrado"
                });
            }

            return celular;
        }

        // =====================================================
        // POST
        // =====================================================
        [HttpPost]
        public async Task<ActionResult<Celulares>> CreateCelular([FromBody] Celulares celular)
        {
            if (celular == null)
            {
                return BadRequest(new
                {
                    message = "Datos inválidos"
                });
            }

            // VALIDAR IMEI DUPLICADO
            var existe = await _context.Celulares
                .AnyAsync(c => c.IMEI == celular.IMEI);

            if (existe)
            {
                return BadRequest(new
                {
                    message = "El IMEI ya existe"
                });
            }

            _context.Celulares.Add(celular);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Celular creado correctamente",
                data = celular
            });
        }

        // =====================================================
        // PUT
        // =====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCelular(int id, Celulares celular)
        {
            if (id != celular.ID)
            {
                return BadRequest(new
                {
                    message = "El ID no coincide"
                });
            }

            var celularExistente = await _context.Celulares.FindAsync(id);

            if (celularExistente == null)
            {
                return NotFound(new
                {
                    message = "Celular no encontrado"
                });
            }

            // VALIDAR IMEI DUPLICADO
            var existe = await _context.Celulares
                .AnyAsync(c =>
                    c.IMEI == celular.IMEI &&
                    c.ID != id
                );

            if (existe)
            {
                return BadRequest(new
                {
                    message = "El IMEI ya existe"
                });
            }

            celularExistente.Marca = celular.Marca;
            celularExistente.Modelo = celular.Modelo;
            celularExistente.IMEI = celular.IMEI;
            celularExistente.Operacion = celular.Operacion;
            celularExistente.Celular = celular.Celular;
            celularExistente.Proveedor = celular.Proveedor;
            celularExistente.Estado = celular.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Celular actualizado correctamente"
            });
        }

        // =====================================================
        // DELETE
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCelular(int id)
        {
            var celular = await _context.Celulares.FindAsync(id);

            if (celular == null)
            {
                return NotFound(new
                {
                    message = "Celular no encontrado"
                });
            }

            _context.Celulares.Remove(celular);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Celular eliminado correctamente"
            });
        }

        // =====================================================
        // EXPORTAR A EXCEL
        // =====================================================

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportarExcel()
        {
            var celulares = await _context.Celulares.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Celulares");

            // =========================
            // TITULO
            // =========================
            worksheet.Cell("A1").Value = "REPORTE DE CELULARES";
            worksheet.Range("A1:H1").Merge();

            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            worksheet.Cell("A1").Style.Fill.BackgroundColor =
                XLColor.FromHtml("#2563EB");

            worksheet.Cell("A1").Style.Font.FontColor = XLColor.White;

            // =========================
            // HEADERS
            // =========================
            worksheet.Cell(3, 1).Value = "ID";
            worksheet.Cell(3, 2).Value = "Marca";
            worksheet.Cell(3, 3).Value = "Modelo";
            worksheet.Cell(3, 4).Value = "IMEI";
            worksheet.Cell(3, 5).Value = "Operación";
            worksheet.Cell(3, 6).Value = "Celular";
            worksheet.Cell(3, 7).Value = "Proveedor";
            worksheet.Cell(3, 8).Value = "Estado";

            var headerRange = worksheet.Range("A3:H3");

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#0F172A");

            headerRange.Style.Font.FontColor = XLColor.White;

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // =========================
            // DATA
            // =========================
            int row = 4;

            foreach (var c in celulares)
            {
                worksheet.Cell(row, 1).Value = c.ID;
                worksheet.Cell(row, 2).Value = c.Marca;
                worksheet.Cell(row, 3).Value = c.Modelo;
                worksheet.Cell(row, 4).Value = c.IMEI;
                worksheet.Cell(row, 5).Value = c.Operacion;
                worksheet.Cell(row, 6).Value = c.Celular;
                worksheet.Cell(row, 7).Value = c.Proveedor;
                worksheet.Cell(row, 8).Value = c.Estado;

                row++;
            }

            // =========================
            // BORDES
            // =========================
            var dataRange = worksheet.Range($"A3:H{row - 1}");

            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // =========================
            // AUTO SIZE
            // =========================
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Celulares_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }


    }
}