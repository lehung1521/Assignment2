SE Lab Assignment 2 - B2B Wholesale Management System
Course: Software Engineering Lab (Sem 1_2627)

GitHub repository : https://github.com/lehung1521/Assignment2
Video demo (optional): <paste link here>

Web option: Option C (Exer 4) - ASP.NET Core MVC, Code First (.NET 10)

Projects in the solution:
- BackOffice.WinForms : WinForms back-office application (Admin only)
- AgentPortal.Web     : ASP.NET Core MVC Agent Portal (Agent only)
- B2BWholesale.Data   : EF Core models, DbContext, migrations and seed data
- B2BWholesale.Tests  : Unit test project (xUnit)
- Database/B2BWholesaleDB.sql : reference script only (design notes and integrity tests).
  The real database is created from Migrations/ + SeedData.cs (see "How to run").

Members and contribution (must total 100%):
- Nguyen Le Hung (524H0051)      : 20%
- Nguyen Ngoc Quy (524H0184)     : 20%
- Le Quang Phuc (524H0181)       : 20%
- Phan Hoai An (524H0139)        : 20%
- Vo Huynh Duy Tan (524H0187)    : 20%

How to configure the database connection:
The connection string is NOT hardcoded. It is stored in two files:
1. WinForms : B2BWholesale/BackOffice.WinForms/App.config
   -> edit the "Server=" part of the connection string named "DefaultConnection".
2. Web      : B2BWholesale/AgentPortal.Web/appsettings.json
   -> edit the "Server=" part of ConnectionStrings:DefaultConnection.
Default value: Server=(localdb)\MSSQLLocalDB;Database=B2BWholesaleDB;Trusted_Connection=True;TrustServerCertificate=True;
If your computer uses another SQL Server (for example .\SQLEXPRESS), change only the
Server= part in BOTH files. Windows Authentication is used, so no password is stored.

Requirements:
- .NET 10 SDK, Visual Studio 2022 (17.13 or newer) or Visual Studio 2026
  with the ".NET desktop" and "ASP.NET and web" workloads.
- SQL Server LocalDB (installed with Visual Studio) or any SQL Server instance.

How to run:
1. Open B2BWholesale/B2BWholesale.slnx in Visual Studio and build the solution.
2. Check the connection string in BOTH config files (see above). The default works with LocalDB.
3. Create the database. Open Tools > NuGet Package Manager > Package Manager Console and run:
   Update-Database -Project B2BWholesale.Data -StartupProject AgentPortal.Web
   This creates B2BWholesaleDB with all tables, constraints and the seed data.
4. Set BackOffice.WinForms or AgentPortal.Web as the startup project and press F5.

Test accounts (seed data):
- Admin (WinForms) : admin01 / Admin@123
- Agent (Web)      : agent01 / Agent@123
- agent19 is locked on purpose (to test the lock feature).

Task 3.2 (Agent Portal): log in as agent01, open "Catalogue" or "New Order", add several items and place the order.
- The Order, its OrderDetails and the stock deduction run in ONE database transaction (OrderPlacementService).
- Try to order more than the stock (for example 9 packs of "Bánh quy Cosy", stock 8): the whole order is blocked and nothing changes.
- The new order appears in "My Orders" (web) and in the Orders tab of the WinForms app.

Task 1.4 (WinForms): log in as admin01 and open the "Dashboard" tab.
- Best-selling items, low-stock warning (red rows) and revenue by agent, with date range / Top N / agent filters.
- The default low-stock threshold (20) is stored in BackOffice.WinForms/App.config (appSettings, key LowStockThreshold).
- Cancelled orders are not counted as sales.