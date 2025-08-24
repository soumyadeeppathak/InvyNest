import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: 'workspaces',
        loadComponent: () => import('../workspaceView/workspaceView/workspace-list/workspace-list').then(m => m.WorkspaceList)
    },
    {
        path: 'workspaces/:id',
        loadComponent: () => import('../workspaceView/workspaceView/workspace-detail/workspace-detail').then(m => m.WorkspaceDetail)
    }
];
