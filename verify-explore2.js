const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  page.setDefaultTimeout(30000);

  const apiCalls = [];
  page.on('request', req => {
    const url = req.url();
    if (url.includes('/api/')) {
      apiCalls.push({ url, method: req.method() });
    }
  });

  page.on('response', async res => {
    const url = res.url();
    if (url.includes('/VendorType') && !url.match(/VendorType\//)) {
      try {
        const body = await res.json();
        console.log('VendorType response body sample:');
        const arr = body?.value ?? body;
        const items = Array.isArray(arr) ? arr : (arr?.items ?? []);
        console.log('  Total items:', items.length);
        if (items.length > 0) {
          console.log('  First item keys:', Object.keys(items[0]));
          console.log('  First item:', JSON.stringify(items[0]));
        }
      } catch(e) {
        console.log('Could not parse VendorType JSON:', e.message);
      }
    }
    if (url.includes('/Vendor') && !url.includes('VendorType')) {
      console.log('Vendor API response:', res.status(), url.split('?')[1] || '(no params)');
    }
  });

  // Load vendor tab and wait long enough
  console.log('\nLoading vendor tab...');
  await page.goto('http://localhost:4200/explore?tab=vendors', { waitUntil: 'domcontentloaded', timeout: 15000 });
  await page.waitForTimeout(10000);

  console.log('\nAll API calls made (10s window):');
  apiCalls.forEach(r => console.log(' ', r.method, r.url));

  // Check console errors from Angular
  const consoleErrors = [];
  page.on('console', msg => { if (msg.type() === 'error') consoleErrors.push(msg.text()); });

  console.log('\nChecking page console errors...');
  await page.waitForTimeout(1000);
  consoleErrors.forEach(e => console.log('  ERROR:', e));

  // Check if vendorTypes is populated in Angular
  const vendorTypesInfo = await page.evaluate(() => {
    // Try to access Angular's internal state via debug
    try {
      const el = document.querySelector('app-explore');
      const ng = window['ng'];
      if (ng && el) {
        const ctx = ng.getComponent(el);
        return {
          vendorTypesLength: ctx?.vendorTypes?.length ?? 'N/A',
          vendorTypes: ctx?.vendorTypes?.slice(0, 3) ?? [],
          allVendorsLength: ctx?.allVendors?.length ?? 'N/A',
          loading: ctx?.loading,
          activeTab: ctx?.activeTab,
        };
      }
    } catch(e) { return { error: e.message }; }
    return { error: 'No ng debug' };
  });
  console.log('\nAngular component state:', JSON.stringify(vendorTypesInfo, null, 2));

  await browser.close();
})().catch(e => { console.error('FATAL:', e.message); process.exit(1); });
