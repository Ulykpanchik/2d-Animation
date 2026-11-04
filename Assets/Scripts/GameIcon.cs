using UnityEngine;
using UnityEngine.UI;

// Resolution-independent artwork; no external fonts or icon textures required.
[RequireComponent(typeof(CanvasRenderer))]
public class GameIcon : MaskableGraphic
{
    public enum Symbol { Heart, Pause, Jump, Shoot, Left, Right, Play, Diamond, Restart, Exit, Crown }
    public Symbol symbol;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (symbol == Symbol.Heart) Polygon(vh, new[] {new Vector2(.5f,.08f),new Vector2(.08f,.48f),new Vector2(.08f,.76f),new Vector2(.25f,.91f),new Vector2(.42f,.91f),new Vector2(.5f,.79f),new Vector2(.58f,.91f),new Vector2(.75f,.91f),new Vector2(.92f,.76f),new Vector2(.92f,.48f)});
        else if (symbol == Symbol.Pause) { Box(vh,.23f,.18f,.39f,.82f); Box(vh,.61f,.18f,.77f,.82f); }
        else if (symbol == Symbol.Jump) { Line(vh,new Vector2(.5f,.18f),new Vector2(.5f,.8f),.12f); Line(vh,new Vector2(.22f,.53f),new Vector2(.5f,.81f),.12f); Line(vh,new Vector2(.78f,.53f),new Vector2(.5f,.81f),.12f); Box(vh,.2f,.07f,.8f,.14f); }
        else if (symbol == Symbol.Shoot) { Line(vh,new Vector2(.16f,.22f),new Vector2(.83f,.78f),.085f); Line(vh,new Vector2(.51f,.78f),new Vector2(.83f,.78f),.085f); Line(vh,new Vector2(.83f,.46f),new Vector2(.83f,.78f),.085f); Line(vh,new Vector2(.24f,.19f),new Vector2(.13f,.43f),.085f); Line(vh,new Vector2(.24f,.19f),new Vector2(.48f,.25f),.085f); }
        else if(symbol==Symbol.Crown) {
            Polygon(vh,new[]{new Vector2(.15f,.32f),new Vector2(.08f,.81f),new Vector2(.32f,.60f),new Vector2(.5f,.94f),new Vector2(.68f,.60f),new Vector2(.92f,.81f),new Vector2(.85f,.32f)});
            Box(vh,.16f,.15f,.84f,.25f);
        }
        else if(symbol==Symbol.Restart) {
            for(int i=0;i<18;i++) {float a=(40+i*15)*Mathf.Deg2Rad,b=(40+(i+1)*15)*Mathf.Deg2Rad;Line(vh,new Vector2(.5f+.32f*Mathf.Cos(a),.5f+.32f*Mathf.Sin(a)),new Vector2(.5f+.32f*Mathf.Cos(b),.5f+.32f*Mathf.Sin(b)),.09f);}
            Polygon(vh,new[]{new Vector2(.71f,.91f),new Vector2(.92f,.65f),new Vector2(.58f,.67f)});
        }
        else if(symbol==Symbol.Exit) {Line(vh,new Vector2(.2f,.15f),new Vector2(.2f,.85f),.08f);Line(vh,new Vector2(.2f,.85f),new Vector2(.52f,.85f),.08f);Line(vh,new Vector2(.2f,.15f),new Vector2(.52f,.15f),.08f);Line(vh,new Vector2(.4f,.5f),new Vector2(.85f,.5f),.09f);Line(vh,new Vector2(.65f,.72f),new Vector2(.87f,.5f),.09f);Line(vh,new Vector2(.65f,.28f),new Vector2(.87f,.5f),.09f);}
        else if (symbol == Symbol.Diamond) Polygon(vh,new[]{new Vector2(.5f,0),new Vector2(1,.5f),new Vector2(.5f,1),new Vector2(0,.5f)});
        else if (symbol == Symbol.Left) Polygon(vh,new[]{new Vector2(.25f,.5f),new Vector2(.72f,.85f),new Vector2(.72f,.15f)});
        else Polygon(vh,new[]{new Vector2(.78f,.5f),new Vector2(.28f,.15f),new Vector2(.28f,.85f)});
    }
    void Box(VertexHelper vh,float x,float y,float r,float t) { Polygon(vh,new[]{new Vector2(x,y),new Vector2(r,y),new Vector2(r,t),new Vector2(x,t)}); }
    void Line(VertexHelper vh,Vector2 a,Vector2 b,float width) { Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*width/2; Polygon(vh,new[]{a+n,a-n,b-n,b+n}); }
    void Polygon(VertexHelper vh,Vector2[] points)
    {
        Rect r=rectTransform.rect; int start=vh.currentVertCount;
        Vector2 center=Vector2.zero; foreach(var p in points) center+=p; center/=points.Length;
        vh.AddVert(new Vector3(r.x+center.x*r.width,r.y+center.y*r.height),color,Vector2.zero);
        foreach(var p in points) vh.AddVert(new Vector3(r.x+p.x*r.width,r.y+p.y*r.height),color,Vector2.zero);
        for(int i=0;i<points.Length;i++) vh.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);
    }
}

