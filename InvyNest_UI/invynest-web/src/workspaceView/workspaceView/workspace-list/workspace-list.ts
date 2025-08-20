import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkspaceDto, WorkspaceService } from '../../../services/workspace-service';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { WorkspaceCreateDialog } from '../../workspace-create-dialog/workspace-create-dialog';

@Component({
  selector: 'app-workspace-list',
  imports: [CommonModule, ButtonModule, WorkspaceCreateDialog, ProgressSpinnerModule],
  templateUrl: './workspace-list.html',
  styleUrl: './workspace-list.scss'
})
export class WorkspaceList implements OnInit {
  testEmail = 'you@example.com';
  constructor(public workspaceService: WorkspaceService) { }

  ngOnInit(): void {
    this.workspaceService.fetchMyWorkspaces(this.testEmail);
  }

  onCreateWorkspace = (name: string) => {
    this.workspaceService.createWorkspace({ name, ownerEmail: this.testEmail }, this.testEmail);
  };
}
