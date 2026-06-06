/**
 * Automated chat smoke test — REST + SignalR against deployed API.
 * Run: node scripts/chat-smoke-test.mjs
 */
import * as signalR from '@microsoft/signalr';
import { randomUUID } from 'crypto';

const API = 'https://epichub-gateway-drfbgkb8bphrdfdw.francecentral-01.azurewebsites.net/api';
const HUB = 'https://epichub-gateway-drfbgkb8bphrdfdw.francecentral-01.azurewebsites.net/Hub/chatHub';

const ACCOUNTS = {
  customer: { email: 'customer@example.com', password: 'Customer@123' },
  vendor: { email: 'vendor@example.com', password: 'Vendor@123' },
};

const results = [];

function pass(step, detail = '') {
  results.push({ step, ok: true, detail });
  console.log(`✅ ${step}${detail ? ` — ${detail}` : ''}`);
}

function fail(step, detail = '') {
  results.push({ step, ok: false, detail });
  console.log(`❌ ${step}${detail ? ` — ${detail}` : ''}`);
}

function normalizeConversation(raw) {
  if (raw.userId) return raw;
  const lastMsg = raw.lastMessage;
  const lastMessageText =
    typeof lastMsg === 'string' ? lastMsg : lastMsg?.content;
  const lastMessageAt =
    typeof lastMsg === 'object' && lastMsg?.sentAt
      ? String(lastMsg.sentAt)
      : raw.lastMessageAt;
  return {
    userId: String(raw.otherUserId ?? raw.userId ?? ''),
    userName: raw.otherUserName ?? raw.userName,
    lastMessage: lastMessageText,
    lastMessageAt,
    unreadCount: raw.unreadCount ?? 0,
  };
}

function decodeJwt(token) {
  const payload = token.split('.')[1];
  return JSON.parse(Buffer.from(payload, 'base64url').toString());
}

async function login(label, creds) {
  const res = await fetch(`${API}/Authentication/Login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      IdempotencyKey: randomUUID(),
    },
    body: JSON.stringify({ email: creds.email, password: creds.password }),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`${label} login failed (${res.status}): ${body}`);
  }
  const data = await res.json();
  const claims = decodeJwt(data.accessToken);
  const userId =
    claims.sub ||
    claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
  return { token: data.accessToken, userId, email: data.email, name: data.name };
}

async function getConversations(token) {
  const res = await fetch(`${API}/Chat/conversations`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`conversations ${res.status}: ${await res.text()}`);
  return res.json();
}

async function getMessages(token, otherUserId) {
  const res = await fetch(`${API}/Chat/messages/${otherUserId}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`messages ${res.status}: ${await res.text()}`);
  return res.json();
}

function connectHub(token) {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB, {
      accessTokenFactory: () => token,
      transport: signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Debug)
    .build();
}

async function waitForEvent(connection, event, timeoutMs = 15000) {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error(`Timeout waiting for ${event}`)), timeoutMs);
    connection.on(event, (payload) => {
      clearTimeout(timer);
      resolve(payload);
    });
  });
}

