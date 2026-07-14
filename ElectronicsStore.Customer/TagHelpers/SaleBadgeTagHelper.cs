using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace ElectronicsStore.Customer.TagHelpers
{
    [HtmlTargetElement("sale-badge")]
    public class SaleBadgeTagHelper : TagHelper
    {
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            
            // CSS classes để làm cho badge đẹp hơn
            var currentClass = output.Attributes["class"]?.Value.ToString() ?? "";
            output.Attributes.SetAttribute("class", $"badge bg-danger rounded-pill px-2 py-1 {currentClass}");

            if (OriginalPrice > 0 && OriginalPrice > SalePrice)
            {
                // Tính phần trăm giảm giá
                var discount = (OriginalPrice - SalePrice) / OriginalPrice * 100;
                output.Content.SetContent($"-{Math.Round(discount, 0)}%");
            }
            else
            {
                // Ẩn thẻ nếu không có giảm giá
                output.SuppressOutput(); 
            }
        }
    }
}
