using TedToolkit.Units;

[assembly: Units<double>(
    AmountOfSubstance = AmountOfSubstanceUnit.Centimol,
    ElectricCurrent = ElectricCurrentUnit.Abampere,
    Length = LengthUnit.Centimeter,
    Flag = UnitFlag.InternalUnit)]