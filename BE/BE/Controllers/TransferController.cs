using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly QLKhoContext _context;
        public TransferController(QLKhoContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetTransfers()
        {
            var transfers = await _context.WmsTransfers
                .Include(t => t.WmsTransferLines)
                .OrderByDescending(t => t.TransferId)
                .ToListAsync();

            var result = transfers.Select(t => new TransferDto
            {
                Id = t.TransferId,
                Code = t.TransferCode ?? $"DC{t.TransferId.ToString().PadLeft(4, '0')}",
                Date = t.TransferDate?.ToString("yyyy-MM-dd") ?? "",
                FromWarehouseId = t.FromWh, // Trả về ID kho xuất để Giao diện map tên
                ToWarehouseId = t.ToWh,     // Trả về ID kho nhập để Giao diện map tên
                Note = t.Note ?? "",
                Status = t.Status ?? "pending",
                Items = t.WmsTransferLines.Select(l => new TransferItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    FromLocationId = l.FromLocationId,
                    ToLocationId = l.ToLocationId,
                    Qty = l.Qty ?? 0m,
                    Nsx = l.Nsx?.ToString("yyyy-MM-dd"),
                    Hsd = l.Hsd?.ToString("yyyy-MM-dd")
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] TransferDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int maxId = await _context.WmsTransfers.MaxAsync(r => (int?)r.TransferId) ?? 0;
                    finalCode = $"DC{(maxId + 1).ToString().PadLeft(4, '0')}";
                }

                var transfer = new WmsTransfer
                {
                    TransferCode = finalCode,
                    TransferDate = DateTime.TryParse(req.Date, out var d) ? d : DateTime.Now,
                    FromWh = req.FromWarehouseId, // Lưu ID kho xuất vào DB
                    ToWh = req.ToWarehouseId,     // Lưu ID kho nhập vào DB
                    Status = "pending",
                    Note = req.Note
                };

                _context.WmsTransfers.Add(transfer);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.WmsTransferLines.Add(new WmsTransferLine
                        {
                            TransferId = transfer.TransferId,
                            VariantId = item.VariantId,
                            FromLocationId = item.FromLocationId,
                            ToLocationId = item.ToLocationId,
                            Qty = Convert.ToInt32(item.Qty ?? 0),
                            Nsx = DateTime.TryParse(item.Nsx, out var n) ? n : null,
                            Hsd = DateTime.TryParse(item.Hsd, out var h) ? h : null
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Lập lệnh điều chuyển thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteTransfer(int id)
        {
            var transfer = await _context.WmsTransfers.Include(t => t.WmsTransferLines).FirstOrDefaultAsync(t => t.TransferId == id);
            if (transfer == null || transfer.Status == "completed")
                return BadRequest(new { message = "Phiếu không tồn tại hoặc đã xử lý!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in transfer.WmsTransferLines)
                {

                    // 1. TRỪ HÀNG Ở KỆ CŨ
                    var sourceStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId &&
                        s.LocationId == item.FromLocationId &&
                        s.Nsx == item.Nsx &&
                        s.Hsd == item.Hsd);

                    if (sourceStock == null || sourceStock.Quantity < item.Qty)
                        throw new Exception($"Kệ nguồn không đủ hàng cho Sản phẩm ID {item.VariantId}!");

                    sourceStock.Quantity -= item.Qty;
                    if (sourceStock.Quantity <= 0) _context.WmsStockBalances.Remove(sourceStock);

                    // 2. CỘNG HÀNG VÀO KỆ MỚI
                    var destStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId &&
                        s.LocationId == item.ToLocationId &&
                        s.Nsx == item.Nsx &&
                        s.Hsd == item.Hsd);

                    if (destStock != null)
                    {
                        destStock.Quantity += item.Qty;
                    }
                    else
                    {
                        _context.WmsStockBalances.Add(new WmsStockBalance
                        {
                            VariantId = item.VariantId,
                            LocationId = item.ToLocationId,
                            WarehouseId = transfer.ToWh ?? 1, // Lưu đúng mã Kho Đích
                            Quantity = item.Qty,
                            Nsx = item.Nsx,
                            Hsd = item.Hsd
                        });
                    }
                }

                transfer.Status = "completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Điều chuyển thành công! Hàng đã được luân chuyển trên DB." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class TransferDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Date { get; set; }

        // Bổ sung 2 cột này để map từ Giao diện xuống
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }

        public string? Note { get; set; }
        public string? Status { get; set; }
        public List<TransferItemDto>? Items { get; set; } = new List<TransferItemDto>();
    }

    public class TransferItemDto
    {
        public int? VariantId { get; set; }
        public int? FromLocationId { get; set; }
        public int? ToLocationId { get; set; }
        public decimal? Qty { get; set; }
        public string? Nsx { get; set; }
        public string? Hsd { get; set; }
    }
}