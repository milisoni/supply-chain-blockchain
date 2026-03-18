import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, TransactionHistoryItem } from '../../services/api.service';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css'
})
export class TransactionsComponent implements OnInit {
  list: TransactionHistoryItem[] = [];
  loading = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.listTransactions(100).subscribe({
      next: (l) => { this.list = l; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  typeClass(t: string): string {
    switch (t) {
      case 'CreateShipment': return 'bg-primary';
      case 'UpdateStatus': return 'bg-info';
      case 'FundPayment': return 'bg-warning text-dark';
      case 'ReleasePayment': return 'bg-success';
      default: return 'bg-secondary';
    }
  }
}
