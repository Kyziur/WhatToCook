using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure;

public static class RecipesSeeder
{
    public static IEnumerable<Recipe> Seed()
    {
        const string description =
            @"Wszystkie składniki powinny być w temperaturze pokojowej.
                W naczyniu wymieszać mąkę pszenną, proszek do pieczenia, sól i płatki owsiane.
                W misie miksera utrzeć masło i oba cukry do otrzymania jasnej i puszystej masy maślanej. 
                Dodać jajko i wanilię, utrzeć dokładnie do połączenia się składników. Połączyć zawartość dwóch naczyń i zmiksować. Na koniec wmieszać żurawinę, czekoladę i orzechy. Powstałe ciasto schłodzić w lodówce przez 15 minut.
                Z ciasta formować kulki wielkości orzecha włoskiego, układać na blaszce wyłożonej papierem do pieczenia, zachowując duże odstępy; najłatwiej robić to zanurzając dłonie w mące – ciasto mocno się klei (ale tak ma być), można też użyć łyżki do lodów. Każdą kulkę lekko spłaszczyć łyżką.
                Ciastka z białą czekoladą, żurawiną i płatkami owsianymi piec w temperaturze 170ºC przez około 10 – 12 minut lub do zarumienienia brzegów ciastek. Po upieczeniu ciastka będą bardzo miękkie. Należy odczekać, aż stwardnieją i przełożyć na kratkę, by wystygły do końca.";

        return Enumerable.Range(0, 100)
            .Select(x => new Recipe
            (
                $"Ciastka z białą czekoladą, żurawiną i płatkami owsianymi {x}",
                description,
                "Short",
                [
                    new Ingredient("140 g mąki pszennej"),
                    new Ingredient("pół łyżeczki proszku do pieczenia"),
                    new Ingredient("szczypta soli"),
                    new Ingredient("75 g płatków owsianych"),
                    new Ingredient("125 g masła"),
                    new Ingredient("75 g ciemnego brązowego cukru"),
                    new Ingredient("50 g drobnego cukru do wypieków"),
                    new Ingredient("1 duże jajko"),
                    new Ingredient("pół łyżeczki ekstraktu z wanilii"),
                    new Ingredient("75 g suszonej żurawiny"),
                    new Ingredient("50 g orzechów pekan, z grubsza posiekanych"),
                    new Ingredient("140 g posiekanej białej czekolady lub białych chocolate chips")
                ],
                new Statistics(),
                "Images/default_image.png"));
    }
}