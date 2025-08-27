
import { Component, Input } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-workspace-create-dialog',
  imports: [DialogModule, FormsModule, ButtonModule, InputTextModule],
  templateUrl: './workspace-create-dialog.html',
  styleUrl: './workspace-create-dialog.scss'
})
export class WorkspaceCreateDialog {
  @Input() output: (name: string) => void = () => {};
  
  display = false;
  workspaceName = '';

  open() {
    this.display = true;
    this.workspaceName = '';
  }

  close() {
    this.display = false;
  }

  create() {
    if (this.workspaceName.trim()) {
      this.output(this.workspaceName.trim());
      this.close();
    }
  }
}
