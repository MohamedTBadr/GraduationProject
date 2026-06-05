import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface CreateCompanyInquiryDto {
  companyName: string;
  contactPerson: string;
  phoneNumber: string;
  email: string;
  eventTypeId: string;
  expectedDate: string;
  estimatedAttendees: number;
  approximateBudget: number;
  additionalRequirements: string;
}

export interface CompanyInquiryResponse {
  id: string;
  companyName: string;
  contactPerson: string;
  phoneNumber: string;
  email: string;
  eventType?: { id: string; name: string };
  expectedDate: string;
  estimatedAttendees: number;
  approximateBudget: number;
  additionalRequirements: string;
  status: 'Pending' | 'Reviewed' | 'Closed';
}

export interface UpdateCompanyInquiryDto {
  id: string;
  companyName: string;
  contactPerson: string;
  phoneNumber: string;
  email: string;
  eventTypeId: string;
  expectedDate: string;
  estimatedAttendees: number;
  approximateBudget: number;
  additionalRequirements: string;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class CompanyInquiryService {
  private readonly apiUrl = `${environment.apiUrl}/CompanyInquiry`;

  constructor(private http: HttpClient) {}

  submitInquiry(dto: CreateCompanyInquiryDto): Observable<any> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.post(this.apiUrl, dto, { headers });
  }

  /** Admin: GET /CompanyInquiry?pageIndex=1&pageSize=20 */
  getAll(pageIndex = 1, pageSize = 20): Observable<{ items: CompanyInquiryResponse[]; totalCount: number }> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<any>(this.apiUrl, { params }).pipe(
      map(res => {
        const data = res?.value ?? res;
        const items: CompanyInquiryResponse[] = Array.isArray(data)
          ? data
          : (Array.isArray(data?.items) ? data.items : []);
        const totalCount: number = data?.totalCount ?? items.length;
        return { items, totalCount };
      })
    );
  }

  /** Admin: GET /CompanyInquiry/{id} */
  getById(id: string): Observable<CompanyInquiryResponse> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(res => res?.value ?? res)
    );
  }

  /** Admin: PUT /CompanyInquiry/{id} */
  update(id: string, dto: UpdateCompanyInquiryDto): Observable<void> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto, { headers });
  }

  /** Admin: update status only — builds full update DTO from existing inquiry */
  updateStatus(inquiry: CompanyInquiryResponse, status: string): Observable<void> {
    const dto: UpdateCompanyInquiryDto = {
      id: inquiry.id,
      companyName: inquiry.companyName,
      contactPerson: inquiry.contactPerson,
      phoneNumber: inquiry.phoneNumber,
      email: inquiry.email,
      eventTypeId: inquiry.eventType?.id ?? '',
      expectedDate: inquiry.expectedDate,
      estimatedAttendees: inquiry.estimatedAttendees,
      approximateBudget: inquiry.approximateBudget,
      additionalRequirements: inquiry.additionalRequirements,
      status
    };
    return this.update(inquiry.id, dto);
  }

  /** Admin: DELETE /CompanyInquiry/{id} */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
