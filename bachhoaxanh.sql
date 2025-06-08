USE BachHoaXanh
GO


DROP TABLE IF EXISTS PromotionDetails;
DROP TABLE IF EXISTS Promotions;
DROP TABLE IF EXISTS OrderStockDetails;
DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS ProductImages;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS SubCategories;
DROP TABLE IF EXISTS FavoriteProducts;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Reviews;
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

-- Bảng Reviews: Lưu thông tin đánh giá
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL, -- Liên kết với bảng Users đã có
    ProductId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5), -- Số sao từ 1 đến 5
    Comment NVARCHAR(MAX),
    ReviewDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
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
    CanCancel BIT, -- Cho phép hủy đơn hàng
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

CREATE TABLE OrderStockDetails (
    OrderStockDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderDetailID INT NOT NULL,
    StockID INT NOT NULL,
    Quantity INT DEFAULT 0 NOT NULL,
    FOREIGN KEY (OrderDetailID) REFERENCES OrderDetails(OrderDetailID) ON DELETE CASCADE,
    FOREIGN KEY (StockID) REFERENCES ProductStocks(StockID) ON DELETE NO ACTION
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
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);


CREATE TABLE __EFMigrationsHistory (
    MigrationId nvarchar(150) NOT NULL,
    ProductVersion nvarchar(32) NOT NULL,
    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
);

CREATE TABLE Promotions (
    PromotionID INT PRIMARY KEY IDENTITY(1,1),
    PromotionName NVARCHAR(250) NOT NULL,
    StartDate DATETIME NOT NULL, 
    EndDate DATETIME NOT NULL,
    ImagePath NVARCHAR(255) NOT NULL,
    ShowOnTop BIT NOT NULL DEFAULT 0 
);

