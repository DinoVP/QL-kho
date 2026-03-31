using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // THÊM DÒNG NÀY ĐỂ FIX LỖI

namespace BE.Models;

public partial class HrmBranch
{
    public int BranchId { get; set; }
    public int? DistrictId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? ManagerId { get; set; }
    public bool? IsActive { get; set; }

    // =======================================================
    // ĐÃ THÊM CỘT NÀY ĐỂ NHẬN SỐ LƯỢNG KHO SẾP NHẬP TỪ MÀN HÌNH
    // =======================================================
    public int? WarehouseCount { get; set; }

    public virtual GeoDistrict? District { get; set; }

    // --- CHỈ RÕ MỐI QUAN HỆ 1: NGƯỜI QUẢN LÝ ---
    [ForeignKey("ManagerId")]
    [InverseProperty("ManagedBranches")]
    public virtual HrmEmployee? Manager { get; set; }

    // --- CHỈ RÕ MỐI QUAN HỆ 2: NHÂN VIÊN LÀM VIỆC TẠI ĐÂY ---
    [InverseProperty("Branch")]
    public virtual ICollection<HrmEmployee> HrmEmployees { get; set; } = new List<HrmEmployee>();

    public virtual ICollection<HrmDepartment> HrmDepartments { get; set; } = new List<HrmDepartment>();
    public virtual ICollection<WmsWarehouse> WmsWarehouses { get; set; } = new List<WmsWarehouse>();
}