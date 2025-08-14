using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleColorAnim : MonoBehaviour
{
    [SerializeField] private Material material;
    private float intensity = 0.15f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(AnimCoroutine());
    }

    private IEnumerator AnimCoroutine()
    {
        material.color = Color.white;
        yield return new WaitForSeconds(intensity);
        material.color = Color.red;
        yield return new WaitForSeconds(intensity);
        StartCoroutine(AnimCoroutine());
    }

}