CREATE TABLE PromotionDetails (
    PromotionDetailID INT PRIMARY KEY IDENTITY(1,1),
    PromotionID INT,
    ProductID INT UNIQUE,
    NewPrice DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (PromotionID) REFERENCES Promotions(PromotionID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
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
(N'ho_nguyen_s', N'Hồ Nguyễn S', N'ho.s@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345695', N'Customer'),
(N'Lam', N'Nguyễn Ngọc Thanh Lâm', N'nguyenngocthanhlamk16@siu.edu.vn', N'4297f44b13955235245b2497399d7a93', N'0123456789', N'Admin');


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


-- Rau muống (ProductId = 1)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(1, 1, 5, N'Rau muống tươi, lá xanh mướt, nấu canh ngon.', '2025-05-10 08:00:00'),
(2, 1, 4, N'Rau khá tươi, nhưng có vài lá hơi héo.', '2025-05-12 14:30:00'),
(3, 1, 3, N'Rau tạm được, giá hơi cao so với chợ.', '2025-05-15 10:15:00'),
(4, 1, 5, N'Rau muống sạch, đóng gói cẩn thận.', '2025-05-18 16:20:00'),
(5, 1, 4, N'Tươi ngon, giao hàng nhanh.', '2025-05-20 09:45:00'),
(6, 1, 2, N'Rau hơi dập, cần cải thiện đóng gói.', '2025-05-22 11:00:00'),
(7, 1, 5, N'Rất hài lòng, rau tươi như mới hái.', '2025-05-25 13:10:00'),
(8, 1, 4, N'Rau muống chất lượng, nhưng hơi ít.', '2025-05-27 15:30:00'),
(9, 1, 3, N'Bình thường, không có gì đặc biệt.', '2025-05-30 17:00:00'),
(10, 1, 5, N'Rau muống rất ngon, sẽ mua lại.', '2025-06-02 12:20:00');

-- Cải thìa (ProductId = 2)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(1, 2, 4, N'Cải thìa tươi, nấu lẩu rất hợp.', '2025-05-11 09:00:00'),
(2, 2, 5, N'Rau sạch, không có sâu, rất thích.', '2025-05-13 12:45:00'),
(3, 2, 3, N'Rau ổn, nhưng giá hơi cao.', '2025-05-16 14:20:00'),
(4, 2, 4, N'Cải thìa tươi ngon, giao hàng đúng giờ.', '2025-05-19 10:30:00'),
(5, 2, 5, N'Rất tươi, đóng gói kỹ càng.', '2025-05-21 16:15:00'),
(6, 2, 2, N'Rau hơi héo, cần kiểm tra kỹ hơn.', '2025-05-23 11:50:00'),
(7, 2, 4, N'Rau sạch, nấu canh ngon.', '2025-05-26 13:40:00'),
(8, 2, 5, N'Hài lòng, rau tươi và giòn.', '2025-05-28 15:10:00'),
(9, 2, 3, N'Rau tạm được, nhưng có vài lá úa.', '2025-05-31 17:20:00'),
(10, 2, 4, N'Cải thìa ngon, sẽ tiếp tục ủng hộ.', '2025-06-03 09:30:00');

-- Khoai tây (ProductId = 3)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(1, 3, 5, N'Khoai tây to, tươi, chiên ngon.', '2025-05-10 10:00:00'),
(2, 3, 4, N'Khoai chất lượng, nhưng hơi lẫn đất.', '2025-05-12 15:30:00'),
(3, 3, 3, N'Khoai bình thường, giá hơi cao.', '2025-05-15 11:45:00'),
(4, 3, 5, N'Rất ngon, làm khoai tây chiên tuyệt.', '2025-05-18 17:20:00'),
(5, 3, 4, N'Khoai tươi, giao hàng nhanh.', '2025-05-20 12:00:00'),
(6, 3, 2, N'Có vài củ bị hỏng, cần kiểm tra kỹ.', '2025-05-22 14:10:00'),
(7, 3, 5, N'Khoai tây chất lượng, rất hài lòng.', '2025-05-25 16:30:00'),
(8, 3, 4, N'Khoai ngon, nhưng túi hơi ít.', '2025-05-27 10:50:00'),
(9, 3, 3, N'Không đặc biệt, giá ổn.', '2025-05-30 13:20:00'),
(10, 3, 5, N'Rất thích, khoai to và tươi.', '2025-06-02 15:40:00'),
(11, 3, 4, N'Khoai tây tốt, nấu súp rất ngon.', '2025-06-04 11:00:00');

-- Cà rốt (ProductId = 4)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 4, 5, N'Cà rốt ngọt, tươi, rất đáng tiền.', '2025-05-11 08:30:00'),
(3, 4, 4, N'Cà rốt tươi, nhưng có củ nhỏ.', '2025-05-13 14:00:00'),
(4, 4, 3, N'Chất lượng ổn, nhưng giá hơi cao.', '2025-05-16 16:10:00'),
(5, 4, 5, N'Cà rốt giòn, làm salad ngon.', '2025-05-19 12:20:00'),
(6, 4, 4, N'Tươi ngon, giao hàng nhanh.', '2025-05-21 17:30:00'),
(7, 4, 2, N'Có củ bị dập, cần cải thiện.', '2025-05-23 13:00:00'),
(8, 4, 5, N'Rất hài lòng, cà rốt ngọt.', '2025-05-26 15:10:00'),
(9, 4, 4, N'Cà rốt tươi, nhưng hơi ít.', '2025-05-28 11:20:00'),
(10, 4, 3, N'Bình thường, không quá đặc biệt.', '2025-05-31 14:30:00'),
(1, 4, 5, N'Cà rốt rất ngon, sẽ mua lại.', '2025-06-03 16:40:00');

-- Nấm bào ngư (ProductId = 5)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 5, 4, N'Nấm tươi, xào rất ngon.', '2025-05-10 09:00:00'),
(3, 5, 5, N'Nấm bào ngư sạch, chất lượng tốt.', '2025-05-12 13:30:00'),
(4, 5, 3, N'Nấm bình thường, giá hơi cao.', '2025-05-15 15:00:00'),
(5, 5, 5, N'Rất tươi, làm món chay tuyệt vời.', '2025-05-18 17:10:00'),
(6, 5, 4, N'Nấm ngon, giao hàng nhanh.', '2025-05-20 11:20:00'),
(7, 5, 2, N'Nấm hơi héo, cần kiểm tra kỹ.', '2025-05-22 14:50:00'),
(8, 5, 5, N'Nấm bào ngư tươi, rất hài lòng.', '2025-05-25 16:00:00'),
(9, 5, 4, N'Nấm ngon, nhưng gói hơi nhỏ.', '2025-05-27 12:30:00'),
(10, 5, 3, N'Chất lượng ổn, không đặc biệt.', '2025-05-30 15:10:00'),
(11, 5, 5, N'Nấm tươi ngon, sẽ mua lại.', '2025-06-02 17:20:00');

-- Táo Fuji (ProductId = 6)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 6, 5, N'Táo Fuji ngọt, giòn, rất ngon.', '2025-05-11 10:00:00'),
(13, 6, 4, N'Táo tươi, nhưng có quả hơi nhỏ.', '2025-05-13 15:30:00'),
(14, 6, 3, N'Táo bình thường, giá hơi cao.', '2025-05-16 11:20:00'),
(15, 6, 5, N'Rất ngon, táo giòn và ngọt.', '2025-05-19 13:40:00'),
(16, 6, 4, N'Táo Fuji chất lượng, giao nhanh.', '2025-05-21 16:50:00'),
(17, 6, 2, N'Có quả bị dập, cần cải thiện.', '2025-05-23 12:10:00'),
(18, 6, 5, N'Táo rất tươi, rất hài lòng.', '2025-05-26 14:20:00'),
(19, 6, 4, N'Táo ngon, nhưng túi hơi ít.', '2025-05-28 16:30:00'),
(2, 6, 3, N'Bình thường, không quá đặc biệt.', '2025-05-31 10:40:00'),
(1, 6, 5, N'Táo Fuji rất ngon, sẽ mua lại.', '2025-06-03 12:50:00');

