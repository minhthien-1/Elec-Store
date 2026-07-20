using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ElectronicsStore.Customer.TagHelpers
{
    [HtmlTargetElement("order-status")]
    public class OrderStatusTagHelper : TagHelper
    {
        public string State { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            
            // Logic bắt khớp chuỗi để chọn màu tự động
            string bgClass = State switch
            {
                "Chờ xác nhận" => "bg-warning text-dark",
                "Đang giao hàng" => "bg-info text-white",
                "Hoàn thành" => "bg-success text-white",
                "Đã hủy" => "bg-danger text-white",
                _ => "bg-secondary text-white"
            };

            var currentClass = output.Attributes["class"]?.Value.ToString() ?? "";
            output.Attributes.SetAttribute("class", $"badge rounded-pill {bgClass} {currentClass}");
            
            output.Content.SetContent(State);
        }
    }
}
