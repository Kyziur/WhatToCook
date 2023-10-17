import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MealPlanningComponent } from './meal-planning.component';

describe('MealPlanningComponent', () => {
  let component: MealPlanningComponent;
  let fixture: ComponentFixture<MealPlanningComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MealPlanningComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MealPlanningComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
