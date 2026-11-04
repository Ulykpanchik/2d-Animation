using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchDirection : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public PlayerMovement player;
    public int direction;
    readonly HashSet<int> pointers = new HashSet<int>();
    public void OnPointerDown(PointerEventData e) { pointers.Add(e.pointerId); Apply(); }
    public void OnPointerUp(PointerEventData e) { pointers.Remove(e.pointerId); Apply(); }
    public void OnPointerExit(PointerEventData e) { pointers.Remove(e.pointerId); Apply(); }
    void OnDisable() { pointers.Clear(); Apply(); }
    void Apply() { if(player!=null) player.SetTouchDirection(direction,pointers.Count>0); }
}
