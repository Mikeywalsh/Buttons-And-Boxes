using UnityEngine;
using UnityEngine.UI;

sealed public class NotificationMessage : MonoBehaviour {

    private float startTime;

    void Update()
    {
        if (Time.time - startTime < 3)
            return;

        if (GetComponent<Text>().color.a > 0)
            GetComponent<Text>().color = new Color(1, 0, 0, GetComponent<Text>().color.a - 0.01f);
        else
            gameObject.SetActive(false);
    }

    public void SetMessage(string message)
    {
        GetComponent<Text>().color = new Color(1, 0, 0, 1);
        GetComponent<Text>().text = message;
        startTime = Time.time;
    }
}
