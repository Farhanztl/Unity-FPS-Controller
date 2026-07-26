using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// ???????? ?????? Unity
/// 1. ?? Empty GameObject ?? ???? ????
/// 2. ??? ??????? ?? ??? ????? ??
/// 3. ?? Inspector ??? ???? "Build Castle Now" ???? ??
/// </summary>
public class CastleBuilder : MonoBehaviour
{
    [Header("Castle Settings")]
    [Tooltip("??? true ????? ???? ???? ????? ???? ????? ?????")]
    public bool buildOnStart = false;

    [Header("Materials - ???????")]
    public Material stoneMaterial;
    public Material darkStoneMaterial;
    public Material roofMaterial;
    public Material woodMaterial;
    public Material flagMaterial;
    public Material groundMaterial;

    private Material _stone;
    private Material _darkStone;
    private Material _roof;
    private Material _wood;
    private Material _flag;
    private Material _ground;

    void Start()
    {
        if (buildOnStart) BuildCastle();
    }

    public void BuildCastle()
    {
        DestroyPreviousCastle();
        SetupMaterials();

        GameObject castleRoot = new GameObject("Castle");
        castleRoot.transform.SetParent(this.transform);
        castleRoot.transform.localPosition = Vector3.zero;

        // ????
        CreateCube("Ground", castleRoot, new Vector3(0, -0.5f, 0), new Vector3(36, 1, 36), _ground);

        // ???????? ????
        CreateCube("Wall_Front", castleRoot, new Vector3(0,    2.5f, -10f), new Vector3(20, 6, 1.2f), _stone);
        CreateCube("Wall_Back",  castleRoot, new Vector3(0,    2.5f,  10f), new Vector3(20, 6, 1.2f), _stone);
        CreateCube("Wall_Left",  castleRoot, new Vector3(-10f, 2.5f,   0f), new Vector3(1.2f, 6, 20), _stone);
        CreateCube("Wall_Right", castleRoot, new Vector3( 10f, 2.5f,   0f), new Vector3(1.2f, 6, 20), _stone);

        // ?????????? ?????
        AddBattlements("BF", castleRoot, new Vector3(-8.5f, 5.9f, -10f), 9, Vector3.right   * 2f);
        AddBattlements("BB", castleRoot, new Vector3(-8.5f, 5.9f,  10f), 9, Vector3.right   * 2f);
        AddBattlements("BL", castleRoot, new Vector3(-10f,  5.9f, -8.5f),9, Vector3.forward * 2f);
        AddBattlements("BR", castleRoot, new Vector3( 10f,  5.9f, -8.5f),9, Vector3.forward * 2f);

        // ??????? ????
        BuildCornerTower("Tower_FL", castleRoot, new Vector3(-10f, 0, -10f));
        BuildCornerTower("Tower_FR", castleRoot, new Vector3( 10f, 0, -10f));
        BuildCornerTower("Tower_BL", castleRoot, new Vector3(-10f, 0,  10f));
        BuildCornerTower("Tower_BR", castleRoot, new Vector3( 10f, 0,  10f));

        // Keep ?????
        BuildKeep(castleRoot, Vector3.zero);

        // ?????? ?????
        BuildGate(castleRoot, new Vector3(0, 0, -10f));

        // ???????? ??????
        BuildGateTowers(castleRoot, new Vector3(0, 0, -12f));

        Debug.Log("[CastleBuilder] ???? ?? ?????? ????? ??!");

#if UNITY_EDITOR
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#endif
    }

    void BuildCornerTower(string tName, GameObject parent, Vector3 pos)
    {
        GameObject t = new GameObject(tName);
        t.transform.SetParent(parent.transform);
        t.transform.localPosition = Vector3.zero;

        CreateCylinder(tName+"_Body",  t, pos+new Vector3(0, 4.5f,  0), new Vector3(2.6f,5.0f,2.6f), _stone);
        CreateCube    (tName+"_Belt",  t, pos+new Vector3(0, 9.7f,  0), new Vector3(3.4f,0.7f,3.4f), _darkStone);
        CreateCylinder(tName+"_Spire", t, pos+new Vector3(0, 11.5f, 0), new Vector3(1.8f,2.2f,1.8f), _roof);

        Vector3[] batt = {
            pos+new Vector3(-1.5f,10.5f,-1.5f),
            pos+new Vector3( 1.5f,10.5f,-1.5f),
            pos+new Vector3(-1.5f,10.5f, 1.5f),
            pos+new Vector3( 1.5f,10.5f, 1.5f)
        };
        for(int i=0;i<4;i++)
            CreateCube(tName+"_Batt"+i, t, batt[i], new Vector3(0.8f,1f,0.8f), _darkStone);

        CreateCube(tName+"_Pole", t, pos+new Vector3(0,    14.5f, 0),  new Vector3(0.13f,2.2f,0.13f), _wood);
        CreateCube(tName+"_Flag", t, pos+new Vector3(0.75f,15.3f, 0),  new Vector3(1.5f,0.75f,0.06f), _flag);
    }

