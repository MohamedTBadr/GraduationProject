import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminTopbarComponent } from './admin-topbar.component';

describe('AdminTopbarComponent', () => {
  let component: AdminTopbarComponent;
  let fixture: ComponentFixture<AdminTopbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminTopbarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminTopbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
