import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, PaymentAgreementResponse } from '../../services/api.service';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payments.component.html',
  styleUrl: './payments.component.css'
})
export class PaymentsComponent implements OnInit {
  list: PaymentAgreementResponse[] = [];
  loading = true;
  fundShipmentId = '';
  fundSupplierUserId = '';
  fundAmountWei = '';
  releaseAgreementId = '';
  error = '';
  success = '';
  loadingAction = false;
  copiedId = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.listPayments(true).subscribe({
      next: (l) => { this.list = l; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  fund(): void {
    this.error = '';
    this.success = '';
    if (!this.fundShipmentId.trim() || !this.fundSupplierUserId.trim() || !this.fundAmountWei.trim()) {
      this.error = 'Shipment ID, supplier user ID and amount (Wei) are required.';
      return;
    }
    this.loadingAction = true;
    this.api.fundAgreement({
      blockchainShipmentId: this.fundShipmentId.trim(),
      supplierUserId: this.fundSupplierUserId.trim(),
      amountWei: this.fundAmountWei
    }).subscribe({
      next: () => { this.success = 'Payment agreement funded.'; this.loadingAction = false; this.load(); },
      error: (err) => { this.error = err?.error?.message || err?.error || 'Funding failed.'; this.loadingAction = false; }
    });
  }

  release(): void {
    this.error = '';
    this.success = '';
    if (!this.releaseAgreementId.trim()) {
      this.error = 'Agreement ID is required.';
      return;
    }
    this.loadingAction = true;
    this.api.releasePayment(this.releaseAgreementId.trim()).subscribe({
      next: (r) => { this.success = `Payment released. Tx: ${r.transactionHash}`; this.loadingAction = false; this.load(); },
      error: (err) => { this.error = err?.error?.message || err?.error || 'Release failed. Shipment must be Delivered.'; this.loadingAction = false; }
    });
  }

  copy(id: string): void {
    navigator.clipboard.writeText(id).then(() => {
      this.copiedId = id;
      setTimeout(() => { this.copiedId = ''; }, 2000);
    });
  }

  statusClass(s: string): string {
    return s === 'Released' ? 'bg-success' : 'bg-warning text-dark';
  }
}
