import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VendorJoinComponent } from './vendor-join.component';

describe('VendorJoinComponent', () => {
  let component: VendorJoinComponent;
  let fixture: ComponentFixture<VendorJoinComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorJoinComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VendorJoinComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
