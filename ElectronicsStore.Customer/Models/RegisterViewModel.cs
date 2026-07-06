using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.Customer.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(30, ErrorMessage = "Họ tên không được vượt quá 30 ký tự")]
        public string TenDayDu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        [StringLength(30, ErrorMessage = "Email không được vượt quá 30 ký tự")]
        public string Email { get; set; } = string.Empty;

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải đúng 10 chữ số")]
        [RegularExpression(@"^(0[3|5|7|8|9])[0-9]{8}$",
            ErrorMessage = "Số điện thoại không hợp lệ (VD: 0912345678)")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [StringLength(255, ErrorMessage = "Mật khẩu quá dài")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ThanhPho { get; set; }
    }
}
