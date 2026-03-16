import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VendorSidebarComponent } from './vendor-sidebar.component';

describe('VendorSidebarComponent', () => {
  let component: VendorSidebarComponent;
  let fixture: ComponentFixture<VendorSidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorSidebarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VendorSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
