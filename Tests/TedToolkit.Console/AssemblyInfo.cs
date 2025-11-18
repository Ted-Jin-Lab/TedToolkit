using TedToolkit.QuantExtensions;
using TedToolkit.Quantities;
using UnitsNet;

[assembly: NumberExtension<int, Length>]
[assembly: NumberExtension<int, Angle>]
[assembly: NumberExtension<double, Angle>]

[assembly:Quantities<double>(QuantitySystems.SI, "Length")]
