import { appendFormFile, appendFormFileList } from './form-data.utils';
import { appendServiceAreasToFormData, normalizeAddressFields } from './location.utils';
import { ServiceAreaDTO } from '../types/api.interfaces';

/** Build multipart body for POST /Vendor ([FromForm] CreateVendorRequest). */
export function appendVendorCreateFormData(
  formData: FormData,
  fields: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phone: string;
    name: string;
    businessName: string;
    ownerName: string;
    vendorTypeId: string;
    yearsInBusiness: number;
    description: string;
    address?: {
      street?: string;
      city?: string;
      state?: string;
      postalCode?: string;
    };
    serviceAreas?: ServiceAreaDTO[];
    profilePicture?: File | null;
    documents?: File[];
  }
): void {
  formData.append('FirstName', fields.firstName);
  formData.append('LastName', fields.lastName);
  formData.append('Email', fields.email);
  formData.append('Password', fields.password);
  formData.append('Phone', fields.phone);
  formData.append('Name', fields.name);
  formData.append('BusinessName', fields.businessName);
  formData.append('OwnerName', fields.ownerName);
  formData.append('VendorTypeId', fields.vendorTypeId);
  formData.append('YearsInBusiness', String(fields.yearsInBusiness ?? 0));
  formData.append('Description', fields.description);

  if (fields.address) {
    const { city, state } = normalizeAddressFields(
      fields.address.city || '',
      fields.address.state || ''
    );
    formData.append('Address.Street', fields.address.street || '');
    formData.append('Address.City', city);
    formData.append('Address.State', state);
    formData.append('Address.PostalCode', fields.address.postalCode || '');
    if (fields.serviceAreas?.length) {
      appendServiceAreasToFormData(formData, fields.serviceAreas);
    }
  }

  appendFormFile(formData, 'ProfilePicture', fields.profilePicture);
  if (fields.documents?.length) {
    appendFormFileList(formData, 'Document', fields.documents);
  }
}

/** Build multipart body for PATCH /Vendor/{id} ([FromForm] UpdateVendorRequest). */
export function appendVendorUpdateFormData(
  formData: FormData,
  fields: {
    name?: string;
    businessName?: string;
    ownerName?: string;
    phone?: string;
    description?: string;
    address?: {
      street?: string;
      city?: string;
      state?: string;
      postalCode?: string;
    };
    serviceAreas?: ServiceAreaDTO[];
    profilePicture?: File | null;
  }
): void {
  if (fields.name) formData.append('Name', fields.name);
  if (fields.businessName) formData.append('BusinessName', fields.businessName);
  if (fields.ownerName) formData.append('OwnerName', fields.ownerName);
  if (fields.phone) formData.append('Phone', fields.phone);
  if (fields.description != null) formData.append('Description', fields.description);

  if (fields.address) {
    const { city, state } = normalizeAddressFields(
      fields.address.city || '',
      fields.address.state || ''
    );
    formData.append('Address.Street', fields.address.street || '');
    formData.append('Address.City', city);
    formData.append('Address.State', state);
    formData.append('Address.PostalCode', fields.address.postalCode || '');
  }

  if (fields.serviceAreas?.length) {
    appendServiceAreasToFormData(formData, fields.serviceAreas);
  }

  appendFormFile(formData, 'ProfilePicture', fields.profilePicture);
}
