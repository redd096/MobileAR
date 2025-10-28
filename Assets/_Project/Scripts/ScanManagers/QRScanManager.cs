using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ZXing;
using Unity.Collections;
using UnityEngine.Rendering;

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

        void OnEnable()
        {
            // Register to ar camera events
            if (arCameraManager != null)
                arCameraManager.frameReceived += OnCameraFrameReceived;
        }

        void OnDisable()
        {
            // Unregister events
            if (arCameraManager != null)
                arCameraManager.frameReceived -= OnCameraFrameReceived;
        }

        private async void OnCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            if (args.textures != null && args.textures.Count > 0 && isRequesting == false)
            {
                isRequesting = true;

                try
                {
                    // Get texture from AR camera
                    Texture2D nativeTex = args.textures[0];

                    // Wait GPU Reader
                    var request = AsyncGPUReadback.Request(nativeTex, 0, TextureFormat.RGBA32);
                    while (request.done == false)
                        await System.Threading.Tasks.Task.Yield();

                    if (request.hasError)
                    {
                        Debug.Log("GPU readback error detected.");
                        if (debugText != null)
                            debugText.text = "GPU readback error detected.";

                        isRequesting = false;
                        return;
                    }

                    // Get the data from the request
                    NativeArray<byte> data = request.GetData<byte>();

                    // Convert the byte data to Color32 array
                    Color32[] colors = new Color32[data.Length / 4];
                    System.Buffer.BlockCopy(data.ToArray(), 0, colors, 0, data.Length);

                    // Decode the QR code
                    var result = barcodeReader.Decode(colors, nativeTex.width, nativeTex.height);
                    if (result != null)
                    {
                        scannedQr = result;
                        OnFoundQR?.Invoke();

                        if (debugText != null)
                            debugText.text = result.Text;
                    }

                    if (debugRawImage)
                    {
                        debugRawImage.texture = nativeTex;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error scanning QR code: " + e.Message);
                    if (debugText != null)
                        debugText.text = "Error scanning QR code: " + e.Message;
                }
                finally
                {
                    isRequesting = false;
                }
            }
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