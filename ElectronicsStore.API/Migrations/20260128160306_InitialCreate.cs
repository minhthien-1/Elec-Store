using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ElectronicsStore.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucSanPhams",
                columns: table => new
                {
                    MaDanhMuc = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenDanhMuc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "text", nullable: true),
                    HinhAnh = table.Column<string>(type: "text", nullable: true),
                    TrangThai = table.Column<bool>(type: "boolean", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuaDoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucSanPhams", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "MaKhuyenMais",
                columns: table => new
                {
                    MaKM = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenChienDich = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "text", nullable: true),
                    KieuGiam = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GiaTriGiam = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    GiaTriGiamToiDa = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    SoLuotSuDung = table.Column<int>(type: "integer", nullable: false),
                    GioiHanSoLuotSuDung = table.Column<int>(type: "integer", nullable: true),
                    GiaTriDonHangToiThieu = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrangThai = table.Column<bool>(type: "boolean", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaKhuyenMais", x => x.MaKM);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    MaND = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TenDayDu = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SoDienThoai = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MatKhauHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DiaChiChiTiet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThanhPho = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuocGia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DiaChiMacDinh = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LaQuanTriVien = table.Column<bool>(type: "boolean", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "boolean", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuaDoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.MaND);
                });

            migrationBuilder.CreateTable(
                name: "NhaSanXuats",
                columns: table => new
                {
                    MaNhaSX = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenNhaSX = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DatNuoc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    SoDienThoai = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    TrangThai = table.Column<bool>(type: "boolean", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuaDoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaSanXuats", x => x.MaNhaSX);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    MaDH = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaDonHangGoc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaND = table.Column<int>(type: "integer", nullable: false),
                    NgayTaoDon = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TongGiaTruocGiam = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    PhiVanChuyen = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TienGiamGia = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TongGiaSauGiam = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    TrangThaiDon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DiaChiGiaoHang = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThanhPhoPhuong = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SodTLienHe = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NguoiDungMaND = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.MaDH);
                    table.ForeignKey(
                        name: "FK_DonHangs_NguoiDungs_NguoiDungMaND",
                        column: x => x.NguoiDungMaND,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaND");
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    MaSP = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenSP = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MoTaChiTiet = table.Column<string>(type: "text", nullable: true),
                    ThongTinKyThuat = table.Column<string>(type: "text", nullable: true),
                    MaDanhMuc = table.Column<int>(type: "integer", nullable: false),
                    MaNhaSX = table.Column<int>(type: "integer", nullable: true),
                    GiaBan = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    GiaGiamGia = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    SoLuongTonKho = table.Column<int>(type: "integer", nullable: false),
                    HinhAnh = table.Column<string>(type: "text", nullable: true),
                    DanhGiaXepHang = table.Column<decimal>(type: "numeric(2,1)", nullable: false),
                    SoLuotDanhGia = table.Column<int>(type: "integer", nullable: false),
                    SoLuotXem = table.Column<int>(type: "integer", nullable: false),
                    TrangThai = table.Column<bool>(type: "boolean", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuaDoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DanhMucMaDanhMuc = table.Column<int>(type: "integer", nullable: true),
                    NhaSXMaNhaSX = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.MaSP);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucSanPhams_DanhMucMaDanhMuc",
                        column: x => x.DanhMucMaDanhMuc,
                        principalTable: "DanhMucSanPhams",
                        principalColumn: "MaDanhMuc");
                    table.ForeignKey(
                        name: "FK_SanPhams_NhaSanXuats_NhaSXMaNhaSX",
                        column: x => x.NhaSXMaNhaSX,
                        principalTable: "NhaSanXuats",
                        principalColumn: "MaNhaSX");
                });

            migrationBuilder.CreateTable(
                name: "LichSuDonHangs",
                columns: table => new
                {
                    MaLS = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaDH = table.Column<int>(type: "integer", nullable: false),
                    TrangThaiCu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TrangThaiMoi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaNguoiCapNhat = table.Column<int>(type: "integer", nullable: true),
                    LyDo = table.Column<string>(type: "text", nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DonHangMaDH = table.Column<int>(type: "integer", nullable: true),
                    NguoiDungMaND = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDonHangs", x => x.MaLS);
                    table.ForeignKey(
                        name: "FK_LichSuDonHangs_DonHangs_DonHangMaDH",
                        column: x => x.DonHangMaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH");
                    table.ForeignKey(
                        name: "FK_LichSuDonHangs_NguoiDungs_NguoiDungMaND",
                        column: x => x.NguoiDungMaND,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaND");
                });

            migrationBuilder.CreateTable(
                name: "LichSuThanhToans",
                columns: table => new
                {
                    MaLS = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaDH = table.Column<int>(type: "integer", nullable: false),
                    MaND = table.Column<int>(type: "integer", nullable: false),
                    SoTienThanhToan = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    PhuongThuc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrangThaiGD = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaThamChieu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GhiChuLoi = table.Column<string>(type: "text", nullable: true),
                    NgayThanhToan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DonHangMaDH = table.Column<int>(type: "integer", nullable: true),
                    NguoiDungMaND = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuThanhToans", x => x.MaLS);
                    table.ForeignKey(
                        name: "FK_LichSuThanhToans_DonHangs_DonHangMaDH",
                        column: x => x.DonHangMaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH");
                    table.ForeignKey(
                        name: "FK_LichSuThanhToans_NguoiDungs_NguoiDungMaND",
                        column: x => x.NguoiDungMaND,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaND");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaDH = table.Column<int>(type: "integer", nullable: false),
                    MaSP = table.Column<int>(type: "integer", nullable: false),
                    SoLuong = table.Column<int>(type: "integer", nullable: false),
                    GiaTaiThoiDiem = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    GiamGiaCungChiTiet = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DonHangMaDH = table.Column<int>(type: "integer", nullable: true),
                    SanPhamMaSP = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_DonHangMaDH",
                        column: x => x.DonHangMaDH,
                        principalTable: "DonHangs",
                        principalColumn: "MaDH");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_SanPhams_SanPhamMaSP",
                        column: x => x.SanPhamMaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaSanPhams",
                columns: table => new
                {
                    MaDG = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaSP = table.Column<int>(type: "integer", nullable: false),
                    MaND = table.Column<int>(type: "integer", nullable: false),
                    DemSao = table.Column<int>(type: "integer", nullable: false),
                    TieuDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NoiDung = table.Column<string>(type: "text", nullable: true),
                    CoDienTieuDung = table.Column<bool>(type: "boolean", nullable: false),
                    DuocDuyet = table.Column<bool>(type: "boolean", nullable: false),
                    SoLuotThichChu = table.Column<int>(type: "integer", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuaDoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SanPhamMaSP = table.Column<int>(type: "integer", nullable: true),
                    NguoiDungMaND = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaSanPhams", x => x.MaDG);
                    table.ForeignKey(
                        name: "FK_DanhGiaSanPhams_NguoiDungs_NguoiDungMaND",
                        column: x => x.NguoiDungMaND,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaND");
                    table.ForeignKey(
                        name: "FK_DanhGiaSanPhams_SanPhams_SanPhamMaSP",
                        column: x => x.SanPhamMaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    MaGioHang = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaND = table.Column<int>(type: "integer", nullable: false),
                    MaSP = table.Column<int>(type: "integer", nullable: false),
                    SoLuong = table.Column<int>(type: "integer", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NguoiDungMaND = table.Column<int>(type: "integer", nullable: true),
                    SanPhamMaSP = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.MaGioHang);
                    table.ForeignKey(
                        name: "FK_GioHangs_NguoiDungs_NguoiDungMaND",
                        column: x => x.NguoiDungMaND,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaND");
                    table.ForeignKey(
                        name: "FK_GioHangs_SanPhams_SanPhamMaSP",
                        column: x => x.SanPhamMaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateTable(
                name: "ThongKeLuotXems",
                columns: table => new
                {
                    MaTK = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaSP = table.Column<int>(type: "integer", nullable: false),
                    MaND = table.Column<int>(type: "integer", nullable: true),
                    NgayXem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SoLuotXemTrongNgay = table.Column<int>(type: "integer", nullable: false),
                    ThemTrongDB = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SanPhamMaSP = table.Column<int>(type: "integer", nullable: true),
                    NguoiDungMaND = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongKeLuotXems", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_ThongKeLuotXems_NguoiDungs_NguoiDungMaND",
                        column: x => x.NguoiDungMaND,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaND");
                    table.ForeignKey(
                        name: "FK_ThongKeLuotXems_SanPhams_SanPhamMaSP",
                        column: x => x.SanPhamMaSP,
                        principalTable: "SanPhams",
                        principalColumn: "MaSP");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_DonHangMaDH",
                table: "ChiTietDonHangs",
                column: "DonHangMaDH");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_SanPhamMaSP",
                table: "ChiTietDonHangs",
                column: "SanPhamMaSP");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaSanPhams_NguoiDungMaND",
                table: "DanhGiaSanPhams",
                column: "NguoiDungMaND");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaSanPhams_SanPhamMaSP",
                table: "DanhGiaSanPhams",
                column: "SanPhamMaSP");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_NguoiDungMaND",
                table: "DonHangs",
                column: "NguoiDungMaND");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_NguoiDungMaND",
                table: "GioHangs",
                column: "NguoiDungMaND");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_SanPhamMaSP",
                table: "GioHangs",
                column: "SanPhamMaSP");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDonHangs_DonHangMaDH",
                table: "LichSuDonHangs",
                column: "DonHangMaDH");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDonHangs_NguoiDungMaND",
                table: "LichSuDonHangs",
                column: "NguoiDungMaND");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuThanhToans_DonHangMaDH",
                table: "LichSuThanhToans",
                column: "DonHangMaDH");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuThanhToans_NguoiDungMaND",
                table: "LichSuThanhToans",
                column: "NguoiDungMaND");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_DanhMucMaDanhMuc",
                table: "SanPhams",
                column: "DanhMucMaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_NhaSXMaNhaSX",
                table: "SanPhams",
                column: "NhaSXMaNhaSX");

            migrationBuilder.CreateIndex(
                name: "IX_ThongKeLuotXems_NguoiDungMaND",
                table: "ThongKeLuotXems",
                column: "NguoiDungMaND");

            migrationBuilder.CreateIndex(
                name: "IX_ThongKeLuotXems_SanPhamMaSP",
                table: "ThongKeLuotXems",
                column: "SanPhamMaSP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "DanhGiaSanPhams");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "LichSuDonHangs");

            migrationBuilder.DropTable(
                name: "LichSuThanhToans");

            migrationBuilder.DropTable(
                name: "MaKhuyenMais");

            migrationBuilder.DropTable(
                name: "ThongKeLuotXems");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "DanhMucSanPhams");

            migrationBuilder.DropTable(
                name: "NhaSanXuats");
        }
    }
}
