# 2. Requirement Analysis

## Preface: Scope and Method of This Analysis

The requirements documented in this section were derived exclusively from a direct inspection of the SmartMed Pharmacy Management System source code, database scripts, configuration files, and automated test suite. No requirement listed below has been assumed, extrapolated from the project name, or copied from a generic pharmacy-system template. Where the implementation is incomplete — for instance, where a menu item exists but its handler performs no action, or where a database table is documented but not backed by a runnable creation script — this is stated explicitly rather than presented as a working feature. This approach was taken deliberately, because a requirements specification that describes a system more capable than the one actually delivered would mislead anyone using this document to plan testing, maintenance, or further development.

The system is built as a seven-project .NET Framework 4.8 solution — `SmartMed.Common`, `SmartMed.Models`, `SmartMed.DAL`, `SmartMed.BLL`, `SmartMed.Reports`, `SmartMed.UI`, and `SmartMed.Tests` — structured as a classic layered desktop application. The `SmartMed.UI` project is a Windows Forms executable; there is no web or mobile client. Business logic sits in `SmartMed.BLL`, data access in `SmartMed.DAL` using hand-written ADO.NET against SQL Server, and shared entities in `SmartMed.Models`. This architecture matters to the requirements that follow, because it explains why certain behaviours — such as session management being confined to a single running process, or reports being rendered through GDI+ printing rather than a dedicated reporting engine — are structural characteristics of the system rather than incidental omissions.

The system recognises four user roles through the `RoleType` enumeration: **Administrator**, **Pharmacist**, **Cashier**, and **Customer**. The first three share the same application shell (`MainShellForm`) and are distinguished only by which menu items become visible to them at runtime; the fourth, Customer, uses an entirely separate, lightweight lookup screen that was not designed to reach the same functional depth as the staff-facing modules. For this reason, Section 2.1 treats Administrator, Pharmacist, and Cashier together as the internal "staff" requirement set, calling out explicitly which functions are restricted to Administrators alone, and Section 2.2 is reserved for the Customer-facing capability, which is considerably narrower in scope than the staff side.

---

## 2.1 Functional Requirements – Administrator, Pharmacist, and Cashier (Staff)

### 2.1.1 Overview of Staff Access Control

Before listing individual requirements, it is worth explaining how access is actually enforced, because this shapes several of the requirements below. Role checking happens in three places. First, `SessionManager.HasRole(RoleType)` provides a simple equality check against the role stored in the current in-memory session. Second, `SalesService` contains a private `AuthorizeSaleOperation()` method that specifically restricts sale creation and cancellation to Administrators, Pharmacists, and Cashiers. Third, and most visibly, `MainShellForm.UpdateMenuVisibility()` toggles the `.Visible` property of top-level menu strips based on the logged-in user's role:

```csharp
bool isAdmin = session != null && session.Role == RoleType.Administrator;
bool isPharmacist = session != null && session.Role == RoleType.Pharmacist;
bool isCashier = session != null && session.Role == RoleType.Cashier;

_administrationMenu.Visible = isAdmin;
_prescriptionsMenu.Visible = isAdmin || isPharmacist;
_salesMenu.Visible = isAdmin || isCashier;
_reportsMenu.Visible = isAdmin;
```

It is important to note that this enforcement is inconsistent across the codebase. Outside of the Sales module and the menu-visibility logic quoted above, most `SmartMed.BLL` services — `MedicineService`, `SupplierService`, `PurchaseService`, `UserService`, and others — do not perform their own role checks. In practice this means that a form which is hidden from a given role in the shell could still be invoked programmatically without a role guard at the service layer. This gap is noted here because it is directly relevant to any future security hardening effort, and a requirements document that omitted it would understate a real characteristic of the delivered system.

### 2.1.2 Functional Requirements Table

| ID | Requirement Name | Priority |
|---|---|---|
| FR-STF-01 | Staff Authentication and Account Lockout | High |
| FR-STF-02 | Session Management and Timeout | High |
| FR-STF-03 | Medicine Category Management | Medium |
| FR-STF-04 | Medicine and Inventory Item Management | High |
| FR-STF-05 | Supplier Management | Medium |
| FR-STF-06 | Purchase Order Creation and Confirmation | High |
| FR-STF-07 | Stock Synchronisation and FIFO Batch Tracking | High |
| FR-STF-08 | Point-of-Sale Transaction Processing | High |
| FR-STF-09 | Sale Cancellation and Stock Reversal | Medium |
| FR-STF-10 | Report Generation and Export | Medium |
| FR-STF-11 | Operational Dashboard | Medium |
| FR-STF-12 | System Settings Management | Low |
| FR-STF-13 | Database Backup and Restore | Medium |
| FR-STF-14 | System Health Check and Performance Monitoring | Low |

