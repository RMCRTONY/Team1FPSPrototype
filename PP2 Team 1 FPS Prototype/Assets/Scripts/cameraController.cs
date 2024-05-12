using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class cameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY; // for later settings buildout
    [SerializeField] float interactDist; //for doors
    [SerializeField] LayerMask layers;

    float rotX; // rotation on x-axis

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false; // bye bye
        Cursor.lockState = CursorLockMode.Locked; // good boy, stay.
    }

    // Update is called once per frame
    void Update()
    {

        // get that input gurl
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity;

        // inversion query check
        if (invertY)
            rotX += mouseY;
        else
            rotX -= mouseY;

        // clamp the rotx on the x
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        // rotate cam on x
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        // rotate player on y
        transform.parent.Rotate(Vector3.up * mouseX);

        //check for door interaction
        doorInteract();

    }

    public void doorInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDist, layers))
        {
            if (hit.collider.gameObject.GetComponent<Door>())
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.gameObject.GetComponent<Door>().openClose();
                }
            }
        }
    }
}
