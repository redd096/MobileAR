using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using System.Collections;

namespace redd096
{
    /// <summary>
    /// This is the QRScanner by using ZXing plugin and AR Foundation
    /// </summary>
    public class QRScanManager : MonoBehaviour
    {
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private TMPro.TMP_Text debugText;
        [SerializeField] private UnityEngine.UI.RawImage debugRawImage;

        /// <summary>
        /// Register to this event to know when a qr is found. Then you can call GetQRValue to get it
        /// </summary>
        public System.Action OnFoundQR { get; set; }

        private BarcodeReader barcodeReader;
        private Result scannedQr;
        private bool isRequesting;

        private void Awake()
        {
            barcodeReader = new BarcodeReader();
        }

		void Update()
        {
            AsynchronousConversion();
		}

		void AsynchronousConversion()
        {
            // If a conversion is already in progress, do nothing
            if (isRequesting)
                return;

            StartCoroutine(ConvertImageAsync());
        }

        IEnumerator ConvertImageAsync()
        {
            isRequesting = true;

            // Acquire an XRCpuImage
            if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image) == false)
            {
                // If fail, break
                isRequesting = false;
                yield break;
            }

            // Create the async conversion request
            var request = image.ConvertAsync(new XRCpuImage.ConversionParams
            {
                // Use the full image
                inputRect = new RectInt(0, 0, image.width, image.height),

                // Optionally downsample by 2
                outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

                // Output an RGB color image format
                outputFormat = TextureFormat.RGB24,

                // Flip across the Y axis
                transformation = XRCpuImage.Transformation.MirrorY
            });

            // It is safe to dispose the image before the async operation completes
            image.Dispose();

            // Wait for the conversion to complete
            while (!request.status.IsDone())
                yield return null;

            // Check status to see if the conversion completed successfully
            if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
            {
                // Something went wrong
                Debug.LogErrorFormat("Request failed with status {0}", request.status);

                request.Dispose();
                isRequesting = false;
                yield break;
            }

            // Image data is ready. Let's apply it to a Texture2D
            var rawData = request.GetData<byte>();

            Texture2D texture = new Texture2D(
                request.conversionParams.outputDimensions.x,
                request.conversionParams.outputDimensions.y,
                request.conversionParams.outputFormat,
                false);

            texture.LoadRawTextureData(rawData);
            texture.Apply();

            // Dispose the request including raw data
            request.Dispose();

            // Try scan qr and save value
            try
            {
                var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);
                if (result != null)
                {
                    scannedQr = result;
                    OnFoundQR?.Invoke();

                    if (debugText)
                        debugText.text = result.Text;
                }
            }
            catch (System.Exception e)
			{
                Debug.LogError(e.Message);
			}

            // Destroy texture (or previous and this one keep for debug raw image)
            if (debugRawImage)
            {
                if (debugRawImage.texture)
                    Destroy(debugRawImage.texture);

                debugRawImage.texture = texture;
            }
            else
            {
                Destroy(texture);
            }

            yield return null;
            isRequesting = false;
        }

        /// <summary>
        /// Try get value
        /// </summary>
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
    }
}