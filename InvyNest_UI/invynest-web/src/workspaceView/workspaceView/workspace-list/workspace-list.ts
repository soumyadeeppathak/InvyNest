import { Component, OnInit, ChangeDetectionStrategy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { WorkspaceDto, WorkspaceService } from '../../../services/workspace-service';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { PrimeIcons, MenuItem } from 'primeng/api';
import { WorkspaceCreateDialog } from '../../workspace-create-dialog/workspace-create-dialog';

@Component({
  selector: 'app-workspace-list',
  imports: [CommonModule, ButtonModule, WorkspaceCreateDialog, ProgressSpinnerModule],
  templateUrl: './workspace-list.html',
  styleUrl: './workspace-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceList implements OnInit {
  public workspaceService = inject(WorkspaceService);
  private router = inject(Router);
  
  testEmail = 'you@example.com';

  ngOnInit(): void {
    this.workspaceService.fetchMyWorkspaces(this.testEmail);
  }

  onCreateWorkspace = (name: string) => {
    this.workspaceService.createWorkspace({ name, ownerEmail: this.testEmail }, this.testEmail);
  };

  onDeleteWorkspace = (id: string) => {
    this.workspaceService.deleteWorkspace(id, this.testEmail);
  };

  onWorkspaceClick(id: string) {
    this.router.navigate(['/workspaces', id]);
  }
}
