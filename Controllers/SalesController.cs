using Microsoft.AspNetCore.Mvc;
using Kiosco.Data;
using Kiosco.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Kiosco.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Kiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protege todos los endpoints del controlador
    public class SalesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales?pageNumber=1&pageSize=5&minTotal=100&maxTotal=500&orderBy=FechaVenta
        [HttpGet]
        public IActionResult GetSales(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] decimal? minTotal = null,
            [FromQuery] decimal? maxTotal = null,
            [FromQuery] string orderBy = "FechaVenta") // Ordenamiento por defecto
        {
            // Validar que pageNumber y pageSize sean mayores que 0
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "pageNumber y pageSize deben ser mayores que 0." });
            }

            // Construir la consulta base
            var query = _context.Sales
                .Include(s => s.User) // Incluir datos relacionados del usuario
                .AsQueryable();

            // Aplicar filtros opcionales
            if (minTotal.HasValue)
            {
                query = query.Where(s => s.Total >= minTotal.Value);
            }

            if (maxTotal.HasValue)
            {
                query = query.Where(s => s.Total <= maxTotal.Value);
            }

            // Aplicar ordenamiento dinámico
            query = orderBy.ToLower() switch
            {
                "total" => query.OrderBy(s => s.Total),
                "descuento" => query.OrderBy(s => s.Descuento),
                "fechaventa" => query.OrderBy(s => s.FechaVenta),
                _ => query.OrderBy(s => s.FechaVenta) // Por defecto
            };

            // Calcular el número total de registros
            var totalRecords = query.Count();

            // Obtener los registros paginados
            var sales = query
                .Skip((pageNumber - 1) * pageSize) // Saltar los registros anteriores
                .Take(pageSize) // Tomar los registros de la página actual
                .ToList();

            // Construir la respuesta
            var response = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Sales = sales
            };

            return Ok(response);
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Sale>> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound(new { message = "Venta no encontrada." });
            }

            return Ok(sale);
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<ActionResult<Sale>> PostSale(SaleDto saleDto)
        {
            // Mapear el DTO al modelo de entidad
            var sale = new Sale
            {
                FechaVenta = DateTime.Now,
                Total = saleDto.Total,
                EmpleadoId = saleDto.EmpleadoId,
                Descuento = saleDto.Descuento
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        // POST: api/Sales/CompleteSale
        [HttpPost("CompleteSale")]
        public async Task<IActionResult> PostCompleteSale(CompleteSaleDto completeSaleDto)
        {
            // Iniciar una transacción para asegurar la atomicidad de la operación
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Crear el encabezado de la venta
                var sale = new Sale
                {
                    EmpleadoId = completeSaleDto.EmpleadoId,
                    Total = completeSaleDto.Total,
                    Descuento = completeSaleDto.Descuento,
                    FechaVenta = DateTime.Now
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync(); // Se asigna el Id a la venta

                // 2. Procesar cada detalle de la venta
                foreach (var detailDto in completeSaleDto.Detalles)
                {
                    // Verificar que el producto exista
                    var product = await _context.Products.FindAsync(detailDto.ProductoId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Producto con ID {detailDto.ProductoId} no encontrado." });
                    }

                    // Verificar que hay stock suficiente
                    if (product.Stock < detailDto.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Stock insuficiente para el producto {product.Nombre}. Stock disponible: {product.Stock}" });
                    }

                    // Actualizar el stock del producto
                    product.Stock -= detailDto.Cantidad;
                    _context.Products.Update(product);

                    // Registrar el detalle de la venta
                    var saleDetail = new SaleDetail
                    {
                        VentaId = sale.Id,
                        ProductoId = detailDto.ProductoId,
                        Cantidad = detailDto.Cantidad,
                        PrecioUnitario = detailDto.PrecioUnitario
                    };
                    _context.SaleDetails.Add(saleDetail);
                }

                // Guardar cambios: encabezado, detalles y actualización de stock
                await _context.SaveChangesAsync();

                // (Opcional) Registrar auditoría
                var auditLog = new AuditLog
                {
                    VentaId = sale.Id,
                    EmpleadoId = completeSaleDto.EmpleadoId,
                    FechaModificacion = DateTime.Now,
                    Accion = "Venta completa registrada y stock actualizado"
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                // Confirmar la transacción
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al registrar la venta", error = ex.Message });
            }
        }

        // PUT: api/Sales/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSale(int id, UpdateSaleDto updateSaleDto)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound(new { message = "Venta no encontrada." });
            }

            // Actualizar solo los campos proporcionados
            if (updateSaleDto.Descuento.HasValue)
            {
                sale.Descuento = updateSaleDto.Descuento.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Sales.Any(e => e.Id == id))
                {
                    return NotFound(new { message = "Venta no encontrada." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound(new { message = "Venta no encontrada." });
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
