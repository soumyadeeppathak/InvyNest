import { Component, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { WorkspaceList } from "../workspaceView/workspaceView/workspace-list/workspace-list";
import { WorkspaceDashboard } from "../workspaceView/workspace-dashboard";

@Component({
  selector: 'app-root',
  imports: [
    WorkspaceDashboard
],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  protected readonly title = signal('invynest-web');
}
