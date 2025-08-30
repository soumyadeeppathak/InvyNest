import { Component, Input } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-workspace-add-member-dialog',
  imports: [CommonModule, DialogModule, FormsModule, ButtonModule, InputTextModule],
  templateUrl: './workspace-add-member-dialog.html',
  styleUrl: './workspace-add-member-dialog.scss'
})
export class WorkspaceAddMemberDialog {
  @Input() output: (memberName: string, memberEmail: string | null) => void = () => {};
  
  display = false;
  memberName = '';
  memberEmail = '';
  emailError = '';
  nameError = '';

  open() {
    this.display = true;
    this.memberName = '';
    this.memberEmail = '';
    this.emailError = '';
    this.nameError = '';
  }

  close() {
    this.display = false;
  }

  private validateEmail(email: string): boolean {
    if (!email.trim()) return true; // Email is optional
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email.trim());
  }

  private validateForm(): boolean {
    this.emailError = '';
    this.nameError = '';

    // At least one field must be filled
    if (!this.memberName.trim() && !this.memberEmail.trim()) {
      this.nameError = 'Either name or email must be provided';
      return false;
    }

    // Validate email format if provided
    if (this.memberEmail.trim() && !this.validateEmail(this.memberEmail)) {
      this.emailError = 'Please enter a valid email address';
      return false;
    }

    return true;
  }

  addMember() {
    if (this.validateForm()) {
      const email = this.memberEmail.trim() || null;
      this.output(this.memberName.trim(), email);
      this.close();
    }
  }
}
