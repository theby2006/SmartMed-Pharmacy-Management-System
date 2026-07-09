# SmartMed Pharmacy Management System

A layered .NET Framework 4.8 Windows Forms application for managing a pharmacy's medicine catalogue, suppliers, purchasing, stock, point-of-sale transactions, customer orders, and reporting.

## Prerequisites

- Windows with Visual Studio 2015 or later (Visual Studio 2022 recommended) — the solution targets `net48` using the modern SDK-style project format
- .NET Framework 4.8 Developer Pack
- Microsoft SQL Server (any edition — LocalDB, Express, or a full instance) reachable as `Data Source=.` with Windows Integrated Security, or update the connection string described below

## Database Setup

1. Open `SmartMed.DAL/Scripts/000_SetupDatabase.sql` in SQL Server Management Studio (or `sqlcmd`) connected to your target SQL Server instance.
2. Execute the script. It creates the `SmartMedDb` database if it does not already exist, then creates every table (Users, AuditLogs, MedicineCategories, Medicines, Suppliers, Purchases, PurchaseItems, StockBatches, StockMovements, Sales, SaleItems, Payments, Customers, Orders, OrderItems, Settings, BackupHistory, UserPreferences, ErrorLogs, PerformanceLogs) and seeds sample data, including one login account per staff role.
3. All scripts are idempotent (`IF NOT EXISTS` guarded), so re-running `000_SetupDatabase.sql` is safe.

No further manual steps are required — the application does not run migrations at startup.

### Default seeded credentials

| Role | Username / Identifier | Password / PIN |
|---|---|---|
| Administrator | `admin` | `Admin@123` |
| Pharmacist | `pharmacist` | `Pharm@123` |
| Cashier | `cashier` | `Cash@123` |
| Customer (example) | phone `0771234567` or email `jane.doe@example.com` | PIN login is disabled by default (`CustomerPinEnabled=false` in `App.config`); identifier alone is sufficient unless enabled |

Change these after first login in a real deployment — they exist to let a marker or reviewer exercise every role immediately after setup.

## Connection String

`SmartMed.UI/App.config` and `SmartMed.Tests/App.config` both define the `SmartMedDb` connection string:

```xml
<connectionStrings>
  <add name="SmartMedDb"
       connectionString="Data Source=.;Initial Catalog=SmartMedDb;Integrated Security=True;MultipleActiveResultSets=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

Edit `Data Source=.` if your SQL Server instance is named differently (e.g. `.\SQLEXPRESS`).

## Building and Running

1. Open `SmartMed.sln` in Visual Studio.
2. Restore/build the solution (`Build > Build Solution`, or `dotnet build SmartMed.sln` from a command prompt).
3. Set `SmartMed.UI` as the startup project and run.
4. The login screen accepts staff credentials directly, or a "Login as Customer?" link for the customer identification flow.

## Solution Structure

| Project | Responsibility |
|---|---|
| `SmartMed.Common` | Cross-cutting utilities: configuration access (`AppSettings`), validation guards (`Guard`), the custom exception hierarchy |
| `SmartMed.Models` | Entity classes, enums, report DTOs, and the `OperationResult` result-pattern wrapper shared by every layer |
| `SmartMed.DAL` | Data access layer — hand-written parameterized ADO.NET repositories against SQL Server, plus the DDL scripts under `Scripts/` |
| `SmartMed.BLL` | Business logic layer — one service per domain area, returning `OperationResult`/`OperationResult<T>` rather than throwing outward |
| `SmartMed.Reports` | Reserved for reporting/export infrastructure |
| `SmartMed.UI` | The WinForms desktop application — forms, manual dependency wiring in `Bootstrap/ApplicationBootstrapper.cs`, and print/export components |
| `SmartMed.Tests` | MSTest unit and integration tests covering BLL, DAL, Models, Common, and selected UI forms |

## Feature Overview

- **Authentication** — staff login with PBKDF2-hashed passwords, account lockout after repeated failures, session timeout, and audit logging; a separate customer identification/lookup login.
- **Catalogue management** — medicine categories and medicines, including reorder thresholds and expiry tracking.
- **Procurement** — supplier records and a purchase order workflow (create → confirm → stock batch creation).
- **Inventory** — batch-level, FIFO-consumed stock tracking with low-stock and near-expiry visibility.
- **Point of sale** — cart-based sales with discounts, tax, payment capture, and printed invoices.
- **Customer domain** — customer registration, order placement against the medicine catalogue, and an admin-managed order status workflow (Pending → Approved/Processing → Completed, or Cancelled), with optional prescription file upload for medicines flagged as requiring one.
- **Reporting** — a range of sales, purchasing, inventory, and analytics reports with CSV export and print support.
- **Administration** — database-backed dynamic settings, backup/restore, health checks, and performance monitoring at the service layer.

## Known Limitations

This is coursework-stage software; the following are documented rather than hidden:

- Session management assumes a single logged-in user per running process (a desktop-application characteristic, not a multi-terminal server).
- Some administrative capabilities (settings management, backup/restore, health/performance monitoring) are implemented at the service layer without a dedicated UI screen in this iteration.
