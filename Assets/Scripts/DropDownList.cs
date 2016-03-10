using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DropDownList : MonoBehaviour {

	public GameObject linkedButton;
	public DropDownList listBelow;
	public DropDownList listAbove;
    public int height;

    public static int droppedCount;
	static float lastClick;

	private bool dropped;
	private float actualYPos;
	private Vector2 baseOffsetMin;
	
	void Start () {
		dropped = true;
        droppedCount = 4;
		baseOffsetMin = gameObject.GetComponent<RectTransform>().offsetMin;
		actualYPos = transform.parent.gameObject.GetComponent<RectTransform> ().anchoredPosition.y;
	}

	public void ButtonClicked()
	{
		if(Time.time - 0.25f < lastClick)
			return;

		iTween.ValueTo(gameObject, iTween.Hash("from", gameObject.GetComponent<RectTransform>().offsetMin.y , "to", dropped? baseOffsetMin.y + 75 : baseOffsetMin.y, "time", 0.25f, "easetype", iTween.EaseType.linear, "onUpdate", "ResizeList"));
		iTween.RotateTo(linkedButton, iTween.Hash("z", dropped? 180 : 270, "time", 0.15f, "easetype", iTween.EaseType.linear));

		if(listBelow)
			listBelow.StartMoveList(dropped, height);

		dropped = !dropped;
		lastClick = Time.time;

		if(!dropped)
		{
			for(int i = 0; i < transform.childCount; i++)
				transform.GetChild(i).gameObject.SetActive(false);

            droppedCount--;
		}
		else
		{
            droppedCount++;
			StartCoroutine(EnableDroppedList());
		}
	}

	private void ResizeList(float newHeight)
	{
		gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(gameObject.GetComponent<RectTransform>().offsetMin.x, newHeight);
	}

	private void StartMoveList(bool d, float aboveHeight)
	{
		actualYPos += d ? aboveHeight : - aboveHeight;
		iTween.ValueTo(gameObject, iTween.Hash("from",transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition.y, "to", actualYPos, "time", 0.25f, "easetype", iTween.EaseType.linear, "onUpdate", "MoveList"));
		if(listBelow)
			listBelow.StartMoveList(d, aboveHeight);
	}

	private void MoveList(float newPos)
	{
		Vector2 parentPos = gameObject.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition;
		gameObject.transform.parent.gameObject.GetComponent<RectTransform> ().anchoredPosition = new Vector2(parentPos.x, newPos);
	}

	IEnumerator EnableDroppedList()
	{
		yield return new WaitForSeconds (0.2f);

		for(int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.SetActive(true);
	}
}