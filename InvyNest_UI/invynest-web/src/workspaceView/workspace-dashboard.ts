import { Component } from '@angular/core';
import { WorkspaceList } from './workspaceView/workspace-list/workspace-list';
import { DrawerModule } from 'primeng/drawer';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-workspace-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, WorkspaceList, DrawerModule, ButtonModule],
  templateUrl: './workspace-dashboard.html',
  styleUrl: './workspace-dashboard.scss',
})
export class WorkspaceDashboard {
  showSidebar = false;
  mobileMode = false;

  constructor() {
    this.mobileMode = window.matchMedia('(max-width: 767px)').matches;
    window.matchMedia('(max-width: 767px)').addEventListener('change', (e) => {
      this.mobileMode = e.matches;
    });
  }

  onToggleSidebar() {
    this.showSidebar = !this.showSidebar;
  }
}
