import { Injectable } from '@angular/core';
import {
  HttpEvent, HttpHandler, HttpInterceptor, HttpRequest
} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class JsonInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // Skip FormData (file uploads)
    if (req.body instanceof FormData) {
      return next.handle(req);
    }

    let headersConfig: any = {
      'Accept': 'application/json'
    };

    if (req.method !== 'GET' && req.method !== 'DELETE') {
      headersConfig['Content-Type'] = 'application/json';
    }

    const jsonReq = req.clone({
      setHeaders: headersConfig
    });

    return next.handle(jsonReq);
  }
}