-- Lê Hàn Quốc (ProductId = 7)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 7, 4, N'Lê ngọt, nhưng có quả hơi mềm.', '2025-05-10 11:00:00'),
(3, 7, 5, N'Lê Hàn Quốc giòn, rất ngon.', '2025-05-12 14:40:00'),
(4, 7, 3, N'Lê ổn, nhưng giá hơi cao.', '2025-05-15 16:50:00'),
(5, 7, 5, N'Rất tươi, lê ngọt và mọng nước.', '2025-05-18 12:20:00'),
(6, 7, 4, N'Lê ngon, giao hàng đúng giờ.', '2025-05-20 14:30:00'),
(7, 7, 2, N'Có quả bị dập, cần kiểm tra kỹ.', '2025-05-22 16:40:00'),
(8, 7, 5, N'Lê rất ngon, rất hài lòng.', '2025-05-25 10:50:00'),
(9, 7, 4, N'Lê ngọt, nhưng túi hơi ít.', '2025-05-27 13:00:00'),
(10, 7, 3, N'Bình thường, không đặc biệt.', '2025-05-30 15:20:00'),
(11, 7, 5, N'Lê Hàn Quốc rất ngon, sẽ mua lại.', '2025-06-02 17:30:00');

-- Xoài cát (ProductId = 8)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 8, 5, N'Xoài cát ngọt, rất thơm.', '2025-05-11 09:10:00'),
(13, 8, 4, N'Xoài ngon, nhưng có quả hơi chua.', '2025-05-13 11:20:00'),
(14, 8, 3, N'Xoài bình thường, giá hơi cao.', '2025-05-16 13:30:00'),
(15, 8, 5, N'Xoài cát ngọt, rất đáng tiền.', '2025-05-19 15:40:00'),
(16, 8, 4, N'Xoài tươi, giao hàng nhanh.', '2025-05-21 17:50:00'),
(17, 8, 2, N'Có quả bị dập, cần cải thiện.', '2025-05-23 10:00:00'),
(18, 8, 5, N'Xoài rất ngon, rất hài lòng.', '2025-05-26 12:10:00'),
(19, 8, 4, N'Xoài ngọt, nhưng túi hơi ít.', '2025-05-28 14:20:00'),
(2, 8, 3, N'Bình thường, không đặc biệt.', '2025-05-31 16:30:00'),
(1, 8, 5, N'Xoài cát rất ngon, sẽ mua lại.', '2025-06-03 10:40:00');

-- Dứa (ProductId = 9)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 9, 4, N'Dứa ngọt, nhưng hơi nhỏ.', '2025-05-10 12:00:00'),
(3, 9, 5, N'Dứa tươi, rất thơm và ngon.', '2025-05-12 15:30:00'),
(4, 9, 3, N'Dứa ổn, nhưng giá hơi cao.', '2025-05-15 17:40:00'),
(5, 9, 5, N'Dứa ngọt, làm sinh tố rất ngon.', '2025-05-18 13:50:00'),
(6, 9, 4, N'Dứa tươi, giao hàng nhanh.', '2025-05-20 16:00:00'),
(7, 9, 2, N'Dứa hơi chua, cần chọn kỹ hơn.', '2025-05-22 10:10:00'),
(8, 9, 5, N'Dứa rất ngon, rất hài lòng.', '2025-05-25 12:20:00'),
(9, 9, 4, N'Dứa ngọt, nhưng quả hơi nhỏ.', '2025-05-27 14:30:00'),
(10, 9, 3, N'Bình thường, không đặc biệt.', '2025-05-30 16:40:00'),
(11, 9, 5, N'Dứa rất ngon, sẽ mua lại.', '2025-06-02 10:50:00');

-- Cam sành (ProductId = 10)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 10, 5, N'Cam sành ngọt, nhiều nước.', '2025-05-11 10:20:00'),
(13, 10, 4, N'Cam tươi, nhưng có quả hơi chua.', '2025-05-13 12:30:00'),
(14, 10, 3, N'Cam bình thường, giá hơi cao.', '2025-05-16 14:40:00'),
(15, 10, 5, N'Cam rất ngon, làm nước ép tuyệt.', '2025-05-19 16:50:00'),
(16, 10, 4, N'Cam tươi, giao hàng nhanh.', '2025-05-21 10:00:00'),
(17, 10, 2, N'Có quả bị hỏng, cần kiểm tra kỹ.', '2025-05-23 12:10:00'),
(18, 10, 5, N'Cam sành rất ngon, rất hài lòng.', '2025-05-26 14:20:00'),
(19, 10, 4, N'Cam ngọt, nhưng túi hơi ít.', '2025-05-28 16:30:00'),
(1, 10, 3, N'Bình thường, không đặc biệt.', '2025-05-31 10:40:00'),
(1, 10, 5, N'Cam sành rất ngon, sẽ mua lại.', '2025-06-03 12:50:00');

-- Mì Hảo Hảo (ProductId = 11)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 11, 5, N'Mì Hảo Hảo ngon, đúng vị.', '2025-05-10 09:30:00'),
(3, 11, 4, N'Mì ngon, nhưng gói hơi nhỏ.', '2025-05-12 11:40:00'),
(4, 11, 3, N'Mì ổn, nhưng giá hơi cao.', '2025-05-15 13:50:00'),
(5, 11, 5, N'Rất ngon, mì thơm và đậm đà.', '2025-05-18 15:00:00'),
(6, 11, 4, N'Mì Hảo Hảo chất lượng, giao nhanh.', '2025-05-20 17:10:00'),
(7, 11, 2, N'Mì bình thường, không đặc biệt.', '2025-05-22 10:20:00'),
(8, 11, 5, N'Mì rất ngon, rất hài lòng.', '2025-05-25 12:30:00'),
(9, 11, 4, N'Mì ngon, nhưng hơi cay.', '2025-05-27 14:40:00'),
(10, 11, 3, N'Bình thường, giá ổn.', '2025-05-30 16:50:00'),
(11, 11, 5, N'Mì Hảo Hảo tuyệt vời, sẽ mua lại.', '2025-06-02 11:00:00');

