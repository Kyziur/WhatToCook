import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FavouritesButtonComponent } from './favourites-button.component';

describe('FavouritesButtonComponent', () => {
  let component: FavouritesButtonComponent;
  let fixture: ComponentFixture<FavouritesButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [FavouritesButtonComponent],
}).compileComponents();

    fixture = TestBed.createComponent(FavouritesButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
