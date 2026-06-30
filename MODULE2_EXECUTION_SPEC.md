# Module 2: Authentication — Execution Specification

## Summary

Implement a complete authentication subsystem for the SmartMed Pharmacy Management System. The module delivers secure staff login with PBKDF2 password hashing, account lockout on repeated failures, persistent audit logging of all authentication events, a customer lookup/login workflow, role-based session management, and logout with session teardown. All code follows the established Module 1 patterns: interface-based programming, `OperationResult` return types, custom exceptions layered on `AppException`, constructor injection, WinForms presentation, and MSTest unit tests.

---

## Implementation Tasks

| Task ID | Task | Layer | Effort |
|---|---|---|---|
| T-01 | Create `AuditAction` enum in SmartMed.Models.Enums | Models | Small |
| T-02 | Create `User` entity with `PasswordSalt`, `FailedLoginAttempts`, `LockedUntil`, `LastLogin` in SmartMed.Models.Entities | Models | Medium |
| T-03 | Create `AuditLogEntry` entity in SmartMed.Models.Entities | Models | Small |
| T-04 | Create `AuthenticationException` in SmartMed.Common.Exceptions | Common | Small |
| T-05 | Add config keys to `ConfigKeys` in SmartMed.Common.Constants | Common | Small |
| T-06 | Add new app settings to `SmartMed.UI/App.config` and `SmartMed.Tests/App.config` | Config | Small |
| T-07 | Create `IPasswordHasher` interface in SmartMed.BLL.Interfaces | BLL | Small |
| T-08 | Create `PasswordHasher` service in SmartMed.BLL.Services | BLL | Medium |
| T-09 | Create `IUserRepository` interface in SmartMed.DAL.Interfaces | DAL | Medium |
| T-10 | Create `UserRepository` in SmartMed.DAL.Repositories | DAL | Large |
| T-11 | Create `IAuditLogRepository` interface in SmartMed.DAL.Interfaces | DAL | Small |
| T-12 | Create `AuditLogRepository` in SmartMed.DAL.Repositories | DAL | Medium |
| T-13 | Create `ISessionManager` interface in SmartMed.BLL.Interfaces | BLL | Small |
| T-14 | Create `SessionManager` service in SmartMed.BLL.Services | BLL | Medium |
| T-15 | Create `IAuthenticationService` interface in SmartMed.BLL.Interfaces | BLL | Small |
| T-16 | Create `AuthenticationService` in SmartMed.BLL.Services | BLL | Large |
| T-17 | Create `LoginForm` in SmartMed.UI.Forms | UI | Large |
| T-18 | Create `CustomerLookupForm` in SmartMed.UI.Forms | UI | Medium |
| T-19 | Update `ApplicationBootstrapper` to register all new services and compose login flow | UI | Medium |
| T-20 | Update `MainShellForm` to accept `ISessionManager`, add role-based menus, logout, status bar | UI | Large |
| T-21 | Update `Program.cs` to handle `AuthenticationException` | UI | Small |
| T-22 | Update `ExceptionTests` in SmartMed.Tests.Common | Tests | Small |
| T-23 | Create `PasswordHasherTests` in SmartMed.Tests.BLL | Tests | Medium |
| T-24 | Create `AuthenticationServiceTests` in SmartMed.Tests.BLL | Tests | Large |
| T-25 | Create `SessionManagerTests` in SmartMed.Tests.BLL | Tests | Medium |
| T-26 | Create `UserRepositoryTests` in SmartMed.Tests.DAL | Tests | Medium |
| T-27 | Create `AuditLogRepositoryTests` in SmartMed.Tests.DAL | Tests | Medium |
| T-28 | Create `UserModelTests` in SmartMed.Tests.Models | Tests | Small |
| T-29 | Create `AuditLogEntryModelTests` in SmartMed.Tests.Models | Tests | Small |
| T-30 | Create `LoginFormTests` in SmartMed.Tests.UI | Tests | Medium |
| T-31 | Create `CustomerLookupFormTests` in SmartMed.Tests.UI | Tests | Medium |

---

## Files to Create

### SmartMed.Models

| # | File Path | Description |
|---|---|---|
| 1 | `SmartMed.Models/Enums/AuditAction.cs` | Enum: Login=1, Logout=2, FailedLogin=3 |
| 2 | `SmartMed.Models/Entities/User.cs` | User entity extending BaseEntity (see Classes section) |
| 3 | `SmartMed.Models/Entities/AuditLogEntry.cs` | Audit log entry POCO (see Classes section) |

### SmartMed.Common

| # | File Path | Description |
|---|---|---|
| 4 | `SmartMed.Common/Exceptions/AuthenticationException.cs` | Custom exception extending `AppException` |

### SmartMed.DAL

