using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Localization;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using System.Diagnostics.CodeAnalysis;

namespace SiGen.Layouts.Builders
{
    public class LayoutStringsBuilder : LayoutBuilderBase
    {

        public LayoutStringsBuilder(StringedInstrumentLayout layout, InstrumentLayoutConfiguration configuration) : base(layout, configuration)
        {
        }

        public override void BuildLayoutCore()
        {
            if (NumberOfStrings == 1)
            {
                if (Configuration.ScaleLength.Mode != ScaleLengthMode.Single)
                {
                    string? scaleMode = Texts.ResourceManager.GetString($"ScaleLengthMode.{Configuration.ScaleLength.Mode}", Texts.Culture);
                    AddWarning($"Scale length mode '{scaleMode}' not supported for single string.");
                }

                if (Measure.IsNullOrEmpty(Configuration.ScaleLength.SingleScale))
                {
                    AddError(Texts.SingleScaleNotConfigured);
                    return;
                }

                Layout.AddElement(CreateStringElement(0, Measure.Zero, Measure.Zero, Configuration.ScaleLength.SingleScale));
                return;
            }

            if (Configuration.ScaleLength.Mode == ScaleLengthMode.Multiscale &&
                    (
                        Configuration.ScaleLength.BassScale == null ||
                        Configuration.ScaleLength.TrebleScale == null
                    ))
            {
                if (Configuration.ScaleLength.BassScale == null)
                    AddError(string.Format(Texts.ScaleLengthSideNotConfigured, Texts.FingerboardSide_Bass));

                if (Configuration.ScaleLength.TrebleScale == null)
                    AddError(string.Format(Texts.ScaleLengthSideNotConfigured, Texts.FingerboardSide_Treble));
            }
            else if (Configuration.ScaleLength.Mode == ScaleLengthMode.PerString
                && !Configuration.StringConfigurations.All(x => x.ScaleLength != null))
            {
                for (int i = 0; i < Configuration.StringConfigurations.Count; i++)
                {
                    if (Configuration.StringConfigurations[i].ScaleLength == null)
                        AddError(string.Format(Texts.ScaleLengthStringNotConfigured, i + 1));
                }
            }

            if (Configuration.NutSpacing.CenterAlignment == LayoutCenterAlignment.Manual && !Configuration.NutSpacing.AlignmentRatio.HasValue)
            {
                AddError(Texts.StringSpacingManualAlignmentRatioNotSet, Texts.FingerboardEnd_Nut);
            }

            if (Configuration.BridgeSpacing.CenterAlignment == LayoutCenterAlignment.Manual && !Configuration.BridgeSpacing.AlignmentRatio.HasValue)
            {
                AddError(Texts.StringSpacingManualAlignmentRatioNotSet, Texts.FingerboardEnd_Bridge);
            }

            if (Configuration.NutSpacing.CenterAlignment == LayoutCenterAlignment.Manual &&
                Configuration.BridgeSpacing.CenterAlignment == LayoutCenterAlignment.Manual)
            {
                AddError(Texts.StringSpacingManualAlignmentBothEnds);
            }


            if (Messages.Any(x => x.Type == ValidationMessageType.Error))
                return;

            var stringPaths = CreateStringsPaths();

            if (stringPaths.Length == 0) return;

            ApplyMultiscaleAlignment(stringPaths);

            ApplyBassTrebleSkew(stringPaths);

            if (Configuration.NutSpacing.IsSymmetric ||
                Configuration.BridgeSpacing.IsSymmetric)
            {
                ApplySymmetry(stringPaths);
            }

            for (int i = 0; i < NumberOfStrings; i++)
            {
                //if (Configuration.StringConfigurations[i] is StringGroupConfiguration group)
                //{
                //    var offsetX = group.GetTotalSpacing() * -0.5d;

                //}
                //else
                //{
                //    Layout.AddElement(new StringElement(i, stringPaths[i]));
                //}
                Layout.AddElement(new StringElement(i, stringPaths[i]));
            }

            for (int i = 0; i < NumberOfStrings - 1; i++)
            {
                var string1 = stringPaths[i];
                var string2 = stringPaths[i + 1];

                var medianPath = new LinearPath(
                    (string1.Start + string2.Start) / 2d,
                    (string1.End + string2.End) / 2d
                );

                Layout.AddElement(new StringMedianElement(i, medianPath));
            }


        }

