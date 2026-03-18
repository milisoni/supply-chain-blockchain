import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService, AuthResponse } from '../../services/auth.service';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  user: AuthResponse | null = null;
  recentShipments: { id: string; productId: string; status: string; destination: string }[] = [];
  loading = true;

  constructor(public auth: AuthService, private api: ApiService) {}

  ngOnInit(): void {
    this.user = this.auth.currentUser();
    this.api.listShipments(true).subscribe({
      next: (list) => {
        this.recentShipments = list.slice(0, 5).map(s => ({ id: s.id, productId: s.productId, status: s.status, destination: s.destination }));
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }
}
