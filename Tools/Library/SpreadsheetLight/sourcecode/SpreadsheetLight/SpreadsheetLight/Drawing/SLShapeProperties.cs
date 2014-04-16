using System;
using System.Collections.Generic;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using SLA = SpreadsheetLight.Drawing;

namespace SpreadsheetLight.Drawing
{
    internal class SLShapeProperties
    {
        internal List<System.Drawing.Color> listThemeColors;

        internal bool HasShapeProperties
        {
            get
            {
                // take note of HasGeometry when it's used

                return this.HasTransform2D || this.Fill.HasFill || this.Outline.HasLine
                    || this.EffectList.HasEffectList || this.Rotation3D.HasCamera || this.Format3D.HasLighting
                    || this.Format3D.HasBevelTop || this.Format3D.HasBevelBottom || this.Format3D.HasExtrusionColor
                    || this.Format3D.HasContourColor || this.Format3D.ExtrusionHeight != 0
                    || this.Format3D.ContourWidth != 0 || this.Format3D.Material != A.PresetMaterialTypeValues.WarmMatte
                    || this.Rotation3D.DistanceZ != 0;
            }
        }

        // for pictures? Not used for charts. Leave it here first...
        internal bool HasTransform2D;
        internal SLTransform2D Transform2D { get; set; }

        // we'll ignore custom geometry. For pictures? Leave it here first...
        internal bool HasGeometry;
        internal A.ShapeTypeValues vPresetGeometry;
        internal A.ShapeTypeValues PresetGeometry
        {
            get { return vPresetGeometry; }
            set
            {
                vPresetGeometry = value;
                HasGeometry = true;
            }
        }

        internal SLFill Fill { get; set; }
        internal SLLinePropertiesType Outline { get; set; }

        internal SLEffectList EffectList { get; set; }

        internal SLRotation3D Rotation3D { get; set; }
        internal SLFormat3D Format3D { get; set; }

        internal SLShapeProperties(List<System.Drawing.Color> ThemeColors)
        {
            int i;
            this.listThemeColors = new List<System.Drawing.Color>();
            for (i = 0; i < ThemeColors.Count; ++i)
            {
                this.listThemeColors.Add(ThemeColors[i]);
            }

            this.SetAllNull();
        }

        private void SetAllNull()
        {
            this.Transform2D = new SLTransform2D();
            this.HasTransform2D = false;
            this.vPresetGeometry = A.ShapeTypeValues.Rectangle;
            this.HasGeometry = false;

            this.Fill = new SLFill(this.listThemeColors);
            this.Outline = new SLLinePropertiesType(this.listThemeColors);
            this.EffectList = new SLEffectList(this.listThemeColors);

            this.Rotation3D = new SLRotation3D();
            this.Format3D = new SLFormat3D(this.listThemeColors);
        }

        // the logic is exactly the same for C.ChartShapeProperties and A.ShapeProperties,
        // but we're duplicating it because the base class is different
        internal C.ChartShapeProperties ToChartShapeProperties()
        {
            C.ChartShapeProperties sp = new C.ChartShapeProperties();

            if (this.HasTransform2D) sp.Transform2D = this.Transform2D.ToTransform2D();

            if (this.Fill.HasFill) sp.Append(this.Fill.ToFill());

            if (this.Outline.HasLine) sp.Append(this.Outline.ToOutline());

            if (this.EffectList.HasEffectList) sp.Append(this.EffectList.ToEffectList());

            // the bevel top and bottom seems to require camera and lighting.
            // Not sure if that's all the relationship linking, so just leave as it is first...
            if (this.Rotation3D.HasCamera || this.Format3D.HasLighting
                || this.Format3D.HasBevelTop || this.Format3D.HasBevelBottom)
            {
                A.Scene3DType scene3d = new A.Scene3DType();
                if (this.Rotation3D.HasCamera)
                {
                    scene3d.Camera = new A.Camera();
                    scene3d.Camera.Preset = this.Rotation3D.CameraPreset;
                    if (this.Rotation3D.HasPerspectiveSet)
                    {
                        scene3d.Camera.FieldOfView = SLA.SLDrawingTool.CalculateFovAngle(this.Rotation3D.Perspective);
                    }
                    if (this.Rotation3D.HasXYZSet)
                    {
                        scene3d.Camera.Rotation = new A.Rotation();
                        scene3d.Camera.Rotation.Latitude = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Rotation3D.Y);
                        scene3d.Camera.Rotation.Longitude = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Rotation3D.X);
                        scene3d.Camera.Rotation.Revolution = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Rotation3D.Z);
                    }
                }
                else
                {
                    scene3d.Camera = new A.Camera() { Preset = A.PresetCameraValues.OrthographicFront };
                }

