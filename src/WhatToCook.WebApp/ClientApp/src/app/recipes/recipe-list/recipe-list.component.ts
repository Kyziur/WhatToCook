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
    img: "/assets/images/Tomato.jpg"
  },
  {
    name: 'Flaczki 2',
    description: "Opis flaczków 2",
    img: "/assets/images/Tomato.jpg"
  },
{
  name: 'Flaczki 3',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
{
  name: 'Flaczki 4',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
{
  name: 'Flaczki 5',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
{
  name: 'Flaczki 6',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
{
  name: 'Flaczki 7',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
{
  name: 'Flaczki 8',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
{
  name: 'Flaczki 9',
  description: "Opis flaczków 2",
  img: "/assets/images/Tomato.jpg"
},
];
   
  constructor(){
  }

  ngOnInit() {
      //Tutaj pobieramy dane z API - możemy tu wykonywać operacje z async
  }
}
