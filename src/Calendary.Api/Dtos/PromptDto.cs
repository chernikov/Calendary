using Calendary.Model.Enums;

namespace Calendary.Api.Dtos
{
    public class PromptDto
    {
        public int? Id { get; set; }
        public int ThemeId { get; set; }
        public AgeGenderEnum AgeGender { get; set; } = AgeGenderEnum.Male;
        public string Text { get; set; } = string.Empty;
        public string ThemeName { get; set; } = string.Empty;
        public List<PromptSeedDto> Seeds { get; set; } = [];
    }
}
