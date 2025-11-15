using UnityEngine;

public class AccessibilityFilter : MonoBehaviour
{
    public enum FilterMode
    {
        Normal,
        HighContrast,
        Protanopia,
        Deuteranopia,
        Tritanopia
    }

    public FilterMode currentFilter = FilterMode.Normal;

    private Material filterMaterial;


    private static readonly float[][] ProtanopiaMatrix = {
        new float[]{0.567f, 0.433f, 0f},
        new float[]{0.558f, 0.442f, 0f},
        new float[]{0f,     0.242f, 0.758f}
    };

    private static readonly float[][] DeuteranopiaMatrix = {
        new float[]{0.625f, 0.375f, 0f},
        new float[]{0.7f,   0.3f,  0f},
        new float[]{0f,     0.3f,  0.7f}
    };

    private static readonly float[][] TritanopiaMatrix = {
        new float[]{0.95f,  0.05f,  0f},
        new float[]{0f,     0.433f, 0.567f},
        new float[]{0f,     0.475f, 0.525f}
    };

    private void Start()
    {
        Shader shader = Shader.Find("Hidden/AccessibilityFilter");
        filterMaterial = new Material(shader);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (currentFilter == FilterMode.Normal)
        {
            Graphics.Blit(src, dst);
            return;
        }

        if (currentFilter == FilterMode.HighContrast)
        {
            filterMaterial.SetFloat("_Contrast", 1.4f);
            filterMaterial.SetFloat("_ApplyMatrix", 0);
        }
        else
        {
            filterMaterial.SetFloat("_Contrast", 1f);
            filterMaterial.SetFloat("_ApplyMatrix", 1);

            switch (currentFilter)
            {
                case FilterMode.Protanopia:
                    filterMaterial.SetMatrix("_ColorMatrix", ToMatrix(ProtanopiaMatrix));
                    break;
                case FilterMode.Deuteranopia:
                    filterMaterial.SetMatrix("_ColorMatrix", ToMatrix(DeuteranopiaMatrix));
                    break;
                case FilterMode.Tritanopia:
                    filterMaterial.SetMatrix("_ColorMatrix", ToMatrix(TritanopiaMatrix));
                    break;
            }
        }

        Graphics.Blit(src, dst, filterMaterial);
    }

    Matrix4x4 ToMatrix(float[][] array)
    {
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = array[0][0]; m[0, 1] = array[0][1]; m[0, 2] = array[0][2];
        m[1, 0] = array[1][0]; m[1, 1] = array[1][1]; m[1, 2] = array[1][2];
        m[2, 0] = array[2][0]; m[2, 1] = array[2][1]; m[2, 2] = array[2][2];
        m[3, 3] = 1;
        return m;
    }
}

