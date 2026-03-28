using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BE.Models; // Đảm bảo đúng tên namespace Models của sếp

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly QLKhoContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(QLKhoContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Class nhận dữ liệu từ Vue.js gửi lên
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Tìm user trong Database (Kèm theo Role của họ)
            var user = await _context.SysUsers
                .Include(u => u.Roles) // Móc nối sang bảng Roles
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive == true);

            // 2. Kiểm tra tài khoản và mật khẩu (Đồ án nên tạm so sánh chuỗi thường, thực tế phải băm Hash)
            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu, hoặc tài khoản đã bị khóa!" });
            }

            // 3. Lấy mã Role (vd: 'admin', 'ql_kho'...)
            var roleCode = user.Roles.FirstOrDefault()?.RoleCode ?? "nv_kho";

            // 4. Tạo Thẻ thông hành (JWT Token)
            var tokenHandler = new JwtSecurityTokenHandler();
            // Khóa bí mật (Tí mình cấu hình trong appsettings)
            var secretKey = _configuration["Jwt:Key"] ?? "DayLaMotCaiKhoaBiMatSieuDaiCuaSepHuyWms2026!!!";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, roleCode) // Gắn Role vào Token để Vue.js đọc
                }),
                Expires = DateTime.UtcNow.AddHours(8), // Token sống 8 tiếng (Bằng 1 ca làm việc)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 5. Trả về cho Frontend
            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = tokenString,
                role = roleCode,
                username = user.Username
            });
        }
    }
}