        private LinearPath[] CreateStringsPaths()
        {
            if(!GetStringPositions(FingerboardEnd.Nut, out var nutPositions))
                return [];

            if (!GetStringPositions(FingerboardEnd.Bridge, out var bridgePositions))
                return [];

            var bassScaleLength = GetScaleLength(FingerboardSide.Bass);
            var trebScaleLength = GetScaleLength(FingerboardSide.Treble);

            if (bassScaleLength == null || trebScaleLength == null)
            {
                AddError("Scale lengths are not configured properly.");
                return [];
            }


            ScaleLengthCalculationMethod lengthCalculationMethod = Configuration.ScaleLength.CalculationMethod;

            if (lengthCalculationMethod == ScaleLengthCalculationMethod.Auto)
            {
                lengthCalculationMethod = Configuration.ScaleLength.Mode == ScaleLengthMode.Single ? 
                    ScaleLengthCalculationMethod.AlongFingerboard : ScaleLengthCalculationMethod.AlongString;
            }

            //the scale length is applied vertically by default, so the string will end up a tiny bit longer due to the taper of the fingerboard
            //if we wish the string to be exaclty the length specified we need to calculate the base of the triangle
            if (Configuration.ScaleLength.CalculationMethod == lengthCalculationMethod)
            {
                bassScaleLength = AdjustScaleLengthForTaper(bassScaleLength, nutPositions[0], bridgePositions[0]);
                trebScaleLength = AdjustScaleLengthForTaper(trebScaleLength, nutPositions[^1], bridgePositions[^1]);
            }

            LinearPath[] stringPaths = new LinearPath[NumberOfStrings];
            var bassStrPath = stringPaths[0] = CreateStringPath(nutPositions[0], bridgePositions[0], bassScaleLength);
            var trebStrPath = stringPaths[^1] = CreateStringPath(nutPositions[^1], bridgePositions[^1], trebScaleLength);

            //if (Configuration.NutSpacing.CenterAlignment == LayoutCenterAlignment.SymmetricStrings ||
            //    Configuration.NutSpacing.CenterAlignment == LayoutCenterAlignment.SymmetricFingerboard)
            //{
            //    SetSymmetricStringPositions(FingerboardEnd.Nut, bassScaleLength, ref bassStrPath, trebScaleLength, ref trebStrPath, ref nutPositions, ref bridgePositions);
            //    stringPaths[0] = bassStrPath;
            //    stringPaths[^1] = trebStrPath;
            //}

            //if (Configuration.BridgeSpacing.CenterAlignment == LayoutCenterAlignment.SymmetricStrings ||
            //    Configuration.BridgeSpacing.CenterAlignment == LayoutCenterAlignment.SymmetricFingerboard)
            //{
            //    SetSymmetricStringPositions(FingerboardEnd.Bridge, bassScaleLength, ref bassStrPath, trebScaleLength, ref trebStrPath, ref nutPositions, ref bridgePositions);
            //    stringPaths[0] = bassStrPath;
            //    stringPaths[^1] = trebStrPath;
            //}

            if (NumberOfStrings > 2)
            {
                if (Configuration.ScaleLength.Mode == ScaleLengthMode.PerString)
                {
                    for (int i = 1; i < NumberOfStrings - 1; i++)
                    {
                        var scaleLength = GetStringConfig(i)!.ScaleLength!;
                        if (Configuration.ScaleLength.CalculationMethod == lengthCalculationMethod)
                            scaleLength = AdjustScaleLengthForTaper(scaleLength, nutPositions[i], bridgePositions[i]);
                        stringPaths[i] = CreateStringPath(nutPositions[i], bridgePositions[i], scaleLength);
                    }
                }
                else
                {
                    var nutLine = new LinearPath(bassStrPath.Start, trebStrPath.Start);
                    var bridgeLine = new LinearPath(bassStrPath.End, trebStrPath.End);

                    for (int i = 1; i < NumberOfStrings - 1; i++)
                    {
                        var nutX = nutPositions[i].NormalizedValue;
                        var bridgeX = bridgePositions[i].NormalizedValue;
                        var nutPos = nutLine.SnapToLine(new VectorD(nutX, 0), LinearPath.LineSnapDirection.Vertical, false);
                        var bridgePos = bridgeLine.SnapToLine(new VectorD(bridgeX, 0), LinearPath.LineSnapDirection.Vertical, false);

                        stringPaths[i] = new LinearPath(nutPos, bridgePos);
                    }
                }
            }

            return stringPaths;
        }