-- Gạo ST25 (ProductId = 12)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 12, 5, N'Gạo ST25 thơm, dẻo, rất ngon.', '2025-05-11 11:30:00'),
(13, 12, 4, N'Gạo chất lượng, nhưng túi hơi nặng.', '2025-05-13 13:40:00'),
(14, 12, 3, N'Gạo ổn, nhưng giá hơi cao.', '2025-05-16 15:50:00'),
(15, 12, 5, N'Gạo ST25 nấu cơm rất ngon.', '2025-05-19 17:00:00'),
(16, 12, 4, N'Gạo thơm, giao hàng nhanh.', '2025-05-21 10:10:00'),
(17, 12, 2, N'Gạo hơi lẫn tạp chất, cần kiểm tra.', '2025-05-23 12:20:00'),
(18, 12, 5, N'Gạo rất ngon, rất hài lòng.', '2025-05-26 14:30:00'),
(19, 12, 4, N'Gạo thơm, nhưng túi hơi ít.', '2025-05-28 16:40:00'),
(6, 12, 3, N'Bình thường, không đặc biệt.', '2025-05-31 10:50:00'),
(1, 12, 5, N'Gạo ST25 rất ngon, sẽ mua lại.', '2025-06-03 13:00:00');

-- Cá ngừ đóng hộp (ProductId = 13)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 13, 4, N'Cá ngừ ngon, nhưng hơi mặn.', '2025-05-10 10:40:00'),
(3, 13, 5, N'Cá ngừ chất lượng, rất tiện lợi.', '2025-05-12 12:50:00'),
(4, 13, 3, N'Cá ổn, nhưng giá hơi cao.', '2025-05-15 14:00:00'),
(5, 13, 5, N'Rất ngon, làm salad tuyệt vời.', '2025-05-18 16:10:00'),
(6, 13, 4, N'Cá ngừ chất lượng, giao nhanh.', '2025-05-20 18:20:00'),
(7, 13, 2, N'Cá hơi khô, cần cải thiện.', '2025-05-22 10:30:00'),
(8, 13, 5, N'Cá ngừ rất ngon, rất hài lòng.', '2025-05-25 12:40:00'),
(9, 13, 4, N'Cá ngon, nhưng hộp hơi nhỏ.', '2025-05-27 14:50:00'),
(10, 13, 3, N'Bình thường, không đặc biệt.', '2025-05-30 17:00:00'),
(11, 13, 5, N'Cá ngừ rất ngon, sẽ mua lại.', '2025-06-02 11:10:00');

-- Thịt ba chỉ heo (ProductId = 14)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 14, 5, N'Thịt ba chỉ tươi, rất ngon.', '2025-05-11 12:40:00'),
(13, 14, 4, N'Thịt chất lượng, nhưng hơi nhiều mỡ.', '2025-05-13 14:50:00'),
(14, 14, 3, N'Thịt ổn, nhưng giá hơi cao.', '2025-05-16 16:00:00'),
(15, 14, 5, N'Thịt tươi, làm thịt kho tàu ngon.', '2025-05-19 18:10:00'),
(16, 14, 4, N'Thịt ba chỉ tươi, giao nhanh.', '2025-05-21 10:20:00'),
(17, 14, 2, N'Thịt hơi hôi, cần kiểm tra kỹ.', '2025-05-23 12:30:00'),
(18, 14, 5, N'Thịt rất ngon, rất hài lòng.', '2025-05-26 14:40:00'),
(19, 14, 4, N'Thịt tươi, nhưng hơi ít.', '2025-05-28 16:50:00'),
(2, 14, 3, N'Bình thường, không đặc biệt.', '2025-05-31 11:00:00'),
(1, 14, 5, N'Thịt ba chỉ rất ngon, sẽ mua lại.', '2025-06-03 13:10:00');

-- Đùi gà tươi (ProductId = 15)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 15, 4, N'Đùi gà tươi, nhưng hơi nhỏ.', '2025-05-10 11:50:00'),
(3, 15, 5, N'Đùi gà chất lượng, rất ngon.', '2025-05-12 13:00:00'),
(4, 15, 3, N'Gà ổn, nhưng giá hơi cao.', '2025-05-15 15:10:00'),
(5, 15, 5, N'Đùi gà tươi, chiên giòn ngon.', '2025-05-18 17:20:00'),
(6, 15, 4, N'Gà tươi, giao hàng nhanh.', '2025-05-20 19:30:00'),
(7, 15, 2, N'Gà hơi dai, cần cải thiện.', '2025-05-22 11:40:00'),
(8, 15, 5, N'Đùi gà rất ngon, rất hài lòng.', '2025-05-25 13:50:00'),
(9, 15, 4, N'Gà ngon, nhưng hơi ít.', '2025-05-27 15:00:00'),
(10, 15, 3, N'Bình thường, không đặc biệt.', '2025-05-30 17:10:00'),
(11, 15, 5, N'Đùi gà rất ngon, sẽ mua lại.', '2025-06-02 11:20:00');

