using System.Collections;
using UnityEngine;
using ZXing;

namespace redd096
{
    /// <summary>
    /// This is the QRScanner by using ZXing plugin
    /// </summary>
    public class QRScanManagerOld : MonoBehaviour
    {
        [SerializeField] private bool startOnAwake = true;

        [Header("Debug - Use test QR instead of camera")]
        [SerializeField] private bool useTestQr;
        [SerializeField] private Texture2D testQr;
        [SerializeField] private TMPro.TMP_Text debugText;
        [SerializeField] private UnityEngine.UI.RawImage debugRawImage;

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

		private void Awake()
        {
            if (startOnAwake)
                StartQRScan();			
		}

		/// <summary>
		/// Play frontcamera and start coroutine to check if find some qr
		/// </summary>
		public void StartQRScan()
        {
            CheckFrontCamera();

            if (frontCamera.isPlaying == false)
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
            if (frontCamera != null && frontCamera.isPlaying)
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
            try
            {
                if (frontCamera == null)
                    frontCamera = new WebCamTexture(WebCamTexture.devices[0].name);

                if (debugRawImage)
                    debugRawImage.texture = frontCamera;
            }
            catch (System.Exception e)
            {
                Debug.LogError("No front camera found. " + e.Message);
                if (debugText) 
                    debugText.text = e.Message;
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
                Color32[] pixels = useTestQr ? testQr.GetPixels32() : (frontCamera ? frontCamera.GetPixels32() : new Color32[1]);
                int width = useTestQr ? testQr.width : (frontCamera ? frontCamera.width : 1);
                int height = useTestQr ? testQr.height : (frontCamera ? frontCamera.height : 1);

                //check if this is a qr
                IBarcodeReader barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(pixels, width, height);
                if (result != null)
                {
                    scannedQr = result;
                    OnFoundQR?.Invoke();

                    if (debugText)
                        debugText.text = result.Text;
                }

                yield return null;
            }
        }

        #endregion
    }
}