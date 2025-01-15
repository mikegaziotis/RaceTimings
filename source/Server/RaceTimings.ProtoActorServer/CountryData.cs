using RaceTimings.Messages;

namespace RaceTimings.ProtoActorServer;

public static class CountryData
    {
        public static readonly List<Country> Countries =
        [
            new Country { Id = "AFG", FullName = "Afghanistan", Colour = 0x00563F }, // Green (from flag)
            new Country { Id = "ALB", FullName = "Albania", Colour = 0xE41A1C }, // Red (from flag)
            new Country { Id = "DZA", FullName = "Algeria", Colour = 0x006233 }, // Green (from flag)
            new Country { Id = "ARG", FullName = "Argentina", Colour = 0x74ACDF }, // Light Blue (from flag)
            new Country { Id = "AUS", FullName = "Australia", Colour = 0xFFD700 }, // Gold (sports associations)

            new Country { Id = "AUT", FullName = "Austria", Colour = 0xED2939 }, // Red (national color)
            new Country { Id = "BRA", FullName = "Brazil", Colour = 0x009C3B }, // Green (sports and flag)
            new Country { Id = "BGD", FullName = "Bangladesh", Colour = 0x006A4E }, // Green (flag color)
            new Country { Id = "CAN", FullName = "Canada", Colour = 0xFF0000 }, // Red (symbolizes maple leaf on flag)
            new Country { Id = "CHN", FullName = "China", Colour = 0xFFDE00 }, // Yellow (stars from flag)

            new Country { Id = "COL", FullName = "Colombia", Colour = 0xFFD700 }, // Yellow (one of the most prominent colors of flag)
            new Country { Id = "CUB", FullName = "Cuba", Colour = 0x002A8F }, // Blue (flag stripes)
            new Country { Id = "DNK", FullName = "Denmark", Colour = 0xC60C30 }, // Red (from flag)
            new Country { Id = "EGY", FullName = "Egypt", Colour = 0x000000 }, // Black (national prominence)
            new Country { Id = "FRA", FullName = "France", Colour = 0x0055A4 }, // Blue (flag tricolor)

            new Country { Id = "DEU", FullName = "Germany", Colour = 0x000000 }, // Black (from flag)
            new Country { Id = "GRC", FullName = "Greece", Colour = 0x0D5EAF }, // Blue (flag stripes)
            new Country { Id = "IND", FullName = "India", Colour = 0xFF9933 }, // Orange (saffron in flag)
            new Country { Id = "IDN", FullName = "Indonesia", Colour = 0xFF0000 }, // Red (prominent color of flag)
            new Country { Id = "IRL", FullName = "Ireland", Colour = 0x169B62 }, // Green (flag tricolor)

            new Country { Id = "ISR", FullName = "Israel", Colour = 0x0038B8 }, // Blue (flag color)
            new Country { Id = "ITA", FullName = "Italy", Colour = 0x009246 }, // Green (flag first stripe)
            new Country { Id = "JPN", FullName = "Japan", Colour = 0xBC002D }, // Red (sun in flag)
            new Country { Id = "MEX", FullName = "Mexico", Colour = 0x006341 }, // Green (flag)
            new Country { Id = "NLD", FullName = "Netherlands", Colour = 0x21468B }, // Blue (flag stripe)

            new Country { Id = "NZL", FullName = "New Zealand", Colour = 0x00247D }, // Blue (sports colors)
            new Country { Id = "NOR", FullName = "Norway", Colour = 0xEF2B2D }, // Red (flag prominence)
            new Country { Id = "POL", FullName = "Poland", Colour = 0xDC143C }, // Red (flag bottom)
            new Country { Id = "PRT", FullName = "Portugal", Colour = 0x006600 }, // Green (flag)
            new Country { Id = "ESP", FullName = "Spain", Colour = 0xAA151B },
            
            new Country { Id = "CHE", FullName = "Switzerland", Colour = 0xFF0000 }, // Red (Flag background)
            new Country { Id = "RUS", FullName = "Russia", Colour = 0xB22234 }, // Blue (middle stripe from flag)
            new Country { Id = "BEL", FullName = "Belgium", Colour = 0x000000 }, // Black (left stripe of flag)
            new Country { Id = "USA", FullName = "United States", Colour = 0x0039A6 }, // Red (stripes from flag)
            new Country { Id = "SWE", FullName = "Sweden", Colour = 0xFFD700 }, // Yellow (cross from flag)
            
            new Country { Id = "FIN", FullName = "Finland", Colour = 0x003580 }, // Blue (cross from flag)
            new Country { Id = "JAM", FullName = "Jamaica", Colour = 0x007847 }, // Green (dominant color in flag)
            new Country { Id = "ROU", FullName = "Romania", Colour = 0xFFD700 }, // Yellow (center stripe of flag)
            new Country { Id = "TUR", FullName = "Turkey", Colour = 0xE30A17 }, // Red (from flag background)
            new Country { Id = "NGA", FullName = "Nigeria", Colour = 0x008753 } // Green (flag's vertical stripes)
        ];
    }