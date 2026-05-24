import { Injectable } from '@angular/core';
import { AiService } from './ai.service';

/**
 * @deprecated Use AiService instead. This is kept for backward compatibility.
 */
@Injectable({ providedIn: 'root' })
export class GeminiService extends AiService {}
