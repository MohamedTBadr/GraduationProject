# Frontend API Integration Guide (New Features)

This document outlines the new API endpoints and data structures added to support Advanced Search, AI Planning, and Collaborative Features.

---

## 🔍 1. Advanced Search (Lucene.NET)

High-performance fuzzy search and advanced filtering for Vendors and Services.

### Vendors
**Endpoint:** `GET /api/Vendor`  
**Base Parameters:**
*   `searchTerm` (string): Fuzzy search across Name, Bio, and Services.
*   `category` (string): Filter by vendor category (e.g., "Catering").
*   `city` (string): Filter by location.
*   `minPrice` (decimal): Minimum price point.
*   `maxPrice` (decimal): Maximum price point.

### Services
**Endpoint:** `GET /api/Service`  
**Base Parameters:**
*   `searchTerm` (string): Fuzzy search across Service Name and Description.
*   `serviceTypeId` (Guid): Filter by specific Service Type.
*   `minPrice` / `maxPrice` (decimal): Price range filtering.

---

## 🤖 2. AI Event Planning Tools

### Smart Budget Allocation
Suggests budget portions based on event type and total amount.

**Endpoint:** `POST /api/AI/budget-allocation`  
**Request Body:**
```json
{
  "totalBudget": 50000,
  "eventTypeName": "Wedding"
}
```
**Response Object:**
```json
{
  "totalBudget": 50000,
  "eventType": "Wedding",
  "categories": [
    {
      "name": "Venue",
      "amount": 20000,
      "percentage": 40,
      "description": "Premium ballroom and setup."
    }
  ],
  "advice": "Consider booking the venue 6 months in advance for better rates."
}
```

### AI Event Timeline
Generates a minute-by-minute day-of-event schedule.

**Endpoint:** `POST /api/AI/event-timeline/{eventId}`  
**Response Object:**
```json
{
  "eventId": "guid",
  "eventTitle": "Summer Wedding",
  "timeline": [
    {
      "time": "06:00 PM",
      "activity": "Guest Arrival & Welcome Drinks",
      "duration": "1 hour",
      "importance": "High"
    }
  ],
  "planningNotes": "Ensure the catering team arrives 2 hours early."
}
```

### Vendor Vibe Summary
AI-generated summary of customer reviews for a specific vendor.

**Endpoint:** `GET /api/Vendor/{id}/vibe`  
**Response Object:**
```json
{
  "vendorId": "guid",
  "summary": "Highly praised for punctuality and creative decor.",
  "keyStrengths": ["Professionalism", "Timing", "Creativity"],
  "sentiment": "Positive"
}
```

---

## 👥 3. Collaborative Event Spaces

Invite family, friends, or planners to help manage an event.

### Invite Collaborator
**Endpoint:** `POST /api/Event/{id}/collaborators`  
**Request Body:**
```json
{
  "userEmailOrName": "user@example.com",
  "role": "Editor" // or "Viewer"
}
```

### List Collaborators
**Endpoint:** `GET /api/Event/{id}/collaborators`

### Remove Collaborator
**Endpoint:** `DELETE /api/Event/{id}/collaborators/{userId}`

---

## ⚙️ 4. Maintenance (Admin Only)

### Rebuild Search Index
Triggers a full rebuild of the Lucene search index from the database.

**Endpoint:** `POST /api/Search/rebuild`

---

## 💡 Integration Tips
1.  **Idempotency:** All `POST` and `PUT` requests support the `X-Idempotency-Key` header to prevent duplicate operations.
2.  **Caching:** High-traffic data (Vendor lists, Service Types) is cached using Redis. Use cache-busting headers if real-time data is strictly required.
3.  **Images:** The AI response for `budget-allocation` and `event-timeline` is pure JSON. Ensure your frontend can handle the hierarchical lists for display.