Priority in this table reflects two combined signals found in the codebase itself rather than external judgement: the centrality of the function to the core purchase-to-sale workflow, and the depth of automated test coverage the developers invested in it (for example, the purchase workflow carries over forty test cases, while settings management and health checks carry only a handful). Where a function sits outside the direct sales/inventory pipeline but was still clearly built with care — such as reporting — it has been rated Medium rather than Low.

### 2.1.3 Detailed Requirement Descriptions

**FR-STF-01 — Staff Authentication and Account Lockout.** Administrators, Pharmacists, and Cashiers authenticate through `LoginForm`, which calls `AuthenticationService.LoginAdmin(username, password)`. The inputs are a username and password entered on the form. Processing validates that both fields are non-empty, retrieves the matching `User` record, confirms the account is active and not currently locked (checked against `User.LockedUntil`), and verifies the password using `PasswordHasher.VerifyPassword`, which recomputes a PBKDF2-SHA256 hash over the supplied password using the stored salt and compares it to the stored hash in constant time to resist timing attacks. A successful login resets the account's failed-attempt counter, records the login timestamp, writes an audit entry through `IAuditLogRepository`, and starts a session via `ISessionManager`. A failed login increments `User.FailedLoginAttempts`; once this reaches the configured maximum (`MaxFailedLoginAttempts`, set to 5 in `App.config`), the account is locked for a configured duration (`LockoutDurationMinutes`, set to 15 minutes) by writing a future timestamp into `User.LockedUntil`. The output in either case is a session object or a failure message displayed to the user, plus a persisted audit record. Related modules: `SmartMed.BLL.Services.AuthenticationService`, `SmartMed.BLL.Services.PasswordHasher`, `SmartMed.DAL.Repositories.UserRepository`, `SmartMed.UI.Forms.LoginForm`.

**FR-STF-02 — Session Management and Timeout.** Once authenticated, a user's identity, role, and activity timestamps are held by `SessionManager`, which implements `ISessionManager` as a thread-safe, single-session store — appropriate for a desktop application where one process serves one logged-in user at a time. Every access to the session refreshes `LastActivityTimeUtc`; if the configured idle period (`SessionTimeoutMinutes`, 20 minutes by default) elapses without activity, the session is treated as expired. Logout, triggered from the File menu in `MainShellForm`, records an audit entry and clears the session. Related modules: `SmartMed.BLL.Services.SessionManager`, `SmartMed.Models.Session.SessionContext`.

**FR-STF-03 — Medicine Category Management.** `MedicineCategoryForm` provides a data grid backed by `MedicineCategoryService`, allowing staff to add, update, deactivate, and refresh medicine categories (name and description). Categories are a required foreign key on every medicine record, so this function underpins the catalogue structure used throughout the rest of the system. Related modules: `SmartMed.BLL.Services.MedicineCategoryService`, `SmartMed.DAL.Repositories.MedicineCategoryRepository`, `SmartMed.UI.Forms.MedicineCategoryForm`.

**FR-STF-04 — Medicine and Inventory Item Management.** `MedicineForm` supports full CRUD management of medicine records — name, brand, dosage form, strength, unit, category, reorder level, unit price, and expiry — through `MedicineService`. The form additionally surfaces low-stock and near-expiry indicators directly in the UI, computed against the thresholds configured in `App.config` (`DefaultLowStockThreshold`, `NearExpiryThresholdDays`). It should be noted that although this form is fully implemented, it is not currently reachable from any menu item in `MainShellForm` — the same is true of `SupplierForm` — which means staff cannot navigate to it through the shipped navigation menu even though the underlying functionality works. Related modules: `SmartMed.BLL.Services.MedicineService`, `SmartMed.DAL.Repositories.MedicineRepository`, `SmartMed.UI.Forms.MedicineForm`.

