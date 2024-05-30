using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class cameraController : MonoBehaviour
{
    [Header("---------- Camera Components ----------")]
    [SerializeField] Transform playerBody;
    [Range(50, 250)][SerializeField] public float sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY; // for later settings buildout
    [SerializeField] float interactDist; //for doors
    [SerializeField] LayerMask layers;
    [Header("---------- Shake Settings ----------")]
    [SerializeField] float shakeDuration;
    [SerializeField] AnimationCurve curve;
    [SerializeField] bool shakeStart = false; // testing camera shake

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
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime ;
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime ;

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
        playerBody.Rotate(Vector3.up * mouseX);

        //check for door interaction
        doorInteract();

        // Camera Shake Testing
        if (shakeStart)
        {
            shakeStart = false;
            StartCoroutine(Shaking());
        }
    }

    public void doorInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDist, layers))
        {
            if (hit.collider.gameObject.GetComponent<Door>())
            {
                if (UserInput.instance.InteractPressed) // T: I changed this to use unity's input system instead
                {
                    hit.collider.gameObject.GetComponent<Door>().openClose();
                }
            }
        }
    }

    IEnumerator Shaking()
    {
        //Vector3 startPosition = transform.position;
        //Transform playerTransform = transform.parent; // Get reference to player
        Vector3 originalLocalPosition = transform.localPosition; // Store the original local position
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float shakeStrength = curve.Evaluate(elapsedTime / shakeDuration);
            //transform.position = startPosition + Random.insideUnitSphere * shakeStrength;
            transform.localPosition = originalLocalPosition + Random.insideUnitSphere * shakeStrength;
            yield return null;
        }

        //transform.position = startPosition;
        transform.localPosition = originalLocalPosition; // Restore the original local position
    }
}
