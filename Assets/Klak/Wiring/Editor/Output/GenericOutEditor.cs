//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Klak.Wiring
{
    public class GenericOutEditor<T> : Editor
    {
        SerializedProperty _target;
        SerializedProperty _propertyName;
        SerializedProperty _particleSystemModuleName;

        // Component list cache and its parent game object
        string[] _componentList;
        GameObject _cachedGameObject;

        // Property list cache and its parent type
        string[] _propertyList;
        Type _cachedType;

        // Particle System Modules list
        string [] _moduleList;
        
        public virtual void OnEnable()
        {
            _target = serializedObject.FindProperty("_target");
            _propertyName = serializedObject.FindProperty("_propertyName");
            _particleSystemModuleName = serializedObject.FindProperty("_particleSystemModuleName");

            CacheModuleList();
        }

        void OnDisable()
        {
            _target = null;
            _propertyName = null;

            _componentList = null;
            _cachedGameObject = null;

            _propertyList = null;
            _cachedType = null;

            _moduleList = null;
            _particleSystemModuleName = null;
        }

        // Cache particle system modules with targetable properties
        void CacheModuleList()
        {
            if (_moduleList != null) return;

            var _fullModuleList = typeof(ParticleSystem).GetProperties().Where(x => x.PropertyType.ToString().Contains("Module")).ToArray();

            int _targetableModules = 0;
            List<int> _targetableIndices = new List<int>();

            for (int i = 0; i < _fullModuleList.Length; i++)
            {
                var _propList = _fullModuleList[i].PropertyType.GetProperties();
                bool _targetable = false;

                foreach (var prop in _propList)
                {
                    if (!_targetable && IsTargetable(prop))
                    {
                        _targetableModules++;
                        _targetableIndices.Add(i);
                        _targetable = true;
                    }
                }
            }
            _moduleList = new string[_targetableModules + 1];
            _moduleList[0] = "<none>";

            for (int i = 1; i <= _targetableIndices.Count; i++)
            {
                _moduleList[i] = _fullModuleList[_targetableIndices[i-1]].Name;
            }
        }

        // Cache component of a given game object if it's
        // different from a previously given game object.
        void CacheComponentList(GameObject gameObject)
        {
            if (_cachedGameObject == gameObject) return;

            _componentList = gameObject.GetComponents<Component>().
                Select(x => x.GetType().Name).ToArray();

            _cachedGameObject = gameObject;
        }

        // Check if a given property is capable of being a target.
        bool IsTargetable(PropertyInfo info)
        {
            return info.GetSetMethod() != null && info.PropertyType == typeof(T);
        }

        // Cache properties of a given type if it's
        // different from a previously given type.
        void CachePropertyList(Type type)
        {
            if (_cachedType == type) return;

            var _list = type.GetProperties().ToArray();

            _propertyList = type.GetProperties().
                Where(x => IsTargetable(x)).Select(x => x.Name).ToArray();

            _cachedType = type;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_target);

            // If the target is set...
            if (_target.objectReferenceValue != null)
            {
                var component = (Component)_target.objectReferenceValue;
                // Cache the component list of the target.
                CacheComponentList(component.gameObject);
                // Show the drop-down list.
                var index = Array.IndexOf(_componentList, component.GetType().Name);
                var newIndex = EditorGUILayout.Popup("Component", index, _componentList);
                // Update the component if the selection was changed.
                if (index != newIndex)
                    _target.objectReferenceValue = component.GetComponent(_componentList[newIndex]);
            

                if (component.GetType() == typeof(ParticleSystem))
                {
                    if (_moduleList.Length > 1)
                    {
                        index = Array.IndexOf(_moduleList, _particleSystemModuleName.stringValue);
                        newIndex = EditorGUILayout.Popup("Module", index, _moduleList);
                        // Update the module if the selection was changed.
                        if (index != newIndex)
                            _particleSystemModuleName.stringValue = _moduleList[newIndex];

                        if (!string.IsNullOrEmpty(_particleSystemModuleName.stringValue) && _particleSystemModuleName.stringValue != "<none>")
                        {
                            var moduleProp = typeof(ParticleSystem).GetProperty(_particleSystemModuleName.stringValue);
                            
                            // Cache the property list of the target module
                            CachePropertyList(moduleProp.PropertyType);
                            // If there are suitable candidates...
                            if (_propertyList.Length > 0)
                            {
                                // Show the drop-down list.
                                index = Array.IndexOf(_propertyList, _propertyName.stringValue);
                                newIndex = EditorGUILayout.Popup("Property", index, _propertyList);
                                // Update the property if the selection was changed.
                                if (index != newIndex)
                                _propertyName.stringValue = _propertyList[newIndex];
                            }   
                            
                            else
                                _propertyName.stringValue = ""; // reset on failure
                        }
                    }
                }
                
                if (component.GetType() != typeof(ParticleSystem) || _particleSystemModuleName == null || _particleSystemModuleName.stringValue == "<none>")
                {
                    // Cache the property list of the target.
                    CachePropertyList(_target.objectReferenceValue.GetType());
                    // If there are suitable candidates...
                    if (_propertyList.Length > 0)
                    {
                        // Show the drop-down list.
                        index = Array.IndexOf(_propertyList, _propertyName.stringValue);
                        newIndex = EditorGUILayout.Popup("Property", index, _propertyList);
                        // Update the property if the selection was changed.
                        if (index != newIndex)
                        _propertyName.stringValue = _propertyList[newIndex];
                    }   
                    else
                        _propertyName.stringValue = ""; // reset on failure
                }
            }
            else
                _propertyName.stringValue = ""; // reset on failure

            serializedObject.ApplyModifiedProperties();
        }      
    }
}
