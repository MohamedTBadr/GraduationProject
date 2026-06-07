# Implementation Plan - EpicHub Graduation Project Demo Recording Suite

This plan details the design and deployment of a suite of automated browser-recording scripts using Playwright. These scripts will run against the deployed staging website to capture video recordings of the primary user, vendor, and administrator workflows.

## User Review Required

> [!IMPORTANT]
> - **Execution Context:** The scripts will run headlessly on your machine and record high-quality $1280 \times 900$ (or custom device sizes) videos of the browser.
> - **Outputs:** Captured videos will be output to `verify-shots/videos/` as `.webm` files.
> - **Flexible Base URL (Staging / Local Fallback):** All scripts will define their target URL using a configurable `BASE` variable:
>   `const BASE = process.env.BASE_URL || 'https://epichubweb-g9h9a3a8bxafekdm.francecentral-01.azurewebsites.net';`
>   This allows the suite to target the live Azure staging site by default. If the live site is down or unresponsive, we can start your local .NET API and Angular servers, and run the recording suite with `BASE_URL=http://localhost:4200` to capture everything locally.

---

## Proposed Demo Video Flows

We will divide the demo into 10 separate scripts, one for each core flow. This keeps the videos focused, high-performing, and easy to watch.

### 1. User: Explore, Find Vendor & Request Booking
- **Script:** `record-01-explore-and-book.js`
- **Actions:** 
  1. Open homepage, browse features and slider sections.
  2. Navigate to Explore Services/Vendors page.
  3. Explore a few vendor cards, check details, reviews, ratings, and packages.
  4. Perform login as `customer@example.com`.
  5. Go to My Events, create a new Event.
  6. Go back to the vendor service and send a booking request.

### 2. User: AI Event fast-tracking & Smart Budgeting
- **Script:** `record-02-ai-generation.js`
- **Actions:**
  1. Login as `customer@example.com`.
  2. Open Event Studio and navigate to Package Planning / AI Event Generator.
  3. Input event preferences (e.g., Wedding in Cairo, budget: 100,000 EGP).
  4. Generate and watch the AI recommendations populate live.
  5. Switch to the Smart Budget tab, customize values, and view the breakdown.
  6. Switch to the Timeline tab to inspect the generated checklist.

### 3. User: Booking Payment Flow
- **Script:** `record-03-user-payment.js`
- **Actions:**
  1. Login as `customer@example.com`.
  2. Go to My Bookings and find a booking with status `Approved`.
  3. Click "Pay Now".
  4. Tour the checkout page (billing detail summary, total amount).
  5. Complete checkout (simulated redirect/success state).

### 4. Vendor: Profile Setup & Service Creation
- **Script:** `record-04-vendor-profile-setup.js`
- **Actions:**
  1. Login as a new/test vendor user (e.g., `vendor@example.com`).
  2. Go to Vendor Profile, update address details, business description, and service areas.
  3. Go to Services tab, click "Add Service", fill in service type, price, setup duration, and save.

### 5. Vendor: Dashboard Tour & Booking Approval
- **Script:** `record-05-vendor-bookings.js`
- **Actions:**
  1. Login as `catering.abouelsid@placeholder.com` (seeded vendor).
  2. Tour the Vendor Dashboard (inspect revenue chart, booking status counters).
  3. Check customer Reviews and rating summary.
  4. Go to Booking Requests page.
  5. Click on a pending booking request and click "Accept Booking".

### 6. Vendor: Messaging, Real-time Chat & Earnings
- **Script:** `record-06-vendor-chat-earnings.js`
- **Actions:**
  1. Login as `catering.abouelsid@placeholder.com`.
  2. Open the Chat sidebar / Messenger page.
  3. Select an active customer and send a reply message.
  4. Navigate to Earnings/Wallet tab, inspect transaction history and pending payouts.

### 7. Admin: Operations & Vendor Management
- **Script:** `record-07-admin-dashboard.js`
- **Actions:**
  1. Login as `admin@example.com`.
  2. Go to Admin Dashboard, view global stats (active users, total revenue, pending vendors).
  3. Navigate to Vendor Approvals list, view pending applications, approve/suspend a vendor.
  4. View Activity Log / System Audit Trail.
  5. Open Payout Requests management, check vendor payout statuses.

### 8. Admin: Taxonomy Page Tour (Very Important)
- **Script:** `record-08-admin-taxonomy.js`
- **Actions:**
  1. Login as `admin@example.com`.
  2. Navigate to Admin Taxonomy management page.
  3. Tour the categories, subcategories, and service attributes structure.
  4. Edit or add a mock taxonomy item to show how taxonomy defines the system's dynamic fields.

### 9. Mobile Responsiveness Showcase
- **Script:** `record-09-mobile-responsiveness.js`
- **Actions:**
  1. Set viewport to a mobile device (e.g., iPhone 14 Pro viewport: $393 \times 852$, with touch events enabled).
  2. Open the homepage, navigate through the burger menu.
  3. Browse vendors and services on mobile, demonstrating the responsive grid.
  4. Open the Event Studio on mobile and verify the mobile layout.

### 10. Real-time Split-Screen Chat & Notifications Demo
- **Script:** `record-10-split-chat-notifications.js`
- **Actions:**
  1. Launch two separate browser contexts simultaneously:
     - **Context A (Left Half):** Viewport $640 \times 900$, login as `customer@example.com`.
     - **Context B (Right Half):** Viewport $640 \times 900$, login as `catering.abouelsid@placeholder.com`.
  2. Lay the browser contexts next to each other (or run them and switch focus in the recording to show the live update).
  3. Customer sends a booking request -> Vendor receives a real-time notification badge increment on their screen instantly.
  4. Customer opens chat and sends a message -> Vendor's messenger window receives the message in real-time.
  5. Vendor replies -> Customer's chat window shows the message instantly.

---

## Verification & Execution Plan

- We will write these scripts in `C:\Users\KAITECH\Desktop\Amira Gabr\a\epichub\GraduationProject\verify-shots\scripts`.
- We will install Playwright in the project root if it is not already installed.
- We will provide a master script `run-recording-suite.js` to execute all scripts in sequence and save the resulting video recordings.
