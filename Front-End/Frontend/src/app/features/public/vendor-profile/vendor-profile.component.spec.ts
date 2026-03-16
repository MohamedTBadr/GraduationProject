import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VendorProfileComponent } from './vendor-profile.component';

describe('VendorProfileComponent', () => {
  let component: VendorProfileComponent;
  let fixture: ComponentFixture<VendorProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorProfileComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VendorProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
