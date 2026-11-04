using System.Collections;
using UnityEngine;

public class VictoryReveal : MonoBehaviour
{
    public CanvasGroup overlay, heading, message, actions;
    public RectTransform card, emblem;
    public RectTransform[] sparks;
    public float duration = .65f;
    Vector2[] origins;

    public void Play()
    {
        overlay.alpha = heading.alpha = message.alpha = actions.alpha = 0;
        actions.interactable = false;
        actions.blocksRaycasts = false;
        origins = new Vector2[sparks.Length];
        for (int i=0;i<sparks.Length;i++) origins[i]=sparks[i].anchoredPosition;
        StartCoroutine(Reveal());
    }
    IEnumerator Reveal()
    {
        float time=0;
        float length=Mathf.Max(.65f,duration);
        while(time<length)
        {
            time+=Time.unscaledDeltaTime;
            float t=Mathf.Clamp01(time/length);
            float ease=1-Mathf.Pow(1-t,3);
            overlay.alpha=Mathf.Clamp01(t*3);
            card.anchoredPosition=new Vector2(0,Mathf.Lerp(-32,0,ease));
            card.localScale=Vector3.one*Mathf.Lerp(.92f,1,ease);
            emblem.localScale=Vector3.one*Mathf.Lerp(.65f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(t*1.8f)));
            heading.alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.12f)/.4f));
            message.alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.28f)/.45f));
            actions.alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.45f)/.5f));
            yield return null;
        }
        overlay.alpha=heading.alpha=message.alpha=actions.alpha=1;
        card.anchoredPosition=Vector2.zero; card.localScale=emblem.localScale=Vector3.one;
        actions.interactable=actions.blocksRaycasts=true;
        while(true)
        {
            time+=Time.unscaledDeltaTime;
            for(int i=0;i<sparks.Length;i++)
            {
                sparks[i].anchoredPosition=origins[i]+Vector2.up*(Mathf.Sin(time*1.4f+i*1.7f)*5);
                sparks[i].localScale=Vector3.one*(.8f+.2f*Mathf.Sin(time*2+i));
            }
            yield return null;
        }
    }
}
