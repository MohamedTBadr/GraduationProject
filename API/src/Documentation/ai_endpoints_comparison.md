# AI Endpoints — What Each Tries to Do & Should You Remove One?

---

## What Each Endpoint Is Trying to Do

### `POST /api/event/createEventByAI/{eventId}`
> **"Plan my whole event for me."**

The user already created an event (budget, guest count, event type). They want the AI to **automatically select a set of services** from the catalog and compose a complete event package.

**Flow:**
```
Event (budget + type) → SQL: all services where price < budget
    → Llama: "pick the best combo, stay within budget"
        → Full plan: selected services, cost breakdown, tips
```

**What the user gets:**
- A complete list of services to book
- Total cost and remaining budget
- A plan summary and pro tips

---

### `GET /api/AI/clients-like-you/{eventId}`
> **"What should I book next, based on people like me?"**

The user has an event in progress and has already booked some services. They want **personalized suggestions** for what to add next, informed by what similar users booked.

**Flow:**
```
User's booking history → Lucene: find similar users
    → Their booked services (filtered: exclude already booked)
        → Llama: "rank the best 3 candidates with reasoning"
            → 3 recommendations + human-readable explanation
```

**What the user gets:**
- 3 specific service suggestions
- Reasoning like "People planning similar weddings also booked..."

---

## Do They Overlap?

| Question | createEventByAI | clients-like-you |
|---|---|---|
| Uses Llama? | ✅ | ✅ |
| Recommends services? | ✅ | ✅ |
| Personalized to user? | ❌ (event only) | ✅ (booking history) |
| Triggered at start? | ✅ (fresh event, no bookings yet) | ❌ (needs booking history) |
| Triggered mid-event? | ❌ (not useful if you already booked) | ✅ (designed for this) |
| Outputs actionable items? | ✅ Full plan | ✅ Top 3 next picks |
| Uses Lucene? | ❌ | ✅ |

They **do overlap in intent** (both suggest services), but they **target different moments in the user journey**.

---

## Weaknesses of Each

### `createEventByAI` — Weaknesses
- **No personalization.** Budget filter is extremely blunt — `price < budget` returns every cheap service in the DB regardless of quality or relevance.
- **Not user-aware.** Same event type + same budget = same candidates for every user.
- **AI context is poor.** Llama receives a raw list of services (potentially dozens) with no relevance ranking — the AI is doing what a smart filter should do.
- **Doesn't scale well.** As the service catalog grows, the candidate list grows uncontrolled.
- **Output is raw string.** `aiResult.Value` is returned as an untyped string — no deserialization, no validation, inconsistent if Llama wraps in markdown.

### `clients-like-you` — Weaknesses
- **Cold start problem.** If the user has no booking history, it falls back to generic Llama suggestions (no personalization at all).
- **Requires Lucene UserProfile index to be warm.** If the Hangfire sync job hasn't run, similar users won't be found.
- **Only returns 3 items.** Doesn't compose a full plan — user still needs to manually book each.
- **Returns VendorId as ServiceId** — the consumer needs to know this is a VendorId, not a ServiceId.

---

## Verdict: Should You Remove One?

> [!IMPORTANT]
> **Keep both — but they serve different stages. Fix `createEventByAI`'s data quality problem.**

### Why keep both:

| Stage | Right endpoint |
|---|---|
| User just created event, wants a full plan immediately | `createEventByAI` |
| User is mid-planning and wants smart next suggestions | `clients-like-you` |

They are **complementary**, not duplicates. Removing either removes a user journey.

---

## What to Fix Instead

### Fix `createEventByAI` — it has a real quality problem

> [!WARNING]
> The current SQL filter `price < budget` sends **every cheap service in the DB** to Llama. This is wasteful and produces mediocre plans.

**Better approach — replace `AIFilterAsync` with Lucene:**

```csharp
// Instead of this (returns everything under budget):
var servicesResult = await serviceManager.ServiceService.AIFilterAsync(request, cancellationToken);

// Do this (returns relevant services by event type + budget):
var serviceIds = await searchService.SearchServicesAsync(
    query: eventObject.EventTypeName,
    serviceTypeId: null,
    minPrice: null,
    maxPrice: eventObject.TotalBudget
);
```

This makes the candidate list **relevant** before Llama sees it, dramatically improving plan quality.

### Fix `clients-like-you` — ServiceId naming

> [!NOTE]  
> `RecommendationItem.ServiceId` actually contains a `VendorId`. Rename to `VendorId` or return the actual `ServiceId` by doing a lookup in the candidate list before sending to Llama.

---

## Summary

```
createEventByAI  →  "Build me a plan from scratch"  →  Keep ✅ (fix data quality)
clients-like-you →  "What should I add next?"        →  Keep ✅ (it works well)
```

Neither should be removed. They cover different moments in the user lifecycle.
The real issue is that `createEventByAI` currently feeds Llama with a low-quality, unfiltered candidate set. Fix that and both endpoints become genuinely useful and non-overlapping.
