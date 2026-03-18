import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, TrackShipmentResponse } from '../../services/api.service';

@Component({
  selector: 'app-shipment-track',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './shipment-track.component.html',
  styleUrl: './shipment-track.component.css'
})
export class ShipmentTrackComponent {
  blockchainId = '';
  result: TrackShipmentResponse | null = null;
  error = '';
  loading = false;

  constructor(private api: ApiService) {}

  search(): void {
    this.error = '';
    this.result = null;
    if (!this.blockchainId.trim()) {
      this.error = 'Enter a shipment ID (blockchain ID).';
      return;
    }
    this.loading = true;
    this.api.trackShipment(this.blockchainId.trim()).subscribe({
      next: (r) => { this.result = r; this.loading = false; },
      error: (err) => { this.error = err?.error?.message || err?.status === 404 ? 'Shipment not found.' : 'Error loading shipment.'; this.loading = false; }
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
