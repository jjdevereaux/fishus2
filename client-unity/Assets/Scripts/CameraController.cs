using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static float WorldSize = 0.0f;
    [SerializeField]
    private Vector3 CameraOffset = new Vector3(0.0f, 10.0f, 0.0f);
    private Vector3 FishermanTransformPosition = new Vector3();
	private void LateUpdate()
    {
        var arenaCenterTransform = new Vector3(WorldSize / 2, WorldSize / 2, -10.0f);
        if (PlayerController.Local == null || !GameManager.IsConnected())
        {
            // Set the camera to be in middle of the arena if we are not connected or 
            // there is no local player
            transform.position = arenaCenterTransform;
            return;
        }

        FishermanTransformPosition = (Vector3)PlayerController.Local.FishermanLocation();
        if (FishermanTransformPosition != null)
        {
            Vector3 temp = new Vector3();
            temp.x = FishermanTransformPosition.x + CameraOffset.x;
            temp.y = FishermanTransformPosition.y + CameraOffset.y;
            temp.z = FishermanTransformPosition.z + CameraOffset.z;
            transform.position=temp;

        } else {
            transform.position = arenaCenterTransform;
        }

		float targetCameraSize = CalculateCameraSize(PlayerController.Local);
		Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetCameraSize, Time.deltaTime * 2);
        Camera.main.transform.LookAt(FishermanTransformPosition);
	}

	private float CalculateCameraSize(PlayerController player)
	{
		return 50f;
           
	}
}

