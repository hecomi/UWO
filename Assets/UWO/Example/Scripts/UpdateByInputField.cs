using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpdateByInputField : MonoBehaviour, IPointerClickHandler
{
	public InputField inputField;

	void Start()
	{
		inputField.gameObject.SetActive(false);
	}

	public void UpdateText(string text)
	{
		GetComponent<Text>().text = text;
	}

	public void UpdateTextAndFinishInput(string text)
	{
		UpdateText(text);
		inputField.enabled = false;
		inputField.gameObject.SetActive(false);
		GlobalState.isInputting = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		inputField.gameObject.SetActive(true);
		inputField.enabled = true;
		GlobalState.isInputting = true;
		EventSystem.current.SetSelectedGameObject(inputField.gameObject);
	}
}
