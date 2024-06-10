import { CommonModule } from '@angular/common';
import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
  inject,
  Input,
} from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import {
  bootstrapBookmarkStar,
  bootstrapBookmarkStarFill,
  bootstrapPlusSquare,
  bootstrapPlusSquareFill,
} from '@ng-icons/bootstrap-icons';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { AppConfigService } from '../../app.config.service';
import { BadgeComponent } from '../../shared/components/badge/badge.component';
import { PrepareTimeToBadgePipe } from '../prepare-time-to-badge.pipe';
import { RecipeResponse } from '../../api-contracts/model/recipeResponse';

export type RecipeCard = RecipeResponse & { isSelected: boolean };
@Component({
  selector: 'app-recipe-card',
  templateUrl: './recipe-card.component.html',
  standalone: true,
  imports: [
    CommonModule,
    BadgeComponent,
    NgIconComponent,
    RouterModule,
    PrepareTimeToBadgePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    provideIcons({
      bootstrapBookmarkStar,
      bootstrapBookmarkStarFill,
      bootstrapPlusSquare,
      bootstrapPlusSquareFill,
    }),
  ],
})
export class RecipeCardComponent {
  @Input({ required: true }) recipe!: RecipeCard;
  recipeChange = output<RecipeCard>();
  selectable = input<boolean>(false);

  private readonly router = inject(Router);
  readonly defaultImagePath =
    inject(AppConfigService).getConfig().defaultImagePath;

  viewRecipeDetails(name: string) {
    this.router.navigate([`/recipes/${name}`]);
  }

  toggle() {
    this.recipe.isSelected = !this.recipe.isSelected;
    this.recipeChange.emit(this.recipe);
  }
}
