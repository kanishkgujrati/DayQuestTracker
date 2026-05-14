import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { CategoryService } from '../../core/services/category.service';
import * as CategoryActions from './category.actions';

@Injectable()
export class CategoryEffects {
  private actions$ = inject(Actions);
  private categoryService = inject(CategoryService);

  loadCategories$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.loadCategories),
      switchMap(() =>
        this.categoryService.getCategories().pipe(
          map((categories) => CategoryActions.loadCategoriesSuccess({ categories })),
          catchError((error) =>
            of(
              CategoryActions.loadCategoriesFailure({
                error: error.error?.error || 'Failed to load categories.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  createCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.createCategory),
      switchMap(({ request }) =>
        this.categoryService.createCategory(request).pipe(
          map((category) => CategoryActions.createCategorySuccess({ category })),
          catchError((error) =>
            of(
              CategoryActions.createCategoryFailure({
                error:
                  error.error?.error ||
                  error.error?.errors?.Name?.[0] ||
                  'Failed to create category.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  updateCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.updateCategory),
      switchMap(({ id, request }) =>
        this.categoryService.updateCategory(id, request).pipe(
          map((category) => CategoryActions.updateCategorySuccess({ category })),
          catchError((error) =>
            of(
              CategoryActions.updateCategoryFailure({
                error: error.error?.error || 'Failed to update category.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  deleteCategory$ = createEffect(() =>
    this.actions$.pipe(
      ofType(CategoryActions.deleteCategory),
      switchMap(({ id, force }) =>
        this.categoryService.deleteCategory(id, force).pipe(
          map(() => CategoryActions.deleteCategorySuccess({ id })),
          catchError((error) =>
            of(
              CategoryActions.deleteCategoryFailure({
                error: error.error?.error || 'Failed to delete category.',
              }),
            ),
          ),
        ),
      ),
    ),
  );
}