| # | File Path | Description |
|---|---|---|
| 5 | `SmartMed.DAL/Interfaces/IUserRepository.cs` | User data access contract |
| 6 | `SmartMed.DAL/Interfaces/IAuditLogRepository.cs` | Audit log data access contract |
| 7 | `SmartMed.DAL/Repositories/UserRepository.cs` | ADO.NET implementation of IUserRepository |
| 8 | `SmartMed.DAL/Repositories/AuditLogRepository.cs` | ADO.NET implementation of IAuditLogRepository |

### SmartMed.BLL

| # | File Path | Description |
|---|---|---|
| 9 | `SmartMed.BLL/Interfaces/IPasswordHasher.cs` | Password hashing contract |
| 10 | `SmartMed.BLL/Interfaces/ISessionManager.cs` | Session management contract |
| 11 | `SmartMed.BLL/Interfaces/IAuthenticationService.cs` | Authentication orchestration contract |
| 12 | `SmartMed.BLL/Services/PasswordHasher.cs` | PBKDF2 password hasher |
| 13 | `SmartMed.BLL/Services/SessionManager.cs` | Singleton session holder with timeout |
| 14 | `SmartMed.BLL/Services/AuthenticationService.cs` | Login/logout orchestration with lockout and audit |

### SmartMed.UI

| # | File Path | Description |
|---|---|---|
| 15 | `SmartMed.UI/Forms/LoginForm.cs` | Staff login form |
| 16 | `SmartMed.UI/Forms/CustomerLookupForm.cs` | Customer lookup/login form |

### SmartMed.Tests

| # | File Path | Description |
|---|---|---|
| 17 | `SmartMed.Tests/BLL/PasswordHasherTests.cs` | Unit tests for PasswordHasher |
| 18 | `SmartMed.Tests/BLL/AuthenticationServiceTests.cs` | Unit tests for AuthenticationService |
| 19 | `SmartMed.Tests/BLL/SessionManagerTests.cs` | Unit tests for SessionManager |
| 20 | `SmartMed.Tests/DAL/UserRepositoryTests.cs` | Unit tests for UserRepository |
| 21 | `SmartMed.Tests/DAL/AuditLogRepositoryTests.cs` | Unit tests for AuditLogRepository |
| 22 | `SmartMed.Tests/Models/UserModelTests.cs` | Unit tests for User entity |
| 23 | `SmartMed.Tests/Models/AuditLogEntryModelTests.cs` | Unit tests for AuditLogEntry entity |
| 24 | `SmartMed.Tests/UI/LoginFormTests.cs` | Unit tests for LoginForm |
| 25 | `SmartMed.Tests/UI/CustomerLookupFormTests.cs` | Unit tests for CustomerLookupForm |

---

## Files to Modify

| # | File Path | Change Description |
|---|---|---|
| M-01 | `SmartMed.Common/Constants/ConfigKeys.cs` | Add: `HashIterations`, `MaxFailedLoginAttempts`, `LockoutDurationMinutes`, `CustomerPinEnabled` |
| M-02 | `SmartMed.UI/App.config` | Add 4 new appSetting keys with default values |
| M-03 | `SmartMed.Tests/App.config` | Add 4 new appSetting keys with test values |
| M-04 | `SmartMed.UI/Bootstrap/ApplicationBootstrapper.cs` | Register `IPasswordHasher`, `IUserRepository`, `IAuditLogRepository`, `ISessionManager`, `IAuthenticationService`; compose login flow before MainShellForm |
| M-05 | `SmartMed.UI/Forms/MainShellForm.cs` | Accept `ISessionManager` in constructor; add MenuStrip with File > Logout/Exit; enable/disable menu items by RoleType; add StatusStrip with user/role label; wire Logout click handler |
| M-06 | `SmartMed.UI/Program.cs` | Add catch for `AuthenticationException` in Main alongside existing `AppException` catch |
| M-07 | `SmartMed.Tests/Common/ExceptionTests.cs` | Add test: `AuthenticationException_ShouldInheritFromAppException` |

---

## Public Interfaces

### `IPasswordHasher` (SmartMed.BLL.Interfaces)

```csharp
namespace SmartMed.BLL.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password, string salt);
        bool VerifyPassword(string password, string hash, string salt);
        string GenerateSalt();
    }
}
```

### `IUserRepository` (SmartMed.DAL.Interfaces)

```csharp
namespace SmartMed.DAL.Interfaces
{
    public interface IUserRepository : IRepository
    {
        User GetById(int userId);
        User GetByUsername(string username);
        void IncrementFailedAttempts(int userId);
        void ResetFailedAttempts(int userId);
        void SetLockedUntil(int userId, DateTime? lockedUntil);
        void UpdateLastLogin(int userId, DateTime loginTime);
    }
}
```

### `IAuditLogRepository` (SmartMed.DAL.Interfaces)

