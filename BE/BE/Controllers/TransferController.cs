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
                FromWarehouseId = t.FromWh,
                ToWarehouseId = t.ToWh,
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
                    FromWh = req.FromWarehouseId,
                    ToWh = req.ToWarehouseId,
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

        // =======================================================================
        // API: XÁC NHẬN KHO XUẤT ĐÃ GIAO HÀNG CHO XE TẢI (STATUS = SHIPPING)
        // =======================================================================
        [HttpPut("{id}/shipping")]
        public async Task<IActionResult> ShippingTransfer(int id)
        {
            var transfer = await _context.WmsTransfers.FindAsync(id);
            if (transfer == null)
                return BadRequest(new { message = "Phiếu điều chuyển không tồn tại!" });

            if (transfer.Status == "shipping" || transfer.Status == "completed")
                return BadRequest(new { message = "Lỗi: Lệnh này đã được xuất đi hoặc đã hoàn thành!" });

            transfer.Status = "shipping";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xuất hàng! Hàng đang trên đường vận chuyển." });
        }


        // =======================================================================
        // API (ĐÃ SỬA CHUẨN ERP): KHO NHẬN XÁC NHẬN -> HÀNG VÀO "KHU CHỜ NHẬP"
        // =======================================================================
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
                    // 1. TRỪ HÀNG Ở KỆ CŨ (KHO XUẤT)
                    var sourceStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId &&
                        s.LocationId == item.FromLocationId &&
                        s.Nsx == item.Nsx &&
                        s.Hsd == item.Hsd);

                    if (sourceStock == null || sourceStock.Quantity < item.Qty)
                        throw new Exception($"Kệ nguồn (ID {item.FromLocationId}) không đủ hàng cho Sản phẩm ID {item.VariantId}!");

                    sourceStock.Quantity -= item.Qty;
                    if (sourceStock.Quantity <= 0) _context.WmsStockBalances.Remove(sourceStock);


                    // 2. CỘNG HÀNG VÀO "KHU CHỜ NHẬP" CỦA KHO MỚI (LocationId = null)
                    // Hàng sẽ nằm lơ lửng trong Kho (hiện trên màn hình Kho bãi) để Thủ kho tự xếp lên kệ sau.
                    var destStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                        s.VariantId == item.VariantId &&
                        s.WarehouseId == transfer.ToWh && // Tìm đúng Kho Nhập
                        s.LocationId == null &&           // LocationId = null tức là Khu chờ nhập
                        s.Nsx == item.Nsx &&
                        s.Hsd == item.Hsd);

                    if (destStock != null)
                    {
                        destStock.Quantity += item.Qty; // Đã có sẵn hàng ở khu chờ nhập -> Cộng dồn
                    }
                    else
                    {
                        // Chưa có hàng ở khu chờ nhập -> Tạo mới
                        _context.WmsStockBalances.Add(new WmsStockBalance
                        {
                            VariantId = item.VariantId,
                            LocationId = null,             // Gắn = null để chui vào Cất hàng (Putaway)
                            WarehouseId = transfer.ToWh ?? 1,
                            Quantity = item.Qty,
                            Nsx = item.Nsx,
                            Hsd = item.Hsd
                        });
                    }
                }

                transfer.Status = "completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Điều chuyển thành công! Hàng đã nằm trong Khu Chờ Nhập." });
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