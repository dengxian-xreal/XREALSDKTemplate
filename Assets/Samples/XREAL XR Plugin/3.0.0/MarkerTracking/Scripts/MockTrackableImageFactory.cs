using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL.Samples
{
    public class MockTrackableImageFactory : SingletonMonoBehaviour<MockTrackableImageFactory>
    {

        public event Action<List<ARTrackedImage>, List<ARTrackedImage>> OnTrackablesChanged;

        public List<ARTrackedImage> m_TrackingImageList;
        public List<ToggleGroup<ARTrackedImage>> m_ToggleGroups;
        public static void GetTrackables(List<ARTrackedImage> trackingImageList)
        {
            trackingImageList.Clear();
            for (int i = 0; i < Singleton.m_TrackingImageList.Count; i++)
            {
                var item = Singleton.m_TrackingImageList[i];
                trackingImageList.Add(item);
            }

        }

        protected override void Awake()
        {
            base.Awake();
            m_ToggleGroups = new List<ToggleGroup<ARTrackedImage>>();
            m_TrackingImageList = new List<ARTrackedImage>();

            NewMarkerGroup(0, 1, new Vector3(0, 1f, 3));
            NewMarkerGroup(2, 4, new Vector3(0.3f, 1f, 2));
            NewMarkerGroup(5, 10, new Vector3(-0.3f, 1f, 2));
        }

        private void ToggleGroup_OnIndexChanged(ToggleGroup<ARTrackedImage> toggleGroup, int oldIndex, int newIndex)
        {
            List<ARTrackedImage> removed = new List<ARTrackedImage>();
            List<ARTrackedImage> added = new List<ARTrackedImage>();
            if (toggleGroup.TryGetData(oldIndex, out var oldImage))
            {
                removed.Add(oldImage);
                SetImageData(oldImage, new XRTrackedImage(
                    new TrackableId(0,(ulong)oldIndex),
                    Guid.Empty,
                    new Pose(oldImage.transform.position,oldImage.transform.rotation),
                    Vector2.one,
                    TrackingState.None,IntPtr.Zero));
            }
            if (toggleGroup.TryGetData(newIndex, out var newImage))
            {
                added.Add(newImage);
                SetImageData(newImage, new XRTrackedImage(
                    new TrackableId(0, (ulong)newIndex),
                    Guid.Empty,
                    new Pose(newImage.transform.position, newImage.transform.rotation),
                    Vector2.one,
                    TrackingState.Tracking, IntPtr.Zero));
            }
            OnTrackablesChanged?.Invoke(removed, added);
        }

        private ToggleGroup<ARTrackedImage> NewMarkerGroup(int startIndex, int endIndex, Vector3 mockPos)
        {
            ToggleGroup<ARTrackedImage> toggleGroup = new ToggleGroup<ARTrackedImage>();
            for (int i = startIndex; i <= endIndex; ++i)
            {
                var trackableImage = new GameObject($"mock_{i}", typeof(ARTrackedImage)).GetComponent<ARTrackedImage>();
                m_TrackingImageList.Add(trackableImage);
                trackableImage.transform.position = mockPos;
                toggleGroup.Add(i, trackableImage);
            }

            m_ToggleGroups.Add(toggleGroup);
            toggleGroup.OnIndexChanged += ToggleGroup_OnIndexChanged;

            return toggleGroup;
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < m_ToggleGroups.Count; ++i)
            {
                GUILayout.Space(50);
                var group = m_ToggleGroups[i];
                group.OnGUI();
            }
            GUILayout.EndHorizontal();
        }

        private void SetImageData(ARTrackedImage image, XRTrackedImage data)
        {
            var methodInfo = typeof(ARTrackedImage).GetMethod("SetSessionRelativeData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo?.Invoke(image, new object[] { data });
        }


    }
}