**FR-STF-05 — Supplier Management.** `SupplierForm` allows staff to record and maintain supplier details — code, name, company, contact person, address, tax number, and notes — with an active/inactive filter, through `SupplierService`. As with medicine management, this form exists and functions correctly but has no menu entry point in the current build of `MainShellForm`. Related modules: `SmartMed.BLL.Services.SupplierService`, `SmartMed.DAL.Repositories.SupplierRepository`, `SmartMed.UI.Forms.SupplierForm`.

**FR-STF-06 — Purchase Order Creation and Confirmation.** `PurchaseForm` allows staff to select a supplier and build a purchase order from line items, each carrying batch number, expiry date, quantity, purchase price, selling price, discount, and tax. `PurchaseService.CreatePurchase` persists the order in a Pending state; `PurchaseService.ConfirmPurchase` then creates the corresponding `StockBatch` records and `StockMovement` entries and updates the affected medicines' stock quantities, all within a single database transaction so that a partially applied purchase cannot corrupt stock figures. Orders may also be cancelled before confirmation via `CancelPurchase`. This is the mechanism by which new stock enters the system, and it is the most heavily tested piece of business logic in the solution, with over forty associated automated tests. Related modules: `SmartMed.BLL.Services.PurchaseService`, `SmartMed.DAL.Repositories.PurchaseRepository`, `PurchaseItemRepository`, `StockBatchRepository`, `SmartMed.UI.Forms.PurchaseForm`.

**FR-STF-07 — Stock Synchronisation and FIFO Batch Tracking.** `InventoryService` maintains stock at the batch level rather than as a single running total per medicine. `GetFIFOBatches` and `DeductFIFO` implement first-in-first-out consumption, deducting quantity from the oldest active batch first and deactivating batches once exhausted; `SyncMedicineStock` recomputes a medicine's aggregate `StockQuantity` from its batch totals inside a transaction managed through `SqlUnitOfWork`, guarding against drift between the batch-level and medicine-level figures. This batch discipline is what allows the system to track expiry dates and cost basis per lot rather than treating stock as a single undifferentiated pool. Related modules: `SmartMed.BLL.Services.InventoryService`, `SmartMed.DAL.Repositories.StockBatchRepository`, `StockMovementRepository`, `SmartMed.DAL.Infrastructure.SqlUnitOfWork`.

**FR-STF-08 — Point-of-Sale Transaction Processing.** `SalesForm` is the system's point-of-sale screen: staff search for medicines, add them to a cart with batch selection, apply discounts and tax, take payment, and print an invoice. On submission, `SalesService.CreateSale` first confirms the caller holds an authorised role, validates every line item (positive quantity, discount and tax within 0–100 percent, payment amount greater than zero), then opens a manual database transaction in which it deducts stock via `InventoryService.DeductFIFO`, calculates totals through `PricingService`, generates a sequential sale number via `SaleNumberGenerator` (which uses a `SERIALIZABLE`-isolation transaction with an update lock hint to prevent duplicate numbers under concurrent use), inserts the sale, its line items, and the payment record, writes stock-movement entries, and updates medicine stock quantities — committing only if every step succeeds. The resulting invoice is rendered for printing by `SalePrintDocument`. This is the single most heavily tested user-facing screen in the solution, with twenty-seven dedicated UI tests. Related modules: `SmartMed.BLL.Services.SalesService`, `PricingService`, `SaleNumberGenerator`, `PaymentService`, `SmartMed.UI.Forms.SalesForm`, `SmartMed.UI.Components.SalePrintDocument`.

**FR-STF-09 — Sale Cancellation and Stock Reversal.** `SalesService.CancelSale` reverses a previously completed sale, restoring the deducted stock quantities and marking the sale record as cancelled, so that inventory figures remain accurate if a transaction needs to be voided after the fact. Related modules: `SmartMed.BLL.Services.SalesService`, `SmartMed.DAL.Repositories.SaleRepository`.

**FR-STF-10 — Report Generation and Export.** `ReportsForm`, visible only to Administrators, offers twenty-two distinct report types spanning sales (daily, monthly, by date range, by cashier, payment summary, revenue, monthly trend), purchasing (by supplier, by date, cost analysis), inventory (current stock, low stock, near expiry, expired, stock movements, batch report, category summary), and analytics (top-selling and slow-moving medicines, supplier summary, profit report). Reports are rendered in a data grid and can be exported or printed. `ReportService.ExportToCsv` produces a genuine CSV file through reflection over the result rows; `ExportToExcel`, despite its name, produces a tab-separated text file rather than a native `.xlsx` workbook — this is a real characteristic of the implementation, not a limitation of this description. Printing is handled by `ReportPrintDocument`, a hand-built GDI+ renderer rather than a dedicated reporting engine. Related modules: `SmartMed.BLL.Services.ReportService`, `SmartMed.DAL.Repositories.ReportRepository`, `SmartMed.UI.Forms.ReportsForm`, `SmartMed.UI.Components.ReportPrintDocument`.

