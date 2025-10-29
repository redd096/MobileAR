using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace redd096
{
    public class ImageTrackingTest : MonoBehaviour
    {
        [SerializeField] ImageTrackingManager imageTrackingManager;
        [SerializeField] QRScanManager qrScanManager;
        [SerializeField] TMP_Text debugText;
        [SerializeField] FPrefabs[] prefabs;

        //list of tracked images
        private List<ARTrackedImage> trackedImages = new List<ARTrackedImage>();
        //list of found qr string
        private string currentQrValue;
        //links between string and tracked image
        private Dictionary<string, ARTrackedImage> qrImages = new Dictionary<string, ARTrackedImage>();
        //links between string and instantiated prefab (child of tracked image)
        private Dictionary<string, GameObject> qrPrefabs = new Dictionary<string, GameObject>();

        private void Awake()
        {
            //register to ar foundation events
            imageTrackingManager.OnAddImage += OnAddImage;
            imageTrackingManager.OnUpdateImage += OnUpdateImage;
            imageTrackingManager.OnRemoveImage += OnRemoveImage;

            //register to qr events
            qrScanManager.OnFoundQR += OnFoundQR;
        }

        private void OnDestroy()
        {
            //unregister events
            imageTrackingManager.OnAddImage -= OnAddImage;
            imageTrackingManager.OnUpdateImage -= OnUpdateImage;
            imageTrackingManager.OnRemoveImage -= OnRemoveImage;

            if (qrScanManager)
                qrScanManager.OnFoundQR -= OnFoundQR;
        }

        #region events - populate trackedImages and currentQrValue

        private void OnAddImage(TrackableId id, ARTrackedImage image)
        {
            // Give the initial image a reasonable default scale
            float minLocalScalar = Mathf.Min(image.size.x, image.size.y) * 0.5f;
            image.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);

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

            //add to the list
            if (trackedImages.Contains(image) == false)
            {
                trackedImages.Add(image);
                OnAddImage(image);
			}
        }

        private void OnUpdateImage(TrackableId id, ARTrackedImage image)
        {
            //add or remove from the list
            if (image.trackingState == TrackingState.Limited || image.trackingState == TrackingState.None)
            {
                if (trackedImages.Contains(image))
                {
                    trackedImages.Remove(image);
                    OnRemoveImage(image);
                }
            }
            else //if (image.trackingState == TrackingState.Tracking)
			{
                if (trackedImages.Contains(image) == false)
                {
                    trackedImages.Add(image);
                    OnAddImage(image);
                }
            }
		}

        private void OnRemoveImage(TrackableId id, ARTrackedImage image)
        {
            //remove from the list
            if (trackedImages.Contains(image))
            {
                trackedImages.Remove(image);
                OnRemoveImage(image);
            }
        }

        private void OnFoundQR()
        {
            //call function if qr is correct
            string value = qrScanManager.GetQRValue();
            if (string.IsNullOrEmpty(value) == false)
            {
                currentQrValue = value;
                if (debugText) debugText.text = currentQrValue;
                OnFoundCorrectQR();
            }
        }

        #endregion

        private void OnAddImage(ARTrackedImage image)
        {
            //find image in dictionary
            string key = null;
            foreach (var v in qrImages)
            {
                if (v.Value == image)
                {
                    key = v.Key;
                    break;
                }
            }

            //if image was in dictionary, then it already has a link, just recreate the prefab
            if (string.IsNullOrEmpty(key) == false)
            {
                InstantiatePrefab(key, image);
            }
            //else, this still need a link to a qr string
            else
            {
                //if previously was found a QRString, now we can link it
                if (string.IsNullOrEmpty(currentQrValue) == false && qrImages.ContainsKey(currentQrValue) == false)
                    LinkQrAndImage(currentQrValue, image);
            }
        }
        
        private void OnRemoveImage(ARTrackedImage image)
        {
            //find image in dictionary
            string key = null;
            foreach (var v in qrImages)
            {
                if (v.Value == image)
                {
                    key = v.Key;
                    break;
                }
            }

            //if image was in dictionary, than keep association but destroy prefab
            if (string.IsNullOrEmpty(key) == false && qrPrefabs.ContainsKey(key))
			{
                Destroy(qrPrefabs[key]);
                qrPrefabs.Remove(key);
			}
		}

        private void OnFoundCorrectQR()
        {
			//find an image not in dictionary
            foreach (var image in trackedImages)
            {
                if (qrImages.ContainsValue(image) == false)
                {
                    //if there is, then previously was found an Image, now we can link it
                    LinkQrAndImage(currentQrValue, image);
                    break;
                }
            }
		}

        #region instantiate and associate QR and image

        private void LinkQrAndImage(string value, ARTrackedImage image)
        {
            //add to dictionary (associate)
            qrImages.Add(currentQrValue, image);

            InstantiatePrefab(value, image);
        }

        private void InstantiatePrefab(string value, ARTrackedImage image)
        {
            //find prefab
            foreach (var element in prefabs)
            {
                if (element.qr.Equals(value, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    //and instantiate as child of this image
                    GameObject go = Instantiate(element.prefab, image.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

                    qrPrefabs.Add(value, go);
                    break;
                }
            }
        }
        
        #endregion

        [System.Serializable]
        public struct FPrefabs
        {
            public GameObject prefab;
            public string qr;
        }
    }
}