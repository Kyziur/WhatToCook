import { TestBed } from '@angular/core/testing';

import { MealPlanCreatorService } from './meal-plan-creator.service';

describe('MealPlanCreatorService', () => {
  let service: MealPlanCreatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MealPlanCreatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
