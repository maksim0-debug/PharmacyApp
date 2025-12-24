DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'ALTER TABLE ' +
QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id))

+ '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) +
' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id IN (
OBJECT_ID('dbo.Medicines'),
OBJECT_ID('dbo.Categories'),
OBJECT_ID('dbo.Sales'),
OBJECT_ID('dbo.Users'),
OBJECT_ID('dbo.Stock'),
OBJECT_ID('dbo.SaleItems')
);
EXEC sp_executesql @sql;
GO

IF OBJECT_ID('dbo.SaleItems', 'U') IS NOT NULL DROP TABLE dbo.SaleItems;
IF OBJECT_ID('dbo.Stock', 'U') IS NOT NULL DROP TABLE dbo.Stock;
IF OBJECT_ID('dbo.Sales', 'U') IS NOT NULL DROP TABLE dbo.Sales;
IF OBJECT_ID('dbo.Medicines', 'U') IS NOT NULL DROP TABLE dbo.Medicines;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
UserID INT IDENTITY(1,1) PRIMARY KEY,
Login NVARCHAR(50) NOT NULL,
Password NVARCHAR(50) NOT NULL,
Role NVARCHAR(20) NOT NULL
);

CREATE TABLE dbo.Categories (
CategoryID INT IDENTITY(1,1) PRIMARY KEY,
CategoryName NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.Medicines (
MedicineID INT IDENTITY(1,1) PRIMARY KEY,
Name NVARCHAR(100) NOT NULL,
Manufacturer NVARCHAR(100),
CategoryID INT,
CONSTRAINT FK_Medicines_Categories FOREIGN KEY (CategoryID) REFERENCES

dbo.Categories(CategoryID)
);

CREATE TABLE dbo.Stock (
StockID INT IDENTITY(1,1) PRIMARY KEY,
MedicineID INT NOT NULL,
Quantity INT NOT NULL DEFAULT 0,
PurchasePrice DECIMAL(18, 2),
SellingPrice DECIMAL(18, 2),
ExpiryDate DATE,
CONSTRAINT FK_Stock_Medicines FOREIGN KEY (MedicineID) REFERENCES

dbo.Medicines(MedicineID)
);

CREATE TABLE dbo.Sales (
SaleID INT IDENTITY(1,1) PRIMARY KEY,
SaleDate DATETIME DEFAULT GETDATE(),
TotalAmount DECIMAL(18, 2),
UserID INT
);

CREATE TABLE dbo.SaleItems (
SaleItemID INT IDENTITY(1,1) PRIMARY KEY,
SaleID INT NOT NULL,
MedicineID INT NOT NULL,
Quantity INT NOT NULL,
PriceAtSale DECIMAL(18, 2),
CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleID) REFERENCES

dbo.Sales(SaleID),

CONSTRAINT FK_SaleItems_Medicines FOREIGN KEY (MedicineID) REFERENCES

dbo.Medicines(MedicineID)
);
GO

INSERT INTO Users (Login, Password, Role) VALUES (N'admin', N'admin',
N'Admin');
INSERT INTO Users (Login, Password, Role) VALUES (N'user', N'1234',
N'User');

INSERT INTO Categories (CategoryName) VALUES (N'Антибіотики'),
(N'Вітаміни'), (N'Знеболюючі');

INSERT INTO Medicines (Name, Manufacturer, CategoryID) VALUES
(N'Амоксил', N'Київмедпрепарат', 1),
(N'Вітамін C', N'Дарниця', 2),
(N'Анальгін', N'Фармак', 3),
(N'Цитрамон', N'Дарниця', 3);

INSERT INTO Stock (MedicineID, Quantity, PurchasePrice, SellingPrice,
ExpiryDate) VALUES
(1, 10, 50.00, 85.50, '2025-12-31'),
(2, 50, 20.00, 45.00, '2027-05-10'),
(3, 20, 15.00, 30.00, '2026-12-20'),
(4, 100, 10.00, 25.00, '2026-11-15');
GO
