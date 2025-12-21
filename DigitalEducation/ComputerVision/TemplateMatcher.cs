using OpenCvSharp;
using System.Drawing;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace DigitalEducation.ComputerVision.Services
{
    public class TemplateMatcher : IDisposable
    {
        private readonly Mat _screenMat;
        private readonly Dictionary<string, Mat> _templateCache = new Dictionary<string, Mat>();

        public TemplateMatcher(Bitmap screenBitmap)
        {
            _screenMat = BitmapToMat(screenBitmap);
        }

        public RecognitionResult FindTemplate(string templatePath, double confidenceThreshold = 0.8)
        {
            if (!_templateCache.ContainsKey(templatePath))
            {
                using (var templateImage = new Mat(templatePath, ImreadModes.Color))
                {
                    _templateCache[templatePath] = templateImage.Clone();
                }
            }

            var template = _templateCache[templatePath];

            if (template.Width > _screenMat.Width || template.Height > _screenMat.Height)
                return RecognitionResult.NotFound();

            using (var result = new Mat())
            {
                Cv2.MatchTemplate(_screenMat, template, result, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

                if (maxVal >= confidenceThreshold)
                {
                    var location = new System.Drawing.Point((int)maxLoc.X, (int)maxLoc.Y);
                    var size = new System.Drawing.Size((int)template.Width, (int)template.Height);
                    return RecognitionResult.Found(location, size, maxVal);
                }
            }

            return RecognitionResult.NotFound();
        }

        public RecognitionResult FindTemplateInRegion(string templatePath, System.Drawing.Rectangle searchRegion, double confidenceThreshold = 0.8)
        {
            using (var regionBitmap = new Bitmap(searchRegion.Width, searchRegion.Height))
            using (var graphics = Graphics.FromImage(regionBitmap))
            {
                var screenBitmap = MatToBitmap(_screenMat);
                graphics.DrawImage(screenBitmap, 0, 0, searchRegion, GraphicsUnit.Pixel);

                var regionMatcher = new TemplateMatcher(regionBitmap);
                var result = regionMatcher.FindTemplate(templatePath, confidenceThreshold);
                regionMatcher.Dispose();

                if (result.IsDetected)
                {
                    var adjustedLocation = new System.Drawing.Point(
                        result.Location.X + searchRegion.X,
                        result.Location.Y + searchRegion.Y
                    );
                    return RecognitionResult.Found(adjustedLocation, result.Size, result.Confidence);
                }

                return result;
            }
        }

        public List<RecognitionResult> FindAllMatches(string templatePath, double confidenceThreshold = 0.8)
        {
            var results = new List<RecognitionResult>();
            var tempScreenMat = _screenMat.Clone();

            while (true)
            {
                var result = FindTemplateInMat(tempScreenMat, templatePath, confidenceThreshold);
                if (!result.IsDetected) break;

                results.Add(result);

                var maskRect = new Rect(
                    result.Location.X - 5,
                    result.Location.Y - 5,
                    result.Size.Width + 10,
                    result.Size.Height + 10
                );
                Cv2.Rectangle(tempScreenMat, maskRect, Scalar.Black, -1);
            }

            tempScreenMat.Dispose();
            return results;
        }

        private RecognitionResult FindTemplateInMat(Mat sourceMat, string templatePath, double confidenceThreshold)
        {
            if (!_templateCache.ContainsKey(templatePath))
            {
                using (var templateImage = new Mat(templatePath, ImreadModes.Color))
                {
                    _templateCache[templatePath] = templateImage.Clone();
                }
            }

            var template = _templateCache[templatePath];

            if (template.Width > sourceMat.Width || template.Height > sourceMat.Height)
                return RecognitionResult.NotFound();

            using (var result = new Mat())
            {
                Cv2.MatchTemplate(sourceMat, template, result, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                if (maxVal >= confidenceThreshold)
                {
                    var location = new System.Drawing.Point((int)maxLoc.X, (int)maxLoc.Y);
                    var size = new System.Drawing.Size((int)template.Width, (int)template.Height);
                    return RecognitionResult.Found(location, size, maxVal);
                }
            }

            return RecognitionResult.NotFound();
        }

        private Mat BitmapToMat(Bitmap bitmap)
        {
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                var mat = new Mat(bitmap.Height, bitmap.Width, MatType.CV_8UC3);
                long matDataLength = mat.Total() * mat.ElemSize();
                byte[] matData = new byte[matDataLength];

                byte[] bitmapDataArray = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];
                Marshal.Copy(bitmapData.Scan0, bitmapDataArray, 0, bitmapDataArray.Length);

                int matIndex = 0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int bitmapIndex = y * Math.Abs(bitmapData.Stride) + x * 3;

                        matData[matIndex] = bitmapDataArray[bitmapIndex + 2];     
                        matData[matIndex + 1] = bitmapDataArray[bitmapIndex + 1]; 
                        matData[matIndex + 2] = bitmapDataArray[bitmapIndex];     

                        matIndex += 3;
                    }
                }

                Marshal.Copy(matData, 0, mat.Data, matData.Length);
                return mat;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            var bitmap = new Bitmap(mat.Width, mat.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                long matDataLength = mat.Total() * mat.ElemSize();
                byte[] matData = new byte[matDataLength];
                Marshal.Copy(mat.Data, matData, 0, (int)matDataLength);

                byte[] bitmapDataArray = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];

                int matIndex = 0;
                for (int y = 0; y < mat.Height; y++)
                {
                    for (int x = 0; x < mat.Width; x++)
                    {
                        int bitmapIndex = y * Math.Abs(bitmapData.Stride) + x * 3;

                        bitmapDataArray[bitmapIndex] = matData[matIndex + 2];     
                        bitmapDataArray[bitmapIndex + 1] = matData[matIndex + 1]; 
                        bitmapDataArray[bitmapIndex + 2] = matData[matIndex];     

                        matIndex += 3;
                    }
                }

                Marshal.Copy(bitmapDataArray, 0, bitmapData.Scan0, bitmapDataArray.Length);
                return bitmap;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public void ClearCache()
        {
            foreach (var mat in _templateCache.Values)
            {
                mat.Dispose();
            }
            _templateCache.Clear();
        }

        public void Dispose()
        {
            _screenMat?.Dispose();
            ClearCache();
        }
    }
}