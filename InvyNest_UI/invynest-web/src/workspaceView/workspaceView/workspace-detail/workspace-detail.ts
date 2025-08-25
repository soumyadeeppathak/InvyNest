import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ItemService, HierarchyNode, CreateWorkspaceItemDto, UpdateItemNameDto, UpdateItemQuantityDto } from '../../../services/item-service';

@Component({
  selector: 'app-workspace-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule],
  templateUrl: './workspace-detail.html',
  styleUrl: './workspace-detail.scss',
})
export class WorkspaceDetail implements OnInit {
  workspaceId!: string;
  members = signal<{ holder: string, items: HierarchyNode[] }[]>([]);
  loading = signal(true);
  editingItemId: string | null = null;
  editName: string = '';
  editQuantity: number | null = null;

  constructor(private route: ActivatedRoute, private itemService: ItemService) {}

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
    // Example: fetch holders (members) for this workspace, then for each, fetch hierarchy
    // Replace with actual member fetching logic as needed
    const holders = ['Taniya']; // TODO: Replace with real member list
    Promise.all(
      holders.map(holder =>
        this.itemService.getHierarchy(this.workspaceId, holder).toPromise().then((items: HierarchyNode[] | null | undefined) => ({
          holder,
          items: items ?? []
        }))
      )
    ).then(results => {
      this.members.set(results);
      this.loading.set(false);
    });
  }

  addItem(holder: string, parentItemId?: string) {
    // Open dialog or inline form to collect item details, then:
    // this.itemService.createItem({ ...dto, WorkspaceId: this.workspaceId, Holder: holder, ParentWorkspaceItemId: parentItemId })
    //   .subscribe(() => this.fetchMembersAndItems());
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
    // this.itemService.deleteItem(itemId)
    //   .subscribe(() => this.fetchMembersAndItems());
  }

  canAddChild(item: HierarchyNode): boolean {
    // Only allow add if item has no children
    return !item.children || item.children.length === 0;
  }
}