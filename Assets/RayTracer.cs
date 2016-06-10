using UnityEngine;
using System.Collections;

public class RayTracer : MonoBehaviour
{

    public Color backgroundColor = Color.black;
    public float RenderResolution = 1f;
    public float maxDist = 100f;
    public int maxRecursion = 4;
    public Camera maincamera;

    private Light[] lights;
    private Texture2D renderTexture;

    void Awake()
    {
        renderTexture = new Texture2D((int)(Screen.width * RenderResolution), (int)(Screen.height * RenderResolution));
        lights = FindObjectsOfType(typeof(Light)) as Light[];
    }

    void Start()
    {
        RayTrace();
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTexture);
    }

    void RayTrace()
    {
        for (int x = 0; x < renderTexture.width; x++)
        {
            for (int y = 0; y < renderTexture.height; y++)
            {

                Color color = Color.black;
                
                Ray ray = maincamera.ScreenPointToRay(new Vector3(x / RenderResolution, y / RenderResolution, 0));

                renderTexture.SetPixel(x, y, TraceRay(ray, color, 0));
            }
        }

        renderTexture.Apply();
    }

    Color TraceRay(Ray ray, Color color, int recursiveLevel)
    {

        if (recursiveLevel < maxRecursion)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDist))
            {
                Vector3 viewVector = ray.direction;
                Vector3 pos = hit.point + hit.normal * 0.0001f;
                Vector3 normal = hit.normal;

                RayTracerObject rto = hit.collider.gameObject.GetComponent<RayTracerObject>();

                Material mat = hit.collider.GetComponent<Renderer>().material;
                if (mat.mainTexture)
                {
                    color += (mat.mainTexture as Texture2D).GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
                }
                else
                {
                    color += mat.color;
                }

                color *= TraceLight(rto, viewVector, pos, normal);

                if (rto.reflectiveCoeff > 0)
                {
                    float reflet = 2.0f * Vector3.Dot(viewVector, normal);
                    Ray newRay = new Ray(pos, viewVector - reflet * normal);
                    color += rto.reflectiveCoeff * TraceRay(newRay, color, recursiveLevel + 1);
                }

                if (rto.transparentCoeff > 0)
                {
                    Ray newRay = new Ray(hit.point - hit.normal * 0.0001f, viewVector);
                    color += rto.transparentCoeff * TraceRay(newRay, color, recursiveLevel + 1);
                }
            }
        }

        return color;

    }

    Color TraceLight(RayTracerObject rto, Vector3 viewVector, Vector3 pos, Vector3 normal)
    {
        Color c = RenderSettings.ambientLight;

        foreach (Light light in lights)
        {
            if (light.enabled)
            {
                c += LightTrace(rto, light, viewVector, pos, normal);
            }
        }
        return c;
    }

    Color LightTrace(RayTracerObject rto, Light light, Vector3 viewVector, Vector3 pos, Vector3 normal)
    {


        float dot, distance, contribution;
        Vector3 direction;
        switch (light.type)
        {
            case LightType.Directional:
                contribution = 0;
                direction = -light.transform.forward;
                dot = Vector3.Dot(direction, normal);
                if (dot > 0)
                {
                    if (Physics.Raycast(pos, direction, maxDist))
                    {
                        return Color.black;
                    }

                    if (rto.lambertCoeff > 0)
                    {
                        contribution += dot * rto.lambertCoeff;
                    }
                    if (rto.reflectiveCoeff > 0)
                    {
                        if (rto.phongCoeff > 0)
                        {
                            float reflet = 2.0f * Vector3.Dot(viewVector, normal);
                            Vector3 phongDir = viewVector - reflet * normal;
                            float phongTerm = max(Vector3.Dot(phongDir, viewVector), 0.0f);
                            phongTerm = rto.reflectiveCoeff * Mathf.Pow(phongTerm, rto.phongPower) * rto.phongCoeff;

                            contribution += phongTerm;
                        }
                        if (rto.blinnPhongCoeff > 0)
                        {
                            Vector3 blinnDir = -light.transform.forward - viewVector;
                            float temp = Mathf.Sqrt(Vector3.Dot(blinnDir, blinnDir));
                            if (temp != 0.0f)
                            {
                                blinnDir = (1.0f / temp) * blinnDir;
                                float blinnTerm = max(Vector3.Dot(blinnDir, normal), 0.0f);
                                blinnTerm = rto.reflectiveCoeff * Mathf.Pow(blinnTerm, rto.blinnPhongPower) * rto.blinnPhongCoeff;

                                contribution += blinnTerm;
                            }
                        }
                    }
                }
                return light.color * light.intensity * contribution;
            case LightType.Point:
                contribution = 0;
                direction = (light.transform.position - pos).normalized;
                dot = Vector3.Dot(normal, direction);
                distance = Vector3.Distance(pos, light.transform.position);
                if ((distance < light.range) && (dot > 0))
                {
                    if (Physics.Raycast(pos, direction, distance))
                    {
                        return Color.black;
                    }

                    if (rto.lambertCoeff > 0)
                    {
                        contribution += dot * rto.lambertCoeff;
                    }
                    if (rto.reflectiveCoeff > 0)
                    {
                        if (rto.phongCoeff > 0)
                        {
                            float reflet = 2.0f * Vector3.Dot(viewVector, normal);
                            Vector3 phongDir = viewVector - reflet * normal;
                            float phongTerm = max(Vector3.Dot(phongDir, viewVector), 0.0f);
                            phongTerm = rto.reflectiveCoeff * Mathf.Pow(phongTerm, rto.phongPower) * rto.phongCoeff;

                            contribution += phongTerm;
                        }
                        if (rto.blinnPhongCoeff > 0)
                        {
                            Vector3 blinnDir = -light.transform.forward - viewVector;
                            float temp = Mathf.Sqrt(Vector3.Dot(blinnDir, blinnDir));
                            if (temp != 0.0f)
                            {
                                blinnDir = (1.0f / temp) * blinnDir;
                                float blinnTerm = max(Vector3.Dot(blinnDir, normal), 0.0f);
                                blinnTerm = rto.reflectiveCoeff * Mathf.Pow(blinnTerm, rto.blinnPhongPower) * rto.blinnPhongCoeff;

                                contribution += blinnTerm;
                            }
                        }
                    }
                }
                if (contribution == 0)
                {
                    return Color.black;
                }
                return light.color * light.intensity * contribution;
            case LightType.Spot:
                contribution = 0;
                direction = (light.transform.position - pos).normalized;
                dot = Vector3.Dot(normal, direction);
                distance = Vector3.Distance(pos, light.transform.position);
                if (distance < light.range && dot > 0)
                {
                    float dot2 = Vector3.Dot(-light.transform.forward, direction);
                    if (dot2 > (1 - light.spotAngle / 180))
                    {
                        if (Physics.Raycast(pos, direction, distance))
                        {
                            return Color.black;
                        }
                        if (rto.lambertCoeff > 0)
                        {
                            contribution += dot * rto.lambertCoeff;
                        }
                        if (rto.reflectiveCoeff > 0)
                        {
                            if (rto.phongCoeff > 0)
                            {
                                float reflet = 2.0f * Vector3.Dot(viewVector, normal);
                                Vector3 phongDir = viewVector - reflet * normal;
                                float phongTerm = max(Vector3.Dot(phongDir, viewVector), 0.0f);
                                phongTerm = rto.reflectiveCoeff * Mathf.Pow(phongTerm, rto.phongPower) * rto.phongCoeff;

                                contribution += phongTerm;
                            }
                            if (rto.blinnPhongCoeff > 0)
                            {
                                Vector3 blinnDir = -light.transform.forward - viewVector;
                                float temp = Mathf.Sqrt(Vector3.Dot(blinnDir, blinnDir));
                                if (temp != 0.0f)
                                {
                                    blinnDir = (1.0f / temp) * blinnDir;
                                    float blinnTerm = max(Vector3.Dot(blinnDir, normal), 0.0f);
                                    blinnTerm = rto.reflectiveCoeff * Mathf.Pow(blinnTerm, rto.blinnPhongPower) * rto.blinnPhongCoeff;

                                    contribution += blinnTerm;
                                }
                            }
                        }
                    }
                }
                if (contribution == 0)
                {
                    return Color.black;
                }
                return light.color * light.intensity * contribution;
        }
        return Color.black;
    }

    float max(float x0, float x1)
    {
        return x0 > x1 ? x0 : x1;
    }
}