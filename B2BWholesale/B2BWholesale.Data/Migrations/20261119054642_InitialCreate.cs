using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace B2BWholesale.Data.Migrations
{
    /// <inheritdoc /> 
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agent",
                columns: table => new
                {
                    AgentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agent", x => x.AgentID);
                    table.CheckConstraint("CK_Agent_Phone", "[Phone] NOT LIKE '%[^0-9]%' AND LEN([Phone]) BETWEEN 9 AND 11");
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemID);
                    table.CheckConstraint("CK_Item_Price", "[Price] > 0");
                    table.CheckConstraint("CK_Item_Stock", "[StockQuantity] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AgentID = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.OrderID);
                    table.CheckConstraint("CK_Order_Status", "[OrderStatus] IN ('Pending','Shipped','Delivered','Cancelled')");
                    table.ForeignKey(
                        name: "FK_Order_Agent_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agent",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    @lock = table.Column<bool>(name: "lock", type: "bit", nullable: false),
                    Role = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    AgentID = table.Column<int>(type: "int", nullable: true),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_Users_AgentLink", "([Role] = 'Agent' AND [AgentID] IS NOT NULL) OR ([Role] = 'Admin' AND [AgentID] IS NULL)");
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('Admin','Agent')");
                    table.ForeignKey(
                        name: "FK_Users_Agent_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agent",
                        principalColumn: "AgentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.ID);
                    table.CheckConstraint("CK_OrderDetail_Qty", "[Quantity] > 0");
                    table.CheckConstraint("CK_OrderDetail_Unit", "[UnitAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_OrderDetail_Item_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Item",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Order",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Agent",
                columns: new[] { "AgentID", "Address", "AgentName", "Phone" },
                values: new object[,]
                {
                    { 1, "123 Nguyễn Văn Linh, Quận 7, TP.HCM", "Tạp hóa Minh Phát", "0974814543" },
                    { 2, "45 Quang Trung, Gò Vấp, TP.HCM", "Đại lý Hồng Phúc", "0939275570" },
                    { 3, "78 Lê Văn Việt, TP. Thủ Đức, TP.HCM", "Cửa hàng Bình An", "0916657005" },
                    { 4, "210 Phạm Văn Thuận, Biên Hòa, Đồng Nai", "Siêu thị mini Phương Nam", "0915018435" },
                    { 5, "15 Yersin, Thủ Dầu Một, Bình Dương", "Đại lý Tân Tiến", "0911518048" },
                    { 6, "92 Nguyễn Trãi, Quận 5, TP.HCM", "Tạp hóa Ngọc Lan", "0942003530" },
                    { 7, "36 Hùng Vương, TP. Tân An, Long An", "Cửa hàng Thành Đạt", "0945260578" },
                    { 8, "58 Ấp Bắc, TP. Mỹ Tho, Tiền Giang", "Đại lý Kim Ngân", "0931954313" },
                    { 9, "11 Đồng Khởi, TP. Bến Tre, Bến Tre", "Tạp hóa Hòa Bình", "0946513472" },
                    { 10, "204 Trần Hưng Đạo, Ninh Kiều, Cần Thơ", "Nhà phân phối Cửu Long", "0949415025" },
                    { 11, "67 Lê Hồng Phong, TP. Vũng Tàu, Bà Rịa - Vũng Tàu", "Siêu thị Gia Huy", "0977807235" },
                    { 12, "19 Hoàng Văn Thụ, Tân Bình, TP.HCM", "Cửa hàng Thiên Phú", "0982019999" },
                    { 13, "143 Cách Mạng Tháng Tám, Quận 10, TP.HCM", "Đại lý Quốc Thịnh", "0924564694" },
                    { 14, "88 Võ Văn Ngân, TP. Thủ Đức, TP.HCM", "Tạp hóa Lộc Phát", "0953014507" },
                    { 15, "27 Nguyễn Huệ, TP. Long Xuyên, An Giang", "Cửa hàng An Khang", "0985746235" },
                    { 16, "302 Phan Đình Phùng, TP. Đà Lạt, Lâm Đồng", "Đại lý Hưng Thịnh", "0942681680" },
                    { 17, "54 Trần Phú, TP. Nha Trang, Khánh Hòa", "Siêu thị mini Vạn Lợi", "0914260997" },
                    { 18, "9 Lý Thường Kiệt, Quận 11, TP.HCM", "Tạp hóa Mai Anh", "0997662698" },
                    { 19, "176 Nguyễn Ảnh Thủ, Quận 12, TP.HCM", "Đại lý Đông Dương", "0907220595" },
                    { 20, "41 Phạm Ngọc Thạch, TP. Cà Mau, Cà Mau", "Cửa hàng Sao Mai", "0971902802" }
                });

            migrationBuilder.InsertData(
                table: "Item",
                columns: new[] { "ItemID", "ItemName", "Price", "Size", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Nước mắm Nam Ngư", 420000m, "12 chai x 500ml", 180 },
                    { 2, "Dầu ăn Neptune", 540000m, "12 chai x 1L", 150 },
                    { 3, "Đường tinh luyện Biên Hòa", 230000m, "10 gói x 1kg", 300 },
                    { 4, "Muối i-ốt Bạc Liêu", 120000m, "20 gói x 500g", 250 },
                    { 5, "Mì gói Hảo Hảo tôm chua cay", 115000m, "30 gói x 75g", 600 },
                    { 6, "Gạo ST25", 650000m, "Bao 25kg", 120 },
                    { 7, "Sữa đặc Ông Thọ", 960000m, "48 lon x 380g", 90 },
                    { 8, "Cà phê hòa tan G7 3in1", 1250000m, "24 hộp x 16 gói", 70 },
                    { 9, "Trà xanh C2 hương chanh", 190000m, "24 chai x 455ml", 400 },
                    { 10, "Nước suối Aquafina", 85000m, "24 chai x 500ml", 800 },
                    { 11, "Nước ngọt Coca-Cola", 210000m, "24 lon x 330ml", 500 },
                    { 12, "Nước tương Maggi", 410000m, "24 chai x 300ml", 140 },
                    { 13, "Bột giặt OMO Matic", 520000m, "4 túi x 3kg", 110 },
                    { 14, "Nước rửa chén Sunlight", 330000m, "12 chai x 750ml", 130 },
                    { 15, "Giấy vệ sinh Pulppy", 360000m, "6 lốc x 10 cuộn", 200 },
                    { 16, "Xà phòng Lifebuoy", 540000m, "72 cục x 90g", 95 },
                    { 17, "Dầu gội Clear Men", 780000m, "6 chai x 650g", 12 },
                    { 18, "Sữa tươi Vinamilk 100%", 320000m, "12 hộp x 1L", 160 },
                    { 19, "Bánh quy Cosy", 280000m, "24 gói x 150g", 8 },
                    { 20, "Snack Oishi tôm cay", 300000m, "40 gói x 40g", 220 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AgentID", "email", "FailedLoginCount", "lock", "password", "Role", "UserName" },
                values: new object[,]
                {
                    { 1, null, "admin01@b2bwholesale.example", 0, false, "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7", "Admin", "admin01" },
                    { 2, null, "admin02@b2bwholesale.example", 0, false, "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7", "Admin", "admin02" },
                    { 3, null, "warehouse01@b2bwholesale.example", 0, false, "E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7", "Admin", "warehouse01" }
                });

            migrationBuilder.InsertData(
                table: "Order",
                columns: new[] { "OrderID", "AgentID", "OrderDate", "OrderStatus" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 2, 2, new DateTime(2026, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 3, 3, new DateTime(2026, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 4, 4, new DateTime(2026, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 5, 5, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 6, 6, new DateTime(2026, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 7, 7, new DateTime(2026, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 8, 8, new DateTime(2026, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 9, 9, new DateTime(2026, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cancelled" },
                    { 10, 10, new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 11, 11, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cancelled" },
                    { 12, 12, new DateTime(2026, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 13, 13, new DateTime(2026, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 14, 14, new DateTime(2026, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped" },
                    { 15, 15, new DateTime(2026, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 16, 16, new DateTime(2026, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 17, 17, new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 18, 18, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 19, 1, new DateTime(2026, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered" },
                    { 20, 4, new DateTime(2026, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pending" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AgentID", "email", "FailedLoginCount", "lock", "password", "Role", "UserName" },
                values: new object[,]
                {
                    { 4, 1, "agent01@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent01" },
                    { 5, 2, "agent02@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent02" },
                    { 6, 3, "agent03@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent03" },
                    { 7, 4, "agent04@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent04" },
                    { 8, 5, "agent05@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent05" },
                    { 9, 6, "agent06@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent06" },
                    { 10, 7, "agent07@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent07" },
                    { 11, 8, "agent08@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent08" },
                    { 12, 9, "agent09@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent09" },
                    { 13, 10, "agent10@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent10" },
                    { 14, 11, "agent11@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent11" },
                    { 15, 12, "agent12@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent12" },
                    { 16, 13, "agent13@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent13" },
                    { 17, 14, "agent14@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent14" },
                    { 18, 15, "agent15@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent15" },
                    { 19, 16, "agent16@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent16" },
                    { 20, 17, "agent17@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent17" },
                    { 21, 18, "agent18@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent18" },
                    { 22, 19, "agent19@b2bwholesale.example", 5, true, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent19" },
                    { 23, 20, "agent20@b2bwholesale.example", 0, false, "C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34", "Agent", "agent20" }
                });

            migrationBuilder.InsertData(
                table: "OrderDetail",
                columns: new[] { "ID", "ItemID", "OrderID", "Quantity", "UnitAmount" },
                values: new object[,]
                {
                    { 1, 14, 1, 25, 330000m },
                    { 2, 17, 2, 20, 780000m },
                    { 3, 3, 2, 12, 230000m },
                    { 4, 15, 3, 40, 360000m },
                    { 5, 17, 4, 5, 780000m },
                    { 6, 20, 4, 20, 300000m },
                    { 7, 16, 5, 30, 540000m },
                    { 8, 13, 6, 25, 520000m },
                    { 9, 6, 6, 15, 650000m },
                    { 10, 16, 7, 30, 540000m },
                    { 11, 9, 8, 30, 190000m },
                    { 12, 5, 8, 12, 115000m },
                    { 13, 9, 9, 40, 190000m },
                    { 14, 4, 10, 15, 120000m },
                    { 15, 3, 10, 40, 230000m },
                    { 16, 11, 11, 40, 210000m },
                    { 17, 17, 12, 12, 780000m },
                    { 18, 6, 12, 20, 650000m },
                    { 19, 5, 13, 15, 115000m },
                    { 20, 17, 14, 5, 780000m },
                    { 21, 5, 14, 40, 115000m },
                    { 22, 18, 15, 12, 320000m },
                    { 23, 6, 16, 10, 650000m },
                    { 24, 13, 16, 15, 520000m },
                    { 25, 11, 17, 15, 210000m },
                    { 26, 15, 18, 20, 360000m },
                    { 27, 10, 18, 12, 85000m },
                    { 28, 13, 19, 30, 520000m },
                    { 29, 5, 20, 5, 115000m },
                    { 30, 18, 20, 10, 320000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agent_Phone",
                table: "Agent",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemName_Size",
                table: "Item",
                columns: new[] { "ItemName", "Size" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_AgentID",
                table: "Order",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ItemID",
                table: "OrderDetail",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderID_ItemID",
                table: "OrderDetail",
                columns: new[] { "OrderID", "ItemID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AgentID",
                table: "Users",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Agent");
        }
    }
}
