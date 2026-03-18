import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-shipment-add',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './shipment-add.component.html',
  styleUrl: './shipment-add.component.css'
})
export class ShipmentAddComponent {
  productId = '';
  quantity = 1;
  destination = '';
  transactionRef = '';
  error = '';
  loading = false;

  constructor(private api: ApiService, private router: Router) {}

  submit(): void {
    this.error = '';
    if (!this.productId.trim() || !this.destination.trim()) {
      this.error = 'Product ID and destination are required.';
      return;
    }
    this.loading = true;
    this.api.createShipment({
      productId: this.productId,
      quantity: this.quantity,
      destination: this.destination,
      transactionRef: this.transactionRef
    }).subscribe({
      next: () => this.router.navigate(['/shipments']),
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || err?.error || 'Failed to create shipment. Is the blockchain configured?';
      }
    });
  }
}
