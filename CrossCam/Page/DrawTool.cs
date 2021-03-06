﻿using System;
using SkiaSharp;

namespace CrossCam.Page
{
    public class DrawTool
    {
        private const float FLOATY_ZERO = 0.00001f;

        public static void DrawImagesOnCanvas(
            SKCanvas canvas, SKBitmap leftBitmap, SKBitmap rightBitmap,
            int borderThickness, bool addBorder, BorderColor borderColor,
            int leftLeftCrop, int leftRightCrop, int rightLeftCrop, int rightRightCrop,
            int leftTopCrop, int leftBottomCrop, int rightTopCrop, int rightBottomCrop,
            float leftRotation, float rightRotation, int alignment,
            int leftZoom, int rightZoom,
            float leftKeystone, float rightKeystone, 
            bool switchForParallel = false)
        {
            if (leftBitmap == null && rightBitmap == null) return;

            var canvasWidth = canvas.DeviceClipBounds.Width;
            var canvasHeight = canvas.DeviceClipBounds.Height;

            var innerBorderThickness = leftBitmap != null && rightBitmap != null && addBorder ? borderThickness : 0;
            var leftBitmapWidthLessCrop = 0;
            var rightBitmapWidthLessCrop = 0;
            var bitmapHeightLessCrop = 0;
            var aspectRatio = 0f;

            if (leftBitmap != null)
            {
                leftBitmapWidthLessCrop = leftBitmap.Width - leftLeftCrop - leftRightCrop;
                bitmapHeightLessCrop = leftBitmap.Height - leftTopCrop - leftBottomCrop - Math.Abs(alignment);
                aspectRatio = leftBitmap.Height / (1f * leftBitmap.Width);
            }

            if (rightBitmap != null)
            {
                rightBitmapWidthLessCrop = rightBitmap.Width - rightLeftCrop - rightRightCrop;
                if (leftBitmap == null)
                {
                    bitmapHeightLessCrop = rightBitmap.Height - rightTopCrop - rightBottomCrop - Math.Abs(alignment);
                    aspectRatio = rightBitmap.Height / (1f * rightBitmap.Width);
                }
            }

            int effectiveJoinedWidth;
            if (leftBitmapWidthLessCrop > rightBitmapWidthLessCrop)
            {
                effectiveJoinedWidth = leftBitmapWidthLessCrop * 2;
            }
            else
            {
                effectiveJoinedWidth = rightBitmapWidthLessCrop * 2;
            }

            effectiveJoinedWidth += 4 * innerBorderThickness;
            var effectiveJoinedHeight = bitmapHeightLessCrop + 2 * innerBorderThickness;

            var widthRatio = effectiveJoinedWidth / (1f * canvasWidth);
            var heightRatio = effectiveJoinedHeight / (1f * canvasHeight);

            var scalingRatio = widthRatio > heightRatio ? widthRatio : heightRatio;

            var previewY = canvasHeight / 2f - bitmapHeightLessCrop / scalingRatio / 2f;
            var previewHeight = bitmapHeightLessCrop / scalingRatio;

            float leftPreviewX;
            float rightPreviewX;
            float leftPreviewWidth;
            float rightPreviewWidth;
            float innerLeftRotation;
            float innerRightRotation;
            float innerLeftKeystone;
            float innerRightKeystone;
            if (switchForParallel)
            {
                leftPreviewX = canvasWidth / 2f + innerBorderThickness / scalingRatio;
                rightPreviewX = canvasWidth / 2f - (rightBitmapWidthLessCrop + innerBorderThickness) / scalingRatio;
                leftPreviewWidth = rightBitmapWidthLessCrop / scalingRatio;
                rightPreviewWidth = leftBitmapWidthLessCrop / scalingRatio;
                innerRightRotation = leftRotation;
                innerLeftRotation = rightRotation;
                innerRightKeystone = leftKeystone;
                innerLeftKeystone = rightKeystone;
            }
            else
            {
                leftPreviewX = canvasWidth / 2f - (leftBitmapWidthLessCrop + innerBorderThickness) / scalingRatio;
                rightPreviewX = canvasWidth / 2f + innerBorderThickness / scalingRatio;
                leftPreviewWidth = leftBitmapWidthLessCrop / scalingRatio;
                rightPreviewWidth = rightBitmapWidthLessCrop / scalingRatio;
                innerRightRotation = rightRotation;
                innerLeftRotation = leftRotation;
                innerRightKeystone = rightKeystone;
                innerLeftKeystone = leftKeystone;
            }
            var isRightRotated = Math.Abs(innerRightRotation) > FLOATY_ZERO;
            var isLeftRotated = Math.Abs(innerLeftRotation) > FLOATY_ZERO;
            var isRightKeystoned = Math.Abs(innerRightKeystone) > FLOATY_ZERO;
            var isLeftKeystoned = Math.Abs(innerLeftKeystone) > FLOATY_ZERO;

            if (leftBitmap != null)
            {
                SKBitmap transformed = null;
                if (isLeftRotated ||
                    leftZoom > 0 ||
                    isLeftKeystoned)
                {
                    transformed = ZoomAndRotate(leftBitmap, aspectRatio, leftZoom, isLeftRotated, innerLeftRotation, isLeftKeystoned, innerLeftKeystone);
                }
                
                canvas.DrawBitmap(
                    transformed ?? leftBitmap,
                    SKRect.Create(
                        leftLeftCrop,
                        leftTopCrop + (alignment > 0 ? alignment : 0),
                        (transformed?.Width ?? leftBitmap.Width) - leftLeftCrop - leftRightCrop,
                        (transformed?.Height ?? leftBitmap.Height) - leftTopCrop - leftBottomCrop - Math.Abs(alignment)),
                    SKRect.Create(
                        leftPreviewX,
                        previewY,
                        leftPreviewWidth,
                        previewHeight));
                transformed?.Dispose();
            }

            if (rightBitmap != null)
            {
                SKBitmap transformed = null;
                if (isRightRotated ||
                    rightZoom > 0 || 
                    isRightKeystoned)
                {
                    transformed = ZoomAndRotate(rightBitmap, aspectRatio, rightZoom, isRightRotated, innerRightRotation, isRightKeystoned, innerRightKeystone);
                }
                
                canvas.DrawBitmap(
                    transformed ?? rightBitmap,
                    SKRect.Create(
                        rightLeftCrop,
                        rightTopCrop - (alignment < 0 ? alignment : 0),
                        (transformed?.Width ?? rightBitmap.Width) - rightLeftCrop - rightRightCrop,
                        (transformed?.Height ?? rightBitmap.Height) - rightTopCrop - rightBottomCrop - Math.Abs(alignment)),
                    SKRect.Create(
                        rightPreviewX,
                        previewY,
                        rightPreviewWidth,
                        previewHeight));
                transformed?.Dispose();
            }

            if (innerBorderThickness > 0)
            {
                var borderPaint = new SKPaint
                {
                    Color = borderColor == BorderColor.Black ? SKColor.Parse("000000") : SKColor.Parse("ffffff"),
                    Style = SKPaintStyle.StrokeAndFill
                };
                var originX = leftPreviewX - innerBorderThickness / scalingRatio;
                var originY = previewY - innerBorderThickness / scalingRatio;
                var fullPreviewWidth = leftPreviewWidth + rightPreviewWidth + 4 * innerBorderThickness / scalingRatio;
                var fullPreviewHeight = previewHeight + 2 * innerBorderThickness / scalingRatio;
                var scaledBorderThickness = innerBorderThickness / scalingRatio;
                var endX = rightPreviewX + rightPreviewWidth;
                var endY = previewY + previewHeight;
                canvas.DrawRect(originX, originY, fullPreviewWidth, scaledBorderThickness, borderPaint);
                canvas.DrawRect(originX, originY, scaledBorderThickness, fullPreviewHeight, borderPaint);
                canvas.DrawRect(canvasWidth / 2f - scaledBorderThickness, originY, scaledBorderThickness, fullPreviewHeight, borderPaint);
                canvas.DrawRect(canvasWidth / 2f, originY, scaledBorderThickness, fullPreviewHeight, borderPaint);
                canvas.DrawRect(endX, originY, scaledBorderThickness, fullPreviewHeight, borderPaint);
                canvas.DrawRect(originX, endY, fullPreviewWidth, scaledBorderThickness, borderPaint);
            }
        }

