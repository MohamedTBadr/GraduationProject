import { Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { SKIP_AUTH, SKIP_ERROR_TOAST } from './auth.service';

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

const SUBMIT_SUCCESS_MESSAGE = 'Your inquiry has been submitted. We will contact you soon.';

@Injectable({
  providedIn: 'root'
})
export class CompanyInquiryService {
  private readonly apiUrl = `${environment.apiUrl}/CompanyInquiry`;

  constructor(private http: HttpClient) {}

  private pickField(obj: any, ...keys: string[]): any {
    if (!obj) return undefined;
    for (const key of keys) {
      const val = obj[key];
      if (val !== undefined && val !== null) return val;
    }
    return undefined;
  }

  private normalizeInquiry(raw: any): CompanyInquiryResponse {
    const eventTypeRaw = raw?.eventType ?? raw?.EventType;
    return {
      id: String(this.pickField(raw, 'id', 'Id') ?? ''),
      companyName: String(this.pickField(raw, 'companyName', 'CompanyName') ?? ''),
      contactPerson: String(this.pickField(raw, 'contactPerson', 'ContactPerson') ?? ''),
      phoneNumber: String(this.pickField(raw, 'phoneNumber', 'PhoneNumber') ?? ''),
      email: String(this.pickField(raw, 'email', 'Email') ?? ''),
      eventType: eventTypeRaw ? {
        id: String(this.pickField(eventTypeRaw, 'id', 'Id') ?? ''),
        name: String(this.pickField(eventTypeRaw, 'name', 'Name') ?? '')
      } : undefined,
      expectedDate: String(this.pickField(raw, 'expectedDate', 'ExpectedDate') ?? ''),
      estimatedAttendees: Number(this.pickField(raw, 'estimatedAttendees', 'EstimatedAttendees') ?? 0),
      approximateBudget: Number(this.pickField(raw, 'approximateBudget', 'ApproximateBudget') ?? 0),
      additionalRequirements: String(this.pickField(raw, 'additionalRequirements', 'AdditionalRequirements') ?? ''),
      status: (this.pickField(raw, 'status', 'Status') ?? 'Pending') as CompanyInquiryResponse['status']
    };
  }

  private extractSubmitMessage(res: any): string {
    if (typeof res === 'string' && res.trim()) return res;
    return this.pickField(res, 'message', 'Message') ?? SUBMIT_SUCCESS_MESSAGE;
  }

  submitInquiry(dto: CreateCompanyInquiryDto): Observable<{ message: string }> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    const context = new HttpContext().set(SKIP_AUTH, true).set(SKIP_ERROR_TOAST, true);

    return this.http.post(this.apiUrl, dto, { headers, context }).pipe(
      map(res => ({ message: this.extractSubmitMessage(res) })),
      catchError(err => {
        // Idempotent API returns 406 when the same IdempotencyKey is replayed after a successful save.
        if (err.status === 406) {
          return of({ message: SUBMIT_SUCCESS_MESSAGE });
        }
        return throwError(() => err);
      })
    );
  }

  /** Admin: GET /CompanyInquiry?pageIndex=1&pageSize=20 */
  getAll(pageIndex = 1, pageSize = 20): Observable<{ items: CompanyInquiryResponse[]; totalCount: number }> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<any>(this.apiUrl, { params }).pipe(
      map(res => {
        const data = res?.value ?? res?.Value ?? res;
        const rawItems = Array.isArray(data)
          ? data
          : (data?.items ?? data?.Items ?? []);
        const items = (Array.isArray(rawItems) ? rawItems : []).map(i => this.normalizeInquiry(i));
        const totalCount = data?.totalCount ?? data?.TotalCount ?? items.length;
        return { items, totalCount };
      })
    );
  }

  /** Admin: GET /CompanyInquiry/{id} */
  getById(id: string): Observable<CompanyInquiryResponse> {
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map(res => this.normalizeInquiry(res?.value ?? res?.Value ?? res))
    );
  }

  /** Admin: PUT /CompanyInquiry/{id} */
  update(id: string, dto: UpdateCompanyInquiryDto): Observable<void> {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    return this.http.put<void>(`${this.apiUrl}/${id}`, dto, { headers });
  }

  /** Admin: update status only — builds full update DTO from existing inquiry */
  updateStatus(inquiry: CompanyInquiryResponse, status: string): Observable<void> {
    const eventTypeId = inquiry.eventType?.id;
    if (!eventTypeId) {
      return throwError(() => new Error('Cannot update inquiry: event type is missing.'));
    }

    const dto: UpdateCompanyInquiryDto = {
      id: inquiry.id,
      companyName: inquiry.companyName,
      contactPerson: inquiry.contactPerson,
      phoneNumber: inquiry.phoneNumber,
      email: inquiry.email,
      eventTypeId,
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
