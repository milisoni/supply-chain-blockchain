import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, UserResponse, UserUpdateRequest } from '../../services/api.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css'
})
export class AdminUsersComponent implements OnInit {
  list: UserResponse[] = [];
  loading = true;
  editingId: string | null = null;
  editName = '';
  editAddress = '';
  editActive = true;
  error = '';
  success = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.listUsers().subscribe({
      next: (l) => { this.list = l; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  startEdit(u: UserResponse): void {
    this.editingId = u.id;
    this.editName = u.name;
    this.editAddress = u.blockchainAddress ?? '';
    this.editActive = u.isActive;
    this.error = '';
    this.success = '';
  }

  cancelEdit(): void {
    this.editingId = null;
  }

  save(): void {
    if (!this.editingId) return;
    this.error = '';
    this.success = '';
    const body: UserUpdateRequest = { name: this.editName, blockchainAddress: this.editAddress || undefined, isActive: this.editActive };
    this.api.updateUser(this.editingId, body).subscribe({
      next: () => {
        this.success = 'User updated.';
        this.editingId = null;
        this.load();
      },
      error: (err) => { this.error = err?.error?.message || 'Update failed.'; }
    });
  }
}