**FR-STF-11 — Operational Dashboard.** `DashboardForm`, reachable only by Administrators, presents a set of summary metrics — today's transaction count, revenue, and profit; total medicines; low-stock, expired, and near-expiry counts; month-to-date sales and purchase totals; and active supplier count — sourced from `ReportService.GetDashboardSummary`. These figures are displayed as plain labels arranged in a grid; no charting library is referenced anywhere in the solution, so there are no graphs, trend lines, or visual charts in this or any other screen. Related modules: `SmartMed.BLL.Services.ReportService`, `SmartMed.UI.Forms.DashboardForm`.

**FR-STF-12 — System Settings Management.** `SettingService` provides a database-backed key-value configuration store, distinct from the static `App.config` file, with typed accessors (`GetIntValue`, `GetBoolValue`) and add/update/delete operations. The `Settings` table is seeded with default values covering pharmacy profile, security, notification, and backup categories. No dedicated settings-management form was found in `SmartMed.UI`, so this capability, while fully implemented at the service and data layer, currently has no direct UI entry point. Related modules: `SmartMed.BLL.Services.SettingService`, `SmartMed.DAL.Repositories.SettingRepository`.

**FR-STF-13 — Database Backup and Restore.** `BackupService` supports creating a backup (optionally to a custom directory), restoring from a backup file, listing backup history, deleting old backups, retrieving the most recent backup's metadata, and pruning older backups beyond a configured retention count, all tracked through the `BackupHistory` table. As with settings management, no dedicated UI form for triggering backups was located, so this function is available at the service layer but not currently exposed through the desktop interface. Related modules: `SmartMed.BLL.Services.BackupService`, `SmartMed.DAL.Repositories.BackupHistoryRepository`.

**FR-STF-14 — System Health Check and Performance Monitoring.** `HealthCheckService.RunHealthCheck` produces a `HealthCheckResult`, and `PerformanceMonitorService` allows any operation to be timed (`BeginOperation` returns a disposable timer) and recorded, with `GetSlowOperations` surfacing operations exceeding a configurable duration threshold. These are diagnostic services intended to support operational troubleshooting; like settings and backup, no UI screen was found that surfaces this data to a user. Related modules: `SmartMed.BLL.Services.HealthCheckService`, `PerformanceMonitorService`, `SmartMed.DAL.Repositories.PerformanceLogRepository`.

**A note on Prescriptions.** The staff menu includes "Prescriptions" with sub-items "New Prescription" and "View Prescriptions", visible to Administrators and Pharmacists, and `App.config` defines a `PrescriptionUploadRootPath` setting. However, no `Prescription` entity, repository, or service class exists anywhere in the codebase, and both menu handlers are empty stubs (`(s, e) => { }`). Prescription handling is therefore recorded here as a **planned but unimplemented** capability rather than as a functional requirement, consistent with the evidence found.

---

## 2.2 Functional Requirements – Customer

The customer-facing side of the system is deliberately minimal compared to the staff side, and this section reflects that rather than inflating it to match the depth of Section 2.1.

**FR-CUS-01 — Customer Lookup and Identification.** From the main `LoginForm`, a "Login as Customer?" link opens `CustomerLookupForm`, which allows a customer to identify themselves by phone number or email through `AuthenticationService.LoginCustomer(identifier, pin)`. This path is gated to users whose role is specifically `RoleType.Customer` and is separate from the staff login flow — a customer record cannot authenticate through `LoginAdmin`, and vice versa. Optionally, if the `CustomerPinEnabled` setting in `App.config` is switched on (it is `false` by default in the shipped configuration), the customer must also supply a 4-to-8-digit PIN, which is verified before the session is granted. Related modules: `SmartMed.BLL.Services.AuthenticationService`, `SmartMed.UI.Forms.CustomerLookupForm`.