    void BuildKeep(GameObject parent, Vector3 pos)
    {
        GameObject keep = new GameObject("Keep");
        keep.transform.SetParent(parent.transform);
        keep.transform.localPosition = Vector3.zero;

        CreateCube("Keep_Platform", keep, pos+new Vector3(0,-0.4f,0),   new Vector3(9,0.8f,9),      _stone);
        CreateCube("Keep_Base",     keep, pos+new Vector3(0, 4.5f,0),   new Vector3(8,10,8),         _darkStone);
        CreateCube("Keep_Mid",      keep, pos+new Vector3(0, 10f, 0),   new Vector3(7,2,7),          _darkStone);
        CreateCube("Keep_Crown",    keep, pos+new Vector3(0, 11.6f,0),  new Vector3(9.5f,0.9f,9.5f), _stone);

        AddBattlements("KN", keep, new Vector3(-3.5f,12.7f, 4.8f), 4, Vector3.right *2.2f);
        AddBattlements("KS", keep, new Vector3(-3.5f,12.7f,-4.8f), 4, Vector3.right *2.2f);
        AddBattlements("KW", keep, new Vector3(-4.8f,12.7f, 3.5f), 4, Vector3.back  *2.2f);
        AddBattlements("KE", keep, new Vector3( 4.8f,12.7f, 3.5f), 4, Vector3.back  *2.2f);

        CreateCylinder("Keep_Spire", keep, pos+new Vector3(0,15.5f,0), new Vector3(3.2f,4.5f,3.2f), _roof);
        CreateCube("Keep_Pole", keep, pos+new Vector3(0,   21f,0),  new Vector3(0.2f,3.5f,0.2f), _wood);
        CreateCube("Keep_Flag", keep, pos+new Vector3(1.1f,22.2f,0), new Vector3(2.2f,1.1f,0.08f),_flag);

        float wy = pos.y+6f;
        CreateCube("Win_N", keep, new Vector3( 0,   wy, 4.1f), new Vector3(1.4f,2f,0.3f), _darkStone);
        CreateCube("Win_S", keep, new Vector3( 0,   wy,-4.1f), new Vector3(1.4f,2f,0.3f), _darkStone);
        CreateCube("Win_E", keep, new Vector3( 4.1f,wy, 0),    new Vector3(0.3f,2f,1.4f), _darkStone);
        CreateCube("Win_W", keep, new Vector3(-4.1f,wy, 0),    new Vector3(0.3f,2f,1.4f), _darkStone);
    }

    void BuildGate(GameObject parent, Vector3 wallPos)
    {
        GameObject gate = new GameObject("Gate");
        gate.transform.SetParent(parent.transform);
        gate.transform.localPosition = Vector3.zero;

        CreateCube("Gate_PillarL", gate, wallPos+new Vector3(-1.9f,2.5f,0),     new Vector3(1f,5.5f,1.5f),   _darkStone);
        CreateCube("Gate_PillarR", gate, wallPos+new Vector3( 1.9f,2.5f,0),     new Vector3(1f,5.5f,1.5f),   _darkStone);
        CreateCube("Gate_Lintel",  gate, wallPos+new Vector3(0,5.5f,0),          new Vector3(4.8f,1f,1.5f),   _darkStone);
        CreateCube("Gate_ArchDec", gate, wallPos+new Vector3(0,6.3f,0),          new Vector3(3f,0.6f,1.5f),   _stone);
        CreateCube("Gate_DoorL",   gate, wallPos+new Vector3(-0.65f,2.2f,0.1f), new Vector3(1.2f,3.8f,0.15f),_wood);
        CreateCube("Gate_DoorR",   gate, wallPos+new Vector3( 0.65f,2.2f,0.1f), new Vector3(1.2f,3.8f,0.15f),_wood);
    }

