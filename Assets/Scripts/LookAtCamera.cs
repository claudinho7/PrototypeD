using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private void Update()
    {
        // Get the direction from the text position to the camera position
        if (Camera.main != null)
        {
            var directionToCamera = Camera.main.transform.position - transform.position;

            // Ensure the text always faces the camera but doesn't rotate with it
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }

        transform.Rotate(0, 180, 0); // Optionally rotate 180 degrees to face the camera properly
    }
}
