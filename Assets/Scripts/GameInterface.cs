using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// Created once per scene, including when a level is run directly in the editor.
public class GameInterface : MonoBehaviour
{
    static readonly Color Ink = new Color(.045f,.035f,.095f,1f);
    static readonly Color Panel = new Color(.105f,.085f,.18f,1f);
    static readonly Color Gold = new Color(.95f,.72f,.36f);
    static readonly Color White = new Color(.94f,.95f,.93f);
    static readonly Color Muted = new Color(.76f,.75f,.83f);
    static readonly Color Red = new Color(.96f,.29f,.36f);
    public static bool BlocksGameplay { get; private set; }
    RectTransform safe;
    GameObject modal, controls, hud;
    TMP_Text modalTitle, volumeText, bossText;
    Image bossFill;
    GameObject bossPanel;
    GameIcon[] hearts;
    PlayerHealth health;
    PlayerMovement player;
    Boss boss;
    bool menu, paused, victoryShown, loadingScene;
    float previousScale = 1;
    Rect lastSafe;
    Vector2Int lastSize;
    TMP_FontAsset font;
    static Sprite rounded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() { BlocksGameplay=false; SceneManager.sceneLoaded-=Loaded; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register() { SceneManager.sceneLoaded+=Loaded; }
    static void Loaded(Scene scene,LoadSceneMode mode)
    {
        if(mode==LoadSceneMode.Additive || FindAnyObjectByType<GameInterface>()!=null) return;
        new GameObject("Game Interface").AddComponent<GameInterface>();
    }
    void Awake()
    {
        menu=SceneManager.GetActiveScene().name=="MainMenu";
        BlocksGameplay=menu;
        Time.timeScale=1; AudioListener.pause=false;
        AudioListener.volume=PlayerPrefs.GetFloat("MasterVolume",.8f);
        font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        EnsureSprite();
        var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=100; canvas.pixelPerfect=true;
        var scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720); scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        gameObject.AddComponent<GraphicRaycaster>();
        if(EventSystem.current==null) new GameObject("UI Event System",typeof(EventSystem),typeof(InputSystemUIInputModule));
        if(menu) MakeMenuBackdrop();
        safe=Rect("Safe Area",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        ApplySafeArea();
        if(menu) BuildMenu(); else BuildGame();
    }
    static void EnsureSprite()
    {
        if(rounded!=null) return;
        var texture=new Texture2D(64,64,TextureFormat.RGBA32,false); texture.name="UI rounded panel";
        texture.wrapMode=TextureWrapMode.Clamp; texture.filterMode=FilterMode.Bilinear;
        for(int y=0;y<64;y++) for(int x=0;x<64;x++) {
            float dx=Mathf.Max(Mathf.Abs(x-31.5f)-17.5f,0),dy=Mathf.Max(Mathf.Abs(y-31.5f)-17.5f,0);
            texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(14-Mathf.Sqrt(dx*dx+dy*dy))));
        }
        texture.Apply(); rounded=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(18,18,18,18));
    }
    RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 size,Vector2 pos)
    {
        var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rt.SetParent(parent,false);
        rt.anchorMin=min;rt.anchorMax=max;rt.sizeDelta=size;rt.anchoredPosition=pos;return rt;
    }
    RectTransform At(string name,Transform parent,Vector2 anchor,Vector2 size,Vector2 pos) { return Rect(name,parent,anchor,anchor,size,pos); }
    Image Surface(RectTransform rt,Color color,bool roundedCorners=true)
    {
        var img=rt.gameObject.AddComponent<Image>();img.color=color;img.raycastTarget=false;
        if(roundedCorners){img.sprite=rounded;img.type=Image.Type.Sliced;} return img;
    }
    TMP_Text Label(string text,Transform parent,Vector2 anchor,Vector2 size,Vector2 pos,int fontSize,Color color,TextAnchor alignment=TextAnchor.MiddleCenter)
    {
        var label=At(text,parent,anchor,size,pos).gameObject.AddComponent<TextMeshProUGUI>();
        label.font=font; label.text=text; label.fontSize=Mathf.Max(18,fontSize);
        label.fontStyle=FontStyles.Bold; label.characterSpacing=1.2f;
        label.color=color; label.raycastTarget=false;
        label.alignment=alignment==TextAnchor.MiddleLeft ? TextAlignmentOptions.MidlineLeft :
            alignment==TextAnchor.MiddleRight ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.Midline;
        label.textWrappingMode=TextWrappingModes.NoWrap; label.overflowMode=TextOverflowModes.Overflow;
        return label;
    }
    void Rule(Transform parent, float width, float y)
    {
        Surface(At("Fine rule",parent,new Vector2(.5f,.5f),new Vector2(width,1),new Vector2(0,y)),new Color(.95f,.72f,.36f,.35f),false);
        Icon(GameIcon.Symbol.Diamond,parent,new Vector2(8,8),new Vector2(0,y),Gold);
    }
    void Frame(RectTransform rt)
    {
        var outline=rt.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.79f,.66f,.45f,.65f);outline.effectDistance=new Vector2(1,-1);
        var shadow=rt.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.45f);shadow.effectDistance=new Vector2(0,-7);
        foreach(float x in new[]{0f,1f}) foreach(float y in new[]{0f,1f}) {
            var anchor=new Vector2(x,y);var sign=new Vector2(x==0?1:-1,y==0?1:-1);
            Surface(At("Corner horizontal",rt,anchor,new Vector2(22,2),new Vector2(sign.x*20,sign.y*10)),Gold,false);
            Surface(At("Corner vertical",rt,anchor,new Vector2(2,22),new Vector2(sign.x*10,sign.y*20)),Gold,false);
        }
    }
    GameIcon Icon(GameIcon.Symbol symbol,Transform parent,Vector2 size,Vector2 pos,Color color)
    {
        var icon=At(symbol.ToString(),parent,new Vector2(.5f,.5f),size,pos).gameObject.AddComponent<GameIcon>();
        icon.symbol=symbol;icon.color=color;icon.raycastTarget=false;return icon;
    }
    Button Button(string name,Transform parent,Vector2 anchor,Vector2 size,Vector2 pos,Color color,UnityAction action)
    {
        var rt=At(name,parent,anchor,size,pos);var image=Surface(rt,color);image.raycastTarget=true;
        var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;
        var colors=button.colors;colors.highlightedColor=new Color(1.16f,1.16f,1.16f);colors.pressedColor=new Color(.72f,.75f,.8f);colors.fadeDuration=.08f;button.colors=colors;
        var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;
        var edge=rt.gameObject.AddComponent<Outline>();edge.effectColor=new Color(.9f,.76f,.55f,.5f);edge.effectDistance=new Vector2(1,-1);
        var shadow=rt.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.5f);shadow.effectDistance=new Vector2(0,-4);
        Surface(Rect("Upper bevel",rt,new Vector2(0,1),new Vector2(1,1),new Vector2(-22,2),new Vector2(0,-4)),new Color(1,1,1,.25f),false);
        rt.gameObject.AddComponent<ButtonMotion>();
        if(action!=null)button.onClick.AddListener(action);return button;
    }
    Button TextButton(string text,Transform parent,Vector2 pos,UnityAction action,bool primary=false)
    {
        var b=Button(text,parent,new Vector2(.5f,.5f),new Vector2(380,66),pos,primary?Gold:new Color(.19f,.16f,.28f),action);
        Label(text,b.transform,new Vector2(.5f,.5f),new Vector2(300,54),new Vector2(12,0),24,primary?Ink:White);Icon(text=="RESTART" ? GameIcon.Symbol.Restart : text=="QUIT" ? GameIcon.Symbol.Exit : GameIcon.Symbol.Play,
            b.transform,new Vector2(26,26),new Vector2(-142,0),primary?Ink:Gold);
        return b;
    }
    void MakeMenuBackdrop()
    {
        if(Camera.main==null){var camera=new GameObject("Menu Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;}
        var backdrop=Rect("Night landscape",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var raw=backdrop.gameObject.AddComponent<RawImage>();raw.texture=Resources.Load<Texture2D>("Interface/NightBackdrop");raw.raycastTarget=false;
        Surface(Rect("Night veil",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero),new Color(.025f,.01f,.07f,.25f),false);
    }
    void BuildMenu()
    {
        var card=At("Main Menu",safe,new Vector2(.5f,.5f),new Vector2(540,580),Vector2.zero);Surface(card,Panel);Frame(card);
        Icon(GameIcon.Symbol.Diamond,card,new Vector2(30,30),new Vector2(0,228),Gold);
        Label("THE ADVENTURE",card,new Vector2(.5f,.5f),new Vector2(500,65),new Vector2(0,156),42,White);
        Label("BENEATH THE MOONLIGHT",card,new Vector2(.5f,.5f),new Vector2(460,30),new Vector2(0,105),18,Gold);
        Rule(card,400,65);
        TextButton("PLAY",card,new Vector2(0,1),()=>Load("Level1"),true);
        TextButton("QUIT",card,new Vector2(0,-87),QuitApplication);
        Rule(card,400,-151);
        Label("A / D   MOVE     SPACE   JUMP     F   SHOOT",card,new Vector2(.5f,.5f),new Vector2(480,32),new Vector2(0,-205),18,Muted);
    }
    void BuildGame()
    {
        health=FindAnyObjectByType<PlayerHealth>();player=FindAnyObjectByType<PlayerMovement>();boss=FindAnyObjectByType<Boss>();
        // Disable only the old controls, preserving their canvas and victory panel.
        foreach(var rt in FindObjectsByType<RectTransform>(FindObjectsInactive.Include))
            if(rt.name=="FireButton" || rt.name=="JumpButton" || rt.name=="Joystick")rt.gameObject.SetActive(false);
        foreach(var heart in FindObjectsByType<Heart>())heart.gameObject.SetActive(false);
        hud=Rect("HUD",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
        var hp=At("Player Health",hud.transform,new Vector2(0,1),new Vector2(220,88),new Vector2(134,-68));Surface(hp,Panel);
        Label("HEALTH",hp,new Vector2(.5f,1),new Vector2(160,20),new Vector2(0,-18),18,Muted,TextAnchor.MiddleLeft);
        hearts=new GameIcon[health!=null?Mathf.Max(1,health.maxHealth):3];
        float step=Mathf.Min(53,164f/hearts.Length);
        for(int i=0;i<hearts.Length;i++)hearts[i]=Icon(GameIcon.Symbol.Heart,hp,new Vector2(Mathf.Min(38,step-4),36),new Vector2((i-(hearts.Length-1)/2f)*step,-10),Red);
        var pause=Button("Pause",hud.transform,new Vector2(1,1),new Vector2(78,78),new Vector2(-63,-63),Panel,()=>SetPaused(true));
        Icon(GameIcon.Symbol.Pause,pause.transform,new Vector2(40,40),Vector2.zero,White);
        bossPanel=At("Boss Health",hud.transform,new Vector2(.5f,1),new Vector2(450,82),new Vector2(0,-65)).gameObject;Surface((RectTransform)bossPanel.transform,Panel);
        bossText=Label("BOSS",bossPanel.transform,new Vector2(.5f,.5f),new Vector2(390,24),new Vector2(0,18),19,White);
        var track=At("Health Track",bossPanel.transform,new Vector2(.5f,.5f),new Vector2(394,16),new Vector2(0,-14));Surface(track,Ink);
        bossFill=Surface(Rect("Health Fill",track,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero),Red);
        bossPanel.SetActive(boss!=null);
        controls=Rect("Touch Controls",safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
        MakeDirection(-1,new Vector2(76,86));MakeDirection(1,new Vector2(184,86));
        ActionButton("JUMP",GameIcon.Symbol.Jump,new Vector2(-202,92),new Color(.12f,.24f,.28f,1),()=>{if(player!=null)player.RequestJump();});
        ActionButton("SHOOT",GameIcon.Symbol.Shoot,new Vector2(-78,100),new Color(.36f,.23f,.12f,1),()=>{if(player!=null)player.RequestFire();});
        BuildPause();
    }
    void MakeDirection(int direction,Vector2 pos)
    {
        var b=Button(direction<0?"Move Left":"Move Right",controls.transform,Vector2.zero,new Vector2(92,92),pos,Panel,null);
        Icon(direction<0?GameIcon.Symbol.Left:GameIcon.Symbol.Right,b.transform,new Vector2(38,38),Vector2.zero,White);
        var touch=b.gameObject.AddComponent<TouchDirection>();touch.player=player;touch.direction=direction;
    }
    void ActionButton(string text,GameIcon.Symbol icon,Vector2 pos,Color color,UnityAction action)
    {
        var b=Button(text,controls.transform,new Vector2(1,0),new Vector2(112,112),pos,color,action);
        Icon(icon,b.transform,new Vector2(48,48),new Vector2(0,12),Gold);
        Label(text,b.transform,new Vector2(.5f,.5f),new Vector2(96,22),new Vector2(0,-32),18,White);
    }
    void BuildPause()
    {
        modal=Rect("Pause Overlay",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
        Surface((RectTransform)modal.transform,new Color(.015f,.025f,.045f,.86f),false).raycastTarget=true;
        var card=At("Pause Panel",modal.transform,new Vector2(.5f,.5f),new Vector2(540,620),Vector2.zero);Surface(card,Panel);Frame(card);
        Icon(GameIcon.Symbol.Pause,card,new Vector2(28,28),new Vector2(0,259),Gold);
        modalTitle=Label("PAUSED",card,new Vector2(.5f,.5f),new Vector2(460,60),new Vector2(0,207),40,White);
        Label("YOUR JOURNEY CAN WAIT",card,new Vector2(.5f,.5f),new Vector2(460,30),new Vector2(0,161),18,Muted);
        Rule(card,420,127);
        Label("MASTER VOLUME",card,new Vector2(.5f,.5f),new Vector2(290,32),new Vector2(-45,88),20,White,TextAnchor.MiddleLeft);
        volumeText=Label("",card,new Vector2(.5f,.5f),new Vector2(80,32),new Vector2(150,88),22,Gold,TextAnchor.MiddleRight);
        var sliderRoot=At("Master Volume",card,new Vector2(.5f,.5f),new Vector2(380,46),new Vector2(0,44));
        Surface(sliderRoot,new Color(0,0,0,0),false).raycastTarget=true;
        var track=Rect("Track",sliderRoot,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(0,12),Vector2.zero);Surface(track,Ink);
        var fillArea=Rect("Fill Area",sliderRoot,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(-28,12),Vector2.zero);
        var fill=Rect("Fill",fillArea,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);Surface(fill,Gold);
        var handleArea=Rect("Handle Area",sliderRoot,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(-28,0),Vector2.zero);
        var handle=At("Handle",handleArea,new Vector2(0,.5f),new Vector2(30,32),Vector2.zero);var handleImage=Surface(handle,White);handleImage.raycastTarget=true;
        Surface(At("Grip",handle,new Vector2(.5f,.5f),new Vector2(3,14),Vector2.zero),Ink,false);
        var slider=sliderRoot.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=handleImage;
        slider.minValue=0;slider.maxValue=1;slider.value=AudioListener.volume;SetVolume(slider.value);
        slider.onValueChanged.AddListener(SetVolume);
        TextButton("RESUME",card,new Vector2(0,-33),()=>SetPaused(false),true);
        TextButton("RESTART",card,new Vector2(0,-116),()=>Load(SceneManager.GetActiveScene().name));
        TextButton("QUIT",card,new Vector2(0,-199),()=>Load("MainMenu"));
        Label("Quit returns to the main menu",card,new Vector2(.5f,.5f),new Vector2(440,30),new Vector2(0,-263),18,Muted);
        modal.SetActive(false);
    }
    public void ShowVictory(string destination, bool finalVictory, float duration = .65f)
    {
        if (menu || victoryShown) return;
        victoryShown = true;
        paused = false;
        BlocksGameplay = true;
        if (player != null) player.ClearInput();
        Time.timeScale = 0;
        AudioListener.pause = true;
        modal.SetActive(false); controls.SetActive(false); hud.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);

        var overlay = Rect("Victory Overlay",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        Surface(overlay,new Color(.02f,.01f,.055f,.88f),false).raycastTarget=true;
        var overlayGroup=overlay.gameObject.AddComponent<CanvasGroup>();
        var safeVictory=Rect("Victory Safe Area",overlay,safe.anchorMin,safe.anchorMax,Vector2.zero,Vector2.zero);
        var card=At("Victory Card",safeVictory,new Vector2(.5f,.5f),new Vector2(580,600),Vector2.zero);
        Surface(card,Panel);Frame(card);
        var emblem=At("Victory Emblem",card,new Vector2(.5f,.5f),new Vector2(110,110),new Vector2(0,194));
        Surface(emblem,new Color(.20f,.15f,.24f));
        Icon(GameIcon.Symbol.Crown,emblem,new Vector2(72,72),Vector2.zero,Gold);
        var sparks=new RectTransform[8];
        for(int i=0;i<sparks.Length;i++) {
            float angle=(i*45+22.5f)*Mathf.Deg2Rad;
            Vector2 pos=new Vector2(Mathf.Cos(angle)*91,194+Mathf.Sin(angle)*69);
            sparks[i]=Icon(GameIcon.Symbol.Diamond,card,new Vector2(i%2==0?9:5,i%2==0?9:5),pos,Gold).rectTransform;
        }
        var heading=Rect("Victory Heading",card,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<CanvasGroup>();
        Label("VICTORY",heading.transform,new Vector2(.5f,.5f),new Vector2(500,76),new Vector2(0,94),52,finalVictory?Gold:White);
        var message=Rect("Victory Details",card,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<CanvasGroup>();
        string scene=SceneManager.GetActiveScene().name;
        int stage=finalVictory?3:scene=="Level2"?2:1;
        Label(finalVictory?"THE BOSS HAS FALLEN":"LEVEL 0"+stage+" COMPLETE",message.transform,new Vector2(.5f,.5f),new Vector2(520,30),new Vector2(0,39),20,Gold);
        Rule(message.transform,440,3);
        for(int i=0;i<3;i++) {
            float x=(i-1)*100;
            if(i<2)Surface(At("Journey link",message.transform,new Vector2(.5f,.5f),new Vector2(68,2),new Vector2(x+50,-39)),i<stage-1?Gold:new Color(.33f,.29f,.40f),false);
            Icon(GameIcon.Symbol.Diamond,message.transform,new Vector2(18,18),new Vector2(x,-39),i<stage?Gold:new Color(.33f,.29f,.40f));
            Label(i==2?"BOSS":"0"+(i+1),message.transform,new Vector2(.5f,.5f),new Vector2(80,28),new Vector2(x,-70),18,i<stage?White:Muted);
        }
        var actions=Rect("Victory Actions",card,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject.AddComponent<CanvasGroup>();
        TextButton(finalVictory?"PLAY AGAIN":"NEXT LEVEL",actions.transform,new Vector2(0,-146),()=>Load(destination),true);
        Label(finalVictory?"Start a new journey from Level 1":"The next portal is waiting.",actions.transform,new Vector2(.5f,.5f),new Vector2(520,32),new Vector2(0,-211),18,Muted);
        var reveal=overlay.gameObject.AddComponent<VictoryReveal>();
        reveal.overlay=overlayGroup;reveal.card=card;reveal.emblem=emblem;
        reveal.heading=heading;reveal.message=message;reveal.actions=actions;reveal.sparks=sparks;
        reveal.duration=duration;reveal.Play();
    }
    void SetVolume(float value) { AudioListener.volume=value;PlayerPrefs.SetFloat("MasterVolume",value);if(volumeText!=null)volumeText.text=Mathf.RoundToInt(value*100)+"%"; }
    public void SetPaused(bool value)
    {
        if(menu || victoryShown || paused==value)return;
        paused=value;BlocksGameplay=value;
        if(player!=null)player.ClearInput();
        if(value){previousScale=Time.timeScale;Time.timeScale=0;}else Time.timeScale=previousScale;
        AudioListener.pause=value;modal.SetActive(value);controls.SetActive(!value);hud.SetActive(!value);
        if(!value)PlayerPrefs.Save();
        EventSystem.current?.SetSelectedGameObject(null);
    }
    void Update()
    {
        ApplySafeArea();
        if(menu)return;
        if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)SetPaused(!paused);
        if(health!=null)for(int i=0;i<hearts.Length;i++)hearts[i].color=i<health.CurrentHealth?Red:new Color(.23f,.28f,.34f);
        bool show=boss!=null && boss.isActiveAndEnabled && !boss.IsDead;
        bossPanel.SetActive(show);
        if(show){float fraction=boss.CurrentHealth/(float)Mathf.Max(1,boss.maxHealth);bossFill.rectTransform.anchorMax=new Vector2(fraction,1);bossText.text="BOSS   /   "+boss.CurrentHealth+" HP";}
    }
    void ApplySafeArea()
    {
        var area=Screen.safeArea;var size=new Vector2Int(Screen.width,Screen.height);
        if(area==lastSafe && size==lastSize)return;lastSafe=area;lastSize=size;
        if(size.x==0 || size.y==0)return;
        safe.anchorMin=new Vector2(area.xMin/size.x,area.yMin/size.y);safe.anchorMax=new Vector2(area.xMax/size.x,area.yMax/size.y);
    }
    void OnApplicationFocus(bool focus) { if(!focus && !menu && modal!=null)SetPaused(true); }
    void OnApplicationPause(bool value) { if(value && !menu && modal!=null)SetPaused(true); }
    void OnDestroy() { Time.timeScale=1;AudioListener.pause=false;BlocksGameplay=false; }
    void Load(string scene) { if(loadingScene || !Application.CanStreamedLevelBeLoaded(scene)) return; loadingScene=true; PlayerPrefs.Save();Time.timeScale=1;AudioListener.pause=false;BlocksGameplay=false;SceneManager.LoadScene(scene); }
    void QuitApplication()
    {
        PlayerPrefs.Save();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
        #elif UNITY_WEBGL
        // Browsers cannot close their own tab. Keep the menu usable.
        #else
        Application.Quit();
        #endif
    }
}


