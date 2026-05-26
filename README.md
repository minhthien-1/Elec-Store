PAYMENT TEST:
Chọn "Thẻ nội địa và tài khoản ngân hàng"

Ngân hàng: Chọn NCB (bắt buộc đúng ngân hàng)

Số thẻ: 9704198526191432198

Tên chủ thẻ: NGUYEN VAN A 

Ngày phát hành: 07/15

OTP: 123456

Trang lịch sử đơn hàng nằm trong trang giỏ hàng

https://sandbox.vnpayment.vn/apis/vnpay-demo/

Nhiệm vụ: Phụ trách luồng dữ liệu độc lập bằng Razor Pages và dọn dẹp mã nguồn HTML bằng Tag Helpers (Hoạt động chủ yếu ở Admin và các Form).

Task 1 (Razor Pages): Thầu nguyên khu vực Account/Auth. Bốc toàn bộ LoginController, RegisterController đập đi, chuyển hết thành các file Login.cshtml (kèm .cs) đặt trong thư mục /Pages/Account/. Nếu dư thời gian, chuyển thêm trang Dashboard Thống kê bên Admin sang Razor Pages luôn.

Task 2 (Tag Helpers): Dọn dẹp "bãi rác" code cũ. Đi vòng quanh các file .cshtml từ Admin (Form thêm/sửa đồ điện tử, thiết bị) đến Customer (Form Checkout, Form nhập địa chỉ). Chuyển hết thẻ HTML thuần hoặc Html.Helper cũ sang chuẩn <input asp-for="..." />, <form asp-controller="..." ...>.
