using ElectronicsStore.Customer.Models.ViewModels;

namespace ElectronicsStore.Customer.Builders
{
    public class ProductBuilder : IProductBuilder
    {
        private ProductDetailViewModel _product = new ProductDetailViewModel();

        public ProductBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _product = new ProductDetailViewModel();
        }

        public IProductBuilder SetBasicInfo(int id, string name, decimal price)
        {
            _product.MaSP = id;
            _product.TenSP = name;
            _product.GiaBan = price;
            return this;
        }

        public IProductBuilder SetDiscountPrice(decimal? discountPrice)
        {
            _product.GiaGiamGia = discountPrice;
            return this;
        }

        public IProductBuilder SetDescription(string description, string specs)
        {
            _product.MoTaChiTiet = description;
            _product.ThongTinKyThuat = specs;
            return this;
        }

        public IProductBuilder SetStock(int stock)
        {
            _product.SoLuongTonKho = stock;
            return this;
        }

        public IProductBuilder SetImage(string? imageUrl)
        {
            _product.HinhAnh = imageUrl;
            return this;
        }

        public IProductBuilder SetCategory(int id, string name)
        {
            _product.DanhMuc = new CategoryDto { MaDanhMuc = id, TenDanhMuc = name };
            return this;
        }

        public IProductBuilder SetBrand(int id, string name)
        {
            _product.NhaSX = new BrandDto { MaNhaSX = id, TenNhaSX = name };
            return this;
        }

        public ProductDetailViewModel Build()
        {
            // LOGIC THÔNG MINH: Tự động gắn tag dựa trên mức giảm giá
            if (_product.GiaGiamGia.HasValue && _product.GiaBan > 0)
            {
                // Tính toán phần trăm giảm giá: (GiaBan - GiaGiamGia) / GiaBan
                decimal discountPercent = (_product.GiaBan - _product.GiaGiamGia.Value) / _product.GiaBan;

                if (discountPercent >= 0.5m) // Nếu giảm từ 50% trở lên
                {
                    _product.TenSP = "[SIÊU GIẢM GIÁ] " + _product.TenSP;
                }
            }

            ProductDetailViewModel result = _product;
            Reset();
            return result;
        }
    }
}