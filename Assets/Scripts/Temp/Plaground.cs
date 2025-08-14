using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plaground : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string str = "a text element for the status";
        str = str.ToLower();
        var b = str.Contains("a text element");
        Debug.LogWarning(str);
        Debug.Log(b);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
