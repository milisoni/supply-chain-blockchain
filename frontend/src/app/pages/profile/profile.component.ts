import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, UserResponse } from '../../services/api.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  user: UserResponse | null = null;
  name = '';
  blockchainAddress = '';
  error = '';
  success = '';
  loading = true;
  saving = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getMe().subscribe({
      next: (u) => {
        this.user = u;
        this.name = u.name;
        this.blockchainAddress = u.blockchainAddress ?? '';
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  save(): void {
    this.error = '';
    this.success = '';
    this.saving = true;
    this.api.updateMe({ name: this.name, blockchainAddress: this.blockchainAddress || undefined }).subscribe({
      next: () => {
        this.success = 'Profile updated.';
        this.saving = false;
        if (this.user) {
          this.user.name = this.name;
          this.user.blockchainAddress = this.blockchainAddress || undefined;
        }
      },
      error: (err) => { this.error = err?.error?.message || 'Update failed.'; this.saving = false; }
    });
  }
}
