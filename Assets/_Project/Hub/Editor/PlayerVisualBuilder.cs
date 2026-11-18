using UnityEditor;
using UnityEngine;

namespace MovingIn.Hub.EditorTools{
    public static class PlayerVisualBuilder
    {
        const string ArtFolder = "Assets/_Project/Hub/Art/Player/";
        const string FrontPath = ArtFolder + "Player_Front.png";
        const string BackPath = ArtFolder + "Player_Back.png";
        const string ShadowPath = ArtFolder + "Player_Shadow.png";
        const float PixelsPerUnit = 30f;

        [MenuItem("MovingIn/Setup Player Sprite")]
        static void Setup()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[PlayerVisualBuilder] Objeto 'Player' nao encontrado na cena aberta.");
                return;
            }

            if (!Import(FrontPath, SpriteAlignment.BottomCenter) |
                !Import(BackPath, SpriteAlignment.BottomCenter) |
                !Import(ShadowPath, SpriteAlignment.Center))
            {
                Debug.LogError($"[PlayerVisualBuilder] Coloque Player_Front.png, Player_Back.png e Player_Shadow.png em {ArtFolder} e rode de novo.");
                return;
            }

            Sprite front = AssetDatabase.LoadAssetAtPath<Sprite>(FrontPath);
            Sprite back = AssetDatabase.LoadAssetAtPath<Sprite>(BackPath);
            Sprite shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ShadowPath);

            Transform old = player.transform.Find("Visual");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            MeshRenderer capsule = player.GetComponent<MeshRenderer>();
            if (capsule != null)
            {
                Undo.RecordObject(capsule, "Setup Player Sprite");
                capsule.enabled = false;
            }

            GameObject visual = new GameObject("Visual");
            Undo.RegisterCreatedObjectUndo(visual, "Setup Player Sprite");
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, FeetY(player), 0f);
            Vector3 s = player.transform.lossyScale;
            visual.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);

            GameObject body = new GameObject("Body");
            body.transform.SetParent(visual.transform, false);
            SpriteRenderer bodyRenderer = body.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = front;
            bodyRenderer.sortingOrder = 1;

            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(visual.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = shadowSprite;
            shadowRenderer.color = new Color(0f, 0f, 0f, 0.3f);
            shadowRenderer.sortingOrder = 0;

            PlayerSpriteVisual pv = visual.AddComponent<PlayerSpriteVisual>();
            SerializedObject so = new SerializedObject(pv);
            so.FindProperty("target").objectReferenceValue = player.transform;
            so.FindProperty("body").objectReferenceValue = bodyRenderer;
            so.FindProperty("shadow").objectReferenceValue = shadow.transform;
            so.FindProperty("frontSprite").objectReferenceValue = front;
            so.FindProperty("backSprite").objectReferenceValue = back;
            so.FindProperty("cam").objectReferenceValue = Camera.main;
            so.ApplyModifiedPropertiesWithoutUndo();

            Camera cam = Camera.main;
            if (cam == null)
                Debug.LogWarning("[PlayerVisualBuilder] Nenhuma camera com a tag MainCamera. Ligue o campo Cam do Visual e coloque o PixelCamera na camera a mao.");
            else if (cam.GetComponent<PixelCamera>() == null)
                Undo.AddComponent<PixelCamera>(cam.gameObject);

            Selection.activeGameObject = visual;
            Debug.Log("[PlayerVisualBuilder] Sprite do garoto montado. Aperte Play e ande pela casa.");
        }

        static float FeetY(GameObject player)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) return cc.center.y - cc.height / 2f;
            CapsuleCollider col = player.GetComponent<CapsuleCollider>();
            if (col != null) return col.center.y - col.height / 2f;
            return 0f;
        }

        static bool Import(string path, SpriteAlignment alignment)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return false;

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = PixelsPerUnit;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;

            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)alignment;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);

            ti.SaveAndReimport();
            return true;
        }
    }
}