It is important to state plainly what this feature is not: there is no customer-facing catalogue browsing screen, no self-service ordering or cart, no order history view, and no customer profile management screen anywhere in `SmartMed.UI`. The `Sale` entity does carry an optional `CustomerId` field and `ReportRepository.GetCustomerSales` queries against it, but the table script that creates the `Sales` table (`006_CreateSalesModule.sql`) does not actually define a `CustomerId` column — meaning the model has drifted ahead of the database schema at this point, and customer-linked sales reporting cannot function against the database as currently scripted. This is recorded here as a known implementation gap rather than a working feature.

---

## 2.3 Additional Features Implemented

Beyond the core catalogue-purchase-sale cycle, the implementation includes several supporting features that go beyond what a minimal requirements list would demand, and each earns its place in the system for a concrete operational reason.

**FIFO batch-level inventory valuation.** Rather than tracking a single stock count per medicine, the system tracks discrete batches with their own expiry date and cost, and consumes them oldest-first during a sale (`InventoryService.DeductFIFO`). This gives the pharmacy accurate cost-of-goods figures per sale (used in the profit report) and lets expiry-sensitive stock be identified and cleared before it lapses, which is a meaningful operational concern for a business selling medicines with shelf lives.

**Audit logging.** Every login, logout, and failed login attempt is written to an `AuditLogEntry` record via `IAuditLogRepository`, capturing the username, machine name, timestamp, and outcome. This gives the business a record to investigate suspicious access patterns after the fact, independent of whatever the operating system's own logs might capture.

**Account lockout after repeated failed logins.** Beyond simple password checking, the system automatically locks an account for a configured cool-down period after five consecutive failed attempts, which is a standard mitigation against automated password-guessing without requiring any additional infrastructure such as a CAPTCHA or rate-limiting proxy.

**CSV and delimited-text export of reports.** Any of the twenty-two report types can be exported for use outside the application — genuinely as CSV for `ExportToCsv`, and as tab-separated text (despite being labelled "Excel export") for `ExportToExcel`. This allows report data to be opened in a spreadsheet application or fed into other tools without requiring direct database access.

**Printed documents via GDI+.** Both sales invoices (`SalePrintDocument`) and generic tabular reports (`ReportPrintDocument`) are rendered directly using `System.Drawing.Printing.PrintDocument` and manual `Graphics.DrawString`/`DrawLine` calls, rather than through a third-party reporting or PDF library. This keeps the solution free of external reporting dependencies at the cost of a more limited visual layout than a dedicated report designer would offer.

**Database-backed dynamic settings.** The `Settings` table, distinct from the static `App.config` file, lets values such as pharmacy profile information and notification preferences be changed without redeploying the application or editing a configuration file on disk — though, as noted in FR-STF-12, no UI currently exposes this to an end user.

**Backup, health check, and performance monitoring services.** `BackupService`, `HealthCheckService`, and `PerformanceMonitorService` provide the operational tooling one would expect around a production line-of-business system — the ability to back up and restore the database, verify the application's own health at startup (`StartupDiagnosticsService`), and record which operations are running slowly for later investigation. These are present and functional at the service layer even though, as noted above, they are not yet wired into a visible screen.

**Centralised error logging.** `ErrorLogger`, implementing `ILogger`, persists application errors, warnings, and informational messages to an `ErrorLog` table, and is deliberately written so that a failure while logging cannot itself crash the calling operation (its own exception handling is silently absorbed). This is a defensive design choice worth noting because it reflects an explicit engineering decision rather than an oversight.

---

## 2.4 Non-Functional Requirements

