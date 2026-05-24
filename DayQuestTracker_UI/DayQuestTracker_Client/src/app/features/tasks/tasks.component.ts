import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { HabitTask, FrequencyType } from '../../core/models/task.models';
import { Category } from '../../core/models/category.models';
import {
  loadTasks,
  createTask,
  updateTask,
  deleteTask,
  clearTaskError,
} from '../../store/tasks/task.actions';
import {
  selectAllTasks,
  selectTasksLoading,
  selectTasksSaving,
  selectTaskError,
} from '../../store/tasks/task.selectors';
import { loadCategories } from '../../store/category/category.actions';
import { selectAllCategories } from '../../store/category/category.selectors';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tasks.component.html',
})
export class TasksComponent implements OnInit {
  tasks$!: Observable<HabitTask[]>;
  categories$!: Observable<Category[]>;
  isLoading$!: Observable<boolean>;
  isSaving$!: Observable<boolean>;
  error$!: Observable<string | null>;

  showForm = false;
  editingTask: HabitTask | null = null;
  deletingId: string | null = null;
  taskForm!: FormGroup;

  FrequencyType = FrequencyType;

  dayNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  difficultyLabels: { [key: number]: string } = {
    1: 'Very Easy',
    2: 'Easy',
    3: 'Medium',
    4: 'Hard',
    5: 'Very Hard',
  };

  constructor(
    private store: Store,
    private fb: FormBuilder,
  ) {}

  ngOnInit(): void {
    this.tasks$ = this.store.select(selectAllTasks);
    this.categories$ = this.store.select(selectAllCategories);
    this.isLoading$ = this.store.select(selectTasksLoading);
    this.isSaving$ = this.store.select(selectTasksSaving);
    this.error$ = this.store.select(selectTaskError);

    this.store.dispatch(loadTasks({}));
    this.store.dispatch(loadCategories());
    this.initForm();
  }

  initForm(task?: HabitTask): void {
    this.taskForm = this.fb.group({
      categoryId: [task?.categoryId ?? '', Validators.required],
      title: [task?.title ?? '', [Validators.required, Validators.maxLength(200)]],
      description: [task?.description ?? ''],
      difficulty: [
        task?.difficulty ?? 3,
        [Validators.required, Validators.min(1), Validators.max(5)],
      ],
      frequencyType: [task?.frequencyType ?? FrequencyType.Daily, Validators.required],
      targetPerWeek: [task?.targetPerWeek ?? null],
      scheduledDays: [task?.scheduledDays ?? []],
    });
  }

  onAddNew(): void {
    this.editingTask = null;
    this.initForm();
    this.showForm = true;
    this.store.dispatch(clearTaskError());
  }

  onEdit(task: HabitTask): void {
    this.editingTask = task;
    this.initForm(task);
    this.showForm = true;
    this.store.dispatch(clearTaskError());
  }

  onCancel(): void {
    this.showForm = false;
    this.editingTask = null;
  }

  onSubmit(): void {
    if (this.taskForm.invalid) return;

    const formValue = this.taskForm.value;

    // Clean up based on frequency type
    if (
      [FrequencyType.Daily, FrequencyType.OnceAWeek, FrequencyType.OnceAMonth].includes(
        formValue.frequencyType,
      )
    ) {
      formValue.scheduledDays = null;
      formValue.targetPerWeek = null;
    } else if (formValue.frequencyType === FrequencyType.Weekly) {
      formValue.targetPerWeek = null;
    }

    if (this.editingTask) {
      this.store.dispatch(updateTask({ id: this.editingTask.id, request: formValue }));
    } else {
      this.store.dispatch(createTask({ request: formValue }));
    }

    this.showForm = false;
    this.editingTask = null;
  }

  onDelete(id: string): void {
    this.deletingId = id;
  }

  confirmDelete(): void {
    if (!this.deletingId) return;
    this.store.dispatch(deleteTask({ id: this.deletingId }));
    this.deletingId = null;
  }

  cancelDelete(): void {
    this.deletingId = null;
  }

  toggleDay(day: number): void {
    const current: number[] = this.taskForm.get('scheduledDays')?.value ?? [];
    const updated = current.includes(day)
      ? current.filter((d) => d !== day)
      : [...current, day].sort();
    this.taskForm.patchValue({ scheduledDays: updated });
  }

  isDaySelected(day: number): boolean {
    const days: number[] = this.taskForm.get('scheduledDays')?.value ?? [];
    return days.includes(day);
  }

  getFrequencyLabel(type: FrequencyType): string {
    switch (type) {
      case FrequencyType.Daily:
        return 'Daily';
      case FrequencyType.Weekly:
        return 'Weekly';
      case FrequencyType.Custom:
        return 'Custom';
      case FrequencyType.OnceAWeek:
        return 'Once a Week';
      case FrequencyType.OnceAMonth:
        return 'Once a Month';
    }
  }

  getScheduledDaysLabel(days: number[]): string {
    if (!days?.length) return '';
    return days.map((d) => this.dayNames[d]).join(', ');
  }

  getDifficultyStars(difficulty: number): string {
    return '⭐'.repeat(difficulty);
  }

  getXPColor(xp: number): string {
    if (xp >= 60) return 'text-purple-400';
    if (xp >= 40) return 'text-yellow-400';
    return 'text-green-400';
  }
}
