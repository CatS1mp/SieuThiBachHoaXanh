USE BachHoaXanh
GO

DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS ProductImages;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS SubCategories;
DROP TABLE IF EXISTS FavoriteProducts;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS PaymentMethods;
DROP TABLE IF EXISTS Categories;
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
    StockQuantity INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT DEFAULT 1, -- Sản phẩm có khả dụng không
    FOREIGN KEY (SubCategoryID) REFERENCES SubCategories(SubCategoryID) ON DELETE SET NULL
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
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
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

-- Dữ liệu cho bảng Categories
INSERT INTO Categories (CategoryName) 
VALUES 
(N'Điện tử'),
(N'Gia dụng'),
(N'Thuốc & Sức khỏe'),
(N'Thực phẩm'),
(N'Nước giải khát'),
(N'Văn phòng phẩm'),
(N'Mỹ phẩm'),
(N'Trái cây tươi'),
(N'Rau củ quả'),
(N'Chăm sóc mẹ và bé');

-- Dữ liệu cho bảng SubCategories
INSERT INTO SubCategories (SubCategoryName, CategoryID) 
VALUES 
(N'Thiết bị di động', 1),
(N'Ti vi', 1),
(N'Quạt điện', 2),
(N'Máy xay sinh tố', 2),
(N'Thuốc giảm đau', 3),
(N'Vitamins & Supplements', 3),
(N'Thịt tươi', 4),
(N'Cơm & Mì', 4),
(N'Nước ngọt', 5),
(N'Cafe', 5),
(N'Sổ', 6),
(N'Máy tính', 6),
(N'Son môi', 7),
(N'Sữa rửa mặt', 7),
(N'Bưởi', 8),
(N'Dưa hấu', 8),
(N'Khoai tây', 9),
(N'Ớt chuông', 9),
(N'Bánh kẹo', 10);

-- Dữ liệu cho bảng Products
INSERT INTO Products (ProductName, Description, Price, SubCategoryID, StockQuantity) 
VALUES 
(N'Iphone 13', N'Điện thoại thông minh Apple, 128GB', 20000000, 1, 100),
(N'Smart TV Samsung 50 inch', N'Ti vi thông minh, độ phân giải 4K', 15000000, 2, 50),
(N'Quạt điện Panasonic', N'Quạt đứng, 3 tốc độ', 800000, 3, 150),
(N'Máy xay sinh tố Philips', N'Máy xay sinh tố 600W', 1200000, 4, 120),
(N'Thuốc giảm đau Panadol', N'Thuốc giảm đau, hạ sốt', 50000, 5, 200),
(N'Vitamins D3', N'Viên uống vitamin D3', 150000, 6, 80),
(N'Thịt bò tươi', N'Thịt bò nhập khẩu từ Mỹ', 350000, 7, 300),
(N'Cơm tấm 500g', N'Cơm tấm sườn nướng, gạo tấm ngon', 70000, 8, 400),
(N'Nước ngọt Pepsi', N'Nước ngọt Pepsi, chai 1.5L', 15000, 9, 500),
(N'Cafe nguyên chất', N'Cafe hạt rang xay nguyên chất', 95000, 10, 250),
(N'Sổ tay A5', N'Sổ tay bìa cứng', 25000, 11, 600),
(N'Máy tính xách tay Dell', N'Máy tính xách tay Dell 14 inch', 12000000, 12, 75),
(N'Son môi MAC', N'Son môi màu đỏ, lâu trôi', 500000, 13, 200),
(N'Sữa rửa mặt Olay', N'Sữa rửa mặt Olay cho da nhạy cảm', 150000, 14, 100),
(N'Bưởi Da xanh', N'Bưởi Da xanh tươi ngon', 50000, 15, 250),
(N'Dưa hấu không hạt', N'Dưa hấu ngọt, không hạt', 30000, 16, 400),
(N'Khoai tây tươi', N'Khoai tây tươi, nhập khẩu', 35000, 17, 600),
(N'Ớt chuông đỏ', N'Ớt chuông đỏ, tươi ngon', 60000, 18, 180),
(N'Bánh Oreo', N'Bánh Oreo gói 200g', 45000, 19, 500),
(N'Kẹo dẻo Haribo', N'Kẹo dẻo Haribo gói 100g', 30000, 19, 700);

-- Dữ liệu cho bảng Users
INSERT INTO Users (UserName, FullName, Email, Password, Phone, Role)
VALUES 
(N'admin', N'Lê Anh Tuấn', N'leanhtuank16@siu.edu.vn', N'4297f44b13955235245b2497399d7a93', N'0123456789', N'Admin'),
(N'tranvanminh', N'Trần Văn Minh', N'tranvanminhk16@siu.edu.vn', N'4297f44b13955235245b2497399d7a93', N'0912345678', N'Customer'),
(N'kho', N'Lê Minh C', N'le.minh.c@email.com', N'4297f44b13955235245b2497399d7a93', N'0912345679', N'Customer'),
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
UPDATE Users SET Address = N'789 Đường DEF, Phường 3, Quận 3, TP.HCM' WHERE UserName = N'le_minh_c';
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

-- Dữ liệu cho bảng ProductImages
INSERT INTO ProductImages (ProductID, ImagePath, IsMainImage)
VALUES 
(1, N'iphone13.jpg', 1),
(2, N'smarttv.jpg', 1),
(3, N'quatdien.jpg', 1),
(4, N'mayxaysinhto.jpg', 1),
(5, N'pan10.jpg', 1),
(6, N'vitaminD3.jpg', 1),
(7, N'thitbo.jpg', 1),
(8, N'comtam.jpg', 1),
(9, N'pepsi.jpg', 1),
(10, N'cafe.jpg', 1),
(11, N'sotay.jpg', 1),
(12, N'maytinhxachtay.jpg', 1),
(13, N'sonmoi.jpg', 1),
(14, N'suaruamat.jpg', 1),
(15, N'buoi1.jpg', 1),
(16, N'duahuakhonghat.jpg', 1),
(17, N'khoaitay.jpg', 1),
(18, N'otchuong.jpg', 1),
(19, N'oreo.jpg', 1),
(20, N'keodeo.jpg', 1),
(1, N'iphone13_2.jpg', 0),
(2, N'smarttv_2.jpg', 0),
(3, N'quatdien_2.jpg', 0),
(4, N'mayxaysinhto_2.jpg', 0),
(5, N'panadol_2.jpg', 0),
(6, N'vitaminD3_2.jpg', 0),
(7, N'thitbo_2.jpg', 0),
(8, N'comtam_2.jpg', 0),
(9, N'pepsi_2.jpg', 0),
(10, N'cafe_2.jpg', 0),
(11, N'sotay_2.jpg', 0),
(12, N'maytinhxachtay_2.jpg', 0),
(13, N'sonmoi_2.jpg', 0),
(14, N'suaruamat_2.jpg', 0),
(15, N'buoi2.jpg', 0),
(16, N'duahuakhonghat_2.jpg', 0),
(17, N'khoaitay_2.jpg', 0),
(18, N'otchuong_2.jpg', 0),
(19, N'oreo_2.jpg', 0),
(20, N'keodeo_2.jpg', 0);

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