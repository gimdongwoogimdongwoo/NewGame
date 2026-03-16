using UnityEditor;
using UnityEngine;

public class PrefabCreator
{
    [MenuItem("Tools/Create Prefab From Selected")]
    static void CreatePrefab()
    {
        // 현재 선택된 GameObject 가져오기
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
        {
            Debug.LogWarning("Hierarchy에서 GameObject를 선택하세요.");
            return;
        }

        // Prefab 저장 경로 설정
        string path = "Assets/Prefabs/" + obj.name + ".prefab";

        // Prefabs 폴더가 없으면 생성
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Prefab 저장
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Debug.Log("Prefab 생성 완료: " + path);
    }
}