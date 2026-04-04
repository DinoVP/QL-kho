using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BinTransferController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public BinTransferController(QLKhoContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> TransferBin([FromBody] BinTransferDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm lô hàng ở Kệ Nguồn (Khu chờ nhập)
                var fromStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                    s.VariantId == req.VariantId &&
                    s.LocationId == req.FromLocationId &&
                    s.Nsx == req.Nsx &&
                    s.Hsd == req.Hsd);

                if (fromStock == null || fromStock.Quantity < req.Qty)
                    return BadRequest(new { message = "Lỗi: Số lượng tồn trên Kệ nguồn không đủ hoặc lô hàng không khớp NSX/HSD!" });

                // 2. Trừ tồn ở Kệ Nguồn
                fromStock.Quantity -= req.Qty;
                int warehouseId = fromStock.WarehouseId ?? 1; // Giữ lại ID kho để gán cho kệ mới

                if (fromStock.Quantity <= 0)
                {
                    _context.WmsStockBalances.Remove(fromStock);
                }

                // 3. Cộng tồn vào Kệ Đích (Kệ chính thức)
                var toStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                    s.VariantId == req.VariantId &&
                    s.LocationId == req.ToLocationId &&
                    s.Nsx == req.Nsx &&
                    s.Hsd == req.Hsd);

                if (toStock != null)
                {
                    toStock.Quantity += req.Qty;
                }
                else
                {
                    // Nếu kệ đích chưa có lô này thì tạo mới
                    _context.WmsStockBalances.Add(new WmsStockBalance
                    {
                        VariantId = req.VariantId,
                        LocationId = req.ToLocationId,
                        WarehouseId = warehouseId, // Vẫn nằm trong cùng 1 kho
                        Quantity = req.Qty,
                        Nsx = req.Nsx,
                        Hsd = req.Hsd
                    });
                }

                // Lưu thay đổi
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Cất hàng lên kệ thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }

    public class BinTransferDto
    {
        public int? VariantId { get; set; }
        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }
        public int Qty { get; set; }
        public DateTime? Nsx { get; set; }
        public DateTime? Hsd { get; set; }
    }
}