```csharp
namespace SmartMed.DAL.Interfaces
{
    public interface IAuditLogRepository
    {
        void LogLogin(int userId, string username, string machineName);
        void LogLogout(int? userId, string username, string machineName);
        void LogFailedAttempt(string username, string machineName, string details);
    }
}
```

### `ISessionManager` (SmartMed.BLL.Interfaces)

```csharp
namespace SmartMed.BLL.Interfaces
{
    public interface ISessionManager
    {
        SessionContext StartSession(User user);
        void EndSession();
        SessionContext CurrentSession { get; }
        bool IsActive { get; }
        bool HasRole(RoleType role);
    }
}
```

### `IAuthenticationService` (SmartMed.BLL.Interfaces)

```csharp
namespace SmartMed.BLL.Interfaces
{
    public interface IAuthenticationService
    {
        OperationResult<SessionContext> LoginAdmin(string username, string password);
        OperationResult<SessionContext> LoginCustomer(string identifier, string? pin = null);
        OperationResult Logout();
        bool IsAuthenticated { get; }
        SessionContext CurrentSession { get; }
    }
}
```

---

## Classes

### `User` (SmartMed.Models.Entities)

- **Inherits:** `BaseEntity`
- **Properties:**
  - `string Username` — unique, 2–50 characters
  - `string PasswordHash` — PBKDF2-derived hash (base64)
  - `string PasswordSalt` — per-user random salt (base64)
  - `string DisplayName` — friendly name for UI
  - `RoleType Role` — Administrator / Pharmacist / Cashier / Customer
  - `string? Email` — optional
  - `int FailedLoginAttempts` — default 0
  - `DateTime? LockedUntil` — null when not locked
  - `DateTime? LastLogin` — null until first login

### `AuditLogEntry` (SmartMed.Models.Entities)

- **Properties:**
  - `int Id` — primary key
  - `int? UserId` — null for failed attempts where user not found
  - `string Username` — always populated
  - `AuditAction Action` — Login / Logout / FailedLogin
  - `string MachineName` — `Environment.MachineName`
  - `DateTime Timestamp` — `DateTime.UtcNow`
  - `string? Details` — optional context (e.g., "Max attempts reached")

### `AuditAction` (SmartMed.Models.Enums)

```csharp
public enum AuditAction
{
    Login = 1,
    Logout = 2,
    FailedLogin = 3
}
```

### `AuthenticationException` (SmartMed.Common.Exceptions)

- **Inherits:** `AppException`
- **Constructors:** `(string message)`, `(string message, Exception innerException)`
- **Attribute:** `[Serializable]`

### `PasswordHasher` (SmartMed.BLL.Services)

- **Implements:** `IPasswordHasher`
- **Fields:**
  - `int _iterations` — read from `AppSettings.GetRequiredInt(ConfigKeys.HashIterations)`
- **Behavior:**
  - `GenerateSalt()`: uses `RandomNumberGenerator.Create()` to produce 16 bytes, returns base64
  - `HashPassword(password, salt)`: decodes salt from base64, runs PBKDF2 with `_iterations`, returns base64 hash (32 bytes output)
  - `VerifyPassword(password, hash, salt)`: re-hashes with same salt, constant-time comparison of byte sequences

### `UserRepository` (SmartMed.DAL.Repositories)

- **Implements:** `IUserRepository`
- **Constructor:** `UserRepository(IDbConnectionFactory connectionFactory)` — Guard against null
- **Queries:**
  - `GetById`: `SELECT * FROM Users WHERE Id = @Id`
  - `GetByUsername`: `SELECT * FROM Users WHERE Username = @Username`
  - `IncrementFailedAttempts`: `UPDATE Users SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @Id`
  - `ResetFailedAttempts`: `UPDATE Users SET FailedLoginAttempts = 0, LockedUntil = NULL WHERE Id = @Id`
  - `SetLockedUntil`: `UPDATE Users SET LockedUntil = @LockedUntil WHERE Id = @Id`
  - `UpdateLastLogin`: `UPDATE Users SET LastLogin = @LastLogin WHERE Id = @Id`
- **Pattern:** Guard parameters; open connection via `_connectionFactory.CreateConnection()`; wrap in try/catch → `DataAccessException`

### `AuditLogRepository` (SmartMed.DAL.Repositories)

- **Implements:** `IAuditLogRepository`
- **Constructor:** `AuditLogRepository(IDbConnectionFactory connectionFactory)` — Guard against null
- **Queries:**
  - `LogLogin`: `INSERT INTO AuditLogs (UserId, Username, Action, MachineName, Timestamp, Details) VALUES (@UserId, @Username, 1, @MachineName, @Timestamp, NULL)`
  - `LogLogout`: same with Action = 2
  - `LogFailedAttempt`: same with Action = 3, UserId = NULL, Details populated
- **Pattern:** Same as UserRepository

### `SessionManager` (SmartMed.BLL.Services)

