const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  page.setDefaultTimeout(30000);

  const apiCalls = [];
  page.on('request', req => {
    const url = req.url();
    if (url.includes('/Vendor') || url.includes('/Service') || url.includes('/VendorType') || url.includes('/ServiceType')) {
      apiCalls.push(url);
    }
  });
  page.on('response', async res => {
    const url = res.url();
    if (url.includes('/VendorType') && !url.match(/VendorType\//)) {
      const status = res.status();
      const body = await res.text().catch(() => '');
      console.log('  VendorType response status:', status, '| body length:', body.length);
    }
  });

  // ── Test 1: Vendor tab initial load — wait longer for Azure ──
  console.log('\nTEST1 - Loading vendor tab...');
  await page.goto('http://localhost:4200/explore?tab=vendors', { waitUntil: 'domcontentloaded', timeout: 15000 });
  await page.waitForTimeout(8000); // Azure API can be slow on cold start
  console.log('  API calls made:');
  apiCalls.forEach(u => console.log('   ', u));
  const vendorInitCall = apiCalls.find(u => u.includes('/Vendor') && !u.includes('VendorType'));
  console.log('  VendorType fired:', apiCalls.some(u => u.includes('/VendorType')));
  console.log('  Vendor API fired:', !!vendorInitCall);
  console.log('  Vendor URL:', vendorInitCall || '(none)');

  // ── Test 2: Check vendor type chips ──
  await page.click('text=Vendor Type');
  await page.waitForTimeout(500);
  const chips = await page.evaluate(() =>
    Array.from(document.querySelectorAll('.fp-chip')).map(e => e.textContent.trim())
  );
  console.log('\nTEST2 - Vendor type chips:', chips);
  const hasRealTypes = chips.some(c => c && c !== 'Loading…');
  console.log('  Dynamic chips loaded:', hasRealTypes ? 'YES ✅' : 'NO - API still loading');

  // ── Test 3: Select first vendor type → vendorTypeId in URL ──
  apiCalls.length = 0;
  const firstRealChip = chips.find(c => c && c !== 'Loading…');
  if (firstRealChip) {
    console.log('\nTEST3 - Selecting vendor type:', firstRealChip);
    await page.evaluate((name) => {
      const chip = Array.from(document.querySelectorAll('.fp-chip')).find(e => e.textContent.trim() === name);
      if (chip) chip.click();
    }, firstRealChip);
    await page.waitForTimeout(200);
    await page.evaluate(() => {
      const btn = document.querySelector('.fp-apply');
      if (btn) btn.click();
    });
    await page.waitForTimeout(5000);
    const vendorTypeCall = apiCalls.find(u => u.includes('/Vendor') && !u.includes('VendorType'));
    console.log('  Vendor API URL:', vendorTypeCall || '(none)');
    console.log('  vendorTypeId present:', vendorTypeCall && vendorTypeCall.includes('vendorTypeId=') ? 'YES ✅' : 'NO ❌');
    const vendorCards = await page.evaluate(() => document.querySelectorAll('app-vendor-card').length);
    console.log('  Vendor cards shown:', vendorCards);
  } else {
    console.log('\nTEST3 - SKIPPED: no vendor type chips loaded (API too slow)');
  }

  // ── Test 4: Location filter → city in URL ──
  apiCalls.length = 0;
  await page.keyboard.press('Escape');
  await page.waitForTimeout(300);
  await page.click('text=Location').catch(() => {});
  await page.waitForTimeout(500);
  await page.evaluate(() => {
    const chips = Array.from(document.querySelectorAll('.fp-chip'));
    const cairo = chips.find(e => e.textContent.trim() === 'Cairo');
    if (cairo) cairo.click();
  });
  await page.waitForTimeout(200);
  await page.evaluate(() => {
    const btns = Array.from(document.querySelectorAll('.fp-apply'));
    if (btns.length) btns[btns.length - 1].click();
  });
  await page.waitForTimeout(5000);
  const vendorLocCall = apiCalls.find(u => u.includes('/Vendor') && !u.includes('VendorType'));
  console.log('\nTEST4 - Vendor API with location:', vendorLocCall || '(none)');
  console.log('  city param present:', vendorLocCall && vendorLocCall.includes('city=') ? 'YES ✅' : 'NO ❌');

  // ── Test 5: Services tab → serviceTypeId ──
  apiCalls.length = 0;
  await page.click('text=Services');
  await page.waitForTimeout(5000);
  const svcInitCall = apiCalls.find(u => u.includes('/Service') && !u.includes('ServiceType'));
  console.log('\nTEST5 - Services init URL:', svcInitCall || '(none)');

  await page.click('text=Service Type').catch(() => {});
  await page.waitForTimeout(800);
  const svcChips = await page.evaluate(() =>
    Array.from(document.querySelectorAll('.fp-chip')).map(e => e.textContent.trim())
  );
  console.log('  Service type chips count:', svcChips.length, '| samples:', svcChips.slice(0, 3));

  apiCalls.length = 0;
  const firstSvcChip = svcChips.find(c => c && c !== 'Apply' && c !== 'Clear');
  if (firstSvcChip) {
    console.log('  Clicking service type:', firstSvcChip);
    await page.evaluate((name) => {
      const chip = Array.from(document.querySelectorAll('.fp-chip')).find(e => e.textContent.trim() === name);
      if (chip) chip.click();
    }, firstSvcChip);
    await page.waitForTimeout(5000);
    const svcCall = apiCalls.find(u => u.includes('/Service') && !u.includes('ServiceType'));
    console.log('  Service API URL:', svcCall || '(none)');
    console.log('  serviceTypeId present:', svcCall && svcCall.includes('serviceTypeId=') ? 'YES ✅' : 'NO ❌');
    const svcCards = await page.evaluate(() => document.querySelectorAll('.svc-card').length);
    console.log('  Service cards shown:', svcCards);
  }

  // ── Test 6: explore-services dedicated page (already known to work) ──
  apiCalls.length = 0;
  await page.goto('http://localhost:4200/explore-services', { waitUntil: 'domcontentloaded', timeout: 15000 });
  await page.waitForTimeout(5000);
  const esInitCall = apiCalls.find(u => u.includes('/Service') && !u.includes('ServiceType'));
  console.log('\nTEST6 - /explore-services init URL:', esInitCall || '(none)');

  await page.click('text=Service Type').catch(() => {});
  await page.waitForTimeout(800);
  const esChips = await page.evaluate(() =>
    Array.from(document.querySelectorAll('.fp-chip')).map(e => e.textContent.trim())
  );

  apiCalls.length = 0;
  const firstEsChip = esChips.find(c => c && c !== 'Apply' && c !== 'Clear');
  if (firstEsChip) {
    console.log('  Clicking:', firstEsChip);
    await page.evaluate((name) => {
      const chip = Array.from(document.querySelectorAll('.fp-chip')).find(e => e.textContent.trim() === name);
      if (chip) chip.click();
    }, firstEsChip);
    await page.waitForTimeout(5000);
    const esCall = apiCalls.find(u => u.includes('/Service') && !u.includes('ServiceType'));
    console.log('  Service API URL:', esCall || '(none)');
    console.log('  serviceTypeId present:', esCall && esCall.includes('serviceTypeId=') ? 'YES ✅' : 'NO ❌');
    const esCards = await page.evaluate(() => document.querySelectorAll('.svc-card, .service-card').length);
    console.log('  Service cards shown:', esCards);
  }

  await browser.close();
  console.log('\n=== Done ===');
})().catch(e => { console.error('FATAL:', e.message); process.exit(1); });
