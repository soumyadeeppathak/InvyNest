import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Workspace, WorkspaceService } from '../../../services/workspace-service';

@Component({
  selector: 'app-workspace-list',
  imports: [CommonModule],
  templateUrl: './workspace-list.html',
  styleUrl: './workspace-list.scss'
})
export class WorkspaceList implements OnInit {
  workspaces: Workspace[] = [];
  loading = true;
  testEmail = 'you@example.com';
  constructor(private workspaceService: WorkspaceService) {}

  ngOnInit(): void {
    this.workspaceService.getUserWorkspaces(this.testEmail).subscribe({
      next: (data) => {
        this.workspaces = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
