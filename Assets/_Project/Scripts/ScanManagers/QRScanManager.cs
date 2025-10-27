using System.Collections;
using UnityEngine;
using ZXing;

namespace redd096
{
    /// <summary>
    /// This is the QRScanner by using ZXing plugin
    /// </summary>
    public class QRScanManager : MonoBehaviour
    {
        [Header("Debug - Use test QR instead of camera")]
        [SerializeField] private bool useTestQr;
        [SerializeField] private Texture2D testQr;

        private WebCamTexture frontCamera;
        private Result scannedQr;

        /// <summary>
        /// Register to this event to know when a qr is found. Then you can call GetQRValue to get it
        /// </summary>
        public System.Action OnFoundQR { get; set; }
        /// <summary>
        /// It's true when call StartQRScan and false when call StopQRScan
        /// </summary>
        public bool IsScanning { get; set; }

        /// <summary>
        /// Play frontcamera and start coroutine to check if find some qr
        /// </summary>
        public void StartQRScan()
        {
            CheckFrontCamera();

            // if (frontCamera.isPlaying == false)
            {
                IsScanning = true;
                if (frontCamera) frontCamera.Play();
                StartCoroutine(FindQRCoroutine());
            }
        }

        /// <summary>
        /// Stop frontCamera and stop coroutine
        /// </summary>
        public void StopQRScan()
        {
            // if (frontCamera != null && frontCamera.isPlaying)
            {
                IsScanning = false;
                if (frontCamera) frontCamera.Stop();
                StopCoroutine(nameof(FindQRCoroutine));
            }
        }

        /// <summary>
        /// Try get value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string GetQRValue()
        {
            if (scannedQr != null)
            {
                return scannedQr.Text; 
                //scannedQr.BarcodeFormat
            }

            return null;
        }

        #region private API

        /// <summary>
        /// Set WebCamTexture with first webcam
        /// </summary>
        private void CheckFrontCamera()
        {
            if (frontCamera == null)
            {
                if (WebCamTexture.devices.Length > 0)
                    frontCamera = new WebCamTexture(WebCamTexture.devices[0].name);
                else
                    Debug.LogError("No front camera found.");
            }
        }

        /// <summary>
        /// If find qr, call event
        /// </summary>
        /// <returns></returns>
        private IEnumerator FindQRCoroutine()
        {
            // while (frontCamera != null && frontCamera.isPlaying)
            while (true)
            {
                //use test qr or front camera
                Color32[] pixels = useTestQr ? testQr.GetPixels32() : (frontCamera ? frontCamera.GetPixels32() : default);
                int width = useTestQr ? testQr.width : (frontCamera ? frontCamera.width : 1);
                int height = useTestQr ? testQr.height : (frontCamera ? frontCamera.height : 1);

                //check if this is a qr
                IBarcodeReader barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(pixels, width, height);
                if (result != null)
                {
                    scannedQr = result;
                    OnFoundQR?.Invoke();
                }

                yield return null;
            }
        }

        #endregion
    }
}