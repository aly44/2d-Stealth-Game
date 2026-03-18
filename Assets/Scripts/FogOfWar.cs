using UnityEngine;

// attach to player, handles the fog of war
public class FogOfWar : MonoBehaviour
{
    [SerializeField] private float visibilityRadius = 1.8f;
    [SerializeField] private float darknessAlpha = 0.93f;
    [SerializeField] private int darknessSortOrder = 50;
    [SerializeField] private int fovSortOrder = 60;

    public static int FOVSortOrder = 60;

    private void Awake()
    {
        FOVSortOrder = fovSortOrder;
        BuildMask();
        BuildDarknessOverlay();
    }

    private void BuildMask()
    {
        const int RESOLUTION = 256;
        Texture2D texture = new Texture2D(RESOLUTION, RESOLUTION, TextureFormat.RGBA32, false);

        float centerX = RESOLUTION * 0.5f;
        float innerRadius = RESOLUTION * 0.38f; // fully visible inside here
        float outerRadius = RESOLUTION * 0.50f; // fully hidden outside here, soft edge in between

        for (int pixelY = 0; pixelY < RESOLUTION; pixelY++)
        {
            for (int pixelX = 0; pixelX < RESOLUTION; pixelX++)
            {
                float pixelDistance = Vector2.Distance(new Vector2(pixelX, pixelY), new Vector2(centerX, centerX));
                float alpha;
                if (pixelDistance < innerRadius)
                {
                    alpha = 1f;
                }
                else if (pixelDistance > outerRadius)
                {
                    alpha = 0f;
                }
                else
                {
                    alpha = 1f - (pixelDistance - innerRadius) / (outerRadius - innerRadius); // fade out smoothly
                }
                texture.SetPixel(pixelX, pixelY, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        float pixelsPerUnit = RESOLUTION / (visibilityRadius * 2f);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, RESOLUTION, RESOLUTION), Vector2.one * 0.5f, pixelsPerUnit);

        // lock scale so player scale doesnt stretch the mask
        GameObject maskObject = new GameObject("VisibilityMask");
        maskObject.transform.SetParent(transform);
        maskObject.transform.localPosition = Vector3.zero;
        maskObject.transform.localScale = Vector3.one;

        SpriteMask spriteMask = maskObject.AddComponent<SpriteMask>();
        spriteMask.sprite = sprite;
        spriteMask.alphaCutoff = 0.25f;
    }

    private void BuildDarknessOverlay()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);

        GameObject overlayObject = new GameObject("DarknessOverlay");
        overlayObject.transform.SetParent(transform);
        overlayObject.transform.localPosition = Vector3.zero;
        overlayObject.transform.localScale = new Vector3(400f, 400f, 1f); // big enough to cover the whole map

        SpriteRenderer spriteRenderer = overlayObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = new Color(0.04f, 0.04f, 0.06f, darknessAlpha);
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask; // darkness shows everywhere except inside the mask
        spriteRenderer.sortingOrder = darknessSortOrder;
    }
}
