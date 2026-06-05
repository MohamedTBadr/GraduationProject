const { chromium } = require('C:/Users/win11/AppData/Roaming/npm/node_modules/playwright');
const path = require('path');
const fs = require('fs');

const BASE = 'https://epichubweb-g9h9a3a8bxafekdm.francecentral-01.azurewebsites.net';
const SHOTS = 'C:/Users/win11/Documents/GitHub/GraduationProject/verify-shots';

if (!fs.existsSync(SHOTS)) fs.mkdirSync(SHOTS, { recursive: true });

async function shot(page, name) {
  const p = path.join(SHOTS, name + '.png');
  await page.screenshot({ path: p, fullPage: false });
  console.log('  [shot]', name);
  return p;
}

async function goto(page, url) {
  await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 30000 });
  await page.waitForTimeout(5000); // wait for Angular + API calls
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 } });
  const page = await ctx.newPage();

  const errors = [];
  page.on('pageerror', e => errors.push(e.message));

  try {
    // ─── Login ─────────────────────────────────────────────────────────
    console.log('\n=== Login ===');
    await goto(page, BASE);
    const loginBtn = page.locator('.btn-nav-login, button:has-text("Login")').first();
    await loginBtn.click();
    await page.waitForTimeout(1500);
    await page.locator('input[type="email"]').first().fill('customer@example.com');
    await page.locator('input[type="password"]').first().fill('Customer@123');
    await page.locator('button[type="submit"]').first().click();
    await page.waitForTimeout(6000);
    await shot(page, '00-after-login');
    console.log('URL:', page.url());
    const loggedIn = page.url().includes('/user/');
    console.log('Auth guard allowed access:', loggedIn);

    // ─── Step 1: My Events ─────────────────────────────────────────────
    console.log('\n=== Step 1: My Events ===');
    await goto(page, BASE + '/user/my-events');
    await shot(page, '01-my-events');
    console.log('URL:', page.url());
    const onEvents = page.url().includes('/user/my-events');
    console.log('On my-events:', onEvents);

    if (onEvents) {
      const eventTabs = await page.locator('.event-tab-item').allTextContents().catch(() => []);
      console.log('Event tabs:', eventTabs.map(t => t.trim().replace(/\s+/g,' ')).filter(t => t));

      const payNowSection = await page.locator('.pay-now-section').count();
      console.log('Pay-now section present:', payNowSection > 0);

      if (payNowSection > 0) {
        const payBtn = page.locator('.pay-now-section button').first();
        const payTxt = await payBtn.textContent().catch(() => '');
        const payDisabled = await payBtn.isDisabled().catch(() => null);
        console.log('Pay Now text:', payTxt.trim().replace(/\s+/g,' ').substring(0,80));
        console.log('Pay Now disabled:', payDisabled);

        const payHints = await page.locator('.pay-hint').allTextContents().catch(() => []);
        console.log('Pay hints:', payHints.map(h => h.trim().replace(/\s+/g,' ').substring(0,80)));
      }

      const serviceTabs = await page.locator('.event-service-tab').allTextContents().catch(() => []);
      console.log('Service tabs:', serviceTabs.map(t => t.trim()));

      const badges = await page.locator('.vendors-list .badge').allTextContents().catch(() => []);
      console.log('Service badges:', badges.map(b => b.trim()).filter(b => b));

      const collabHeading = await page.locator('h3:has-text("Collaborators")').count();
      console.log('Collaborators heading:', collabHeading > 0);
      const inviteInput = await page.locator('input[placeholder="Email or name"]').count();
      console.log('Collaborator input:', inviteInput > 0);
      const inviteBtn = await page.locator('button:has-text("Invite")').count();
      console.log('Invite button:', inviteBtn > 0);
    }

    // ─── Step 2: Probe — Approved-only tab ────────────────────────────
    console.log('\n=== Step 2 (probe): Approved-only tab ===');
    if (page.url().includes('/user/my-events')) {
      const approvedTab = page.locator('button:has-text("Approved only")');
      if (await approvedTab.count() > 0) {
        await approvedTab.click();
        await page.waitForTimeout(600);
        const afterBadges = await page.locator('.vendors-list .badge').allTextContents().catch(() => []);
        const emptyHint = await page.locator('.empty-services-hint').textContent().catch(() => null);
        console.log('Approved-only tab badges:', afterBadges.map(b => b.trim()));
        console.log('Empty hint:', emptyHint ? emptyHint.trim() : '(none)');
        await shot(page, '02-approved-tab');
      } else {
        console.log('No Approved-only tab visible');
      }
    }

    // ─── Step 3: My Bookings ───────────────────────────────────────────
    console.log('\n=== Step 3: My Bookings ===');
    await goto(page, BASE + '/user/bookings');
    await shot(page, '03-my-bookings');
    console.log('URL:', page.url());
    const onBookings = page.url().includes('/user/bookings');
    console.log('On bookings:', onBookings);

    if (onBookings) {
      const orderCards = await page.locator('[class*="order-card"], .bk-order-card').count();
      console.log('Order cards:', orderCards);

      const payBtns = await page.locator('button').all();
      let foundAny = false;
      for (const btn of payBtns) {
        const txt = await btn.textContent().catch(() => '');
        if (txt.toLowerCase().includes('pay') || txt.toLowerCase().includes('await') || txt.toLowerCase().includes('approv')) {
          const disabled = await btn.isDisabled();
          console.log('  Button:', txt.trim().replace(/\s+/g,' ').substring(0,70), '| disabled:', disabled);
          foundAny = true;
        }
      }
      if (!foundAny) console.log('  No pay-related buttons found');

      const hints = await page.locator('[class*="pending-hint"]').allTextContents().catch(() => []);
      console.log('Booking hints:', hints.map(h => h.trim().replace(/\s+/g,' ').substring(0,80)));

      const chips = await page.locator('[class*="bk-status-chip"]').allTextContents().catch(() => []);
      console.log('Status chips:', chips.map(c => c.trim()).filter(c => c));
    }

    // ─── Step 4: Checkout (billing form removed) ──────────────────────
    console.log('\n=== Step 4: Checkout page ===');
    await goto(page, BASE + '/checkout/00000000-0000-0000-0000-000000000000');
    await shot(page, '04-checkout');
    console.log('URL:', page.url());
    const forms = await page.locator('form').count();
    console.log('Forms on page:', forms, '(expected 0)');
    const billingText = await page.locator('.billing-card, [class*="billing"]').count();
    console.log('Billing section present:', billingText > 0);
    const detailRows = await page.locator('.detail-row').count();
    console.log('Detail rows:', detailRows, '(read-only billing fields)');

    // ─── Step 5 (probe): Pay Now click when disabled ──────────────────
    console.log('\n=== Step 5 (probe): Pay Now click ===');
    await goto(page, BASE + '/user/my-events');
    const payBtn2 = page.locator('.pay-now-section button').first();
    if (await payBtn2.count() > 0) {
      const isDisabled = await payBtn2.isDisabled();
      console.log('Pay Now disabled:', isDisabled);
      if (!isDisabled) {
        console.log('Enabled! Clicking...');
        await payBtn2.click();
        await page.waitForTimeout(5000);
        await shot(page, '05-after-pay-click');
        console.log('URL after click:', page.url());
        console.log('On checkout:', page.url().includes('/checkout'));
      } else {
        console.log('Correctly disabled; click guarded by approvedItems.length === 0 check');
      }
    } else {
      console.log('No pay-now section (possibly no events)');
    }

    console.log('\n=== Page errors ===');
    console.log('Errors:', errors.length > 0 ? errors.slice(0,5) : 'none');

  } catch (e) {
    console.log('[SCRIPT ERROR]', e.message);
    await shot(page, 'error-state').catch(() => {});
  }

  await browser.close();
  console.log('\nDone.');
})();
