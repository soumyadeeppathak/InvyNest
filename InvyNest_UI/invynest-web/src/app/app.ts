import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { WorkspaceList } from "../workspaceView/workspaceView/workspace-list/workspace-list";

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    WorkspaceList
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('invynest-web');
}
