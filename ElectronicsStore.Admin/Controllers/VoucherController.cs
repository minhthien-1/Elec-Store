using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ElectronicsStore.Admin.Controllers
{
    public class VoucherController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiBase;

        public VoucherController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _apiBase = _configuration["ApiSettings:BaseUrl"] + "/api/promotion";
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        // GET: /Admin/Voucher
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            var vouchers = new List<VoucherAdminViewModel>();
            try
            {
                var response = await _httpClient.GetAsync(_apiBase);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<VoucherListResponse>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    vouchers = result?.data ?? new List<VoucherAdminViewModel>();
                }
                else
                {
                    ViewBag.Error = "Không thể tải danh sách voucher";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi kết nối API: " + ex.Message;
            }
            return View(vouchers);
        }

        // GET: /Admin/Voucher/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: /Admin/Voucher/Create
        [HttpPost]
        public async Task<IActionResult> Create(VoucherCreateRequest request)
        {
            SetAuthHeader();
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiBase, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Tạo voucher thành công!";
                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Lỗi: " + error;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi kết nối API: " + ex.Message;
            }
            return View(request);
        }

        // GET: /Admin/Voucher/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBase}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var voucher = JsonSerializer.Deserialize<VoucherAdminViewModel>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return View(voucher);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        // POST: /Admin/Voucher/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(int id, VoucherUpdateRequest request)
        {
            SetAuthHeader();
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_apiBase}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật voucher thành công!";
                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Lỗi: " + error;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi kết nối API: " + ex.Message;
            }
            return View(request);
        }

        // POST: /Admin/Voucher/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBase}/{id}");
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Xóa voucher thành công!" });

                return Json(new { success = false, message = "Xóa thất bại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ViewModels
    public class VoucherAdminViewModel
    {
        public int maKM { get; set; }
        public string maCode { get; set; } = string.Empty;
        public string? tenChienDich { get; set; }
        public string? moTa { get; set; }
        public string? kieuGiam { get; set; }
        public decimal? giaTriGiam { get; set; }
        public decimal? giaTriGiamToiDa { get; set; }
        public int? soLuotSuDung { get; set; }
        public int? gioiHanSoLuotSuDung { get; set; }
        public decimal? giaTriDonHangToiThieu { get; set; }
        public DateTime? ngayBatDau { get; set; }
        public DateTime? ngayKetThuc { get; set; }
        public bool trangThai { get; set; }
    }

    public class VoucherListResponse
    {
        public int total { get; set; }
        public List<VoucherAdminViewModel> data { get; set; } = new();
    }

    public class VoucherCreateRequest
    {
        public string MaCode { get; set; } = string.Empty;
        public string? TenChienDich { get; set; }
        public string? MoTa { get; set; }
        public string? KieuGiam { get; set; } // "PhanTram" hoac "TienCoThuc"
        public decimal? GiaTriGiam { get; set; }
        public decimal? GiaTriGiamToiDa { get; set; }
        public int? GioiHanSoLuotSuDung { get; set; }
        public decimal? GiaTriDonHangToiThieu { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    public class VoucherUpdateRequest
    {
        public string? TenChienDich { get; set; }
        public string? MoTa { get; set; }
        public decimal? GiaTriGiam { get; set; }
        public decimal? GiaTriGiamToiDa { get; set; }
        public int? GioiHanSoLuotSuDung { get; set; }
        public decimal? GiaTriDonHangToiThieu { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}