using System.Net;

namespace WhatToCook.Web.Components.Features.Recipes.Import.Services;

/// <summary>
/// Converts bounded API failure categories into actionable, user-safe Polish guidance.
/// Never reflects arbitrary remote page text, private URLs or exception messages.
/// </summary>
public static class RecipeImportFailureGuidance
{
    public static string ForCode(string? code, HttpStatusCode status) =>
        code switch
        {
            "invalid_url" => "Sprawdź adres URL przepisu i spróbuj ponownie.",
            "unsupported_source" =>
                "Ta witryna nie jest obsługiwana. Wybierz przepis z obsługiwanego źródła.",
            "timeout" => "Strona przepisu nie odpowiedziała na czas. Spróbuj ponownie za chwilę.",
            "dns_resolution" or "http_transport" =>
                "Nie udało się połączyć ze stroną przepisu. Sprawdź połączenie i spróbuj ponownie.",
            "http_status" =>
                "Witryna odrzuciła pobranie przepisu. Otwórz stronę w przeglądarce lub użyj innego źródła.",
            "blocked_destination" or "request_rejected" =>
                "Nie można bezpiecznie pobrać tego adresu. Użyj publicznego adresu obsługiwanej strony.",
            "invalid_response" =>
                "Strona zwróciła nieobsługiwaną odpowiedź. Wybierz inną stronę z przepisem.",
            "extraction_failed" =>
                "Nie udało się odczytać przepisu z tej strony. Sprawdź, czy zawiera pełną recepturę.",
            "invalid_draft" =>
                "Pobrany przepis wymaga poprawek. Spróbuj innej strony z pełnym opisem przepisu.",
            "unexpected" => "Import chwilowo nie działa. Spróbuj ponownie później.",
            _ when status == HttpStatusCode.BadRequest =>
                "Sprawdź adres i dane przepisu, a następnie spróbuj ponownie.",
            _ => "Nie udało się zaimportować przepisu. Spróbuj ponownie lub wybierz inne źródło.",
        };
}
