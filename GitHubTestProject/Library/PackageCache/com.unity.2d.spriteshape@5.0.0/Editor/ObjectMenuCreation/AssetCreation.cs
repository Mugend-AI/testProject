﻿using System;
using System.IO;
using System.Linq;
using UnityEditor.Presets;
using UnityEditor.U2D.Common;
using UnityEngine;

namespace UnityEditor.U2D.SpriteShape
{
    internal static class AssetCreation
    {
        const int k_AssetMenuPriority = 81;
        const int k_SpriteShapeAssetMenuPriority = k_AssetMenuPriority + 2;
        static internal Action<int, ProjectWindowCallback.EndNameEditAction, string, Texture2D, string> StartNewAssetNameEditingDelegate = ProjectWindowUtil.StartNameEditingIfProjectWindowExists;
        
        [MenuItem("Assets/Create/2D/Sprite Shape Profile", priority = k_SpriteShapeAssetMenuPriority)]
        static void MenuItem_AssetsCreate2DSpriteShapeProfile(MenuCommand menuCommand)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteShape>("Packages/com.unity.2d.spriteshape/Editor/ObjectMenuCreation/DefaultAssets/Sprite Shape Profiles/Sprite Shape Profile.asset");
            var preset = new PresetType(asset);
            var defaults = Preset.GetDefaultPresetsForType(preset).Count(x => x.enabled);
            if (defaults == 0)
                CreateAssetObject(asset);
            else
                CreateAssetObject<UnityEngine.U2D.SpriteShape>(null);
        }
        
        static public T CreateAssetObject<T>(T obj) where T : UnityEngine.Object
        {
            var assetSelectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var isFolder = false;
            if(!string.IsNullOrEmpty(assetSelectionPath))
                isFolder = File.GetAttributes(assetSelectionPath).HasFlag(FileAttributes.Directory);
            var path = InternalEditorBridge.GetProjectWindowActiveFolderPath();
            if (isFolder)
            {
                path = assetSelectionPath;
            }

            string resourceFile = "";
            string destName = "";
            int instanceId = 0;
            if (obj != null)
            {
                resourceFile = AssetDatabase.GetAssetPath(obj);
                var fileName = System.IO.Path.GetFileName(resourceFile);
                destName = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(path, fileName));
            }
            else
            {
                var asset = Activator.CreateInstance<T>();
                instanceId = asset.GetInstanceID();
                destName = "Sprite Shape Profile.asset";
            }
            var icon = InternalEditorBridge.GetIconContent<T>().image as Texture2D;
            StartNewAssetNameEditing(resourceFile, destName, icon, instanceId);
            return Selection.activeObject as T;
        }
        
        static private void StartNewAssetNameEditing(string source, string dest, Texture2D icon, int instanceId)
        {
            CreateAssetEndNameEditAction action = ScriptableObject.CreateInstance<CreateAssetEndNameEditAction>();
            StartNewAssetNameEditingDelegate(instanceId, action, dest, icon, source);
        }

        internal class CreateAssetEndNameEditAction : ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var uniqueName = AssetDatabase.GenerateUniqueAssetPath(pathName);
                if (instanceId == 0 && !string.IsNullOrEmpty(resourceFile))
                {
                    AssetDatabase.CopyAsset(resourceFile, uniqueName);
                }
                else
                {
                    var obj = EditorUtility.InstanceIDToObject(instanceId);
                    AssetDatabase.CreateAsset(obj, uniqueName);
                }
            }
        }
    }
}