| Category | Requirement | Measurable Criterion (as configured/implemented) |
|---|---|---|
| Security | Passwords must be stored using a computationally expensive, salted hash rather than in plain text or with a fast hash. | PBKDF2-HMAC-SHA256 via `Rfc2898DeriveBytes`, 600,000 iterations (`HashIterations` in `App.config`), 16-byte random salt, 32-byte derived key, constant-time comparison in `PasswordHasher`. |
| Security | Repeated failed login attempts must trigger a temporary account lockout. | Lockout after 5 consecutive failures (`MaxFailedLoginAttempts`), lasting 15 minutes (`LockoutDurationMinutes`), enforced in `AuthenticationService.LoginAdmin`. |
| Security | Idle staff sessions must expire automatically. | 20-minute idle timeout (`SessionTimeoutMinutes`), enforced by `SessionManager`. |
| Security | Database access must not rely on credentials embedded in source or configuration files. | Connection string uses `Integrated Security=True` (Windows authentication) rather than a SQL login and password. |
| Reliability | Multi-step operations that touch stock, sales, or purchase data must not leave the database in a partially updated state if an error occurs mid-operation. | `SalesService.CreateSale`/`CancelSale`, `PurchaseService.ConfirmPurchase`, and `InventoryService.SyncMedicineStock` all wrap their steps in an explicit ADO.NET transaction (`SqlTransaction` or `SqlUnitOfWork`) with commit/rollback in a try/catch/finally block. |
| Reliability | Sale numbers must remain unique even if two sales are being finalised concurrently. | `SaleNumberGenerator` uses a `SERIALIZABLE`-isolation transaction with an `UPDLOCK` hint when generating the next sequential number. |
| Data Integrity | Referential relationships between core tables must be enforced by the database itself, not solely by application logic. | Foreign key and CHECK constraints are defined directly in `005_CreatePurchaseModule.sql` and `006_CreateSalesModule.sql` (e.g., `CK_StockMovements_MovementType`, cascading deletes from `Purchases` to `PurchaseItems`). |
| Performance | The system must be able to flag operations that are running unacceptably slowly. | `PerformanceMonitorService.GetSlowOperations` defaults to a 1,000-millisecond threshold, configurable per call. |
| Maintainability | Business-layer methods must communicate success or failure in a consistent, predictable shape rather than relying on unhandled exceptions reaching the UI. | Nearly every `SmartMed.BLL` service method returns `OperationResult` or `OperationResult<T>` rather than throwing outward; the custom exception hierarchy (`AppException`, `ValidationException`, `DataAccessException`, `AuthenticationException`, `ConfigurationException`) is caught and translated inside the services. |
| Maintainability | Input validation must be applied consistently at the entry point of business operations. | The `Guard` helper class (`AgainstNull`, `AgainstNullOrWhiteSpace`, `AgainstNegative`, `AgainstZeroOrNegative`) is used across BLL and DAL constructors and methods. |
| Usability | Inventory items nearing exhaustion or expiry must be visible to staff without requiring a separate report to be run. | `MedicineForm` surfaces low-stock and near-expiry indicators directly, using `DefaultLowStockThreshold` (10 units) and `NearExpiryThresholdDays` (30 days) from `App.config`. |
| Availability/Scalability | The system is architected as a single-user desktop application; it is not designed to serve multiple concurrent staff sessions from one running instance. | `SessionManager` holds exactly one `SessionContext` per process. This is stated here as an architectural constraint rather than a defect — any requirement for multi-terminal concurrent staff use would require a different session-management design than the one currently implemented. |
| Backup and Recovery | The system must provide a means of creating and restoring database backups without relying solely on external DBA tooling. | `BackupService.CreateBackup`/`RestoreDatabase`, with history tracked in the `BackupHistory` table and configurable retention via `CleanupOldBackups`. |
| Compatibility | The application must run on the Windows desktop environment for which it was built. | `SmartMed.UI` targets `net48` with `UseWindowsForms=true`; it has no web or cross-platform UI layer. |
| Supportability | Runtime errors must be captured centrally to support after-the-fact diagnosis. | `ErrorLogger` persists structured error records (message, exception, source, stack trace, machine name) to the `ErrorLogs` table. |
| Portability | The connection to the underlying database must be configurable without recompiling the application. | The `SmartMedDb` connection string is read from `App.config` via `ConfigurationManager`, not hard-coded in source. |

---

## 2.5 Technical Requirements

