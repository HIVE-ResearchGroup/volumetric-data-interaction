using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            gameObject.transform.position += new Vector3(-0.2f, 0, 0);
        if (Input.GetKey(KeyCode.D))
            gameObject.transform.position += new Vector3(0.2f, 0, 0);
        if (Input.GetKey(KeyCode.W))
            gameObject.transform.position += new Vector3(0, 0.2f, 0);
        if (Input.GetKey(KeyCode.S))
            gameObject.transform.position += new Vector3(0, -0.2f, 0);
        if (Input.GetKey(KeyCode.Y))
            gameObject.transform.position += new Vector3(0, 0, 0.2f);
        if (Input.GetKey(KeyCode.X))
            gameObject.transform.position += new Vector3(0, 0, -0.2f);
    }
}

