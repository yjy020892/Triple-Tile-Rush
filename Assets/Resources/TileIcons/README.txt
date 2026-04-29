Tile icon sprites loaded at runtime by SortingItemView.

로딩 규칙
---------
런타임에 SortingItemView 가  Resources.Load<Sprite>("TileIcons/<TypeName>")
로 시도합니다. PNG가 없으면 SortingProceduralIconProvider 가 둥글둥글한
폴백 아이콘을 절차적으로 생성합니다(테마별 액센트 + 글리프 포함).

→ 출시 전에 일러스트 파일을 같은 이름으로 떨어트리면 자동 적용됩니다.
   (지금은 일부만 PNG, 나머지는 절차 폴백으로 동작 중)

타입 ↔ 표시 이름 ↔ 테마
-------------------------
SortingItemCatalog.cs 가 단일 진실원천. 새 종을 추가하면 이 카탈로그에도 반드시 추가.

Legacy (1..8)
- Shirt.png   -> Strawberry  (Food)
- Shoes.png   -> Orange      (Food)
- Hat.png     -> Banana      (Food)
- Bag.png     -> Grapes      (Food)
- Watch.png   -> Cherry      (Food)
- Glasses.png -> Clover      (Plant)
- Book.png    -> Blossom     (Plant)
- Toy.png     -> Lolli       (Sweet)

Animal      : Cat, Dog, Rabbit, Fox, Bear, Panda
Bug         : Bee, Ladybug, Butterfly, Snail, Beetle
Fantasy     : Unicorn, Dragon, Wizard, Crystal, StarMage
Food (extra): Apple, Watermelon, Peach, Pineapple, Lemon
Sweet       : Donut, Cupcake, IceCream, Candy, Cookie
Plant       : Mushroom, Cactus, Flower, Leaf
Vehicle     : Car, Plane, Boat, Rocket, Bike
Weather     : Sun, Cloud, Rainbow, Snowflake, Lightning
Tool        : Wrench, Hammer, Scissors, Paint, Magnet

Import settings (recommended for Unity):
- Texture Type:        Sprite (2D and UI)
- Mesh Type:           Full Rect
- Pixels Per Unit:     100
- Filter Mode:         Bilinear
- Max Size:            512
- Compression:         Normal Quality
- Alpha Is Transparency: On
- sRGB (Color Texture): On
