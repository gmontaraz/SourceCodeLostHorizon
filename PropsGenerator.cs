using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceGraphicsToolkit;

public class PropsGenerator:MonoBehaviour 
{
    PropsSettings settings;
    Transform planet;
    Transform props;
    SgtSharedMaterial sharedMaterial;
    public void UpdateSettings(PropsSettings settings,Transform planet, Transform props,SgtSharedMaterial sharedMaterial)
    {
        this.settings = settings;
        this.planet = planet;
        this.props = props;
        this.sharedMaterial = sharedMaterial;
    }
    public void GenerateProp()
    {
        for (int i = 0; i < settings.quantity; i++)
        {
            
            GameObject prop;
            Vector3 randomdirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            RaycastHit hit;
            Ray ray = new Ray(planet.position, randomdirection);
            Vector3 origin = ray.GetPoint(settings.radius*2);
            Vector3 direction = origin - planet.position;
            Debug.DrawRay(origin, direction * (settings.radius * 2));
            if (Physics.Raycast(origin, -direction, out hit, Mathf.Infinity, settings.planet))
            {
                if (Vector3.Distance(origin, hit.point) < (settings.radius-settings.sea_level))
                {
                    int random_prop = Random.Range(0, settings.props.Length);
                    float random_scale = Random.Range(settings.minRandom, settings.maxRandom);
                    prop = Instantiate(settings.props[random_prop], hit.point, Quaternion.identity, props);
                    prop.transform.localScale = new Vector3(random_scale, random_scale, random_scale);
                    prop.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
                    sharedMaterial.AddRenderer(prop.GetComponent<MeshRenderer>());
                    Vector3 gravityUp = (prop.transform.position - planet.position).normalized;
                    Vector3 localUp = prop.transform.up;
                    prop.transform.rotation = Quaternion.FromToRotation(localUp, gravityUp) * prop.transform.rotation;
                }
            }
        }
    }
}