                if (this.Format3D.HasLighting)
                {
                    scene3d.LightRig = new A.LightRig();
                    scene3d.LightRig.Rig = this.Format3D.Lighting;
                    scene3d.LightRig.Direction = A.LightRigDirectionValues.Top;
                    if (this.Format3D.Angle != 0)
                    {
                        scene3d.LightRig.Rotation = new A.Rotation()
                        {
                            Latitude = 0,
                            Longitude = 0,
                            Revolution = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Format3D.Angle)
                        };
                    }
                }
                else
                {
                    scene3d.LightRig = new A.LightRig();
                    scene3d.LightRig.Rig = A.LightRigValues.ThreePoints;
                    scene3d.LightRig.Direction = A.LightRigDirectionValues.Top;
                }

                sp.Append(scene3d);
            }

            if (this.Format3D.HasBevelTop || this.Format3D.HasBevelBottom || this.Format3D.HasExtrusionColor
                || this.Format3D.HasContourColor || this.Format3D.ExtrusionHeight != 0
                || this.Format3D.ContourWidth != 0 || this.Format3D.Material != A.PresetMaterialTypeValues.WarmMatte
                || this.Rotation3D.DistanceZ != 0)
            {
                A.Shape3DType shape3d = new A.Shape3DType();

                if (this.Format3D.HasBevelTop)
                {
                    shape3d.BevelTop = new A.BevelTop();
                    if (this.Format3D.BevelTopWidth != 6m) shape3d.BevelTop.Width = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelTopWidth);
                    if (this.Format3D.BevelTopHeight != 6m) shape3d.BevelTop.Height = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelTopHeight);
                    if (this.Format3D.BevelTopPreset != A.BevelPresetValues.Circle) shape3d.BevelTop.Preset = this.Format3D.BevelTopPreset;
                }

                if (this.Format3D.HasBevelBottom)
                {
                    shape3d.BevelBottom = new A.BevelBottom();
                    if (this.Format3D.BevelBottomWidth != 6m) shape3d.BevelBottom.Width = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelBottomWidth);
                    if (this.Format3D.BevelBottomHeight != 6m) shape3d.BevelBottom.Height = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelBottomHeight);
                    if (this.Format3D.BevelBottomPreset != A.BevelPresetValues.Circle) shape3d.BevelBottom.Preset = this.Format3D.BevelBottomPreset;
                }

                if (this.Format3D.HasExtrusionColor)
                {
                    shape3d.ExtrusionColor = new A.ExtrusionColor();
                    if (this.Format3D.clrExtrusionColor.IsRgbColorModelHex)
                    {
                        shape3d.ExtrusionColor.RgbColorModelHex = this.Format3D.clrExtrusionColor.ToRgbColorModelHex();
                    }
                    else
                    {
                        shape3d.ExtrusionColor.SchemeColor = this.Format3D.clrExtrusionColor.ToSchemeColor();
                    }
                }

                if (this.Format3D.HasContourColor)
                {
                    shape3d.ContourColor = new A.ContourColor();
                    if (this.Format3D.clrContourColor.IsRgbColorModelHex)
                    {
                        shape3d.ContourColor.RgbColorModelHex = this.Format3D.clrContourColor.ToRgbColorModelHex();
                    }
                    else
                    {
                        shape3d.ContourColor.SchemeColor = this.Format3D.clrContourColor.ToSchemeColor();
                    }
                }

                if (this.Rotation3D.DistanceZ != 0m)
                {
                    shape3d.Z = SLA.SLDrawingTool.CalculateCoordinate(this.Rotation3D.DistanceZ);
                }

                if (this.Format3D.ExtrusionHeight != 0m)
                {
                    shape3d.ExtrusionHeight = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.ExtrusionHeight);
                }

                if (this.Format3D.ContourWidth != 0m)
                {
                    shape3d.ContourWidth = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.ContourWidth);
                }

                if (this.Format3D.Material != A.PresetMaterialTypeValues.WarmMatte)
                {
                    shape3d.PresetMaterial = this.Format3D.Material;
                }

                sp.Append(shape3d);
            }

            return sp;
        }

        // the logic is exactly the same for C.ChartShapeProperties and C.ShapeProperties,
        // but we're duplicating it because the base class is different
        /// <summary>
        /// This is for C.ShapeProperties, not A.ShapeProperties
        /// </summary>
        /// <returns></returns>
        internal C.ShapeProperties ToCShapeProperties()
        {
            C.ShapeProperties sp = new C.ShapeProperties();

            if (this.HasTransform2D) sp.Transform2D = this.Transform2D.ToTransform2D();

            if (this.Fill.HasFill) sp.Append(this.Fill.ToFill());

            if (this.Outline.HasLine) sp.Append(this.Outline.ToOutline());

            if (this.EffectList.HasEffectList) sp.Append(this.EffectList.ToEffectList());

            // the bevel top and bottom seems to require camera and lighting.
            // Not sure if that's all the relationship linking, so just leave as it is first...
            if (this.Rotation3D.HasCamera || this.Format3D.HasLighting
                || this.Format3D.HasBevelTop || this.Format3D.HasBevelBottom)
            {
                A.Scene3DType scene3d = new A.Scene3DType();
                if (this.Rotation3D.HasCamera)
                {
                    scene3d.Camera = new A.Camera();
                    scene3d.Camera.Preset = this.Rotation3D.CameraPreset;
                    if (this.Rotation3D.HasPerspectiveSet)
                    {
                        scene3d.Camera.FieldOfView = SLA.SLDrawingTool.CalculateFovAngle(this.Rotation3D.Perspective);
                    }
                    if (this.Rotation3D.HasXYZSet)
                    {
                        scene3d.Camera.Rotation = new A.Rotation();
                        scene3d.Camera.Rotation.Latitude = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Rotation3D.Y);
                        scene3d.Camera.Rotation.Longitude = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Rotation3D.X);
                        scene3d.Camera.Rotation.Revolution = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Rotation3D.Z);
                    }
                }
                else
                {
                    scene3d.Camera = new A.Camera() { Preset = A.PresetCameraValues.OrthographicFront };
                }

                if (this.Format3D.HasLighting)
                {
                    scene3d.LightRig = new A.LightRig();
                    scene3d.LightRig.Rig = this.Format3D.Lighting;
                    scene3d.LightRig.Direction = A.LightRigDirectionValues.Top;
                    if (this.Format3D.Angle != 0)
                    {
                        scene3d.LightRig.Rotation = new A.Rotation()
                        {
                            Latitude = 0,
                            Longitude = 0,
                            Revolution = SLA.SLDrawingTool.CalculatePositiveFixedAngle(this.Format3D.Angle)
                        };
                    }
                }
                else
                {
                    scene3d.LightRig = new A.LightRig();
                    scene3d.LightRig.Rig = A.LightRigValues.ThreePoints;
                    scene3d.LightRig.Direction = A.LightRigDirectionValues.Top;
                }

                sp.Append(scene3d);
            }

            if (this.Format3D.HasBevelTop || this.Format3D.HasBevelBottom || this.Format3D.HasExtrusionColor
                || this.Format3D.HasContourColor || this.Format3D.ExtrusionHeight != 0
                || this.Format3D.ContourWidth != 0 || this.Format3D.Material != A.PresetMaterialTypeValues.WarmMatte
                || this.Rotation3D.DistanceZ != 0)
            {
                A.Shape3DType shape3d = new A.Shape3DType();

                if (this.Format3D.HasBevelTop)
                {
                    shape3d.BevelTop = new A.BevelTop();
                    if (this.Format3D.BevelTopWidth != 6m) shape3d.BevelTop.Width = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelTopWidth);
                    if (this.Format3D.BevelTopHeight != 6m) shape3d.BevelTop.Height = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelTopHeight);
                    if (this.Format3D.BevelTopPreset != A.BevelPresetValues.Circle) shape3d.BevelTop.Preset = this.Format3D.BevelTopPreset;
                }

                if (this.Format3D.HasBevelBottom)
                {
                    shape3d.BevelBottom = new A.BevelBottom();
                    if (this.Format3D.BevelBottomWidth != 6m) shape3d.BevelBottom.Width = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelBottomWidth);
                    if (this.Format3D.BevelBottomHeight != 6m) shape3d.BevelBottom.Height = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.BevelBottomHeight);
                    if (this.Format3D.BevelBottomPreset != A.BevelPresetValues.Circle) shape3d.BevelBottom.Preset = this.Format3D.BevelBottomPreset;
                }

                if (this.Format3D.HasExtrusionColor)
                {
                    shape3d.ExtrusionColor = new A.ExtrusionColor();
                    if (this.Format3D.clrExtrusionColor.IsRgbColorModelHex)
                    {
                        shape3d.ExtrusionColor.RgbColorModelHex = this.Format3D.clrExtrusionColor.ToRgbColorModelHex();
                    }
                    else
                    {
                        shape3d.ExtrusionColor.SchemeColor = this.Format3D.clrExtrusionColor.ToSchemeColor();
                    }
                }

                if (this.Format3D.HasContourColor)
                {
                    shape3d.ContourColor = new A.ContourColor();
                    if (this.Format3D.clrContourColor.IsRgbColorModelHex)
                    {
                        shape3d.ContourColor.RgbColorModelHex = this.Format3D.clrContourColor.ToRgbColorModelHex();
                    }
                    else
                    {
                        shape3d.ContourColor.SchemeColor = this.Format3D.clrContourColor.ToSchemeColor();
                    }
                }

                if (this.Rotation3D.DistanceZ != 0m)
                {
                    shape3d.Z = SLA.SLDrawingTool.CalculateCoordinate(this.Rotation3D.DistanceZ);
                }

                if (this.Format3D.ExtrusionHeight != 0m)
                {
                    shape3d.ExtrusionHeight = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.ExtrusionHeight);
                }

                if (this.Format3D.ContourWidth != 0m)
                {
                    shape3d.ContourWidth = SLA.SLDrawingTool.CalculatePositiveCoordinate(this.Format3D.ContourWidth);
                }

                if (this.Format3D.Material != A.PresetMaterialTypeValues.WarmMatte)
                {
                    shape3d.PresetMaterial = this.Format3D.Material;
                }

                sp.Append(shape3d);
            }

            return sp;
        }

        internal SLShapeProperties Clone()
        {
            SLShapeProperties sp = new SLShapeProperties(this.listThemeColors);
            sp.HasTransform2D = this.HasTransform2D;
            sp.Transform2D = this.Transform2D.Clone();
            sp.HasGeometry = this.HasGeometry;
            sp.vPresetGeometry = this.vPresetGeometry;
            sp.Fill = this.Fill.Clone();
            sp.Outline = this.Outline.Clone();
            sp.EffectList = this.EffectList.Clone();
            sp.Rotation3D = this.Rotation3D.Clone();
            sp.Format3D = this.Format3D.Clone();

            return sp;
        }
    }
}
