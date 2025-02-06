using Microsoft.AspNetCore.Mvc;
using Kiosco.Data;
using Kiosco.Models;
using Kiosco.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Kiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
        }

        // GET: api/Products/5
        [Authorize(Roles = "Jefe")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDto productDto)
        {
            // Mapear el DTO al modelo de entidad
            var product = new Product
            {
                Nombre = productDto.Nombre,
                Descripcion = productDto.Descripcion,
                Precio = productDto.Precio,
                Stock = productDto.Stock,
                CategoriaId = productDto.CategoriaId,
                ProveedorId = productDto.ProveedorId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos proporcionados
            product.Nombre = updateProductDto.Nombre;

            if (updateProductDto.Descripcion != null)
            {
                product.Descripcion = updateProductDto.Descripcion;
            }

            if (updateProductDto.Precio.HasValue)
            {
                product.Precio = updateProductDto.Precio.Value;
            }

            if (updateProductDto.Stock.HasValue)
            {
                product.Stock = updateProductDto.Stock.Value;
            }

            if (updateProductDto.CategoriaId.HasValue)
            {
                product.CategoriaId = updateProductDto.CategoriaId.Value;
            }

            if (updateProductDto.ProveedorId.HasValue)
            {
                product.ProveedorId = updateProductDto.ProveedorId.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
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

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
