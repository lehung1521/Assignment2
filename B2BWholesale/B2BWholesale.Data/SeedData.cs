using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data;

/// <summary>
/// Sample (seed) data: 15-30 rows per table. All data is fictional.
/// Passwords are SHA-256 hex hashes. Demo passwords: Admin@123 (Admin) and Agent@123 (Agent).
/// </summary>
public static class SeedData
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().HasData(
            new Item { ItemID = 1, ItemName = "Nước mắm Nam Ngư", Size = "12 chai x 500ml", Price = 420000m, StockQuantity = 180 },
            new Item { ItemID = 2, ItemName = "Dầu ăn Neptune", Size = "12 chai x 1L", Price = 540000m, StockQuantity = 150 },
            new Item { ItemID = 3, ItemName = "Đường tinh luyện Biên Hòa", Size = "10 gói x 1kg", Price = 230000m, StockQuantity = 300 },
            new Item { ItemID = 4, ItemName = "Muối i-ốt Bạc Liêu", Size = "20 gói x 500g", Price = 120000m, StockQuantity = 250 },
            new Item { ItemID = 5, ItemName = "Mì gói Hảo Hảo tôm chua cay", Size = "30 gói x 75g", Price = 115000m, StockQuantity = 600 },
            new Item { ItemID = 6, ItemName = "Gạo ST25", Size = "Bao 25kg", Price = 650000m, StockQuantity = 120 },
            new Item { ItemID = 7, ItemName = "Sữa đặc Ông Thọ", Size = "48 lon x 380g", Price = 960000m, StockQuantity = 90 },
            new Item { ItemID = 8, ItemName = "Cà phê hòa tan G7 3in1", Size = "24 hộp x 16 gói", Price = 1250000m, StockQuantity = 70 },
            new Item { ItemID = 9, ItemName = "Trà xanh C2 hương chanh", Size = "24 chai x 455ml", Price = 190000m, StockQuantity = 400 },
            new Item { ItemID = 10, ItemName = "Nước suối Aquafina", Size = "24 chai x 500ml", Price = 85000m, StockQuantity = 800 },
            new Item { ItemID = 11, ItemName = "Nước ngọt Coca-Cola", Size = "24 lon x 330ml", Price = 210000m, StockQuantity = 500 },
            new Item { ItemID = 12, ItemName = "Nước tương Maggi", Size = "24 chai x 300ml", Price = 410000m, StockQuantity = 140 },
            new Item { ItemID = 13, ItemName = "Bột giặt OMO Matic", Size = "4 túi x 3kg", Price = 520000m, StockQuantity = 110 },
            new Item { ItemID = 14, ItemName = "Nước rửa chén Sunlight", Size = "12 chai x 750ml", Price = 330000m, StockQuantity = 130 },
            new Item { ItemID = 15, ItemName = "Giấy vệ sinh Pulppy", Size = "6 lốc x 10 cuộn", Price = 360000m, StockQuantity = 200 },
            new Item { ItemID = 16, ItemName = "Xà phòng Lifebuoy", Size = "72 cục x 90g", Price = 540000m, StockQuantity = 95 },
            new Item { ItemID = 17, ItemName = "Dầu gội Clear Men", Size = "6 chai x 650g", Price = 780000m, StockQuantity = 12 },
            new Item { ItemID = 18, ItemName = "Sữa tươi Vinamilk 100%", Size = "12 hộp x 1L", Price = 320000m, StockQuantity = 160 },
            new Item { ItemID = 19, ItemName = "Bánh quy Cosy", Size = "24 gói x 150g", Price = 280000m, StockQuantity = 8 },
            new Item { ItemID = 20, ItemName = "Snack Oishi tôm cay", Size = "40 gói x 40g", Price = 300000m, StockQuantity = 220 });

        modelBuilder.Entity<Agent>().HasData(
            new Agent { AgentID = 1, AgentName = "Tạp hóa Minh Phát", Address = "123 Nguyễn Văn Linh, Quận 7, TP.HCM", Phone = "0974814543" },
            new Agent { AgentID = 2, AgentName = "Đại lý Hồng Phúc", Address = "45 Quang Trung, Gò Vấp, TP.HCM", Phone = "0939275570" },
            new Agent { AgentID = 3, AgentName = "Cửa hàng Bình An", Address = "78 Lê Văn Việt, TP. Thủ Đức, TP.HCM", Phone = "0916657005" },
            new Agent { AgentID = 4, AgentName = "Siêu thị mini Phương Nam", Address = "210 Phạm Văn Thuận, Biên Hòa, Đồng Nai", Phone = "0915018435" },
            new Agent { AgentID = 5, AgentName = "Đại lý Tân Tiến", Address = "15 Yersin, Thủ Dầu Một, Bình Dương", Phone = "0911518048" },
            new Agent { AgentID = 6, AgentName = "Tạp hóa Ngọc Lan", Address = "92 Nguyễn Trãi, Quận 5, TP.HCM", Phone = "0942003530" },
            new Agent { AgentID = 7, AgentName = "Cửa hàng Thành Đạt", Address = "36 Hùng Vương, TP. Tân An, Long An", Phone = "0945260578" },
            new Agent { AgentID = 8, AgentName = "Đại lý Kim Ngân", Address = "58 Ấp Bắc, TP. Mỹ Tho, Tiền Giang", Phone = "0931954313" },
            new Agent { AgentID = 9, AgentName = "Tạp hóa Hòa Bình", Address = "11 Đồng Khởi, TP. Bến Tre, Bến Tre", Phone = "0946513472" },
            new Agent { AgentID = 10, AgentName = "Nhà phân phối Cửu Long", Address = "204 Trần Hưng Đạo, Ninh Kiều, Cần Thơ", Phone = "0949415025" },
            new Agent { AgentID = 11, AgentName = "Siêu thị Gia Huy", Address = "67 Lê Hồng Phong, TP. Vũng Tàu, Bà Rịa - Vũng Tàu", Phone = "0977807235" },
            new Agent { AgentID = 12, AgentName = "Cửa hàng Thiên Phú", Address = "19 Hoàng Văn Thụ, Tân Bình, TP.HCM", Phone = "0982019999" },
            new Agent { AgentID = 13, AgentName = "Đại lý Quốc Thịnh", Address = "143 Cách Mạng Tháng Tám, Quận 10, TP.HCM", Phone = "0924564694" },
            new Agent { AgentID = 14, AgentName = "Tạp hóa Lộc Phát", Address = "88 Võ Văn Ngân, TP. Thủ Đức, TP.HCM", Phone = "0953014507" },
            new Agent { AgentID = 15, AgentName = "Cửa hàng An Khang", Address = "27 Nguyễn Huệ, TP. Long Xuyên, An Giang", Phone = "0985746235" },
            new Agent { AgentID = 16, AgentName = "Đại lý Hưng Thịnh", Address = "302 Phan Đình Phùng, TP. Đà Lạt, Lâm Đồng", Phone = "0942681680" },
            new Agent { AgentID = 17, AgentName = "Siêu thị mini Vạn Lợi", Address = "54 Trần Phú, TP. Nha Trang, Khánh Hòa", Phone = "0914260997" },
            new Agent { AgentID = 18, AgentName = "Tạp hóa Mai Anh", Address = "9 Lý Thường Kiệt, Quận 11, TP.HCM", Phone = "0997662698" },
            new Agent { AgentID = 19, AgentName = "Đại lý Đông Dương", Address = "176 Nguyễn Ảnh Thủ, Quận 12, TP.HCM", Phone = "0907220595" },
            new Agent { AgentID = 20, AgentName = "Cửa hàng Sao Mai", Address = "41 Phạm Ngọc Thạch, TP. Cà Mau, Cà Mau", Phone = "0971902802" });

        modelBuilder.Entity<User>().HasData(
            new User { UserID = 1, UserName = "admin01", Email = "admin01@b2bwholesale.example", Password = "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7", Lock = false, Role = "Admin", AgentID = null, FailedLoginCount = 0 },
            new User { UserID = 2, UserName = "admin02", Email = "admin02@b2bwholesale.example", Password = "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7", Lock = false, Role = "Admin", AgentID = null, FailedLoginCount = 0 },
            new User { UserID = 3, UserName = "warehouse01", Email = "warehouse01@b2bwholesale.example", Password = "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7", Lock = false, Role = "Admin", AgentID = null, FailedLoginCount = 0 },
            new User { UserID = 4, UserName = "agent01", Email = "agent01@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 1, FailedLoginCount = 0 },
            new User { UserID = 5, UserName = "agent02", Email = "agent02@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 2, FailedLoginCount = 0 },
            new User { UserID = 6, UserName = "agent03", Email = "agent03@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 3, FailedLoginCount = 0 },
            new User { UserID = 7, UserName = "agent04", Email = "agent04@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 4, FailedLoginCount = 0 },
            new User { UserID = 8, UserName = "agent05", Email = "agent05@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 5, FailedLoginCount = 0 },
            new User { UserID = 9, UserName = "agent06", Email = "agent06@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 6, FailedLoginCount = 0 },
            new User { UserID = 10, UserName = "agent07", Email = "agent07@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 7, FailedLoginCount = 0 },
            new User { UserID = 11, UserName = "agent08", Email = "agent08@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 8, FailedLoginCount = 0 },
            new User { UserID = 12, UserName = "agent09", Email = "agent09@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 9, FailedLoginCount = 0 },
            new User { UserID = 13, UserName = "agent10", Email = "agent10@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 10, FailedLoginCount = 0 },
            new User { UserID = 14, UserName = "agent11", Email = "agent11@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 11, FailedLoginCount = 0 },
            new User { UserID = 15, UserName = "agent12", Email = "agent12@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 12, FailedLoginCount = 0 },
            new User { UserID = 16, UserName = "agent13", Email = "agent13@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 13, FailedLoginCount = 0 },
            new User { UserID = 17, UserName = "agent14", Email = "agent14@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 14, FailedLoginCount = 0 },
            new User { UserID = 18, UserName = "agent15", Email = "agent15@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 15, FailedLoginCount = 0 },
            new User { UserID = 19, UserName = "agent16", Email = "agent16@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 16, FailedLoginCount = 0 },
            new User { UserID = 20, UserName = "agent17", Email = "agent17@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 17, FailedLoginCount = 0 },
            new User { UserID = 21, UserName = "agent18", Email = "agent18@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 18, FailedLoginCount = 0 },
            new User { UserID = 22, UserName = "agent19", Email = "agent19@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = true, Role = "Agent", AgentID = 19, FailedLoginCount = 5 },
            new User { UserID = 23, UserName = "agent20", Email = "agent20@b2bwholesale.example", Password = "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", Lock = false, Role = "Agent", AgentID = 20, FailedLoginCount = 0 });

        modelBuilder.Entity<Order>().HasData(
            new Order { OrderID = 1, OrderDate = new DateTime(2026, 9, 1), AgentID = 1, OrderStatus = "Shipped" },
            new Order { OrderID = 2, OrderDate = new DateTime(2026, 9, 4), AgentID = 2, OrderStatus = "Delivered" },
            new Order { OrderID = 3, OrderDate = new DateTime(2026, 9, 8), AgentID = 3, OrderStatus = "Shipped" },
            new Order { OrderID = 4, OrderDate = new DateTime(2026, 9, 11), AgentID = 4, OrderStatus = "Delivered" },
            new Order { OrderID = 5, OrderDate = new DateTime(2026, 9, 15), AgentID = 5, OrderStatus = "Delivered" },
            new Order { OrderID = 6, OrderDate = new DateTime(2026, 9, 19), AgentID = 6, OrderStatus = "Shipped" },
            new Order { OrderID = 7, OrderDate = new DateTime(2026, 9, 22), AgentID = 7, OrderStatus = "Pending" },
            new Order { OrderID = 8, OrderDate = new DateTime(2026, 9, 26), AgentID = 8, OrderStatus = "Delivered" },
            new Order { OrderID = 9, OrderDate = new DateTime(2026, 9, 29), AgentID = 9, OrderStatus = "Cancelled" },
            new Order { OrderID = 10, OrderDate = new DateTime(2026, 10, 3), AgentID = 10, OrderStatus = "Delivered" },
            new Order { OrderID = 11, OrderDate = new DateTime(2026, 10, 7), AgentID = 11, OrderStatus = "Cancelled" },
            new Order { OrderID = 12, OrderDate = new DateTime(2026, 10, 10), AgentID = 12, OrderStatus = "Delivered" },
            new Order { OrderID = 13, OrderDate = new DateTime(2026, 10, 14), AgentID = 13, OrderStatus = "Shipped" },
            new Order { OrderID = 14, OrderDate = new DateTime(2026, 10, 17), AgentID = 14, OrderStatus = "Shipped" },
            new Order { OrderID = 15, OrderDate = new DateTime(2026, 10, 21), AgentID = 15, OrderStatus = "Delivered" },
            new Order { OrderID = 16, OrderDate = new DateTime(2026, 10, 25), AgentID = 16, OrderStatus = "Pending" },
            new Order { OrderID = 17, OrderDate = new DateTime(2026, 10, 28), AgentID = 17, OrderStatus = "Pending" },
            new Order { OrderID = 18, OrderDate = new DateTime(2026, 11, 1), AgentID = 18, OrderStatus = "Pending" },
            new Order { OrderID = 19, OrderDate = new DateTime(2026, 11, 4), AgentID = 1, OrderStatus = "Delivered" },
            new Order { OrderID = 20, OrderDate = new DateTime(2026, 11, 8), AgentID = 4, OrderStatus = "Pending" });

        modelBuilder.Entity<OrderDetail>().HasData(
            new OrderDetail { ID = 1, OrderID = 1, ItemID = 14, Quantity = 25, UnitAmount = 330000m },
            new OrderDetail { ID = 2, OrderID = 2, ItemID = 17, Quantity = 20, UnitAmount = 780000m },
            new OrderDetail { ID = 3, OrderID = 2, ItemID = 3, Quantity = 12, UnitAmount = 230000m },
            new OrderDetail { ID = 4, OrderID = 3, ItemID = 15, Quantity = 40, UnitAmount = 360000m },
            new OrderDetail { ID = 5, OrderID = 4, ItemID = 17, Quantity = 5, UnitAmount = 780000m },
            new OrderDetail { ID = 6, OrderID = 4, ItemID = 20, Quantity = 20, UnitAmount = 300000m },
            new OrderDetail { ID = 7, OrderID = 5, ItemID = 16, Quantity = 30, UnitAmount = 540000m },
            new OrderDetail { ID = 8, OrderID = 6, ItemID = 13, Quantity = 25, UnitAmount = 520000m },
            new OrderDetail { ID = 9, OrderID = 6, ItemID = 6, Quantity = 15, UnitAmount = 650000m },
            new OrderDetail { ID = 10, OrderID = 7, ItemID = 16, Quantity = 30, UnitAmount = 540000m },
            new OrderDetail { ID = 11, OrderID = 8, ItemID = 9, Quantity = 30, UnitAmount = 190000m },
            new OrderDetail { ID = 12, OrderID = 8, ItemID = 5, Quantity = 12, UnitAmount = 115000m },
            new OrderDetail { ID = 13, OrderID = 9, ItemID = 9, Quantity = 40, UnitAmount = 190000m },
            new OrderDetail { ID = 14, OrderID = 10, ItemID = 4, Quantity = 15, UnitAmount = 120000m },
            new OrderDetail { ID = 15, OrderID = 10, ItemID = 3, Quantity = 40, UnitAmount = 230000m },
            new OrderDetail { ID = 16, OrderID = 11, ItemID = 11, Quantity = 40, UnitAmount = 210000m },
            new OrderDetail { ID = 17, OrderID = 12, ItemID = 17, Quantity = 12, UnitAmount = 780000m },
            new OrderDetail { ID = 18, OrderID = 12, ItemID = 6, Quantity = 20, UnitAmount = 650000m },
            new OrderDetail { ID = 19, OrderID = 13, ItemID = 5, Quantity = 15, UnitAmount = 115000m },
            new OrderDetail { ID = 20, OrderID = 14, ItemID = 17, Quantity = 5, UnitAmount = 780000m },
            new OrderDetail { ID = 21, OrderID = 14, ItemID = 5, Quantity = 40, UnitAmount = 115000m },
            new OrderDetail { ID = 22, OrderID = 15, ItemID = 18, Quantity = 12, UnitAmount = 320000m },
            new OrderDetail { ID = 23, OrderID = 16, ItemID = 6, Quantity = 10, UnitAmount = 650000m },
            new OrderDetail { ID = 24, OrderID = 16, ItemID = 13, Quantity = 15, UnitAmount = 520000m },
            new OrderDetail { ID = 25, OrderID = 17, ItemID = 11, Quantity = 15, UnitAmount = 210000m },
            new OrderDetail { ID = 26, OrderID = 18, ItemID = 15, Quantity = 20, UnitAmount = 360000m },
            new OrderDetail { ID = 27, OrderID = 18, ItemID = 10, Quantity = 12, UnitAmount = 85000m },
            new OrderDetail { ID = 28, OrderID = 19, ItemID = 13, Quantity = 30, UnitAmount = 520000m },
            new OrderDetail { ID = 29, OrderID = 20, ItemID = 5, Quantity = 5, UnitAmount = 115000m },
            new OrderDetail { ID = 30, OrderID = 20, ItemID = 18, Quantity = 10, UnitAmount = 320000m });
    }
}