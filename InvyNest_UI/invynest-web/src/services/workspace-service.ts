import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
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

  constructor(private http: HttpClient
    //@Inject(API_BASE_URL) private apiBase: string,
  ) {}

  private readonly base = `/api/workspace`;

  /** POST /api/workspace */
  createWorkspace(payload: CreateWorkspaceDto): Observable<WorkspaceDto> {
    return this.http
      .post<WorkspaceDto>(`${this.base}`, payload)
      .pipe(catchError(this.handleError));
  }

  /** POST /api/workspace/{id}/members */
  addMember(workspaceId: string, payload: AddMemberDto): Observable<any> {
    return this.http
      .post<any>(`${this.base}/${encodeURIComponent(workspaceId)}/members`, payload)
      .pipe(catchError(this.handleError));
  }

  /** GET /api/workspace/mine?email=you@example.com */
  getMyWorkspaces(email: string): Observable<WorkspaceDto[]> {
    const params = new HttpParams().set('email', email.trim().toLowerCase());
    return this.http
      .get<WorkspaceDto[]>(`${this.base}/mine`, { params })
      .pipe(catchError(this.handleError));
  }

  // ——— helpers ———

  private handleError(err: HttpErrorResponse) {
    // Surface server-provided messages when available (e.g., 400/404/409 from controller)
    const message =
      (err.error && (err.error.title || err.error.detail || err.error)) ||
      err.message ||
      'Request failed';
    return throwError(() => new Error(message));
  }
}
