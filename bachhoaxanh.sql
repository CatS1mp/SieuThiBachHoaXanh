USE BachHoaXanh
GO


DROP TABLE IF EXISTS PromotionDetails;
DROP TABLE IF EXISTS Promotions;
DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS ProductImages;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS SubCategories;
DROP TABLE IF EXISTS FavoriteProducts;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS PaymentMethods;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Addresses;
DROP TABLE IF EXISTS ProductStocks;
DROP TABLE IF EXISTS FaceData;
DROP TABLE IF EXISTS FaceAuthHistory;
DROP TABLE IF EXISTS __EFMigrationsHistory;

GO

-- Tạo bảng Users để quản lý thông tin người dùng
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    UserName VARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(15),
    Address NVARCHAR(255),
    Points decimal(18, 2) NOT NULL DEFAULT 0,
    Rank NVARCHAR(50) NOT NULL DEFAULT N'Chưa xếp hạng',
    Role NVARCHAR(20) DEFAULT 'Customer', -- Quản lý phân quyền: Admin, Customer
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);

CREATE TABLE SubCategories (
    SubCategoryID INT PRIMARY KEY IDENTITY(1,1),
    SubCategoryName NVARCHAR(100) NOT NULL,
    CategoryID INT NOT NULL, 
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE
);

-- Tạo bảng Products để quản lý sản phẩm
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(250) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    SubCategoryID INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    Status TINYINT DEFAULT 2, -- Sản phẩm có khả dụng không
    FOREIGN KEY (SubCategoryID) REFERENCES SubCategories(SubCategoryID) ON DELETE SET NULL
);

CREATE TABLE ProductStocks (
    StockID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT,
    Quantity INT DEFAULT 0,
    ExpirationDate DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);

CREATE TABLE FavoriteProducts(
	FavoriteID INT PRIMARY KEY IDENTITY(1,1),
	UserID INT NOT NULL,
	ProductID INT NOT NULL,
	FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE,
	FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Tạo bảng ProductImages để lưu đường dẫn ảnh sản phẩm
CREATE TABLE ProductImages (
    ImageID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT,
    ImagePath NVARCHAR(255) NOT NULL,
    IsMainImage BIT DEFAULT 0, -- Đánh dấu ảnh chính
FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);

-- Tạo bảng PaymentMethods để quản lý các phương thức thanh toán
CREATE TABLE PaymentMethods (
    PaymentMethodID INT PRIMARY KEY IDENTITY(1,1),
    MethodName NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1
);

-- Tạo bảng Orders để quản lý đơn hàng
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    TotalAmount DECIMAL(18, 2),
    PaymentMethodID INT,
    OrderStatus NVARCHAR(50) DEFAULT 'Pending', -- Trạng thái đơn hàng: Pending, Confirmed, Cancelled
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    ShippingAddress NVARCHAR(255),
	Note NVARCHAR(255),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (PaymentMethodID) REFERENCES PaymentMethods(PaymentMethodID)
);

-- Tạo bảng OrderDetails để quản lý chi tiết từng đơn hàng
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    ProductID INT,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    TotalPrice AS (Quantity * UnitPrice), -- Tính tổng giá tự động
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

