# Entity Framework 資料遷移過程

## 遷移步驟

### 1. 檢查資料庫連線設定
檢查了 `appsettings.json` 中的資料庫連線字串：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MinimalStoreDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 2. 安裝 Entity Framework Core 工具

```bash
dotnet tool install --global dotnet-ef
```


### 3. 建立初始資料遷移

```bash
dotnet ef migrations add InitialCreate
```



### 4. 執行資料遷移到資料庫

```bash
dotnet ef database update
```

## 建立的資料表

遷移成功建立了以下資料表：

1. **Categories** - 商品分類表
   - Id (主鍵, IDENTITY)
   - Name (分類名稱)
   - Description (分類描述)
   - CreatedAt (建立時間)

2. **Products** - 商品表
   - Id (主鍵, IDENTITY)
   - Name (商品名稱)
   - Description (商品描述)
   - Price (decimal 18,2)
   - Stock (庫存數量)
   - CategoryId (外鍵關聯到 Categories)
   - CreatedAt (建立時間)

3. **Orders** - 訂單表
   - Id (主鍵, IDENTITY)
   - CustomerName (客戶姓名)
   - CustomerEmail (客戶信箱)
   - TotalAmount (decimal 18,2)
   - Status (訂單狀態)
   - CreatedAt (建立時間)

4. **OrderItems** - 訂單項目表
   - Id (主鍵, IDENTITY)
   - OrderId (外鍵關聯到 Orders)
   - ProductId (外鍵關聯到 Products)
   - Quantity (數量)
   - UnitPrice (單價, decimal 18,2)

5. **Users** - 使用者表
   - Id (主鍵, IDENTITY)
   - UserName (使用者名稱)
   - Email (電子信箱)
   - PasswordHash (密碼雜湊)
   - CreatedAt (建立時間)

6. **RefreshTokens** - 刷新令牌表
   - Id (主鍵, IDENTITY)
   - Token (令牌)
   - UserId (外鍵關聯到 Users)
   - ExpiresAt (過期時間)
   - CreatedAt (建立時間)
   - IsRevoked (是否已撤銷)
   - RevokedBy (撤銷者)
   - RevokedAt (撤銷時間)

## 建立的索引

- IX_OrderItems_OrderId
- IX_OrderItems_ProductId
- IX_Products_CategoryId
- IX_RefreshTokens_UserId

## 外鍵關係

- Products.CategoryId → Categories.Id (CASCADE DELETE)
- RefreshTokens.UserId → Users.Id (CASCADE DELETE)
- OrderItems.OrderId → Orders.Id (CASCADE DELETE)
- OrderItems.ProductId → Products.Id (CASCADE DELETE)

## 遷移檔案

- 遷移 ID: 20250925033817_InitialCreate
- EF Core 版本: 8.0.7