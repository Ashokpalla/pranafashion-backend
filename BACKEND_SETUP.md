# 🔧 Prana Fashion Studio — Backend Setup Guide

## Understanding the 3 Projects

```
backend/
├── PranaFashion.sln                    ← Master file that links all 3 projects
│
├── PranaFashion.Core/                  ← Layer 1: Pure C# classes, no DB, no HTTP
│   ├── Models/Models.cs                   Product, User, Order, Enquiry
│   ├── DTOs/DTOs.cs                       Request/Response data shapes
│   └── PranaFashion.Core.csproj           (no external dependencies)
│
├── PranaFashion.Infrastructure/        ← Layer 2: Database access
│   ├── Data/AppDbContext.cs               EF Core DbContext + seed data
│   └── PranaFashion.Infrastructure.csproj → REFERENCES Core ✓
│
└── PranaFashion.API/                   ← Layer 3: HTTP controllers
    ├── Controllers/*.cs                   Auth, Products, Orders, Enquiries
    ├── Program.cs                         App startup, JWT, CORS, Swagger
    ├── appsettings.json                   DB connection string + JWT secret
    └── PranaFashion.API.csproj            → REFERENCES Core + Infrastructure ✓
```

The .csproj files already wire everything together.
You do NOT manually inject dependencies — dotnet restore handles it all.

---

## Step 1 — Install Prerequisites (one time)

1. Download .NET 8 SDK:
   https://dotnet.microsoft.com/download/dotnet/8.0

2. Download SQL Server Express (free):
   https://www.microsoft.com/en-in/sql-server/sql-server-downloads
   → Choose "Express" edition

3. Verify installation:
   dotnet --version    (should show 8.x.x)

---

## Step 2 — Open backend folder in terminal

   cd path\to\prana-fashion-studio\backend

---

## Step 3 — Restore all NuGet packages

   dotnet restore PranaFashion.sln

This downloads all dependencies for all 3 projects automatically.

---

## Step 4 — Build to verify no errors

   dotnet build PranaFashion.sln

Expected output:
   Build succeeded.
       0 Warning(s)
       0 Error(s)

---

## Step 5 — Configure Database Connection

Open: backend/PranaFashion.API/appsettings.json

For LocalDB (comes with Visual Studio):
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PranaFashionDB;Trusted_Connection=True;TrustServerCertificate=True;"

For SQL Server Express:
   "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PranaFashionDB;Trusted_Connection=True;TrustServerCertificate=True;"

---

## Step 6 — Run the API

   cd PranaFashion.API
   dotnet run

First run automatically:
  - Creates PranaFashionDB database
  - Creates all tables (Products, Users, Orders, Enquiries)
  - Seeds 8 demo products

You will see:
   Now listening on: http://localhost:5000

---

## Step 7 — Verify it works

Open browser: http://localhost:5000/swagger

You will see Swagger UI with all API endpoints listed.
Try clicking GET /api/products/featured → Execute → you will see the 8 demo products.

---

## Running Frontend + Backend Together

Terminal 1 (Backend):
   cd backend\PranaFashion.API
   dotnet run

Terminal 2 (Frontend):
   cd prana-fashion-studio
   npm install
   ng serve

Open: http://localhost:4200

---

## Common Errors

ERROR: "Cannot connect to SQL Server"
FIX: Open Windows Services → Start "SQL Server (SQLEXPRESS)" or "SQL Server (MSSQLSERVER)"

ERROR: "LocalDB is not installed"
FIX: Change connection string to use localhost\\SQLEXPRESS instead

ERROR: "Port 5000 already in use"
FIX: Run on different port:
   dotnet run --urls "http://localhost:5001"
   Then update src/environments/environment.ts → apiUrl: 'http://localhost:5001/api'

ERROR: "namespace not found" during build
FIX: Make sure you run from the backend/ folder, not inside a project folder:
   cd backend
   dotnet restore PranaFashion.sln
   dotnet build PranaFashion.sln

---

## Adding EF Migrations (when you change Models)

   cd backend
   dotnet ef migrations add DescribeYourChange --project PranaFashion.Infrastructure --startup-project PranaFashion.API
   dotnet ef database update --project PranaFashion.Infrastructure --startup-project PranaFashion.API

---

## API Quick Reference (at http://localhost:5000)

POST   /api/auth/register      Create account         (no login needed)
POST   /api/auth/login         Login, get JWT token   (no login needed)
GET    /api/products           List + filter products (no login needed)
GET    /api/products/featured  Homepage products      (no login needed)
GET    /api/products/{id}      Single product         (no login needed)
POST   /api/enquiries          Contact form           (no login needed)
GET    /api/orders/my          My order history       (login required)
POST   /api/orders             Place an order         (login required)

Test all at: http://localhost:5000/swagger
