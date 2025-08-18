import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkspaceDto, WorkspaceService } from '../../../services/workspace-service';

@Component({
  selector: 'app-workspace-list',
  imports: [CommonModule],
  templateUrl: './workspace-list.html',
  styleUrl: './workspace-list.scss'
})
export class WorkspaceList implements OnInit {
  workspaces: WorkspaceDto[] = [];
  loading = true;
  testEmail = 'you@example.com';
  constructor(private workspaceService: WorkspaceService) {}

  ngOnInit(): void {
    this.loading = true;
    this.workspaceService.getMyWorkspaces(this.testEmail).subscribe({
      next: (data) => {
        this.workspaces = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load workspaces:', err);
        this.loading = false;
      }
    });
  }
}