-- Tôm sú tươi (ProductId = 16)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 16, 5, N'Tôm sú tươi, rất ngon.', '2025-05-11 13:50:00'),
(13, 16, 4, N'Tôm tươi, nhưng có con hơi nhỏ.', '2025-05-13 15:00:00'),
(14, 16, 3, N'Tôm ổn, nhưng giá hơi cao.', '2025-05-16 17:10:00'),
(15, 16, 5, N'Tôm sú tươi, làm lẩu ngon.', '2025-05-19 19:20:00'),
(16, 16, 4, N'Tôm tươi, giao hàng nhanh.', '2025-05-21 11:30:00'),
(17, 16, 2, N'Có con bị ươn, cần kiểm tra kỹ.', '2025-05-23 13:40:00'),
(18, 16, 5, N'Tôm rất ngon, rất hài lòng.', '2025-05-26 15:50:00'),
(19, 16, 4, N'Tôm tươi, nhưng hơi ít.', '2025-05-28 17:00:00'),
(2, 16, 3, N'Bình thường, không đặc biệt.', '2025-05-31 11:10:00'),
(1, 16, 5, N'Tôm sú rất ngon, sẽ mua lại.', '2025-06-03 13:20:00');

-- Thịt bò đông lạnh (ProductId = 17)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 17, 4, N'Thịt bò chất lượng, nhưng hơi cứng.', '2025-05-10 12:00:00'),
(3, 17, 5, N'Thịt bò ngon, làm bít tết tuyệt.', '2025-05-12 14:10:00'),
(4, 17, 3, N'Thịt ổn, nhưng giá hơi cao.', '2025-05-15 16:20:00'),
(5, 17, 5, N'Thịt bò chất lượng, rất hài lòng.', '2025-05-18 18:30:00'),
(6, 17, 4, N'Thịt tươi, giao hàng nhanh.', '2025-05-20 10:40:00'),
(7, 17, 2, N'Thịt hơi dai, cần cải thiện.', '2025-05-22 12:50:00'),
(8, 17, 5, N'Thịt bò rất ngon, sẽ mua lại.', '2025-05-25 14:00:00'),
(9, 17, 4, N'Thịt ngon, nhưng hơi ít.', '2025-05-27 16:10:00'),
(10, 17, 3, N'Bình thường, không đặc biệt.', '2025-05-30 18:20:00'),
(11, 17, 5, N'Thịt bò rất ngon, đáng tiền.', '2025-06-02 12:30:00');

-- Mực ống đông lạnh (ProductId = 18)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 18, 5, N'Mực ống tươi, rất ngon.', '2025-05-11 14:00:00'),
(13, 18, 4, N'Mực chất lượng, nhưng hơi nhỏ.', '2025-05-13 16:10:00'),
(14, 18, 3, N'Mực ổn, nhưng giá hơi cao.', '2025-05-16 18:20:00'),
(15, 18, 5, N'Mực ống ngon, làm món nướng tuyệt.', '2025-05-19 10:30:00'),
(16, 18, 4, N'Mực tươi, giao hàng nhanh.', '2025-05-21 12:40:00'),
(17, 18, 2, N'Mực hơi dai, cần kiểm tra kỹ.', '2025-05-23 14:50:00'),
(18, 18, 5, N'Mực rất ngon, rất hài lòng.', '2025-05-26 16:00:00'),
(19, 18, 4, N'Mực ngon, nhưng hơi ít.', '2025-05-28 18:10:00'),
(2, 18, 3, N'Bình thường, không đặc biệt.', '2025-05-31 12:20:00'),
(1, 18, 5, N'Mực ống rất ngon, sẽ mua lại.', '2025-06-03 14:30:00');

-- Nước ngọt Pepsi (ProductId = 19)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 19, 4, N'Pepsi ngon, nhưng hơi ngọt.', '2025-05-10 13:10:00'),
(3, 19, 5, N'Nước ngọt Pepsi rất ngon, đúng vị.', '2025-05-12 15:20:00'),
(4, 19, 3, N'Nước ổn, nhưng giá hơi cao.', '2025-05-15 17:30:00'),
(5, 19, 5, N'Pepsi mát lạnh, rất thích.', '2025-05-18 19:40:00'),
(6, 19, 4, N'Nước ngọt chất lượng, giao nhanh.', '2025-05-20 11:50:00'),
(7, 19, 2, N'Hơi nhạt, cần cải thiện.', '2025-05-22 13:00:00'),
(8, 19, 5, N'Pepsi rất ngon, rất hài lòng.', '2025-05-25 15:10:00'),
(9, 19, 4, N'Nước ngon, nhưng chai hơi nhỏ.', '2025-05-27 17:20:00'),
(10, 19, 3, N'Bình thường, không đặc biệt.', '2025-05-30 19:30:00'),
(11, 19, 5, N'Pepsi rất ngon, sẽ mua lại.', '2025-06-02 13:40:00');

