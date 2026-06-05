# 🔒 Secure Cookie Migration Plan (XSS & CSRF Prevention)

Storing sensitive credentials like refresh tokens in client-side storage (`localStorage` or `sessionStorage`) exposes them to theft via **Cross-Site Scripting (XSS)**. If an attacker injects a malicious script (e.g., via a compromised npm package or unescaped user input), they can execute `localStorage.getItem('eventora_refresh_token')` and steal the token.

The industry-standard solution is to store **Refresh Tokens** inside an **`HttpOnly`**, **`Secure`**, and **`SameSite`** Cookie. This document outlines the exact changes required by both the Backend and Frontend developers to migrate to this secure architecture.

---

## 🛠️ Section A: What the Backend Developer Needs to Do

The .NET Core Web API needs to set the cookie in the HTTP response headers when a user logs in, registers, or refreshes their token, and clear the cookie on logout.

### 1. Update the CORS Configuration
When using cookies, the browser requires cross-origin requests to be explicitly permitted to send credentials. The wildcard origin `*` is **prohibited** when `.AllowCredentials()` is enabled.

In `Program.cs` (or your startup configuration):
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://yourproductiondomain.com") // Must be explicit list
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 👈 CRITICAL: Must be enabled for cookies to work!
    });
});
```

---

### 2. Add Cookie Helpers in the Controller
In `AuthenticationController.cs` (under `Web.Api/Controllers/AuthenticationController.cs`), add two helper methods to manage appending and deleting the refresh token cookie:

```csharp
private void SetRefreshTokenCookie(string refreshToken)
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,        // 👈 Prevents client-side JS (XSS) from reading the cookie
        Secure = true,          // 👈 Ensures cookie is only sent over HTTPS (local localhost is also allowed)
        SameSite = SameSiteMode.Strict, // 👈 Mitigates CSRF attacks by not sending cookie in cross-site requests
        Expires = DateTime.UtcNow.AddDays(7), // Should match your actual RefreshToken lifetime
        Path = "/api/Authentication/RefreshToken" // 👈 Scope cookie strictly to the refresh endpoint only
    };

    Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
}

private void DeleteRefreshTokenCookie()
{
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/api/Authentication/RefreshToken"
    };

    Response.Cookies.Delete("refreshToken", cookieOptions);
}
```

---

### 3. Update Controller Action Methods

#### A. Login & Register Endpoint Updates
In `Login` and `Register` actions, extract the `RefreshToken` from the service response, append it as a cookie, and remove it from the JSON body to keep the response clean:

```csharp
[HttpPost("Login")]
[AllowAnonymous]
public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken)
{
    // ... validation logic ...
    var result = await ServiceManager.AuthenticationService.LogIn(loginRequest, cancellationToken);
    
    if (result.IsSuccess)
    {
        // 1. Set the RefreshToken as an HttpOnly secure cookie
        SetRefreshTokenCookie(result.Value.RefreshToken);

        // 2. Return UserResponse WITHOUT the RefreshToken field
        var secureResponse = new 
        {
            result.Value.name,
            result.Value.email,
            result.Value.AccessToken, // Keep AccessToken in memory/JSON
            result.Value.role
        };
        return Ok(secureResponse);
    }

    return result.ToActionResult();
}
```

#### B. RefreshToken Endpoint Update
Since JavaScript can no longer read the refresh token, the frontend will no longer send it in the request JSON payload. The backend will instead read it directly from the HTTP request cookies:

```csharp
[HttpPost("RefreshToken")]
[ProducesResponseType(200, Type = typeof(object))]
[ProducesResponseType(401)]
public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
{
    // 1. Read refresh token from the HttpOnly cookie
    var refreshToken = Request.Cookies["refreshToken"];
    if (string.IsNullOrWhiteSpace(refreshToken))
    {
        return Unauthorized(new { message = "Refresh token is missing or expired." });
    }

    try
    {
        // 2. Call your service using the token from the cookie
        var request = new RefreshTokenRequest { RefreshToken = refreshToken };
        var response = await ServiceManager.AuthenticationService.RefreshTokenAsync(request, cancellationToken);
       
        // 3. Set the newly generated refresh token as a cookie
        SetRefreshTokenCookie(response.RefreshToken);
        
        // 4. Return only the new AccessToken to the frontend
        return Ok(new 
        { 
            accessToken = response.AccessToken 
        });
    }
    catch (UnauthorizedException ex)
    {
        DeleteRefreshTokenCookie(); // Clear cookie on failure
        return Unauthorized(new { message = ex.Message });
    }
}
```

#### C. Logout Endpoint Update
When the user logs out, we need to instruct the browser to delete the cookie:

```csharp
[HttpPost("Logout")]
[Authorize]
public async Task<IActionResult> Logout(CancellationToken cancellationToken)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
    {
        return Unauthorized();
    }
    
    await ServiceManager.AuthenticationService.LogoutAsync(Guid.Parse(userId), cancellationToken);
    
    // Clear the secure cookie on logout
    DeleteRefreshTokenCookie();
    
    var result = Result<string>.Success("Logged out successfully");
    return Ok(result);
}
```

---

## 💻 Section B: What the Frontend Developer Needs to Do

The Angular frontend needs to configure the HTTP client to send and receive credentials (cookies) and remove refresh token storage logic from `localStorage`.

### 1. Update `AuthService` (`core/services/auth.service.ts`)

Modify the methods to set/read from cookies automatically via `withCredentials: true` and remove `eventora_refresh_token` references:

```typescript
// 1. Remove refresh token from local storage inside the constructor try/catch or helper methods
// localStorage.removeItem('eventora_refresh_token'); // ❌ No longer needed!

