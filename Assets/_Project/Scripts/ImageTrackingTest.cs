using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace redd096
{
    public class ImageTrackingTest : MonoBehaviour
    {
        [SerializeField] ImageTrackingManager imageTrackingManager;
        [SerializeField] QRScanManager qrScanManager;
        [SerializeField] FPrefabs[] prefabs;

        private void Awake()
        {
            //register to ar foundation events
            imageTrackingManager.OnAddImage += OnAddImage;
            imageTrackingManager.OnUpdateImage += OnUpdateImage;
            imageTrackingManager.OnRemoveImage += OnRemoveImage;

            //start qr scan
            qrScanManager.OnFoundQR += OnFoundQR;
            qrScanManager.StartQRScan();
        }

		private void OnDestroy()
        {
            //unregister events
            imageTrackingManager.OnAddImage -= OnAddImage;
            imageTrackingManager.OnUpdateImage -= OnUpdateImage;
            imageTrackingManager.OnRemoveImage -= OnRemoveImage;

            if (qrScanManager && qrScanManager.IsScanning)
            {
                qrScanManager.OnFoundQR -= OnFoundQR;
                qrScanManager.StopQRScan();
            }
        }

        private void OnAddImage(TrackableId id, ARTrackedImage image)
        {
            // // Give the initial image a reasonable default scale
            // float minLocalScalar = Mathf.Min(image.size.x, image.size.y) * 0.5f;
            // image.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);

            // ARTrackableManager will instantiate an EmptyObject with ARTrackedImage script.
            // If inside ARTrackedImageManager's inspector is setted a prefab, 
            // ARTrackableManager will instantiate it instead of EmptyObject but always with ARTrackedImage script
 
            // ARTrackableManager will set its position like this:
            // var origin = imageTrackingManager.ArFoundationTrackImageManager.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            // Pose worldSpacePose = origin.TrackablesParent.TransformPose(image.pose);
            // image.transform.SetPositionAndRotation(worldSpacePose.position, worldSpacePose.rotation);
 
            // and OnUpdateImage will update its position and rotation
            // OnRemoveImage will destroy it
 
            // you can use the ARTrackedImage object to get its position in the world and instantiate anything as a child
        }

        private void OnUpdateImage(TrackableId id, ARTrackedImage image)
        {
        }

        private void OnRemoveImage(TrackableId id, ARTrackedImage image)
        {
        }

        private void OnFoundQR()
        {
            //unregister from events and stop scan
            qrScanManager.OnFoundQR -= OnFoundQR;
            qrScanManager.StopQRScan();

            //call function if qr is correct
            string value = qrScanManager.GetQRValue();
            if (string.IsNullOrEmpty(value) == false)
            {
                Debug.Log(value);
            }
            //or show error and restart qr scan
            else
            {
                Debug.LogWarning("Found invalid QR");

                qrScanManager.OnFoundQR += OnFoundQR;
                qrScanManager.StartQRScan();
            }

		}

        [System.Serializable]
        public struct FPrefabs
        {
            public GameObject prefab;
            public string qr;
        }
    }
}