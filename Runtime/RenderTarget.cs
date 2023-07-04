using Parang;
using UnityEngine;

public enum PivotList
{
    Facility,
    Character,
    CaveUncle,
    Harbor,
    Trasure,
}

public class RenderTarget : MonoBehaviour
{
    public Camera Camera;
    public Vector2Int Size;
    public Transform[] Pivots;
    public int[] CameraSize;

    public RenderTexture Texture { get; private set; }

    public void Setup()
    {
        Texture = new RenderTexture(Size.x, Size.y, 24, RenderTextureFormat.ARGB32);
        Camera.targetTexture = Texture;
    }

    public Transform GetPivot(PivotList pivot)
    {
        int index = (int)pivot;
        Pivots.SetActive(false);
        Pivots[index].SetActive(true);
        Camera.orthographicSize = CameraSize[index];
        return Pivots[index];
    }
}
