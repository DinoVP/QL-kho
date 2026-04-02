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
    public class OutboundController : ControllerBase
    {
        private readonly QLKhoContext _context;

        public OutboundController(QLKhoContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetReceipts()
        {
            var deliveries = await _context.SalDeliveries
                .Include(r => r.SalDeliveryLines)
                .OrderByDescending(r => r.Doid)
                .ToListAsync();

            var result = deliveries.Select(r => new OutboundReceiptDto
            {
                Id = r.Doid,
                Code = r.Docode,
                Date = r.DeliveryDate?.ToString("yyyy-MM-dd") ?? "",
                CustomerId = r.CustomerId ?? 0,
                CustomerName = "Khách hàng (ID: " + r.CustomerId + ")",
                Note = r.Note ?? "",
                Status = r.Status ?? "pending",
                TotalQty = r.SalDeliveryLines.Sum(l => l.DeliveredQty ?? 0m),
                TotalPrice = r.SalDeliveryLines.Sum(l => (l.DeliveredQty ?? 0m) * (l.Price ?? 0m)),
                Items = r.SalDeliveryLines.Select(l => new OutboundItemDto
                {
                    VariantId = l.VariantId ?? 0,
                    Qty = l.DeliveredQty ?? 0m,
                    Price = l.Price ?? 0m,
                    LocationId = l.LocationId,
                    Nsx = l.Nsx?.ToString("yyyy-MM-dd"), // Lấy NSX lô xuất
                    Hsd = l.Hsd?.ToString("yyyy-MM-dd")  // Lấy HSD lô xuất
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] OutboundReceiptDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string finalCode = req.Code;
                if (string.IsNullOrEmpty(finalCode))
                {
                    int maxId = await _context.SalDeliveries.MaxAsync(r => (int?)r.Doid) ?? 0;
                    finalCode = $"PX{(maxId + 1).ToString().PadLeft(4, '0')}";
                }

                var delivery = new SalDelivery
                {
                    Docode = finalCode,
                    DeliveryDate = DateTime.TryParse(req.Date, out var d) ? d : DateTime.Now,
                    CustomerId = req.CustomerId,
                    Status = "pending",
                    Note = req.Note
                };
                _context.SalDeliveries.Add(delivery);
                await _context.SaveChangesAsync();

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.SalDeliveryLines.Add(new SalDeliveryLine
                        {
                            Doid = delivery.Doid,
                            VariantId = item.VariantId,
                            DeliveredQty = Convert.ToInt32(item.Qty ?? 0),
                            Price = item.Price,
                            LocationId = item.LocationId,
                            Nsx = DateTime.TryParse(item.Nsx, out var n) ? n : null,
                            Hsd = DateTime.TryParse(item.Hsd, out var h) ? h : null
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Lập phiếu xuất thành công!", code = delivery.Docode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReceipt(int id, [FromBody] OutboundReceiptDto req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var delivery = await _context.SalDeliveries.Include(r => r.SalDeliveryLines).FirstOrDefaultAsync(r => r.Doid == id);
                if (delivery == null || delivery.Status != "pending") return BadRequest(new { message = "Chỉ được sửa phiếu chờ duyệt!" });

                delivery.CustomerId = req.CustomerId; delivery.DeliveryDate = DateTime.TryParse(req.Date, out var d) ? d : DateTime.Now; delivery.Note = req.Note;
                _context.SalDeliveryLines.RemoveRange(delivery.SalDeliveryLines);

                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        _context.SalDeliveryLines.Add(new SalDeliveryLine
                        {
                            Doid = delivery.Doid,
                            VariantId = item.VariantId,
                            DeliveredQty = Convert.ToInt32(item.Qty ?? 0),
                            Price = item.Price,
                            LocationId = item.LocationId,
                            Nsx = DateTime.TryParse(item.Nsx, out var n) ? n : null,
                            Hsd = DateTime.TryParse(item.Hsd, out var h) ? h : null
                        });
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var delivery = await _context.SalDeliveries.Include(r => r.SalDeliveryLines).FirstOrDefaultAsync(r => r.Doid == id);
            if (delivery == null || delivery.Status != "pending") return BadRequest(new { message = "Chỉ được xóa phiếu chờ duyệt!" });
            _context.SalDeliveryLines.RemoveRange(delivery.SalDeliveryLines);
            _context.SalDeliveries.Remove(delivery);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveReceipt(int id)
        {
            var delivery = await _context.SalDeliveries.FindAsync(id);
            if (delivery != null) { delivery.Status = "approved"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Đã duyệt phiếu xuất!" });
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectReceipt(int id)
        {
            var delivery = await _context.SalDeliveries.FindAsync(id);
            if (delivery != null) { delivery.Status = "rejected"; await _context.SaveChangesAsync(); }
            return Ok(new { message = "Đã từ chối!" });
        }

        // TRỪ KHO CHUẨN XÁC THEO NSX, HSD VÀ VỊ TRÍ
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteReceipt(int id)
        {
            var delivery = await _context.SalDeliveries.Include(r => r.SalDeliveryLines).FirstOrDefaultAsync(r => r.Doid == id);
            if (delivery == null || delivery.Status != "approved") return BadRequest(new { message = "Phiếu chưa duyệt!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in delivery.SalDeliveryLines)
                {
                    var stockType = typeof(WmsStockBalance);
                    var qtyProp = stockType.GetProperty("Quantity") ?? stockType.GetProperty("Qty") ?? stockType.GetProperty("StockQty");

                    if (qtyProp != null)
                    {
                        // Tìm ĐÚNG dòng tồn kho: Cùng SP + Cùng Kệ + Cùng NSX + Cùng HSD
                        var existingStock = await _context.WmsStockBalances.FirstOrDefaultAsync(s =>
                            s.VariantId == item.VariantId &&
                            s.LocationId == item.LocationId &&
                            s.Nsx == item.Nsx &&
                            s.Hsd == item.Hsd);

                        if (existingStock != null)
                        {
                            decimal currentQty = Convert.ToDecimal(qtyProp.GetValue(existingStock) ?? 0m);
                            decimal newQty = currentQty - (item.DeliveredQty ?? 0m);

                            // Nếu xuất hết sạch lô đó thì xóa dòng tồn kho, ngược lại thì cập nhật số lượng
                            if (newQty <= 0) _context.WmsStockBalances.Remove(existingStock);
                            else
                            {
                                Type targetType = Nullable.GetUnderlyingType(qtyProp.PropertyType) ?? qtyProp.PropertyType;
                                qtyProp.SetValue(existingStock, Convert.ChangeType(newQty, targetType));
                            }
                        }
                        else
                        {
                            throw new Exception($"Không tìm thấy Tồn kho hợp lệ cho Sản phẩm ID {item.VariantId} tại Kệ {item.LocationId}. Hàng có thể đã xuất hết!");
                        }
                    }
                }
                delivery.Status = "completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Đã xuất kho thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class OutboundReceiptDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Date { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalPrice { get; set; }
        public List<OutboundItemDto>? Items { get; set; } = new List<OutboundItemDto>();
    }

    public class OutboundItemDto
    {
        public int? VariantId { get; set; }
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Price { get; set; }
        public int? LocationId { get; set; }
        public string? LocationCode { get; set; }
        public string? Nsx { get; set; } // Map NSX
        public string? Hsd { get; set; } // Map HSD
    }
}