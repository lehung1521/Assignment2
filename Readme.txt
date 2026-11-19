SE Lab Assignment 2 - B2B Wholesale Management System
Course: Software Engineering Lab (Sem 1_2627)

GitHub repository : https://github.com/lehung1521/Assignment2
Video demo (optional): <paste link here>

Web option: Option C (Exer 4) - ASP.NET Core MVC, Code First (.NET 10)

Projects in the solution:
- BackOffice.WinForms : WinForms back-office application (Admin only)
- AgentPortal.Web     : ASP.NET Core MVC Agent Portal (Agent only)
- B2BWholesale.Tests  : Unit test project (xUnit)
- Database/B2BWholesaleDB.sql : database design and seed data (reference script)

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
Default value: Server=(localdb)\MSSQLLocalDB;Database=B2BWholesaleDB;Trusted_Connection=True;
If your computer uses another SQL Server (for example .\SQLEXPRESS), change only the
Server= part in BOTH files. Windows Authentication is used, so no password is stored.

How to run:
1. Open the .sln file in Visual Studio (.NET 10 SDK required).
2. Create the database (see Database/ folder and the project notes), then build the solution.
3. Set BackOffice.WinForms or AgentPortal.Web as the startup project and press F5.
4. Open Package Manager Console, pick Startup Project as AgentPortal.Web, and run Update-Database -Project B2BWholesale.Data -StartupProject AgentPortal.Web