        private void ApplyMultiscaleAlignment(LinearPath[] stringPaths)
        {
            //var maxY = stringPaths.Max(x => x.Start.Y);
            var maxHeight = stringPaths.Max(x => x.Size.Y);

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var vDiff = maxHeight - stringPaths[i].Size.Y;
                var alignRatio = GetAlignmentRatio(i);
                var vOffset = MathD.Map(0, 1, vDiff * 0.5d, vDiff * -0.5d, alignRatio);
                stringPaths[i].Offset(0, vOffset);
            }
        }

        private void ApplyBassTrebleSkew(LinearPath[] stringPaths)
        {
            if (Measure.IsNullOrEmpty(Configuration.ScaleLength.BassTrebleSkew))
                return;

            //var maxLength = Measure.Max(Configuration.get)

            var maxX = MathD.Abs(MathD.Max(stringPaths[^1].Start.X, stringPaths[^1].End.X));
            var minX = MathD.Abs(MathD.Min(stringPaths[0].Start.X, stringPaths[0].End.X));
            maxX = MathD.Max(minX, maxX);
            var skewAmount = Configuration.ScaleLength.BassTrebleSkew.NormalizedValue;
            VectorD skewVector(VectorD vec)
            {
                var yAmount = MathD.Map(-maxX, maxX, skewAmount * 0.5, skewAmount * -0.5, vec.X);
                return new VectorD(vec.X, vec.Y + yAmount);
            }
            for (int i = 0; i < NumberOfStrings; i++)
            {
                stringPaths[i].Start = skewVector(stringPaths[i].Start);
                stringPaths[i].End = skewVector(stringPaths[i].End);
            }
        }

