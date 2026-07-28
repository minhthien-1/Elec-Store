using ElectronicsStore.API.Models.Entities;

namespace ElectronicsStore.Customer.Service.Payment
{
    public class VnPayPayment : IPayment
    {
        public string GeneratePaymentUrl(DonHang order, HttpContext httpContext)
        {
            Console.WriteLine($"[PAYMENT - VNPAY] ---> Đang đóng gói dữ liệu gửi cổng VNPay...");
            Console.WriteLine($"[PAYMENT - VNPAY] ---> Mã giao dịch (TxnRef): {order.MaDH} | Số tiền (Amount): {order.TongGiaSauGiam:N0}đ");

            string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string vnp_TmnCode = "DSGBBMYW";
            string vnp_HashSecret = "KNHULPWPUBSXXNTYECLDDAZJUCFWCIXB";

            // Lấy scheme và host động thay vì hard-code localhost:44371
            var request = httpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            string vnp_Returnurl = $"{baseUrl}/Checkout/PaymentCallback";

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)order.TongGiaSauGiam * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");

            // Lấy IP thật của user
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + order.MaDH);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.MaDH.ToString());

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }
    }
}