
# Minimal Store - 倉儲後端 API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4)](https://docs.microsoft.com/en-us/ef/)
[![Test-Driven Development](https://img.shields.io/badge/Development-TDD-green)](https://en.wikipedia.org/wiki/Test-driven_development)
[![xUnit](https://img.shields.io/badge/Testing-xUnit-blue)](https://xunit.net/)

一個使用 **.NET 8** 建構的倉儲 API，展示了 **分層式架構**、**TDD 測試驅動開發** 與 **商業邏輯驗證** 的實作。此專案強調專業開發實務，包括職責分離、測試覆蓋與錯誤處理。

---

## 📌 專案亮點

* **測試驅動開發 (TDD)** - 實作完整的 Red-Green-Refactor 流程
* **分層式架構** - 清楚區分 Controller、Service、Repository 與 Model
* **測試策略** - 以整合測試 (Integration Test) 驗證 API 行為與商業邏輯

---

## 🏗️ 系統架構

```
Minimal.Store.API/
├── Controllers/           # API 控制器 (HTTP 入口)
├── Services/             # 商業邏輯層
│   ├── Interfaces/       # Service 介面定義
│   └── Implementations/  # Service 實作
├── Data/                 # 資料存取層
│   ├── Context/         # EF Core DbContext
│   └── Repositories/    # Repository 模式實作
├── Models/              # 資料模型
│   ├── Entities/       # 資料庫實體
│   └── DTOs/           # 資料傳輸物件
└── Extensions/          # DI 與系統設定

Minimal.Store.Tests/
└── Controllers/        # 整合測試
```

---

## ⚙️ 技術堆疊

### 後端技術

* **.NET 8** - 最新 LTS 框架
* **ASP.NET Core Web API** - 建立 RESTful API
* **Entity Framework Core 8** - ORM，提供 LINQ 查詢能力
* **SQL Server** - 關聯式資料庫

### 測試框架

* **xUnit** - 測試框架
* **FluentAssertions** - 具表達性的斷言工具
* **InMemory Database** - 測試用資料庫

### 開發實務

* **Repository Pattern** - 抽象化資料存取
* **Dependency Injection (DI)** - 降低耦合、提升可測試性
* **DTO Pattern** - 分離 API contract 與 Domain Model
* **Service Layer Pattern** - 封裝商業邏輯

---

## 🗄️ 資料庫設計

### 主要實體 (Core Entities)

**Categories**

```sql
- Id (int, PK)
- Name (nvarchar(100))
- Description (nvarchar(500))
- CreatedAt (datetime2)
```

**Products**

```sql
- Id (int, PK)
- Name (nvarchar(200))
- Description (nvarchar(1000))
- Price (decimal(10,2))
- Stock (int)
- CategoryId (int, FK)
- CreatedAt (datetime2)
```

**Orders**

```sql
- Id (int, PK)
- CustomerName (nvarchar(100))
- CustomerEmail (nvarchar(200))
- TotalAmount (decimal(10,2))
- Status (nvarchar(20))
- CreatedAt (datetime2)
```

**OrderItems**

```sql
- Id (int, PK)
- OrderId (int, FK)
- ProductId (int, FK)
- Quantity (int)
- UnitPrice (decimal(10,2))
```

---

##  API 端點

### 類別 (Categories)

```http
GET    /api/categories           # 取得所有類別
GET    /api/categories/{id}      # 依 ID 取得類別
POST   /api/categories           # 建立新類別
PUT    /api/categories/{id}      # 更新類別
DELETE /api/categories/{id}      # 刪除類別
```

### 商品 (Products)

```http
GET    /api/products             # 取得所有商品
GET    /api/products/{id}        # 依 ID 取得商品
POST   /api/products             # 建立新商品
PUT    /api/products/{id}        # 更新商品
DELETE /api/products/{id}        # 刪除商品
```

### 訂單 (Orders)

```http
GET    /api/orders               # 取得所有訂單
GET    /api/orders/{id}          # 取得訂單詳細
POST   /api/orders               # 建立新訂單
PUT    /api/orders/{id}/status   # 更新訂單狀態
```

---

##  測試策略

### 覆蓋範圍 (Test Coverage)

* **整合測試 (Integration Tests)**

  * 驗證 API 行為與 HTTP 回應
  * 測試同時涵蓋 Controller、Service、Repository 與資料存取流程
  * 使用 InMemory Database 模擬資料庫

### 商業邏輯驗證

* 建立訂單前的庫存檢查
* 防止價格與庫存為負值
* 防止刪除仍有商品的分類
* 驗證訂單總金額計算正確性
* 訂單處理後自動更新庫存

---

##  快速開始

### 環境需求

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB 或 Express)
* [Visual Studio 2022](https://visualstudio.microsoft.com/) 或 [VS Code](https://code.visualstudio.com/)

### 安裝與設定

1. **下載專案**

   ```bash
   git clone https://github.com/kangcy28/Minimal.Store.git
   cd minimal-store
   ```

2. **設定資料庫連線**

   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MinimalStoreDB;Trusted_Connection=true;"
     }
   }
   ```

3. **執行資料庫遷移**

   ```bash
   dotnet ef database update
   ```

4. **啟動 API**

   ```bash
   dotnet run --project Minimal.Store.API
   ```

5. **執行測試**

   ```bash
   dotnet test
   ```

---

##  開發方法論

### TDD 循環

專案遵循 **Red-Green-Refactor** 流程：

1. **🔴 Red** - 先撰寫失敗的測試
2. **🟢 Green** - 撰寫最小實作使測試通過
3. **🔄 Refactor** - 重構程式碼並保持測試綠燈

---

##  主要特性

### 工程實務

* **整潔程式碼 (Clean Code)** - 清楚、可維護
* **SOLID 原則** - 適當的依賴管理與單一職責
* **錯誤處理** - 例外狀況管理
* **輸入驗證** - 確保資料完整與安全

---

## 專案指標

* **API 端點**：15+ RESTful endpoints
* **商業規則**：10+ 驗證案例
* **資料實體**：4 個核心 Domain Models
* **設計模式**：Repository、Service、DTO、DI

---
##  專業能力展示

此專案展現以下能力：

* 使用 **.NET 技術** 開發後端 API
* **測試驅動開發 (TDD)** 方法論
* **資料庫設計** 與 EF Core 應用
* **分層式架構** 的實作
* **RESTful API** 設計實務

---
## 未來規劃

* 加入身分驗證與授權 (JWT)
* 建立快取層 (Redis)
* 整合訊息佇列
* Docker 容器化
* CI/CD pipeline
* 效能監控
* API 流量限制