- **Implements:** `ISessionManager`
- **Fields:**
  - `SessionContext? _currentSession` — null initially
  - `object _lock` — for thread safety
  - `int _sessionTimeoutMinutes` — from config
- **Behavior:**
  - `StartSession(User)`: lock, create `SessionContext` from user, set UTC timestamps
  - `EndSession()`: lock, set `_currentSession = null`
  - `CurrentSession` getter: check timeout on access (auto-expire if exceeded)
  - `IsActive`: delegated to `CurrentSession != null`
  - `HasRole(RoleType)`: checks IsActive && `CurrentSession.Role == role`

### `AuthenticationService` (SmartMed.BLL.Services)

- **Implements:** `IAuthenticationService`
- **Constructor:** `AuthenticationService(IUserRepository, IPasswordHasher, ISessionManager, IAuditLogRepository)` — Guard all
- **Fields:**
  - `int _maxFailedAttempts` — from config
  - `int _lockoutDurationMinutes` — from config
- **Behavior:**
  - `LoginAdmin(username, password)`:
    1. Guard parameters
    2. `UserRepository.GetByUsername(username)`
    3. If null → `AuditLogRepository.LogFailedAttempt(...)` → return `Failure("Invalid username or password.")`
    4. If `!user.IsActive` → `AuditLogRepository.LogFailedAttempt(...)` → return `Failure("This account has been disabled. Please contact your system administrator.")`
    5. If `user.LockedUntil > UtcNow` → return `Failure($"Account is temporarily locked. Please try again after {remainingMinutes} minutes.")`
    6. If `user.LockedUntil <= UtcNow` → `UserRepository.SetLockedUntil(user.Id, null)` (auto-unlock)
    7. `PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt)`
    8. If false → `UserRepository.IncrementFailedAttempts(user.Id)`; if `user.FailedLoginAttempts + 1 >= _maxFailedAttempts` → `UserRepository.SetLockedUntil(user.Id, UtcNow + _lockoutDurationMinutes)`; `AuditLogRepository.LogFailedAttempt(...)` → return `Failure("Invalid username or password.")`
    9. If true → `UserRepository.ResetFailedAttempts(user.Id)`; `UserRepository.UpdateLastLogin(user.Id, UtcNow)`; `AuditLogRepository.LogLogin(...)`; `SessionManager.StartSession(user)`; return `Success(sessionContext)`
  - `LoginCustomer(identifier, pin?)`:
    1. Lookup user by telephone/email (via UserRepository or a dedicated query — assumes identifier maps to User.Username or User.Email)
    2. If `CustomerPinEnabled` → validate PIN (stored in PasswordHash for customer role users)
    3. If not found → `AuditLogRepository.LogFailedAttempt(...)` → return `Failure("Customer not found.")`
    4. If found → `AuditLogRepository.LogLogin(...)` → `SessionManager.StartSession(user)` → return `Success(sessionContext)`
  - `Logout()`:
    1. If `IsAuthenticated` → `AuditLogRepository.LogLogout(session.UserId, session.Username, machineName)`
    2. `SessionManager.EndSession()`
    3. Return `Success("Logged out successfully.")`

### `LoginForm` (SmartMed.UI.Forms)

- **Inherits:** `Form`
- **Properties:** `IAuthenticationService AuthService` (injected)
- **Controls:** `TextBox txtUsername`, `TextBox txtPassword` (UseSystemPasswordChar=true), `Button btnLogin`, `Button btnCancel`, `LinkLabel linkCustomerLookup`, `Label lblStatus`
- **Behavior:**
  - Constructor: accept `IAuthenticationService`, initialize components
  - `btnLogin_Click`: call `AuthService.LoginAdmin(txtUsername.Text, txtPassword.Text)`; on success → `DialogResult = OK`; on failure → show message in `lblStatus` (red)
  - Lockout cooling: maintain `_failedAttemptCount`; after 3 consecutive failures, disable form for 30 seconds with countdown
  - `linkCustomerLookup_LinkClicked`: hide LoginForm, show CustomerLookupForm modally, re-show LoginForm on return
  - `btnCancel_Click`: `Application.Exit()`

### `CustomerLookupForm` (SmartMed.UI.Forms)

- **Inherits:** `Form`
- **Properties:** `IAuthenticationService AuthService` (injected)
- **Controls:** `TextBox txtPhoneOrEmail`, `TextBox txtPin` (visible only if `CustomerPinEnabled`), `Button btnLookup`, `Button btnCancel`, `Label lblStatus`
- **Behavior:**
  - Constructor: accept `IAuthenticationService`, initialize components, hide PIN field if `CustomerPinEnabled == false`
  - `btnLookup_Click`: call `AuthService.LoginCustomer(txtPhoneOrEmail.Text, txtPin.Text)`;
  - on success → `DialogResult = OK`;
  - on failure → show message in `lblStatus` (red)
  - `btnCancel_Click` or Escape key → `DialogResult = Cancel` (returns to staff login)