-- Nước ép cam (ProductId = 20)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 20, 5, N'Nước ép cam thơm, rất ngon.', '2025-05-11 15:20:00'),
(13, 20, 4, N'Nước ép tươi, nhưng hơi chua.', '2025-05-13 17:30:00'),
(14, 20, 3, N'Nước ổn, nhưng giá hơi cao.', '2025-05-16 19:40:00'),
(15, 20, 5, N'Nước ép cam rất ngon, đáng tiền.', '2025-05-19 11:50:00'),
(16, 20, 4, N'Nước ép chất lượng, giao nhanh.', '2025-05-21 13:00:00'),
(17, 20, 2, N'Hơi nhạt, cần cải thiện.', '2025-05-23 15:10:00'),
(18, 20, 5, N'Nước ép rất ngon, rất hài lòng.', '2025-05-26 17:20:00'),
(19, 20, 4, N'Nước ép ngon, nhưng chai hơi nhỏ.', '2025-05-28 19:30:00'),
(2, 20, 3, N'Bình thường, không đặc biệt.', '2025-05-31 13:40:00'),
(1, 20, 5, N'Nước ép cam rất ngon, sẽ mua lại.', '2025-06-03 15:50:00');

-- Nước suối Lavie (ProductId = 21)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 21, 4, N'Nước suối sạch, nhưng chai nhỏ.', '2025-05-10 14:30:00'),
(3, 21, 5, N'Nước suối Lavie chất lượng, rất tốt.', '2025-05-12 16:40:00'),
(4, 21, 3, N'Nước ổn, nhưng giá hơi cao.', '2025-05-15 18:50:00'),
(5, 21, 5, N'Nước suối sạch, rất hài lòng.', '2025-05-18 10:00:00'),
(6, 21, 4, N'Nước suối chất lượng, giao nhanh.', '2025-05-20 12:10:00'),
(7, 21, 2, N'Chai hơi móp, cần cải thiện.', '2025-05-22 14:20:00'),
(8, 21, 5, N'Nước suối rất tốt, rất hài lòng.', '2025-05-25 16:30:00'),
(9, 21, 4, N'Nước sạch, nhưng chai hơi nhỏ.', '2025-05-27 18:40:00'),
(10, 21, 3, N'Bình thường, không đặc biệt.', '2025-05-30 10:50:00'),
(11, 21, 5, N'Nước suối Lavie rất tốt, sẽ mua lại.', '2025-06-02 13:00:00');

-- Nước mắm Nam Ngư (ProductId = 22)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 22, 5, N'Nước mắm Nam Ngư thơm, đậm đà.', '2025-05-11 16:40:00'),
(13, 22, 4, N'Nước mắm ngon, nhưng hơi mặn.', '2025-05-13 18:50:00'),
(14, 22, 3, N'Nước mắm ổn, nhưng giá hơi cao.', '2025-05-16 10:00:00'),
(15, 22, 5, N'Nước mắm rất ngon, chấm gì cũng hợp.', '2025-05-19 12:10:00'),
(16, 22, 4, N'Nước mắm chất lượng, giao nhanh.', '2025-05-21 14:20:00'),
(17, 22, 2, N'Hơi nhạt, cần cải thiện.', '2025-05-23 16:30:00'),
(18, 22, 5, N'Nước mắm rất ngon, rất hài lòng.', '2025-05-26 18:40:00'),
(19, 22, 4, N'Nước mắm thơm, nhưng chai nhỏ.', '2025-05-28 10:50:00'),
(2, 22, 3, N'Bình thường, không đặc biệt.', '2025-05-31 13:00:00'),
(1, 22, 5, N'Nước mắm Nam Ngư rất ngon, sẽ mua lại.', '2025-06-03 15:10:00');

-- Tiêu đen (ProductId = 23)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 23, 4, N'Tiêu đen thơm, nhưng gói nhỏ.', '2025-05-10 15:50:00'),
(3, 23, 5, N'Tiêu đen chất lượng, rất thơm.', '2025-05-12 17:00:00'),
(4, 23, 3, N'Tiêu ổn, nhưng giá hơi cao.', '2025-05-15 19:10:00'),
(5, 23, 5, N'Tiêu đen rất ngon, đáng tiền.', '2025-05-18 11:20:00'),
(6, 23, 4, N'Tiêu chất lượng, giao nhanh.', '2025-05-20 13:30:00'),
(7, 23, 2, N'Tiêu hơi nhạt, cần cải thiện.', '2025-05-22 15:40:00'),
(8, 23, 5, N'Tiêu đen rất thơm, rất hài lòng.', '2025-05-25 17:50:00'),
(9, 23, 4, N'Tiêu ngon, nhưng gói hơi ít.', '2025-05-27 19:00:00'),
(10, 23, 3, N'Bình thường, không đặc biệt.', '2025-05-30 11:10:00'),
(11, 23, 5, N'Tiêu đen rất ngon, sẽ mua lại.', '2025-06-02 14:20:00');

