import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { logout } from '../../store/auth/auth.actions';
import { selectCurrentUser } from '../../store/auth/auth.selectors';
import { Observable } from 'rxjs';
import { AuthUser } from '../../core/models/auth.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-slate-900 text-white p-8">
      <div class="max-w-4xl mx-auto">
        <div class="flex justify-between items-center mb-8">
          <div>
            <h1 class="text-3xl font-bold">Dashboard</h1>
            @if (user$ | async; as user) {
              <p class="text-slate-400 mt-1">
                Welcome back, {{ user.username }} — Level {{ user.level }}
              </p>
            }
          </div>
          <button
            (click)="onLogout()"
            class="bg-slate-700 hover:bg-slate-600 px-4 py-2 rounded-lg text-sm transition"
          >
            Logout
          </button>
        </div>
        <div class="bg-slate-800 rounded-2xl p-8 border border-slate-700">
          <p class="text-slate-400">Dashboard coming soon...</p>
        </div>
      </div>
    </div>
  `,
})
export class DashboardComponent implements OnInit {
  user$!: Observable<AuthUser | null>;

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.user$ = this.store.select(selectCurrentUser);
  }

  onLogout(): void {
    this.store.dispatch(logout());
  }
}
