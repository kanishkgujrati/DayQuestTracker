import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { Category } from '../../core/models/category.models';
import {
  loadCategories,
  createCategory,
  updateCategory,
  deleteCategory,
  clearCategoryError,
} from '../../store/category/category.actions';
import {
  selectAllCategories,
  selectCategoriesLoading,
  selectCategoriesSaving,
  selectCategoryError,
} from '../../store/category/category.selectors';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.component.html',
})
export class CategoriesComponent implements OnInit {
  categories$!: Observable<Category[]>;
  isLoading$!: Observable<boolean>;
  isSaving$!: Observable<boolean>;
  error$!: Observable<string | null>;

  showForm = false;
  editingCategory: Category | null = null;
  deletingId: string | null = null;
  categoryForm!: FormGroup;

  colorOptions = [
    '#FF5733',
    '#FF8C00',
    '#FFD700',
    '#32CD32',
    '#00CED1',
    '#1E90FF',
    '#8A2BE2',
    '#FF69B4',
    '#00FA9A',
    '#FF6347',
    '#4169E1',
    '#DC143C',
  ];

  iconOptions = [
    'dumbbell',
    'brain',
    'briefcase',
    'apple',
    'moon',
    'heart',
    'book',
    'music',
    'run',
    'meditation',
    'code',
    'money',
  ];

  iconEmojis: { [key: string]: string } = {
    dumbbell: '🏋️',
    brain: '🧠',
    briefcase: '💼',
    apple: '🍎',
    moon: '🌙',
    heart: '❤️',
    book: '📚',
    music: '🎵',
    run: '🏃',
    meditation: '🧘',
    code: '💻',
    money: '💰',
  };

  constructor(
    private store: Store,
    private fb: FormBuilder,
  ) {}

  ngOnInit(): void {
    this.categories$ = this.store.select(selectAllCategories);
    this.isLoading$ = this.store.select(selectCategoriesLoading);
    this.isSaving$ = this.store.select(selectCategoriesSaving);
    this.error$ = this.store.select(selectCategoryError);

    this.store.dispatch(loadCategories());
    this.initForm();
  }

  initForm(category?: Category): void {
    this.categoryForm = this.fb.group({
      name: [category?.name ?? '', [Validators.required, Validators.maxLength(100)]],
      color: [category?.color ?? '#1E90FF', Validators.required],
      icon: [category?.icon ?? null],
    });
  }

  onAddNew(): void {
    this.editingCategory = null;
    this.initForm();
    this.showForm = true;
    this.store.dispatch(clearCategoryError());
  }

  onEdit(category: Category): void {
    this.editingCategory = category;
    this.initForm(category);
    this.showForm = true;
    this.store.dispatch(clearCategoryError());
  }

  onCancel(): void {
    this.showForm = false;
    this.editingCategory = null;
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) return;

    const { name, color, icon } = this.categoryForm.value;

    if (this.editingCategory) {
      this.store.dispatch(
        updateCategory({
          id: this.editingCategory.id,
          request: { name, color, icon },
        }),
      );
    } else {
      this.store.dispatch(
        createCategory({
          request: { name, color, icon },
        }),
      );
    }

    this.showForm = false;
    this.editingCategory = null;
  }

  onDelete(id: string): void {
    this.deletingId = id;
  }

  confirmDelete(force = false): void {
    if (!this.deletingId) return;
    this.store.dispatch(
      deleteCategory({
        id: this.deletingId,
        force,
      }),
    );
    this.deletingId = null;
  }

  cancelDelete(): void {
    this.deletingId = null;
  }

  getIconEmoji(icon: string | null): string {
    if (!icon) return '📁';
    return this.iconEmojis[icon] ?? '📁';
  }
}
