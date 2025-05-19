using System;
using TRavljen.EditorUtility;
using UnityEngine;

namespace TRavljen.Tooltip
{
    
    [Serializable]
    public class TooltipLayout
    {

        [Tooltip("Specifies the canvas where the rect transform resides")]
        public Canvas canvas;
        [Tooltip("Root rect transform of the tooltip. Used for retrieving position, anchor, " +
                 "size, and updating it based on potential overflow.")]
        public RectTransform rectTransform;
        
        [Header("Layout")]
        [Tooltip("Vertical alignment relative to the cursor.")]
        [SerializeField] private VerticalAlignment verticalAlignment = VerticalAlignment.Bottom;
        [Tooltip("Horizontal alignment relative to the cursor.")]
        [SerializeField] private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Right;
        
        [SerializeField] private Vector2 screenPadding = Vector2.zero;
        [Tooltip("Offset of the tooltip UI from the cursor position.")]
        [SerializeField] private Vector2 rectOffset;
        
        [Header("Overflow")]
        [Tooltip("Specifies if the alignment should change when tooltip overflows the screen.")]
        [SerializeField] private bool changeAlignment = true;
        [Tooltip("Specifies the changing alignment effects the offset as well or not.")]
        [SerializeField] private bool changeAlignmentIncludesOffset = true;
        [Tooltip("Specifies if positions of tooltip UI are restricted within the screen bounds.")]
        [SerializeField] private bool clampPosition = true;
        
        private float _scaledRectWidth;
        private float _scaledRectHeight;
        private Vector2 _scaledScreenPadding;
        private float _canvasScaleFactor;

        private bool _enabled = true;
        
        public void Setup()
        {
            if (!changeAlignment && !clampPosition)
            {
                _enabled = false;
                return;
            }
            
            if (canvas == null || rectTransform == null)
            {
                Debug.LogWarning("Overflow for tooltip required canvas and rect transform to calculate position and size of tooltip in screen space.");
                _enabled = false;
                return;
            }
            
            rectTransform.pivot = Vector2.zero;
        }

        #if UNITY_EDITOR
        internal void RenderDebugGUI()
        {
            // Overflow & debug must be enabled for debug textures to render.
            if (!enableDebug || !_enabled) return;
            debugTracking.RenderTextures();
        }
        #endif

        /// <summary>
        /// Fixes the position for overflowing <see cref="rectTransform"/> on this position.
        /// </summary>
        public Vector2 FixOverflow(Vector2 position)
        {
            if (!_enabled) return position;
            
            position += rectOffset * canvas.scaleFactor;

            _canvasScaleFactor = canvas.scaleFactor;
            _scaledScreenPadding = new Vector2(screenPadding.x * _canvasScaleFactor,
                screenPadding.y * _canvasScaleFactor);
            _scaledRectWidth = rectTransform.rect.width * _canvasScaleFactor;
            _scaledRectHeight = rectTransform.rect.height * _canvasScaleFactor;

            position = OffsetAlignment(position);
            
            #if UNITY_EDITOR
            debugTracking.UpdateDebugStartPosition(position, _scaledRectWidth, _scaledRectHeight);
            #endif

            // Handles overflow
            if (changeAlignment)
                position = SwitchSide(position);

            if (clampPosition)
                position = ClampPosition(position);
            
            #if UNITY_EDITOR
            debugTracking.UpdateDebugEndPosition(position, _scaledRectWidth, _scaledRectHeight);
            #endif

            return position;
        }

        /// <summary>
        /// Offsets position based on alignment. Pivot alignment is removed on start.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 OffsetAlignment(Vector2 position)
        {
            if (verticalAlignment == VerticalAlignment.Bottom)
                position.y -= _scaledRectHeight;
            else if (verticalAlignment == VerticalAlignment.Center)
                position.y -= _scaledRectHeight / 2;

            if (horizontalAlignment == HorizontalAlignment.Left)
                position.x -= _scaledRectWidth;
            else if (horizontalAlignment == HorizontalAlignment.Center)
                position.x -= _scaledRectWidth / 2;

            return position;
        }

