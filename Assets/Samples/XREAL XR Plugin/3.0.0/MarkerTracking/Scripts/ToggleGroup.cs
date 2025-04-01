using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    public class ToggleGroup<T>
    {
        #region events
        public event Action<ToggleGroup<T>, int/*last*/, int/*new*/> OnIndexChanged;
        #endregion

        public Dictionary<int, T> m_IndexDataDict;
        public int m_Index = -1;
        public ToggleGroup()
        {
            m_IndexDataDict = new Dictionary<int, T>();
        }
        public void Add(int index, T trackableImage)
        {
            m_IndexDataDict.Add(index, trackableImage);
        }
        public void SetActiveByIndex(int index)
        {
            int oldIndex = m_Index;
            m_Index = index;
            OnIndexChanged?.Invoke(this, oldIndex, m_Index);
        }
        public bool TryGetData(int index, out T data)
        {
            return m_IndexDataDict.TryGetValue(index, out data);
        }

        public void OnGUI()
        {
            foreach (var kv in m_IndexDataDict)
            {
                int index = kv.Key;
                var normalColor = GUI.backgroundColor;
                if (m_Index == index)
                {
                    GUI.color = Color.green;
                }
                else
                {
                    GUI.color = normalColor;
                }

                if (GUILayout.Button($"{index}", GUILayout.Width(50), GUILayout.Height(50)))
                {
                    SetActiveByIndex(index);
                }
                GUI.backgroundColor = normalColor;
            }
        }
    }
}