        private void ApplySymmetry(LinearPath[] stringPaths)
        {
            var bassPath = stringPaths[0];
            var trebPath = stringPaths[^1];

            void ApplyAtEnd(FingerboardEnd end)
            {
                bool symmetricFingerboard = Configuration.GetStringSpacing(end).CenterAlignment == LayoutCenterAlignment.SymmetricFingerboard;
                bool compensateForStrings = Configuration.Margin.CompensateForStrings;

                var posY = end == FingerboardEnd.Nut ?
                    MathD.Max(bassPath.Start.Y, trebPath.Start.Y) :
                    MathD.Min(bassPath.End.Y, trebPath.End.Y);
                var endLine = new LineD(0, posY);

                var p1 = bassPath.GetEquation().GetPointForY(posY);

                if (symmetricFingerboard)
                {
                    var bassPerp = bassPath.GetEquation().GetPerpendicular(bassPath.Start);

                    PreciseDouble totalMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Bass).NormalizedValue;
                    var stringWidth = Configuration.StringConfigurations[0]?.GetTotalWidth();
                    if (compensateForStrings && !Measure.IsNullOrEmpty(stringWidth))
                        totalMargin += stringWidth.NormalizedValue / 2d;

                    var marginOffset = bassPerp.Vector * totalMargin;
                    var edgeLine = LineD.FromPoints(bassPath.Start - marginOffset, bassPath.End - marginOffset);
                    
                    p1.X -= MathD.Abs(totalMargin);

                    if (edgeLine.Intersects(endLine, out var inter))
                        p1 = inter;
                }

                var p2 = trebPath.GetEquation().GetPointForY(posY);

                if (symmetricFingerboard)
                {
                    var trebPerp = trebPath.GetEquation().GetPerpendicular(trebPath.Start);

                    PreciseDouble totalMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Treble).NormalizedValue;
                    var stringWidth = Configuration.StringConfigurations[^1]?.GetTotalWidth();
                    if (compensateForStrings && !Measure.IsNullOrEmpty(stringWidth))
                        totalMargin += stringWidth!.NormalizedValue / 2d;

                    var marginOffset = trebPerp.Vector * totalMargin;
                    var edgeLine = LineD.FromPoints(trebPath.Start + marginOffset, trebPath.End + marginOffset);

                    p2.X += MathD.Abs(totalMargin);

                    if (edgeLine.Intersects(endLine, out var inter))
                        p2 = inter;
                }

                var width = MathD.Abs(p1.X - p2.X);
                var center = (p1 + p2) / 2d;
                center.Y = 0;

