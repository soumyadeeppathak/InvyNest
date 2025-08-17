import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Workspace {
  id: string;
  name: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class WorkspaceService {
  private apiUrl = '/api/Workspace/mine'; // Adjust if your API route differs

  constructor(private http: HttpClient) {}

  getUserWorkspaces(email: string): Observable<Workspace[]> {
    return this.http.get<Workspace[]>(`${this.apiUrl}?email=${encodeURIComponent(email)}`);
  }
}
