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
    [FromQuery] string orderBy = "FechaVenta",
    [FromQuery] string sortOrder = "asc", // Orden ascendente o descendente
    [FromQuery] string userName = null,
    [FromQuery] string productName = null)
        {
            var query = _context.Sales
                .Include(s => s.User)
                .Include(s => s.SaleDetails).ThenInclude(d => d.Product)
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

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(s => s.User.Nombre.Contains(userName) || s.User.Apellido.Contains(userName));
            }

            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(s => s.SaleDetails.Any(d => d.Product.Nombre.Contains(productName)));
            }

            // Ordenamiento dinámico con soporte para asc/desc
            query = orderBy.ToLower() switch
            {
                "total" => sortOrder == "asc" ? query.OrderBy(s => s.Total) : query.OrderByDescending(s => s.Total),
                "descuento" => sortOrder == "asc" ? query.OrderBy(s => s.Descuento) : query.OrderByDescending(s => s.Descuento),
                "fechaventa" => sortOrder == "asc" ? query.OrderBy(s => s.FechaVenta) : query.OrderByDescending(s => s.FechaVenta),
                _ => sortOrder == "asc" ? query.OrderBy(s => s.FechaVenta) : query.OrderByDescending(s => s.FechaVenta)
            };

            // Paginación después del orden
            var totalRecords = query.Count();
            var sales = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    Id = s.Id,
                    FechaVenta = s.FechaVenta,
                    Total = s.Total,
                    MedioPago = s.MedioPago,
                    EmpleadoId = s.User.Id,
                    User = new { s.User.Id, s.User.Nombre, s.User.Apellido, s.User.Email },
                    Detalles = s.SaleDetails.Select(d => new
                    {
                        Producto = d.Product.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        StockDisponible = d.Product.Stock
                    }).ToList()
                })
                .ToList();

            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Sales = sales
            });
        }


        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.User) // Incluir datos del empleado
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound(new { message = "Venta no encontrada." });
            }

            return Ok(new
            {
                sale.Id,
                sale.FechaVenta,
                sale.Total,
                sale.Descuento,
                sale.MedioPago,  // Incluir MedioPago
                Empleado = new { sale.User.Id, sale.User.Nombre, sale.User.Apellido, sale.User.Email }
            });
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<IActionResult> PostSale(SaleDto saleDto)
        {
            // Validar que el empleado exista
            var empleado = await _context.Users.FindAsync(saleDto.EmpleadoId);
            if (empleado == null)
            {
                return BadRequest(new { message = "El empleado con el ID proporcionado no existe." });
            }

            // Crear la venta
            var sale = new Sale
            {
                FechaVenta = DateTime.Now,
                Total = saleDto.Total,
                EmpleadoId = saleDto.EmpleadoId,
                Descuento = (decimal)saleDto.Descuento,
                MedioPago = saleDto.MedioPago // Asignar el medio de pago
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        // POST: api/Sales/CompleteSale
        [HttpPost("CompleteSale")]
        public async Task<IActionResult> PostCompleteSale([FromBody] CompleteSaleDto completeSaleDto)
        {
            if (completeSaleDto == null || completeSaleDto.Detalles == null || completeSaleDto.Detalles.Count == 0)
            {
                return BadRequest(new { message = "Los detalles de la venta son inválidos." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Crear la venta
                var sale = new Sale
                {
                    EmpleadoId = completeSaleDto.EmpleadoId,
                    Total = completeSaleDto.Total,
                    Descuento = completeSaleDto.Descuento ?? 0, // Usar 0 si Descuento es null
                    FechaVenta = DateTime.Now,
                    MedioPago = completeSaleDto.MedioPago ?? "Efectivo" // Usar "Efectivo" si MedioPago es null
                };
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync(); // Asigna el ID de la venta

                // Procesar los detalles de la venta
                foreach (var detailDto in completeSaleDto.Detalles)
                {
                    // Verificar si el producto existe
                    var product = await _context.Products.FindAsync(detailDto.ProductoId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Producto con ID {detailDto.ProductoId} no encontrado." });
                    }

                    // Verificar si hay suficiente stock
                    if (product.Stock < detailDto.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Stock insuficiente para el producto con ID {detailDto.ProductoId}. Stock actual: {product.Stock}, Cantidad solicitada: {detailDto.Cantidad}" });
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

                // Guardar cambios y confirmar transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Mapear la entidad Sale a SaleDto para la respuesta
                var saleDto = new SaleDto
                {
                    Id = sale.Id,
                    FechaVenta = sale.FechaVenta,
                    Total = sale.Total,
                    EmpleadoId = sale.EmpleadoId,
                    Descuento = sale.Descuento,
                    MedioPago = sale.MedioPago,
                    SaleDetails = sale.SaleDetails.Select(sd => new SaleDetailDto
                    {
                        Id = sd.Id,
                        VentaId = sd.VentaId,
                        ProductoId = sd.ProductoId,
                        Cantidad = sd.Cantidad,
                        PrecioUnitario = sd.PrecioUnitario
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, saleDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine   ("Error en CompleteSale: {Message}", ex.Message); // Log del error
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
