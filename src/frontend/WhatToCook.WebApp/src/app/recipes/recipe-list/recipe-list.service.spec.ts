import { TestBed } from '@angular/core/testing';
import { RecipeListService } from './recipe-list.service';
import { map } from 'rxjs';

describe('RecipeListService', () => {
  let service: RecipeListService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RecipeListService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('#toggleSelect()', () => {

    it('should toggle select state of a recipe card', () => {
      const id = 1;
      service.toggleSelect(id);

      // Assuming that initially all cards are not selected, and we have at least one card,
      // check the select state for the given id after toggle.
      const firstCardSelected = service.recipeCards$.pipe(
        map(recipes => recipes[0].isSelected)
      ).subscribe(isSelected => {
        expect(isSelected).toBe(true);
        service.toggleSelect(id);
        isSelected = service.recipeCards$.pipe(
          map(recipes => recipes[0].isSelected)
        ).subscribe(updatedIsSelected => {
          expect(updatedIsSelected).toBe(false);
        });
      });
    });

    it('should handle case where card with given id does not exist', () => {
      const nonExistingId = 99;
      service.toggleSelect(nonExistingId);

      // Verify that the state remains unchanged for a non-existing card ID.
      expect(service.recipeCards$.pipe(map(recipes => recipes[0].isSelected)).subscribe(isSelected => {
        expect(isSelected).toBe(false);
      })).toBeTruthy();
    });

  });

  describe('#getSelected$', () => {

    it('should return selected recipe cards', () => {
      const ids = [1, 3];
      service.select(ids);

      // Verify that the method correctly filters out only the selected cards.
      expect(service.getSelected$().pipe(
        map(recipes => recipes.map(recipe => recipe.id))
      ).subscribe(selectedIds => {
        expect(selectedIds).toEqual(ids);
      })).toBeTruthy();
    });

  });
});