/** POST /Authentication/Login */
login(credentials: LoginCredentials): Observable<AuthResponse> {
  const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
  const body = JSON.stringify(credentials);

  return this.http
    .post<AuthApiResponse>(`${this.apiUrl}/Authentication/Login`, body, {
      headers,
      context: new HttpContext().set(SKIP_AUTH, true),
      withCredentials: true // 👈 Allows receiving the set-cookie header
    })
    .pipe(
      tap((res) => {
        // Only save session (name, email, role, accessToken) - NO refresh token is in the JSON!
        const session: UserSession = {
          name: res.name,
          email: res.email,
          role: res.role,
        };
        localStorage.setItem('eventora_session', JSON.stringify(session));
        localStorage.setItem('eventora_token', res.accessToken);
        this.currentUser.set(session);
      }),
      map((res) => ({
        name: res.name,
        email: res.email,
        role: res.role,
        token: res.accessToken
      }))
    );
}

/** POST /Authentication/RefreshToken */
refreshToken(): Observable<{ accessToken: string }> {
  // We no longer send the refresh token in the JSON body.
  // It is automatically attached by the browser from the HttpOnly cookie!
  return this.http
    .post<{ accessToken: string }>(
      `${this.apiUrl}/Authentication/RefreshToken`,
      {}, // 👈 Empty body
      {
        context: new HttpContext().set(SKIP_AUTH, true),
        withCredentials: true // 👈 CRITICAL: Tells the browser to send cookies with the cross-origin request
      }
    )
    .pipe(
      tap((res) => {
        localStorage.setItem('eventora_token', res.accessToken);
      })
    );
}

/** POST /Authentication/Logout */
logout(): void {
  // Clear frontend local state
  this.currentUser.set(null);
  localStorage.removeItem('eventora_session');
  localStorage.removeItem('eventora_token');
  
  // Call backend to revoke refresh token and delete cookie
  this.http.post(`${this.apiUrl}/Authentication/Logout`, {}, { withCredentials: true }).subscribe({
    next: () => {
      this.router.navigate(['/']);
    },
    error: () => {
      this.router.navigate(['/']);
    }
  });
}
```

---

### Summary Checklist for Both Developers

| Action Item | Role | Status |
|---|---|---|
| Explicit CORS Policy in `Program.cs` (`WithOrigins` + `AllowCredentials`) | **Backend** | 📋 Pending |
| Add helper methods `SetRefreshTokenCookie` and `DeleteRefreshTokenCookie` | **Backend** | 📋 Pending |
| Modify `Login` / `Register` to set cookie & exclude refresh token from JSON body | **Backend** | 📋 Pending |
| Modify `RefreshToken` to read from cookie instead of request body | **Backend** | 📋 Pending |
| Update Angular `withCredentials: true` in HTTP calls | **Frontend** | 📋 Pending |
| Remove local storage storage & retrieval of refresh tokens | **Frontend** | 📋 Pending |
