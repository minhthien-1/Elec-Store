using System.Text.RegularExpressions;
using ElectronicsStore.API.Models;

namespace ElectronicsStore.API.Helpers
{

    public class ValidationHelper
    {
        // 3 tới 20 ký tự
        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) &&
                   name.Length >= 3 &&
                   name.Length <= 20;
        }

        // Email đúng định dạng
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        // số điện thoại bắt đầu từ số 0 và 10 số
        public static bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) &&
                   Regex.IsMatch(phone, @"^0\d{9}$");
        }

        // mật khẩu bao gồm 1 ký tự đặc biệt, 1 chữ thường, 1 chữ hoa và lớn hơn 6 ký tự
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 7)
                return false;

            bool hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
            bool hasLowerCase = Regex.IsMatch(password, @"[a-z]");
            bool hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>?\/\\|`~]");

            return hasUpperCase && hasLowerCase && hasSpecialChar;
        }

        public static List<string> ValidateRegister(RegisterRequest request)
        {
            var errors = new List<string>();

            if (!IsValidName(request.TenDayDu))
                errors.Add("Tên phải từ 3-20 ký tự");

            if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
                errors.Add("Email phải có dạng xxx@xxx.com");

            if (!IsValidPhone(request.SoDienThoai))
                errors.Add("Số điện thoại phải bắt đầu 0 và có 10 số");

            if (!IsValidPassword(request.MatKhau))
                errors.Add("Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường, 1 ký tự đặc biệt và >= 7 ký tự");

            return errors;
        }
    }
}