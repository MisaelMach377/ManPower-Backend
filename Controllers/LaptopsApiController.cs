using ClosedXML.Excel;
using ManPower.Data;
using ManPower.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManPower.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaptopsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LaptopsApiController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Laptops>>> GetLaptops()
        {
            return await _context.Laptops.ToListAsync();
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<ActionResult<Laptops>> GetLaptop(int id)
        {
            var laptop = await _context.Laptops.FindAsync(id);

            if (laptop == null)
            {
                return NotFound(new { message = "Laptop no encontrada" });
            }

            return laptop;
        }

        // =========================
        // POST
        // =========================
        [HttpPost]
        public async Task<ActionResult<Laptops>> CreateLaptop([FromBody] Laptops laptop)
        {
            if (laptop == null)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            _context.Laptops.Add(laptop);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Laptop creada correctamente",
                data = laptop
            });
        }

        // =========================
        // PUT
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLaptop(int id, Laptops laptop)
        {
            if (id != laptop.Id)
            {
                return BadRequest(new { message = "El ID no coincide" });
            }

            var existing = await _context.Laptops.FindAsync(id);

            if (existing == null)
            {
                return NotFound(new { message = "Laptop no encontrada" });
            }

            existing.Marca = laptop.Marca;
            existing.Modelo = laptop.Modelo;
            existing.Serie = laptop.Serie;
            existing.Proveedor = laptop.Proveedor;
            existing.Observaciones = laptop.Observaciones;
            existing.Estado = laptop.Estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Laptop actualizada correctamente"
            });
        }

        // =========================
        // DELETE
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLaptop(int id)
        {
            var laptop = await _context.Laptops.FindAsync(id);

            if (laptop == null)
            {
                return NotFound(new { message = "Laptop no encontrada" });
            }

            _context.Laptops.Remove(laptop);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Laptop eliminada correctamente"
            });
        }

        // =========================
        // EXPORT EXCEL
        // =========================
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportarExcel()
        {
            var laptops = await _context.Laptops.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Laptops");

            // =========================
            // TITULO
            // =========================
            worksheet.Cell("A1").Value = "REPORTE DE LAPTOPS";
            worksheet.Range("A1:F1").Merge();

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
            worksheet.Cell(3, 4).Value = "Serie";
            worksheet.Cell(3, 5).Value = "Proveedor";
            worksheet.Cell(3, 6).Value = "Estado";

            var headerRange = worksheet.Range("A3:F3");

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

            foreach (var l in laptops)
            {
                worksheet.Cell(row, 1).Value = l.Id;
                worksheet.Cell(row, 2).Value = l.Marca;
                worksheet.Cell(row, 3).Value = l.Modelo;
                worksheet.Cell(row, 4).Value = l.Serie;
                worksheet.Cell(row, 5).Value = l.Proveedor;
                worksheet.Cell(row, 6).Value = l.Estado;

                row++;
            }

            // =========================
            // BORDES
            // =========================
            var dataRange = worksheet.Range($"A3:F{row - 1}");

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
                $"Laptops_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }
    }
}