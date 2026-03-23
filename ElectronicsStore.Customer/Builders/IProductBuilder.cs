using ElectronicsStore.Customer.Models.ViewModels;

namespace ElectronicsStore.Customer.Builders
{
    public interface IProductBuilder
    {
        IProductBuilder SetBasicInfo(int id, string name, decimal price);
        IProductBuilder SetDiscountPrice(decimal? discountPrice);
        IProductBuilder SetDescription(string description, string specs);
        IProductBuilder SetStock(int stock);
        IProductBuilder SetImage(string? imageUrl);
        IProductBuilder SetCategory(int id, string name);
        IProductBuilder SetBrand(int id, string name);
        ProductDetailViewModel Build();
    }
}