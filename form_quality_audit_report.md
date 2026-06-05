# Frontend Form Quality Audit Report

This audit maps and reviews all forms in the frontend project, matching their payload structures, data types, and API endpoints against the backend C# controllers and DTOs. 

---

## 1. Authentication Module

### Login Form
* **Form Name**: `LoginComponent` (Auth Login)
* **Status**: `OK` (with minor cleanup suggestion)
* **API Endpoint**: `POST /api/Authentication/Login`
* **Data Collected**: `email`, `password`, `role`
* **Backend Expected DTO**: `LoginRequest` (`email`, `password`)
* **Issues**:
  * **Redundant Form Control**: The `loginForm` Group contains a `role` control initialized to `'user'`. This role selector was commented out in `login.component.html` and is completely ignored in `auth.service.ts` when building the HTTP payload.
* **Suggested Fix**: Remove the unused `role` control from `loginForm` in `login.component.ts`.

---

### Register Form
* **Form Name**: `RegisterComponent` (Auth Register)
* **Status**: `OK`
* **API Endpoint**: `POST /api/Authentication/Register`
* **Data Collected**: `firstName`, `lastName`, `name` (Username), `phoneNumber`, `email`, `password`
* **Backend Expected DTO**: `SignUpRequest` (`firstName`, `lastName`, `name`, `email`, `password`, `phoneNumber`, `referralCode`)
* **Issues**: None. All mandatory parameters are populated, and the service generates an `IdempotencyKey` UUID in the headers as expected by the C# `[Idempotent]` attribute.
* **Suggested Fix**: None required.

---

### Forgot Password Form
* **Form Name**: `ForgotPasswordComponent` (Auth Forgot Password)
* **Status**: `OK`
* **API Endpoint**: `POST /api/Authentication/ForgetPassword?email={email}`
* **Data Collected**: `email`
* **Backend Expected DTO**: Sent as `[FromQuery][Required] string email`
* **Issues**: None.
* **Suggested Fix**: None.

---

### Reset Password Form
* **Form Name**: `ResetPasswordComponent` (Auth Reset Password)
* **Status**: `OK`
* **API Endpoint**: `POST /api/Authentication/ResetPassword`
* **Data Collected**: `newPassword` (form field), `email` & `token` (automatically parsed from URL query params)
* **Backend Expected DTO**: `ResetPasswordRequest` (`email`, `token`, `newPassword`)
* **Issues**: None.
* **Suggested Fix**: None.

---

### Change Password Form
* **Form Name**: `ChangePasswordComponent` (Auth Change Password)
* **Status**: **Broken** (High-Risk/Critical)
* **API Endpoint**: `POST /api/Authentication/ChangePassword`
* **Data Collected**: `currentPassword`, `newPassword`
* **Backend Expected DTO**: N/A (Missing Endpoint!)
* **Issues**:
  * **Complete Backend Absence**: The backend `AuthenticationController` has **no ChangePassword endpoint** whatsoever. There are zero references to it in the backend solution. Any submission from this form will immediately fail with a `404 Not Found` error.
* **Suggested Fix**: Implement the `ChangePassword` endpoint in C# `AuthenticationController.cs` using the standard ASP.NET Core `UserManager<ApplicationUser>.ChangePasswordAsync(user, currentPassword, newPassword)`.

---

## 2. Vendor Management Module

### Vendor Services Form (Create & Update)
* **Form Name**: `ServicesComponent` (Vendor Services)
* **Status**: **Partial / Broken**
* **API Endpoint**: 
  * Create: `POST /api/Service` (multipart/form-data)
  * Update: `PUT /api/Service/{id}` (multipart/form-data)
  * Status Toggle: `HttpPatch("{id}/status")`
