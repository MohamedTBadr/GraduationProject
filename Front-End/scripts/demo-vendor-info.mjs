/**
 * Print demo vendor account details and verify staging data.
 * Run: node scripts/demo-vendor-info.mjs
 */
const API = 'https://epichub-gateway-drfbgkb8bphrdfdw.francecentral-01.azurewebsites.net/api';

export const DEMO_VENDOR = {
  id: '30b2656a-219e-4d43-9873-691e400b6f40',
  email: 'catering.cilantro@placeholder.com',
  password: 'Vendor@123',
  businessName: 'Cilantro Catering Excellence',
};

export const DEMO_CUSTOMER = {
  email: 'customer@example.com',
  password: 'Customer@123',
};

async function login(email, password) {
  const res = await fetch(`${API}/Authentication/Login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      IdempotencyKey: crypto.randomUUID(),
    },
    body: JSON.stringify({ email, password }),
  });
  const data = await res.json();
  if (!res.ok) throw new Error(`${email}: ${JSON.stringify(data)}`);
  const payload = JSON.parse(
    Buffer.from(data.accessToken.split('.')[1], 'base64url').toString()
  );
  const userId =
    payload.sub ||
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
  return { token: data.accessToken, userId, email: data.email };
}

async function getJson(token, path) {
  const res = await fetch(`${API}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  const text = await res.text();
  try {
    return { status: res.status, body: JSON.parse(text) };
  } catch {
    return { status: res.status, body: text };
  }
}

function unwrap(res) {
  return res?.value ?? res?.Value ?? res;
}

async function main() {
  console.log('\n=== EpicHub demo vendor ===\n');
  console.log('Vendor login:', DEMO_VENDOR.email, '/', DEMO_VENDOR.password);
  console.log('Customer login:', DEMO_CUSTOMER.email, '/', DEMO_CUSTOMER.password);
  console.log('Public profile: /vendor/' + DEMO_VENDOR.id);
  console.log('');

  const vendor = await login(DEMO_VENDOR.email, DEMO_VENDOR.password);

  const profile = unwrap((await getJson(vendor.token, `/Vendor/${DEMO_VENDOR.id}`)).body);
  const bookings = (await getJson(vendor.token, '/Vendor/bookings')).body;
  const convs = (await getJson(vendor.token, '/Chat/conversations')).body;
  const pkgRes = unwrap((await getJson(vendor.token, `/Package?vendorId=${DEMO_VENDOR.id}`)).body);
  const packages = pkgRes?.items ?? pkgRes?.Items ?? [];

  const services = profile?.services ?? profile?.Services ?? [];
  const bookingList = Array.isArray(bookings) ? bookings : [];

  console.log('Profile');
  console.log('  Name:   ', profile?.businessName ?? profile?.BusinessName);
  console.log('  Phone:  ', profile?.phone ?? profile?.Phone ?? '(not in response)');
  console.log('  Rating: ', profile?.rating ?? profile?.Rating);
  console.log('  About:  ', (profile?.description ?? profile?.Description ?? '').slice(0, 80) + '…');
  console.log('');
  console.log('Counts');
  console.log('  Services:      ', services.length);
  console.log('  Packages:      ', packages.length);
  console.log('  Bookings:      ', bookingList.length);
  console.log('  Conversations: ', Array.isArray(convs) ? convs.length : 0);

  if (bookingList.length) {
    console.log('\nBookings');
    for (const b of bookingList) {
      console.log(
        `  - ${b.BookingStatus ?? b.bookingStatus}: ${b.ServiceName ?? b.serviceName} (${b.EventTitle ?? b.eventTitle})`
      );
    }
  }

  if (Array.isArray(convs) && convs[0]) {
    const last = convs[0].lastMessage?.content ?? convs[0].LastMessage?.content;
    console.log('\nChat');
    console.log('  With:', convs[0].otherUserName ?? convs[0].OtherUserName);
    console.log('  Last: ', last ? `"${last.slice(0, 60)}…"` : '(none)');
  }

  console.log('\nSee Front-End/DEMO_VENDOR.md for full walkthrough.\n');
}

main().catch((e) => {
  console.error('Error:', e.message);
  process.exit(1);
});