    void BuildGateTowers(GameObject parent, Vector3 pos)
    {
        GameObject gh = new GameObject("Gatehouse");
        gh.transform.SetParent(parent.transform);
        gh.transform.localPosition = Vector3.zero;

        CreateCylinder("GH_BodyL",  gh, pos+new Vector3(-3.5f, 4f,   0), new Vector3(2.2f,4.5f,2.2f), _stone);
        CreateCube    ("GH_BeltL",  gh, pos+new Vector3(-3.5f, 8.6f, 0), new Vector3(3f,0.6f,3f),     _darkStone);
        CreateCylinder("GH_SpireL", gh, pos+new Vector3(-3.5f, 10f,  0), new Vector3(1.3f,1.8f,1.3f), _roof);

        CreateCylinder("GH_BodyR",  gh, pos+new Vector3( 3.5f, 4f,   0), new Vector3(2.2f,4.5f,2.2f), _stone);
        CreateCube    ("GH_BeltR",  gh, pos+new Vector3( 3.5f, 8.6f, 0), new Vector3(3f,0.6f,3f),     _darkStone);
        CreateCylinder("GH_SpireR", gh, pos+new Vector3( 3.5f, 10f,  0), new Vector3(1.3f,1.8f,1.3f), _roof);

        CreateCube("GH_Bridge", gh, pos+new Vector3(0,8.6f,0), new Vector3(4f,0.6f,2.5f), _stone);
    }

    void AddBattlements(string prefix, GameObject parent, Vector3 start, int count, Vector3 step)
    {
        for(int i=0;i<count;i++)
            CreateCube("Batt_"+prefix+"_"+i, parent, start+step*i, new Vector3(0.85f,1f,0.85f), _stone);
    }

    GameObject CreateCube(string n, GameObject p, Vector3 lPos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = n;
        go.transform.SetParent(p.transform);
        go.transform.localPosition = lPos;
        go.transform.localScale    = scale;
        go.transform.localRotation = Quaternion.identity;
        ApplyMat(go, mat);
        return go;
    }

    GameObject CreateCylinder(string n, GameObject p, Vector3 lPos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = n;
        go.transform.SetParent(p.transform);
        go.transform.localPosition = lPos;
        go.transform.localScale    = scale;
        go.transform.localRotation = Quaternion.identity;
        ApplyMat(go, mat);
        return go;
    }

    void ApplyMat(GameObject go, Material mat)
    {
        if(mat==null) return;
        var r = go.GetComponent<Renderer>();
        if(r) r.sharedMaterial = mat;
    }

    void SetupMaterials()
    {
        _stone     = stoneMaterial     != null ? stoneMaterial     : MakeMat("Castle_Stone",     new Color(0.68f,0.62f,0.54f));
        _darkStone = darkStoneMaterial != null ? darkStoneMaterial : MakeMat("Castle_DarkStone", new Color(0.30f,0.26f,0.23f));
        _roof      = roofMaterial      != null ? roofMaterial      : MakeMat("Castle_Roof",      new Color(0.16f,0.12f,0.10f));
        _wood      = woodMaterial      != null ? woodMaterial      : MakeMat("Castle_Wood",      new Color(0.40f,0.26f,0.11f));
        _flag      = flagMaterial      != null ? flagMaterial      : MakeMat("Castle_Flag",      new Color(0.78f,0.08f,0.08f));
        _ground    = groundMaterial    != null ? groundMaterial    : MakeMat("Castle_Ground",    new Color(0.26f,0.44f,0.16f));
    }

    Material MakeMat(string n, Color c)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if(s==null || s.name.Contains("Error")) s = Shader.Find("Standard");
        if(s==null) s = Shader.Find("Diffuse");
        Material m = new Material(s);
        m.name  = n;
        m.color = c;
        return m;
    }

    void DestroyPreviousCastle()
    {
        Transform prev = transform.Find("Castle");
        if(prev==null) return;
#if UNITY_EDITOR
        DestroyImmediate(prev.gameObject);
#else
        Destroy(prev.gameObject);
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CastleBuilder))]
public class CastleBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(12);
        CastleBuilder b = (CastleBuilder)target;

        GUI.backgroundColor = new Color(0.25f,0.75f,0.35f);
        if(GUILayout.Button("Build Castle Now", GUILayout.Height(42)))
        {
            Undo.RegisterFullObjectHierarchyUndo(b.gameObject, "Build Castle");
            b.BuildCastle();
        }

        EditorGUILayout.Space(4);

        GUI.backgroundColor = new Color(0.85f,0.25f,0.25f);
        if(GUILayout.Button("Destroy Castle", GUILayout.Height(30)))
        {
            Transform castle = b.transform.Find("Castle");
            if(castle!=null) Undo.DestroyObjectImmediate(castle.gameObject);
        }

        GUI.backgroundColor = Color.white;
    }
}
#endif
