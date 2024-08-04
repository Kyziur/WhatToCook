import { CommonModule } from '@angular/common';
import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
  inject,
  computed,
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
import { RecipeCard } from '../recipe.types';

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
  recipe = input.required<RecipeCard>();
  selectable = input<boolean>(false);
  selectClicked = output<number>();

  private readonly router = inject(Router);
  private readonly appConfig = inject(AppConfigService);

  viewRecipeDetails(name: string | undefined) {
    if (name === undefined) {
      return;
    }

    this.router.navigate([`/recipes/${name}`]);
  }

  imagePath = computed(() => {
    return (
      this.recipe()?.imagePath ?? this.appConfig.getConfig().defaultImagePath
    );
  });
}