### `ApplicationBootstrapper` (SmartMed.UI.Bootstrap) — Modified

- **New private methods:**
  - `RegisterServices()`: instantiate `IPasswordHasher`, `IUserRepository`, `IAuditLogRepository`, `ISessionManager`, `IAuthenticationService`; store in fields
  - `ShowLoginFlow()`: create `LoginForm`, show dialog; if `DialogResult != OK`, exit; else proceed
  - `ShowCustomerLookupFlow()`: create `CustomerLookupForm`, show dialog; if OK, use returned session; if Cancel, return to login
- **Modified** `BuildMainForm()`: call `RegisterServices()` then `ShowLoginFlow()` before constructing `MainShellForm`; pass `ISessionManager` to `MainShellForm`

### `MainShellForm` (SmartMed.UI.Forms) — Modified

- **Constructor:** accept `ISessionManager sessionManager` in addition to `ApplicationStartupContext`
- **New fields:** `ISessionManager _sessionManager`
- **New MenuStrip items:**
  - 文件 (File): 注销 (Logout), 退出 (Exit)
  - 管理员 (Administration) — visible only for `RoleType.Administrator`
  - 处方 (Prescriptions) — visible for `Administrator` and `Pharmacist`
  - 销售 (Sales) — visible for `Cashier` and `Administrator`
  - 报表 (Reports) — visible for `Administrator`
- **New StatusStrip:** label showing logged-in user display name and role
- **Logout handler:** call `IAuthenticationService.Logout()` (resolved via ApplicationBootstrapper stored reference), close form, re-display login

---

## Database Changes

### New Table: `Users`

```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    PasswordSalt NVARCHAR(512) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    Role INT NOT NULL,
    Email NVARCHAR(256) NULL,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    LockedUntil DATETIME2 NULL,
    LastLogin DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);
```

### New Table: `AuditLogs`

```sql
CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Username NVARCHAR(50) NOT NULL,
    Action INT NOT NULL,
    MachineName NVARCHAR(100) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Details NVARCHAR(500) NULL
);
```

### Seed Data

```sql
-- Password: Admin@123 (hashed and salted at seed time by PasswordHasher)
INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, IsActive)
VALUES ('admin', '<runtime-generated-hash>', '<runtime-generated-salt>', 'System Administrator', 1, 1);

-- Password: Pharm@123
INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, IsActive)
VALUES ('pharmacist', '<runtime-generated-hash>', '<runtime-generated-salt>', 'Default Pharmacist', 2, 1);

-- Password: Cashier@123
INSERT INTO Users (Username, PasswordHash, PasswordSalt, DisplayName, Role, IsActive)
VALUES ('cashier', '<runtime-generated-hash>', '<runtime-generated-salt>', 'Default Cashier', 3, 1);
```

The seed script is a standalone console utility or SQL script that uses `PasswordHasher` to generate the hash and salt values at runtime. It runs once during initial deployment.

---

## UI Changes

### New Forms

| Form | Purpose | Trigger |
|---|---|---|
| `LoginForm` | Staff username/password entry | Application startup |
| `CustomerLookupForm` | Customer phone/email + optional PIN lookup | Link from LoginForm |

### LoginForm Layout

```
┌──────────────────────────────────────┐
│         SmartMed 登录                │
│                                      │
│  用户名:  [txtUsername          ]    │
│  密码:    [txtPassword          ]    │
│                                      │
│   [lblStatus — red on failure]       │
│                                      │
│      [btnLogin]   [btnCancel]        │
│                                      │
│    ──────────────────────────────    │
│  以客户身份登录? [linkCustomerLookup] │
└──────────────────────────────────────┘
```

### CustomerLookupForm Layout

```
┌──────────────────────────────────────┐
│      SmartMed 客户查找              │
│                                      │
│  电话或电子邮件: [txtPhoneOrEmail]    │
│  PIN (可选):     [txtPin          ]  │
│                                      │
│   [lblStatus — red on failure]       │
│                                      │
│       [btnLookup]   [btnCancel]      │
└──────────────────────────────────────┘
```

### MainShellForm Changes

- **New MenuStrip structure:**
  - 文件 → 注销, 退出
  - 管理 (visible: Administrator only)
  - 处方 (visible: Administrator, Pharmacist)
  - 销售 (visible: Administrator, Cashier)
  - 报表 (visible: Administrator)
- **New StatusStrip:** label `tsslUserInfo` showing `"{DisplayName} ({Role})"`
- **FormClosing:** if user logged in, confirm exit with `MessageBox`

### Flow

