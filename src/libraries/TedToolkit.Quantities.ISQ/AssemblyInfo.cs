using TedToolkit.Quantities;

[assembly: Quantities<double>(QuantitySystems.ALL, 
    "Area",
    Flag = UnitFlag.GenerateExtensionProperties)]