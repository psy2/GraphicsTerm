using UnityEngine;
using System.Collections;

public class turnX : MonoBehaviour {
    float TurnSpeed;
    Vector3 V3;
    // Use this for initialization
    void Start () {
        TurnSpeed = 2f;

    }

    // Update is called once per frame
    void Update () {
        V3 = new Vector3(0, Input.GetAxis("Mouse X"), 0);
        transform.Rotate(V3 * TurnSpeed);

    }
}
