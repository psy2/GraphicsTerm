using UnityEngine;
using System.Collections;

public class turnY : MonoBehaviour {
    float TurnSpeed;
    Vector3 V3;
    // Use this for initialization
    void Start()
    {
        TurnSpeed = 2f;
    }

    // Update is called once per frame
    void Update () {
        V3 = new Vector3(-Input.GetAxis("Mouse Y"), 0, 0);
        transform.Rotate(V3 * TurnSpeed);

    }
}