* **Data Collected**: `name`, `description`, `serviceTypeId`, `vendorTypeId`, `classification`, `eventTypeIds`, `price`, `duration` (SetupDuration), `leadTime` (LeadTimeRequired), `ServiceImages` (Create) or `Images` (Update)
* **Backend Expected DTO**: `CreateServiceRequest` (Create) / `UpdateServiceDTO` (Update)
* **Issues**:
  * **Status Toggle Payload Mismatch**: In `updateServiceStatus()`, the component calls `this.productService.update(service.id, updateData)` passing a **raw JSON object** (`UpdateProductRequest`). Because the `PUT /api/Service/{id}` endpoint expects a `[FromForm] UpdateServiceDTO dto` (form-data), the C# model binder will fail to bind the JSON payload, crashing the activation/pausing flow. Furthermore, the backend has a lightweight `PATCH /api/Service/{id}/status` endpoint specifically for status toggles, which the frontend ignores.
  * **Redundant Field in Form**: `vendorTypeId` is defined in the Reactive Form group but is completely ignored and excluded during payload creation. This is technically fine as the `ServiceTypeId` implies the vendor category, but it represents dead code.
  * **Awkward Backend Field Inconsistency**: The file upload key is `ServiceImages` when creating, but `Images` when updating. The frontend currently matches this awkward backend discrepancy, but it remains a code maintenance hazard.
* **Suggested Fix**:
  1. Add a `patchStatus(id: string)` method in `ProductService` that calls `PATCH /api/Service/{id}/status`.
  2. Update `updateServiceStatus()` in the component to call this lightweight endpoint instead of performing a full `PUT` update with JSON.
  3. Clean up the unused `vendorTypeId` in the service form definition.

---

### Vendor Packages Form
* **Form Name**: `PackagesComponent` (Vendor Packages)
* **Status**: **Broken** (High-Risk)
* **API Endpoint**: None (Mocked!)
* **Data Collected**: `name`, `description`, `price`, `priceType`, `selectedServices`
* **Backend Expected DTO**: N/A (Missing Controller/Entity!)
* **Issues**:
  * **Entirely Mocked Feature**: The package creation, deletion, and toggle flows are performed purely in-memory on the frontend (`this.packages`). There is **no API integration**, no backend `PackageController`, and no database table for packages.
* **Suggested Fix**: Design a C# `Package` domain entity, implement `PackageController` and its associated DTOs, and replace the in-memory array manipulation in the frontend component with HTTP client calls.

---

### Vendor Onboarding Form
* **Form Name**: `VendorJoinComponent` (Public Vendor Registration)
* **Status**: `OK` (with minor payload mismatches)
* **API Endpoint**: `POST /api/Vendor` (multipart/form-data)
* **Data Collected**: `firstName`, `lastName`, `email`, `phone`, `name` (Username), `password`, `businessName`, `ownerName`, `vendorTypeId`, `yearsInBusiness`, `description`, `portfolioLink`, Address (`street`, `city`, `state`, `postalCode`), `ProfilePicture`, `Document`
* **Backend Expected DTO**: `CreateVendorRequest` (`FirstName`, `LastName`, `Email`, `Password`, `ProfilePicture`, `Phone`, `Name`, `BusinessName`, `OwnerName`, `VendorTypeId`, `YearsInBusiness`, `Description`, `PortfolioLink`, `Address`, `Document`, `ServiceAreas`)
* **Issues**:
  * **Extra Field Sent**: The frontend address sub-group collects and sends `Address.PostalCode`. However, the backend C# address class (`Domain.Entities.Address`) does not possess a `PostalCode` property. This field is silently ignored.
  * **Functional Gap (ServiceAreas)**: `ServiceAreas` is present in `CreateVendorRequest` but is never collected in the multi-step onboarding wizard. While this does not cause registration to crash, it leaves the new vendor completely hidden from localized search results until they manually configure service areas post-onboarding.
* **Suggested Fix**:
  * Remove `postalCode` from the form.
  * Add a simple step in the vendor onboarding wizard to allow selecting coverage areas (City & Region) and map them to the `ServiceAreas` array payload.

---

### Admin Vendor-Create Form
* **Form Name**: `VendorCreateComponent` (Admin Panel Vendor Creation)
* **Status**: **Broken** (Critical)
* **API Endpoint**: `POST /api/Vendor` (expects form-data)
* **Data Collected**: Same as VendorJoin
* **Backend Expected DTO**: `CreateVendorRequest`
* **Issues**:
  * **Form-Data Payload Type Mismatch**: While `VendorJoinComponent` correctly builds a `FormData` object to support the backend's `[FromForm]` requirement, the Admin `VendorCreateComponent` sends the registration payload as a **raw JSON object** (`this.vendorService.create(payload)`). Because the C# endpoint is decorated with `[FromForm]`, the model binder will fail to parse this JSON payload, throwing `400 Bad Request` or validation errors on all mandatory fields.
