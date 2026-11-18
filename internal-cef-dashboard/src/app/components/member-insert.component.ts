import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-member-insert',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="card p-4 shadow-sm" style="max-width: 600px; margin: 2rem auto;">
      <h3 class="mb-3">Enroll New Member</h3>

      <form [formGroup]="memberForm" (ngSubmit)="onSubmit()">
        <!-- Member ID -->
        <div class="mb-3">
          <label class="form-label">Member ID</label>
          <input type="text" class="form-control" formControlName="memberId" placeholder="e.g., MBR-2026-TN01" />
        </div>

        <!-- First Name & Last Name -->
        <div class="row mb-3">
          <div class="col">
            <label class="form-label">First Name</label>
            <input type="text" class="form-control" formControlName="firstName" placeholder="First Name" />
          </div>
          <div class="col">
            <label class="form-label">Last Name</label>
            <input type="text" class="form-control" formControlName="lastName" placeholder="Last Name" />
          </div>
        </div>

        <!-- Date of Birth & Gender -->
        <div class="row mb-3">
          <div class="col">
            <label class="form-label">Date of Birth</label>
            <input type="date" class="form-control" formControlName="dateOfBirth" />
          </div>
          <div class="col">
            <label class="form-label">Gender</label>
            <select class="form-select" formControlName="gender">
              <option value="M">Male (M)</option>
              <option value="F">Female (F)</option>
              <option value="O">Other (O)</option>
            </select>
          </div>
        </div>

        <!-- Member Status (Matches PostgreSQL chk_member_status) -->
        <div class="mb-3">
          <label class="form-label">Member Status</label>
          <select class="form-select" formControlName="memberStatus">
            <option *ngFor="let status of validMemberStatuses" [value]="status">
              {{ status }}
            </option>
          </select>
        </div>

        <button type="submit" class="btn btn-primary w-100" [disabled]="memberForm.invalid || isSubmitting">
          {{ isSubmitting ? 'Saving Member Record...' : 'Insert Member' }}
        </button>
      </form>

      <div *ngIf="statusMessage" class="alert mt-3" [ngClass]="isError ? 'alert-danger' : 'alert-success'">
        {{ statusMessage }}
      </div>
    </div>
  `
})
export class MemberInsertComponent {
  memberForm: FormGroup;
  isSubmitting = false;
  statusMessage = '';
  isError = false;

  // Exact match to PostgreSQL chk_member_status
  validMemberStatuses = ['INCOMPLETE', 'ACTIVE', 'INACTIVE'];

  private apiUrl = 'http://localhost:5141/api/members';

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.memberForm = this.fb.group({
      memberId: ['', [Validators.required, Validators.maxLength(30)]],
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      dateOfBirth: ['', Validators.required],
      gender: ['M', [Validators.required, Validators.maxLength(1)]],
      memberStatus: ['INCOMPLETE', Validators.required] // Default: INCOMPLETE
    });
  }

  onSubmit(): void {
    if (this.memberForm.invalid) return;

    this.isSubmitting = true;
    this.statusMessage = '';

    this.http.post(this.apiUrl, this.memberForm.value).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.isError = false;
        this.statusMessage = `Member ${this.memberForm.value.memberId} enrolled successfully.`;
        this.memberForm.reset({ gender: 'M', memberStatus: 'INCOMPLETE' });
      },
      error: (err) => {
        this.isSubmitting = false;
        this.isError = true;
        this.statusMessage = `Insert failed: ${err.error?.message || err.message}`;
      }
    });
  }
}
