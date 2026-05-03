import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

export const responseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse) {
        if (event.body && typeof event.body === 'object') {
          const bodyAny = event.body as any;
          
          if (bodyAny.isSuccess !== undefined || bodyAny.IsSuccess !== undefined) {
            const isSuccess = bodyAny.isSuccess !== undefined ? bodyAny.isSuccess : bodyAny.IsSuccess;
            
            if (isSuccess) {
              // The backend returns { IsSuccess: true, Value: { ... } } or { isSuccess: true, value: { ... } }
              const unwrappedValue = bodyAny.value !== undefined ? bodyAny.value : bodyAny.Value;
              
              // If unwrappedValue is undefined, the API returned an empty success result
              return event.clone({ body: unwrappedValue !== undefined ? unwrappedValue : null });
            }
          }
        }
      }
      return event;
    })
  );
};
