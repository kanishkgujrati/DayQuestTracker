export interface Category {
  id: string;
  name: string;
  color: string;
  icon: string | null;
  createdAt: string;
}

export interface CreateCategoryRequest {
  name: string;
  color: string;
  icon?: string;
}

export interface UpdateCategoryRequest {
  name?: string;
  color?: string;
  icon?: string;
}
