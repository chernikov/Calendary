using Calendary.Model.Enums;

namespace Calendary.Api.Dtos
{
    public class PromptDto
    {
        public int? Id { get; set; }
        public int ThemeId { get; set; }
        public GenderEnum Gender { get; set; } = GenderEnum.Male;
        public string Text { get; set; } = string.Empty;
        public string ThemeName { get; set; } = string.Empty;    
    }
}