async function main() {
  console.log('\n=== Chat smoke test ===\n');

  let customer, vendor;
  try {
    customer = await login('customer', ACCOUNTS.customer);
    pass('Login customer', customer.email);
  } catch (e) {
    fail('Login customer', e.message);
    return summary();
  }

  try {
    vendor = await login('vendor', ACCOUNTS.vendor);
    pass('Login vendor', vendor.email);
  } catch (e) {
    fail('Login vendor', e.message);
    return summary();
  }

  // REST conversations — raw API shape
  let rawConversations;
  try {
    rawConversations = await getConversations(customer.token);
    pass('GET /Chat/conversations (customer)', `${rawConversations.length} thread(s)`);
  } catch (e) {
    fail('GET /Chat/conversations (customer)', e.message);
    return summary();
  }

  if (rawConversations.length > 0) {
    const raw = rawConversations[0];
    const hasApiShape = !!(raw.otherUserId || raw.otherUserName);
    if (hasApiShape) {
      pass('API returns ConversationDto shape', `otherUserId=${raw.otherUserId}`);
    } else {
      fail('API ConversationDto shape', JSON.stringify(raw).slice(0, 120));
    }

    const mapped = normalizeConversation(raw);
    if (mapped.userId && mapped.userName) {
      pass('DTO normalization', `${mapped.userName} (${mapped.userId.slice(0, 8)}…)`);
    } else {
      fail('DTO normalization', JSON.stringify(mapped));
    }
  } else {
    pass('API returns ConversationDto shape', 'no threads yet (first-message flow)');
  }

  // SignalR connect
  const vendorHub = connectHub(vendor.token, 'vendor');
  const customerHub = connectHub(customer.token, 'customer');

  try {
    await vendorHub.start();
    pass('SignalR connect (vendor)');
  } catch (e) {
    fail('SignalR connect (vendor)', e.message);
    return summary();
  }

  try {
    await customerHub.start();
    pass('SignalR connect (customer)');
  } catch (e) {
    fail('SignalR connect (customer)', e.message);
    await vendorHub.stop();
    return summary();
  }

  // MarkAsRead on existing message (hub method health check)
  try {
    const history = await getMessages(customer.token, rawConversations[0].otherUserId);
    const unread = history.find((m) => m.receiverId === customer.userId && !m.isRead);
    if (unread?.id) {
      await customerHub.invoke('MarkAsRead', unread.id);
      pass('MarkAsRead via hub', unread.id.slice(0, 8) + '…');
    } else {
      pass('MarkAsRead via hub', 'skipped (no unread in history)');
    }
  } catch (e) {
    fail('MarkAsRead via hub', e.message);
  }

  // Send message customer → vendor
  const testContent = `Smoke test ${new Date().toISOString()}`;
  const receivePromise = waitForEvent(vendorHub, 'ReceiveMessage');

  try {
    await customerHub.invoke('SendMessage', vendor.userId, testContent);
    pass('SendMessage via hub (customer → vendor)');
  } catch (e) {
    const detail = e.message + (e.innerErrors ? ` | ${JSON.stringify(e.innerErrors)}` : '');
    fail('SendMessage via hub', detail);
    // Retry with alternate receiver (existing conversation partner)
    if (rawConversations.length > 0) {
      const altId = rawConversations[0].otherUserId;
      try {
        await customerHub.invoke('SendMessage', altId, testContent + ' (alt)');
        pass('SendMessage to existing conversation partner', String(altId).slice(0, 8) + '…');
      } catch (e2) {
        fail('SendMessage to existing conversation partner', e2.message);
      }
    }
    await customerHub.stop();
    await vendorHub.stop();
    return summary();
  }

  try {
    const received = await receivePromise;
    if (received?.content === testContent) {
      pass('Vendor received message in real time', `"${received.content.slice(0, 40)}"`);
    } else {
      fail('Vendor received message', `got: ${JSON.stringify(received)}`);
    }
  } catch (e) {
    fail('Vendor received message in real time', e.message);
  }

  // Customer echo
  try {
    const echoPromise = waitForEvent(customerHub, 'ReceiveMessage', 5000);
    // already sent; check history instead if echo missed
    const history = await getMessages(customer.token, vendor.userId);
    const found = history.some((m) => m.content === testContent);
    if (found) {
      pass('Message persisted in REST history', `${history.length} message(s)`);
    } else {
      fail('Message persisted in REST history', `not found in ${history.length} msgs`);
    }
    echoPromise.catch(() => {}); // ignore echo timeout
  } catch (e) {
    fail('Message persisted in REST history', e.message);
  }

  // Vendor conversations after send
  try {
    const vendorConvs = (await getConversations(vendor.token)).map(normalizeConversation);
    const thread = vendorConvs.find((c) => c.userId === customer.userId);
    if (thread?.userName) {
      pass('Vendor conversation list', `with ${thread.userName}, unread=${thread.unreadCount}`);
    } else {
      fail('Vendor conversation list', 'customer thread missing or unnamed');
    }
  } catch (e) {
    fail('Vendor conversation list', e.message);
  }

  await customerHub.stop();
  await vendorHub.stop();
  return summary();
}

function summary() {
  const passed = results.filter((r) => r.ok).length;
  const failed = results.filter((r) => !r.ok).length;
  console.log(`\n=== Result: ${passed} passed, ${failed} failed ===\n`);
  process.exit(failed > 0 ? 1 : 0);
}

main().catch((e) => {
  console.error('Fatal:', e);
  process.exit(1);
});
