using UnityEngine;
using UnityEngine.EventSystems;

// Uses unscaled time so feedback remains responsive in the pause menu.
public class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    bool over, pressed;
    public void OnPointerEnter(PointerEventData e) { over = true; }
    public void OnPointerExit(PointerEventData e) { over = pressed = false; }
    public void OnPointerDown(PointerEventData e) { pressed = true; }
    public void OnPointerUp(PointerEventData e) { pressed = false; }
    void Update()
    {
        float target = pressed ? .96f : over ? 1.025f : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale,Vector3.one*target,
            1-Mathf.Exp(-24*Time.unscaledDeltaTime));
    }
    void OnDisable() { over=pressed=false; transform.localScale=Vector3.one; }
}
