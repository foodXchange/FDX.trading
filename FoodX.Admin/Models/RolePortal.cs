using MudBlazor;

namespace FoodX.Admin.Models
{
    public class RolePortal
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public Color Color { get; set; }
        public string GradientClass { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public bool IsSpecial { get; set; } = false;
        public string? SpecialStyle { get; set; }
        public string? AlertMessage { get; set; }
    }
}