using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalEducation.ComputerVision.Services
{
    public class RecognitionResult
    {
        public bool IsDetected { get; private set; }
        public System.Drawing.Point Location { get; private set; }
        public System.Drawing.Size Size { get; private set; }
        public double Confidence { get; private set; }
        public System.Drawing.Rectangle Bounds
        {
            get { return new System.Drawing.Rectangle(Location, Size); }
        }

        private RecognitionResult() { }

        public static RecognitionResult Found(System.Drawing.Point location, System.Drawing.Size size, double confidence)
        {
            return new RecognitionResult
            {
                IsDetected = true,
                Location = location,
                Size = size,
                Confidence = confidence
            };
        }

        public static RecognitionResult NotFound()
        {
            return new RecognitionResult
            {
                IsDetected = false,
                Location = System.Drawing.Point.Empty,
                Size = System.Drawing.Size.Empty,
                Confidence = 0
            };
        }
    }
}