CREATE TABLE Addresses (
    AddressID INT IDENTITY (1, 1) NOT NULL,
    UserID INT NOT NULL,
    Province NVARCHAR (100) NULL,
    District NVARCHAR (100) NULL,
    Ward NVARCHAR (100) NULL,
    Street NVARCHAR (255) NOT NULL,
    IsDefault BIT DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED (AddressID ASC),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

CREATE TABLE FaceData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    FaceEmbedding VARBINARY(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE FaceAuthHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    AttemptTime DATETIME DEFAULT GETDATE(),
    Result NVARCHAR(50) NOT NULL, -- 'Success' or 'Failed'
    FailedImagePath NVARCHAR(255) NULL, -- Ảnh nếu thất bại
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);


CREATE TABLE __EFMigrationsHistory (
    MigrationId nvarchar(150) NOT NULL,
    ProductVersion nvarchar(32) NOT NULL,
    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
);

-- Inserting data for Categories (focused on greens, packaged food, and related essentials)
INSERT INTO Categories (CategoryName) 
VALUES 
(N'Rau củ quả tươi'),
(N'Trái cây tươi'),
(N'Thực phẩm đóng gói'),
(N'Thịt & Hải sản tươi'),
(N'Thực phẩm đông lạnh'),
(N'Nước giải khát'),
(N'Gia vị & Nước chấm'),
(N'Đồ ăn vặt'),
(N'Sản phẩm từ sữa'),
(N'Chăm sóc cá nhân');

-- Inserting data for SubCategories (logically grouped for Bách Hóa Xanh)
INSERT INTO SubCategories (SubCategoryName, CategoryID) 
VALUES 
(N'Rau lá xanh', 1),          -- Fresh greens like spinach, lettuce
(N'Củ & Rễ', 1),             -- Root vegetables like potatoes, carrots
(N'Nấm tươi', 1),            -- Fresh mushrooms
(N'Táo & Lê', 2),            -- Apples and pears
(N'Trái cây nhiệt đới', 2),  -- Tropical fruits like mango, pineapple
(N'Cam & Quýt', 2),          -- Citrus fruits
(N'Mì & Bún khô', 3),        -- Packaged noodles
(N'Gạo & Ngũ cốc', 3),       -- Rice and cereals
(N'Đồ hộp', 3),              -- Canned goods
(N'Thịt heo tươi', 4),       -- Fresh pork
(N'Thịt gà tươi', 4),        -- Fresh chicken
(N'Hải sản tươi', 4),        -- Fresh seafood
(N'Thịt đông lạnh', 5),      -- Frozen meat
(N'Hải sản đông lạnh', 5),   -- Frozen seafood
(N'Nước ngọt', 6),           -- Soft drinks
(N'Nước trái cây', 6),       -- Fruit juices
(N'Nước suối', 6),           -- Bottled water
(N'Nước mắm & Nước chấm', 7),-- Fish sauce and dipping sauces
(N'Gia vị khô', 7),          -- Dry spices
(N'Bánh kẹo', 8),            -- Cookies and candies
(N'Snack mặn', 8),           -- Savory snacks
(N'Sữa tươi & Sữa chua', 9), -- Milk and yogurt
(N'Phô mai', 9),             -- Cheese
(N'Sữa tắm & Dầu gội', 10),  -- Body wash and shampoo
(N'Kem đánh răng', 10);      -- Toothpaste

-- Inserting data for Products (aligned with Bách Hóa Xanh's focus)
INSERT INTO Products (ProductName, Description, Price, SubCategoryID) 
VALUES 
(N'Rau muống', N'Rau muống tươi, bó 500g', 15000, 1),
(N'Cải thìa', N'Rau cải thìa tươi, bó 300g', 12000, 1),
(N'Khoai tây', N'Khoai tây tươi, túi 1kg', 35000, 2),
(N'Cà rốt', N'Cà rốt tươi, túi 1kg', 30000, 2),
(N'Nấm bào ngư', N'Nấm bào ngư tươi, gói 200g', 20000, 3),
(N'Táo Fuji', N'Táo Fuji nhập khẩu, túi 1kg', 60000, 4),
(N'Lê Hàn Quốc', N'Lê ngọt, túi 1kg', 55000, 4),
(N'Xoài cát', N'Xoài cát tươi, túi 1kg', 45000, 5),
(N'Dứa', N'Dứa tươi, quả 1kg', 25000, 5),
(N'Cam sành', N'Cam sành tươi, túi 1kg', 35000, 6),
(N'Mì Hảo Hảo', N'Mì ăn liền Hảo Hảo, gói 75g', 5000, 7),
(N'Gạo ST25', N'Gạo thơm ST25, túi 5kg', 120000, 8),
(N'Cá ngừ đóng hộp', N'Cá ngừ ngâm dầu, hộp 185g', 35000, 9),
(N'Thịt ba chỉ heo', N'Thịt ba chỉ heo tươi, 500g', 85000, 10),
(N'Đùi gà tươi', N'Đùi gà tươi, 500g', 60000, 11),
(N'Tôm sú tươi', N'Tôm sú tươi, 500g', 150000, 12),
(N'Thịt bò đông lạnh', N'Thịt bò nhập khẩu đông lạnh, 500g', 120000, 13),
(N'Mực ống đông lạnh', N'Mực ống đông lạnh, 500g', 100000, 14),
(N'Nước ngọt Pepsi', N'Pepsi chai 1.5L', 15000, 15),
(N'Nước ép cam', N'Nước ép cam nguyên chất, chai 500ml', 25000, 16),
(N'Nước suối Lavie', N'Nước suối Lavie, chai 500ml', 8000, 17),
(N'Nước mắm Nam Ngư', N'Nước mắm nguyên chất, chai 750ml', 45000, 18),
(N'Tiêu đen', N'Hạt tiêu đen, gói 50g', 20000, 19),
(N'Bánh Oreo', N'Bánh Oreo, gói 200g', 45000, 20),
(N'Snack khoai tây', N'Snack khoai tây vị BBQ, gói 100g', 20000, 21),
(N'Sữa tươi Vinamilk', N'Sữa tươi tiệt trùng, hộp 1L', 35000, 22),
(N'Sữa chua Vinamilk', N'Sữa chua có đường, hộp 100g', 6000, 22),
(N'Phô mai Con Bò Cười', N'Phô mai Con Bò Cười, hộp 120g', 40000, 23),
(N'Sữa tắm Dove', N'Sữa tắm Dove dưỡng ẩm, chai 500ml', 120000, 24),
(N'Kem đánh răng Colgate', N'Kem đánh răng Colgate, tuýp 100g', 35000, 25);

UPDATE Products
SET Status = 0

-- Declare variables
DECLARE @RowCount INT = @@ROWCOUNT; -- Get the number of inserted rows
DECLARE @StartID INT = SCOPE_IDENTITY() - @RowCount + 1; -- Get the first ProductID
DECLARE @CurrentID INT = @StartID; -- Start with the first ProductID

-- Loop from 1 to the number of rows to insert 2 images per product
WHILE @CurrentID <= (@StartID + @RowCount - 1)
BEGIN
    -- Insert two images with naming [ProductID]_[OrderNumber].jpg
    INSERT INTO ProductImages (ProductID, ImagePath, IsMainImage)
    VALUES 
        (@CurrentID, CAST(@CurrentID AS NVARCHAR) + '_1.jpg', 1), -- First image, main
        (@CurrentID, CAST(@CurrentID AS NVARCHAR) + '_2.jpg', 0); -- Second image

    -- Move to the next ProductID
    SET @CurrentID = @CurrentID + 1;
END;

-- Dữ liệu cho bảng Users
INSERT INTO Users (UserName, FullName, Email, Password, Phone, Role)
VALUES 
(N'admin', N'Lê Anh Tuấn', N'leanhtuank16@siu.edu.vn', N'4297f44b13955235245b2497399d7a93', N'0123456789', N'Admin'),
(N'tranvanminh', N'Trần Văn Minh', N'tranvanminhk16@siu.edu.vn', N'4297f44b13955235245b2497399d7a93', N'0912345678', N'Customer'),
(N'kho', N'Lê Minh C', N'socola200500@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345679', N'Admin'),
(N'tran_thi_d', N'Trần Thị D', N'tran.d@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345680', N'Customer'),
(N'hoang_van_e', N'Hoàng Văn E', N'hoang.e@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345681', N'Customer'),
(N'nguyen_thi_f', N'Nguyễn Thị F', N'nguyen.f@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345682', N'Customer'),
(N'pham_minh_g', N'Phạm Minh G', N'pham.g@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345683', N'Customer'),
(N'le_thi_h', N'Lê Thị H', N'le.h@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345684', N'Customer'),
(N'ho_nguyen_i', N'Hồ Nguyễn I', N'ho.i@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345685', N'Customer'),
(N'trinh_vu_j', N'Trịnh Vũ J', N'trinh.j@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345686', N'Customer'),
(N'hoang_thi_k', N'Hoàng Thị K', N'hoang.k@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345687', N'Customer'),
(N'nguyen_vu_l', N'Nguyễn Vũ L', N'nguyen.l@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345688', N'Customer'),
(N'tran_thi_m', N'Trần Thị M', N'tran.m@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345689', N'Customer'),
(N'le_minh_n', N'Lê Minh N', N'le.minh.n@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345690', N'Customer'),
(N'pham_nguyen_o', N'Phạm Nguyễn O', N'pham.o@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345691', N'Customer'),
(N'nguyen_quang_p', N'Nguyễn Quang P', N'nguyen.p@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345692', N'Customer'),
(N'le_thi_q', N'Lê Thị Q', N'le.q@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345693', N'Customer'),
(N'tran_thi_r', N'Trần Thị R', N'tran.r@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345694', N'Customer'),
(N'ho_nguyen_s', N'Hồ Nguyễn S', N'ho.s@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345695', N'Customer');

UPDATE Users SET Address = N'123 Đường ABC, Phường 1, Quận 1, TP.HCM' WHERE UserName = N'admin';
UPDATE Users SET Address = N'456 Đường XYZ, Phường 2, Quận 2, TP.HCM' WHERE UserName = N'tranvanminh';
UPDATE Users SET Address = N'789 Đường DEF, Phường 3, Quận 3, TP.HCM' WHERE UserName = N'kho';
UPDATE Users SET Address = N'111 Đường GHI, Phường 4, Quận 4, TP.HCM' WHERE UserName = N'tran_thi_d';
UPDATE Users SET Address = N'222 Đường JKL, Phường 5, Quận 5, TP.HCM' WHERE UserName = N'hoang_van_e';
UPDATE Users SET Address = N'333 Đường MNO, Phường 6, Quận 6, TP.HCM' WHERE UserName = N'nguyen_thi_f';
UPDATE Users SET Address = N'444 Đường PQR, Phường 7, Quận 7, TP.HCM' WHERE UserName = N'pham_minh_g';
UPDATE Users SET Address = N'555 Đường STU, Phường 8, Quận 8, TP.HCM' WHERE UserName = N'le_thi_h';
UPDATE Users SET Address = N'666 Đường VWX, Phường 9, Quận 9, TP.HCM' WHERE UserName = N'ho_nguyen_i';
UPDATE Users SET Address = N'777 Đường YZA, Phường 10, Quận 10, TP.HCM' WHERE UserName = N'trinh_vu_j';
UPDATE Users SET Address = N'888 Đường BCD, Phường 11, Quận 11, TP.HCM' WHERE UserName = N'hoang_thi_k';
UPDATE Users SET Address = N'999 Đường EFG, Phường 12, Quận 12, TP.HCM' WHERE UserName = N'nguyen_vu_l';
UPDATE Users SET Address = N'101 Đường HIJ, Phường 13, Quận 1, TP.HCM' WHERE UserName = N'tran_thi_m';
UPDATE Users SET Address = N'202 Đường KLM, Phường 14, Quận 2, TP.HCM' WHERE UserName = N'le_minh_n';
UPDATE Users SET Address = N'303 Đường NOP, Phường 15, Quận 3, TP.HCM' WHERE UserName = N'pham_nguyen_o';
UPDATE Users SET Address = N'404 Đường QRS, Phường 16, Quận 4, TP.HCM' WHERE UserName = N'nguyen_quang_p';
UPDATE Users SET Address = N'505 Đường TUV, Phường 17, Quận 5, TP.HCM' WHERE UserName = N'le_thi_q';
UPDATE Users SET Address = N'606 Đường WXY, Phường 18, Quận 6, TP.HCM' WHERE UserName = N'tran_thi_r';
UPDATE Users SET Address = N'707 Đường ZAB, Phường 19, Quận 7, TP.HCM' WHERE UserName = N'ho_nguyen_s';

-- Dữ liệu cho bảng PaymentMethods
INSERT INTO PaymentMethods (MethodName)
VALUES 
(N'Trả tiền mặt'),
(N'Trả qua thẻ tín dụng'),
(N'Trả qua chuyển khoản'),
(N'Trả qua ví điện tử'),
(N'Trả qua thẻ ngân hàng'),
(N'Trả qua Momo'),
(N'Trả qua ZaloPay'),
(N'Trả qua VNPay'),
(N'Trả qua PayPal'),
(N'Trả qua thanh toán COD');

-- Dữ liệu cho bảng Orders
INSERT INTO Orders (UserID, TotalAmount, PaymentMethodID, OrderStatus, ShippingAddress)
VALUES 
(1, 5000000, 1, 'Pending', N'123 Đường ABC, Quận 1, TP.HCM'),
(2, 350000, 2, 'Confirmed', N'456 Đường XYZ, Quận 2, TP.HCM'),
(3, 1200000, 3, 'Cancelled', N'789 Đường DEF, Quận 3, TP.HCM'),
(4, 1500000, 4, 'Pending', N'101 Đường GHI, Quận 4, TP.HCM'),
(5, 700000, 5, 'Confirmed', N'202 Đường JKL, Quận 5, TP.HCM'),
(6, 950000, 6, 'Pending', N'303 Đường MNO, Quận 6, TP.HCM'),
(7, 800000, 7, 'Confirmed', N'404 Đường PQR, Quận 7, TP.HCM'),
(8, 2200000, 8, 'Pending', N'505 Đường STU, Quận 8, TP.HCM'),
(9, 50000, 9, 'Cancelled', N'606 Đường VWX, Quận 9, TP.HCM'),
(10, 600000, 10, 'Pending', N'707 Đường YZA, Quận 10, TP.HCM'),
(11, 1300000, 1, 'Confirmed', N'808 Đường BCD, Quận 11, TP.HCM'),
(12, 800000, 2, 'Pending', N'909 Đường EFG, Quận 12, TP.HCM'),
(13, 450000, 3, 'Cancelled', N'111 Đường HIJ, Quận 1, TP.HCM'),
(14, 320000, 4, 'Confirmed', N'222 Đường KLM, Quận 2, TP.HCM'),
(15, 700000, 5, 'Pending', N'333 Đường NOP, Quận 3, TP.HCM'),
(16, 200000, 6, 'Confirmed', N'444 Đường QRS, Quận 4, TP.HCM'),
(17, 900000, 7, 'Pending', N'555 Đường TUV, Quận 5, TP.HCM'),
(18, 400000, 8, 'Cancelled', N'666 Đường WXY, Quận 6, TP.HCM'),
(19, 1100000, 9, 'Pending', N'777 Đường ZAB, Quận 7, TP.HCM');

-- Dữ liệu cho bảng OrderDetails
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(1, 1, 2, 20000000),
(1, 2, 1, 15000000),
(2, 5, 5, 50000),
(2, 3, 2, 800000),
(3, 4, 1, 1200000),
(4, 6, 10, 150000),
(5, 7, 3, 350000),
(6, 8, 1, 70000),
(7, 9, 2, 15000),
(8, 10, 1, 95000),
(9, 11, 1, 25000),
(10, 12, 1, 12000000),
(11, 13, 1, 500000),
(12, 14, 2, 150000),
(13, 15, 3, 50000),
(14, 16, 4, 30000),
(15, 17, 2, 35000),
(16, 18, 1, 60000),
(17, 19, 1, 45000),
(18, 20, 3, 30000);

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20250603153716_InitialCreate', '9.0.5');

DECLARE @pid INT = 1;
DECLARE @i INT;
DECLARE @qty INT;
DECLARE @import_dt DATE;

WHILE @pid <= 30
BEGIN
    SET @i = 1;
    WHILE @i <= 2
    BEGIN
        SET @qty = FLOOR(RAND(CHECKSUM(NEWID())) * 200 + 50); -- 50 đến 249
        SET @import_dt = DATEADD(DAY, FLOOR(RAND(CHECKSUM(NEWID())) * 180), CAST(GETDATE() AS DATE)); -- trong 180 ngày tới

        INSERT INTO ProductStocks (ProductID, Quantity, ExpirationDate)
        VALUES (@pid, @qty, @import_dt);

        SET @i = @i + 1;
    END
    SET @pid = @pid + 1;
END
