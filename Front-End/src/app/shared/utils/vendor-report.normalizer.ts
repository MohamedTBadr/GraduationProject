export interface VendorReportView {
  generatedAt?: string;
  kpis: {
    lifetimeRevenue: number;
    currentMonthRevenue: number;
    lastMonthRevenue: number;
    growthPercentage: number;
    totalOrders: number;
    averageOrderValue: number;
    averageMonthlyRevenue: number;
  };
  insights: {
    bestMonth: { label: string; revenue: number } | null;
    worstMonth: { label: string; revenue: number } | null;
  };
  topServices: Array<{
    serviceName: string;
    orders: number;
    quantitySold: number;
    revenue: number;
    revenueShare: number;
  }>;
  revenueHistory: Array<{ label: string; revenue: number; orders: number }>;
  recentOrders: Array<{
    orderId: string;
    customerName: string;
    serviceName: string;
    amount: number;
    createdAt: string;
  }>;
  recommendations: string[];
  executiveSummary?: string;
}

function num(value: unknown): number {
  const n = Number(value);
  return Number.isFinite(n) ? n : 0;
}

function pick<T>(obj: any, ...keys: string[]): T | undefined {
  if (!obj) return undefined;
  for (const key of keys) {
    const val = obj[key];
    if (val != null) return val;
  }
  return undefined;
}

function normalizeMonthPoint(raw: any): { label: string; revenue: number } | null {
  if (!raw) return null;
  return {
    label: pick<string>(raw, 'label', 'Label', 'month', 'Month') ?? '',
    revenue: num(pick(raw, 'revenue', 'Revenue', 'monthlyRevenue', 'MonthlyRevenue'))
  };
}

/** Normalize POST /Dashboard/vendor-report for vendor dashboard templates. */
export function normalizeVendorReport(raw: any): VendorReportView | null {
  if (!raw) return null;

  const d = raw?.value ?? raw?.Value ?? raw;
  if (!d || typeof d !== 'object') return null;

  const rawKpis = d.kpIs ?? d.KPIs ?? d.kpis ?? {};
  const kpis = {
    lifetimeRevenue: num(pick(rawKpis, 'lifetimeRevenue', 'LifetimeRevenue')),
    currentMonthRevenue: num(pick(rawKpis, 'currentMonthRevenue', 'CurrentMonthRevenue')),
    lastMonthRevenue: num(pick(rawKpis, 'lastMonthRevenue', 'LastMonthRevenue')),
    growthPercentage: num(pick(rawKpis, 'growthPercentage', 'GrowthPercentage')),
    totalOrders: num(pick(rawKpis, 'totalOrders', 'TotalOrders')),
    averageOrderValue: num(pick(rawKpis, 'averageOrderValue', 'AverageOrderValue')),
    averageMonthlyRevenue: num(pick(rawKpis, 'averageMonthlyRevenue', 'AverageMonthlyRevenue'))
  };

  const rawInsights = d.insights ?? d.Insights ?? {};
  const insights = {
    bestMonth: normalizeMonthPoint(rawInsights.bestMonth ?? rawInsights.BestMonth),
    worstMonth: normalizeMonthPoint(rawInsights.worstMonth ?? rawInsights.WorstMonth)
  };

  const hist = d.revenueHistory ?? d.RevenueHistory ?? [];
  const revenueHistory = (Array.isArray(hist) ? hist : []).map((point: any) => ({
    label: pick<string>(point, 'label', 'Label', 'month', 'Month') ?? '',
    revenue: num(pick(point, 'revenue', 'Revenue', 'monthlyRevenue', 'MonthlyRevenue')),
    orders: num(pick(point, 'orders', 'Orders', 'orderCount', 'OrderCount'))
  }));

  const topRaw = d.topServices ?? d.TopServices ?? [];
  const topList = (Array.isArray(topRaw) ? topRaw : []).map((service: any) => ({
    serviceName:
      pick<string>(service, 'serviceName', 'ServiceName', 'service', 'Service', 'name', 'Name') ?? 'Service',
    orders: num(pick(service, 'orders', 'Orders')),
    quantitySold: num(pick(service, 'quantitySold', 'QuantitySold')),
    revenue: num(pick(service, 'revenue', 'Revenue'))
  }));
  const totalTopRevenue = topList.reduce((sum, service) => sum + service.revenue, 0);
  const topServices = topList.map(service => ({
    ...service,
    revenueShare: totalTopRevenue > 0 ? Math.round((service.revenue / totalTopRevenue) * 1000) / 10 : 0
  }));

  const ordersRaw = d.recentOrders ?? d.RecentOrders ?? [];
  const recentOrders = (Array.isArray(ordersRaw) ? ordersRaw : []).map((order: any) => ({
    orderId: String(pick(order, 'orderId', 'OrderId', 'id', 'Id') ?? ''),
    customerName:
      pick<string>(order, 'customerName', 'CustomerName', 'customer', 'Customer') ?? 'Customer',
    serviceName: pick<string>(order, 'serviceName', 'ServiceName', 'service', 'Service') ?? '',
    amount: num(pick(order, 'amount', 'Amount')),
    createdAt: pick<string>(order, 'createdAt', 'CreatedAt') ?? ''
  }));

  const recs = d.recommendations ?? d.Recommendations ?? [];

  return {
    generatedAt: pick<string>(d, 'generatedAt', 'GeneratedAt'),
    kpis,
    insights,
    topServices,
    revenueHistory,
    recentOrders,
    recommendations: Array.isArray(recs) ? recs : [],
    executiveSummary: pick<string>(d, 'executiveSummary', 'ExecutiveSummary')
  };
}
