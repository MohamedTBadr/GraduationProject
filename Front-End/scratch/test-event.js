const http = require('http');

async function test() {
    console.log("Registering temp user...");
    // 1. Register a fake user to get a token
    const regRes = await fetch("http://localhost:5000/api/Authentication/Register", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "IdempotencyKey": crypto.randomUUID()
        },
        body: JSON.stringify({
            name: `Test User ${Date.now()}`,
            email: `test_${Date.now()}@test.com`,
            password: "Password123!"
        })
    });
    
    const regData = await regRes.json();
    if(!regRes.ok) throw new Error(JSON.stringify(regData));
    
    const token = regData.value.accessToken;
    // get user ID from token
    const payload = JSON.parse(Buffer.from(token.split('.')[1], 'base64').toString());
    const userId = payload.sub || payload.nameid || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.id;
    
    console.log("User registered. ID:", userId);

    // 2. Fetch Categories to get a valid CategoryId
    console.log("Fetching Categories...");
    const catRes = await fetch("http://localhost:5000/api/Category", {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const catData = await catRes.json();
    const categoryId = Array.isArray(catData) ? catData[0].id : catData.value[0].id;

    console.log("Found CategoryId:", categoryId);

    // 3. Trigger POST /api/Event
    console.log("Creating Event POST Request...");
    const eventBody = {
        userId: userId,
        title: "Test Error StackTrace Event",
        categoryId: categoryId,
        eventDate: new Date().toISOString(),
        totalBudget: 1000,
        guestCount: 50,
        notes: "Test",
        location: { street: "123", city: "Cairo", state: "Cairo" }
    };
    
    const postRes = await fetch("http://localhost:5000/api/Event", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`,
            "IdempotencyKey": crypto.randomUUID()
        },
        body: JSON.stringify(eventBody)
    });
    
    const status = postRes.status;
    const bodyText = await postRes.text();
    console.log("Response Status:", status);
    console.log("Response Body (first 1000 chars):", bodyText.substring(0, 1000));
}

test().catch(console.error);
