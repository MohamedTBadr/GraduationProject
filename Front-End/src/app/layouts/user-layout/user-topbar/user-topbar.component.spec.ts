import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserTopbarComponent } from './user-topbar.component';

describe('UserTopbarComponent', () => {
  let component: UserTopbarComponent;
  let fixture: ComponentFixture<UserTopbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserTopbarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserTopbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
