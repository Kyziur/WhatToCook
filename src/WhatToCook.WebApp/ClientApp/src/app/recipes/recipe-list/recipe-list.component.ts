import { Component, OnInit } from '@angular/core';
import { RecipeService } from '../recipe.service';
import { Recipe } from '../Recipe';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss']
})
export class RecipeListComponent implements OnInit {
  recipes: Recipe[]= [];

constructor(private recipeService: RecipeService){}
  ngOnInit(): void{
      //Tutaj pobieramy dane z API - możemy tu wykonywać operacje z async
    this.recipeService.get().subscribe((recipe)=>{
      console.error(recipe)
      this.recipes = recipe;},
    (error) => {console.error(error);}
  )}

}
/*recipeCards: RecipeCardViewModel[] = [{ 
  name: 'Flaczki',
  description: "Opis flaczków",
  img: "/assets/images/Tomato.jpg"
},


];
*/