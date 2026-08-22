using System.ComponentModel.DataAnnotations;

namespace HanhTrangLop1.Models.ViewModels;

public class ChildProfileListViewModel
{
    public IReadOnlyList<ChildProfile> Children { get; set; } = [];
    public Guid? SelectedChildProfileId { get; set; }
}

public class ChildProfileFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên bé.")]
    [MaxLength(80, ErrorMessage = "Tên bé không quá 80 ký tự.")]
    public string Nickname { get; set; } = string.Empty;

    [Range(2000, 2999, ErrorMessage = "Năm sinh chưa phù hợp.")]
    public int? BirthYear { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhân vật.")]
    [MaxLength(100)]
    public string AvatarKey { get; set; } = "soc-nau";

    [Range(5, 60, ErrorMessage = "Thời lượng học nên từ 5 đến 60 phút.")]
    public int DailyLearningMinutes { get; set; } = 15;

    public bool SoundEnabled { get; set; } = true;
}
