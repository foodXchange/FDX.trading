using MudBlazor;

namespace FoodX.Admin.Themes
{
    public static class FoodXTheme
    {
        public static readonly MudTheme Theme = new()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#2E7D32", // Green for food/agriculture
                Secondary = "#FF6F00", // Orange for energy/warmth
                Tertiary = "#1565C0", // Blue for trust
                AppbarBackground = "#2E7D32",
                Background = "#f5f5f5",
                Surface = "#ffffff",
                DrawerBackground = "#ffffff",
                DrawerText = "rgba(0,0,0, 0.87)",
                DrawerIcon = "rgba(0,0,0, 0.54)",
                AppbarText = "#ffffff",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.60)",
                ActionDefault = "#757575",
                ActionDisabled = "rgba(0,0,0, 0.26)",
                ActionDisabledBackground = "rgba(0,0,0, 0.12)",
                Divider = "rgba(0,0,0, 0.12)",
                DividerLight = "rgba(0,0,0, 0.06)",
                TableStriped = "rgba(0,0,0, 0.02)",
                TableHover = "rgba(0,0,0, 0.04)",
                Info = "#3498db",
                Success = "#27ae60",
                Warning = "#f39c12",
                Error = "#e74c3c",
                Dark = "#27272f",
                LinesDefault = "#E0E0E0",
                LinesInputs = "#BDBDBD",
                TextDisabled = "rgba(0,0,0, 0.38)"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#66BB6A", // Lighter green for dark mode
                Secondary = "#FFB74D", // Lighter orange for dark mode
                Tertiary = "#42A5F5", // Lighter blue for dark mode
                AppbarBackground = "#1e1e2e",
                Background = "#121212",
                Surface = "#1e1e2e",
                DrawerBackground = "#1e1e2e",
                DrawerText = "rgba(255,255,255, 0.87)",
                DrawerIcon = "rgba(255,255,255, 0.54)",
                AppbarText = "#ffffff",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.60)",
                ActionDefault = "#9E9E9E",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                Divider = "rgba(255,255,255, 0.12)",
                DividerLight = "rgba(255,255,255, 0.06)",
                TableStriped = "rgba(255,255,255, 0.02)",
                TableHover = "rgba(255,255,255, 0.04)",
                Info = "#5dade2",
                Success = "#52c77e",
                Warning = "#f5b041",
                Error = "#ec7063",
                Dark = "#27272f",
                LinesDefault = "#424242",
                LinesInputs = "#616161",
                TextDisabled = "rgba(255,255,255, 0.38)"
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "4px",
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "300px",
                AppbarHeight = "64px"
            }
        };
    }
}