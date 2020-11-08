using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;


public class ResolutionControlTool : EditorWindow
{
    private static IEnumerator _iEnumerator = null;
    [MenuItem("ResolutionControlTool/Open")]
    public static void ShowExample()
    {
        ResolutionControlTool wnd = GetWindow<ResolutionControlTool>();
        wnd.titleContent = new GUIContent("ResolutionControlTool");
        EditorApplication.update += Update;
    }
    private ObjectField presetObjectField;
    private CameraOutputControl cameraOutputControl;
    private SerializedObject cameraOutputControlSerialized;
    private VisualElement root;
    private VisualElement buttons;
    public void OnEnable()
    {
        EditorApplication.update += Update;
        root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ResolutionControlTool/Editor/ResolutionControlTool.uxml");
        VisualElement labelFromUXML = visualTree.CloneTree();
        root.Add(labelFromUXML);


        buttons = root.Q<VisualElement>("buttons");

        presetObjectField = root.Q<ObjectField>("outputField");
        presetObjectField.objectType = typeof(CameraOutputControl);

      
        presetObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
        {
            if (presetObjectField.value != null)
            {
                cameraOutputControl = (CameraOutputControl) presetObjectField.value;
                cameraOutputControlSerialized = new SerializedObject(cameraOutputControl);

            }
            else
            {
                cameraOutputControl = null;
            }

            PopulatePresetList();

        });



        root.Q<Button>("search").clicked += Search;
        _iEnumerator = Init();
    }
  
    public static void Open (){
        EditorApplication.update += Update;
    }
    private static void Update(){
        if(_iEnumerator != null){
            //コルーチンを実行
            _iEnumerator.MoveNext();
        }
    }
    private void Search()
    {
        presetObjectField.value = null;
        // Debug.Log("search");
        
        var output = GameObject.FindObjectsOfType<CameraOutputControl>();
        // Debug.Log(output.Length);
        if (output.Length == 1)
        {
            root.Q<Label>("warning").text = "";
            presetObjectField.value = output.First();
            // 
        }
        else
        {
            presetObjectField.value = null;
            root.Q<Label>("warning").text = "CameraOutputControlをhierarchyからドロップシてください";
        }
        
        
    }
    
    

    private void ClearButtons()
    {
        while (buttons.childCount > 0)
        {
            buttons.RemoveAt(0);
        }
    }
    
    public IEnumerator Init()
    {
        // 初期化タイミングにずれがあるので、ちょっとだけ待ってから初期化処理する。
        double timeSinceStartup = EditorApplication.timeSinceStartup;
        
        while (EditorApplication.timeSinceStartup - timeSinceStartup < 0.3f) {
            yield return 0;
        }
        EditorApplication.QueuePlayerLoopUpdate();
        
        Search();
        _iEnumerator = null;
    }
    public void PopulatePresetList()
    {
        ClearButtons();
        if (cameraOutputControl != null && cameraOutputControl.outputTextures != null)
        {
            foreach (var tex in cameraOutputControl.outputTextures)
            {
                var button = new Button();
                button.text = tex.name;
                button.style.height = 30;
                buttons.Add(button);

                button.clicked += (() =>
                {
                    cameraOutputControl.GetComponent<RawImage>().texture = tex;
                    if(!GameViewSizeHelper.Contains(GameViewSizeGroupType.Standalone, GameViewSizeHelper.GameViewSizeType.FixedResolution, tex.width, tex.height, tex.name)){
                        AddGameViewSize(tex.width, tex.height, tex.name);
                    }

                    GameViewSizeHelper.ChangeGameViewSize(GameViewSizeGroupType.Standalone, GameViewSizeHelper.GameViewSizeType.FixedResolution, tex.width, tex.height, tex.name);

                });
                // 
            }
        }
        // _iEnumerator = null;
    }
    
    private static void AddGameViewSize(int width,int height,string name){
        GameViewSizeHelper.AddCustomSize(GameViewSizeGroupType.Standalone, GameViewSizeHelper.GameViewSizeType.FixedResolution, width, height, name);
    }

}