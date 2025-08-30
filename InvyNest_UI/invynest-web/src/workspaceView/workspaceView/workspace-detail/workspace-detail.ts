import { Component, OnInit, signal, ChangeDetectionStrategy, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ItemService, HierarchyNode, CreateWorkspaceItemDto, UpdateItemNameDto, UpdateItemQuantityDto } from '../../../services/item-service';
import { WorkspaceService, AddMemberDto, WorkspaceMemberDto } from '../../../services/workspace-service';
import { CardModule } from 'primeng/card';
import { WorkspaceAddMemberDialog } from '../../workspace-add-member-dialog/workspace-add-member-dialog';

@Component({
  selector: 'app-workspace-detail',
  imports: [CommonModule, FormsModule, ButtonModule, CardModule, WorkspaceAddMemberDialog],
  templateUrl: './workspace-detail.html',
  styleUrl: './workspace-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private itemService = inject(ItemService);
  private workspaceService = inject(WorkspaceService);
  
  workspaceId!: string;
  members = signal<{ holder: string, items: HierarchyNode[] }[]>([]);
  loading = signal(true);
  editingItemId: string | null = null;
  editName: string = '';
  editQuantity: number | null = null;
  addingItemParentId: string | null = null;
  addingItemHolder: string | null = null;
  newItemName: string = '';
  newItemQuantity: number | null = null;

  ngOnInit() {
    // Get workspaceId from route params
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.workspaceId = id;
        this.fetchMembersAndItems();
      }
    });
  }

  fetchMembersAndItems() {
    this.loading.set(true);
    
    // First fetch the workspace members
    this.workspaceService.getWorkspaceMembers(this.workspaceId).subscribe({
      next: (workspaceMembers: WorkspaceMemberDto[]) => {
        // Extract member names, using memberName or fallback to email
        const memberNames = workspaceMembers.map(member => 
          member.memberName || member.memberEmail || 'Unknown Member'
        );
        
        // For each member, fetch their items
        Promise.all(
          memberNames.map(memberName =>
            this.itemService.getHierarchy(this.workspaceId, memberName)
              .toPromise()
              .then((items: HierarchyNode[] | null | undefined) => ({
                holder: memberName,
                items: items ?? []
              }))
          )
        ).then(results => {
          this.members.set(results);
          this.loading.set(false);
        }).catch(error => {
          console.error('Error fetching items for members:', error);
          this.loading.set(false);
        });
      },
      error: (error) => {
        console.error('Error fetching workspace members:', error);
        // Fallback to hardcoded member if API fails
        const holders = ['DevLocal1']; // Default user as mentioned in backend
        Promise.all(
          holders.map(holder =>
            this.itemService.getHierarchy(this.workspaceId, holder)
              .toPromise()
              .then((items: HierarchyNode[] | null | undefined) => ({
                holder,
                items: items ?? []
              }))
          )
        ).then(results => {
          this.members.set(results);
          this.loading.set(false);
        });
      }
    });
  }

  addItem(holder: string, parentItemId?: string) {
    // Show inline form for adding item (parent or child)
    this.addingItemHolder = holder;
    this.addingItemParentId = parentItemId ?? null;
    this.newItemName = '';
    this.newItemQuantity = null;
  }

  saveNewItem() {
    if (!this.addingItemHolder || !this.newItemName || this.newItemQuantity == null) return;
    const dto: CreateWorkspaceItemDto = {
      name: this.newItemName,
      quantity: this.newItemQuantity,
      workspaceId: this.workspaceId,
      holder: this.addingItemHolder,
      parentWorkspaceItemId: this.addingItemParentId || undefined
    };
    this.itemService.createItem(dto)
      .subscribe(() => {
        this.fetchMembersAndItems();
        this.cancelAddItem();
      });
  }

  cancelAddItem() {
    this.addingItemHolder = null;
    this.addingItemParentId = null;
    this.newItemName = '';
    this.newItemQuantity = null;
  }

  startEdit(item: any) {
    this.editingItemId = item.id;
    this.editName = item.name;
    this.editQuantity = item.quantity;
  }

  saveEdit(item: any) {
    if (this.editingItemId) {
      const updateName = item.name !== this.editName;
      const updateQuantity = item.quantity !== this.editQuantity;
      const actions: Promise<any>[] = [];
      if (updateName) {
        actions.push(this.itemService.updateItemName(item.id, { name: this.editName }).toPromise());
      }
      if (updateQuantity) {
        actions.push(this.itemService.updateItemQuantity(item.id, { quantity: this.editQuantity ?? 0 }).toPromise());
      }
      Promise.all(actions).then(() => {
        this.fetchMembersAndItems();
        this.editingItemId = null;
      });
      if (!updateName && !updateQuantity) {
        this.editingItemId = null;
      }
    }
  }

  cancelEdit() {
    this.editingItemId = null;
  }

  deleteItem(itemId: string) {
    this.itemService.deleteItem(itemId)
      .subscribe(() => this.fetchMembersAndItems());
  }

  canAddChild(item: HierarchyNode): boolean {
    // Only allow add if item has no children
    return !item.children || item.children.length === 0;
  }

  isParentItem(item: HierarchyNode, member: { holder: string, items: HierarchyNode[] }): boolean {
    // Only top-level items in member.items are parents
    return member.items.some(i => i.id === item.id);
  }

  onAddMember = (memberName: string, memberEmail: string | null) => {
    const addMemberDto: AddMemberDto = {
      memberName,
      memberEmail,
      role: 'editor' // Default role as mentioned
    };
    
    this.workspaceService.addMember(this.workspaceId, addMemberDto).subscribe({
      next: () => {
        // Refresh the members list to show the new member
        this.fetchMembersAndItems();
      },
      error: (error: any) => {
        console.error('Error adding member:', error);
      }
    });
  };
}