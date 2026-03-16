import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserSidebarComponent } from './user-sidebar.component';

describe('UserSidebarComponent', () => {
  let component: UserSidebarComponent;
  let fixture: ComponentFixture<UserSidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserSidebarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
