using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email hoặc Số điện thoại bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Tên bắt buộc")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Tên phải từ 3-20 ký tự")]
    public string TenDayDu { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại bắt buộc")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu 0 và có 10 số")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu bắt buộc")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+\-=\[\]{};':"",.<>?\/\\|`~])(?=.{7,}).*$",
        ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường, 1 ký tự đặc biệt và >= 7 ký tự"
    )]
    public string MatKhau { get; set; } = string.Empty;
}