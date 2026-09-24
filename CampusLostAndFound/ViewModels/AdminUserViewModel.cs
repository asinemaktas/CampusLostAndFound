namespace CampusLostAndFound.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsStudent { get; set; }

        public bool IsSecurity { get; set; }

        public bool IsAdmin { get; set; }
    }
}