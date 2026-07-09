# SmartMed Pharmacy Management System — UML & Architecture Diagrams

All diagrams below are Mermaid code and are drawn directly from the actual implementation (real class names, real table/column/foreign-key names, real method call chains) rather than a generic template. Paste any block into the [Mermaid Live Editor](https://mermaid.live), a GitHub Markdown preview, or your report tool to render it.

---

## 1. Use Case Diagram

Mermaid has no native UML use-case notation, so actors and use cases are modeled as a `flowchart` with actor nodes on the left and rounded "use case" nodes grouped by module. The four actors and their visible capabilities are taken directly from the role-gating logic in `MainShellForm.UpdateMenuVisibility()` and `CustomerShellForm`'s sidebar (Session 2).

```mermaid
flowchart LR
    Admin(["👤 Administrator"])
    Pharmacist(["👤 Pharmacist"])
    Cashier(["👤 Cashier"])
    Customer(["👤 Customer"])

    subgraph Catalogue["Catalogue Management"]
        UC1(("Manage Medicines"))
        UC2(("Manage Categories"))
    end

    subgraph Procurement["Procurement"]
        UC3(("Manage Suppliers"))
        UC4(("Create/Confirm Purchases"))
    end

    subgraph POS["Point of Sale"]
        UC5(("Process Sale"))
    end

    subgraph OrdersMod["Customer Orders"]
        UC6(("Manage Orders / Advance Status"))
        UC7(("View Prescription"))
        UC8(("Register Account"))
        UC9(("Browse Medicines"))
        UC10(("Manage Cart"))
        UC11(("Place Order"))
        UC12(("Track My Orders"))
        UC13(("Cancel My Order"))
    end

    subgraph PeopleMod["People"]
        UC14(("Manage Customers"))
    end

    subgraph Insights["Insights"]
        UC15(("Generate Reports"))
        UC16(("Export CSV/Excel/PDF"))
        UC17(("View Dashboard"))
    end

    subgraph AuthMod["Authentication"]
        UC18(("Staff Login"))
        UC19(("Customer Lookup Login"))
    end

    Admin --> UC1 & UC2 & UC3 & UC4 & UC5 & UC6 & UC7 & UC14 & UC15 & UC16 & UC17 & UC18
    Pharmacist --> UC1 & UC2 & UC6 & UC7 & UC18
    Cashier --> UC5 & UC18
    Customer --> UC8 & UC9 & UC10 & UC11 & UC12 & UC13 & UC19
```

---

## 2. Three-Layer Architecture Diagram

Reflects the real project reference graph: `SmartMed.UI` depends on `SmartMed.BLL`, `SmartMed.Reports`, `SmartMed.Models`, and `SmartMed.Common`; `SmartMed.BLL` depends on `SmartMed.DAL`, `SmartMed.Reports`, `SmartMed.Models`, and `SmartMed.Common`; `SmartMed.DAL` depends only on `SmartMed.Models` and `SmartMed.Common`, and talks to SQL Server via `System.Data.SqlClient`.

```mermaid
flowchart TB
    subgraph Presentation["Presentation Layer — SmartMed.UI"]
        direction LR
        Forms["Forms\n(LoginForm, MainShellForm, CustomerShellForm,\nMedicineForm, SalesForm, OrderManagementForm, ...)"]
        Theme["Theme System\n(AppTheme, RoundedButton, ModernTextBox,\nSidebarNavControl, DataGridViewStyler)"]
        Bootstrapper["ApplicationBootstrapper\n(manual DI composition root)"]
    end

    subgraph Business["Business Logic Layer — SmartMed.BLL"]
        direction LR
        Services["Services\n(AuthenticationService, MedicineService, SalesService,\nPurchaseService, OrderService, CustomerService,\nMedicineSearchService, ReportService, PrescriptionService)"]
        Results["OperationResult / OperationResult&lt;T&gt;\nresult pattern"]
    end

    subgraph DataAccess["Data Access Layer — SmartMed.DAL"]
        direction LR
        Repositories["Repositories\n(one per aggregate, raw ADO.NET,\nparameterized SQL)"]
        Infra["Infrastructure\n(SqlConnectionFactory, SqlUnitOfWork)"]
    end

    subgraph Shared["Shared / Cross-Cutting"]
        direction LR
        Models["SmartMed.Models\n(Entities, Enums, Results, Reports DTOs)"]
        Common["SmartMed.Common\n(Guard, AppSettings, Exceptions)"]
        Reports["SmartMed.Reports\n(PdfExporter)"]
    end

    DB[("SQL Server\nSmartMedDb")]

    Presentation --> Business
    Business --> DataAccess
    DataAccess --> DB
    Presentation -.-> Shared
    Business -.-> Shared
    DataAccess -.-> Shared
    Presentation -.-> Reports
    Business -.-> Reports
```

---

## 3. Component Diagram

Shows the concrete components inside each layer and their dependency arrows, matching how `ApplicationBootstrapper.RegisterServices()` actually wires everything together.

```mermaid
flowchart TB
    subgraph UI["SmartMed.UI"]
        LoginForm
        MainShellForm
        CustomerShellForm
        MedicineForm
        SalesForm
        OrderManagementForm
        CustomerManagementForm
        BrowseMedicinesForm
        CartAndCheckoutForm
        DashboardForm
    end

    subgraph BLL["SmartMed.BLL Services"]
        AuthenticationService
        SessionManager
        MedicineService
        MedicineCategoryService
        SupplierService
        PurchaseService
        InventoryService
        SalesService
        PricingService
        SaleNumberGenerator
        OrderService
        OrderNumberGenerator
        CustomerService
        PrescriptionService
        MedicineSearchService
        ReportService
    end

    subgraph DAL["SmartMed.DAL Repositories"]
        UserRepository
        CustomerRepository
        MedicineRepository
        MedicineCategoryRepository
        SupplierRepository
        PurchaseRepository
        PurchaseItemRepository
        StockBatchRepository
        StockMovementRepository
        SaleRepository
        SaleItemRepository
        PaymentRepository
        OrderRepository
        OrderItemRepository
        ReportRepository
        AuditLogRepository
    end

    DB[("SQL Server")]

    LoginForm --> AuthenticationService
    MainShellForm --> SalesService & PurchaseService & MedicineService & SupplierService & OrderService & CustomerService & ReportService
    CustomerShellForm --> MedicineSearchService & OrderService & PrescriptionService & CustomerService
    MedicineForm --> MedicineService & MedicineCategoryService
    SalesForm --> SalesService & InventoryService & PricingService & SaleNumberGenerator
    OrderManagementForm --> OrderService & CustomerService
    CustomerManagementForm --> CustomerService & OrderService
    BrowseMedicinesForm --> MedicineSearchService
    CartAndCheckoutForm --> OrderService & PrescriptionService
    DashboardForm --> ReportService

    AuthenticationService --> UserRepository & CustomerRepository & AuditLogRepository & SessionManager
    MedicineService --> MedicineRepository
    PurchaseService --> PurchaseRepository & PurchaseItemRepository & StockBatchRepository & StockMovementRepository
    SalesService --> SaleRepository & SaleItemRepository & PaymentRepository & InventoryService
    InventoryService --> StockBatchRepository & StockMovementRepository
    OrderService --> OrderRepository & OrderItemRepository & InventoryService
    CustomerService --> CustomerRepository
    ReportService --> ReportRepository
    MedicineSearchService --> MedicineRepository

    UserRepository & CustomerRepository & MedicineRepository & SupplierRepository & PurchaseRepository & StockBatchRepository & SaleRepository & OrderRepository & ReportRepository --> DB
```

---

## 4. Class Diagram

Core domain entities from `SmartMed.Models/Entities/*.cs`, all inheriting `BaseEntity` (`Id`, `IsActive`, `CreatedDate`, `UpdatedDate`) except the logging POCOs.

```mermaid
classDiagram
    class BaseEntity {
        +int Id
        +bool IsActive
        +DateTime CreatedDate
        +DateTime? UpdatedDate
    }

    class Medicine {
        +int CategoryId
        +string Name
        +string Brand
        +DosageForm DosageForm
        +string Strength
        +string Unit
        +int StockQuantity
        +int ReorderLevel
        +decimal UnitPrice
        +DateTime? ExpiryDate
        +string Description
        +decimal DiscountPercent
        +string PromotionLabel
        +bool RequiresPrescription
    }

    class MedicineCategory {
        +string Name
        +string Description
    }

    class Supplier {
        +string SupplierCode
        +string SupplierName
        +string CompanyName
        +string ContactPerson
        +string PhoneNumber
        +string Email
        +string Address
    }

    class Purchase {
        +string PurchaseNumber
        +DateTime PurchaseDate
        +int SupplierId
        +int CreatedByUserId
        +PurchaseStatus Status
        +decimal TotalAmount
        +List~PurchaseItem~ Items
    }

    class PurchaseItem {
        +int PurchaseId
        +int MedicineId
        +string BatchNumber
        +DateTime ExpiryDate
        +int Quantity
        +decimal PurchasePrice
        +decimal SellingPrice
        +decimal LineTotal
    }

    class StockBatch {
        +int MedicineId
        +string BatchNumber
        +DateTime ExpiryDate
        +int CurrentQuantity
        +int InitialQuantity
        +int PurchaseItemId
        +BatchStatus Status
    }

    class Sale {
        +string SaleNumber
        +DateTime SaleDate
        +int CashierId
        +int? CustomerId
        +decimal SubTotal
        +decimal GrandTotal
        +SaleStatus Status
    }

    class SaleItem {
        +int SaleId
        +int MedicineId
        +int StockBatchId
        +int Quantity
        +decimal SellingPrice
        +decimal LineTotal
    }

    class Payment {
        +int SaleId
        +PaymentMethod PaymentMethod
        +decimal AmountPaid
        +decimal ChangeAmount
        +PaymentStatus PaymentStatus
    }

    class Customer {
        +string FullName
        +string PhoneNumber
        +string Email
        +string PinHash
        +string PinSalt
        +string Address
        +string City
    }

    class Order {
        +string OrderNumber
        +int CustomerId
        +DateTime OrderDate
        +OrderStatus Status
        +decimal GrandTotal
        +string PrescriptionFilePath
        +List~OrderItem~ Items
    }

    class OrderItem {
        +int OrderId
        +int MedicineId
        +int Quantity
        +decimal UnitPrice
        +decimal DiscountPercent
        +decimal LineTotal
    }

    class User {
        +string Username
        +string PasswordHash
        +string DisplayName
        +RoleType Role
        +int FailedLoginAttempts
        +DateTime? LockedUntil
    }

    BaseEntity <|-- Medicine
    BaseEntity <|-- MedicineCategory
    BaseEntity <|-- Supplier
    BaseEntity <|-- Purchase
    BaseEntity <|-- PurchaseItem
    BaseEntity <|-- StockBatch
    BaseEntity <|-- Sale
    BaseEntity <|-- SaleItem
    BaseEntity <|-- Payment
    BaseEntity <|-- Customer
    BaseEntity <|-- Order
    BaseEntity <|-- OrderItem
    BaseEntity <|-- User

    MedicineCategory "1" --> "many" Medicine
    Supplier "1" --> "many" Purchase
    Purchase "1" *-- "many" PurchaseItem
    Medicine "1" --> "many" PurchaseItem
    PurchaseItem "1" --> "1" StockBatch
    Medicine "1" --> "many" StockBatch
    User "1" --> "many" Sale : cashier
    Customer "0..1" --> "many" Sale
    Sale "1" *-- "many" SaleItem
    Medicine "1" --> "many" SaleItem
    StockBatch "1" --> "many" SaleItem
    Sale "1" --> "many" Payment
    Customer "1" --> "many" Order
    Order "1" *-- "many" OrderItem
    Medicine "1" --> "many" OrderItem
```

---

## 5. ER Diagram

Reflects the real SQL tables/foreign keys from `SmartMed.DAL/Scripts/*.sql`, including the `Sales.CustomerId` fix and the `Orders`/`OrderItems`/`Customers` tables added this session.

```mermaid
erDiagram
    USERS ||--o{ SALES : "cashier"
    USERS ||--o{ PURCHASES : "created by"
    USERS ||--o{ PAYMENTS : "processed by"
    USERS ||--o{ AUDITLOGS : "acted by"

    CUSTOMERS ||--o{ ORDERS : places
    CUSTOMERS |o--o{ SALES : "optional walk-in link"

    MEDICINECATEGORIES ||--o{ MEDICINES : contains

    SUPPLIERS ||--o{ PURCHASES : supplies

    PURCHASES ||--o{ PURCHASEITEMS : contains
    MEDICINES ||--o{ PURCHASEITEMS : "ordered as"
    PURCHASEITEMS ||--|| STOCKBATCHES : "creates"
    MEDICINES ||--o{ STOCKBATCHES : "stocked as"

    SALES ||--o{ SALEITEMS : contains
    MEDICINES ||--o{ SALEITEMS : "sold as"
    STOCKBATCHES ||--o{ SALEITEMS : "drawn from"
    SALES ||--o{ PAYMENTS : "paid by"

    ORDERS ||--o{ ORDERITEMS : contains
    MEDICINES ||--o{ ORDERITEMS : "ordered as"

    MEDICINES ||--o{ STOCKMOVEMENTS : "tracked by"
    STOCKBATCHES ||--o{ STOCKMOVEMENTS : "tracked by"

    USERS {
        int Id PK
        nvarchar Username UK
        nvarchar PasswordHash
        nvarchar PasswordSalt
        nvarchar DisplayName
        tinyint Role
        int FailedLoginAttempts
        datetime2 LockedUntil
    }

    CUSTOMERS {
        int Id PK
        nvarchar FullName
        nvarchar PhoneNumber UK
        nvarchar Email UK
        nvarchar PinHash
        nvarchar PinSalt
    }

    MEDICINECATEGORIES {
        int Id PK
        nvarchar Name UK
    }

    MEDICINES {
        int Id PK
        int CategoryId FK
        nvarchar Name
        int StockQuantity
        int ReorderLevel
        decimal UnitPrice
        decimal DiscountPercent
        bit RequiresPrescription
    }

    SUPPLIERS {
        int Id PK
        nvarchar SupplierCode UK
        nvarchar SupplierName UK
    }

    PURCHASES {
        int Id PK
        nvarchar PurchaseNumber UK
        int SupplierId FK
        int CreatedByUserId FK
        tinyint Status
        decimal TotalAmount
    }

    PURCHASEITEMS {
        int Id PK
        int PurchaseId FK
        int MedicineId FK
        nvarchar BatchNumber
        int Quantity
        decimal PurchasePrice
        decimal SellingPrice
    }

    STOCKBATCHES {
        int Id PK
        int MedicineId FK
        int PurchaseItemId FK
        nvarchar BatchNumber
        int CurrentQuantity
        tinyint BatchStatus
    }

    SALES {
        int Id PK
        nvarchar SaleNumber UK
        int CashierId FK
        int CustomerId FK
        decimal GrandTotal
        tinyint Status
    }

    SALEITEMS {
        int Id PK
        int SaleId FK
        int MedicineId FK
        int StockBatchId FK
        int Quantity
        decimal LineTotal
    }

    PAYMENTS {
        int Id PK
        int SaleId FK
        int ProcessedByUserId FK
        tinyint PaymentMethod
        decimal AmountPaid
    }

    ORDERS {
        int Id PK
        nvarchar OrderNumber UK
        int CustomerId FK
        tinyint Status
        decimal GrandTotal
        nvarchar PrescriptionFilePath
    }

    ORDERITEMS {
        int Id PK
        int OrderId FK
        int MedicineId FK
        int Quantity
        decimal LineTotal
    }

    STOCKMOVEMENTS {
        int Id PK
        int StockBatchId FK
        int MedicineId FK
        tinyint MovementType
        int Quantity
        nvarchar ReferenceType
        int ReferenceId
    }

    AUDITLOGS {
        int Id PK
        int UserId FK
        nvarchar Username
        tinyint Action
        datetime2 Timestamp
    }
```

---

## 6. Activity Diagram

The end-to-end customer order fulfillment workflow, mirroring the real branching logic in `OrderService.PlaceOrder` and `OrderService.UpdateOrderStatus`/`ValidateTransition` (Session 1/2).

```mermaid
flowchart TD
    Start([Customer browses medicines]) --> AddCart[Add items to cart]
    AddCart --> Checkout[Open Cart & Checkout]
    Checkout --> RxCheck{Any item\nRequiresPrescription?}
    RxCheck -- Yes --> RxUploaded{Prescription\nfile attached?}
    RxUploaded -- No --> BlockOrder[Block: show inline error]
    BlockOrder --> Checkout
    RxUploaded -- Yes --> StockCheck
    RxCheck -- No --> StockCheck{Sufficient stock\nfor all lines?}
    StockCheck -- No --> Fail[OperationResult.Failure]
    StockCheck -- Yes --> PlaceOrder[OrderService.PlaceOrder\ngenerate order number, insert Order + OrderItems]
    PlaceOrder --> Pending[[Order status: Pending]]

    Pending --> AdminReview{Admin/Pharmacist\nreviews order}
    AdminReview -- Reject --> Rejected[[Status: Rejected]]
    AdminReview -- Cancel --> Cancelled1[[Status: Cancelled\nno stock to reverse]]
    AdminReview -- Approve --> Approved[[Status: Approved]]

    Approved --> ReadyDecision{Admin marks\nReady for Pickup}
    ReadyDecision -- Cancel --> Cancelled2[[Status: Cancelled\nno stock to reverse]]
    ReadyDecision -- Proceed --> CommitStock[DeductFIFO per line,\nwrite StockMovements,\nupdate Medicine.StockQuantity]
    CommitStock --> Processing[[Status: Processing]]

    Processing --> DeliverDecision{Admin marks\nDelivered or Cancels}
    DeliverDecision -- Deliver --> Completed[[Status: Completed]]
    DeliverDecision -- Cancel --> ReverseStock[Reverse stock quantities]
    ReverseStock --> Cancelled3[[Status: Cancelled\nstock reversed]]

    Completed --> End([Customer sees\n'Delivered' in stepper])
    Rejected --> End
    Cancelled1 --> End
    Cancelled2 --> End
    Cancelled3 --> End
```

---

## 7. Sequence Diagram

The point-of-sale flow, matching the real call sequence inside `SalesService.CreateSale` (transaction, FIFO deduction, sale number generation, stock movement logging).

```mermaid
sequenceDiagram
    actor Cashier
    participant SF as SalesForm
    participant SNG as SaleNumberGenerator
    participant SS as SalesService
    participant IS as InventoryService
    participant PS as PricingService
    participant SaleRepo as SaleRepository
    participant SaleItemRepo as SaleItemRepository
    participant PayRepo as PaymentRepository
    participant StockMoveRepo as StockMovementRepository
    participant MedRepo as MedicineRepository
    participant DB as SQL Server

    Cashier->>SF: Search medicine, add to cart, enter payment
    SF->>SS: CreateSale(sale, items, payment)
    SS->>SS: AuthorizeSaleOperation() [role check]
    SS->>DB: BeginTransaction()
    loop for each SaleItem
        SS->>IS: DeductFIFO(medicineId, quantity, connection, transaction)
        IS->>DB: SELECT batches ORDER BY ExpiryDate (FIFO)
        IS->>DB: UPDATE StockBatches.CurrentQuantity
        IS-->>SS: List of BatchDeduction
    end
    SS->>PS: CalculateDiscountAmount / CalculateTaxAmount / CalculateGrandTotal
    PS-->>SS: subTotal, discountAmount, taxAmount, grandTotal
    SS->>SaleRepo: Add(sale, connection, transaction)
    SaleRepo->>DB: INSERT INTO Sales
    DB-->>SaleRepo: new SaleId
    SS->>SaleItemRepo: AddRange(items, connection, transaction)
    SaleItemRepo->>DB: INSERT INTO SaleItems
    SS->>PS: CalculateChange(amountPaid, grandTotal)
    SS->>PayRepo: Add(payment, connection, transaction)
    PayRepo->>DB: INSERT INTO Payments
    loop for each BatchDeduction
        SS->>StockMoveRepo: Add(stockMovement, connection, transaction)
        StockMoveRepo->>DB: INSERT INTO StockMovements
        SS->>MedRepo: UpdateStockQuantity(medicineId, -qty, connection, transaction)
        MedRepo->>DB: UPDATE Medicines.StockQuantity
    end
    SS->>DB: Commit()
    SS-->>SF: OperationResult(int).Success(saleId)
    SF-->>Cashier: Print invoice via SalePrintDocument
```
