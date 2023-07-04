using UnityEngine;
using UnityEngine.UI;

public class UIRenderTarget : MonoBehaviour
{
    public RawImage Image;

    public RenderTarget Render { get; private set; }

    public void Setup(RenderTarget render)
    {
        Render = render;
        Image.texture = render.Texture;
    }
}
