import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { FileUploadResponse } from '../../shared/types/api.interfaces';

@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * POST /files/upload
   * Uploads a file using multipart/form-data.
   * @param file - The File object to upload
   * @param fieldName - The form field name expected by the server (default: 'file')
   */
  upload(file: File, fieldName = 'file'): Observable<FileUploadResponse> {
    const formData = new FormData();
    formData.append(fieldName, file, file.name);
    return this.http.post<any>(`${this.apiUrl}/files/upload`, formData).pipe(
      map(res => {
        const data = res?.value ?? res?.Value ?? res ?? {};
        return {
          url: data.url ?? data.Url ?? '',
          fileName: data.fileName ?? data.FileName ?? file.name,
          size: data.size ?? data.Size ?? file.size
        };
      })
    );
  }
}
