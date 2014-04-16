using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;

namespace SpreadsheetLight.Drawing
{
    // TODO: use ShapeProperties!! Ermahgerd...

    /// <summary>
    /// Encapsulates properties and methods for a picture to be inserted into a worksheet.
    /// </summary>
    public class SLPicture
    {
        internal enum SLPictureFillType
        {
            None = 1,
            Solid
            // not supporting gradient fills yet
        }

        // as opposed to data in byte array
        internal bool DataIsInFile;
        internal string PictureFileName;
        internal byte[] PictureByteData;
        internal ImagePartType PictureImagePartType = ImagePartType.Bmp;

        internal double TopPosition;
        internal double LeftPosition;
        internal bool UseEasyPositioning;

        // as opposed to absolute position. Not supporting TwoCellAnchor
        internal bool UseRelativePositioning;

        // used when relative positioning
        internal int AnchorRowIndex;
        internal int AnchorColumnIndex;

        // in units of EMU
        internal long OffsetX;
        internal long OffsetY;
        internal long WidthInEMU;
        internal long HeightInEMU;

        internal int WidthInPixels;
        internal int HeightInPixels;

        private float fHorizontalResolution;
        /// <summary>
        /// The horizontal resolution (DPI) of the picture. This is read-only.
        /// </summary>
        public float HorizontalResolution
        {
            get { return fHorizontalResolution; }
        }

        private float fVerticalResolution;
        /// <summary>
        /// The vertical resolution (DPI) of the picture. This is read-only.
        /// </summary>
        public float VerticalResolution
        {
            get { return fVerticalResolution; }
        }

        private float fTargetHorizontalResolution;
        private float fTargetVerticalResolution;
        private float fCurrentHorizontalResolution;
        private float fCurrentVerticalResolution;

        private float fHorizontalResolutionRatio;
        private float fVerticalResolutionRatio;

        private string sAlternativeText;
        /// <summary>
        /// The text used to assist users with disabilities. This is similar to the alt tag used in HTML.
        /// </summary>
        public string AlternativeText
        {
            get { return sAlternativeText; }
            set { sAlternativeText = value; }
        }

        private bool bLockWithSheet;
        /// <summary>
        /// Indicates whether the picture can be selected (selection is disabled when this is true). Locking the picture only works when the sheet is also protected. Default value is true.
        /// </summary>
        public bool LockWithSheet
        {
            get { return bLockWithSheet; }
            set { bLockWithSheet = value; }
        }

        private bool bPrintWithSheet;
        /// <summary>
        /// Indicates whether the picture is printed when the sheet is printed. Default value is true.
        /// </summary>
        public bool PrintWithSheet
        {
            get { return bPrintWithSheet; }
            set { bPrintWithSheet = value; }
        }

        private A.BlipCompressionValues vCompressionState;
        /// <summary>
        /// Compression settings for the picture. Default value is Print.
        /// </summary>
        public A.BlipCompressionValues CompressionState
        {
            get { return vCompressionState; }
            set { vCompressionState = value; }
        }

        private decimal decBrightness;
        /// <summary>
        /// Picture brightness modifier, ranging from -100% to 100%. Accurate to 1/1000 of a percent. Default value is 0%.
        /// </summary>
        public decimal Brightness
        {
            get { return decBrightness; }
            set
            {
                decBrightness = decimal.Round(value, 3);
                if (decBrightness < -100m) decBrightness = -100m;
                if (decBrightness > 100m) decBrightness = 100m;
            }
        }

        private decimal decContrast;
        /// <summary>
        /// Picture contrast modifier, ranging from -100% to 100%. Accurate to 1/1000 of a percent. Default value is 0%.
        /// </summary>
        public decimal Contrast
        {
            get { return decContrast; }
            set
            {
                decContrast = decimal.Round(value, 3);
                if (decContrast < -100m) decContrast = -100m;
                if (decContrast > 100m) decContrast = 100m;
            }
        }

        // not supporting yet because you need to change the positional offsets too.
        //private decimal decRotationAngle;
        ///// <summary>
        ///// The rotation angle in degrees, ranging from -3600 degrees to 3600 degrees. Accurate to 1/60000 of a degree. The angle increases clockwise, starting from the 12 o'clock position. Default value is 0 degrees.
        ///// </summary>
        //public decimal RotationAngle
        //{
        //    get { return decRotationAngle; }
        //    set
        //    {
        //        decRotationAngle = value;
        //        if (decRotationAngle < -3600m) decRotationAngle = -3600m;
        //        if (decRotationAngle > 3600m) decRotationAngle = 3600m;
        //    }
        //}

        private A.ShapeTypeValues vPictureShape;
        /// <summary>
        /// Set the shape of the picture. Default value is Rectangle.
        /// </summary>
        public A.ShapeTypeValues PictureShape
        {
            get { return vPictureShape; }
            set { vPictureShape = value; }
        }

        internal SLPictureFillType FillType;
        internal string FillClassInnerXml;

        internal bool HasOutline;
        internal A.Outline PictureOutline;
        internal bool HasOutlineFill;
        internal A.SolidFill PictureOutlineFill;

        internal bool HasEffectList
        {
            get
            {
                return HasGlow || HasInnerShadow || HasOuterShadow || HasReflection || HasSoftEdge;
            }
        }

        internal bool HasGlow;
        internal long GlowRadius;
        internal string GlowColorInnerXml;

        internal bool HasInnerShadow;
        internal A.InnerShadow PictureInnerShadow;
        internal bool HasOuterShadow;
        internal A.OuterShadow PictureOuterShadow;

        internal bool HasReflection;
        internal long ReflectionBlurRadius;
        internal int ReflectionStartOpacity;
        internal int ReflectionStartPosition;
        internal int ReflectionEndAlpha;
        internal int ReflectionEndPosition;
        internal long ReflectionDistance;
        internal int ReflectionDirection;
        internal int ReflectionFadeDirection;
        internal int ReflectionHorizontalRatio;
        internal int ReflectionVerticalRatio;
        internal int ReflectionHorizontalSkew;
        internal int ReflectionVerticalSkew;
        internal A.RectangleAlignmentValues ReflectionAlignment;
        internal bool ReflectionRotateWithShape;

        internal bool HasSoftEdge;
        internal long SoftEdgeRadius;

        internal bool HasScene3D;

        internal int CameraLatitude;
        internal int CameraLongitude;
        internal int CameraRevolution;
        internal A.PresetCameraValues CameraPreset;
        internal int CameraFieldOfView;
        internal int CameraZoom;

        internal int LightRigLatitude;
        internal int LightRigLongitude;
        internal int LightRigRevolution;
        internal A.LightRigValues LightRigType;
        internal A.LightRigDirectionValues LightRigDirection;

        // Not supporting yet because don't know the range and type of input values.
        // Excel probably will use points as the input unit, but we'll just leave it for now.
        //internal bool HasBackdrop;
        //internal long BackdropAnchorX;
        //internal long BackdropAnchorY;
        //internal long BackdropAnchorZ;
        //internal long BackdropNormalDx;
        //internal long BackdropNormalDy;
        //internal long BackdropNormalDz;
        //internal long BackdropUpVectorDx;
        //internal long BackdropUpVectorDy;
        //internal long BackdropUpVectorDz;

        internal bool HasShape3D
        {
            get
            {
                return HasBevelTop || HasBevelBottom || HasExtrusion || HasContour || HasMaterialType || HasZDistance;
            }
        }

        internal bool HasBevelTop;
        internal A.BevelPresetValues BevelTopPreset;
        internal long BevelTopWidth;
        internal long BevelTopHeight;

        internal bool HasBevelBottom;
        internal A.BevelPresetValues BevelBottomPreset;
        internal long BevelBottomWidth;
        internal long BevelBottomHeight;

        internal bool HasExtrusion;
        internal long ExtrusionHeight;
        internal string ExtrusionColorInnerXml;

        internal bool HasContour;
        internal long ContourWidth;
        internal string ContourColorInnerXml;

        internal bool HasMaterialType;
        internal A.PresetMaterialTypeValues MaterialType;

        internal bool HasZDistance;
        internal long ZDistance;

        internal bool HasUri;
        internal string HyperlinkUri;
        internal System.UriKind HyperlinkUriKind;
        internal bool IsHyperlinkExternal;

        internal SLPicture()
        {
        }

        /// <summary>
        /// Initializes an instance of SLPicture given the file name of a picture.
        /// </summary>
        /// <param name="PictureFileName">The file name of a picture to be inserted.</param>
        public SLPicture(string PictureFileName)
        {
            InitialisePicture();

            DataIsInFile = true;
            InitialisePictureFile(PictureFileName);

            SetResolution(false, 96, 96);
        }

