import { Component, OnInit } from '@angular/core';
import { RecipeCardViewModel } from '../recipe-card/recipe-card.component';

@Component({
  selector: 'app-recipe-list',
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.scss']
})
export class RecipeListComponent implements OnInit {
  recipeCards: RecipeCardViewModel[] = [{ 
    name: 'Flaczki',
    description: "Opis flaczków",
    img: ""
  },
  {
    name: 'Flaczki 2',
    description: "Opis flaczków 2",
    img: ""
  }];
  
  constructor(){
  }

  ngOnInit() {
      //Tutaj pobieramy dane z API - możemy tu wykonywać operacje z async
  }
}
