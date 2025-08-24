import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface HierarchyNode {
  id: string;
  name: string;
  quantity: number;
  unit?: string;
  holder?: string;
  locationNote?: string;
  children: HierarchyNode[];
}

export interface CreateWorkspaceItemDto {
  workspaceId: string;
  name: string;
  quantity: number;
  unit?: string;
  holder: string;
  locationNote?: string;
  parentWorkspaceItemId?: string;
}

export interface UpdateItemNameDto {
  name: string;
}

export interface UpdateItemQuantityDto {
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly base = '/api/items';

  constructor(private http: HttpClient) {}

  createItem(dto: CreateWorkspaceItemDto): Observable<any> {
    return this.http.post(`${this.base}`, dto);
  }

  getWorkspaceItem(id: string): Observable<any> {
    return this.http.get(`${this.base}/workspaceitem/${id}`);
  }

  updateItemName(id: string, dto: UpdateItemNameDto): Observable<any> {
    return this.http.put(`${this.base}/workspaceitem/${id}/name`, dto);
  }

  updateItemQuantity(id: string, dto: UpdateItemQuantityDto): Observable<any> {
    return this.http.put(`${this.base}/workspaceitem/${id}/quantity`, dto);
  }

  deleteItem(id: string): Observable<any> {
    return this.http.delete(`${this.base}/workspaceitem/${id}`);
  }

  getHierarchy(workspaceId: string, holder?: string): Observable<HierarchyNode[]> {
    let params = new HttpParams().set('workspaceId', workspaceId);
    if (holder) params = params.set('holder', holder);
    return this.http.get<HierarchyNode[]>(`${this.base}/hierarchy`, { params });
  }
}