        private static SKBitmap ZoomAndRotate(SKBitmap originalBitmap, float aspectRatio, int zoom, bool isRotated, float rotation, bool isKeystoned, float keystone)
        {
            var rotatedAndZoomed = new SKBitmap(originalBitmap.Width, originalBitmap.Height);

            using (var tempCanvas = new SKCanvas(rotatedAndZoomed))
            {
                var rightVerticalZoom = aspectRatio * zoom;
                var zoomedX = zoom / -2f;
                var zoomedY = rightVerticalZoom / -2f;
                var zoomedWidth = originalBitmap.Width + zoom;
                var zoomedHeight = originalBitmap.Height + rightVerticalZoom;
                if (isRotated)
                {
                    tempCanvas.RotateDegrees(rotation, originalBitmap.Width / 2f,
                        originalBitmap.Height / 2f);
                }
                tempCanvas.DrawBitmap(
                    originalBitmap,
                    SKRect.Create(
                        0,
                        0,
                        originalBitmap.Width,
                        originalBitmap.Height),
                    SKRect.Create(
                        zoomedX,
                        zoomedY,
                        zoomedWidth,
                        zoomedHeight
                    ));
                if (isRotated)
                {
                    tempCanvas.RotateDegrees(rotation, -1 * originalBitmap.Width / 2f,
                        originalBitmap.Height / 2f);
                }
            }

            SKBitmap keystoned = null;
            if (isKeystoned)
            {
                keystoned = new SKBitmap(originalBitmap.Width, originalBitmap.Height);
                using (var tempCanvas = new SKCanvas(keystoned))
                {
                    tempCanvas.SetMatrix(TaperTransform.Make(new SKSize(originalBitmap.Width, originalBitmap.Height),
                        keystone > 0 ? TaperSide.Left : TaperSide.Right, TaperCorner.Both, 1 - Math.Abs(keystone)));
                    tempCanvas.DrawBitmap(rotatedAndZoomed, 0, 0);
                    rotatedAndZoomed.Dispose();
                }
            }

            return keystoned ?? rotatedAndZoomed;
        }

