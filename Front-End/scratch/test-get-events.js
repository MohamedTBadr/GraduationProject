const http = require('http');

async function test() {
    console.log("Logging in as the original user to get token...");
    // Replace the email and password with generic values, or we can just register a new one and create an event.
    // Let's create a NEW user, Create an event, AND THEN fetch the events, to explicitly reproduce the exact trace of the bug!
    
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
    console.log("Token payload:", payload);
    const userId = payload.sub || payload.nameid || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.id;
    
    console.log("User registered. ID:", userId);

    // 2. Fetch Categories to get a valid CategoryId
    const catRes = await fetch("http://localhost:5000/api/Category", {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const catData = await catRes.json();
    const categoryId = Array.isArray(catData) ? catData[0].id : catData.value[0].id;

    // 3. Trigger POST /api/Event (This will return 500 as we know)
    console.log("Skipping POST Request...");
    /*
    const eventBody = {
        userId: userId,
        title: "Test Event that breaks DB",
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
    
    console.log("Create Event Response Status:", postRes.status);
    console.log(await postRes.text());
    */

    // 4. Trigger GET /api/Event/user/{userId}
    console.log("Fetching User Events...");
    const getRes = await fetch(`http://localhost:5000/api/Event/user/${userId}`, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    console.log("Get User Events Response Status:", getRes.status);
    console.log("Get User Events Body:", await getRes.text());
}

test().catch(console.error);
