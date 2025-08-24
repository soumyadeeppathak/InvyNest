import { Component, OnInit } from '@angular/core';
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
  styleUrl: './workspace-list.scss'
})
export class WorkspaceList implements OnInit {
  testEmail = 'you@example.com';
  constructor(public workspaceService: WorkspaceService, private router: Router) { }

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
