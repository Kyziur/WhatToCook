import { TestBed } from '@angular/core/testing';

import { MealPlanningService } from './meal-planning.service';

describe('MealPlanningService', () => {
  let service: MealPlanningService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MealPlanningService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
