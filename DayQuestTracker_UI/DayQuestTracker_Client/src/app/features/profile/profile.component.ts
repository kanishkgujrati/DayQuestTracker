import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { ProfileService } from '../../core/services/profile.service';
import { UserProfile } from '../../core/models/profile.models';
import { selectCurrentUser } from '../../store/auth/auth.selectors';
import { AuthUser } from '../../core/models/auth.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  user$!: Observable<AuthUser | null>;

  profile: UserProfile | null = null;
  isLoadingProfile = false;
  isSavingProfile = false;
  isSavingPassword = false;

  profileError: string | null = null;
  profileSuccess: string | null = null;
  passwordError: string | null = null;
  passwordSuccess: string | null = null;

  photoUrl: string | null = null;
  isUploadingPhoto = false;
  photoError: string | null = null;

  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  activeTab: 'profile' | 'password' = 'profile';

  timezones = [
    'UTC',
    'Asia/Kolkata',
    'Asia/Dubai',
    'Asia/Singapore',
    'Europe/London',
    'Europe/Paris',
    'America/New_York',
    'America/Chicago',
    'America/Denver',
    'America/Los_Angeles',
    'Australia/Sydney',
    'Pacific/Auckland',
  ];

  constructor(
    private store: Store,
    private profileService: ProfileService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.user$ = this.store.select(selectCurrentUser);
    this.initForms();
    this.loadProfile();
  }

  initForms(): void {
    this.profileForm = this.fb.group({
      username: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern(/^[a-zA-Z0-9_ ]+$/),
        ],
      ],
      timezone: ['UTC', Validators.required],
    });

    this.passwordForm = this.fb.group(
      {
        currentPassword: ['', Validators.required],
        newPassword: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.pattern(/^(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{8,}$/),
          ],
        ],
        confirmNewPassword: ['', Validators.required],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPass = group.get('newPassword')?.value;
    const confirm = group.get('confirmNewPassword')?.value;
    return newPass === confirm ? null : { passwordMismatch: true };
  }

  loadProfile(): void {
    this.isLoadingProfile = false;
    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.profileForm.patchValue({
          username: profile.username,
          timezone: profile.timezone,
        });
        this.isLoadingProfile = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.profileError = 'Failed to load profile.';
        this.isLoadingProfile = false;
        this.cdr.detectChanges();
      },
    });
  }

  onUpdateProfile(): void {
    if (this.profileForm.invalid) return;

    this.isSavingProfile = true;
    this.profileError = null;
    this.profileSuccess = null;

    this.profileService.updateProfile(this.profileForm.value).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.profileSuccess = 'Profile updated successfully.';
        this.isSavingProfile = false;
        setTimeout(() => (this.profileSuccess = null), 3000);
      },
      error: (err) => {
        this.profileError = err.error?.error || 'Failed to update profile.';
        this.isSavingProfile = false;
      },
    });
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid) return;

    this.isSavingPassword = true;
    this.passwordError = null;
    this.passwordSuccess = null;

    this.profileService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.passwordSuccess = 'Password changed. Please log in again.';
        this.passwordForm.reset();
        this.isSavingPassword = false;
        setTimeout(() => (this.passwordSuccess = null), 5000);
      },
      error: (err) => {
        this.passwordError = err.error?.error || 'Failed to change password.';
        this.isSavingPassword = false;
      },
    });
  }

  getLevelProgress(totalXP: number): number {
    return ((totalXP % 500) / 500) * 100;
  }

  getXPToNextLevel(totalXP: number): number {
    return 500 - (totalXP % 500);
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    this.isUploadingPhoto = true;
    this.photoError = null;

    this.profileService.uploadPhoto(file).subscribe({
      next: (response) => {
        this.photoUrl = response.photoUrl;
        if (this.profile) this.profile.profilePhotoUrl = response.photoUrl;
        this.isUploadingPhoto = false;
      },
      error: (err) => {
        this.photoError = err.error?.error || 'Failed to upload photo.';
        this.isUploadingPhoto = false;
      },
    });
  }
}