        public static int CalculateCanvasWidth(SKBitmap leftBitmap, SKBitmap rightBitmap, 
            int leftLeftCrop, int leftRightCrop, int rightLeftCrop, int rightRightCrop,
            int borderThickness)
        {
            if (leftBitmap == null && rightBitmap == null) return 0;

            if (leftBitmap == null || rightBitmap == null)
            {
                if (leftBitmap != null)
                {
                    return leftBitmap.Width * 2;
                }

                return rightBitmap.Width * 2;
            }

            return leftBitmap.Width + rightBitmap.Width -
                leftLeftCrop - leftRightCrop - rightLeftCrop - rightRightCrop +
                4 * borderThickness;
        }

        public static int CalculateCanvasHeight(SKBitmap leftBitmap, SKBitmap rightBitmap, 
            int leftTopCrop, int leftBottomCrop, int rightTopCrop, int rightBottomCrop,
            int alignment, int borderThickness)
        {
            if (leftBitmap == null && rightBitmap == null) return 0;

            if (leftBitmap == null || rightBitmap == null)
            {
                return leftBitmap?.Height ?? rightBitmap.Height;
            }

            return leftBitmap.Height - leftTopCrop - leftBottomCrop - Math.Abs(alignment) +
                   2 * borderThickness;
            var rightHeight = rightBitmap.Height - rightTopCrop - rightBottomCrop +
                              2 * borderThickness; // not sure when this is not the same as left
        }
    }
}