        /// <summary>
        /// Initializes an instance of SLPicture given the file name of a picture, and the targeted computer's horizontal and vertical resolution. This scales the picture according to how it will be displayed on the targeted computer screen.
        /// </summary>
        /// <param name="PictureFileName">The file name of a picture to be inserted.</param>
        /// <param name="TargetHorizontalResolution">The targeted computer's horizontal resolution (DPI).</param>
        /// <param name="TargetVerticalResolution">The targeted computer's vertical resolution (DPI).</param>
        public SLPicture(string PictureFileName, float TargetHorizontalResolution, float TargetVerticalResolution)
        {
            InitialisePicture();

            DataIsInFile = true;
            InitialisePictureFile(PictureFileName);

            SetResolution(true, TargetHorizontalResolution, TargetVerticalResolution);
        }

        // byte array as picture data suggested by Rob Hutchinson, with sample code sent in.

        /// <summary>
        /// Initializes an instance of SLPicture given a picture's data in a byte array.
        /// </summary>
        /// <param name="PictureByteData">The picture's data in a byte array.</param>
        /// <param name="PictureType">The image type of the picture.</param>
        public SLPicture(byte[] PictureByteData, ImagePartType PictureType)
        {
            InitialisePicture();

            DataIsInFile = false;
            this.PictureByteData = PictureByteData;
            this.PictureImagePartType = PictureType;

            SetResolution(false, 96, 96);
        }

        /// <summary>
        /// Initializes an instance of SLPicture given a picture's data in a byte array, and the targeted computer's horizontal and vertical resolution. This scales the picture according to how it will be displayed on the targeted computer screen.
        /// </summary>
        /// <param name="PictureByteData">The picture's data in a byte array.</param>
        /// <param name="PictureType">The image type of the picture.</param>
        /// <param name="TargetHorizontalResolution">The targeted computer's horizontal resolution (DPI).</param>
        /// <param name="TargetVerticalResolution">The targeted computer's vertical resolution (DPI).</param>
        public SLPicture(byte[] PictureByteData, ImagePartType PictureType, float TargetHorizontalResolution, float TargetVerticalResolution)
        {
            InitialisePicture();

            DataIsInFile = false;
            this.PictureByteData = new byte[PictureByteData.Length];
            for (int i = 0; i < PictureByteData.Length; ++i)
            {
                this.PictureByteData[i] = PictureByteData[i];
            }
            this.PictureImagePartType = PictureType;

            SetResolution(true, TargetHorizontalResolution, TargetVerticalResolution);
        }

        private void InitialisePicture()
        {
            // should be true once we get *everyone* to stop using those confoundedly
            // hard to understand EMUs and absolute positionings...
            UseEasyPositioning = false;
            TopPosition = 0;
            LeftPosition = 0;

            UseRelativePositioning = true;
            AnchorRowIndex = 1;
            AnchorColumnIndex = 1;
            OffsetX = 0;
            OffsetY = 0;
            WidthInEMU = 0;
            HeightInEMU = 0;
            WidthInPixels = 0;
            HeightInPixels = 0;
            fHorizontalResolutionRatio = 1;
            fVerticalResolutionRatio = 1;

            this.bLockWithSheet = true;
            this.bPrintWithSheet = true;
            this.vCompressionState = A.BlipCompressionValues.Print;
            this.decBrightness = 0;
            this.decContrast = 0;
            //this.decRotationAngle = 0;

            this.vPictureShape = A.ShapeTypeValues.Rectangle;

            this.FillType = SLPictureFillType.None;
            this.FillClassInnerXml = string.Empty;

            this.HasOutline = false;
            this.PictureOutline = new A.Outline();
            this.HasOutlineFill = false;
            this.PictureOutlineFill = new A.SolidFill();

            this.HasGlow = false;
            this.GlowRadius = 0;
            this.GlowColorInnerXml = string.Empty;

            this.HasInnerShadow = false;
            this.PictureInnerShadow = new A.InnerShadow();
            this.HasOuterShadow = false;
            this.PictureOuterShadow = new A.OuterShadow();

            this.HasReflection = false;
            this.ReflectionBlurRadius = 0;
            this.ReflectionStartOpacity = 100000;
            this.ReflectionStartPosition = 0;
            this.ReflectionEndAlpha = 0;
            this.ReflectionEndPosition = 100000;
            this.ReflectionDistance = 0;
            this.ReflectionDirection = 0;
            this.ReflectionFadeDirection = 5400000;
            this.ReflectionHorizontalRatio = 100000;
            this.ReflectionVerticalRatio = 100000;
            this.ReflectionHorizontalSkew = 0;
            this.ReflectionVerticalSkew = 0;
            this.ReflectionAlignment = A.RectangleAlignmentValues.Bottom;
            this.ReflectionRotateWithShape = true;

            this.HasSoftEdge = false;
            this.SoftEdgeRadius = 0;

            this.HasScene3D = false;

            this.CameraLatitude = 0;
            this.CameraLongitude = 0;
            this.CameraRevolution = 0;
            this.CameraPreset = A.PresetCameraValues.OrthographicFront;
            this.CameraFieldOfView = 0;
            this.CameraZoom = 0;

            this.LightRigLatitude = 0;
            this.LightRigLongitude = 0;
            this.LightRigRevolution = 0;
            this.LightRigType = A.LightRigValues.ThreePoints;
            this.LightRigDirection = A.LightRigDirectionValues.Top;

            //this.HasBackdrop = false;
            //this.BackdropAnchorX = 0;
            //this.BackdropAnchorY = 0;
            //this.BackdropAnchorZ = 0;
            //this.BackdropNormalDx = 0;
            //this.BackdropNormalDy = 0;
            //this.BackdropNormalDz = 0;
            //this.BackdropUpVectorDx = 0;
            //this.BackdropUpVectorDy = 0;
            //this.BackdropUpVectorDz = 0;

            this.HasBevelTop = false;
            this.BevelTopPreset = A.BevelPresetValues.Circle;
            this.BevelTopWidth = 76200;
            this.BevelTopHeight = 76200;

            this.HasBevelBottom = false;
            this.BevelBottomPreset = A.BevelPresetValues.Circle;
            this.BevelBottomWidth = 76200;
            this.BevelBottomHeight = 76200;

            this.HasExtrusion = false;
            this.ExtrusionHeight = 0;
            this.ExtrusionColorInnerXml = string.Empty;

            this.HasContour = false;
            this.ContourWidth = 0;
            this.ContourColorInnerXml = string.Empty;

            this.HasMaterialType = false;
            this.MaterialType = A.PresetMaterialTypeValues.WarmMatte;

            this.HasZDistance = false;
            this.ZDistance = 0;

            this.HasUri = false;
            this.HyperlinkUri = string.Empty;
            this.HyperlinkUriKind = UriKind.Absolute;
            this.IsHyperlinkExternal = true;

            this.DataIsInFile = true;
            this.PictureFileName = string.Empty;
            this.PictureByteData = new byte[1];
            this.PictureImagePartType = ImagePartType.Bmp;
        }

