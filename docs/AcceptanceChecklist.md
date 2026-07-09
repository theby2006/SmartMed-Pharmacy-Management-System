# SmartMed Acceptance Checklist — Session 2 (Shells, Customer Flow, Staff Management, Exports, Dashboard)

This is a manual, end-to-end script for verifying this session's work on a real Windows machine with SQL Server, since the development sandbox cannot render or click through a WinForms UI (no `libgdiplus`/Windows GUI available there — everything below was verified by compilation and structural/unit tests only). Work through it top to bottom; each step assumes the previous ones passed.

## 0. Setup

- [ ] Run `SmartMed.DAL/Scripts/000_SetupDatabase.sql` against a fresh SQL Server instance (creates `SmartMedDb`, all tables, seeds staff + sample customer accounts).
- [ ] Open `SmartMed.sln` in Visual Studio, confirm it builds with zero errors (`Build > Build Solution`).
- [ ] Set `SmartMed.UI` as the startup project and run it.

## 1. Staff Login & Sidebar

- [ ] Log in as `admin` / `Admin@123`. Confirm the shell shows a **dark sidebar** on the left (not the old top MenuStrip) with sections: Dashboard, Inventory (Medicines, Categories), Procurement (Suppliers, Purchases), Sales (New Sale), Orders (Manage Orders), People (Customers), Insights (Reports).
- [ ] Log out, log in as `pharmacist` / `Pharm@123`. Confirm only **Medicines, Categories, Manage Orders** are visible (no Dashboard, Suppliers, Purchases, New Sale, Customers, Reports).
- [ ] Log out, log in as `cashier` / `Cash@123`. Confirm only **New Sale** is visible.
- [ ] As admin, click every sidebar item once and confirm each opens a real, populated screen — none should be blank or throw an error. (Medicines/Suppliers/Dashboard open inline; Categories/Purchases/New Sale/Reports open as a dialog — both are expected per this session's scope.)

## 2. Medicines & Suppliers (previously unreachable, now restyled)

- [ ] Open Medicines from the sidebar. Confirm it now has a search box, a styled grid, and a form section with **Discount %, Promotion label, and "Requires prescription"** fields.
- [ ] Add a medicine with a `DiscountPercent > 0` and one with `RequiresPrescription` checked. Confirm both save and reload correctly (values round-trip through Update too).
- [ ] Open Suppliers from the sidebar. Confirm add/update/delete/search/status-filter all still work exactly as before, just restyled.

## 3. Customer Registration

- [ ] From the login screen, click "Continue as Customer?", then "New here? Create an account".
- [ ] Register a new customer with a valid phone number and PIN. Confirm it signs you straight into the **Customer Portal** shell (separate dark sidebar: Browse Medicines, My Cart, My Orders).
- [ ] Try registering again with the same phone number — confirm it's rejected as a duplicate.

## 4. Browse Medicines (search algorithms)

- [ ] On Browse Medicines, search by a partial medicine name — confirm the result caption shows "linear scan + filters" and the row count is correct.
- [ ] Filter by category only, then by a price range — confirm results narrow correctly each time.
- [ ] Enter the **same value** in both Min $ and Max $ (an exact price, no other filters) — confirm the caption switches to "binary search on price", proving the binary-search code path is actually taken, not just present.
- [ ] Confirm any medicine with `DiscountPercent > 0` shows a promotion badge, and any with `RequiresPrescription` shows an "Rx required" badge.
- [ ] Confirm a medicine with 0 stock shows "Out of stock" and its Add to Cart button is disabled when selected.

## 5. Cart & Checkout

- [ ] Add two or three medicines to the cart (including one requiring a prescription). Go to My Cart.
- [ ] Edit a quantity directly in the grid — confirm the line total and grand total recalculate.
- [ ] Remove a line — confirm totals update and the line disappears.
- [ ] Confirm the prescription-upload panel appears because a line requires one, and that "Place Order" is blocked with an inline error until a file is attached.
- [ ] Attach a `.jpg`/`.png`/`.pdf` file, then place the order. Confirm a success message with the order number appears and the cart empties.

## 6. Track Orders (customer side)

- [ ] Go to My Orders. Confirm the new order appears with status **Pending** and the status stepper shows step 1 (Pending) highlighted.
- [ ] Confirm "Cancel Order" is visible while Pending, and cancelling it moves the status to Cancelled and hides the cancel button.
- [ ] Place a second order (without cancelling) for the next steps.

## 7. Manage Orders (admin/pharmacist side — stock commit/reversal)

- [ ] Log in as admin (or pharmacist) → Manage Orders. Confirm the pending order from step 6 appears, and the status filter pills (All/Pending/Approved/Ready for Pickup/Delivered/Cancelled) filter the grid correctly.
- [ ] Select the order, confirm customer info, line items, and (if a prescription was attached) a working "View Prescription" button that opens the file.
- [ ] Click "Approve Order" → confirm status becomes Approved and the button now reads "Mark Ready for Pickup".
- [ ] Note the medicine's stock quantity (via Medicines screen) **before** the next step.
- [ ] Click "Mark Ready for Pickup" → confirm this is the step that actually deducts stock (compare the medicine's stock quantity before/after — it should drop by the ordered quantity), and status becomes Processing.
- [ ] Click "Mark Delivered" → confirm status becomes Completed.
- [ ] Back in the customer's My Orders screen, confirm the stepper now shows all three steps completed ("Delivered").

## 8. Manage Customers

- [ ] As admin, open Customers. Search for the customer registered in step 3.
- [ ] Edit their name/phone/email, save, confirm the change persists.
- [ ] Confirm "View Orders" / order history sub-grid shows both orders placed in steps 5–6 with correct statuses.
- [ ] Deactivate the customer, confirm the status flips to Inactive and they can no longer log in via Customer Lookup.

## 9. Reports & Exports

- [ ] Open Reports. Generate any report with data (e.g., Medicine List or Category Summary).
- [ ] Click **Export CSV** — confirm the file opens correctly in Excel/a text editor.
- [ ] Click **Export Excel** — confirm the `.xlsx` file opens in Excel/LibreOffice **without a repair prompt**, with a bold header row and correct values.
- [ ] Click **Export PDF** — confirm the `.pdf` file opens in a PDF reader, shows the report title, generation timestamp, and a readable (monospaced) data table; if the report has enough rows to span multiple pages, confirm each page shows "Page N of M" and repeats the header row.
- [ ] From the customer's My Orders screen, click "Export PDF" and "Export Excel" for order history — confirm both produce valid files with the customer's orders.

## 10. Dashboard

- [ ] As admin, open Dashboard. Confirm four KPI cards render (Today's Revenue, Today's Transactions, Low Stock, Near Expiry) with real numbers, not placeholders.
- [ ] Confirm a "Last 7 Days Sales" column chart renders with one bar per day.
- [ ] Confirm a stock-status doughnut chart renders (OK / Low Stock / Near Expiry / Expired slices).
- [ ] If any medicines are low-stock or near-expiry, confirm the amber/red alert panel appears below the charts listing the counts; if none, confirm the panel is hidden rather than shown empty.

## Known, Intentional Limitations (not defects)

- Categories, Purchases, New Sale, and Reports open as classic modal dialogs rather than the new inline sidebar content area — they were wired for reachability this session but not visually restyled (explicitly scoped out).
- Role-based authorization guards were only added to the new customer/order services this session (Session 1 decision, unchanged) — `MedicineService`, `SupplierService`, `PurchaseService`, and `UserService` still rely on UI-level menu gating rather than server-side role checks. This is flagged as recommended follow-up work, not a regression introduced now.
- The hand-rolled `.xlsx` and `.pdf` writers were verified by structural inspection (valid zip parts, valid PDF object/xref structure) and automated tests in this sandbox; final rendering fidelity in Excel/Acrobat should still be spot-checked per the steps above, since this sandbox cannot open them in real Office/Acrobat applications.