        private Vector3 SwitchSide(Vector3 position)
        {
            var offset = changeAlignmentIncludesOffset ? rectOffset * _canvasScaleFactor: Vector2.zero;
            offset *= 2;

            if (IsOverflowingRight(position) && horizontalAlignment == HorizontalAlignment.Right)
                position.x -= _scaledRectWidth + offset.x;
            else if (IsOverflowingLeft(position) && horizontalAlignment == HorizontalAlignment.Left)
                position.x += _scaledRectWidth + offset.x;
            if (IsOverflowingTop(position) && verticalAlignment == VerticalAlignment.Top)
                position.y -= _scaledRectHeight + offset.y;
            else if (IsOverflowingBottom(position) && verticalAlignment == VerticalAlignment.Bottom)
                position.y += _scaledRectHeight + -offset.y;

            return position;
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            if (IsOverflowingRight(position))
                position.x = Screen.width - _scaledRectWidth - _scaledScreenPadding.x;
            else if (IsOverflowingLeft(position))
                position.x = _scaledScreenPadding.x;
            if (IsOverflowingTop(position))
                position.y = Screen.height - _scaledRectHeight - _scaledScreenPadding.y;
            else if (IsOverflowingBottom(position))
                position.y = _scaledScreenPadding.y;

            return position;
        }
        
        #region Convenience

        private bool IsOverflowingRight(Vector3 position)
            => position.x + _scaledRectWidth + _scaledScreenPadding.x > Screen.width;
        
        private bool IsOverflowingLeft(Vector3 position) => position.x < _scaledScreenPadding.x;

        private bool IsOverflowingTop(Vector3 position)
            => position.y + _scaledRectHeight + _scaledScreenPadding.y > Screen.height;

        private void ClampTop(ref Vector2 position)
            => position.y = Mathf.Min(position.y, Screen.height - _scaledRectHeight + _scaledScreenPadding.y);

        private bool IsOverflowingBottom(Vector3 position) => position.y < _scaledScreenPadding.y;
        
        #endregion
        
        #region Debug
        
        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool enableDebug;
        [SerializeField, ShowIf("layout.enableDebug", true)] private DebugTracking debugTracking = new ();
        #endif

        [Serializable]
        private class DebugTracking
        {

            [SerializeField] private Color startColor = Color.red;
            [SerializeField] private Color endColor = Color.green;
            
            private Rect _startRect;
            private Texture _startTexture;
            private Rect _endRect;
            private Texture _endTexture;

            internal void RenderTextures()
            {
                if (_startRect != _endRect && _startTexture is not null)
                    GUI.DrawTexture(_startRect, _startTexture);

                if (_endTexture is not null)
                    GUI.DrawTexture(_endRect, _endTexture);
            }

            public void UpdateDebugStartPosition(Vector3 position, float width, float height)
            {
                var newStartRect = new Rect(position.x, Screen.height - position.y - height, width, height);
                
                if (_startTexture?.width != (int)newStartRect.size.x || _startTexture?.height != (int)newStartRect.size.y)
                {
                    _startTexture =
                        new BorderTextureGenerator(5, (int)newStartRect.width, (int)newStartRect.height, startColor)
                            .GenerateBorderTexture();    
                }
                
                _startRect = newStartRect;
            }

            public void UpdateDebugEndPosition(Vector3 position, float width, float height)
            {
                var newEndRect = new Rect(position.x, Screen.height - position.y - height, width, height);
            
                if (_endTexture?.width != (int)newEndRect.size.x || _endTexture?.height != (int)newEndRect.size.y)
                {
                    _endTexture =
                        new BorderTextureGenerator(5, (int)newEndRect.width, (int)newEndRect.height, endColor)
                            .GenerateBorderTexture();
                }

                _endRect = newEndRect;
            }
        }
        
        #endregion
        
        private enum VerticalAlignment { Top, Center, Bottom }
        private enum HorizontalAlignment { Left, Center, Right }
    }
    
    public class BorderTextureGenerator
    {
        private readonly Color _transparent = new();

        private readonly int _borderWidth;
        private readonly Color _borderColor;
        private readonly int _width;
        private readonly int _height;

        public BorderTextureGenerator(int borderWidth, int width, int height, Color borderColor)
        {
            _borderWidth = borderWidth;
            _width = width;
            _height = height;
            _borderColor = borderColor;
        }

        public Texture2D GenerateBorderTexture()
        {
            if (_width == 0 || _height == 0) return null;
            
            // Create a new texture
            var texture = new Texture2D(_width, _height);
            
            // Fill with transparent pixels
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    texture.SetPixel(x, y, _transparent);
                }
            }

            // Draw the border
            for (var y = 0; y < _height; y++)
            {
                // Top and bottom borders
                if (y >= _borderWidth && y < _height - _borderWidth) continue;
                
                for (var x = 0; x < _width; x++)
                {
                    texture.SetPixel(x, y, _borderColor);
                }
            }

            for (var x = 0; x < _width; x++)
            {
                // Left and right borders
                if (x >= _borderWidth && x < _width - _borderWidth) continue;
                
                for (var y = 0; y < _height; y++)
                {
                    texture.SetPixel(x, y, _borderColor);
                }
            }
            
            // Apply changes to the texture
            texture.Apply();

            return texture;
        }

    }

}