        private void InitialisePictureFile(string FileName)
        {
            this.PictureFileName = FileName.Trim();

            this.PictureImagePartType = SLDrawingTool.GetImagePartType(this.PictureFileName);

            string sImageFileName = this.PictureFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            sImageFileName = sImageFileName.Substring(sImageFileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            this.sAlternativeText = sImageFileName;
        }

        private void SetResolution(bool HasTarget, float TargetHorizontalResolution, float TargetVerticalResolution)
        {
            // this is used to sort of get the current computer's screen DPI
            System.Drawing.Bitmap bmResolution = new System.Drawing.Bitmap(32, 32);

            System.Drawing.Bitmap bm;
            if (this.DataIsInFile)
            {
                bm = new System.Drawing.Bitmap(this.PictureFileName);
            }
            else
            {
                MemoryStream ms = new MemoryStream(this.PictureByteData);
                bm = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(ms);
            }

            this.fHorizontalResolution = bm.HorizontalResolution;
            this.fVerticalResolution = bm.VerticalResolution;

            if (HasTarget)
            {
                this.fTargetHorizontalResolution = TargetHorizontalResolution;
                this.fTargetVerticalResolution = TargetVerticalResolution;
            }
            else
            {
                this.fTargetHorizontalResolution = bmResolution.HorizontalResolution;
                this.fTargetVerticalResolution = bmResolution.VerticalResolution;
            }

            this.fCurrentHorizontalResolution = bmResolution.HorizontalResolution;
            this.fCurrentVerticalResolution = bmResolution.VerticalResolution;
            this.fHorizontalResolutionRatio = this.fTargetHorizontalResolution / this.fCurrentHorizontalResolution;
            this.fVerticalResolutionRatio = this.fTargetVerticalResolution / this.fCurrentVerticalResolution;

            this.WidthInPixels = bm.Width;
            this.HeightInPixels = bm.Height;
            this.ResizeInPixels(bm.Width, bm.Height);
            bm.Dispose();
            bmResolution.Dispose();
        }

        /// <summary>
        /// <strong>Obsolete. </strong>Set the absolute position of the picture in pixels relative to the top-left corner of the worksheet.
        /// Consider using the SetPosition() function instead.
        /// </summary>
        /// <param name="OffsetX">Offset from the left of the worksheet in pixels.</param>
        /// <param name="OffsetY">Offset from the top of the worksheet in pixels.</param>
        [Obsolete("This is an esoteric function. Use the easier-to-understand SetPosition() function instead.")]
        public void SetAbsolutePositionInPixels(int OffsetX, int OffsetY)
        {
            // absolute position is influenced by the image resolution
            long lOffsetXinEMU = Convert.ToInt64((float)OffsetX * this.fHorizontalResolutionRatio * (float)SLConstants.InchToEMU / this.HorizontalResolution);
            long lOffsetYinEMU = Convert.ToInt64((float)OffsetY * this.fVerticalResolutionRatio * (float)SLConstants.InchToEMU / this.VerticalResolution);
            //this.SetAbsolutePositionInEMU(lOffsetXinEMU, lOffsetYinEMU);

            this.UseEasyPositioning = false;
            this.UseRelativePositioning = false;
            this.OffsetX = lOffsetXinEMU;
            this.OffsetY = lOffsetYinEMU;
        }

        /// <summary>
        /// <strong>Obsolete. </strong>Set the absolute position of the picture in English Metric Units (EMUs) relative to the top-left corner of the worksheet.
        /// Consider using the SetPosition() function instead.
        /// </summary>
        /// <param name="OffsetX">Offset from the left of the worksheet in EMUs.</param>
        /// <param name="OffsetY">Offset from the top of the worksheet in EMUs.</param>
        [Obsolete("This is an esoteric function. Use the easier-to-understand SetPosition() function instead.")]
        public void SetAbsolutePositionInEMU(long OffsetX, long OffsetY)
        {
            this.UseEasyPositioning = false;
            this.UseRelativePositioning = false;
            this.OffsetX = OffsetX;
            this.OffsetY = OffsetY;
        }

        /// <summary>
        /// Set the position of the picture relative to the top-left of the worksheet.
        /// </summary>
        /// <param name="Top">Top position based on row index. For example, 0.5 means at the half-way point of the 1st row, 2.5 means at the half-way point of the 3rd row.</param>
        /// <param name="Left">Left position based on column index. For example, 0.5 means at the half-way point of the 1st column, 2.5 means at the half-way point of the 3rd column.</param>
        public void SetPosition(double Top, double Left)
        {
            // make sure to do the calculation upon insertion
            this.UseEasyPositioning = true;
            this.TopPosition = Top;
            this.LeftPosition = Left;
            this.UseRelativePositioning = true;
            this.OffsetX = 0;
            this.OffsetY = 0;
        }

        /// <summary>
        /// <strong>Obsolete. </strong>Set the position of the picture in pixels relative to the top-left of the worksheet. The picture is anchored to the top-left corner of a given cell.
        /// Consider using the SetPosition() function instead.
        /// </summary>
        /// <param name="AnchorRowIndex">Row index of the anchor cell.</param>
        /// <param name="AnchorColumnIndex">Column index of the anchor cell.</param>
        /// <param name="OffsetX">Offset from the left of the anchor cell in pixels.</param>
        /// <param name="OffsetY">Offset from the top of the anchor cell in pixels.</param>
        [Obsolete("This is an esoteric function. Use the easier-to-understand SetPosition() function instead.")]
        public void SetRelativePositionInPixels(int AnchorRowIndex, int AnchorColumnIndex, int OffsetX, int OffsetY)
        {
            long lOffsetXinEMU = (long)OffsetX * SLDocument.PixelToEMU;
            long lOffsetYinEMU = (long)OffsetY * SLDocument.PixelToEMU;
            //this.SetRelativePositionInEMU(AnchorRowIndex, AnchorColumnIndex, lOffsetXinEMU, lOffsetYinEMU);

            this.UseEasyPositioning = false;
            this.UseRelativePositioning = true;
            this.OffsetX = lOffsetXinEMU;
            this.OffsetY = lOffsetYinEMU;

            this.AnchorRowIndex = AnchorRowIndex;
            if (this.AnchorRowIndex < 1) this.AnchorRowIndex = 1;
            if (this.AnchorRowIndex > SLConstants.RowLimit) this.AnchorRowIndex = SLConstants.RowLimit;

            this.AnchorColumnIndex = AnchorColumnIndex;
            if (this.AnchorColumnIndex < 1) this.AnchorColumnIndex = 1;
            if (this.AnchorColumnIndex > SLConstants.ColumnLimit) this.AnchorColumnIndex = SLConstants.ColumnLimit;
        }

        /// <summary>
        /// <strong>Obsolete. </strong>Set the position of the picture in English Metric Units (EMUs) relative to the top-left of the worksheet. The picture is anchored to the top-left corner of a given cell.
        /// Consider using the SetPosition() function instead.
        /// </summary>
        /// <param name="AnchorRowIndex">Row index of the anchor cell.</param>
        /// <param name="AnchorColumnIndex">Column index of the anchor cell.</param>
        /// <param name="OffsetX">Offset from the left of the anchor cell in EMUs.</param>
        /// <param name="OffsetY">Offset from the top of the anchor cell in EMUs.</param>
        [Obsolete("This is an esoteric function. Use the easier-to-understand SetPosition() function instead.")]
        public void SetRelativePositionInEMU(int AnchorRowIndex, int AnchorColumnIndex, long OffsetX, long OffsetY)
        {
            this.UseEasyPositioning = false;
            this.UseRelativePositioning = true;
            this.OffsetX = OffsetX;
            this.OffsetY = OffsetY;

            this.AnchorRowIndex = AnchorRowIndex;
            if (this.AnchorRowIndex < 1) this.AnchorRowIndex = 1;
            if (this.AnchorRowIndex > SLConstants.RowLimit) this.AnchorRowIndex = SLConstants.RowLimit;

            this.AnchorColumnIndex = AnchorColumnIndex;
            if (this.AnchorColumnIndex < 1) this.AnchorColumnIndex = 1;
            if (this.AnchorColumnIndex > SLConstants.ColumnLimit) this.AnchorColumnIndex = SLConstants.ColumnLimit;
        }

        /// <summary>
        /// Resize the picture with new size dimensions in pixels.
        /// </summary>
        /// <param name="Width">The new width in pixels.</param>
        /// <param name="Height">The new height in pixels.</param>
        public void ResizeInPixels(int Width, int Height)
        {
            long lWidthInEMU = Convert.ToInt64((float)Width * this.fHorizontalResolutionRatio * (float)SLConstants.InchToEMU / this.HorizontalResolution);
            long lHeightInEMU = Convert.ToInt64((float)Height * this.fVerticalResolutionRatio * (float)SLConstants.InchToEMU / this.VerticalResolution);
            this.ResizeInEMU(lWidthInEMU, lHeightInEMU);
        }

        /// <summary>
        /// Resize the picture with new size dimension in English Metric Units (EMUs).
        /// </summary>
        /// <param name="Width">The new width in EMUs.</param>
        /// <param name="Height">The new height in EMUs.</param>
        public void ResizeInEMU(long Width, long Height)
        {
            this.WidthInEMU = Width;
            this.HeightInEMU = Height;
        }

        private A.RgbColorModelHex FormRgbColorModelHex(System.Drawing.Color RgbColor, decimal Transparency)
        {
            A.RgbColorModelHex rgbclr = new A.RgbColorModelHex();
            rgbclr.Val = new HexBinaryValue(string.Format("{0}{1}{2}", RgbColor.R.ToString("x2"), RgbColor.G.ToString("x2"), RgbColor.B.ToString("x2")));
            int iAlpha = SLDrawingTool.CalculateAlpha(Transparency);
            // if >= 100000, then transparency was 0 (or negative),
            // then we don't have to append the Alpha class
            if (iAlpha < 100000)
            {
                rgbclr.Append(new A.Alpha() { Val = iAlpha });
            }

            return rgbclr;
        }

        private A.SchemeColor FormSchemeColor(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency)
        {
            A.SchemeColor sclr = new A.SchemeColor();
            if (Tint < -1.0m) Tint = -1.0m;
            if (Tint > 1.0m) Tint = 1.0m;

            sclr.Val = ThemeColor;

            // we don't have to do anything extra if the tint's zero.
            if (Tint < 0.0m)
            {
                Tint += 1.0m;
                Tint *= 100000m;
                sclr.Append(new A.LuminanceModulation() { Val = Convert.ToInt32(Tint) });
            }
            else if (Tint > 0.0m)
            {
                Tint *= 100000m;
                Tint = decimal.Floor(Tint);
                sclr.Append(new A.LuminanceModulation() { Val = Convert.ToInt32(100000m - Tint) });
                sclr.Append(new A.LuminanceOffset() { Val = Convert.ToInt32(Tint) });
            }

            int iAlpha = SLDrawingTool.CalculateAlpha(Transparency);
            // if >= 100000, then transparency was 0 (or negative),
            // then we don't have to append the Alpha class
            if (iAlpha < 100000)
            {
                sclr.Append(new A.Alpha() { Val = iAlpha });
            }

            return sclr;
        }

        private A.SolidFill FormSolidFill(bool UseThemeColour, System.Drawing.Color SolidColor, A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency)
        {
            A.SolidFill solidfill = new A.SolidFill();

            if (!UseThemeColour)
            {
                solidfill.RgbColorModelHex = this.FormRgbColorModelHex(SolidColor, Transparency);
            }
            else
            {
                solidfill.SchemeColor = this.FormSchemeColor(ThemeColor, Tint, Transparency);
            }

            return solidfill;
        }

        /// <summary>
        /// Fill the background of the picture with color. The color will be seen through the transparent parts of the picture.
        /// </summary>
        /// <param name="FillColor">The color used to fill the background of the picture.</param>
        /// <param name="Transparency">Transparency of the fill color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        public void SetSolidFill(System.Drawing.Color FillColor, decimal Transparency)
        {
            A.SolidFill solidfill = this.FormSolidFill(false, FillColor, A.SchemeColorValues.Light1, 0m, Transparency);

            this.FillType = SLPictureFillType.Solid;
            this.FillClassInnerXml = solidfill.InnerXml;
        }

        /// <summary>
        /// Fill the background of the picture with color. The color will be seen through the transparent parts of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used to fill the background of the picture.</param>
        /// <param name="Transparency">Transparency of the fill color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        public void SetSolidFill(A.SchemeColorValues ThemeColor, decimal Transparency)
        {
            A.SolidFill solidfill = this.FormSolidFill(true, new System.Drawing.Color(), ThemeColor, 0m, Transparency);

            this.FillType = SLPictureFillType.Solid;
            this.FillClassInnerXml = solidfill.InnerXml;
        }

        /// <summary>
        /// Fill the background of the picture with color. The color will be seen through the transparent parts of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used to fill the background of the picture.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the fill color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        public void SetSolidFill(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency)
        {
            A.SolidFill solidfill = this.FormSolidFill(true, new System.Drawing.Color(), ThemeColor, Tint, Transparency);

            this.FillType = SLPictureFillType.Solid;
            this.FillClassInnerXml = solidfill.InnerXml;
        }

        /// <summary>
        /// Set the outline color.
        /// </summary>
        /// <param name="OutlineColor">The color used to outline the picture.</param>
        /// <param name="Transparency">Transparency of the outline color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        public void SetSolidOutline(System.Drawing.Color OutlineColor, decimal Transparency)
        {
            this.PictureOutlineFill = this.FormSolidFill(false, OutlineColor, A.SchemeColorValues.Light1, 0m, Transparency);
            this.HasOutline = true;
            this.HasOutlineFill = true;
        }

        /// <summary>
        /// Set the outline color.
        /// </summary>
        /// <param name="ThemeColor">The theme color used to outline the picture.</param>
        /// <param name="Transparency">Transparency of the outline color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        public void SetSolidOutline(A.SchemeColorValues ThemeColor, decimal Transparency)
        {
            this.PictureOutlineFill = this.FormSolidFill(true, new System.Drawing.Color(), ThemeColor, 0m, Transparency);
            this.HasOutline = true;
            this.HasOutlineFill = true;
        }

        /// <summary>
        /// Set the outline color.
        /// </summary>
        /// <param name="ThemeColor">The theme color used to outline the picture.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the outline color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        public void SetSolidOutline(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency)
        {
            this.PictureOutlineFill = this.FormSolidFill(true, new System.Drawing.Color(), ThemeColor, Tint, Transparency);
            this.HasOutline = true;
            this.HasOutlineFill = true;
        }

        /// <summary>
        /// Set the outline style of the picture.
        /// </summary>
        /// <param name="Width">Width of the outline, between 0 pt and 1584 pt. Accurate to 1/12700 of a point.</param>
        /// <param name="CompoundType">Compound type. Default value is single.</param>
        /// <param name="DashType">Dash style of the outline.</param>
        /// <param name="CapType">Line cap type of the outline. Default value is square.</param>
        /// <param name="JoinType">Join type of the outline at the corners. Default value is round.</param>
        public void SetOutlineStyle(decimal Width, A.CompoundLineValues? CompoundType, A.PresetLineDashValues? DashType, A.LineCapValues? CapType, SLLineJoinValues? JoinType)
        {
            this.PictureOutline = new A.Outline();

            if (!this.HasOutlineFill)
            {
                this.PictureOutlineFill = this.FormSolidFill(true, new System.Drawing.Color(), A.SchemeColorValues.Text1, 0m, 0m);
            }

            if (Width < 0m) Width = 0m;
            if (Width > 1584m) Width = 1584m;
            this.PictureOutline.Width = Convert.ToInt32(Width * (decimal)SLConstants.PointToEMU);

            if (CompoundType != null)
            {
                this.PictureOutline.CompoundLineType = CompoundType.Value;
            }

            if (DashType != null)
            {
                A.PresetDash presetdash = new A.PresetDash();
                presetdash.Val = DashType.Value;
                this.PictureOutline.Append(presetdash);
            }

            if (CapType != null)
            {
                this.PictureOutline.CapType = CapType.Value;
            }

            if (JoinType != null)
            {
                switch (JoinType.Value)
                {
                    case SLLineJoinValues.Round:
                        this.PictureOutline.Append(new A.Round());
                        break;
                    case SLLineJoinValues.Bevel:
                        this.PictureOutline.Append(new A.Bevel());
                        break;
                    case SLLineJoinValues.Miter:
                        // 800000 was the default Excel gave
                        this.PictureOutline.Append(new A.Miter() { Limit = 800000 });
                        break;
                }
            }
            else
            {
                this.PictureOutline.Append(new A.Round());
            }

            this.HasOutline = true;
            this.HasOutlineFill = true;
        }

        private A.InnerShadow FormInnerShadow(bool UseThemeColour, System.Drawing.Color ShadowColor, A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal Blur, decimal Angle, decimal Distance)
        {
            A.InnerShadow innershadow = new A.InnerShadow();
            innershadow.BlurRadius = SLDrawingTool.CalculatePositiveCoordinate(Blur);
            // default is 0
            if (innershadow.BlurRadius.Value == 0) innershadow.BlurRadius = null;

            innershadow.Distance = SLDrawingTool.CalculatePositiveCoordinate(Distance);
            // default is 0
            if (innershadow.Distance.Value == 0) innershadow.Distance = null;

            innershadow.Direction = SLDrawingTool.CalculatePositiveFixedAngle(Angle);
            // default is 0
            if (innershadow.Direction.Value == 0) innershadow.Direction = null;

            if (!UseThemeColour)
            {
                innershadow.RgbColorModelHex = this.FormRgbColorModelHex(ShadowColor, Transparency);
            }
            else
            {
                innershadow.SchemeColor = this.FormSchemeColor(ThemeColor, Tint, Transparency);
            }

            return innershadow;
        }

        /// <summary>
        /// Set an inner shadow of the picture.
        /// </summary>
        /// <param name="ShadowColor">The color used for the inner shadow.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void SetInnerShadow(System.Drawing.Color ShadowColor, decimal Transparency, decimal Blur, decimal Angle, decimal Distance)
        {
            this.HasInnerShadow = true;
            this.PictureInnerShadow = this.FormInnerShadow(false, ShadowColor, A.SchemeColorValues.Light1, 0, Transparency, Blur, Angle, Distance);
        }

        /// <summary>
        /// Set an inner shadow of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the inner shadow.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void SetInnerShadow(A.SchemeColorValues ThemeColor, decimal Transparency, decimal Blur, decimal Angle, decimal Distance)
        {
            this.HasInnerShadow = true;
            this.PictureInnerShadow = this.FormInnerShadow(true, new System.Drawing.Color(), ThemeColor, 0, Transparency, Blur, Angle, Distance);
        }

        /// <summary>
        /// Set an inner shadow of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the inner shadow.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void SetInnerShadow(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal Blur, decimal Angle, decimal Distance)
        {
            this.HasInnerShadow = true;
            this.PictureInnerShadow = this.FormInnerShadow(true, new System.Drawing.Color(), ThemeColor, Tint, Transparency, Blur, Angle, Distance);
        }

        private A.OuterShadow FormOuterShadow(bool UseThemeColour, System.Drawing.Color ShadowColor, A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal Blur, decimal Angle, decimal Distance, decimal HorizontalRatio, decimal VerticalRatio, decimal HorizontalSkew, decimal VerticalSkew, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            A.OuterShadow outershadow = new A.OuterShadow();
            outershadow.BlurRadius = SLDrawingTool.CalculatePositiveCoordinate(Blur);
            // default is 0
            if (outershadow.BlurRadius.Value == 0) outershadow.BlurRadius = null;

            outershadow.Distance = SLDrawingTool.CalculatePositiveCoordinate(Distance);
            // default is 0
            if (outershadow.Distance.Value == 0) outershadow.Distance = null;

            outershadow.Direction = SLDrawingTool.CalculatePositiveFixedAngle(Angle);
            // default is 0
            if (outershadow.Direction.Value == 0) outershadow.Direction = null;

            outershadow.HorizontalRatio = SLDrawingTool.CalculatePercentage(HorizontalRatio);
            // default is 100000
            if (outershadow.HorizontalRatio.Value == 100000) outershadow.HorizontalRatio = null;

            outershadow.VerticalRatio = SLDrawingTool.CalculatePercentage(VerticalRatio);
            // default is 100000
            if (outershadow.VerticalRatio.Value == 100000) outershadow.VerticalRatio = null;

            outershadow.HorizontalSkew = SLDrawingTool.CalculateFixedAngle(HorizontalSkew);
            // default is 0
            if (outershadow.HorizontalSkew.Value == 0) outershadow.HorizontalSkew = null;

            outershadow.VerticalSkew = SLDrawingTool.CalculateFixedAngle(VerticalSkew);
            // default is 0
            if (outershadow.VerticalSkew.Value == 0) outershadow.VerticalSkew = null;

            // default is Bottom (b)
            if (Alignment != A.RectangleAlignmentValues.Bottom)
            {
                outershadow.Alignment = Alignment;
            }

            // default is true
            if (!RotateWithPicture)
            {
                outershadow.RotateWithShape = false;
            }

            if (!UseThemeColour)
            {
                outershadow.RgbColorModelHex = this.FormRgbColorModelHex(ShadowColor, Transparency);
            }
            else
            {
                outershadow.SchemeColor = this.FormSchemeColor(ThemeColor, Tint, Transparency);
            }

            return outershadow;
        }

        /// <summary>
        /// Set an outer shadow of the picture.
        /// </summary>
        /// <param name="ShadowColor">The color used for the outer shadow.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="Size">Scale size of shadow based on size of picture in percentage (consider a range of 1% to 200%). Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling. Default value is Bottom.</param>
        /// <param name="RotateWithPicture">True if the shadow should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetOuterShadow(System.Drawing.Color ShadowColor, decimal Transparency, decimal Size, decimal Blur, decimal Angle, decimal Distance, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            this.HasOuterShadow = true;
            this.PictureOuterShadow = this.FormOuterShadow(false, ShadowColor, A.SchemeColorValues.Light1, 0m, Transparency, Blur, Angle, Distance, Size, Size, 0m, 0m, Alignment, RotateWithPicture);
        }

