import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ItemService, HierarchyNode, CreateWorkspaceItemDto, UpdateItemNameDto, UpdateItemQuantityDto } from '../../../services/item-service';

@Component({
  selector: 'app-workspace-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './workspace-detail.html',
  styleUrl: './workspace-detail.scss',
})
export class WorkspaceDetail implements OnInit {
  workspaceId!: string;
  members = signal<{ holder: string, items: HierarchyNode[] }[]>([]);
  loading = signal(true);
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

  editItem(itemId: string, name: string) {
    // this.itemService.updateItemName(itemId, { Name: name })
    //   .subscribe(() => this.fetchMembersAndItems());
  }

  updateQuantity(itemId: string, quantity: number) {
    // this.itemService.updateItemQuantity(itemId, { Quantity: quantity })
    //   .subscribe(() => this.fetchMembersAndItems());
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