| Category | Detail |
|---|---|
| Programming language | C# (all seven projects) |
| Target framework | .NET Framework 4.8 (`net48`), using the modern SDK-style project format for every `.csproj` |
| UI framework | Windows Forms (`SmartMed.UI`, `OutputType=WinExe`, `UseWindowsForms=true`) — not WPF, not ASP.NET |
| Architecture | Layered architecture: UI → Business Logic Layer (`SmartMed.BLL`) → Data Access Layer (`SmartMed.DAL`) → SQL Server, with a shared `SmartMed.Models` project and a `SmartMed.Common` cross-cutting utilities project |
| Dependency composition | Manual dependency wiring in `SmartMed.UI.Bootstrap.ApplicationBootstrapper` (constructs repositories and services directly); no IoC/DI container framework (e.g., Autofac, Unity, Microsoft.Extensions.DependencyInjection) is used |
| Database engine | Microsoft SQL Server, accessed via classic ADO.NET (`System.Data.SqlClient`) — no ORM (no Entity Framework, no Dapper), no stored procedures, views, or triggers; all SQL is hand-written and parameterised |
| Database provisioning | Idempotent DDL scripts under `SmartMed.DAL/Scripts/` (`Module3_DDL.sql`, `Module4_DDL.sql`, `005_CreatePurchaseModule.sql`, `006_CreateSalesModule.sql`, `008_CreateSettingsModule.sql`); no migration framework or migration history table is used |
| Authentication mechanism | Custom-built, PBKDF2-SHA256 password hashing with configurable iteration count; no external identity provider or OAuth integration |
| Reporting/printing | Custom-built: raw SQL aggregation queries in `ReportRepository`, reflection-based CSV/tab-separated export in `ReportService`, and `System.Drawing.Printing.PrintDocument`-based rendering in `SmartMed.UI.Components`; the `SmartMed.Reports` project itself contains no functional code — only an empty placeholder class (`ReportModuleMarker`) |
| NuGet dependencies | `Microsoft.NETFramework.ReferenceAssemblies.net48` (build-time reference assemblies, all projects); `System.Configuration.ConfigurationManager` 8.0.0 (`SmartMed.Common`, for reading `App.config`); test-only: `Microsoft.NET.Test.Sdk` 17.11.1, `MSTest.TestAdapter` 3.6.4, `MSTest.TestFramework` 3.6.4 (`SmartMed.Tests`). No UI component library, charting library, ORM, logging framework, or mapping library is referenced anywhere in the solution |
| Testing framework | MSTest, covering `SmartMed.BLL`, `SmartMed.DAL`, `SmartMed.Models`, `SmartMed.Common`, and selected `SmartMed.UI` forms |
| Operating system / hardware | Windows desktop OS required (WinForms/.NET Framework 4.8 dependency); no specific hardware requirements are declared in the solution beyond what a standard SQL Server client and desktop application need |
| Version control | Git, hosted with a single local repository (`SmartMed.sln` plus the seven project folders). No `.gitignore` file exists in the repository, which is why compiled `bin`/`obj` output and NuGet cache files are currently tracked in version control — this is recorded here as an observed characteristic of the current repository state rather than a recommended practice |
| Build/deployment | Standard MSBuild/.NET Framework build via the `.sln` file; no CI/CD pipeline configuration, installer project, or deployment script was found in the repository |

---

## 2.6 Requirement Traceability Matrix

