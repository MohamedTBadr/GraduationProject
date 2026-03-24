export interface VendorService {
    icon: string;
    name: string;
    price: string;
    desc: string;
    includes?: string[];
    delivery?: string;
    duration?: string;
    images?: string[]; // Added to support multiple service images
}

export interface VendorPackage {
    icon: string;
    name: string;
    price: string;
    desc: string;
}

export interface Vendor {
    id: number;
    name: string;
    category: string;
    location: string;
    rating: number;
    reviews: number;
    startPrice: number;
    emoji: string;
    events?: string[];
    capacity?: string;
    responseTime?: string;
    deposit?: string;
    cancellation?: string;
    services?: VendorService[];
    packages?: VendorPackage[];
    about?: string;
    revenue?: string;
    bookings?: number;
    joined?: string;
    status?: 'active' | 'suspended' | 'pending';
    docs?: string;
    applied?: string;
    amenities?: string[];
}
