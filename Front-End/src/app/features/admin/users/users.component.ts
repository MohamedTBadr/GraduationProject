import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ApiUser, PagedResult } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
  ) {}

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
      pageNumber: this.pageNumber, 
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

  changePage(delta: number) {
    const newPage = this.pageNumber + delta;
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.pageNumber = newPage;
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
}
