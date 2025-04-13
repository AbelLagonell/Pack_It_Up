using UnityEngine;
using UnityEngine.UI;

public class PointAt : MonoBehaviour {
    private RectTransform rectTransform;
    private Image image;
    public GameObject target;
    public Transform referenceTransform;
    public NetworkPlayerManager npm;

    public Vector3 worldUp;

    void Start() {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        image.enabled = false;
        // Find the object with the "GameEnd" tag
        target = GameObject.FindWithTag("GameEnd");

        referenceTransform = npm.player.transform;

        if (target == null) {
            Debug.LogWarning("No object with tag 'GameEnd' found!");
        }
    }

    void Update() {
        //if (GameManager.GlobalTimer > 0) return;
        image.enabled = true;
        if (!target) return;

        // Get the direction to the target
        Vector3 direction = target.transform.position - referenceTransform.position;


        image.color = new Color(1, 1, 1, Mathf.Clamp((direction.magnitude - 3f) / 50f, 0f, 1f));


        // Calculate the angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Calculate current z rotation
        float currentZRotation = rectTransform.eulerAngles.z;

        // Calculate the rotation needed to point at the target (with -90 offset to point correctly)
        float targetZRotation = angle - 90;

        // Calculate the rotation difference
        float rotationDifference = Mathf.DeltaAngle(currentZRotation, targetZRotation);

        // Rotate the RectTransform around the Z axis only
        rectTransform.Rotate(0, 0, rotationDifference);
    }
}