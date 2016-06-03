using UnityEngine;
using System.Collections;

public class characterMove : MonoBehaviour {
    public CharacterController CC;
    float MoveSpeed;
	// Use this for initialization
    void Start () {
        MoveSpeed = 50f;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
            CC.Move(transform.forward * MoveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S))
            CC.Move(transform.forward * -1f * MoveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A))
            CC.Move(transform.right * -1f * MoveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            CC.Move(transform.right * MoveSpeed * Time.deltaTime);


    }
}
