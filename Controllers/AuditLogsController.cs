using Microsoft.AspNetCore.Mvc;
using Kiosco.Data;
using Kiosco.Models;
using Microsoft.EntityFrameworkCore;
using Kiosco.DTOs;

namespace Kiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            var logs = await _context.AuditLogs.ToListAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null)
            {
                return NotFound();
            }
            return auditLog;
        }

        // POST: api/AuditLogs
        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLogDto auditLogDto)
        {
            // Mapear el DTO al modelo de entidad
            var auditLog = new AuditLog
            {
                VentaId = auditLogDto.VentaId,
                EmpleadoId = auditLogDto.EmpleadoId,
                FechaModificacion = DateTime.Now,
                Accion = auditLogDto.Accion
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAuditLog), new { id = auditLog.Id }, auditLog);
        }

        // PUT: api/AuditLogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuditLog(int id, UpdateAuditLogDto updateAuditLogDto)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);

            if (auditLog == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos proporcionados
            if (!string.IsNullOrEmpty(updateAuditLogDto.Accion))
            {
                auditLog.Accion = updateAuditLogDto.Accion;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.AuditLogs.Any(e => e.Id == id))
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

        // DELETE: api/AuditLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditLog(int id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null)
            {
                return NotFound();
            }

            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
