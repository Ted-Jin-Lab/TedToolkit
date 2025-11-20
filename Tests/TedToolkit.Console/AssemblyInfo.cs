global using TedToolkit.Quantities;

using TedToolkit.QuantExtensions;
using UnitsNet;
using Length = UnitsNet.Length;

[assembly: NumberExtension<int, Length>]
[assembly: NumberExtension<int, Angle>]
[assembly: NumberExtension<double, Angle>]

[assembly:Quantities<double>(QuantitySystems.SI, "AbsorbedDose")]