* **Suggested Fix**: Refactor `VendorCreateComponent.onSubmit()` to instantiate a `FormData` object and append parameters sequentially (matching the address format: `Address.Street`, `Address.City`, etc.) before calling `vendorService.create()`.

---

## 3. Client Events & Orders Module

### Add Event Form
* **Form Name**: `AddEventComponent` (User Add Event)
* **Status**: `OK`
* **API Endpoint**: `POST /api/Event`
* **Data Collected**: `name` (Title), `date` (EventDate), `guests` (GuestCount), `city`, `state`, `street` (Location), `budget` (TotalBudget), `notes`, `eventTypeId`
* **Backend Expected DTO**: `CreateEventDto`
* **Issues**: None. Payload maps perfectly, and the `userId` is successfully resolved on the backend from the claims token.
* **Suggested Fix**: None.

---

### Event Checkout & Deposit Payment Form
* **Form Name**: Event Checkout & Deposit Flow (`MyEventsComponent` checkout button)
* **Status**: **Broken** (Highly Critical)
* **API Endpoint**:
  * Order Creation: `POST /api/Order` (expects JSON)
  * Paymob Initiation: `POST /api/payments/paymob` (expects JSON)
* **Data Collected**: Order and Billing Details (Paymob deposit amount, `orderId`, first/last name, phone, email)
* **Issues**:
  * **24-Karat Controller Response Bug**: In `payDeposit()` (lines 298-310), the frontend calls `this.orderService.createOrder(orderPayload)` and expects to receive the newly created `order` object back with its `id` to pass to the Paymob service:
    ```typescript
    this.orderService.createOrder(orderPayload).subscribe({
      next: (order) => {
        this.paymentService.initiatePaymob({
          orderId: order.id, // <-- Crash!
    ```
    However, the backend C# `OrderController.Create` endpoint is defined as:
    ```csharp
    return order.IsSuccess ? NoContent() : order.ToActionResult();
    ```
    It returns `204 NoContent` upon success, completely throwing away the generated `OrderResponse` body! As a result, the frontend receives `undefined` for `order`, causing `orderId` in the subsequent Paymob call to be sent as `undefined` (resulting in a blank or empty Guid lookup crash on the backend `PaymentsController`). The entire deposit payment system is completely broken!
* **Suggested Fix**: Modify the backend `OrderController.Create` action to return the created order response using `Ok(order.Value)` (or `CreatedAtAction`) instead of `NoContent()`.

---

## 4. Admin Taxonomy & Support Modules

### Admin Categories Form
* **Form Name**: `CategoriesComponent` (Admin Categories Dashboard)
* **Status**: `OK`
* **API Endpoint**: 
  * Vendor Type: `POST /api/VendorType` (Create) / `PUT /api/VendorType/{id}` (Update)
  * Service Type: `POST /api/ServiceType` (Create) / `PATCH /api/ServiceType/{id}` (Update)
  * Event Type: `POST /api/EventTypes` (Create) / `PUT /api/EventTypes/{id}` (Update)
* **Data Collected**: `name`, `vendorTypeId` (for Service Type mapping)
* **Backend Expected DTO**: `CreateOrUpdateVendorTypeRequest`, `CreateServiceTypeRequest`, `EventTypeCreateDto`
* **Issues**: None. Endpoints, HTTP verbs, and query shapes match the backend perfectly.
* **Suggested Fix**: None.

---

### Corporate Inquiry Form
* **Form Name**: `CorporateComponent` (Public B2B Inquiry Form)
* **Status**: **Broken** (Critical)
* **API Endpoint**: `POST /api/CompanyInquiry` (expects JSON)
* **Data Collected**: `companyName`, `contactPerson`, `phoneNumber`, `email`, `vendorTypeId`, `expectedDate`, `estimatedAttendees`, `approximateBudget`, `additionalRequirements`
* **Backend Expected DTO**: `CreateCompanyInquiryDto`
* **Issues**:
  * **Database Property Mismatch (`vendorTypeId` vs `EventTypeId`)**: The frontend collections dropdown and service payload binds to `vendorTypeId`. However, the backend DTO `CreateCompanyInquiryDto` expects an **`EventTypeId`**! Because `EventTypeId` is a required Guid in the C# request model and is missing from the JSON payload, it binds as `Guid.Empty` (00000000-0000-0000-0000-000000000000). The database will immediately reject this insertion due to a foreign key violation with the `EventTypes` table, breaking the inquiry submission.
