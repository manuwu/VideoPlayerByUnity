using UnityEngine;
using System.Collections;
 
public class DragCameraControl : MonoBehaviour 
{
	private int MouseWheelSensitivity = 1;
	private int MouseZoomMin = 0;
	private int MouseZoomMax = 3;
	private float normalDistance = 0;

	private float xSpeed = 250.0f;
	private float ySpeed = 120.0f;
 
	private int yMinLimit = -20;
	private int yMaxLimit = 80;
 
	private float x = 0.0f;
	private float y = 0.0f;
	
	void Start () 
	{
		var angles = transform.eulerAngles;
    	x = angles.y;
    	y = angles.x;
	}
 
	void Update ()
	{
		if (PlayerManager.isCanDragCamera)
		{
			if (Input.GetMouseButton(0))
			{
				x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
				y = ClampAngle(y, yMinLimit, yMaxLimit);
				var rotation = Quaternion.Euler(y, x, 0);

				transform.rotation = rotation;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{

				if (normalDistance >= MouseZoomMin && normalDistance <= MouseZoomMax)
				{
					normalDistance -= Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity;
				}

				if (normalDistance < MouseZoomMin)
				{
					normalDistance = MouseZoomMin;
				}

				if (normalDistance > MouseZoomMax)
				{
					normalDistance = MouseZoomMax;
				}

				transform.localPosition = Vector3.forward * normalDistance;
			}
		}
	}
 
	static float ClampAngle (float angle , float min ,float  max) 
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
}