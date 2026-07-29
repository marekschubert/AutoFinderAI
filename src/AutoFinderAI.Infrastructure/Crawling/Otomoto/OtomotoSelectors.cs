namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>
/// Selector map for otomoto.pl, derived from the fixtures in
/// tests/AutoFinderAI.UnitTests/Fixtures/otomoto/ (car_offers_critical_elements.html and
/// single_car_offer_critical_elements.html). Do not change these without a fresh sample page.
///
/// LIST PAGE (search results) — used only to discover listing detail-page URLs:
///   div[data-testid="search-results"] &gt; article[data-id]      → ExternalId
///     article ... h2 &gt; a[href]                                → Url + Title (anchor text)
///
/// DETAIL PAGE — publication date:
///   div[data-testid="content-description-section"] p           → "29 lipca 2026 12:07"
///
/// DETAIL PAGE — every field below is read from an element `[data-testid="&lt;key&gt;"]` whose
/// structure is always `&lt;div data-testid="key"&gt;&lt;div&gt;&lt;div&gt;&lt;p&gt;Label&lt;/p&gt;&lt;/div&gt;&lt;p&gt;Value&lt;/p&gt;&lt;/div&gt;&lt;/div&gt;`
/// (the *last* &lt;p&gt; in the subtree is always the value). Some of these also have a duplicate,
/// simpler "quick facts" card `div[data-testid="detail"]` with two sibling &lt;p&gt; (label, value) —
/// used only as a defensive fallback when the specific testid is missing from the page.
///
/// | Domain field (Car)     | Detail-page data-testid | Fallback via [data-testid="detail"] label |
/// |-------------------------|--------------------------|--------------------------------------------|
/// | Make                    | make                     | —                                          |
/// | Model                   | model                    | —                                          |
/// | ProductionYear           | year                     | —                                          |
/// | Mileage                  | mileage                  | "Przebieg"                                 |
/// | FuelType                 | fuel_type                | "Rodzaj paliwa"                            |
/// | Transmission (gearbox)   | gearbox                  | "Skrzynia biegów"                          |
/// | EnginePowerHp             | engine_power              | "Moc"                                      |
/// | EngineCapacityCm3        | engine_capacity           | "Pojemność skokowa"                        |
/// | BodyType                 | body_type                 | "Typ nadwozia"                             |
/// | DriveType ("Napęd")       | transmission              | —                                          |
/// | Doors                    | door_count                | —                                          |
/// | Seats                    | nr_seats                  | —                                          |
/// | Color                     | color                     | —                                          |
/// | CountryOfOrigin           | country_origin             | —                                          |
/// | IsFirstOwner ("Pierwszy właściciel") | original_owner   | —                                          |
/// | IsDamaged (inverse of "Bezwypadkowy") | no_accident      | —                                          |
/// | Price.Amount              | .offer-price__number (class) | —                                    |
/// | Price.Currency             | .offer-price__currency (class) | —                                  |
///
/// ASSUMPTION: Location and ThumbnailUrl selectors were not present in the supplied fixtures —
/// they are left null pending a confirmed selector map, per the "never guess a selector" rule.
/// </summary>
public static class OtomotoSelectors
{
    public const string SearchResultsContainer = "div[data-testid='search-results']";
    public const string ArticleTag = "article";
    public const string TitleLinkSelector = "h2 a[href]";
    public const string FallbackLinkSelector = "a[data-nextlink]";
    public const string ThumbnailImageSelector = "img";
    public const string PublishedTextSelector = "ul li p";

    public const string PublishedAtSection = "div[data-testid='content-description-section']";
    public const string DetailCardSelector = "div[data-testid='detail']";

    public const string PriceAmountSelector = ".offer-price__number";
    public const string PriceCurrencySelector = ".offer-price__currency";

    public const string Make = "make";
    public const string Model = "model";
    public const string Year = "year";
    public const string Color = "color";
    public const string DoorCount = "door_count";
    public const string SeatCount = "nr_seats";
    public const string Mileage = "mileage";
    public const string FuelType = "fuel_type";
    public const string Gearbox = "gearbox";
    public const string EnginePower = "engine_power";
    public const string EngineCapacity = "engine_capacity";
    public const string BodyType = "body_type";
    public const string Drive = "transmission"; // "Napęd" — not to be confused with Gearbox above.
    public const string CountryOfOrigin = "country_origin";
    public const string NoAccident = "no_accident";
    public const string OriginalOwner = "original_owner";

    public static string DataTestId(string key) => $"[data-testid='{key}']";
}
