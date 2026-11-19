/* ============================================================================
   B2B Wholesale Order Management System  -  SE Lab Assignment 2 (Sem 1_2627)
   Database: B2BWholesaleDB   |   Target: Microsoft SQL Server 2017+ / LocalDB
   Run this whole script once in SSMS (or sqlcmd). It can be re-run safely:
   it drops and recreates all five tables.

   DESIGN DECISIONS (explain these in Report.docx, section Database)
   - [Order] and [lock] are written in brackets because ORDER is a reserved word.
   - Order -> Agent        : ON DELETE NO ACTION  (cannot delete an agent who has orders)
   - OrderDetail -> Order  : ON DELETE CASCADE    (deleting an order removes its lines)
   - OrderDetail -> Item   : ON DELETE NO ACTION  (cannot delete an item that was sold)
   - Users -> Agent        : ON DELETE CASCADE    (deleting an agent removes the login)
   - ON UPDATE CASCADE on every FK (keeps child keys in step if a key is ever changed)
   - Item.StockQuantity has CHECK (>= 0): stock can never go negative
   - OrderDetail.UnitAmount = unit price AT THE TIME OF ORDER (price history is kept)
   - Users.password stores a SHA-256 hex hash (uppercase), never plain text.
     Demo passwords: Admin@123 (Admin users) and Agent@123 (Agent users).
     The WinForms and Web apps must hash the typed password with SHA-256 and compare.
   - Extra columns beyond the assignment: Users.AgentID (links an Agent login to its
     Agent record, needed for "My Orders") and Users.FailedLoginCount (lockout rule).
   All data is fictional sample data.
   ========================================================================== */

IF DB_ID(N'B2BWholesaleDB') IS NULL
    CREATE DATABASE B2BWholesaleDB;
GO
USE B2BWholesaleDB;
GO

/* ---------- 1. Drop old tables (children first) ---------- */
IF OBJECT_ID(N'dbo.OrderDetail','U') IS NOT NULL DROP TABLE dbo.OrderDetail;
IF OBJECT_ID(N'dbo.[Order]','U')     IS NOT NULL DROP TABLE dbo.[Order];
IF OBJECT_ID(N'dbo.Users','U')       IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Item','U')        IS NOT NULL DROP TABLE dbo.Item;
IF OBJECT_ID(N'dbo.Agent','U')       IS NOT NULL DROP TABLE dbo.Agent;
GO

/* ---------- 2. Create tables ---------- */
CREATE TABLE dbo.Item (
    ItemID        INT            IDENTITY(1,1) NOT NULL,
    ItemName      NVARCHAR(100)  NOT NULL,
    Size          NVARCHAR(50)   NOT NULL,
    Price         DECIMAL(18,2)  NOT NULL,
    StockQuantity INT            NOT NULL CONSTRAINT DF_Item_Stock DEFAULT (0),
    CONSTRAINT PK_Item        PRIMARY KEY (ItemID),
    CONSTRAINT UQ_Item_NameSize UNIQUE (ItemName, Size),
    CONSTRAINT CK_Item_Price  CHECK (Price > 0),
    CONSTRAINT CK_Item_Stock  CHECK (StockQuantity >= 0)
);

CREATE TABLE dbo.Agent (
    AgentID   INT           IDENTITY(1,1) NOT NULL,
    AgentName NVARCHAR(100) NOT NULL,
    Address   NVARCHAR(200) NOT NULL,
    Phone     VARCHAR(15)   NOT NULL,
    CONSTRAINT PK_Agent       PRIMARY KEY (AgentID),
    CONSTRAINT UQ_Agent_Phone UNIQUE (Phone),
    CONSTRAINT CK_Agent_Phone CHECK (Phone NOT LIKE '%[^0-9]%' AND LEN(Phone) BETWEEN 9 AND 11)
);

CREATE TABLE dbo.[Order] (
    OrderID     INT         IDENTITY(1,1) NOT NULL,
    OrderDate   DATETIME    NOT NULL CONSTRAINT DF_Order_Date   DEFAULT (GETDATE()),
    AgentID     INT         NOT NULL,
    OrderStatus VARCHAR(20) NOT NULL CONSTRAINT DF_Order_Status DEFAULT ('Pending'),
    CONSTRAINT PK_Order PRIMARY KEY (OrderID),
    CONSTRAINT FK_Order_Agent FOREIGN KEY (AgentID) REFERENCES dbo.Agent(AgentID)
        ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT CK_Order_Status CHECK (OrderStatus IN ('Pending','Shipped','Delivered','Cancelled'))
);

