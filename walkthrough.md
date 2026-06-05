# Angular JWT Authentication — Walkthrough

We have successfully addressed all P1 to P7 issues, significantly enhancing security, stability, error handling, and reliability of the JWT Authentication system in the Angular frontend.

## Changes Made

### 1. AuthService Improvements (`core/services/auth.service.ts`)
- **P1 Crash safety:** Wrapped `JSON.parse` of `eventora_session` in a `try/catch` in the constructor. If corruption occurs, local storage is safely cleared instead of crashing the app.
- **P2 Concurrent Refresh Queue:** Created `refreshTokenOnce()` which returns a shared Observable using `shareReplay(1)` and `finalize`. Parallel `401` errors now share a single token refresh request instead of sending multiple refresh HTTP requests.
- **P3 HttpContextToken:** Declared `SKIP_AUTH` HttpContextToken to explicitly flag requests that should not receive authorization header injection or trigger refresh loops (such as Login, Register, and RefreshToken).
- **P4 Router navigation in logout:** Replaced `window.location.reload()` in `logout()` with clean Angular routing using `Router.navigate(['/'])`.
- **P7 Type-Safe registration:** Changed `register()` signature from `data: any` to `data: RegisterRequest` to enforce compilation-time validation of registration payload.

### 2. Interceptor Changes
- **Auth Interceptor (`core/interceptors/auth.interceptor.ts`):**
  - Updated to check `req.context.get(SKIP_AUTH)` to bypass authorization headers and loop checks cleanly.
  - Used `authService.refreshTokenOnce()` instead of `authService.refreshToken()` to queue concurrent requests.
  - Tagged token refresh errors with `{ handled: true }` so they propagate clearly without triggering redundant toast notifications.
- **Error Interceptor (`core/interceptors/error.interceptor.ts`):**
  - Added a bypass to skip displaying the `"Unauthorized Access"` toast when handling errors already flagged as `{ handled: true }` by the auth interceptor.

### 3. Cleanup
- **P6 Dead code removal:** Deleted unused class-based `JsonInterceptor` files (`json.interceptor.ts` and `json.interceptor.spec.ts`).

---

## Verification & Validation

- Verified that all changes compile successfully without errors.
