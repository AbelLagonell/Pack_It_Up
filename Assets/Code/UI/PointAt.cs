using UnityEngine;

public class PointAt : MonoBehaviour {
    private RectTransform rectTransform;
    private GameObject target;

    [Header("Settings")] [Tooltip("Optional offset angle in degrees")]
    public float angleOffset = 0f;

    [Tooltip("True to keep pointer on screen edges, false to allow it to move anywhere")]
    public bool keepOnScreen = true;

    [Tooltip("Distance from screen edge when keepOnScreen is enabled")]
    public float screenBorderOffset = 25f;

    void Start() {
        rectTransform = GetComponent<RectTransform>();

        // Find the object with the "GameEnd" tag
        target = GameObject.FindWithTag("GameEnd");

        if (target == null) {
            Debug.LogWarning("No object with tag 'GameEnd' found!");
        }
    }

    void Update() {
        if (GameManager.GlobalTimer > 0) return;
        
        if (target == null) return;

        // Get the screen position of the target
        Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        // Calculate if target is behind camera
        bool isBehind = targetScreenPos.z < 0;

        // If target is behind camera, flip the position
        if (isBehind) {
            targetScreenPos.x = Screen.width - targetScreenPos.x;
            targetScreenPos.y = Screen.height - targetScreenPos.y;
        }

        // Calculate direction vector from UI object to target
        Vector3 direction = targetScreenPos - new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // Calculate angle
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + angleOffset;

        // Apply rotation
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        // Handle keeping indicator on screen
        if (keepOnScreen) {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 cappedPosition;

            if (IsPositionOnScreen(targetScreenPos)) {
                // Target is on screen, position the UI element at the target position
                cappedPosition = targetScreenPos;
            } else {
                // Target is off screen, find edge position
                direction.Normalize();

                // Find screen edge intersection
                float screenRadius = Mathf.Min(Screen.width, Screen.height) / 2 - screenBorderOffset;
                cappedPosition = screenCenter + direction * screenRadius;
            }

            // Update UI position on canvas
            Vector3 localPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                                                                    rectTransform.parent as RectTransform,
                                                                    cappedPosition,
                                                                    null,
                                                                    out localPosition);

            rectTransform.position = localPosition;
        }
    }

    bool IsPositionOnScreen(Vector3 position) {
        return position.x > 0 && position.x < Screen.width &&
               position.y > 0 && position.y < Screen.height &&
               position.z > 0;
    }
}