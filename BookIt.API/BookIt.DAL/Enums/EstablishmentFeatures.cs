namespace BookIt.DAL.Enums;

[Flags]
public enum EstablishmentFeatures
{
    None = 0,
    Parking = 1 << 0,
    Pool = 1 << 1,
    Beach = 1 << 2,
    Fishing = 1 << 3,
    Sauna = 1 << 4,
    Restaurant = 1 << 5,
    Smoking = 1 << 6,
    AccessibleForDisabled = 1 << 7,
    ElectricCarCharging = 1 << 8,
    Elevator = 1 << 9,
}