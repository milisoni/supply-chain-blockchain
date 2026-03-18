import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-shipment-status',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './shipment-status.component.html',
  styleUrl: './shipment-status.component.css'
})
export class ShipmentStatusComponent {
  blockchainShipmentId = '';
  status = 1;
  error = '';
  success = '';
  loading = false;
  statusOptions = [
    { value: 1, label: 'Dispatched' },
    { value: 2, label: 'In Transit' },
    { value: 3, label: 'Delivered' }
  ];

  constructor(private api: ApiService) {}

  submit(): void {
    this.error = '';
    this.success = '';
    if (!this.blockchainShipmentId.trim()) {
      this.error = 'Blockchain shipment ID is required.';
      return;
    }
    this.loading = true;
    this.api.updateShipmentStatus({ blockchainShipmentId: this.blockchainShipmentId.trim(), status: this.status }).subscribe({
      next: (r) => {
        this.success = `Status updated. Transaction: ${r.transactionHash}`;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.message || err?.error || 'Update failed.';
        this.loading = false;
      }
    });
  }
}
