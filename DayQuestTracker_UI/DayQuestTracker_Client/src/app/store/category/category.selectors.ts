import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CategoryState } from './category.reducer';

export const selectCategoryState = createFeatureSelector<CategoryState>('categories');

export const selectAllCategories = createSelector(selectCategoryState, (state) => state.categories);

export const selectCategoriesLoading = createSelector(
  selectCategoryState,
  (state) => state.isLoading,
);

export const selectCategoriesSaving = createSelector(
  selectCategoryState,
  (state) => state.isSaving,
);

export const selectCategoryError = createSelector(selectCategoryState, (state) => state.error);

export const selectCategoryById = (id: string) =>
  createSelector(selectAllCategories, (categories) => categories.find((c) => c.id === id));
