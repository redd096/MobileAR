using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

//AR Foundation ImageTracking
//https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.2/manual/features/image-tracking.html
//Google ARCore ImageTracking
//https://docs.unity3d.com/Packages/com.unity.xr.arcore@6.2/manual/features/image-tracking.html

namespace redd096
{
    /// <summary>
    /// Basic manager with events when track images, and a check to know if ImageTracking is supported. Plus some helper APIs
    /// </summary>
    public class ImageTrackingManager : MonoBehaviour
    {
        [SerializeField] ARTrackedImageManager arFoundationTrackImageManager;

        public System.Action<TrackableId, ARTrackedImage> OnAddImage;
        public System.Action<TrackableId, ARTrackedImage> OnUpdateImage;
        public System.Action<TrackableId, ARTrackedImage> OnRemoveImage;

        public bool IsImageTrackingSupported { get; private set; }
        public ARTrackedImageManager ArFoundationTrackImageManager => arFoundationTrackImageManager;

        private void OnEnable()
        {
            arFoundationTrackImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        private void OnDisable()
        {
            arFoundationTrackImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }

        void Start()
        {
            CheckImageTrackingSupported();
            CheckOptionalFeatures();
        }

        /// <summary>
        /// This check works only if XR is initialized. 
        /// By default XR initializes automatically when your app starts, but this is configurable via Project Settings > XR Plug-in Management > Initialize XR on Startup
        /// </summary>
        /// <returns></returns>
        private void CheckImageTrackingSupported()
        {
            IsImageTrackingSupported = LoaderUtility.GetActiveLoader()?.GetLoadedSubsystem<XRImageTrackingSubsystem>() != null;

            if (IsImageTrackingSupported)
                Debug.Log("XRImageTrackingSubsystem was loaded. Image tracking is supported.");
            else
                Debug.LogError("Image Tracking isn't supported!");
        }

        private void CheckOptionalFeatures()
        {
            //debug every options feature supported
            Debug.Log($"Support Moving Images: {arFoundationTrackImageManager.descriptor.supportsMovingImages}");
            Debug.Log($"Require Physical Image Dimensions: {arFoundationTrackImageManager.descriptor.requiresPhysicalImageDimensions}");
            Debug.Log($"Support Mutable Library: {arFoundationTrackImageManager.descriptor.supportsMutableLibrary}");
            Debug.Log($"Support Image Validation: {arFoundationTrackImageManager.descriptor.supportsImageValidation}");
        }

        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            // Handle added event
            foreach (ARTrackedImage addedImage in eventArgs.added)
            {
                OnAddImage?.Invoke(addedImage.trackableId, addedImage);
            }

            // Handle updated event
            foreach (ARTrackedImage updatedImage in eventArgs.updated)
            {
                OnUpdateImage?.Invoke(updatedImage.trackableId, updatedImage);
            }

            // Handle removed event
            foreach (var removed in eventArgs.removed)
            {
                TrackableId removedImageTrackableId = removed.Key;
                ARTrackedImage removedImage = removed.Value;

                OnRemoveImage?.Invoke(removedImageTrackableId, removedImage);
            }
        }

        #region public API

        /// <summary>
        /// Generate a GameObject with ARTrackedImageManager and set as trackImageManager
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public ARTrackedImageManager GenerateArTrackedImageManager(XRReferenceImageLibrary library)
        {
            GameObject go = new GameObject("ARTrackedImageManager");
            ARTrackedImageManager manager = go.AddComponent<ARTrackedImageManager>();
            manager.referenceLibrary = library;
            manager.enabled = true;

            arFoundationTrackImageManager = manager;
            return manager;
        }

        /// <summary>
        /// Set library for manager
        /// </summary>
        /// <param name="library"></param>
        public void SetLibrary(XRReferenceImageLibrary library)
        {
            arFoundationTrackImageManager.referenceLibrary = library;
        }

        /// <summary>
        /// From library in project to runtime
        /// </summary>
        /// <param name="library"></param>
        /// <returns></returns>
        public RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary library)
        {
            return arFoundationTrackImageManager.CreateRuntimeLibrary(library);
        }

        /// <summary>
        /// Add image to mutable library
        /// </summary>
        /// <param name="mutableLibrary"></param>
        /// <param name="tex"></param>
        /// <param name="textureName"></param>
        /// <param name="sizeInMeters"></param>
        public void AddImageToLibrary(MutableRuntimeReferenceImageLibrary mutableLibrary, Texture2D tex, string textureName, float sizeInMeters)
        {
            mutableLibrary.ScheduleAddImageWithValidationJob(
                tex,
                textureName,
                sizeInMeters);
        }

        #endregion
    }
}