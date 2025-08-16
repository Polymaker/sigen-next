using SiGen.Data.Common;
using System.Text.Json.Serialization;

namespace SiGen.Layouts.Configuration
{
    public class StringMaterialConfiguration
    {
        /// <summary>
        /// Modulus of Elasticity (MoE) of the string material, in gigapascals (GPa).
        /// Used for fret compensation and tension calculations.
        /// </summary>
        [JsonPropertyName("MoE")]
        public double? ModulusOfElasticity { get; set; }

        /// <summary>
        /// Unit weight of the string material, in pounds per cubic inch (lbs/in³).
        /// Used for calculating string tension and compensation.
        /// </summary>
        public double? UnitWeight { get; set; }

        /// <summary>
        /// Diameter of the string core, in millimeters.
        /// Used for physical modeling and compensation calculations.
        /// </summary>
        public double? CoreDiameter { get; set; }

        /// <summary>
        /// The material and style type of the string (e.g., SteelWound, NylonPlain).
        /// Used for display and rendering, and to select physical properties.
        /// </summary>
        public StringMaterialType? MaterialType { get; set; }
    }
}
