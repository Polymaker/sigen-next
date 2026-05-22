using Avalonia.Data.Converters;
using SiGen.Data.Common;
using SiGen.Export;
using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Localization;
using SiGen.Measuring;
using SiGen.Physics;
using SiGen.Settings;
using SiGen.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Converters
{
    public class EnumValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ScaleLengthMode scaleLengthMode)
            {
                string key = $"Editor.ScaleLengthMode.{scaleLengthMode}.Tooltip";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is StringSpacingMode spacingMode)
            {
                string key = $"StringSpacingMode.{spacingMode}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is LayoutCenterAlignment centerAlignment)
            {
                string key = $"LayoutCenterAlignment.{centerAlignment}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is MarginMode marginMode)
            {
                string key = $"Editor.MarginMode.{marginMode}";
                return Lang.Resources.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is InstrumentType instrument)
            {
                string key = $"InstrumentType.{instrument}";
                return Lang.Resources.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is Temperament temperament)
            {
                string key = $"Temperament.{temperament}";
                return Texts.ResourceManager.GetString(key, Texts.Culture) ?? key;
            }
            else if (value is TensionBalanceState tensionBalanceState)
            {
                string key = $"TensionBalanceState.{tensionBalanceState}";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is StringMaterialType materialType)
            {
                string key = $"StringMaterialType.{materialType}";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is AppTheme theme)
            {
                string key = $"AppTheme.{theme}";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is AppLanguage language)
            {

                if (language == AppLanguage.System)
                {
                    var systemText = Lang.Resources.ResourceManager.GetString("AppLanguage.System", Lang.Resources.Culture) ?? "System";
                    string languageName = CultureInfo.InstalledUICulture.NativeName;
                    if (languageName.Contains('('))
                        languageName = languageName.Substring(0, languageName.IndexOf('(')).Trim();
                    return $"{systemText} ({languageName})";
                }
                //CultureInfo.InstalledUICulture.DisplayName
                string key = $"AppLanguage.{language}";
                
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is UnitSystem unitSystem)
            {
                string key = $"UnitSystem.{unitSystem}";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is LayoutViewerPreset viewerPreset)
            {
                string key = $"LayoutViewerPreset.{viewerPreset}";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is PdfPageOrientation pageOrientation)
            {
                string key = $"PdfPageOrientation.{pageOrientation}";
                return Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? key;
            }
            else if (value is PdfPaperSize paperSize)
            {
                string key = $"PdfPaperSize.{paperSize.Name}";
                string label = Lang.Resources.ResourceManager.GetString(key, Lang.Resources.Culture) ?? paperSize.Name;
                return $"{label} ({paperSize.Width.ToStringFormatted(useAbbreviation: true)} x {paperSize.Height.ToStringFormatted(useAbbreviation: true)})";
            }
            return value?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