                for (int j = 0; j < stringPaths.Length; j++)
                {
                    if (end == FingerboardEnd.Nut)
                        stringPaths[j].Start = stringPaths[j].Start - center;
                    else
                        stringPaths[j].End = stringPaths[j].End - center;
                }
            }

            for (int iter = 0; iter < 5; iter++)
            {
                if (Configuration.NutSpacing.IsSymmetric)
                    ApplyAtEnd(FingerboardEnd.Nut);

                if (Configuration.BridgeSpacing.IsSymmetric)
                    ApplyAtEnd(FingerboardEnd.Bridge);
            }
            
        }

        //private void SetSymmetricStringPositions(FingerboardEnd end, 
        //    Measure bassScale, ref LinearPath bassStrPath, 
        //    Measure trebScale, ref LinearPath trebStrPath,
        //    ref Measure[] nutPositions, ref Measure[] bridgePositions)
        //{
        //    bool symmetricFingerboard = Configuration.GetStringSpacing(end).CenterAlignment == LayoutCenterAlignment.SymmetricFingerboard;
        //    bool compensateForStrings = Configuration.Margin.CompensateForStrings;

        //    for (int i = 0; i < 5; i++) // 5 iterations should be enough to adjust the bridge positions
        //    {
        //        var posY = end == FingerboardEnd.Nut ?
        //            MathD.Max(bassStrPath.Start.Y, trebStrPath.Start.Y) :
        //            MathD.Min(bassStrPath.End.Y, trebStrPath.End.Y);
        //        var endLine = new LineD(0, posY);

        //        var pt1 = bassStrPath.GetEquation().GetPointForY(posY);
        //        if (symmetricFingerboard)
        //        {
        //            var bassPerp = bassStrPath.GetEquation().GetPerpendicular(bassStrPath.Start);
        //            PreciseDouble totalMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Bass).NormalizedValue;
        //            if (compensateForStrings && !Measure.IsNullOrEmpty(Configuration.StringConfigurations[0].Gauge))
        //                totalMargin  += Configuration.StringConfigurations[0].Gauge!.Value / 2m;
        //            var marginOffset = bassPerp.Vector * totalMargin;
        //            var bassEdgePath = new LinearPath(bassStrPath.Start - marginOffset, bassStrPath.End - marginOffset);
                   
        //            var perpMargin = bassPerp.Vector * totalMargin;
        //            pt1.X -= MathD.Abs(perpMargin.X);

        //            //if (bassEdgePath.GetEquation().Intersects(endLine, out var inter))
        //            //{
        //            //    pt1 = inter;
        //            //}
        //        }
        //        var pt2 = trebStrPath.GetEquation().GetPointForY(posY);
        //        if (symmetricFingerboard)
        //        {
        //            var trebPerp = trebStrPath.GetEquation().GetPerpendicular(trebStrPath.Start);
        //            PreciseDouble totalMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Treble).NormalizedValue;
        //            if (compensateForStrings && !Measure.IsNullOrEmpty(Configuration.StringConfigurations[^1].Gauge))
        //                totalMargin += Configuration.StringConfigurations[^1].Gauge!.Value / 2m;

        //            var marginOffset = trebPerp.Vector * totalMargin;
        //            var trebEdgePath = new LinearPath(trebStrPath.Start + marginOffset, trebStrPath.End + marginOffset);

        //            var perpMargin = trebPerp.Vector * totalMargin;
        //            pt2.X += MathD.Abs(perpMargin.X);

        //            //if (trebEdgePath.GetEquation().Intersects(endLine, out var inter))
        //            //{
        //            //    pt2 = inter;
        //            //}
        //        }
        //        var centerX = (pt1.X + pt2.X) / 2d;
        //        var offset = centerX * -1d;
        //        if (i == 4)
        //        {

        //        }
        //        for (int j = 0; j < nutPositions.Length; j++)
        //        {
        //            if (end == FingerboardEnd.Nut)
        //                nutPositions[j] += Measure.FromNormalizedValue(LengthUnit.Mm, offset);
        //            else
        //                bridgePositions[j] += Measure.FromNormalizedValue(LengthUnit.Mm, offset);
        //        }

        //        bassStrPath = CreateStringPath(nutPositions[0], bridgePositions[0], bassScale);
        //        trebStrPath = CreateStringPath(nutPositions[^1], bridgePositions[^1], trebScale);
        //    }

            
        //}

        private Measure GetStringSpread(FingerboardEnd end)
        {
            var spacingConfig = Configuration.GetStringSpacing(end);
            if (spacingConfig.StringDistances.Count == 1)
                return spacingConfig.StringDistances[0] * (Configuration.NumberOfStrings - 1);

            return spacingConfig.StringDistances.Aggregate(Measure.Zero, (a, b) => a + b);
        }

        private bool GetStringPositions(FingerboardEnd end, [NotNullWhen(true)] out Measure[]? positions)
        {
            var spacingConfig = Configuration.GetStringSpacing(end);

            if (Configuration.NumberOfStrings == 1)
            {
                positions = [Measure.Zero];
                return true;
            }

            if (spacingConfig.StringDistances.Any() != true)
            {
                AddError(Texts.StringSpacingConfigInvalid, end);
                positions = null;
                return false;
            }
            //either one distance for all strings or one for each gap (one less than the number of strings)
            else if (spacingConfig.StringDistances.Count != 1 && 
                spacingConfig.StringDistances.Count != Configuration.NumberOfStrings - 1
            )
            {
                AddError(Texts.StringSpacingConfigInvalid, end);
                positions = null;
                return false;
            }

            positions = new Measure[Configuration.NumberOfStrings];

            var spacings = new List<Measure>();

            if (spacingConfig.StringDistances!.Count == 1)
            {
                if (spacingConfig.SpacingMode == StringSpacingMode.Proportional)
                {
                    if (Configuration.StringConfigurations.Count == 0 ||
                        !Configuration.StringConfigurations.All(x => !Measure.IsNullOrEmpty(x.GetTotalWidth())))
                        AddWarning("EqualSpacing is used but not all strings have gauge configured.");

                    var totalSpreadAdj = spacingConfig.StringDistances[0] * (Configuration.NumberOfStrings - 1);

                    for (int i = 0; i < Configuration.NumberOfStrings - 1; i++)
                    {
                        var gauge1 = GetStringConfig(i)?.GetTotalWidth();
                        if (Measure.IsNullOrEmpty(gauge1))
                        {
                            AddWarning($"String {i + 1} gauge is not configured, using 0mm. The equal spacing of strings cannot be calculated correctly.");
                            gauge1 = Measure.Cm(0);
                        }

                        var gauge2 = GetStringConfig(i + 1)?.GetTotalWidth();
                        if (Measure.IsNullOrEmpty(gauge2))
                            gauge2 = Measure.Cm(0);

                        var gaugeOffset = (gauge1 + gauge2) / 2m;
                        spacings.Add(gaugeOffset);
                        totalSpreadAdj -= gaugeOffset;
                    }

                    var avgSpacing = totalSpreadAdj / (NumberOfStrings - 1);
                    for (int i = 0; i < NumberOfStrings - 1; i++)
                        spacings[i] += avgSpacing;
                }
                else
                {
                    for (int i = 0; i < NumberOfStrings - 1; i++)
                        spacings.Add(spacingConfig.StringDistances[0]);
                }
            }
            else
            {
                spacings = spacingConfig.StringDistances;
            }

            var totalSpread = spacings.Aggregate(Measure.Zero, (a,b) => a + b);

            for (int i = 0; i < NumberOfStrings; i++)
            {
                if (i == 0)
                    positions[i] = Measure.Zero;
                else
                    positions[i] = positions[i - 1] + spacings[i - 1];
            }

            Measure centerOffset = Measure.Mm(0);

            switch (spacingConfig.CenterAlignment)
            {
                default:
                
                case LayoutCenterAlignment.SymmetricFingerboard:
                    //{
                    //    var bassMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Bass);
                    //    if (Configuration.Margin.CompensateForStrings &&
                    //        !Measure.IsNullOrEmpty(Configuration.StringConfigurations[0].Gauge))
                    //        bassMargin += Configuration.StringConfigurations[0].Gauge! / 2m;
                    //    var trebMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Treble);
                    //    if (Configuration.Margin.CompensateForStrings &&
                    //        !Measure.IsNullOrEmpty(Configuration.StringConfigurations[^1].Gauge))
                    //        trebMargin += Configuration.StringConfigurations[^1].Gauge! / 2m;


                    //    centerOffset = positions[^1] / 2m;
                    //    break;
                    //}
                case LayoutCenterAlignment.SymmetricStrings:
                case LayoutCenterAlignment.OuterStrings:
                    {
                        centerOffset = positions[^1] / 2m;
                        break;
                    }
                case LayoutCenterAlignment.MiddleStrings:
                    {
                        int idx1 = NumberOfStrings / 2;
                        int idx2 = idx1 - NumberOfStrings % 2;
                        var p1 = positions[idx2];
                        var p2 = positions[idx1];
                        centerOffset = p1 + (p2 - p1) / 2m;
                        break;
                    }
                case LayoutCenterAlignment.Fingerboard:
                    {
                        var hMargin = Configuration.Margin.GetMargin(end, FingerboardSide.Bass)
                            + Configuration.Margin.GetMargin(end, FingerboardSide.Treble);
                        if (Configuration.Margin.CompensateForStrings)
                        {
                            hMargin += (Configuration.GetString(0)?.GetTotalWidth() ?? Measure.Zero) / 2m;
                            hMargin += (Configuration.GetString(NumberOfStrings - 1)?.GetTotalWidth() ?? Measure.Zero) / 2m;
                        }
                        centerOffset = (positions[^1] + hMargin) / 2m;
                        break;
                    }
                case LayoutCenterAlignment.Manual:
                    var nutSpread = GetStringSpread(FingerboardEnd.Nut);
                    var bridgeSpread = GetStringSpread(FingerboardEnd.Bridge);
                    if (end == FingerboardEnd.Nut)
                    {
                        centerOffset = nutSpread / 2d;

                        if (bridgeSpread > nutSpread && spacingConfig.AlignmentRatio.HasValue &&
                            GetStringPositions(FingerboardEnd.Bridge, out var bridgePos))
                        {
                            var p1 = bridgePos[0];
                            var spreadDiff = bridgeSpread - nutSpread;
                            centerOffset = (bridgePos[0]* -1d) - spreadDiff * MathD.Clamp(spacingConfig.AlignmentRatio.Value);
                        }
                    } 
                    else
                    {
                        centerOffset = nutSpread / 2d;
                        //if (nutSpread > bridgeSpread && spacingConfig.AlignmentRatio.HasValue)
                        //{
                        //    var spreadDiff = nutSpread - bridgeSpread;
                        //    centerOffset = spreadDiff * spacingConfig.AlignmentRatio.Value;
                        //}
                            
                    }
                    break;
                //case LayoutCenterAlignment.Symmetric:
                //    var oppositeEnd = end == FingerboardEnd.Nut ? FingerboardEnd.Bridge : FingerboardEnd.Nut;
                    
                //    break;
            }

            for (int i = 0; i < positions.Length; i++)
                positions[i] -= centerOffset;

            return true;
        }

        private LinearPath CreateStringPath(Measure nutPos, Measure bridgePos, Measure scaleLength)
        {
            var p1 = new PointM(nutPos, scaleLength * 0.5m);
            var p2 = new PointM(bridgePos, scaleLength * -0.5m);
            return new LinearPath(p1.ToVector(), p2.ToVector());
        }

        private StringElement CreateStringElement(int index, Measure nutPos, Measure bridgePos, Measure scaleLength)
        {
            var p1 = new PointM(nutPos, scaleLength * 0.5m);
            var p2 = new PointM(bridgePos, scaleLength * -0.5m);
            return new StringElement(index, p1, p2);
        }

        private Measure AdjustScaleLengthForTaper(Measure scaleLength, Measure nutPos, Measure bridgePos)
        {
            var opp = Measure.Abs(nutPos - bridgePos);
            var theta = MathD.Asin(opp.NormalizedValue / scaleLength.NormalizedValue);
            return scaleLength * MathD.Cos(theta);
        }

        private Measure? GetScaleLength(FingerboardSide side)
        {
            switch (Configuration.ScaleLength.Mode)
            {
                default:
                case ScaleLengthMode.Single:
                    if (Measure.IsNullOrEmpty(Configuration.ScaleLength.SingleScale))
                        throw new ArgumentException(Texts.SingleScaleNotConfigured);
                    return Configuration.ScaleLength.SingleScale;
                case ScaleLengthMode.Multiscale:
                    {
                        var sl = side == FingerboardSide.Bass ? Configuration.ScaleLength.BassScale : Configuration.ScaleLength.TrebleScale;

                        if (Measure.IsNullOrEmpty(sl))
                        {
                            string? sideName = Texts.ResourceManager.GetString($"FingerboardSide.{side}", Texts.Culture);
                            AddError(string.Format(Texts.ScaleLengthSideNotConfigured, sideName));
                        }
                            
                        return sl;
                    }
                case ScaleLengthMode.PerString:
                    {
                        int stringIdx = side == FingerboardSide.Bass ? 0 : Configuration.NumberOfStrings - 1;
                        var sl = Configuration.StringConfigurations[stringIdx].ScaleLength;
                        if (Measure.IsNullOrEmpty(sl))
                            AddError(string.Format(Texts.ScaleLengthStringNotConfigured, stringIdx + 1));
                        return sl;
                    }
            }
        }

        private double GetAlignmentRatio(int stringIndex)
        {
            var strCfg = GetStringConfig(stringIndex);

            return strCfg?.MultiScaleRatio ?? Configuration.ScaleLength.MultiScaleRatio ?? 0.5d;
        }
    }
}