CREATE TABLE dbo.OrderDetail (
    ID         INT           IDENTITY(1,1) NOT NULL,
    OrderID    INT           NOT NULL,
    ItemID     INT           NOT NULL,
    Quantity   INT           NOT NULL,
    UnitAmount DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_OrderDetail PRIMARY KEY (ID),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID) REFERENCES dbo.[Order](OrderID)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_OrderDetail_Item FOREIGN KEY (ItemID) REFERENCES dbo.Item(ItemID)
        ON DELETE NO ACTION ON UPDATE CASCADE,
    CONSTRAINT UQ_OrderDetail_OrderItem UNIQUE (OrderID, ItemID),
    CONSTRAINT CK_OrderDetail_Qty  CHECK (Quantity > 0),
    CONSTRAINT CK_OrderDetail_Unit CHECK (UnitAmount >= 0)
);

CREATE TABLE dbo.Users (
    UserID           INT           IDENTITY(1,1) NOT NULL,
    UserName         NVARCHAR(50)  NOT NULL,
    email            NVARCHAR(100) NOT NULL,
    [password]       VARCHAR(64)   NOT NULL,   -- SHA-256 hex hash
    [lock]           BIT           NOT NULL CONSTRAINT DF_Users_Lock DEFAULT (0),
    [Role]           VARCHAR(10)   NOT NULL,
    AgentID          INT           NULL,
    FailedLoginCount INT           NOT NULL CONSTRAINT DF_Users_Failed DEFAULT (0),
    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_UserName UNIQUE (UserName),
    CONSTRAINT UQ_Users_Email    UNIQUE (email),
    CONSTRAINT CK_Users_Role     CHECK ([Role] IN ('Admin','Agent')),
    CONSTRAINT CK_Users_AgentLink CHECK (([Role] = 'Agent' AND AgentID IS NOT NULL)
                                      OR ([Role] = 'Admin' AND AgentID IS NULL)),
    CONSTRAINT FK_Users_Agent FOREIGN KEY (AgentID) REFERENCES dbo.Agent(AgentID)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

/* ---------- 3. Seed data ---------- */
SET IDENTITY_INSERT dbo.Item ON;
INSERT INTO dbo.Item (ItemID, ItemName, Size, Price, StockQuantity) VALUES
(1, N'Nước mắm Nam Ngư', N'12 chai x 500ml', 420000, 180),
(2, N'Dầu ăn Neptune', N'12 chai x 1L', 540000, 150),
(3, N'Đường tinh luyện Biên Hòa', N'10 gói x 1kg', 230000, 300),
(4, N'Muối i-ốt Bạc Liêu', N'20 gói x 500g', 120000, 250),
(5, N'Mì gói Hảo Hảo tôm chua cay', N'30 gói x 75g', 115000, 600),
(6, N'Gạo ST25', N'Bao 25kg', 650000, 120),
(7, N'Sữa đặc Ông Thọ', N'48 lon x 380g', 960000, 90),
(8, N'Cà phê hòa tan G7 3in1', N'24 hộp x 16 gói', 1250000, 70),
(9, N'Trà xanh C2 hương chanh', N'24 chai x 455ml', 190000, 400),
(10, N'Nước suối Aquafina', N'24 chai x 500ml', 85000, 800),
(11, N'Nước ngọt Coca-Cola', N'24 lon x 330ml', 210000, 500),
(12, N'Nước tương Maggi', N'24 chai x 300ml', 410000, 140),
(13, N'Bột giặt OMO Matic', N'4 túi x 3kg', 520000, 110),
(14, N'Nước rửa chén Sunlight', N'12 chai x 750ml', 330000, 130),
(15, N'Giấy vệ sinh Pulppy', N'6 lốc x 10 cuộn', 360000, 200),
(16, N'Xà phòng Lifebuoy', N'72 cục x 90g', 540000, 95),
(17, N'Dầu gội Clear Men', N'6 chai x 650g', 780000, 12),
(18, N'Sữa tươi Vinamilk 100%', N'12 hộp x 1L', 320000, 160),
(19, N'Bánh quy Cosy', N'24 gói x 150g', 280000, 8),
(20, N'Snack Oishi tôm cay', N'40 gói x 40g', 300000, 220);
SET IDENTITY_INSERT dbo.Item OFF;
GO

SET IDENTITY_INSERT dbo.Agent ON;
INSERT INTO dbo.Agent (AgentID, AgentName, Address, Phone) VALUES
(1, N'Tạp hóa Minh Phát', N'123 Nguyễn Văn Linh, Quận 7, TP.HCM', '0974814543'),
(2, N'Đại lý Hồng Phúc', N'45 Quang Trung, Gò Vấp, TP.HCM', '0939275570'),
(3, N'Cửa hàng Bình An', N'78 Lê Văn Việt, TP. Thủ Đức, TP.HCM', '0916657005'),
(4, N'Siêu thị mini Phương Nam', N'210 Phạm Văn Thuận, Biên Hòa, Đồng Nai', '0915018435'),
(5, N'Đại lý Tân Tiến', N'15 Yersin, Thủ Dầu Một, Bình Dương', '0911518048'),
(6, N'Tạp hóa Ngọc Lan', N'92 Nguyễn Trãi, Quận 5, TP.HCM', '0942003530'),
(7, N'Cửa hàng Thành Đạt', N'36 Hùng Vương, TP. Tân An, Long An', '0945260578'),
(8, N'Đại lý Kim Ngân', N'58 Ấp Bắc, TP. Mỹ Tho, Tiền Giang', '0931954313'),
(9, N'Tạp hóa Hòa Bình', N'11 Đồng Khởi, TP. Bến Tre, Bến Tre', '0946513472'),
(10, N'Nhà phân phối Cửu Long', N'204 Trần Hưng Đạo, Ninh Kiều, Cần Thơ', '0949415025'),
(11, N'Siêu thị Gia Huy', N'67 Lê Hồng Phong, TP. Vũng Tàu, Bà Rịa - Vũng Tàu', '0977807235'),
(12, N'Cửa hàng Thiên Phú', N'19 Hoàng Văn Thụ, Tân Bình, TP.HCM', '0982019999'),
(13, N'Đại lý Quốc Thịnh', N'143 Cách Mạng Tháng Tám, Quận 10, TP.HCM', '0924564694'),
(14, N'Tạp hóa Lộc Phát', N'88 Võ Văn Ngân, TP. Thủ Đức, TP.HCM', '0953014507'),
(15, N'Cửa hàng An Khang', N'27 Nguyễn Huệ, TP. Long Xuyên, An Giang', '0985746235'),
(16, N'Đại lý Hưng Thịnh', N'302 Phan Đình Phùng, TP. Đà Lạt, Lâm Đồng', '0942681680'),
(17, N'Siêu thị mini Vạn Lợi', N'54 Trần Phú, TP. Nha Trang, Khánh Hòa', '0914260997'),
(18, N'Tạp hóa Mai Anh', N'9 Lý Thường Kiệt, Quận 11, TP.HCM', '0997662698'),
(19, N'Đại lý Đông Dương', N'176 Nguyễn Ảnh Thủ, Quận 12, TP.HCM', '0907220595'),
(20, N'Cửa hàng Sao Mai', N'41 Phạm Ngọc Thạch, TP. Cà Mau, Cà Mau', '0971902802');
SET IDENTITY_INSERT dbo.Agent OFF;
GO

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserID, UserName, email, [password], [lock], [Role], AgentID, FailedLoginCount) VALUES
(1, N'admin01', N'admin01@b2bwholesale.example', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', 0, 'Admin', NULL, 0),
(2, N'admin02', N'admin02@b2bwholesale.example', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', 0, 'Admin', NULL, 0),
(3, N'warehouse01', N'warehouse01@b2bwholesale.example', 'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', 0, 'Admin', NULL, 0),
(4, N'agent01', N'agent01@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 1, 0),
(5, N'agent02', N'agent02@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 2, 0),
(6, N'agent03', N'agent03@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 3, 0),
(7, N'agent04', N'agent04@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 4, 0),
(8, N'agent05', N'agent05@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 5, 0),
(9, N'agent06', N'agent06@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 6, 0),
(10, N'agent07', N'agent07@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 7, 0),
(11, N'agent08', N'agent08@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 8, 0),
(12, N'agent09', N'agent09@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 9, 0),
(13, N'agent10', N'agent10@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 10, 0),
(14, N'agent11', N'agent11@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 11, 0),
(15, N'agent12', N'agent12@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 12, 0),
(16, N'agent13', N'agent13@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 13, 0),
(17, N'agent14', N'agent14@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 14, 0),
(18, N'agent15', N'agent15@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 15, 0),
(19, N'agent16', N'agent16@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 16, 0),
(20, N'agent17', N'agent17@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 17, 0),
(21, N'agent18', N'agent18@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 18, 0),
(22, N'agent19', N'agent19@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 1, 'Agent', 19, 5),
(23, N'agent20', N'agent20@b2bwholesale.example', 'C3206CA954A4953C1C1B82B4167BD097DEAB0E228207E3B3296237E369F39D34', 0, 'Agent', 20, 0);
SET IDENTITY_INSERT dbo.Users OFF;
GO

SET IDENTITY_INSERT dbo.[Order] ON;
INSERT INTO dbo.[Order] (OrderID, OrderDate, AgentID, OrderStatus) VALUES
(1, '20260901', 1, 'Shipped'),
(2, '20260904', 2, 'Delivered'),
(3, '20260908', 3, 'Shipped'),
(4, '20260911', 4, 'Delivered'),
(5, '20260915', 5, 'Delivered'),
(6, '20260919', 6, 'Shipped'),
(7, '20260922', 7, 'Pending'),
(8, '20260926', 8, 'Delivered'),
(9, '20260929', 9, 'Cancelled'),
(10, '20261003', 10, 'Delivered'),
(11, '20261007', 11, 'Cancelled'),
(12, '20261010', 12, 'Delivered'),
(13, '20261014', 13, 'Shipped'),
(14, '20261017', 14, 'Shipped'),
(15, '20261021', 15, 'Delivered'),
(16, '20261025', 16, 'Pending'),
(17, '20261028', 17, 'Pending'),
(18, '20261101', 18, 'Pending'),
(19, '20261104', 1, 'Delivered'),
(20, '20261108', 4, 'Pending');
SET IDENTITY_INSERT dbo.[Order] OFF;
GO

SET IDENTITY_INSERT dbo.OrderDetail ON;
INSERT INTO dbo.OrderDetail (ID, OrderID, ItemID, Quantity, UnitAmount) VALUES
(1, 1, 14, 25, 330000),
(2, 2, 17, 20, 780000),
(3, 2, 3, 12, 230000),
(4, 3, 15, 40, 360000),
(5, 4, 17, 5, 780000),
(6, 4, 20, 20, 300000),
(7, 5, 16, 30, 540000),
(8, 6, 13, 25, 520000),
(9, 6, 6, 15, 650000),
(10, 7, 16, 30, 540000),
(11, 8, 9, 30, 190000),
(12, 8, 5, 12, 115000),
(13, 9, 9, 40, 190000),
(14, 10, 4, 15, 120000),
(15, 10, 3, 40, 230000),
(16, 11, 11, 40, 210000),
(17, 12, 17, 12, 780000),
(18, 12, 6, 20, 650000),
(19, 13, 5, 15, 115000),
(20, 14, 17, 5, 780000),
(21, 14, 5, 40, 115000),
(22, 15, 18, 12, 320000),
(23, 16, 6, 10, 650000),
(24, 16, 13, 15, 520000),
(25, 17, 11, 15, 210000),
(26, 18, 15, 20, 360000),
(27, 18, 10, 12, 85000),
(28, 19, 13, 30, 520000),
(29, 20, 5, 5, 115000),
(30, 20, 18, 10, 320000);
SET IDENTITY_INSERT dbo.OrderDetail OFF;
GO

/* ---------- 4. Self-check queries (run after the script) ---------- */
SELECT 'Item' AS TableName, COUNT(*) AS [Rows] FROM dbo.Item
UNION ALL SELECT 'Agent',       COUNT(*) FROM dbo.Agent
UNION ALL SELECT 'Order',       COUNT(*) FROM dbo.[Order]
UNION ALL SELECT 'OrderDetail', COUNT(*) FROM dbo.OrderDetail
UNION ALL SELECT 'Users',       COUNT(*) FROM dbo.Users;   -- each must be 15 to 30
GO

/* ---------- 5. Integrity tests (each should FAIL with an error: take screenshots) ----------
-- a) Missing parent:      INSERT INTO dbo.[Order] (AgentID) VALUES (9999);
-- b) Negative stock:      UPDATE dbo.Item SET StockQuantity = -1 WHERE ItemID = 1;
-- c) Delete agent w/ orders: DELETE FROM dbo.Agent WHERE AgentID = 1;
-- d) Bad status:          UPDATE dbo.[Order] SET OrderStatus = 'Lost' WHERE OrderID = 1;
-- e) Duplicate username:  INSERT INTO dbo.Users (UserName,email,[password],[Role]) VALUES ('admin01','x@x.com','x','Admin');
-- f) Cascade test (should work):  BEGIN TRAN; DELETE FROM dbo.[Order] WHERE OrderID = 2;
--    SELECT * FROM dbo.OrderDetail WHERE OrderID = 2;  -- 0 rows
--    ROLLBACK;
------------------------------------------------------------------------------------ */
