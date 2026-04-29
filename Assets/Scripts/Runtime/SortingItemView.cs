using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SortingItemView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text iconText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconFrameImage;
    [SerializeField] private Image faceImage;
    [SerializeField] private Image baseImage;

    private SortingItemType itemType;
    private SortingGameController gameController;
    private bool isBusy;
    private bool isRemoved;
    private bool isExposed;
    private int stackId;
    private int stackDepth;
    private Color baseIconColor = Color.white;
    private Color baseFaceColor = new Color(1f, 0.965f, 0.88f, 1f);
    private Color baseBaseColor = new Color(0.86f, 0.63f, 0.34f, 1f);
    private List<(Image image, Color baseColor)> faceOverlayColors;

    public SortingItemType ItemType => itemType;
    public bool IsBusy => isBusy;
    public bool IsRemoved => isRemoved;
    public bool IsExposed => isExposed;
    public int StackId => stackId;
    public int StackDepth => stackDepth;
    public RectTransform RectTransform => (RectTransform)transform;

    public void BindUi(Image newBackgroundImage, Image newIconFrameImage, Image newIconImage, TMP_Text newIconText, TMP_Text newTypeText)
    {
        backgroundImage = newBackgroundImage;
        iconFrameImage = newIconFrameImage;
        iconImage = newIconImage;
        iconText = newIconText;
        typeText = newTypeText;
    }

    public void SetBaseTileSurfaces(Image face, Image baseSurface)
    {
        faceImage = face;
        baseImage = baseSurface;
        if (face != null) baseFaceColor = face.color;
        if (baseSurface != null) baseBaseColor = baseSurface.color;
    }

    public void Initialize(SortingItemType newItemType, SortingGameController newGameController)
    {
        itemType = newItemType;
        gameController = newGameController;
        isBusy = false;
        isRemoved = false;
        isExposed = true;
        stackId = -1;
        stackDepth = 0;

        bool hasSprite = false;
        if (iconImage != null)
        {
            Sprite iconSprite = LoadIconSprite(itemType);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.type = Image.Type.Simple;
                iconImage.preserveAspect = true;
                baseIconColor = Color.white;
                hasSprite = true;
            }
            else
            {
                iconImage.sprite = null;
                baseIconColor = GetColor(itemType);
            }

            iconImage.color = baseIconColor;
        }

        if (iconText != null)
        {
            iconText.text = hasSprite ? string.Empty : GetGlyph(itemType);
            iconText.color = new Color(0.29f, 0.18f, 0.08f, 0.96f);
        }

        if (typeText != null)
        {
            typeText.text = string.Empty;
        }

        if (iconFrameImage != null)
        {
            iconFrameImage.color = GetIconFrameColor(itemType);
        }

        if (faceImage != null)
        {
            Sprite tileSprite = LoadTileSprite();
            if (tileSprite != null)
            {
                faceImage.sprite = tileSprite;
                faceImage.type = Image.Type.Simple;
                faceImage.preserveAspect = false;
                baseFaceColor = Color.white;
            }
        }

        SetExposedState(true);
    }

    private void CacheFaceOverlayBases()
    {
        if (faceImage == null || faceOverlayColors != null) return;
        faceOverlayColors = new List<(Image, Color)>(4);
        Transform ft = faceImage.transform;
        for (int c = 0; c < ft.childCount; c++)
        {
            Transform t = ft.GetChild(c);
            if (t.name == "IconHolder" || t.name == "IconText")
            {
                continue;
            }

            Image im = t.GetComponent<Image>();
            if (im != null)
            {
                faceOverlayColors.Add((im, im.color));
            }
        }
    }

    public void SetStackData(int newStackId, int newStackDepth)
    {
        stackId = newStackId;
        stackDepth = newStackDepth;
    }

    public void SetExposedState(bool exposed)
    {
        isExposed = exposed;

        float alpha = exposed ? 1f : 0.82f;
        float faceDim = exposed ? 1f : 0.62f;
        float iconDim = exposed ? 1f : 0.55f;

        if (faceImage != null)
        {
            CacheFaceOverlayBases();
            faceImage.color = new Color(
                baseFaceColor.r * faceDim,
                baseFaceColor.g * faceDim,
                baseFaceColor.b * faceDim,
                baseFaceColor.a);
            if (faceOverlayColors != null)
            {
                for (int i = 0; i < faceOverlayColors.Count; i++)
                {
                    (Image im, Color bc) = faceOverlayColors[i];
                    if (im == null) continue;
                    im.color = new Color(
                        bc.r * faceDim,
                        bc.g * faceDim,
                        bc.b * faceDim,
                        bc.a);
                }
            }
        }
        else if (backgroundImage != null)
        {
            backgroundImage.color = exposed
                ? new Color(1f, 0.99f, 0.94f, 1f)
                : new Color(0.58f, 0.52f, 0.46f, 0.92f);
        }

        if (baseImage != null)
        {
            float baseDim = exposed ? 1f : 0.72f;
            baseImage.color = new Color(
                baseBaseColor.r * baseDim,
                baseBaseColor.g * baseDim,
                baseBaseColor.b * baseDim,
                baseBaseColor.a);
        }

        if (iconImage != null)
        {
            iconImage.color = new Color(
                baseIconColor.r * iconDim,
                baseIconColor.g * iconDim,
                baseIconColor.b * iconDim,
                alpha);
        }

        if (iconText != null)
        {
            Color color = iconText.color;
            color.a = alpha;
            iconText.color = color;
        }

        if (typeText != null)
        {
            Color color = typeText.color;
            color.a = alpha;
            typeText.color = color;
        }

        RectTransform rect = RectTransform;
        if (rect != null)
        {
            rect.localScale = Vector3.one;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isBusy || isRemoved || !isExposed || gameController == null)
        {
            return;
        }

        gameController.OnClickBoardItem(this);
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;
    }

    public void MarkRemoved()
    {
        isRemoved = true;
        isBusy = true;
    }

    private static string GetDisplayName(SortingItemType type)
    {
        SortingItemDef def = SortingItemCatalog.Get(type);
        return def != null ? def.displayName : "Item";
    }

    private static string GetGlyph(SortingItemType type)
    {
        SortingItemDef def = SortingItemCatalog.Get(type);
        return def != null && !string.IsNullOrEmpty(def.glyph) ? def.glyph : "?";
    }

    private static readonly string[] TilePalette = { "tile_orange", "tile_purple", "tile_blue" };
    private static string currentLevelTileSprite = "tile_orange";

    public static void SetLevelTile(int levelIndex)
    {
        currentLevelTileSprite = TilePalette[Mathf.Abs(levelIndex) % TilePalette.Length];
    }

    private static Sprite LoadTileSprite()
    {
        return Resources.Load<Sprite>($"Tiles/{currentLevelTileSprite}");
    }

    private static Sprite LoadIconSprite(SortingItemType type)
    {
        if (type == SortingItemType.None)
        {
            return null;
        }

        Sprite icon = Resources.Load<Sprite>($"TileIcons/{type}");
        if (icon != null)
        {
            return icon;
        }

        return SortingProceduralIconProvider.Get(type);
    }

    private static Color GetIconFrameColor(SortingItemType type)
    {
        SortingItemDef def = SortingItemCatalog.Get(type);
        return def != null ? def.frameColor : Color.white;
    }

    private static Color GetColor(SortingItemType type)
    {
        SortingItemDef def = SortingItemCatalog.Get(type);
        return def != null ? def.mainColor : Color.white;
    }
}
