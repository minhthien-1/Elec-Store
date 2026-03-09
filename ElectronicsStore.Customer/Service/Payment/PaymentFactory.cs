using ElectronicsStore.Customer.Service.Payment;

public class PaymentFactory
{
    public IPayment Create(string paymentMethod)
    {
        Console.WriteLine($"[FACTORY PATTERN] ---> Khách hàng chọn thanh toán qua: {paymentMethod.ToUpper()}");
        Console.WriteLine($"[FACTORY PATTERN] ---> Đang khởi tạo Object xử lý tương ứng...");

        return paymentMethod.ToUpper() switch
        {
            "VNPAY" => new VnPayPayment(),
            "COD" => new CodPayment(),
            _ => throw new NotSupportedException($"Hình thức thanh toán {paymentMethod} chưa được hỗ trợ.")
        };
    }
}