namespace Part3_Raceday.Models
{
    public class CategoriesPageViewModel
    {
        public ApiEvent Event { get; set; } = new();
        public List<ApiCategory> Categories { get; set; } = new();
    }
}