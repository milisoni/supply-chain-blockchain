import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, ShipmentResponse } from '../../services/api.service';

@Component({
  selector: 'app-shipments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './shipments.component.html',
  styleUrl: './shipments.component.css'
})
export class ShipmentsComponent implements OnInit {
  list: ShipmentResponse[] = [];
  loading = true;
  myOnly = false;
  copiedId = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.listShipments(this.myOnly).subscribe({
      next: (l) => { this.list = l; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  toggleMyOnly(): void {
    this.myOnly = !this.myOnly;
    this.load();
  }

  copy(id: string): void {
    navigator.clipboard.writeText(id).then(() => {
      this.copiedId = id;
      setTimeout(() => { this.copiedId = ''; }, 2000);
    });
  }

  statusClass(s: string): string {
    switch (s) {
      case 'Delivered': return 'bg-success';
      case 'InTransit': return 'bg-info';
      case 'Dispatched': return 'bg-primary';
      default: return 'bg-secondary';
    }
  }
}
