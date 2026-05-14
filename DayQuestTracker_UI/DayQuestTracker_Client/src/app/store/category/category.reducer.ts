import { createReducer, on } from '@ngrx/store';
import { Category } from '../../core/models/category.models';
import * as CategoryActions from './category.actions';

export interface CategoryState {
  categories: Category[];
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
}

export const initialState: CategoryState = {
  categories: [],
  isLoading: false,
  isSaving: false,
  error: null,
};

export const categoryReducer = createReducer(
  initialState,

  on(CategoryActions.loadCategories, (state) => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(CategoryActions.loadCategoriesSuccess, (state, { categories }) => ({
    ...state,
    isLoading: false,
    categories,
  })),

  on(CategoryActions.loadCategoriesFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(CategoryActions.createCategory, (state) => ({
    ...state,
    isSaving: true,
    error: null,
  })),

  on(CategoryActions.createCategorySuccess, (state, { category }) => ({
    ...state,
    isSaving: false,
    categories: [...state.categories, category],
  })),

  on(CategoryActions.createCategoryFailure, (state, { error }) => ({
    ...state,
    isSaving: false,
    error,
  })),

  on(CategoryActions.updateCategory, (state) => ({
    ...state,
    isSaving: true,
    error: null,
  })),

  on(CategoryActions.updateCategorySuccess, (state, { category }) => ({
    ...state,
    isSaving: false,
    categories: state.categories.map((c) => (c.id === category.id ? category : c)),
  })),

  on(CategoryActions.updateCategoryFailure, (state, { error }) => ({
    ...state,
    isSaving: false,
    error,
  })),

  on(CategoryActions.deleteCategory, (state) => ({
    ...state,
    isSaving: true,
    error: null,
  })),

  on(CategoryActions.deleteCategorySuccess, (state, { id }) => ({
    ...state,
    isSaving: false,
    categories: state.categories.filter((c) => c.id !== id),
  })),

  on(CategoryActions.deleteCategoryFailure, (state, { error }) => ({
    ...state,
    isSaving: false,
    error,
  })),

  on(CategoryActions.clearCategoryError, (state) => ({
    ...state,
    error: null,
  })),
);
