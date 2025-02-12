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
        /// <summary>
        /// Obtiene una lista de todos los productos con sus categorías asociadas.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category) // Incluye la categoría relacionada
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/Products/5
        /// <summary>
        /// Obtiene un producto específico por su ID.
        /// </summary>
        [Authorize(Roles = "Jefe")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category) // Incluye la categoría relacionada
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });
            }

            return Ok(product);
        }

        // POST: api/Products
        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDto productDto)
        {
            // Validar que la categoría exista
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == productDto.CategoriaId);
            if (!categoryExists)
            {
                return BadRequest(new { message = $"La categoría con ID {productDto.CategoriaId} no existe." });
            }

            // Mapear el DTO al modelo de entidad
            var product = new Product
            {
                Nombre = productDto.Nombre,
                Descripcion = productDto.Descripcion,
                Precio = productDto.Precio,
                Stock = productDto.Stock,
                CategoriaId = productDto.CategoriaId,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/Products/5
        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });
            }

            // Validar que la categoría exista si se proporciona un nuevo ID
            if (updateProductDto.CategoriaId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoriaId.Value);
                if (!categoryExists)
                {
                    return BadRequest(new { message = $"La categoría con ID {updateProductDto.CategoriaId.Value} no existe." });
                }
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                {
                    return NotFound(new { message = $"Producto con ID {id} no encontrado." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Products/5
        /// <summary>
        /// Elimina un producto existente.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}