import { TestBed } from '@angular/core/testing';

import { RecipeService } from '../api-contracts/services/recipe.service';

describe('RecipeServiceService', () => {
  let service: RecipeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RecipeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
