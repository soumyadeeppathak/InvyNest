import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: 'workspaces',
        loadComponent: () => import('../workspaceView/workspaceView/workspace-list/workspace-list').then(m => m.WorkspaceList)
    }
];
