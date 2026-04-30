using System.Collections.Generic;
using UnityEngine;

public sealed class SortingItemDef
{
    public SortingItemType type;
    public SortingTheme theme;
    public string displayName;
    public string glyph;
    public Color mainColor;
    public Color accentColor;
    public Color frameColor;
}

public static class SortingItemCatalog
{
    private static readonly Dictionary<SortingItemType, SortingItemDef> table = BuildTable();
    private static readonly Dictionary<SortingTheme, List<SortingItemType>> byTheme = BuildThemeIndex();

    public static SortingItemDef Get(SortingItemType type)
    {
        return table.TryGetValue(type, out SortingItemDef def) ? def : null;
    }

    public static IReadOnlyList<SortingItemType> GetTypesInTheme(SortingTheme theme)
    {
        return byTheme.TryGetValue(theme, out List<SortingItemType> list) ? list : new List<SortingItemType>();
    }

    public static SortingTheme GetTheme(SortingItemType type)
    {
        SortingItemDef def = Get(type);
        return def != null ? def.theme : SortingTheme.Food;
    }

    public static int ThemeCount => byTheme.Count;

    private static Dictionary<SortingItemType, SortingItemDef> BuildTable()
    {
        Dictionary<SortingItemType, SortingItemDef> t = new Dictionary<SortingItemType, SortingItemDef>();

        Add(t, SortingItemType.Shirt,      SortingTheme.Food,    "Strawberry", "S",  C(0.95f,0.27f,0.32f), C(0.34f,0.62f,0.32f), C(1.00f,0.86f,0.86f));
        Add(t, SortingItemType.Shoes,      SortingTheme.Food,    "Orange",     "O",  C(1.00f,0.59f,0.18f), C(0.36f,0.65f,0.32f), C(1.00f,0.90f,0.78f));
        Add(t, SortingItemType.Hat,        SortingTheme.Food,    "Banana",     "B",  C(1.00f,0.83f,0.20f), C(0.62f,0.42f,0.18f), C(1.00f,0.96f,0.78f));
        Add(t, SortingItemType.Bag,        SortingTheme.Food,    "Grapes",     "G",  C(0.55f,0.32f,0.78f), C(0.36f,0.62f,0.34f), C(0.92f,0.86f,1.00f));
        Add(t, SortingItemType.Watch,      SortingTheme.Food,    "Cherry",     "C",  C(0.86f,0.18f,0.24f), C(0.36f,0.62f,0.34f), C(1.00f,0.84f,0.84f));
        Add(t, SortingItemType.Apple,      SortingTheme.Food,    "Apple",      "A",  C(0.94f,0.34f,0.32f), C(0.36f,0.62f,0.34f), C(1.00f,0.84f,0.82f));
        Add(t, SortingItemType.Watermelon, SortingTheme.Food,    "Watermelon", "W",  C(0.97f,0.40f,0.50f), C(0.30f,0.65f,0.35f), C(1.00f,0.86f,0.88f));
        Add(t, SortingItemType.Peach,      SortingTheme.Food,    "Peach",      "P",  C(1.00f,0.71f,0.62f), C(0.36f,0.62f,0.34f), C(1.00f,0.92f,0.88f));
        Add(t, SortingItemType.Pineapple,  SortingTheme.Food,    "Pineapple",  "P",  C(0.99f,0.83f,0.27f), C(0.30f,0.55f,0.30f), C(1.00f,0.95f,0.78f));
        Add(t, SortingItemType.Lemon,      SortingTheme.Food,    "Lemon",      "L",  C(1.00f,0.92f,0.24f), C(0.62f,0.74f,0.20f), C(1.00f,0.97f,0.80f));

        Add(t, SortingItemType.Toy,        SortingTheme.Sweet,   "Lolli",      "L",  C(1.00f,0.40f,0.70f), C(0.94f,0.94f,0.94f), C(1.00f,0.84f,0.92f));
        Add(t, SortingItemType.Donut,      SortingTheme.Sweet,   "Donut",      "D",  C(0.96f,0.66f,0.45f), C(0.95f,0.30f,0.55f), C(1.00f,0.92f,0.84f));
        Add(t, SortingItemType.Cupcake,    SortingTheme.Sweet,   "Cupcake",    "C",  C(0.98f,0.72f,0.84f), C(0.65f,0.40f,0.78f), C(1.00f,0.92f,0.96f));
        Add(t, SortingItemType.IceCream,   SortingTheme.Sweet,   "Ice",        "I",  C(0.78f,0.92f,0.98f), C(0.85f,0.65f,0.45f), C(0.92f,0.97f,1.00f));
        Add(t, SortingItemType.Candy,      SortingTheme.Sweet,   "Candy",      "C",  C(0.96f,0.40f,0.50f), C(1.00f,0.95f,0.86f), C(1.00f,0.86f,0.92f));
        Add(t, SortingItemType.Cookie,     SortingTheme.Sweet,   "Cookie",     "K",  C(0.78f,0.55f,0.32f), C(0.30f,0.20f,0.12f), C(1.00f,0.92f,0.82f));

        Add(t, SortingItemType.Glasses,    SortingTheme.Plant,   "Clover",     "+",  C(0.36f,0.78f,0.34f), C(0.20f,0.55f,0.20f), C(0.86f,0.98f,0.86f));
        Add(t, SortingItemType.Book,       SortingTheme.Plant,   "Blossom",    "*",  C(1.00f,0.50f,0.72f), C(0.98f,0.86f,0.34f), C(1.00f,0.88f,0.95f));
        Add(t, SortingItemType.Mushroom,   SortingTheme.Plant,   "Mushroom",   "M",  C(0.92f,0.34f,0.34f), C(1.00f,1.00f,1.00f), C(1.00f,0.88f,0.88f));
        Add(t, SortingItemType.Cactus,     SortingTheme.Plant,   "Cactus",     "C",  C(0.40f,0.70f,0.40f), C(0.95f,0.95f,0.85f), C(0.86f,0.95f,0.84f));
        Add(t, SortingItemType.Flower,     SortingTheme.Plant,   "Flower",     "*",  C(0.96f,0.60f,0.78f), C(1.00f,0.86f,0.34f), C(1.00f,0.92f,0.96f));
        Add(t, SortingItemType.Leaf,       SortingTheme.Plant,   "Leaf",       "L",  C(0.42f,0.74f,0.36f), C(0.20f,0.55f,0.20f), C(0.88f,0.96f,0.84f));

        Add(t, SortingItemType.Cat,        SortingTheme.Animal,  "Cat",        "C",  C(1.00f,0.78f,0.45f), C(0.55f,0.30f,0.10f), C(1.00f,0.94f,0.84f));
        Add(t, SortingItemType.Dog,        SortingTheme.Animal,  "Dog",        "D",  C(0.90f,0.65f,0.40f), C(0.40f,0.22f,0.10f), C(1.00f,0.92f,0.82f));
        Add(t, SortingItemType.Rabbit,     SortingTheme.Animal,  "Rabbit",     "R",  C(0.98f,0.92f,0.94f), C(0.95f,0.55f,0.62f), C(1.00f,0.96f,0.96f));
        Add(t, SortingItemType.Fox,        SortingTheme.Animal,  "Fox",        "F",  C(0.97f,0.50f,0.20f), C(1.00f,1.00f,1.00f), C(1.00f,0.88f,0.78f));
        Add(t, SortingItemType.Bear,       SortingTheme.Animal,  "Bear",       "B",  C(0.55f,0.35f,0.22f), C(0.95f,0.85f,0.70f), C(0.95f,0.86f,0.74f));
        Add(t, SortingItemType.Panda,      SortingTheme.Animal,  "Panda",      "P",  C(0.95f,0.95f,0.95f), C(0.10f,0.10f,0.10f), C(0.96f,0.96f,0.96f));

        Add(t, SortingItemType.Bee,        SortingTheme.Bug,     "Bee",        "B",  C(1.00f,0.84f,0.20f), C(0.20f,0.15f,0.10f), C(1.00f,0.95f,0.70f));
        Add(t, SortingItemType.Ladybug,    SortingTheme.Bug,     "Ladybug",    "L",  C(0.92f,0.20f,0.25f), C(0.10f,0.10f,0.10f), C(1.00f,0.84f,0.84f));
        Add(t, SortingItemType.Butterfly,  SortingTheme.Bug,     "Butterfly",  "B",  C(0.65f,0.40f,0.92f), C(1.00f,0.78f,0.95f), C(0.95f,0.86f,1.00f));
        Add(t, SortingItemType.Snail,      SortingTheme.Bug,     "Snail",      "S",  C(0.86f,0.65f,0.45f), C(0.55f,0.40f,0.25f), C(0.96f,0.88f,0.78f));
        Add(t, SortingItemType.Beetle,     SortingTheme.Bug,     "Beetle",     "B",  C(0.30f,0.65f,0.40f), C(0.10f,0.20f,0.12f), C(0.84f,0.96f,0.86f));

        Add(t, SortingItemType.Unicorn,    SortingTheme.Fantasy, "Unicorn",    "U",  C(0.98f,0.80f,0.95f), C(0.78f,0.45f,0.92f), C(1.00f,0.92f,1.00f));
        Add(t, SortingItemType.Dragon,     SortingTheme.Fantasy, "Dragon",     "D",  C(0.45f,0.70f,0.40f), C(0.95f,0.55f,0.20f), C(0.86f,0.96f,0.84f));
        Add(t, SortingItemType.Wizard,     SortingTheme.Fantasy, "Wizard",     "W",  C(0.40f,0.45f,0.85f), C(1.00f,0.86f,0.34f), C(0.86f,0.90f,1.00f));
        Add(t, SortingItemType.Crystal,    SortingTheme.Fantasy, "Crystal",    "X",  C(0.55f,0.85f,0.95f), C(1.00f,1.00f,1.00f), C(0.86f,0.96f,1.00f));
        Add(t, SortingItemType.StarMage,   SortingTheme.Fantasy, "Star",       "*",  C(1.00f,0.84f,0.30f), C(1.00f,0.95f,0.78f), C(1.00f,0.96f,0.78f));

        Add(t, SortingItemType.Car,        SortingTheme.Vehicle, "Car",        "C",  C(0.92f,0.30f,0.34f), C(0.18f,0.18f,0.18f), C(1.00f,0.86f,0.86f));
        Add(t, SortingItemType.Plane,      SortingTheme.Vehicle, "Plane",      "P",  C(0.85f,0.92f,0.98f), C(0.30f,0.55f,0.85f), C(0.92f,0.96f,1.00f));
        Add(t, SortingItemType.Boat,       SortingTheme.Vehicle, "Boat",       "B",  C(0.96f,0.95f,0.86f), C(0.30f,0.55f,0.85f), C(0.95f,0.96f,0.88f));
        Add(t, SortingItemType.Rocket,     SortingTheme.Vehicle, "Rocket",     "R",  C(0.95f,0.95f,0.95f), C(0.95f,0.30f,0.30f), C(0.96f,0.96f,0.96f));
        Add(t, SortingItemType.Bike,       SortingTheme.Vehicle, "Bike",       "B",  C(0.30f,0.65f,0.85f), C(0.18f,0.18f,0.18f), C(0.86f,0.94f,1.00f));

        Add(t, SortingItemType.Sun,        SortingTheme.Weather, "Sun",        "S",  C(1.00f,0.84f,0.20f), C(1.00f,0.62f,0.25f), C(1.00f,0.95f,0.74f));
        Add(t, SortingItemType.Cloud,      SortingTheme.Weather, "Cloud",      "C",  C(0.95f,0.97f,1.00f), C(0.62f,0.74f,0.92f), C(0.95f,0.97f,1.00f));
        Add(t, SortingItemType.Rainbow,    SortingTheme.Weather, "Rainbow",    "R",  C(0.95f,0.55f,0.78f), C(0.45f,0.85f,0.45f), C(1.00f,0.92f,0.96f));
        Add(t, SortingItemType.Snowflake,  SortingTheme.Weather, "Snow",       "*",  C(0.86f,0.96f,1.00f), C(0.40f,0.62f,0.92f), C(0.92f,0.97f,1.00f));
        Add(t, SortingItemType.Lightning,  SortingTheme.Weather, "Bolt",       "/",  C(1.00f,0.86f,0.20f), C(0.95f,0.55f,0.20f), C(1.00f,0.95f,0.78f));

        Add(t, SortingItemType.Wrench,     SortingTheme.Tool,    "Wrench",     "W",  C(0.55f,0.65f,0.78f), C(0.30f,0.35f,0.45f), C(0.88f,0.92f,0.97f));
        Add(t, SortingItemType.Hammer,     SortingTheme.Tool,    "Hammer",     "H",  C(0.65f,0.45f,0.30f), C(0.30f,0.30f,0.30f), C(0.95f,0.86f,0.78f));
        Add(t, SortingItemType.Scissors,   SortingTheme.Tool,    "Scissor",    "X",  C(0.92f,0.40f,0.45f), C(0.55f,0.65f,0.78f), C(1.00f,0.86f,0.88f));
        Add(t, SortingItemType.Paint,      SortingTheme.Tool,    "Paint",      "P",  C(0.40f,0.65f,0.92f), C(1.00f,1.00f,1.00f), C(0.86f,0.94f,1.00f));
        Add(t, SortingItemType.Magnet,     SortingTheme.Tool,    "Magnet",     "M",  C(0.88f,0.30f,0.32f), C(0.95f,0.95f,0.95f), C(1.00f,0.86f,0.86f));

        return t;
    }

    private static Dictionary<SortingTheme, List<SortingItemType>> BuildThemeIndex()
    {
        Dictionary<SortingTheme, List<SortingItemType>> map = new Dictionary<SortingTheme, List<SortingItemType>>();
        foreach (KeyValuePair<SortingItemType, SortingItemDef> pair in table)
        {
            if (!map.TryGetValue(pair.Value.theme, out List<SortingItemType> list))
            {
                list = new List<SortingItemType>();
                map[pair.Value.theme] = list;
            }
            list.Add(pair.Key);
        }
        return map;
    }

    private static void Add(Dictionary<SortingItemType, SortingItemDef> t, SortingItemType type, SortingTheme theme, string name, string glyph, Color main, Color accent, Color frame)
    {
        t[type] = new SortingItemDef
        {
            type = type,
            theme = theme,
            displayName = name,
            glyph = glyph,
            mainColor = main,
            accentColor = accent,
            frameColor = frame,
        };
    }

    private static Color C(float r, float g, float b)
    {
        return new Color(r, g, b, 1f);
    }
}
