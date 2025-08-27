import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, signal, inject } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

//export const API_BASE_URL = '/api/Workspace';

export interface CreateWorkspaceDto {
  name: string;
  ownerEmail: string;
}

export interface AddMemberDto {
  memberEmail: string;
  role: string; // "owner" | "editor" | "viewer"
}

export interface WorkspaceDto {
  id: string;
  name: string;
  ownerEmail: string;
  createdAtUtc: string; // ISO
}

@Injectable({
  providedIn: 'root'
})
export class WorkspaceService {
  private http = inject(HttpClient);
  
  private readonly base = `/api/workspace`;
  private _workspaces = signal<WorkspaceDto[]>([]);
  private _loading = signal(false);
  private _error = signal<string | null>(null);

  get workspaces() {
    return this._workspaces.asReadonly();
  }

  get loading() {
    return this._loading.asReadonly();
  }

  get error() {
    return this._error.asReadonly();
  }

  /** GET /api/workspace/mine?email=you@example.com */
  fetchMyWorkspaces(email: string) {
    this._loading.set(true);
    this._error.set(null);
    const params = new HttpParams().set('email', email.trim().toLowerCase());
    this.http.get<WorkspaceDto[]>(`${this.base}/mine`, { params }).subscribe({
      next: (data) => {
        this._workspaces.set(data);
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(this.handleError(err));
        this._loading.set(false);
      }
    });
  }

  /** POST /api/workspace */
  createWorkspace(payload: CreateWorkspaceDto, refreshEmail: string) {
    this._loading.set(true);
    this._error.set(null);
    this.http.post<WorkspaceDto>(`${this.base}`, payload).subscribe({
      next: () => {
        this.fetchMyWorkspaces(refreshEmail);
      },
      error: (err) => {
        this._error.set(this.handleError(err));
        this._loading.set(false);
      }
    });
  }

  /** POST /api/workspace/{id}/members */
  addMember(workspaceId: string, payload: AddMemberDto) {
    this._loading.set(true);
    this._error.set(null);
    this.http.post<any>(`${this.base}/${encodeURIComponent(workspaceId)}/members`, payload).subscribe({
      next: () => {
        this._loading.set(false);
      },
      error: (err) => {
        this._error.set(this.handleError(err));
        this._loading.set(false);
      }
    });
  }

   /** DELETE /api/workspace/{id} */
  deleteWorkspace(id: string, refreshEmail: string) {
    this._loading.set(true);
    this._error.set(null);
    this.http.delete(`${this.base}/${encodeURIComponent(id)}`).subscribe({
      next: () => {
        this.fetchMyWorkspaces(refreshEmail);
      },
      error: (err) => {
        this._error.set(this.handleError(err));
        this._loading.set(false);
      }
    });
  }

  private handleError(err: HttpErrorResponse): string {
    return (
      (err.error && (err.error.title || err.error.detail || err.error)) ||
      err.message ||
      'Request failed'
    );
  }
}