```
Application.Start
    → Bootstrapper registers all services
    → Bootstrapper creates LoginForm (modal)
        → Staff clicks Login → LoginAdmin called
            → Success → LoginForm closes with OK → proceed
            → Failure → error displayed, retry
        → Staff clicks "Customer Login" → CustomerLookupForm (modal)
            → Customer found → form closes with OK → proceed
            → Not found → error displayed, retry
        → Staff clicks Cancel → Application.Exit()
    → Bootstrapper creates MainShellForm with ISessionManager
    → MainShellForm shown
        → User clicks 文件 > 注销
            → AuthenticationService.Logout() called
            → MainShellForm closes
            → Bootstrapper re-shows LoginForm
```

---

## Validation Rules

### Server-side (BLL Layer — enforced by `AuthenticationService`)

| Method | Parameter | Rule | Violation Response |
|---|---|---|---|
| `LoginAdmin` | `username` | Must not be null, empty, or whitespace | `ValidationException` |
| `LoginAdmin` | `password` | Must not be null, empty, or whitespace | `ValidationException` |
| `LoginCustomer` | `identifier` | Must not be null, empty, or whitespace | `ValidationException` |
| `LoginCustomer` | `pin` | If `CustomerPinEnabled` and not whitespace, must be 4–8 digits | `ValidationException` |

### Client-side (UI Layer — enforced by `LoginForm` / `CustomerLookupForm`)

| Control | Rule | Enforcement |
|---|---|---|
| `txtUsername` | Must not be empty | `btnLogin` disabled until both fields have text |
| `txtPassword` | Must not be empty | `btnLogin` disabled until both fields have text |
| `txtPhoneOrEmail` | Must not be empty | `btnLookup` disabled until field has text |
| Cooling timer | 3 consecutive failures → 30s cooldown | `btnLogin` disabled, countdown displayed in `lblStatus` |

### Entity-level (Model Properties)

| Entity | Property | Rule |
|---|---|---|
| `User` | `Username` | 2–50 characters, alphanumeric + underscore (DB constraint) |
| `User` | `DisplayName` | 1–100 characters |
| `User` | `PasswordHash` | Must not be null or empty |
| `User` | `PasswordSalt` | Must not be null or empty |
| `User` | `Role` | Must be a valid `RoleType` value |
| `User` | `FailedLoginAttempts` | Must be >= 0 |
| `AuditLogEntry` | `Username` | Must not be null or empty |
| `AuditLogEntry` | `Action` | Must be a valid `AuditAction` value |
| `AuditLogEntry` | `MachineName` | Must not be null or empty |

---

## Exception Handling

| Scenario | Exception Thrown | Where Caught | User Feedback |
|---|---|---|---|
| Username is empty | `ValidationException` (via Guard) | `LoginForm` | "Please enter a username." |
| Password is empty | `ValidationException` (via Guard) | `LoginForm` | "Please enter a password." |
| Username not found in DB | `AuthenticationException` (in service) | `LoginForm` | "Invalid username or password." |
| Password does not match | `AuthenticationException` (in service) | `LoginForm` | "Invalid username or password." |
| Account is inactive | `AuthenticationException` (in service) | `LoginForm` | "This account has been disabled. Please contact your system administrator." |
| Account is locked | `AuthenticationException` (in service) | `LoginForm` | "Account is temporarily locked. Please try again after N minutes." |
| Customer not found | `AuthenticationException` (in service) | `CustomerLookupForm` | "Customer not found. Please verify the information." |
| Invalid customer PIN | `AuthenticationException` (in service) | `CustomerLookupForm` | "Invalid PIN." |
| Session expired | `AuthenticationException` (in SessionManager getter) | `MainShellForm` (on next action) | "Your session has expired. Please log in again." |
| Database connection failure | `DataAccessException` | `Program.Main` catch | MessageBox: "A database error occurred. Please try again later." |
| Configuration key missing | `ConfigurationException` | Bootstrap/startup | MessageBox in `Program.Main` AppException catch |

---

## Security Implementation

### Password Hashing

- **Algorithm:** PBKDF2 (Rfc2898DeriveBytes) with SHA-256
- **Salt:** 16 random bytes per user, stored in base64 format in `PasswordSalt` column
- **Iterations:** Configurable via `HashIterations` app setting (default: 600000)
- **Output hash:** 32 bytes, stored in base64 format in `PasswordHash` column
- **Verification:** Extract salt from stored base64, re-hash input password, constant-time byte comparison

### Account Lockout

- **Threshold:** `MaxFailedLoginAttempts` app setting (default: 5 consecutive failures)
- **Duration:** `LockoutDurationMinutes` app setting (default: 15 minutes)
- **Counter persistence:** `FailedLoginAttempts` column incremented on each failure via `UserRepository.IncrementFailedAttempts`
- **Lock persistence:** `LockedUntil` column set to UTC now + duration when threshold reached
- **Auto-unlock:** On login attempt where `LockedUntil <= UtcNow`, the lock is cleared automatically
- **Reset:** On successful login, `FailedLoginAttempts = 0` and `LockedUntil = NULL`

