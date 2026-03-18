using UnityEngine;

// adds a small hole in the fog around enemies so u can see them in the dark
public class VisibilityCircle : MonoBehaviour
{
    [SerializeField] private float radius = 0.8f;

    private void Awake()
    {
        Sprite sprite = BuildCircleSprite();

        GameObject maskObject = new GameObject("VisibilityMask");
        maskObject.transform.SetParent(transform);
        maskObject.transform.localPosition = Vector3.zero;
        maskObject.transform.localScale = Vector3.one;

        // the sprite mask punches a hole in the darkness overlay
        SpriteMask spriteMask = maskObject.AddComponent<SpriteMask>();
        spriteMask.sprite = sprite;
        spriteMask.alphaCutoff = 0.25f; // only punch where alpha is above this
    }

    private Sprite BuildCircleSprite()
    {
        const int RESOLUTION = 256;
        Texture2D texture = new Texture2D(RESOLUTION, RESOLUTION, TextureFormat.RGBA32, false);

        float centerX = RESOLUTION * 0.5f;
        float innerRadius = RESOLUTION * 0.35f; // fully visible inside
        float outerRadius = RESOLUTION * 0.50f; // soft edge fades to invisible here

        // build a circular gradient texture where the center is white and fully visible, and it fades to transparent at the edges
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
                    alpha = 1f - (pixelDistance - innerRadius) / (outerRadius - innerRadius);
                }
                texture.SetPixel(pixelX, pixelY, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        float pixelsPerUnit = RESOLUTION / (radius * 2f);
        return Sprite.Create(texture, new Rect(0, 0, RESOLUTION, RESOLUTION), Vector2.one * 0.5f, pixelsPerUnit);
    }
}
