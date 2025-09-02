using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Item;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Siroshton.Masaya.UI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private InGameUI _inGameUI;
        [SerializeField] private PauseUI _pauseUI;
        [SerializeField] private LevelNameUI _levelNameUI;
        [SerializeField] private ItemPickupsUI _itemPickupsUI;
        [SerializeField] private GameObject _npcMessagePrefab;
        [SerializeField] private Material _backgroundCaptureMaterial;

        private struct Background
        {
            public bool showPause;
            public RenderTexture frameBuffer;
            public bool enableInGameUI;
        }

        Background _background;

        public void ShowPauseUI(bool show)
        {
            if( show )
            {
                _background.showPause = true;
                _background.enableInGameUI = _inGameUI.gameObject.activeSelf;
                _inGameUI.gameObject.SetActive(false);
                StartCoroutine(CaptureRoutine());
            }
            else
            {
                _pauseUI.gameObject.SetActive(false);
                RenderPipelineManager.endContextRendering -= OnContextRenderingEnd;
            }
        }

        private IEnumerator CaptureRoutine()
        {
            // we use this coroutine instead of endContextRendering otherwise ScreenCapture does not seem to work properly.
            yield return new WaitForEndOfFrame();

            // capture screen
            _background.frameBuffer = new RenderTexture(Screen.width, Screen.height, 0);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(_background.frameBuffer);

            // re-enable the InGameUI if needed
            _inGameUI.gameObject.SetActive(_background.enableInGameUI);

            // now wait for the context to finish rendering so we can make use of Graphics.Blit
            RenderPipelineManager.endContextRendering += OnContextRenderingEnd;
        }

        private void OnContextRenderingEnd(ScriptableRenderContext context, List<Camera> list)
        {
            if (!_background.showPause) return;

            // We have to use this event otherwise Graphics.Blit does not work (with URP) according to the Unity docs.

            RenderTexture a = _background.frameBuffer;
            RenderTexture b = new RenderTexture(Screen.width, Screen.height, 0);

            // if needed we can flip the image.
            LocalKeyword flipY = new LocalKeyword(_backgroundCaptureMaterial.shader, "FLIP_Y");
            _backgroundCaptureMaterial.SetKeyword(flipY, false);

            a.filterMode = FilterMode.Bilinear;
            b.filterMode = FilterMode.Bilinear;

            // blur
            int passes = 8;
            for (int pass = 0; pass < passes; pass++)
            {
                float radius = 50.0f * (1.0f - ((float)pass / (float)passes)) + 5;
                _backgroundCaptureMaterial.SetFloat("_Radius", radius);

                // horizontal gaussian pass
                _backgroundCaptureMaterial.SetTexture("_BaseMap", a);
                Graphics.Blit(null, b, _backgroundCaptureMaterial, 0);

                // vertical gaussian pass
                _backgroundCaptureMaterial.SetTexture("_BaseMap", b);
                Graphics.Blit(null, a, _backgroundCaptureMaterial, 1);
            }

            // contrast
            _backgroundCaptureMaterial.SetTexture("_BaseMap", a);
            Graphics.Blit(null, b, _backgroundCaptureMaterial, 2);

            // set background
            _pauseUI.backgroundImage = b;
            _pauseUI.gameObject.SetActive(true);
            _background.showPause = false;
        }

        public void ShowLevelName(string name)
        {
            _levelNameUI.ShowName(name);
        }

        public void PickedUpItem(IItem item)
        {
            _itemPickupsUI.PickedUpItem(item);
        }

        public MessageOverlayUI ShowNPCMessage(MessageOverlay overlay)
        {
            return ShowNPCMessage(overlay.transform, overlay.message, overlay.showButton, overlay.scale);
        }

        public MessageOverlayUI ShowNPCMessage(Transform attachTo, string message, bool showButton, float scale = 1)
        {
            GameObject o = GameObject.Instantiate(_npcMessagePrefab);
            o.transform.SetParent(_inGameUI.transform);

            MessageOverlayUI npc = o.GetComponent<MessageOverlayUI>();
            npc.attachTo = attachTo;
            npc.message = message;
            npc.showButton = showButton;
            npc.scale = scale;

            return npc;
        }

    }
}

