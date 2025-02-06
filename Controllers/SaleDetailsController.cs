using Microsoft.AspNetCore.Mvc;
using Kiosco.Data;
using Kiosco.Models;
using Microsoft.EntityFrameworkCore;
using Kiosco.DTOs;

namespace Kiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SaleDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SaleDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDetail>>> GetSaleDetails()
        {
            return await _context.SaleDetails
                .Include(sd => sd.Sale)
                .Include(sd => sd.Product)
                .ToListAsync();
        }

        // GET: api/SaleDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDetail>> GetSaleDetail(int id)
        {
            var saleDetail = await _context.SaleDetails
                .Include(sd => sd.Sale)
                .Include(sd => sd.Product)
                .FirstOrDefaultAsync(sd => sd.Id == id);

            if (saleDetail == null)
            {
                return NotFound();
            }

            return saleDetail;
        }

        // POST: api/SaleDetails
        [HttpPost]
        public async Task<ActionResult<SaleDetail>> PostSaleDetail(SaleDetailDto saleDetailDto)
        {
            // Mapear el DTO al modelo de entidad
            var saleDetail = new SaleDetail
            {
                VentaId = saleDetailDto.VentaId,
                ProductoId = saleDetailDto.ProductoId,
                Cantidad = saleDetailDto.Cantidad,
                PrecioUnitario = saleDetailDto.PrecioUnitario
            };

            _context.SaleDetails.Add(saleDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSaleDetail), new { id = saleDetail.Id }, saleDetail);
        }

        // PUT: api/SaleDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSaleDetail(int id, UpdateSaleDetailDto updateSaleDetailDto)
        {
            var saleDetail = await _context.SaleDetails.FindAsync(id);

            if (saleDetail == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos proporcionados
            if (updateSaleDetailDto.Cantidad.HasValue)
            {
                saleDetail.Cantidad = updateSaleDetailDto.Cantidad.Value;
            }

            if (updateSaleDetailDto.PrecioUnitario.HasValue)
            {
                saleDetail.PrecioUnitario = updateSaleDetailDto.PrecioUnitario.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SaleDetails.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/SaleDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSaleDetail(int id)
        {
            var saleDetail = await _context.SaleDetails.FindAsync(id);
            if (saleDetail == null)
            {
                return NotFound();
            }

            _context.SaleDetails.Remove(saleDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
