using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Game
{

    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : Editor
    {

        public static string Clipboard
        {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        private bool showGeneralInfo = false;
        private bool showModelInfo = false;
        private bool showLootInfo = false;

        public override void OnInspectorGUI()
        {
            ItemData myTarget = (ItemData)target;


            showGeneralInfo = EditorGUILayout.Foldout(showGeneralInfo, "General Info");
            if (showGeneralInfo)
            {
                myTarget.Title = EditorGUILayout.TextField("Title", myTarget.Title);

                EditorGUILayout.PrefixLabel("Description");
                myTarget.Description = EditorGUILayout.TextArea(myTarget.Description, GUILayout.Height(50));

                myTarget.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", myTarget.Icon, typeof(Sprite), allowSceneObjects: true);
                myTarget.ItemType = (ItemType)EditorGUILayout.EnumMaskPopup("Type", myTarget.ItemType);
                EditorGUILayout.Space();
            }
            showModelInfo = EditorGUILayout.Foldout(showModelInfo, "Model Info");
            if (showModelInfo)
            {
                myTarget.Prefab = (GameObject)EditorGUILayout.ObjectField("Model", myTarget.Prefab, typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.Space();
            }
            showLootInfo = EditorGUILayout.Foldout(showLootInfo, "Loot Info");
            if (showLootInfo)
            {
                EditorGUILayout.MinMaxSlider("Spawnchance", ref myTarget.SpawnChance.min, ref myTarget.SpawnChance.max, 0, 100);
                myTarget.SpawnChance.min = Mathf.Floor(myTarget.SpawnChance.min);
                myTarget.SpawnChance.max = Mathf.Floor(myTarget.SpawnChance.max);
                EditorGUILayout.LabelField((myTarget.SpawnChance.max - myTarget.SpawnChance.min).ToString("0") + "%");

                myTarget.LootType = (LootType)EditorGUILayout.EnumMaskPopup("Loot Type", myTarget.LootType);

                int[] test = (int[])Enum.GetValues(typeof(LootType));

                foreach (int lt in Enum.GetValues(typeof(LootType)))
                {
                    if (((int)myTarget.LootType & lt) == lt && lt != 0)
                    {
                        EditorGUILayout.LabelField(" - " + Enum.GetName(typeof(LootType), lt));
                    }
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.MinMaxSlider("Weight", ref myTarget.WeightDry, ref myTarget.WeightWet, 0, 25);
            myTarget.WeightDry = Mathf.Floor(myTarget.WeightDry * 4) / 4;
            myTarget.WeightWet = Mathf.Floor(myTarget.WeightWet * 4) / 4;
            EditorGUILayout.LabelField(myTarget.WeightDry.ToString("0.00") + "kg Dry - " + myTarget.WeightWet.ToString("0.00") + "kg Wet");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy JSON"))
            {
                string json = JsonUtility.ToJson(myTarget);
                Clipboard = json;
                Debug.Log("JSON " + json);
            }

            if (GUILayout.Button("Paste JSON"))
            {
                try
                {
                    string json = Clipboard;
                    JsonUtility.FromJsonOverwrite(json, myTarget);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}