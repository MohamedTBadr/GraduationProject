import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ApiUser, PagedResult } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  providers: [DatePipe],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {
  users: ApiUser[] = [];
  loading = false;

  // Pagination & Search State
  searchQuery: string = '';
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 1;

  private searchSubject = new Subject<string>();

  constructor(
    private userService: UserService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loadUsers();

    // Setup debounce for typed searching
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageNumber = 1;
      this.loadUsers();
    });
  }

  onSearchChange() {
    this.searchSubject.next(this.searchQuery);
  }

  loadUsers() {
    this.loading = true;
    this.userService.getAll({
      pageIndex: this.pageNumber,
      pageSize: this.pageSize,
      searchTerm: this.searchQuery
    }).subscribe({
      next: (res: any) => {
        this.users = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.totalPages = res.totalPages || 1;
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load users', 'error');
        this.loading = false;
      }
    });
  }

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.pageNumber = page;
      this.loadUsers();
    }
  }

  deleteUser(user: ApiUser) {
    if (confirm(`Are you sure you want to permanently delete ${user.name}? This action cannot be undone.`)) {
      this.loading = true;
      this.userService.delete(user.id).subscribe({
        next: () => {
          this.toastService.show(`User ${user.name} deleted successfully.`, 'success');
          this.loadUsers();
        },
        error: (err) => {
          this.toastService.show('Failed to delete user. They might have active dependencies.', 'error');
          this.loading = false;
        }
      });
    }
  }

  suspendUser(user: ApiUser) {
    const reason = prompt(`Please enter a reason for suspending ${user.name}:`, 'Violation of terms');
    if (reason) {
      this.loading = true;
      this.userService.suspend(user.id, reason).subscribe({
        next: () => {
          this.toastService.show(`User ${user.name} has been suspended.`, 'success');
          this.loadUsers();
        },
        error: (err) => {
          this.toastService.show('Failed to suspend user.', 'error');
          this.loading = false;
        }
      });
    }
  }

  unsuspendUser(user: ApiUser) {
    if (confirm(`Are you sure you want to unsuspend ${user.name}?`)) {
      this.loading = true;
      this.userService.unsuspend(user.id).subscribe({
        next: () => {
          this.toastService.show(`User ${user.name} has been unsuspended.`, 'success');
          this.loadUsers();
        },
        error: (err) => {
          this.toastService.show('Failed to unsuspend user.', 'error');
          this.loading = false;
        }
      });
    }
  }
}