* **Suggested Fix**: Update `corporate.component.ts` and `corporate.component.html` to fetch Event Types from `EventTypeService` instead of Vendor Types, display event classifications in the dropdown, and map the key to `eventTypeId` in the payload.

---

### Support Ticket Escalation Form
* **Form Name**: `TicketDetailComponent` Escalation Dialog (Admin Panel)
* **Status**: **Broken / Authorization Mismatch**
* **API Endpoint**: `POST /api/admin/support/tickets/{ticketId}/escalate` (mismatched!)
* **Data Collected**: `reason`, `escalate_to`, `notify_finance`
* **Backend Expected DTO**: `TicketEscalateRequestDTO`
* **Issues**:
  * **Route Definition Mismatch (404)**: The frontend support service builds the URL as `${this.baseUrl}/${ticketId}/escalate` which translates to `/api/admin/support/tickets/{ticketId}/escalate`. However, the backend `SupportTicketsController.cs` defines the route as `[HttpPost("api/support/tickets/{ticketId}/escalate")]` (without `/admin`). This route mismatch causes a `404 Not Found` error.
  * **Role Authorization Mismatch (403)**: Even if the URL is corrected, the backend escalation endpoint is decorated with `[Authorize(Roles = "Vendor,Customer")]`. Since this escalation form is built inside the **Admin Panel** (`TicketDetailComponent` is only accessible to admins), any attempt by an Admin to click "Escalate" will result in a `403 Forbidden` error because the Admin does not possess the `Vendor` or `Customer` role.
* **Suggested Fix**: 
  1. Define a dedicated Admin escalation endpoint on the backend (e.g., `POST api/admin/support/tickets/{ticketId}/escalate`) that permits the `"Admin"` role.
  2. Correct the frontend `SupportService` API URL mapping to direct to the appropriate admin route.

---

## 5. Summary & High-Risk Diagnostic

### Summary of Critical Issues
* **24-Karat Controller Response Bug**: The payment flow is entirely dead in the water because the backend `OrderController` returns `204 NoContent` instead of returning the order object (or at least the `Id`), which the frontend checkout system expects to trigger Paymob.
* **Corporate Inquiry Event Mismatch**: Corporate B2B quote inquiries fail because the frontend sends `vendorTypeId` but the backend database expects `EventTypeId`, throwing foreign key database violations.
* **Admin Vendor-Create Content-Type Mismatch**: Onboarding new vendors via the Admin panel fails with 400 Bad Request errors because the frontend sends JSON to a `[FromForm]` endpoint.
* **Support Ticket Escalation Crash**: Admin ticket escalation fails due to route mismatches (404) and role protection mismatches (403).

### Missing Forms (Gaps)
1. **Admin Panel User-Creation Form**: The backend `UserController` exposes a `CreateUser` endpoint (`POST /api/User`) expecting a `CreateUserRequest`, but there is no admin form to onboarding normal clients manually.
2. **Admin Panel User-Edit Form**: The backend `UserController` defines `UpdateUser` (`PATCH /api/User/{id}`) but no frontend interface exists.
3. **Client Profile Editing Form**: The frontend client dashboard lacks a form allowing normal customers to update their name, email, or telephone number via the backend `UserController` endpoints.

### High-Risk Bugs
| Risk Level | Impacted Module | Bug Description |
| :--- | :--- | :--- |
| 🚨 **CRITICAL** | Client Orders / Payments | **Order ID Mismatch**: Frontend receives empty response (204) on order creation; sends `orderId: undefined` to Paymob payment gateway, causing total payment flow failure. |
| 🚨 **CRITICAL** | Corporate Leads | **Database Foreign Key Mismatch**: `vendorTypeId` sent instead of `EventTypeId`, causing immediate C# database insertion failure. |
| 🔴 **HIGH** | Admin Vendor Onboarding | **Payload Format Mismatch**: Admin panel sends JSON instead of Form-Data, failing C# binder validation. |
| 🔴 **HIGH** | Auth Security | **Change Password Route Absence**: Form is connected to a non-existent route, resulting in 404s. |
| 🟡 **MEDIUM** | Admin Support | **Escalation Mismatch**: Escalation triggers 404 (incorrect URL structure) and 403 (requires Vendor/Customer role, which Admin lacks). |