| Req ID | Description | Related Module | Implemented Component | Verification Method | Status |
|---|---|---|---|---|---|
| FR-STF-01 | Staff authentication with account lockout | Authentication | `AuthenticationService.LoginAdmin`, `PasswordHasher` | `AuthenticationServiceTests.cs`, `PasswordHasherTests.cs` | Implemented |
| FR-STF-02 | Session management and idle timeout | Authentication | `SessionManager` | `SessionManagerTests.cs` | Implemented |
| FR-STF-03 | Medicine category CRUD | Catalogue | `MedicineCategoryService`, `MedicineCategoryForm` | `MedicineCategoryServiceTests.cs` | Implemented |
| FR-STF-04 | Medicine/inventory item CRUD | Catalogue | `MedicineService`, `MedicineForm` | `MedicineServiceTests.cs`, `MedicineFormTests.cs` | Implemented (form has no menu entry point) |
| FR-STF-05 | Supplier CRUD | Procurement | `SupplierService`, `SupplierForm` | `SupplierServiceTests.cs`, `SupplierFormTests.cs` | Implemented (form has no menu entry point) |
| FR-STF-06 | Purchase order creation and confirmation | Procurement | `PurchaseService`, `PurchaseForm` | `PurchaseServiceTests.cs`, `PurchaseWorkflowTests.cs` | Implemented |
| FR-STF-07 | FIFO batch stock tracking and sync | Inventory | `InventoryService` | `InventoryServiceTests.cs` | Implemented |
| FR-STF-08 | POS sale processing | Sales | `SalesService.CreateSale`, `SalesForm` | `SalesFormTests.cs`, `SaleModelTests.cs`, `SaleRepositoryTests.cs` | Implemented |
| FR-STF-09 | Sale cancellation with stock reversal | Sales | `SalesService.CancelSale` | Covered indirectly via `SalesFormTests.cs`; no dedicated service-level test file | Implemented |
| FR-STF-10 | Report generation and CSV/text export | Reporting | `ReportService`, `ReportRepository`, `ReportsForm` | `ReportServiceTests.cs`, `ReportRepositoryTests.cs` | Implemented |
| FR-STF-11 | Operational dashboard | Reporting | `ReportService.GetDashboardSummary`, `DashboardForm` | `DashboardFormTests.cs` | Implemented (label-based, no charts) |
| FR-STF-12 | System settings management | Administration | `SettingService` | Not covered by a dedicated automated test file | Implemented (service layer only, no UI) |
| FR-STF-13 | Database backup and restore | Administration | `BackupService` | Not covered by a dedicated automated test file | Implemented (service layer only, no UI) |
| FR-STF-14 | Health check and performance monitoring | Administration | `HealthCheckService`, `PerformanceMonitorService` | Not covered by a dedicated automated test file | Implemented (service layer only, no UI) |
| — | Prescription creation and viewing | Prescriptions | Menu items in `MainShellForm` only | Not applicable | Planned – not implemented |
| — | Admin User Management screen | Administration | Menu item in `MainShellForm` only | Not applicable | Planned – not implemented |
| — | Admin Audit Log viewer screen | Administration | Menu item in `MainShellForm` only | Not applicable | Planned – not implemented |
| FR-CUS-01 | Customer lookup and identification login | Customer Access | `AuthenticationService.LoginCustomer`, `CustomerLookupForm` | `CustomerLookupFormTests.cs` | Implemented |
| — | Customer-linked sales reporting | Customer Access | `ReportRepository.GetCustomerSales` vs. `Sale.CustomerId` | `ReportRepositoryTests.cs` (query only; underlying column absent from schema script) | Partially implemented – schema drift |
| NFR-Security-1 | PBKDF2 password hashing, 600,000 iterations | Security | `PasswordHasher` | `PasswordHasherTests.cs` | Implemented |
| NFR-Security-2 | Account lockout after 5 failed attempts | Security | `AuthenticationService.LoginAdmin` | `AuthenticationServiceTests.cs` | Implemented |
| NFR-Reliability-1 | Transactional integrity for sales/purchases/stock sync | Reliability | `SqlUnitOfWork`, manual `SqlTransaction` usage in `SalesService`/`PurchaseService`/`InventoryService` | `SqlUnitOfWorkTests.cs`, `PurchaseWorkflowTests.cs` | Implemented |
| NFR-DataIntegrity-1 | Foreign key/CHECK constraint enforcement | Data Integrity | DDL scripts under `SmartMed.DAL/Scripts/` | Verified by inspection of DDL scripts; no automated schema test | Implemented |
| NFR-Supportability-1 | Centralised error logging | Supportability | `ErrorLogger`, `ErrorLog` table | Not covered by a dedicated automated test file | Implemented |

---

## Summary of Identified Gaps

For clarity, the following items were found in the codebase in a specified-but-not-delivered state and are flagged here as a single consolidated list, rather than being scattered only through the narrative above:

1. **Prescriptions module** — menu entries exist; no backing entity, repository, service, or form exists.
2. **Administrator "User Management" and "Audit Log" screens** — menu entries exist as empty click handlers; the underlying `UserService` and `IAuditLogRepository` are functional, but no form consumes them.
3. **`SmartMed.Reports` project** — present in the solution but contains only an empty placeholder class; all real reporting logic lives in `SmartMed.BLL.Services.ReportService` and `SmartMed.UI.Components`.
4. **`Users` and `AuditLogs` tables** — their structure is documented in `MODULE2_EXECUTION_SPEC.md` but, unlike the other core tables, no corresponding runnable `.sql` script exists under `SmartMed.DAL/Scripts/`.
5. **`Sale.CustomerId`** — present on the `Sale` model and queried by `ReportRepository.GetCustomerSales`, but absent from the `Sales` table as defined in `006_CreateSalesModule.sql`.
6. **`MedicineForm` and `SupplierForm`** — both fully functional but unreachable from any menu item in the current build of `MainShellForm`.
7. **Settings, Backup, and Health/Performance monitoring** — all implemented at the service and data layer, but none currently has a corresponding UI screen for staff to use directly.

These gaps are not defects in the sense of incorrect behaviour; rather, they represent boundaries of the current build that should inform any subsequent development or testing effort based on this system.
