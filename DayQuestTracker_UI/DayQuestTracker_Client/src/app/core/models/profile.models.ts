export interface UserProfile {
  id: string;
  email: string;
  username: string;
  timezone: string;
  totalXP: number;
  level: number;
  createdAt: string;
}

export interface UpdateProfileRequest {
  username?: string;
  timezone?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
