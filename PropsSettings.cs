using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PropsSettings : ScriptableObject
{
    public GameObject[] props;
    public LayerMask planet;
    public int radius;
    public int quantity;
    public int sea_level;
    public float minRandom, maxRandom;
}
