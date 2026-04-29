export interface VendorType {
  id: string;
  name: string;
}

export interface ServiceType {
  id: string;
  name: string;
  vendorTypeId?: string; // Optional if not always returned
}

export interface EventType {
  id: string;
  name: string;
}