-- Bánh Oreo (ProductId = 24)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 24, 5, N'Bánh Oreo ngon, đúng vị.', '2025-05-11 17:00:00'),
(13, 24, 4, N'Bánh ngon, nhưng gói hơi nhỏ.', '2025-05-13 19:10:00'),
(14, 24, 3, N'Bánh ổn, nhưng giá hơi cao.', '2025-05-16 11:20:00'),
(15, 24, 5, N'Bánh Oreo rất ngon, đáng tiền.', '2025-05-19 13:30:00'),
(16, 24, 4, N'Bánh chất lượng, giao nhanh.', '2025-05-21 15:40:00'),
(17, 24, 2, N'Bánh hơi khô, cần cải thiện.', '2025-05-23 17:50:00'),
(18, 24, 5, N'Bánh Oreo rất ngon, rất hài lòng.', '2025-05-26 19:00:00'),
(19, 24, 4, N'Bánh ngon, nhưng gói hơi ít.', '2025-05-28 11:10:00'),
(2, 24, 3, N'Bình thường, không đặc biệt.', '2025-05-31 14:20:00'),
(1, 24, 5, N'Bánh Oreo rất ngon, sẽ mua lại.', '2025-06-03 16:30:00');

-- Snack khoai tây (ProductId = 25)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 25, 4, N'Snack khoai tây ngon, nhưng hơi mặn.', '2025-05-10 16:10:00'),
(3, 25, 5, N'Snack rất ngon, vị BBQ tuyệt vời.', '2025-05-12 18:20:00'),
(4, 25, 3, N'Snack ổn, nhưng giá hơi cao.', '2025-05-15 10:30:00'),
(5, 25, 5, N'Snack khoai tây rất ngon, đáng tiền.', '2025-05-18 12:40:00'),
(6, 25, 4, N'Snack chất lượng, giao nhanh.', '2025-05-20 14:50:00'),
(7, 25, 2, N'Snack hơi nhạt, cần cải thiện.', '2025-05-22 16:00:00'),
(8, 25, 5, N'Snack rất ngon, rất hài lòng.', '2025-05-25 18:10:00'),
(9, 25, 4, N'Snack ngon, nhưng gói nhỏ.', '2025-05-27 10:20:00'),
(10, 25, 3, N'Bình thường, không đặc biệt.', '2025-05-30 12:30:00'),
(11, 25, 5, N'Snack khoai tây rất ngon, sẽ mua lại.', '2025-06-02 15:40:00');

-- Sữa tươi Vinamilk (ProductId = 26)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 26, 5, N'Sữa tươi Vinamilk ngon, đúng vị.', '2025-05-11 18:20:00'),
(13, 26, 4, N'Sữa ngon, nhưng hộp hơi nhỏ.', '2025-05-13 10:30:00'),
(14, 26, 3, N'Sữa ổn, nhưng giá hơi cao.', '2025-05-16 12:40:00'),
(15, 26, 5, N'Sữa tươi rất ngon, đáng tiền.', '2025-05-19 14:50:00'),
(16, 26, 4, N'Sữa chất lượng, giao nhanh.', '2025-05-21 16:00:00'),
(17, 26, 2, N'Sữa hơi nhạt, cần cải thiện.', '2025-05-23 18:10:00'),
(18, 26, 5, N'Sữa tươi rất ngon, rất hài lòng.', '2025-05-26 10:20:00'),
(19, 26, 4, N'Sữa ngon, nhưng hộp nhỏ.', '2025-05-28 12:30:00'),
(2, 26, 3, N'Bình thường, không đặc biệt.', '2025-05-31 15:40:00'),
(1, 26, 5, N'Sữa tươi Vinamilk rất ngon, sẽ mua lại.', '2025-06-03 17:50:00');

-- Sữa chua Vinamilk (ProductId = 27)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 27, 4, N'Sữa chua ngon, nhưng hơi ngọt.', '2025-05-10 17:30:00'),
(3, 27, 5, N'Sữa chua Vinamilk rất ngon, đúng vị.', '2025-05-12 19:40:00'),
(4, 27, 3, N'Sữa chua ổn, nhưng giá hơi cao.', '2025-05-15 11:50:00'),
(5, 27, 5, N'Sữa chua rất ngon, đáng tiền.', '2025-05-18 13:00:00'),
(6, 27, 4, N'Sữa chua chất lượng, giao nhanh.', '2025-05-20 15:10:00'),
(7, 27, 2, N'Sữa chua hơi nhạt, cần cải thiện.', '2025-05-22 17:20:00'),
(8, 27, 5, N'Sữa chua rất ngon, rất hài lòng.', '2025-05-25 19:30:00'),
(9, 27, 4, N'Sữa chua ngon, nhưng hộp nhỏ.', '2025-05-27 11:40:00'),
(10, 27, 3, N'Bình thường, không đặc biệt.', '2025-05-30 13:50:00'),
(11, 27, 5, N'Sữa chua rất ngon, sẽ mua lại.', '2025-06-02 16:00:00');