### Session Management

- **Timeout:** `SessionTimeoutMinutes` app setting (default: 20 minutes of inactivity)
- **Enforcement:** `SessionManager.CurrentSession` getter checks `DateTime.UtcNow - LastActivityTimeUtc > timeout` and auto-calls `EndSession()`
- **Thread safety:** All `SessionManager` operations guarded by `lock(_lock)`

### Audit Logging

| Event | Data Recorded |
|---|---|
| Successful login | UserId, Username, Action=Login, MachineName, Timestamp |
| Logout | UserId, Username, Action=Logout, MachineName, Timestamp |
| Failed login attempt | Username (UserId may be null if not found), Action=FailedLogin, MachineName, Timestamp, Details (reason) |

### Error Message Security

- Login failures always use generic message: "Invalid username or password." — does not reveal whether username exists
- Inactive account message is slightly more specific but does not reveal account status to unauthenticated users
- Locked account message includes remaining time but does not reveal account validity

### Role-Based Access Control (UI Layer)

- `ISessionManager.HasRole(RoleType)` used in `MainShellForm` to toggle menu/toolbar visibility
- Menu items for administration, prescriptions, sales, and reports are conditionally visible
- The `MainShellForm` stores a reference to `ISessionManager` and re-checks on form activation

---

## Test Cases

### `PasswordHasherTests` (SmartMed.Tests.BLL)

| Test Method | Assertion |
|---|---|
| `HashPassword_ShouldReturnNonEmptyHash` | Result not null or empty |
| `VerifyPassword_ShouldReturnTrue_ForCorrectPassword` | `VerifyPassword(password, hash, salt) == true` |
| `VerifyPassword_ShouldReturnFalse_ForIncorrectPassword` | `VerifyPassword(wrong, hash, salt) == false` |
| `GenerateSalt_ShouldReturnNonEmptyBase64String` | Result is a non-empty base64 string |
| `GenerateSalt_ShouldProduceUniqueValuesPerCall` | Two calls produce different strings |
| `HashPassword_ShouldThrow_WhenPasswordIsNull` | `ValidationException` |
| `VerifyPassword_ShouldThrow_WhenPasswordIsNull` | `ValidationException` |

### `AuthenticationServiceTests` (SmartMed.Tests.BLL)

| Test Method | Assertion |
|---|---|
| `LoginAdmin_ShouldReturnSuccess_WithValidCredentials` | `IsSuccess == true`, `Data.IsAuthenticated == true` |
| `LoginAdmin_ShouldReturnFailure_WithInvalidPassword` | `IsSuccess == false`, `Data` is null |
| `LoginAdmin_ShouldReturnFailure_ForUnknownUsername` | `IsSuccess == false`, message contains "Invalid username or password" |
| `LoginAdmin_ShouldReturnFailure_ForInactiveUser` | `IsSuccess == false`, message contains "disabled" |
| `LoginAdmin_ShouldReturnFailure_ForLockedAccount` | `IsSuccess == false`, message contains "locked" |
| `LoginAdmin_ShouldIncrementFailedAttempts_OnFailedPassword` | Verify `IUserRepository.IncrementFailedAttempts` was called |
| `LoginAdmin_ShouldLockAccount_AfterMaxFailedAttempts` | After `_maxFailedAttempts` failures, verify `SetLockedUntil` was called |
| `LoginAdmin_ShouldResetFailedAttempts_OnSuccessfulLogin` | Verify `ResetFailedAttempts` was called |
| `LoginAdmin_ShouldLogAudit_OnSuccessfulLogin` | Verify `IAuditLogRepository.LogLogin` was called |
| `LoginAdmin_ShouldLogAudit_OnFailedLogin` | Verify `IAuditLogRepository.LogFailedAttempt` was called |
| `LoginAdmin_ShouldAutoUnlock_WhenLockExpired` | `LockedUntil` in past → attempt proceeds to password check |
| `Logout_ShouldClearSessionAndLogAudit` | `IsAuthenticated == false`, `LogLogout` called |
| `Constructor_ShouldThrow_WhenAnyDependencyIsNull` | `ValidationException` for each null parameter |

### `SessionManagerTests` (SmartMed.Tests.BLL)

| Test Method | Assertion |
|---|---|
| `StartSession_ShouldPopulateSessionContext` | Context properties match user data |
| `EndSession_ShouldClearSession` | `CurrentSession` is null, `IsActive` is false |
| `HasRole_ShouldReturnTrue_WhenRoleMatches` | `HasRole(Administrator)` for admin user → true |
| `HasRole_ShouldReturnFalse_WhenRoleDoesNotMatch` | `HasRole(Cashier)` for admin user → false |
| `IsActive_ShouldReturnFalse_AfterSessionTimeout` | Simulate time past timeout → `IsActive == false` |
| `StartSession_ShouldReplacePriorSession` | Second call overwrites current session |

