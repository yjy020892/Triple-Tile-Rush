// 매 enum 값은 고유 정수 ID. 절대 변경/재사용 금지(저장된 PlayerPrefs 호환 위해).
// 새로운 종을 추가할 때는 비어있는 ID에 새로 끼워 넣지 말고, 항상 마지막 번호 다음으로 추가할 것.
public enum SortingItemType
{
    None = 0,

    // ── Legacy (1..8) — 게임 출시 초기 8종. 의미는 SortingItemCatalog 가 정의 ──
    Shirt = 1,    // Strawberry (Food)
    Shoes = 2,    // Orange     (Food)
    Hat = 3,      // Banana     (Food)
    Bag = 4,      // Grapes     (Food)
    Watch = 5,    // Cherry     (Food)
    Glasses = 6,  // Clover     (Plant)
    Book = 7,     // Blossom    (Plant)
    Toy = 8,      // Lolli      (Sweet)

    // ── Animal (10..) ──
    Cat = 10,
    Dog = 11,
    Rabbit = 12,
    Fox = 13,
    Bear = 14,
    Panda = 15,

    // ── Bug (20..) ──
    Bee = 20,
    Ladybug = 21,
    Butterfly = 22,
    Snail = 23,
    Beetle = 24,

    // ── Fantasy (30..) ──
    Unicorn = 30,
    Dragon = 31,
    Wizard = 32,
    Crystal = 33,
    StarMage = 34,

    // ── Food (40..) — Legacy 1..5 외 추가분 ──
    Apple = 40,
    Watermelon = 41,
    Peach = 42,
    Pineapple = 43,
    Lemon = 44,

    // ── Sweet (50..) — Legacy 8(Toy=Lolli) 외 추가분 ──
    Donut = 50,
    Cupcake = 51,
    IceCream = 52,
    Candy = 53,
    Cookie = 54,

    // ── Plant (60..) — Legacy 6,7 외 추가분 ──
    Mushroom = 60,
    Cactus = 61,
    Flower = 62,
    Leaf = 63,

    // ── Vehicle (70..) ──
    Car = 70,
    Plane = 71,
    Boat = 72,
    Rocket = 73,
    Bike = 74,

    // ── Weather (80..) ──
    Sun = 80,
    Cloud = 81,
    Rainbow = 82,
    Snowflake = 83,
    Lightning = 84,

    // ── Tool (90..) ──
    Wrench = 90,
    Hammer = 91,
    Scissors = 92,
    Paint = 93,
    Magnet = 94,
}

public enum SortingTheme
{
    Food = 0,
    Sweet = 1,
    Plant = 2,
    Animal = 3,
    Bug = 4,
    Fantasy = 5,
    Vehicle = 6,
    Weather = 7,
    Tool = 8,
}
