﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommon;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// A default editor and project settings provider for <see cref="Naninovel.Configuration"/> assets.
    /// </summary>
    public class ConfigurationSettings : SettingsProvider
    {
        /// <summary>
        /// Relative (to the application data directory) path to store the automatically generated package assets.
        /// </summary>
        public static string GeneratedDataPath
        {
            get => PlayerPrefs.GetString(editorDataPathKey, EngineConfiguration.DefaultGeneratedDataPath);
            protected set => PlayerPrefs.SetString(editorDataPathKey, value);
        }

        protected Type ConfigurationType { get; }
        protected Configuration Configuration { get; private set; }
        protected SerializedObject SerializedObject { get; private set; }
        protected virtual string EditorTitle { get; }
        protected virtual string HelpUri { get; }
        /// <summary>
        /// Override to use custom drawers instead of the default ones in <see cref="DrawDefaultEditor()"/>.
        /// </summary>
        protected virtual Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers { get; }

        private const string editorDataPathKey = "Naninovel." + nameof(ConfigurationSettings) + "." + nameof(GeneratedDataPath);
        private const string settingsPathPrefix = "Project/Naninovel/";
        private static readonly GUIContent helpIcon = new GUIContent(Resources.Load<Texture2D>("Naninovel/HelpIcon"), "Open naninovel guide in web browser.");
        private static readonly Type settingsScopeType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SettingsWindow+GUIScope");
        private static readonly Dictionary<Type, Type> settingsTypeMap = BuildSettingsTypeMap();

        private Dictionary<string, Action<SerializedProperty>> overrideDrawers;

        protected ConfigurationSettings (Type configType) 
            : base(TypeToSettingsPath(configType), SettingsScope.Project)
        {
            Debug.Assert(typeof(Configuration).IsAssignableFrom(configType));
            ConfigurationType = configType;
            EditorTitle = ConfigurationType.Name.Replace("Configuration", string.Empty).InsertCamel();
            HelpUri = $"guide/configuration.html#{ConfigurationType.Name.Replace("Configuration", string.Empty).InsertCamel('-').ToLowerInvariant()}";
            overrideDrawers = OverrideConfigurationDrawers;
        }

        public override void OnActivate (string searchContext, VisualElement rootElement)
        {
            Configuration = Configuration.LoadOrDefault(ConfigurationType);
            SerializedObject = new SerializedObject(Configuration);
            keywords = GetSearchKeywordsFromSerializedObject(SerializedObject);

            // Save the asset in case it was just generated.
            if (!AssetDatabase.Contains(Configuration))
                SaveConfigurationObject();
        }

        public override void OnTitleBarGUI ()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button(helpIcon, GUIStyles.IconButton))
                Application.OpenURL($"https://naninovel.com/{HelpUri}");
        }

        public override void OnGUI (string searchContext)
        {
            if (SerializedObject is null || !ObjectUtils.IsValid(SerializedObject.targetObject))
            {
                EditorGUILayout.HelpBox($"{EditorTitle} configuration asset has been deleted or moved. Try re-opening the settings window or restarting the Unity editor.", MessageType.Error);
                return;
            }

            using (Activator.CreateInstance(settingsScopeType) as IDisposable)
            {
                SerializedObject.Update();
                DrawConfigurationEditor();
                SerializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Override this method for custom configuration editors.
        /// </summary>
        protected virtual void DrawConfigurationEditor ()
        {
            DrawDefaultEditor();
        }

        /// <summary>
        /// Draws a default editor for each serializable property of the configuration object.
        /// Will skip "m_Script" property and use <see cref="OverrideConfigurationDrawers"/> instead of the default drawers.
        /// </summary>
        protected void DrawDefaultEditor ()
        {
            var property = SerializedObject.GetIterator();
            var enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath == "m_Script") continue;
                if (overrideDrawers != null && overrideDrawers.ContainsKey(property.propertyPath))
                {
                    overrideDrawers[property.propertyPath]?.Invoke(property);
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }
        }

        protected static string TypeToSettingsPath (Type type)
        {
            return settingsPathPrefix + type.Name.Replace("Configuration", string.Empty).InsertCamel();
        }

        private void SaveConfigurationObject ()
        {
            var dirPath = PathUtils.Combine(Application.dataPath, $"{GeneratedDataPath}/Resources/{Configuration.ResourcesPath}");
            var assetPath = PathUtils.AbsoluteToAssetPath(PathUtils.Combine(dirPath, $"{ConfigurationType.Name}.asset"));
            System.IO.Directory.CreateDirectory(dirPath);
            AssetDatabase.CreateAsset(Configuration, assetPath);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Builds a <see cref="Naninovel.Configuration"/> to <see cref="ConfigurationSettings"/> types map based on the available implementations in the project. 
        /// When a <see cref="ConfigurationSettings{TConfig}"/> for a configuration is found, will map it, otherwise will use a base <see cref="ConfigurationSettings"/>.
        /// </summary>
        private static Dictionary<Type, Type> BuildSettingsTypeMap ()
        {
            bool IsEditorFor (Type editorType, Type configType)
            {
                var type = editorType.BaseType;
                while (type != null)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConfigurationSettings<>) && type.GetGenericArguments()[0] == configType)
                        return true;
                    type = type.BaseType;
                }
                return false;
            }

            var configTypes = ReflectionUtils.ExportedDomainTypes.Where(t => t.IsSubclassOf(typeof(Configuration)) && !t.IsAbstract);
            var editorTypes = ReflectionUtils.ExportedDomainTypes.Where(t => t.IsSubclassOf(typeof(ConfigurationSettings)) && !t.IsAbstract);
            var typeMap = new Dictionary<Type, Type>();
            foreach (var configType in configTypes)
            {
                var editorType = editorTypes.FirstOrDefault(t => IsEditorFor(t, configType)) ?? typeof(ConfigurationSettings);
                typeMap.Add(configType, editorType);
            }

            return typeMap;
        }

        [SettingsProviderGroup]
        private static SettingsProvider[] CreateProviders ()
        {
            return settingsTypeMap
                .Select(kv => kv.Value == typeof(ConfigurationSettings) ? new ConfigurationSettings(kv.Key) : Activator.CreateInstance(kv.Value) as SettingsProvider).ToArray();
        }

        [MenuItem("Naninovel/Configuration", priority = 1)]
        private static void OpenWindow ()
        {
            var engineSettingsPath = TypeToSettingsPath(typeof(EngineConfiguration));
            SettingsService.OpenProjectSettings(engineSettingsPath);
        }
    }

    /// <summary>
    /// Derive from this class to create custom editors for <see cref="Naninovel.Configuration"/> assets.
    /// </summary>
    /// <typeparam name="TConfig">Type of the configuration asset this editor is built for.</typeparam>
    public abstract class ConfigurationSettings<TConfig> : ConfigurationSettings where TConfig : Configuration
    {
        protected new TConfig Configuration => base.Configuration as TConfig;
        protected static string SettingsPath => TypeToSettingsPath(typeof(TConfig));

        protected ConfigurationSettings ()
            : base(typeof(TConfig)) { }
    }
}