### `UserRepositoryTests` (SmartMed.Tests.DAL)

| Test Method | Assertion |
|---|---|
| `Constructor_ShouldThrow_WhenConnectionFactoryIsNull` | `ValidationException` |
| `GetByUsername_ShouldThrow_WhenUsernameIsNull` | `ValidationException` |
| `GetByUsername_ShouldThrow_WhenUsernameIsWhitespace` | `ValidationException` |
| `IncrementFailedAttempts_ShouldNotThrow_ForValidId` | Executes without exception |
| `ResetFailedAttempts_ShouldNotThrow_ForValidId` | Executes without exception |
| `SetLockedUntil_ShouldNotThrow_ForValidParameters` | Executes without exception |
| `UpdateLastLogin_ShouldNotThrow_ForValidParameters` | Executes without exception |

### `AuditLogRepositoryTests` (SmartMed.Tests.DAL)

| Test Method | Assertion |
|---|---|
| `Constructor_ShouldThrow_WhenConnectionFactoryIsNull` | `ValidationException` |
| `LogLogin_ShouldThrow_WhenUsernameIsNull` | `ValidationException` |
| `LogLogin_ShouldNotThrow_WhenParametersAreValid` | Executes without exception |
| `LogLogout_ShouldNotThrow_WhenParametersAreValid` | Executes without exception |
| `LogFailedAttempt_ShouldNotThrow_WhenParametersAreValid` | Executes without exception |

### `UserModelTests` (SmartMed.Tests.Models)

| Test Method | Assertion |
|---|---|
| `User_ShouldExtendBaseEntity` | Inherits `Id`, `IsActive`, `CreatedDate` |
| `User_ShouldSetFailedLoginAttempts_DefaultZero` | New `User().FailedLoginAttempts == 0` |
| `User_ShouldSetLockedUntil_DefaultNull` | New `User().LockedUntil == null` |
| `User_ShouldSetLastLogin_DefaultNull` | New `User().LastLogin == null` |

### `AuditLogEntryModelTests` (SmartMed.Tests.Models)

| Test Method | Assertion |
|---|---|
| `AuditLogEntry_ShouldSetPropertiesCorrectly` | All properties round-trip correctly |

### `LoginFormTests` (SmartMed.Tests.UI)

| Test Method | Assertion |
|---|---|
| `LoginForm_ShouldInitializeComponents` | All controls created and not null |
| `LoginButton_ShouldBeDisabled_WhenUsernameIsEmpty` | `btnLogin.Enabled == false` |
| `LoginButton_ShouldBeDisabled_WhenPasswordIsEmpty` | `btnLogin.Enabled == false` |
| `LoginButton_ShouldShowErrorStatus_OnFailedLogin` | `lblStatus.Text` contains error, `ForeColor` is red |

### `CustomerLookupFormTests` (SmartMed.Tests.UI)

| Test Method | Assertion |
|---|---|
| `CustomerLookupForm_ShouldInitializeComponents` | All controls created and not null |
| `LookupButton_ShouldBeDisabled_WhenIdentifierIsEmpty` | `btnLookup.Enabled == false` |
| `LookupButton_ShouldShowErrorStatus_OnFailedLookup` | `lblStatus.Text` contains error |

### `ExceptionTests` (SmartMed.Tests.Common) — Modified file

| Test Method | Assertion |
|---|---|
| `AuthenticationException_ShouldInheritFromAppException` | `IsInstanceOfType(exception, typeof(AppException))` |

---

## Completion Criteria

All of the following must be true for Module 2 to be marked complete:

1. **All 25 new files exist** in their correct project directories matching the folder structure.
2. **All 7 modified files** contain the required additions (no Module 1 code removed).
3. **All interfaces** (`IPasswordHasher`, `IUserRepository`, `IAuditLogRepository`, `ISessionManager`, `IAuthenticationService`) are implemented by corresponding classes.
4. **Build passes** with zero errors and zero warnings for `SmartMed.sln` in Debug|x64 configuration.
5. **All 40+ test methods** pass when running `dotnet test` or the MSTest runner against `SmartMed.Tests`.
6. **Login flow works end-to-end:**
   - Application starts → LoginForm appears
   - Invalid credentials → error message shown, no session created
   - Account locked after N failures → lockout message shown
   - Valid credentials → session created → MainShellForm appears with correct role-based menus and status bar
   - Customer lookup → CustomerLookupForm opens → valid identifier creates session
   - Logout → session cleared → LoginForm re-appears
7. **Audit log entries** are written to the `AuditLogs` table for each login (success + failed) and logout.
8. **Failed login attempts** persist to the `Users.FailedLoginAttempts` column and reset on successful login.
9. **No source code files outside the listed 25 new files and 7 modified files** have been changed.
