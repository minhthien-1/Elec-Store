using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ElectronicsStore.Customer.TagHelpers
{
    [HtmlTargetElement("price-format")]
    public class CurrencyFormatTagHelper : TagHelper
    {
        public decimal Value { get; set; }
        
        // Mặc định là giá bán (hiện màu đỏ to). Nếu là giá gạch ngang thì truyền is-sale="false"
        public bool IsSale { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            
            string formattedPrice = Value.ToString("N0") + "₫";
            
            var currentClass = output.Attributes["class"]?.Value.ToString() ?? "";
            
            if (IsSale)
            {
                output.Attributes.SetAttribute("class", $"text-danger fw-bold {currentClass}");
            }
            else
            {
                output.Attributes.SetAttribute("class", $"text-muted text-decoration-line-through small {currentClass}");
            }
            
            output.Content.SetContent(formattedPrice);
        }
    }
}
