using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

namespace redd096
{
    public class ImageTrackingTest : MonoBehaviour
    {
        [SerializeField] ImageTrackingManager imageTrackingManager;
        [SerializeField] XROrigin origin;
        [SerializeField] FPrefabs[] prefabs;

        private void Awake()
        {
            imageTrackingManager.OnAddImage += OnAddImage;
            imageTrackingManager.OnUpdateImage += OnUpdateImage;
            imageTrackingManager.OnRemoveImage += OnRemoveImage;
        }

        private void OnDestroy()
        {
            imageTrackingManager.OnAddImage -= OnAddImage;
            imageTrackingManager.OnUpdateImage -= OnUpdateImage;
            imageTrackingManager.OnRemoveImage -= OnRemoveImage;
        }

        private void OnAddImage(TrackableId id, ARTrackedImage image)
        {
            //// Give the initial image a reasonable default scale
            //float minLocalScalar = Mathf.Min(image.size.x, image.size.y) * 0.5f;
            //image.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);

            ////if in ARTrackedImageManager's inspector is setted a prefab
            ////ARTrackableManager will instantiate it and will set its position like this:
            ////var origin = imageTrackingManager.ArFoundationTrackImageManager.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            ////Pose worldSpacePose = origin.TrackablesParent.TransformPose(image.pose);
            ////image.transform.SetPositionAndRotation(worldSpacePose.position, worldSpacePose.rotation);

            ////and OnUpdateImage will update its position and rotation
            ////OnRemoveImage will destroy it

            ////else you have to instantiate and set positions manually

            Debug.Log(image.referenceImage.name);
        }

        private void OnUpdateImage(TrackableId id, ARTrackedImage image)
        {
        }

        private void OnRemoveImage(TrackableId id, ARTrackedImage image)
        {
        }

        [System.Serializable]
        public struct FPrefabs
        {
            public GameObject prefab;
            public string qr;
        }
    }
}