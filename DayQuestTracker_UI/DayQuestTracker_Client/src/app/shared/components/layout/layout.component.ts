import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { logout } from '../../../store/auth/auth.actions';
import {
  selectCurrentUser,
  selectUserLevel,
  selectUserXP,
} from '../../../store/auth/auth.selectors';
import { AuthUser } from '../../../core/models/auth.model';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './layout.component.html',
})
export class LayoutComponent implements OnInit {
  user$!: Observable<AuthUser | null>;
  level$!: Observable<number>;
  xp$!: Observable<number>;

  navItems = [
    { path: '/dashboard', label: 'Dashboard', icon: '🏠' },
    { path: '/categories', label: 'Categories', icon: '📁' },
    { path: '/tasks', label: 'Tasks', icon: '✅' },
    { path: '/analytics', label: 'Analytics', icon: '📊' },
    { path: '/profile', label: 'Profile', icon: '👤' },
  ];

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.user$ = this.store.select(selectCurrentUser);
    this.level$ = this.store.select(selectUserLevel);
    this.xp$ = this.store.select(selectUserXP);
  }

  onLogout(): void {
    this.store.dispatch(logout());
  }

  // XP needed for next level
  xpForNextLevel(currentXP: number): number {
    const currentLevel = Math.floor(currentXP / 500) + 1;
    return currentLevel * 500;
  }

  // Progress percentage to next level
  xpProgress(currentXP: number): number {
    const xpInCurrentLevel = currentXP % 500;
    return Math.round((xpInCurrentLevel / 500) * 100);
  }
}
