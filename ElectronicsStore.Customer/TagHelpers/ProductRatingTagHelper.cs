using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System;

namespace ElectronicsStore.Customer.TagHelpers
{
    [HtmlTargetElement("product-rating")]
    public class ProductRatingTagHelper : TagHelper
    {
        public decimal Score { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            
            // Nối thêm class hiện tại (nếu có)
            var currentClass = output.Attributes["class"]?.Value.ToString() ?? "";
            output.Attributes.SetAttribute("class", $"text-warning d-inline-block {currentClass}");

            var sb = new StringBuilder();
            
            // Giới hạn điểm từ 0-5
            decimal validScore = Math.Clamp(Score, 0m, 5m);
            
            int fullStars = (int)Math.Floor(validScore);
            bool hasHalfStar = (validScore - fullStars) >= 0.5m;
            int emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

            for (int i = 0; i < fullStars; i++) sb.Append("<i class=\"bi bi-star-fill\"></i> ");
            if (hasHalfStar) sb.Append("<i class=\"bi bi-star-half\"></i> ");
            for (int i = 0; i < emptyStars; i++) sb.Append("<i class=\"bi bi-star\"></i> ");

            if (validScore > 0)
            {
                sb.Append($"<span class=\"ms-1 text-muted small\">({validScore:F1})</span>");
            }

            output.Content.SetHtmlContent(sb.ToString().TrimEnd());
        }
    }
}
