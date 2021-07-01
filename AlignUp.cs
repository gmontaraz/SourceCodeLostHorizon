using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class AlignUp : MonoBehaviour
{
    // Update is called once per frame
    private void Update()
    {
        transform.up = transform.position-planet.position;
    }
    [SerializeField]
    Transform planet;
}
