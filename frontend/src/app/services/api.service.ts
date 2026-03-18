import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

const api = environment.apiUrl;

export interface ShipmentResponse {
  id: string;
  blockchainShipmentId: string;
  productId: string;
  quantity: number;
  destination: string;
  status: string;
  transactionHash?: string;
  transactionRef: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateShipmentRequest {
  productId: string;
  quantity: number;
  destination: string;
  transactionRef?: string;
}

export interface UpdateStatusRequest {
  blockchainShipmentId: string;
  status: number;
}

export interface TrackShipmentResponse {
  blockchainShipmentId: string;
  productId: string;
  quantity: number;
  destination: string;
  status: string;
  transactionHash?: string;
  createdAt: string;
}

export interface PaymentAgreementResponse {
  id: string;
  blockchainAgreementId: string;
  blockchainShipmentId: string;
  buyerUserId: string;
  supplierUserId: string;
  amountWei: string;
  status: string;
  transactionHash?: string;
  createdAt: string;
  releasedAt?: string;
}

export interface CreatePaymentRequest {
  blockchainShipmentId: string;
  supplierUserId: string;
  amountWei: string;
}

export interface TransactionHistoryItem {
  id: string;
  transactionHash: string;
  type: string;
  userId?: string;
  blockchainShipmentId?: string;
  blockchainAgreementId?: string;
  details?: string;
  createdAt: string;
}

export interface UserResponse {
  id: string;
  name: string;
  email: string;
  role: string;
  blockchainAddress?: string;
  createdAt: string;
  isActive: boolean;
}

export interface UserUpdateRequest {
  name?: string;
  blockchainAddress?: string;
  isActive?: boolean;
}

export interface AdminOverview {
  usersCount: number;
  shipmentsCount: number;
  transactionsCount: number;
  paymentsCount: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  // Shipments
  createShipment(body: CreateShipmentRequest): Observable<ShipmentResponse> {
    return this.http.post<ShipmentResponse>(`${api}/shipments`, body);
  }
  listShipments(myOnly = false): Observable<ShipmentResponse[]> {
    return this.http.get<ShipmentResponse[]>(`${api}/shipments`, { params: new HttpParams().set('myOnly', myOnly) });
  }
  getShipment(id: string): Observable<ShipmentResponse> {
    return this.http.get<ShipmentResponse>(`${api}/shipments/${id}`);
  }
  trackShipment(blockchainShipmentId: string): Observable<TrackShipmentResponse> {
    return this.http.get<TrackShipmentResponse>(`${api}/shipments/track/${encodeURIComponent(blockchainShipmentId)}`);
  }
  updateShipmentStatus(body: UpdateStatusRequest): Observable<{ transactionHash: string; status: string }> {
    return this.http.put<{ transactionHash: string; status: string }>(`${api}/shipments/status`, body);
  }

  // Payments
  fundAgreement(body: CreatePaymentRequest): Observable<PaymentAgreementResponse> {
    return this.http.post<PaymentAgreementResponse>(`${api}/payments/fund`, body);
  }
  releasePayment(blockchainAgreementId: string): Observable<{ transactionHash: string; status: string }> {
    return this.http.post<{ transactionHash: string; status: string }>(`${api}/payments/release`, { blockchainAgreementId });
  }
  listPayments(myOnly = true): Observable<PaymentAgreementResponse[]> {
    return this.http.get<PaymentAgreementResponse[]>(`${api}/payments`, { params: new HttpParams().set('myOnly', myOnly) });
  }

  // Transaction history
  listTransactions(limit = 100): Observable<TransactionHistoryItem[]> {
    return this.http.get<TransactionHistoryItem[]>(`${api}/transactionhistory`, { params: new HttpParams().set('limit', limit) });
  }

  // Current user
  getMe(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${api}/users/me`);
  }
  updateMe(body: UserUpdateRequest): Observable<void> {
    return this.http.put<void>(`${api}/users/me`, body);
  }

  // Admin
  listUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(`${api}/admin/users`);
  }
  getUser(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${api}/admin/users/${id}`);
  }
  updateUser(id: string, body: UserUpdateRequest): Observable<void> {
    return this.http.put<void>(`${api}/admin/users/${id}`, body);
  }
  adminOverview(): Observable<AdminOverview> {
    return this.http.get<AdminOverview>(`${api}/admin/overview`);
  }
}
