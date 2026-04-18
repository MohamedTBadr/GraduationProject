import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CompanyInquiryDto {
    companyName: string;
    contactPerson: string;
    phoneNumber: string;
    email: string;
    categoryId: string;
    expectedDate: string;
    estimatedAttendees: number;
    approximateBudget: number;
    additionalRequirements: string;
}

@Injectable({
  providedIn: 'root'
})
export class CompanyInquiryService {
  private apiUrl = `${environment.apiUrl}/CompanyInquiry`;

  constructor(private http: HttpClient) {}

  submitInquiry(dto: CompanyInquiryDto): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }
}