        /// <summary>
        /// Set an outer shadow of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the outer shadow.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="Size">Scale size of shadow based on size of picture in percentage (consider a range of 1% to 200%). Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling. Default value is Bottom.</param>
        /// <param name="RotateWithPicture">True if the shadow should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetOuterShadow(A.SchemeColorValues ThemeColor, decimal Transparency, decimal Size, decimal Blur, decimal Angle, decimal Distance, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            this.HasOuterShadow = true;
            this.PictureOuterShadow = this.FormOuterShadow(true, new System.Drawing.Color(), ThemeColor, 0m, Transparency, Blur, Angle, Distance, Size, Size, 0m, 0m, Alignment, RotateWithPicture);
        }

        /// <summary>
        /// Set an outer shadow of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the outer shadow.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="Size">Scale size of shadow based on size of picture in percentage (consider a range of 1% to 200%). Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling. Default value is Bottom.</param>
        /// <param name="RotateWithPicture">True if the shadow should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetOuterShadow(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal Size, decimal Blur, decimal Angle, decimal Distance, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            this.HasOuterShadow = true;
            this.PictureOuterShadow = this.FormOuterShadow(true, new System.Drawing.Color(), ThemeColor, Tint, Transparency, Blur, Angle, Distance, Size, Size, 0m, 0m, Alignment, RotateWithPicture);
        }

        /// <summary>
        /// Set a perspective shadow of the picture.
        /// </summary>
        /// <param name="ShadowColor">The color used for the perspective shadow.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="HorizontalRatio">Horizontal scaling ratio in percentage (consider a range of -200% to 200%). A negative ratio flips the shadow horizontally. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="VerticalRatio">Vertical scaling ratio in percentage (consider a range of -200% to 200%). A negative ratio flips the shadow vertically. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="HorizontalSkew">Horizontal skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="VerticalSkew">Vertical skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling, angle skews and distance offsets. Default value is Bottom.</param>
        /// <param name="RotateWithPicture">True if the shadow should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetPerspectiveShadow(System.Drawing.Color ShadowColor, decimal Transparency, decimal HorizontalRatio, decimal VerticalRatio, decimal HorizontalSkew, decimal VerticalSkew, decimal Blur, decimal Angle, decimal Distance, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            this.HasOuterShadow = true;
            this.PictureOuterShadow = this.FormOuterShadow(false, ShadowColor, A.SchemeColorValues.Light1, 0m, Transparency, Blur, Angle, Distance, HorizontalRatio, VerticalRatio, HorizontalSkew, VerticalSkew, Alignment, RotateWithPicture);
        }

        /// <summary>
        /// Set a perspective shadow of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the perspective shadow.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="HorizontalRatio">Horizontal scaling ratio in percentage (consider a range of -200% to 200%). A negative ratio flips the shadow horizontally. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="VerticalRatio">Vertical scaling ratio in percentage (consider a range of -200% to 200%). A negative ratio flips the shadow vertically. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="HorizontalSkew">Horizontal skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="VerticalSkew">Vertical skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling, angle skews and distance offsets. Default value is Bottom.</param>
        /// <param name="RotateWithPicture">True if the shadow should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetPerspectiveShadow(A.SchemeColorValues ThemeColor, decimal Transparency, decimal HorizontalRatio, decimal VerticalRatio, decimal HorizontalSkew, decimal VerticalSkew, decimal Blur, decimal Angle, decimal Distance, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            this.HasOuterShadow = true;
            this.PictureOuterShadow = this.FormOuterShadow(true, new System.Drawing.Color(), ThemeColor, 0m, Transparency, Blur, Angle, Distance, HorizontalRatio, VerticalRatio, HorizontalSkew, VerticalSkew, Alignment, RotateWithPicture);
        }

        /// <summary>
        /// Set a perspective shadow of the picture.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the perspective shadow.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the shadow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="HorizontalRatio">Horizontal scaling ratio in percentage (consider a range of -200% to 200%). A negative ratio flips the shadow horizontally. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="VerticalRatio">Vertical scaling ratio in percentage (consider a range of -200% to 200%). A negative ratio flips the shadow vertically. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="HorizontalSkew">Horizontal skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="VerticalSkew">Vertical skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Blur">Shadow blur, ranging from 0 pt to 2147483647 pt (but consider a maximum of 100 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Angle">Angle of shadow projection based on picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means the shadow is to the right of the picture, 90 degrees means it's below, 180 degrees means it's to the left and 270 degrees means it's above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Distance">Distance of shadow away from picture, ranging from 0 pt to 2147483647 pt (but consider a maximum of 200 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling, angle skews and distance offsets. Default value is Bottom.</param>
        /// <param name="RotateWithPicture">True if the shadow should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetPerspectiveShadow(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal HorizontalRatio, decimal VerticalRatio, decimal HorizontalSkew, decimal VerticalSkew, decimal Blur, decimal Angle, decimal Distance, A.RectangleAlignmentValues Alignment, bool RotateWithPicture)
        {
            this.HasOuterShadow = true;
            this.PictureOuterShadow = this.FormOuterShadow(true, new System.Drawing.Color(), ThemeColor, Tint, Transparency, Blur, Angle, Distance, HorizontalRatio, VerticalRatio, HorizontalSkew, VerticalSkew, Alignment, RotateWithPicture);
        }

        /// <summary>
        /// Set a bevelled top.
        /// </summary>
        /// <param name="BevelPreset">The bevel type. Default value is circle.</param>
        /// <param name="Width">Width of the bevel, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 6 pt.</param>
        /// <param name="Height">Height of the bevel, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 6 pt.</param>
        public void Set3DBevelTop(A.BevelPresetValues BevelPreset, decimal Width, decimal Height)
        {
            this.HasBevelTop = true;
            this.BevelTopPreset = BevelPreset;
            this.BevelTopWidth = SLDrawingTool.CalculatePositiveCoordinate(Width);
            this.BevelTopHeight = SLDrawingTool.CalculatePositiveCoordinate(Height);
        }

        /// <summary>
        /// Set a bevelled bottom.
        /// </summary>
        /// <param name="BevelPreset">The bevel type. Default value is circle.</param>
        /// <param name="Width">Width of the bevel, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 6 pt.</param>
        /// <param name="Height">Height of the bevel, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 6 pt.</param>
        public void Set3DBevelBottom(A.BevelPresetValues BevelPreset, decimal Width, decimal Height)
        {
            this.HasBevelBottom = true;
            this.BevelBottomPreset = BevelPreset;
            this.BevelBottomWidth = SLDrawingTool.CalculatePositiveCoordinate(Width);
            this.BevelBottomHeight = SLDrawingTool.CalculatePositiveCoordinate(Height);
        }

        /// <summary>
        /// Set the extrusion (or depth).
        /// </summary>
        /// <param name="ExtrusionColor">The color used for the extrusion.</param>
        /// <param name="Transparency">Transparency of the extrusion color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="ExtrusionHeight">Height of the extrusion, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DExtrusion(System.Drawing.Color ExtrusionColor, decimal Transparency, decimal ExtrusionHeight)
        {
            this.HasExtrusion = true;
            this.ExtrusionHeight = SLDrawingTool.CalculatePositiveCoordinate(ExtrusionHeight);
            this.ExtrusionColorInnerXml = this.FormRgbColorModelHex(ExtrusionColor, Transparency).OuterXml;
        }

        /// <summary>
        /// Set the extrusion (or depth).
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the extrusion.</param>
        /// <param name="Transparency">Transparency of the theme color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="ExtrusionHeight">Height of the extrusion, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DExtrusion(A.SchemeColorValues ThemeColor, decimal Transparency, decimal ExtrusionHeight)
        {
            this.HasExtrusion = true;
            this.ExtrusionHeight = SLDrawingTool.CalculatePositiveCoordinate(ExtrusionHeight);
            this.ExtrusionColorInnerXml = this.FormSchemeColor(ThemeColor, 0m, Transparency).OuterXml;
        }

        /// <summary>
        /// Set the extrusion (or depth).
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the extrusion.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the theme color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="ExtrusionHeight">Height of the extrusion, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DExtrusion(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal ExtrusionHeight)
        {
            this.HasExtrusion = true;
            this.ExtrusionHeight = SLDrawingTool.CalculatePositiveCoordinate(ExtrusionHeight);
            this.ExtrusionColorInnerXml = this.FormSchemeColor(ThemeColor, Tint, Transparency).OuterXml;
        }

        /// <summary>
        /// Set the 3D contour.
        /// </summary>
        /// <param name="ContourColor">The color used for the contour.</param>
        /// <param name="Transparency">Transparency of the contour color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="ContourWidth">Width of the contour, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DContour(System.Drawing.Color ContourColor, decimal Transparency, decimal ContourWidth)
        {
            this.HasContour = true;
            this.ContourWidth = SLDrawingTool.CalculatePositiveCoordinate(ContourWidth);
            this.ContourColorInnerXml = this.FormRgbColorModelHex(ContourColor, Transparency).OuterXml;
        }

        /// <summary>
        /// Set the 3D contour.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the contour.</param>
        /// <param name="Transparency">Transparency of the theme color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="ContourWidth">Width of the contour, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DContour(A.SchemeColorValues ThemeColor, decimal Transparency, decimal ContourWidth)
        {
            this.HasContour = true;
            this.ContourWidth = SLDrawingTool.CalculatePositiveCoordinate(ContourWidth);
            this.ContourColorInnerXml = this.FormSchemeColor(ThemeColor, 0m, Transparency).OuterXml;
        }

        /// <summary>
        /// Set the 3D contour.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the contour.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the theme color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="ContourWidth">Width of the contour, ranging from 0 pt to 2147483647 pt (but consider a maximum of 1584 pt). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DContour(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal ContourWidth)
        {
            this.HasContour = true;
            this.ContourWidth = SLDrawingTool.CalculatePositiveCoordinate(ContourWidth);
            this.ContourColorInnerXml = this.FormSchemeColor(ThemeColor, Tint, Transparency).OuterXml;
        }

        /// <summary>
        /// Set the surface material.
        /// </summary>
        /// <param name="MaterialType">The material used. Default value is WarmMatte.</param>
        public void Set3DMaterialType(A.PresetMaterialTypeValues MaterialType)
        {
            this.HasMaterialType = true;
            this.MaterialType = MaterialType;
        }

        /// <summary>
        /// Set the Z distance.
        /// </summary>
        /// <param name="ZDistance">The Z distance, ranging from -4000 pt to 4000 pt. Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void Set3DZDistance(decimal ZDistance)
        {
            this.HasZDistance = true;
            if (ZDistance < -4000m) ZDistance = -4000m;
            if (ZDistance > 4000m) ZDistance = 4000m;
            this.ZDistance = Convert.ToInt64(ZDistance * (decimal)SLConstants.PointToEMU);
        }

        /// <summary>
        /// Set the camera and light properties.
        /// </summary>
        /// <param name="CameraPreset">A preset set of properties for the camera, which can be overridden. Default value is OrthographicFront.</param>
        /// <param name="FieldOfView">Field of view, ranging from 0 degrees to 180 degrees. Accurate to 1/60000 of a degree.</param>
        /// <param name="Zoom">Zoom percentage, ranging from 0% to 2147483.647%. Accurate to 1/1000 of a percent.</param>
        /// <param name="CameraLatitude">Camera latitude angle, ranging from 0 degrees to 359.9 degrees. Accurate to 1/60000 of a degree.</param>
        /// <param name="CameraLongitude">Camera longitude angle, ranging from 0 degrees to 359.9 degrees. Accurate to 1/60000 of a degree.</param>
        /// <param name="CameraRevolution">Camera revolution angle, ranging from 0 degrees to 359.9 degrees. Accurate to 1/60000 of a degree.</param>
        /// <param name="LightRigType">The type of light used. Default value is ThreePoints.</param>
        /// <param name="LightRigDirection">The direction of the light. Default value is Top.</param>
        /// <param name="LightRigLatitude">Light rig latitude angle, ranging from 0 degrees to 359.9 degrees. Accurate to 1/60000 of a degree.</param>
        /// <param name="LightRigLongitude">Light rig longitude angle, ranging from 0 degrees to 359.9 degrees. Accurate to 1/60000 of a degree.</param>
        /// <param name="LightRigRevolution">Light rig revolution angle, ranging from 0 degrees to 359.9 degrees. Accurate to 1/60000 of a degree.</param>
        /// <remarks>Imagine the screen to be the X-Y plane, the positive X-axis pointing to the right, and the positive Y-axis pointing up.
        /// The positive Z-axis points perpendicularly from the screen towards you.
        /// The latitude value increases as you turn around the X-axis, using the right-hand rule.
        /// The longitude value increases as you turn around the Y-axis, using the <em>left-hand rule</em> (meaning it decreases according to right-hand rule).
        /// The revolution value increases as you turn around the Z-axis, using the right-hand rule.
        /// And if you're mapping values directly from Microsoft Excel, don't treat the X, Y and Z values as values related to the axes.
        /// The latitude maps to the Y value, longitude maps to the X value, and revolution maps to the Z value.</remarks>
        public void Set3DScene(A.PresetCameraValues CameraPreset, decimal FieldOfView, decimal Zoom, decimal CameraLatitude, decimal CameraLongitude, decimal CameraRevolution, A.LightRigValues LightRigType, A.LightRigDirectionValues LightRigDirection, decimal LightRigLatitude, decimal LightRigLongitude, decimal LightRigRevolution)
        {
            this.HasScene3D = true;

            this.CameraPreset = CameraPreset;

            if (FieldOfView < 0m) FieldOfView = 0m;
            if (FieldOfView > 180m) FieldOfView = 180m;
            this.CameraFieldOfView = Convert.ToInt32(FieldOfView * (decimal)SLConstants.DegreeToAngleRepresentation);

            this.CameraLatitude = SLDrawingTool.CalculatePositiveFixedAngle(CameraLatitude);
            this.CameraLongitude = SLDrawingTool.CalculatePositiveFixedAngle(CameraLongitude);
            this.CameraRevolution = SLDrawingTool.CalculatePositiveFixedAngle(CameraRevolution);

            // Zoom is held in an Int32, so the max is 2147483.647 (after the division by 1000).
            // But seriously, 2147483.647% zoom ?!?!
            if (Zoom < 0m) Zoom = 0m;
            if (Zoom > 2147483.647m) Zoom = 2147483.647m;
            this.CameraZoom = Convert.ToInt32(Zoom * 1000m);

            this.LightRigType = LightRigType;
            this.LightRigDirection = LightRigDirection;
            this.LightRigLatitude = SLDrawingTool.CalculatePositiveFixedAngle(LightRigLatitude);
            this.LightRigLongitude = SLDrawingTool.CalculatePositiveFixedAngle(LightRigLongitude);
            this.LightRigRevolution = SLDrawingTool.CalculatePositiveFixedAngle(LightRigRevolution);
        }

        /// <summary>
        /// Set soft edges on the picture.
        /// </summary>
        /// <param name="Radius">Radius of the soft edge, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point.</param>
        public void SetSoftEdge(decimal Radius)
        {
            this.HasSoftEdge = true;
            this.SoftEdgeRadius = SLDrawingTool.CalculatePositiveCoordinate(Radius);
        }

        /// <summary>
        /// Set the picture to glow on the edges.
        /// </summary>
        /// <param name="GlowColor">The color used for the glow.</param>
        /// <param name="Transparency">Transparency of the glow color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="GlowRadius">Radius of the glow, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point.</param>
        public void SetGlow(System.Drawing.Color GlowColor, decimal Transparency, decimal GlowRadius)
        {
            this.HasGlow = true;
            this.GlowColorInnerXml = this.FormRgbColorModelHex(GlowColor, Transparency).OuterXml;
            this.GlowRadius = SLDrawingTool.CalculatePositiveCoordinate(GlowRadius);
        }

        /// <summary>
        /// Set the picture to glow on the edges.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the glow.</param>
        /// <param name="Transparency">Transparency of the theme color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="GlowRadius">Radius of the glow, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point.</param>
        public void SetGlow(A.SchemeColorValues ThemeColor, decimal Transparency, decimal GlowRadius)
        {
            this.HasGlow = true;
            this.GlowColorInnerXml = this.FormSchemeColor(ThemeColor, 0m, Transparency).OuterXml;
            this.GlowRadius = SLDrawingTool.CalculatePositiveCoordinate(GlowRadius);
        }

        /// <summary>
        /// Set the picture to glow on the edges.
        /// </summary>
        /// <param name="ThemeColor">The theme color used for the glow.</param>
        /// <param name="Tint">The tint applied to the theme color, ranging from -1.0 to 1.0. Negative tints darken the theme color and positive tints lighten the theme color.</param>
        /// <param name="Transparency">Transparency of the theme color ranging from 0% to 100%. Accurate to 1/1000 of a percent.</param>
        /// <param name="GlowRadius">Radius of the glow, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point.</param>
        public void SetGlow(A.SchemeColorValues ThemeColor, decimal Tint, decimal Transparency, decimal GlowRadius)
        {
            this.HasGlow = true;
            this.GlowColorInnerXml = this.FormSchemeColor(ThemeColor, Tint, Transparency).OuterXml;
            this.GlowRadius = SLDrawingTool.CalculatePositiveCoordinate(GlowRadius);
        }

        /// <summary>
        /// Set a tight reflection of the picture.
        /// </summary>
        public void SetTightReflection()
        {
            this.SetTightReflection(0m);
        }

        /// <summary>
        /// Set a tight reflection of the picture.
        /// </summary>
        /// <param name="Offset">Offset distance of the reflection below the picture, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void SetTightReflection(decimal Offset)
        {
            this.SetReflection(0.5m, 50m, 0m, 0.3m, 35m, Offset, 90m, 90m, 100m, -100m, 0m, 0m, A.RectangleAlignmentValues.BottomLeft, false);
        }

        /// <summary>
        /// Set a reflection that's about half of the picture.
        /// </summary>
        public void SetHalfReflection()
        {
            this.SetHalfReflection(0m);
        }

        /// <summary>
        /// Set a reflection that's about half of the picture.
        /// </summary>
        /// <param name="Offset">Offset distance of the reflection below the picture, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void SetHalfReflection(decimal Offset)
        {
            this.SetReflection(0.5m, 50m, 0m, 0.3m, 55m, Offset, 90m, 90m, 100m, -100m, 0m, 0m, A.RectangleAlignmentValues.BottomLeft, false);
        }

        /// <summary>
        /// Set a full reflection of the picture.
        /// </summary>
        public void SetFullReflection()
        {
            this.SetFullReflection(0m);
        }

        /// <summary>
        /// Set a full reflection of the picture.
        /// </summary>
        /// <param name="Offset">Offset distance of the reflection below the picture, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        public void SetFullReflection(decimal Offset)
        {
            this.SetReflection(0.5m, 50m, 0m, 0.3m, 90m, Offset, 90m, 90m, 100m, -100m, 0m, 0m, A.RectangleAlignmentValues.BottomLeft, false);
        }

        /// <summary>
        /// Set a reflection of the picture.
        /// </summary>
        /// <param name="BlurRadius">Blur radius of the reflection, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="StartOpacity">Start opacity of the reflection, ranging from 0% to 100%. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="StartPosition">Position of start opacity of the reflection, ranging from 0% to 100%. Accurate to 1/1000 of a percent. Default value is 0%.</param>
        /// <param name="EndAlpha">End alpha of the reflection, ranging from 0% to 100%. Accurate to 1/1000 of a percent. Default value is 0%.</param>
        /// <param name="EndPosition">Position of end alpha of the reflection, ranging from 0% to 100%. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="Distance">Distance of the reflection from the picture, ranging from 0 pt to 2147483647 pt (but consider a much lower maximum). Accurate to 1/12700 of a point. Default value is 0 pt.</param>
        /// <param name="Direction">Direction of the alpha gradient relative to the picture, ranging from 0 degrees to 359.9 degrees. 0 degrees means to the right, 90 degrees is below, 180 degrees is to the right, and 270 degrees is above. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="FadeDirection">Direction to fade the reflection, ranging from 0 degrees to 359.9 degrees. 0 degrees means to the right, 90 degrees is below, 180 degrees is to the right, and 270 degrees is above. Accurate to 1/60000 of a degree. Default value is 90 degrees.</param>
        /// <param name="HorizontalRatio">Horizontal scaling ratio in percentage. A negative ratio flips the reflection horizontally. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="VerticalRatio">Vertical scaling ratio in percentage. A negative ratio flips the reflection vertically. Accurate to 1/1000 of a percent. Default value is 100%.</param>
        /// <param name="HorizontalSkew">Horizontal skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="VerticalSkew">Vertical skew angle, ranging from -90 degrees to 90 degrees. Accurate to 1/60000 of a degree. Default value is 0 degrees.</param>
        /// <param name="Alignment">Sets the origin of the picture for the size scaling, angle skews and distance offsets. Default value is Bottom.</param>
        /// <param name="RotateWithShape">True if the reflection should rotate with the picture if the picture is rotated. False otherwise. Default value is true.</param>
        public void SetReflection(decimal BlurRadius, decimal StartOpacity, decimal StartPosition, decimal EndAlpha, decimal EndPosition, decimal Distance, decimal Direction, decimal FadeDirection, decimal HorizontalRatio, decimal VerticalRatio, decimal HorizontalSkew, decimal VerticalSkew, A.RectangleAlignmentValues Alignment, bool RotateWithShape)
        {
            this.HasReflection = true;

            this.ReflectionBlurRadius = SLDrawingTool.CalculatePositiveCoordinate(BlurRadius);
            this.ReflectionStartOpacity = SLDrawingTool.CalculatePositiveFixedPercentage(StartOpacity);
            this.ReflectionStartPosition = SLDrawingTool.CalculatePositiveFixedPercentage(StartPosition);
            this.ReflectionEndAlpha = SLDrawingTool.CalculatePositiveFixedPercentage(EndAlpha);
            this.ReflectionEndPosition = SLDrawingTool.CalculatePositiveFixedPercentage(EndPosition);
            this.ReflectionDistance = SLDrawingTool.CalculatePositiveCoordinate(Distance);
            this.ReflectionDirection = SLDrawingTool.CalculatePositiveFixedAngle(Direction);
            this.ReflectionFadeDirection = SLDrawingTool.CalculatePositiveFixedAngle(FadeDirection);
            this.ReflectionHorizontalRatio = SLDrawingTool.CalculatePercentage(HorizontalRatio);
            this.ReflectionVerticalRatio = SLDrawingTool.CalculatePercentage(VerticalRatio);
            this.ReflectionHorizontalSkew = SLDrawingTool.CalculateFixedAngle(HorizontalSkew);
            this.ReflectionVerticalSkew = SLDrawingTool.CalculateFixedAngle(VerticalSkew);
            this.ReflectionAlignment = Alignment;
            this.ReflectionRotateWithShape = RotateWithShape;
        }

        /// <summary>
        /// Inserts a hyperlink to a webpage.
        /// </summary>
        /// <param name="URL">The target webpage URL.</param>
        public void InsertUrlHyperlink(string URL)
        {
            this.HasUri = true;
            this.HyperlinkUri = URL;
            this.HyperlinkUriKind = UriKind.Absolute;
            this.IsHyperlinkExternal = true;
        }

        /// <summary>
        /// Inserts a hyperlink to a document on the computer.
        /// </summary>
        /// <param name="FilePath">The relative path to the file based on the location of the spreadsheet.</param>
        public void InsertFileHyperlink(string FilePath)
        {
            this.HasUri = true;
            this.HyperlinkUri = FilePath;
            this.HyperlinkUriKind = UriKind.Relative;
            this.IsHyperlinkExternal = true;
        }

        /// <summary>
        /// Inserts a hyperlink to an email address.
        /// </summary>
        /// <param name="EmailAddress">The email address, such as johndoe@acme.com</param>
        public void InsertEmailHyperlink(string EmailAddress)
        {
            this.HasUri = true;
            this.HyperlinkUri = string.Format("mailto:{0}", EmailAddress);
            this.HyperlinkUriKind = UriKind.Absolute;
            this.IsHyperlinkExternal = true;
        }

        /// <summary>
        /// Inserts a hyperlink to a place in the spreadsheet document.
        /// </summary>
        /// <param name="SheetName">The name of the worksheet being referenced.</param>
        /// <param name="RowIndex">The row index of the referenced cell. If this is invalid, row 1 will be used.</param>
        /// <param name="ColumnIndex">The column index of the referenced cell. If this is invalid, column 1 will be used.</param>
        public void InsertInternalHyperlink(string SheetName, int RowIndex, int ColumnIndex)
        {
            int iRowIndex = RowIndex;
            int iColumnIndex = ColumnIndex;
            if (iRowIndex < 1 || iRowIndex > SLConstants.RowLimit) iRowIndex = 1;
            if (iColumnIndex < 1 || iColumnIndex > SLConstants.ColumnLimit) iColumnIndex = 1;

            this.HasUri = true;
            this.HyperlinkUri = string.Format("#{0}!{1}", SheetName, SLTool.ToCellReference(iRowIndex, iColumnIndex));
            this.HyperlinkUriKind = UriKind.Relative;
            this.IsHyperlinkExternal = false;
        }

        /// <summary>
        /// Inserts a hyperlink to a place in the spreadsheet document.
        /// </summary>
        /// <param name="SheetName">The name of the worksheet being referenced.</param>
        /// <param name="CellReference">The cell reference, such as "A1".</param>
        public void InsertInternalHyperlink(string SheetName, string CellReference)
        {
            this.HasUri = true;
            this.HyperlinkUri = string.Format("#{0}!{1}", SheetName, CellReference);
            this.HyperlinkUriKind = UriKind.Relative;
            this.IsHyperlinkExternal = false;
        }

        /// <summary>
        /// Inserts a hyperlink to a place in the spreadsheet document.
        /// </summary>
        /// <param name="DefinedName">A defined name in the spreadsheet.</param>
        public void InsertInternalHyperlink(string DefinedName)
        {
            this.HasUri = true;
            this.HyperlinkUri = string.Format("#{0}", DefinedName);
            this.HyperlinkUriKind = UriKind.Relative;
            this.IsHyperlinkExternal = false;
        }

        internal SLPicture Clone()
        {
            SLPicture pic = new SLPicture();
            pic.DataIsInFile = this.DataIsInFile;
            pic.PictureFileName = this.PictureFileName;
            pic.PictureByteData = new byte[this.PictureByteData.Length];
            for (int i = 0; i < this.PictureByteData.Length; ++i)
            {
                pic.PictureByteData[i] = this.PictureByteData[i];
            }
            pic.PictureImagePartType = this.PictureImagePartType;

            pic.TopPosition = this.TopPosition;
            pic.LeftPosition = this.LeftPosition;
            pic.UseEasyPositioning = this.UseEasyPositioning;
            pic.UseRelativePositioning = this.UseRelativePositioning;
            pic.AnchorRowIndex = this.AnchorRowIndex;
            pic.AnchorColumnIndex = this.AnchorColumnIndex;
            pic.OffsetX = this.OffsetX;
            pic.OffsetY = this.OffsetY;
            pic.WidthInEMU = this.WidthInEMU;
            pic.HeightInEMU = this.HeightInEMU;
            pic.WidthInPixels = this.WidthInPixels;
            pic.HeightInPixels = this.HeightInPixels;
            pic.fHorizontalResolution = this.fHorizontalResolution;
            pic.fVerticalResolution = this.fVerticalResolution;
            pic.fTargetHorizontalResolution = this.fTargetHorizontalResolution;
            pic.fTargetVerticalResolution = this.fTargetVerticalResolution;
            pic.fCurrentHorizontalResolution = this.fCurrentHorizontalResolution;
            pic.fCurrentVerticalResolution = this.fCurrentVerticalResolution;
            pic.fHorizontalResolutionRatio = this.fHorizontalResolutionRatio;
            pic.fVerticalResolutionRatio = this.fVerticalResolutionRatio;
            pic.sAlternativeText = this.sAlternativeText;
            pic.bLockWithSheet = this.bLockWithSheet;
            pic.bPrintWithSheet = this.bPrintWithSheet;
            pic.vCompressionState = this.vCompressionState;
            pic.decBrightness = this.decBrightness;
            pic.decContrast = this.decContrast;
            pic.vPictureShape = this.vPictureShape;
            pic.FillType = this.FillType;
            pic.FillClassInnerXml = this.FillClassInnerXml;
            pic.HasOutline = this.HasOutline;
            pic.PictureOutline = this.PictureOutline;
            pic.HasOutlineFill = this.HasOutlineFill;
            pic.PictureOutlineFill = this.PictureOutlineFill;
            pic.HasGlow = this.HasGlow;
            pic.GlowRadius = this.GlowRadius;
            pic.GlowColorInnerXml = this.GlowColorInnerXml;
            pic.HasInnerShadow = this.HasInnerShadow;
            pic.PictureInnerShadow = this.PictureInnerShadow;
            pic.HasOuterShadow = this.HasOuterShadow;
            pic.PictureOuterShadow = this.PictureOuterShadow;

            pic.HasReflection = this.HasReflection;
            pic.ReflectionBlurRadius = this.ReflectionBlurRadius;
            pic.ReflectionStartOpacity = this.ReflectionStartOpacity;
            pic.ReflectionStartPosition = this.ReflectionStartPosition;
            pic.ReflectionEndAlpha = this.ReflectionEndAlpha;
            pic.ReflectionEndPosition = this.ReflectionEndPosition;
            pic.ReflectionDistance = this.ReflectionDistance;
            pic.ReflectionDirection = this.ReflectionDirection;
            pic.ReflectionFadeDirection = this.ReflectionFadeDirection;
            pic.ReflectionHorizontalRatio = this.ReflectionHorizontalRatio;
            pic.ReflectionVerticalRatio = this.ReflectionVerticalRatio;
            pic.ReflectionHorizontalSkew = this.ReflectionHorizontalSkew;
            pic.ReflectionVerticalSkew = this.ReflectionVerticalSkew;
            pic.ReflectionAlignment = this.ReflectionAlignment;
            pic.ReflectionRotateWithShape = this.ReflectionRotateWithShape;

            pic.HasSoftEdge = this.HasSoftEdge;
            pic.SoftEdgeRadius = this.SoftEdgeRadius;
            
            pic.HasScene3D = this.HasScene3D;

            pic.CameraLatitude = this.CameraLatitude;
            pic.CameraLongitude = this.CameraLongitude;
            pic.CameraRevolution = this.CameraRevolution;
            pic.CameraPreset = this.CameraPreset;
            pic.CameraFieldOfView = this.CameraFieldOfView;
            pic.CameraZoom = this.CameraZoom;
            pic.LightRigLatitude = this.LightRigLatitude;
            pic.LightRigLongitude = this.LightRigLongitude;
            pic.LightRigRevolution = this.LightRigRevolution;
            pic.LightRigType = this.LightRigType;
            pic.LightRigDirection = this.LightRigDirection;

            pic.HasBevelTop = this.HasBevelTop;
            pic.BevelTopPreset = this.BevelTopPreset;
            pic.BevelTopWidth = this.BevelTopWidth;
            pic.BevelTopHeight = this.BevelTopHeight;
            pic.HasBevelBottom = this.HasBevelBottom;
            pic.BevelBottomPreset = this.BevelBottomPreset;
            pic.BevelBottomWidth = this.BevelBottomWidth;
            pic.BevelBottomHeight = this.BevelBottomHeight;

            pic.HasExtrusion = this.HasExtrusion;
            pic.ExtrusionHeight = this.ExtrusionHeight;
            pic.ExtrusionColorInnerXml = this.ExtrusionColorInnerXml;

            pic.HasContour = this.HasContour;
            pic.ContourWidth = this.ContourWidth;
            pic.ContourColorInnerXml = this.ContourColorInnerXml;

            pic.HasMaterialType = this.HasMaterialType;
            pic.MaterialType = this.MaterialType;

            pic.HasZDistance = this.HasZDistance;
            pic.ZDistance = this.ZDistance;

            pic.HasUri = this.HasUri;
            pic.HyperlinkUri = this.HyperlinkUri;
            pic.HyperlinkUriKind = this.HyperlinkUriKind;
            pic.IsHyperlinkExternal = this.IsHyperlinkExternal;

            return pic;
        }
    }
}