-- Phô mai Con Bò Cười (ProductId = 28)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 28, 5, N'Phô mai Con Bò Cười ngon, đúng vị.', '2025-05-11 19:40:00'),
(13, 28, 4, N'Phô mai ngon, nhưng hộp nhỏ.', '2025-05-13 11:50:00'),
(14, 28, 3, N'Phô mai ổn, nhưng giá hơi cao.', '2025-05-16 13:00:00'),
(15, 28, 5, N'Phô mai rất ngon, đáng tiền.', '2025-05-19 15:10:00'),
(16, 28, 4, N'Phô mai chất lượng, giao nhanh.', '2025-05-21 17:20:00'),
(17, 28, 2, N'Phô mai hơi khô, cần cải thiện.', '2025-05-23 19:30:00'),
(18, 28, 5, N'Phô mai rất ngon, rất hài lòng.', '2025-05-26 11:40:00'),
(19, 28, 4, N'Phô mai ngon, nhưng hộp nhỏ.', '2025-05-28 13:50:00'),
(2, 28, 3, N'Bình thường, không đặc biệt.', '2025-05-31 16:00:00'),
(1, 28, 5, N'Phô mai Con Bò Cười rất ngon, sẽ mua lại.', '2025-06-03 18:10:00');

-- Sữa tắm Dove (ProductId = 29)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(2, 29, 4, N'Sữa tắm Dove thơm, nhưng hơi ít bọt.', '2025-05-10 18:50:00'),
(3, 29, 5, N'Sữa tắm rất thơm, da mềm mịn.', '2025-05-12 10:00:00'),
(4, 29, 3, N'Sữa tắm ổn, nhưng giá hơi cao.', '2025-05-15 12:10:00'),
(5, 29, 5, N'Sữa tắm Dove rất tốt, đáng tiền.', '2025-05-18 14:20:00'),
(6, 29, 4, N'Sữa tắm chất lượng, giao nhanh.', '2025-05-20 16:30:00'),
(7, 29, 2, N'Hơi khô da, cần cải thiện.', '2025-05-22 18:40:00'),
(8, 29, 5, N'Sữa tắm rất thơm, rất hài lòng.', '2025-05-25 10:50:00'),
(9, 29, 4, N'Sữa tắm ngon, nhưng chai nhỏ.', '2025-05-27 13:00:00'),
(10, 29, 3, N'Bình thường, không đặc biệt.', '2025-05-30 15:10:00'),
(11, 29, 5, N'Sữa tắm Dove rất tốt, sẽ mua lại.', '2025-06-02 17:20:00');

-- Kem đánh răng Colgate (ProductId = 30)
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, ReviewDate) VALUES
(12, 30, 5, N'Kem đánh răng Colgate tốt, sạch răng.', '2025-05-11 10:10:00'),
(13, 30, 4, N'Kem đánh răng ngon, nhưng tuýp nhỏ.', '2025-05-13 12:20:00'),
(14, 30, 3, N'Kem đánh răng ổn, nhưng giá cao.', '2025-05-16 14:30:00'),
(15, 30, 5, N'Kem đánh răng rất tốt, đáng tiền.', '2025-05-19 16:40:00'),
(16, 30, 4, N'Kem đánh răng chất lượng, giao nhanh.', '2025-05-21 18:50:00'),
(17, 30, 2, N'Hơi cay, cần cải thiện.', '2025-05-23 10:00:00'),
(18, 30, 5, N'Kem đánh răng rất tốt, rất hài lòng.', '2025-05-26 12:10:00'),
(19, 30, 4, N'Kem đánh răng ngon, nhưng tuýp nhỏ.', '2025-05-28 14:20:00'),
(2, 30, 3, N'Bình thường, không đặc biệt.', '2025-05-31 16:30:00'),
(1, 30, 5, N'Kem đánh răng Colgate rất tốt, sẽ mua lại.', '2025-06-03 18:40:00');


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

UPDATE Orders SET CanCancel = 1 WHERE CanCancel IS NULL;
ALTER TABLE Orders ALTER COLUMN CanCancel bit NOT NULL;
ALTER TABLE Orders ADD CONSTRAINT DF_Orders_CanCancel DEFAULT 1 FOR CanCancel;

INSERT INTO Promotions (PromotionName, StartDate, EndDate, ImagePath, ShowOnTop)
VALUES 
('Summer Blast', '2025-06-01 00:00:00', '2025-06-30 23:59:59', 'promotions/summer-blast.jpg', 1),
('Healthy Week', '2025-06-03 00:00:00', '2025-06-10 23:59:59', 'promotions/healthy-week.jpg', 1),
('Fresh Deals', '2025-06-01 00:00:00', '2025-06-15 23:59:59', 'promotions/fresh-deals.jpg', 0),
('Meat Lovers Sale', '2025-05-28 00:00:00', '2025-06-14 23:59:59', 'promotions/meat-sale.jpg', 0),
('Veggie Boost', '2025-06-02 00:00:00', '2025-06-20 23:59:59', 'promotions/veggie-boost.jpg', 0);


INSERT INTO PromotionDetails (PromotionID, ProductID, NewPrice)
VALUES 
(3, 2, 25000),
(3, 5, 20000),
(3, 10, 90000),
(3, 18, 50000),
(3, 17, 30000),

(4, 3, 499000),
(4, 14, 140000),
(4, 12, 10000000),
(4, 15, 30000),

(5, 6, 130000),
(5, 9, 10000),
(5, 16, 25000),
(5, 4, 1100000);

