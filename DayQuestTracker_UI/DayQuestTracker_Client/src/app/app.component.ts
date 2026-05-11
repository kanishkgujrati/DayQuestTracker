import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { initializeAuth } from './store/auth/auth.actions';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: 'app.html',
  styleUrl: './app.scss',
  standalone: true,
})
export class AppComponent implements OnInit {
  constructor(private store: Store) {}

  ngOnInit(): void {
    // Restore auth state from localStorage on every page refresh
    this.store.dispatch(initializeAuth());
  }
}
