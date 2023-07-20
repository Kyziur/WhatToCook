import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MealPlanCreatorComponent } from './meal-plan-creator.component';

describe('MealPlanningComponent', () => {
  let component: MealPlanCreatorComponent;
  let fixture: ComponentFixture<MealPlanCreatorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MealPlanCreatorComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MealPlanCreatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
