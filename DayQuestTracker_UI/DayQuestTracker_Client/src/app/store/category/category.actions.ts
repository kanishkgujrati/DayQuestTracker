import { createAction, props } from '@ngrx/store';
import {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../../core/models/category.models';

export const loadCategories = createAction('[Categories] Load Categories');

export const loadCategoriesSuccess = createAction(
  '[Categories] Load Categories Success',
  props<{ categories: Category[] }>(),
);

export const loadCategoriesFailure = createAction(
  '[Categories] Load Categories Failure',
  props<{ error: string }>(),
);

export const createCategory = createAction(
  '[Categories] Create Category',
  props<{ request: CreateCategoryRequest }>(),
);

export const createCategorySuccess = createAction(
  '[Categories] Create Category Success',
  props<{ category: Category }>(),
);

export const createCategoryFailure = createAction(
  '[Categories] Create Category Failure',
  props<{ error: string }>(),
);

export const updateCategory = createAction(
  '[Categories] Update Category',
  props<{ id: string; request: UpdateCategoryRequest }>(),
);

export const updateCategorySuccess = createAction(
  '[Categories] Update Category Success',
  props<{ category: Category }>(),
);

export const updateCategoryFailure = createAction(
  '[Categories] Update Category Failure',
  props<{ error: string }>(),
);

export const deleteCategory = createAction(
  '[Categories] Delete Category',
  props<{ id: string; force: boolean }>(),
);

export const deleteCategorySuccess = createAction(
  '[Categories] Delete Category Success',
  props<{ id: string }>(),
);

export const deleteCategoryFailure = createAction(
  '[Categories] Delete Category Failure',
  props<{ error: string }>(),
);

export const clearCategoryError = createAction('[Categories] Clear Error');
