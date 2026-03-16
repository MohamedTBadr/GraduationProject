import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.scss']
})
export class ButtonComponent {
  @Input() variant: 'gold' | 'navy' | 'ghost' | 'green' | 'red' | 'outline' = 'gold';
  @Input() size: 'sm' | 'md' = 'md';
  @Input() disabled: boolean = false;
  @Input() customClass: string = '';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';

  @Output() btnClick = new EventEmitter<Event>();

  get classes(): string {
    return `btn btn-${this.variant} ${this.size === 'sm' ? 'btn-sm' : ''} ${this.customClass}`;
  }
}
