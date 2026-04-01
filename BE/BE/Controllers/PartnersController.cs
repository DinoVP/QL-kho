using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrmPartnersController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public CrmPartnersController(QLKhoContext context)
        {
            _context = context;
        }

        // =========================================================================
        // CAMERA AN NINH TỰ ĐỘNG
        // =========================================================================
        private async Task WriteAuditLogAsync(string actionType, string tableName, string details = "")
        {
            try
            {
                int? currentUserId = null;
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                               ?? User.FindFirst("UserId")
                               ?? User.FindFirst("id");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedId))
                {
                    currentUserId = parsedId;
                }

                var log = new SysAuditLog
                {
                    UserId = currentUserId,
                    ActionType = actionType,
                    TableName = tableName,
                    Details = details,
                    LogDate = DateTime.Now
                };

                _context.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("==== LỖI GHI LOG: " + ex.InnerException?.Message ?? ex.Message);
            }
        }
        // =========================================================================

        // GET: api/CrmPartners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CrmPartner>>> GetPartners()
        {
            var partners = await _context.CrmPartners.OrderByDescending(p => p.PartnerId).ToListAsync();
            return Ok(new { Total = partners.Count, Data = partners });
        }

        // POST: api/CrmPartners
        [HttpPost]
        public async Task<ActionResult<CrmPartner>> CreatePartner(CrmPartner partner)
        {
            if (string.IsNullOrEmpty(partner.Status)) partner.Status = "active";

            _context.CrmPartners.Add(partner);
            await _context.SaveChangesAsync();

            // 🟢 GHI LOG TỰ ĐỘNG
            string details = $"Tạo mới đối tác {partner.PartnerName} với trạng thái [Đang giao dịch].";
            await WriteAuditLogAsync("INSERT", $"Đối tác: Thêm mới {partner.PartnerName} ({partner.PartnerCode})", details);

            return Ok(partner);
        }

        // PUT: api/CrmPartners/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePartner(int id, CrmPartner partner)
        {
            if (id != partner.PartnerId) return BadRequest("ID không khớp.");

            // Lấy thông tin cũ để so sánh
            var oldPartner = await _context.CrmPartners.AsNoTracking().FirstOrDefaultAsync(p => p.PartnerId == id);
            if (oldPartner == null) return NotFound();

            _context.Entry(partner).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // 🟡 GHI LOG CHI TIẾT TỰ ĐỘNG
            string details = $"Cập nhật thông tin đối tác {partner.PartnerName}.";

            if (oldPartner.Status != partner.Status)
            {
                string oldStatusStr = oldPartner.Status == "active" ? "Đang giao dịch" : "Ngừng giao dịch";
                string newStatusStr = partner.Status == "active" ? "Đang giao dịch" : "Ngừng giao dịch";
                details = $"Đã thay đổi trạng thái đối tác từ [{oldStatusStr}] sang [{newStatusStr}].";
            }

            await WriteAuditLogAsync("UPDATE", $"Đối tác: Cập nhật {partner.PartnerName}", details);

            return Ok(partner);
        }

        // DELETE: api/CrmPartners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartner(int id)
        {
            var partner = await _context.CrmPartners.FindAsync(id);
            if (partner == null) return NotFound();

            string partnerName = partner.PartnerName;

            _context.CrmPartners.Remove(partner);
            await _context.SaveChangesAsync();

            // 🔴 GHI LOG TỰ ĐỘNG
            await WriteAuditLogAsync("DELETE", $"Đối tác: Đã xóa {partnerName}", $"Đã xóa đối tác {partnerName} khỏi hệ thống.");

            return Ok();
        }
    }
}