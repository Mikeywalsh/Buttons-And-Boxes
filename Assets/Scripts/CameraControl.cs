using UnityEngine;
using System.Collections;

public sealed class CameraControl : MonoBehaviour {

	//All variables here are for the level editor only
	public bool editor;
    public bool disableRotation;
	Vector3 fixatedPoint;
    Vector3 actualPivotPoint;
	Vector3 actualFixatedPoint;
	float zoom;

	public void ChangeBackgroundColour(Color backgroundColor)
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", GetComponent<Camera>().backgroundColor, "to", backgroundColor, "time", 2f, "onUpdate", "BackgroundColourUpdate", "onupdatetarget", gameObject));
	}
	
	private void BackgroundColourUpdate(Color c)
	{
		GetComponent<Camera>().backgroundColor = c;
	}

	void Start()
	{
		if(!editor)
			return;

		zoom = 10;
	}

	void Update()
	{
		if(!editor || disableRotation)
			return;

		gameObject.transform.LookAt(fixatedPoint);

		#region Camera Control Via Keyboard Input
		if(Input.GetKey(KeyCode.W) && transform.localRotation.eulerAngles.x < 75f)
		{
			gameObject.transform.RotateAround(transform.parent.position, Vector3.Cross(transform.position - transform.parent.position, Vector3.up), 1.3f);
		}
		if(Input.GetKey(KeyCode.S) && transform.localRotation.eulerAngles.x > 15f)
		{
			gameObject.transform.RotateAround(transform.parent.position, Vector3.Cross(transform.parent.position - transform.position, Vector3.up), 1.3f);
		}
		if(Input.GetKey(KeyCode.A))
		{
			gameObject.transform.RotateAround(transform.parent.position, Vector3.up, 1.3f);
		}
		if(Input.GetKey(KeyCode.D))
		{
			gameObject.transform.RotateAround(transform.parent.position, Vector3.up, -1.3f);
		}
		if(Input.GetKey(KeyCode.Q))
		{
			//fixatedPoint += Vector3.Cross(transform.position - pivotPoint, Vector3.up).normalized;
			//transform.Translate(Vector3.Cross(transform.position - pivotPoint, Vector3.up).normalized);
		}
		if(Input.GetKey(KeyCode.E))
		{
			//fixatedPoint += Vector3.Cross(transform.position - pivotPoint, Vector3.up).normalized;
			//transform.Translate(Vector3.Cross(transform.position - pivotPoint, Vector3.up).normalized);
		}
		if(Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			transform.position += (transform.position - fixatedPoint).normalized * 1.5f;
		}
		if(Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			transform.position += (fixatedPoint - transform.position).normalized * 1.5f;
		}
		#endregion
	}

	public void ChangePivotPoint(Vector3 amount)
	{
		actualPivotPoint += amount;
		actualFixatedPoint += amount;
		iTween.ValueTo (gameObject, iTween.Hash ("from", transform.parent.position, "to", actualPivotPoint, "time", .5f, "onupdate", "ChangePivotTween"));
		iTween.ValueTo (gameObject, iTween.Hash ("from", fixatedPoint, "to", actualFixatedPoint, "time", .5f, "onupdate", "ChangeFixateTween"));
	}

    public void SetPivotPoint(Vector3 point)
    {
        actualPivotPoint = point;
        actualFixatedPoint = point;
        transform.parent.position = actualFixatedPoint;
        fixatedPoint = actualFixatedPoint;
    }

    void ChangeZoomTween()
	{
		//TODO Create smoother zooming transition
	}
	
	void ChangePivotTween(Vector3 val)
	{
		transform.parent.position = val;
	}

	void ChangeFixateTween(Vector3 val)
	{
		fixatedPoint = val;
	}
}