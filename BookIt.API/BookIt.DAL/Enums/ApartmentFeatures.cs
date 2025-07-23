namespace BookIt.DAL.Enums;

[Flags]
public enum ApartmentFeatures
{
    None = 0,
    FreeWifi = 1 << 0,
    AirConditioning = 1 << 1,
    Breakfast = 1 << 2,
    Kitchen = 1 << 3,
    TV = 1 << 4,
    Balcony = 1 << 5,
    Bathroom = 1 << 6,
    PetsAllowed = 1 << 7,
}