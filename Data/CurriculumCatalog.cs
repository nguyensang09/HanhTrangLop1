using HanhTrangLop1.Models;

namespace HanhTrangLop1.Data;

public static class CurriculumCatalog
{
    public static readonly IReadOnlyList<CurriculumGroupDefinition> Groups =
    [
        Group("11111111-1111-1111-1111-111111111111", "chu-cai", "Chữ cái", "Nhận biết, phát âm, ghép và viết chữ cái tiếng Việt.", "spellcheck", "#ff8542", 1,
            Topic("aaaaaaaa-1111-1111-1111-111111111111", "kham-pha-chu", "Khám phá chữ cái", 1),
            Topic("aaaaaaaa-1111-1111-1111-111111111112", "chu-in-hoa", "Chữ in hoa", 2),
            Topic("aaaaaaaa-1111-1111-1111-111111111113", "chu-in-thuong", "Chữ in thường", 3),
            Topic("aaaaaaaa-1111-1111-1111-111111111114", "ghep-hoa-thuong", "Ghép chữ hoa - thường", 4),
            Topic("aaaaaaaa-1111-1111-1111-111111111115", "phan-biet-chu", "Phân biệt chữ gần giống", 5)),

        Group("22222222-2222-2222-2222-222222222222", "chu-so", "Chữ số", "Nhận biết hình dạng, tên gọi, thứ tự và cách viết số.", "pin", "#ffd45a", 2,
            Topic("aaaaaaaa-2222-2222-2222-222222222222", "so-0-9", "Số 0-9", 1),
            Topic("aaaaaaaa-2222-2222-2222-222222222223", "so-10-20", "Số 10-20", 2),
            Topic("aaaaaaaa-2222-2222-2222-222222222224", "thu-tu-so", "Thứ tự, trước và sau", 3),
            Topic("aaaaaaaa-2222-2222-2222-222222222225", "phan-biet-so", "Phân biệt số gần giống", 4),
            Topic("aaaaaaaa-2222-2222-2222-222222222226", "viet-so", "Viết chữ số", 5)),

        Group("33333333-3333-3333-3333-333333333333", "so-luong-toan", "Số lượng và toán học", "Đếm, so sánh, tách gộp và cộng bớt trực quan.", "calculate", "#46e6b3", 3,
            Topic("aaaaaaaa-3333-3333-3333-333333333333", "dem-so-luong", "Đếm số lượng", 1),
            Topic("aaaaaaaa-3333-3333-3333-333333333334", "tao-so-luong", "Tạo đúng số lượng", 2),
            Topic("aaaaaaaa-3333-3333-3333-333333333335", "ghep-so-luong", "Ghép số với lượng", 3),
            Topic("aaaaaaaa-3333-3333-3333-333333333336", "so-sanh", "So sánh nhiều - ít", 4),
            Topic("aaaaaaaa-3333-3333-3333-333333333337", "tach-gop", "Tách và gộp nhóm", 5),
            Topic("aaaaaaaa-3333-3333-3333-333333333338", "cong-bot", "Cộng và bớt trực quan", 6)),

        Group("44444444-4444-4444-4444-444444444444", "tu-duy-logic", "Tư duy logic", "Phân loại, quy luật, ghép bóng và tìm điểm khác biệt.", "extension", "#67b7dc", 4,
            Topic("aaaaaaaa-4444-4444-4444-444444444444", "phan-loai", "Phân loại", 1),
            Topic("aaaaaaaa-4444-4444-4444-444444444445", "quy-luat", "Quy luật hình ảnh", 2),
            Topic("aaaaaaaa-4444-4444-4444-444444444446", "ghep-bong", "Ghép bóng vật", 3),
            Topic("aaaaaaaa-4444-4444-4444-444444444447", "tim-khac-biet", "Tìm điểm khác biệt", 4)),

        Group("55555555-5555-5555-5555-555555555555", "ky-nang-song", "Kỹ năng sống và cảm xúc", "Tự phục vụ, an toàn, giao tiếp và nhận biết cảm xúc.", "volunteer_activism", "#c48af2", 5,
            Topic("aaaaaaaa-5555-5555-5555-555555555555", "tu-phuc-vu", "Tự phục vụ", 1),
            Topic("aaaaaaaa-5555-5555-5555-555555555556", "an-toan", "An toàn", 2),
            Topic("aaaaaaaa-5555-5555-5555-555555555557", "cam-xuc", "Nhận biết cảm xúc", 3),
            Topic("aaaaaaaa-5555-5555-5555-555555555558", "giao-tiep", "Giao tiếp và chia sẻ", 4)),

        Group("66666666-6666-6666-6666-666666666666", "ngon-ngu", "Tiền tập đọc và ngôn ngữ", "Vốn từ, nghe hiểu, kể chuyện và nhận biết âm vần.", "record_voice_over", "#ef6ea8", 6,
            Topic("aaaaaaaa-6666-6666-6666-666666666661", "von-tu", "Mở rộng vốn từ", 1),
            Topic("aaaaaaaa-6666-6666-6666-666666666662", "nghe-hieu", "Nghe hiểu", 2),
            Topic("aaaaaaaa-6666-6666-6666-666666666663", "ke-chuyen", "Kể chuyện", 3),
            Topic("aaaaaaaa-6666-6666-6666-666666666664", "am-van", "Làm quen âm và vần", 4)),

        Group("77777777-7777-7777-7777-777777777777", "hinh-dang-khong-gian", "Hình dạng và không gian", "Hình học cơ bản, vị trí, kích thước và định hướng.", "category", "#43b7a5", 7,
            Topic("aaaaaaaa-7777-7777-7777-777777777771", "hinh-dang", "Nhận biết hình dạng", 1),
            Topic("aaaaaaaa-7777-7777-7777-777777777772", "vi-tri", "Vị trí trong không gian", 2),
            Topic("aaaaaaaa-7777-7777-7777-777777777773", "kich-thuoc", "Lớn - nhỏ, dài - ngắn", 3),
            Topic("aaaaaaaa-7777-7777-7777-777777777774", "ghep-hinh", "Ghép và tạo hình", 4)),

        Group("88888888-8888-8888-8888-888888888888", "ghi-nho-tap-trung", "Ghi nhớ và tập trung", "Ghi nhớ vị trí, trình tự và thực hiện yêu cầu.", "psychology", "#f2a93b", 8,
            Topic("aaaaaaaa-8888-8888-8888-888888888881", "ghi-nho", "Ghi nhớ", 1),
            Topic("aaaaaaaa-8888-8888-8888-888888888882", "tap-trung", "Tập trung", 2),
            Topic("aaaaaaaa-8888-8888-8888-888888888883", "lam-theo-yeu-cau", "Làm theo yêu cầu", 3)),

        Group("99999999-9999-9999-9999-999999999999", "van-dong-tinh", "Vận động tinh", "Đi nét, tô tranh tạo hình, mê cung, nối điểm và thao tác khéo léo.", "gesture", "#e97852", 9,
            Topic("aaaaaaaa-9999-9999-9999-999999999991", "net-co-ban", "Nét cơ bản", 1),
            Topic("aaaaaaaa-9999-9999-9999-999999999995", "tao-hinh", "Tô tranh tạo hình", 2),
            Topic("aaaaaaaa-9999-9999-9999-999999999992", "noi-diem", "Nối điểm", 3),
            Topic("aaaaaaaa-9999-9999-9999-999999999993", "me-cung", "Mê cung", 4),
            Topic("aaaaaaaa-9999-9999-9999-999999999994", "kheo-tay", "Khéo tay", 5)),

        Group("10101010-1010-1010-1010-101010101010", "kham-pha", "Khám phá thế giới", "Con vật, cây cối, thời tiết, giao thông và môi trường.", "rocket_launch", "#db5fa5", 10,
            Topic("aaaaaaaa-1010-1010-1010-101010101011", "con-vat", "Con vật", 1),
            Topic("aaaaaaaa-1010-1010-1010-101010101012", "cay-co", "Cây cối", 2),
            Topic("aaaaaaaa-1010-1010-1010-101010101013", "thoi-tiet", "Thời tiết và trang phục", 3),
            Topic("aaaaaaaa-1010-1010-1010-101010101014", "giao-thong", "Phương tiện giao thông", 4))
    ];

    private static CurriculumGroupDefinition Group(
        string id,
        string code,
        string name,
        string description,
        string iconKey,
        string color,
        int sortOrder,
        params CurriculumTopicDefinition[] topics)
    {
        return new(Guid.Parse(id), code, name, description, iconKey, color, sortOrder, topics);
    }

    private static CurriculumTopicDefinition Topic(string id, string code, string name, int sortOrder)
    {
        return new(Guid.Parse(id), code, name, sortOrder);
    }
}

public record CurriculumGroupDefinition(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string IconKey,
    string Color,
    int SortOrder,
    IReadOnlyList<CurriculumTopicDefinition> Topics);

public record CurriculumTopicDefinition(Guid Id, string Code, string Name, int SortOrder);
