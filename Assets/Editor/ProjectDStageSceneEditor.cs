using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageSceneDefinition))]
public sealed class ProjectDStageSceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var name = serializedObject.FindProperty("sceneName");
        string path = EditorBuildSettings.scenes.FirstOrDefault(s => Path.GetFileNameWithoutExtension(s.path) == name.stringValue)?.path;
        SceneAsset current = path != null ? AssetDatabase.LoadAssetAtPath<SceneAsset>(path) : null;
        SceneAsset next = (SceneAsset)EditorGUILayout.ObjectField("場景", current, typeof(SceneAsset), false);
        if (next != current) name.stringValue = next != null ? next.name : "";
        EditorGUILayout.PropertyField(serializedObject.FindProperty("category"), new GUIContent("場景分類"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("completion"), new GUIContent("開門條件"));
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.HelpBox("此資料只描述場景，不包含門類型、獎勵或機率；請在逐關設定的場景池調整權重。", MessageType.Info);
        if (!((StageSceneDefinition)target).Validate(out string error)) EditorGUILayout.HelpBox(error, MessageType.Error);
    }
}
