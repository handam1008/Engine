using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ResponsiveAuthoring
{
    const string Folder = "Artifacts/YouTubeUI/Responsive";
    const string Request = Folder + "/apply.request";
    const string PrefabPath = "Assets/RYU/Prefab/Panel - YouTube.prefab";
    static ResponsiveAuthoring() { EditorApplication.delayCall += Run; }
    struct Bounds { public RectTransform rect; public Vector2 min, max; public bool horizontalOnly; }

    static void Run()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Run; return; }
        try
        {
            Directory.CreateDirectory(Folder);
            var scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/RYU/Scene/MainScene.unity") throw new Exception("MainScene must be open.");
            var canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)).First(c => c.name == "Canvas");
            var panel = canvas.transform.Find("Panel - YouTube") as RectTransform;
            if (!panel) throw new Exception("YouTube panel missing.");
            File.Copy(scene.path, Folder + "/MainScene.before.unity.txt", true);
            File.Copy(PrefabPath, Folder + "/Panel.before.prefab.txt", true);
            Apply(panel);
            VerifyAndCapture(panel);
            var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
            try { Apply(prefab.GetComponent<RectTransform>()); PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            File.WriteAllText(Folder + "/result.txt", "SUCCESS\nScene and prefab updated\n500x350, 1280x860, 1700x960, 1500x500, 1920x1020 checked\nTitle/browser/status follow window edges\nBody follows window width, preserves proportions and scrolls vertically when short\nRelated video scrolling preserved\nNo runtime source edits\nPreviews: minimum.png, default.png, large.png, wide.png, fullscreen.png");
            File.Delete(Request); Debug.Log("YouTube responsive layout verified at five sizes.");
        }
        catch (Exception e) { Directory.CreateDirectory(Folder); File.WriteAllText(Folder + "/error.txt", e.ToString()); File.Delete(Request); Debug.LogException(e); }
    }

    static RectTransform Find(Transform root, string path) { var t = root.Find(path) as RectTransform; if (!t) throw new Exception("Missing UI: " + path); return t; }
    static void Stretch(RectTransform r, Vector2 min, Vector2 max, Vector2 lower, Vector2 upper)
    { r.anchorMin = min; r.anchorMax = max; r.offsetMin = lower; r.offsetMax = upper; }
    static void Top(RectTransform r, float left, float right, float top, float height)
    { Stretch(r, new Vector2(0, 1), Vector2.one, new Vector2(left, -top-height), new Vector2(-right, -top)); }
    static void AutoFont(TMP_Text text)
    { text.fontSizeMax = text.fontSize; text.fontSizeMin = Mathf.Min(8, text.fontSize); text.enableAutoSizing = true; text.overflowMode = TextOverflowModes.Ellipsis; }

    static void Apply(RectTransform panel)
    {
        // Work at the original design dimensions, then restore the user's current window dimensions.
        var savedSize = panel.sizeDelta; var savedPosition = panel.anchoredPosition;
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 860);
        Canvas.ForceUpdateCanvases();
        var page = Find(panel, "Web Page");
        var bounds = new List<Bounds>();
        Snapshot(page, bounds, false);
        foreach (var b in bounds)
        {
            if (b.horizontalOnly)
            {
                var r = b.rect; var yMin = r.offsetMin.y; var yMax = r.offsetMax.y;
                var aMin = r.anchorMin; var aMax = r.anchorMax; aMin.x = b.min.x; aMax.x = b.max.x;
                Stretch(r, aMin, aMax, new Vector2(0,yMin), new Vector2(0,yMax));
            }
            else Stretch(b.rect, b.min, b.max, Vector2.zero, Vector2.zero);
        }
        foreach (var slider in page.GetComponentsInChildren<UnityEngine.UI.Slider>(true))
        {
            Stretch(Find(slider.transform,"Track"), new Vector2(0,.5f), new Vector2(1,.5f), new Vector2(0,-3), new Vector2(0,3));
            Stretch(Find(slider.transform,"Fill Area"), new Vector2(0,.5f), new Vector2(1,.5f), new Vector2(0,-3), new Vector2(0,3));
            Stretch(Find(slider.transform,"Handle Area"), Vector2.zero, Vector2.one, new Vector2(5,0), new Vector2(-5,0));
            slider.handleRect.sizeDelta = new Vector2(11,0); slider.handleRect.anchoredPosition = Vector2.zero;
        }
        foreach (var input in page.GetComponentsInChildren<TMP_InputField>(true)) SetupInput(input);
        foreach (var text in page.GetComponentsInChildren<TMP_Text>(true)) AutoFont(text);
        var scroll = page.GetComponentInChildren<ScrollRect>(true);
        Stretch(scroll.viewport, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-22,0));
        var bar = scroll.verticalScrollbar;
        Stretch(bar.GetComponent<RectTransform>(), new Vector2(1,0), Vector2.one, new Vector2(-18,0), Vector2.zero);
        Stretch(Find(bar.transform,"Sliding Area"), Vector2.zero, Vector2.one, Vector2.one, -Vector2.one);
        var title = Find(panel,"Title Bar"); Top(title,4,4,4,37);
        Top(Find(title,"Window Title"),40,120,2,32);
        AutoFont(Find(title,"Window Title").GetComponent<TMP_Text>());
        string[] controls = { "Minimize", "Maximize", "Close" }; float[] x = {-107,-74,-41};
        for(int i=0;i<3;i++) { var r=Find(title,controls[i]); r.anchorMin=r.anchorMax=Vector2.one; r.pivot=new Vector2(0,1); r.anchoredPosition=new Vector2(x[i],-4); r.sizeDelta=new Vector2(29,28); }
        var chrome=Find(panel,"Browser Chrome"); Top(chrome,4,4,41,123);
        Top(Find(chrome,"Menu Bar"),12,12,3,25); AutoFont(Find(chrome,"Menu Bar").GetComponent<TMP_Text>());
        Top(Find(chrome,"Menu Divider"),0,0,31,1);
        Top(Find(chrome,"Toolbar Labels"),355,12,41,28); AutoFont(Find(chrome,"Toolbar Labels").GetComponent<TMP_Text>());
        Top(Find(chrome,"Address Input"),87,104,85,28); SetupInput(Find(chrome,"Address Input").GetComponent<TMP_InputField>());
        var go=Find(chrome,"Go"); go.anchorMin=go.anchorMax=Vector2.one; go.pivot=new Vector2(0,1); go.anchoredPosition=new Vector2(-94,-84); go.sizeDelta=new Vector2(84,30);
        Stretch(page,Vector2.zero,Vector2.one,new Vector2(5,30),new Vector2(-5,-164));
        if(!page.GetComponent<RectMask2D>()) page.gameObject.AddComponent<RectMask2D>();
        if(!chrome.GetComponent<RectMask2D>()) chrome.gameObject.AddComponent<RectMask2D>();
        var status=Find(panel,"Status Bar"); Stretch(status,Vector2.zero,new Vector2(1,0),new Vector2(4,4),new Vector2(-4,30));
        Top(Find(status,"Status"),10,150,0,25);
        var connection=Find(status,"Connection"); connection.anchorMin=connection.anchorMax=Vector2.one; connection.pivot=new Vector2(1,1); connection.anchoredPosition=new Vector2(-12,0); connection.sizeDelta=new Vector2(130,25);
        connection.GetComponent<TMP_Text>().alignment=TextAlignmentOptions.MidlineRight;
        panel.sizeDelta=savedSize; panel.anchoredPosition=savedPosition;
        MakeBodyScrollable(page);
        EditorUtility.SetDirty(panel);
    }

    static RectTransform NewRect(string name,Transform parent)
    { var go=new GameObject(name,typeof(RectTransform));go.layer=parent.gameObject.layer;go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>(); }
    static void MakeBodyScrollable(RectTransform page)
    {
        if(page.Find("Page Viewport"))return;
        var children=page.Cast<Transform>().ToArray();
        var viewport=NewRect("Page Viewport",page);
        Stretch(viewport,Vector2.zero,Vector2.one,Vector2.zero,new Vector2(-20,0));viewport.gameObject.AddComponent<RectMask2D>();
        var content=NewRect("Page Content",viewport);
        content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=new Vector2(0,666);
        var ratio=content.gameObject.AddComponent<AspectRatioFitter>();ratio.aspectRatio=1270f/666f;ratio.aspectMode=AspectRatioFitter.AspectMode.WidthControlsHeight;
        foreach(var child in children)child.SetParent(content,false);
        var track=NewRect("Page Scrollbar",page);Stretch(track,new Vector2(1,0),Vector2.one,new Vector2(-17,0),Vector2.zero);
        var image=track.gameObject.AddComponent<Image>();image.color=new Color(.93f,.93f,.90f);image.raycastTarget=true;
        var area=NewRect("Sliding Area",track);Stretch(area,Vector2.zero,Vector2.one,Vector2.one,-Vector2.one);
        var handle=NewRect("Handle",area);Stretch(handle,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var grip=handle.gameObject.AddComponent<Image>();grip.color=new Color(.71f,.78f,.89f);grip.raycastTarget=true;
        var scrollbar=track.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=grip;scrollbar.direction=Scrollbar.Direction.BottomToTop;
        var scroll=page.gameObject.AddComponent<ScrollRect>();scroll.content=content;scroll.viewport=viewport;scroll.horizontal=false;scroll.vertical=true;
        scroll.verticalScrollbar=scrollbar;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
        page.GetComponent<Image>().raycastTarget=true;
    }

    static void SetupInput(TMP_InputField input)
    {
        Stretch(input.textViewport, Vector2.zero, Vector2.one, new Vector2(6,1),new Vector2(-6,-1));
        Stretch(input.textComponent.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
    }

    static void Snapshot(RectTransform parent,List<Bounds> list,bool widthOnly)
    {
        if(parent.GetComponent<UnityEngine.UI.Slider>() || parent.GetComponent<TMP_InputField>() || parent.GetComponent<Scrollbar>()) return;
        bool layout=parent.GetComponent<LayoutGroup>() != null;
        foreach(RectTransform child in parent)
        {
            if(child.name=="Content" && child.GetComponent<VerticalLayoutGroup>())
            {
                foreach(RectTransform row in child) Snapshot(row,list,true);
                continue;
            }
            if(!layout)
            {
                var corners=new Vector3[4]; child.GetWorldCorners(corners);
                var min=(Vector2)parent.InverseTransformPoint(corners[0]); var max=(Vector2)parent.InverseTransformPoint(corners[2]);
                list.Add(new Bounds {rect=child,min=(min-parent.rect.min)/parent.rect.size,max=(max-parent.rect.min)/parent.rect.size,horizontalOnly=widthOnly});
            }
            Snapshot(child,list,widthOnly);
        }
    }

    static void VerifyAndCapture(RectTransform panel)
    {
        Vector2[] sizes={new Vector2(500,350),new Vector2(1280,860),new Vector2(1700,960),new Vector2(1500,500),new Vector2(1920,1020)};
        string[] names={"minimum","default","large","wide","fullscreen"};
        for(int i=0;i<sizes.Length;i++) Capture(panel,sizes[i],names[i]);
    }
    static void Capture(RectTransform panel,Vector2 size,string name)
    {
        var canvasGo=new GameObject("Responsive Preview",typeof(RectTransform),typeof(Canvas)); canvasGo.hideFlags=HideFlags.HideAndDontSave;
        var cameraGo=new GameObject("Responsive Camera",typeof(Camera)); cameraGo.hideFlags=HideFlags.HideAndDontSave;
        RenderTexture rt=null; Texture2D image=null; var previous=RenderTexture.active;
        try
        {
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            var camera=cameraGo.GetComponent<Camera>();canvas.worldCamera=camera;
            var frame=size+new Vector2(80,80); canvasGo.GetComponent<RectTransform>().sizeDelta=frame;
            camera.transform.position=new Vector3(0,0,-1000);camera.orthographic=true;camera.orthographicSize=frame.y/2;
            camera.nearClipPlane=.3f;camera.farClipPlane=2000;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.23f,.42f,.65f);
            var clone=UnityEngine.Object.Instantiate(panel.gameObject,canvasGo.transform);clone.SetActive(true);
            var rect=clone.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=size;
            foreach(var t in canvasGo.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=31;
            Canvas.ForceUpdateCanvases();
            foreach(var scroll in clone.GetComponentsInChildren<ScrollRect>(true)){LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);scroll.verticalNormalizedPosition=1;scroll.StopMovement();}Canvas.ForceUpdateCanvases();
            var page=Find(rect,"Web Page");var title=Find(rect,"Title Bar");var chrome=Find(rect,"Browser Chrome");var status=Find(rect,"Status Bar");
            if(Mathf.Abs(title.rect.width-(size.x-8))>.1f || Mathf.Abs(chrome.rect.width-(size.x-8))>.1f || Mathf.Abs(status.rect.width-(size.x-8))>.1f || Mathf.Abs(page.rect.height-(size.y-194))>.1f)throw new Exception("Edge layout mismatch at "+size);
            var body=Find(page,"Page Viewport/Page Content");
            foreach(string path in new[]{"Video Player","Player Controls","Related Videos Scroll View","Header Rule"})
            {
                var r=Find(body,path);var corners=new Vector3[4];r.GetWorldCorners(corners);
                if(r.rect.width<=0 || r.rect.height<=0)throw new Exception("Non-positive size: "+path);
                foreach(var corner in corners){var local=(Vector2)body.InverseTransformPoint(corner);if(local.x<body.rect.xMin-.5f || local.x>body.rect.xMax+.5f || local.y<body.rect.yMin-.5f || local.y>body.rect.yMax+.5f)throw new Exception("Body overflow: "+path+" at "+size);}
            }
            rt=new RenderTexture((int)frame.x,(int)frame.y,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            image=new Texture2D((int)frame.x,(int)frame.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,frame.x,frame.y),0,0);image.Apply();File.WriteAllBytes(Folder+"/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=null;
        }
        finally{RenderTexture.active=previous;if(image)UnityEngine.Object.DestroyImmediate(image);if(rt){rt.Release();UnityEngine.Object.DestroyImmediate(rt);}UnityEngine.Object.DestroyImmediate(canvasGo);UnityEngine.Object.DestroyImmediate(cameraGo);}
    }
}
