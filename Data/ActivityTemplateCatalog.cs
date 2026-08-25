using HanhTrangLop1.Models;

namespace HanhTrangLop1.Data;

public static class ActivityTemplateCatalog
{
    public static readonly IReadOnlyList<ActivityTemplateDefinition> Templates =
    [
        Template(InteractionTypes.SingleChoice, "Chọn một đáp án", "Chọn đúng một phương án từ 2-5 lựa chọn.", "touch_app", "Con hãy chọn một đáp án đúng.", "Con chọn đáp án phù hợp nhé."),
        Template(InteractionTypes.MultiSelect, "Chọn nhiều đáp án", "Chọn tất cả phương án thỏa điều kiện.", "checklist", "Con hãy chọn tất cả đáp án đúng.", "Những đáp án nào phù hợp?"),
        Template(InteractionTypes.ListenAndChoose, "Nghe và chọn", "Nghe âm thanh rồi chọn một đáp án.", "hearing", "Con nghe kỹ rồi chọn đáp án nhé.", "Con vừa nghe thấy gì?", requiresAudio: true),
        Template(InteractionTypes.DragDrop, "Kéo vào vùng đích", "Kéo một phương án vào vùng đích có tên rõ ràng.", "pan_tool", "Con kéo đáp án đúng vào vùng đích.", "Vật nào thuộc vùng này?"),
        Template(InteractionTypes.Matching, "Nối cặp", "Ghép từng mục bên trái với một mục bên phải.", "conversion_path", "Con nối các cặp phù hợp với nhau.", "Con hãy hoàn thành tất cả cặp nối."),
        Template(InteractionTypes.Ordering, "Sắp xếp thứ tự", "Sắp xếp từ 2 mục trở lên theo thứ tự đúng.", "sort", "Con sắp xếp các mục theo đúng thứ tự.", "Thứ tự đúng là gì?"),
        Template(InteractionTypes.Counting, "Đếm đồ vật", "Hiển thị một nhóm đồ vật và chọn số lượng đúng.", "counter_1", "Con chạm để đếm rồi chọn số đúng.", "Có bao nhiêu đồ vật?"),
        Template(InteractionTypes.QuantityBuilder, "Tạo đúng số lượng", "Thêm hoặc bớt đồ vật để tạo số lượng mục tiêu.", "shopping_basket", "Con tạo đúng số lượng được yêu cầu.", "Con cần tạo bao nhiêu đồ vật?"),
        Template(InteractionTypes.Comparison, "So sánh hai nhóm", "So sánh số lượng nhóm A, nhóm B hoặc bằng nhau.", "compare_arrows", "Con quan sát và chọn nhóm có nhiều hơn.", "Nhóm nào có nhiều đồ vật hơn?"),
        Template(InteractionTypes.Classification, "Phân loại", "Đưa từng vật vào đúng một trong ít nhất hai nhóm.", "category", "Con xếp từng vật vào đúng nhóm.", "Mỗi vật thuộc nhóm nào?"),
        Template(InteractionTypes.StoryChoice, "Nghe truyện và chọn", "Nghe đoạn kể, xem tranh rồi trả lời một câu hỏi.", "auto_stories", "Con nghe câu chuyện rồi chọn đáp án.", "Điều gì xảy ra trong câu chuyện?", requiresAudio: true, requiresImage: true)
    ];

    private static readonly IReadOnlyDictionary<string, TopicActivityRule> TopicRules =
        new Dictionary<string, TopicActivityRule>(StringComparer.OrdinalIgnoreCase)
        {
            ["kham-pha-chu"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.ListenAndChoose, InteractionTypes.DragDrop, InteractionTypes.StoryChoice),
            ["chu-in-hoa"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose, InteractionTypes.Matching, InteractionTypes.DragDrop),
            ["chu-in-thuong"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose, InteractionTypes.Matching, InteractionTypes.DragDrop),
            ["ghep-hoa-thuong"] = Rule(InteractionTypes.Matching, InteractionTypes.DragDrop, InteractionTypes.SingleChoice),
            ["phan-biet-chu"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.Matching, InteractionTypes.ListenAndChoose),

            ["so-0-9"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.ListenAndChoose, InteractionTypes.Counting, InteractionTypes.Ordering),
            ["so-10-20"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.Counting, InteractionTypes.Ordering, InteractionTypes.MultiSelect, InteractionTypes.ListenAndChoose),
            ["thu-tu-so"] = Rule(InteractionTypes.Ordering, InteractionTypes.DragDrop, InteractionTypes.SingleChoice, InteractionTypes.MultiSelect),
            ["phan-biet-so"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.Matching),
            ["viet-so"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.Matching),

            ["dem-so-luong"] = Rule(InteractionTypes.Counting, InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose, InteractionTypes.QuantityBuilder),
            ["tao-so-luong"] = Rule(InteractionTypes.QuantityBuilder, InteractionTypes.Counting, InteractionTypes.SingleChoice),
            ["ghep-so-luong"] = Rule(InteractionTypes.DragDrop, InteractionTypes.Matching, InteractionTypes.SingleChoice),
            ["so-sanh"] = Rule(InteractionTypes.Comparison, InteractionTypes.SingleChoice, InteractionTypes.DragDrop),
            ["tach-gop"] = Rule(InteractionTypes.QuantityBuilder, InteractionTypes.DragDrop, InteractionTypes.SingleChoice, InteractionTypes.Counting),
            ["cong-bot"] = Rule(InteractionTypes.QuantityBuilder, InteractionTypes.SingleChoice, InteractionTypes.Counting, InteractionTypes.DragDrop),

            ["phan-loai"] = Rule(InteractionTypes.Classification, InteractionTypes.Matching, InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.DragDrop),
            ["quy-luat"] = Rule(InteractionTypes.Ordering, InteractionTypes.SingleChoice, InteractionTypes.Matching, InteractionTypes.DragDrop),
            ["ghep-bong"] = Rule(InteractionTypes.Matching, InteractionTypes.DragDrop, InteractionTypes.SingleChoice),
            ["tim-khac-biet"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.Classification),

            ["tu-phuc-vu"] = Rule(InteractionTypes.Ordering, InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Matching),
            ["an-toan"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Classification, InteractionTypes.MultiSelect, InteractionTypes.Matching),
            ["cam-xuc"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Classification, InteractionTypes.Matching),
            ["giao-tiep"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.ListenAndChoose, InteractionTypes.SingleChoice, InteractionTypes.Matching, InteractionTypes.Ordering),

            ["von-tu"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.Matching, InteractionTypes.Classification, InteractionTypes.SingleChoice, InteractionTypes.MultiSelect),
            ["nghe-hieu"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Ordering),
            ["ke-chuyen"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.Ordering, InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose),
            ["am-van"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.Matching, InteractionTypes.SingleChoice, InteractionTypes.MultiSelect),
            ["doc-hieu"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose, InteractionTypes.Ordering, InteractionTypes.MultiSelect),

            ["hinh-dang"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.Matching, InteractionTypes.Classification, InteractionTypes.MultiSelect, InteractionTypes.DragDrop),
            ["vi-tri"] = Rule(InteractionTypes.DragDrop, InteractionTypes.SingleChoice, InteractionTypes.StoryChoice, InteractionTypes.Matching),
            ["kich-thuoc"] = Rule(InteractionTypes.Comparison, InteractionTypes.SingleChoice, InteractionTypes.Matching),
            ["ghep-hinh"] = Rule(InteractionTypes.DragDrop, InteractionTypes.Matching, InteractionTypes.SingleChoice),

            ["ghi-nho"] = Rule(InteractionTypes.Matching, InteractionTypes.Ordering, InteractionTypes.MultiSelect, InteractionTypes.SingleChoice),
            ["tap-trung"] = Rule(InteractionTypes.MultiSelect, InteractionTypes.SingleChoice, InteractionTypes.Counting),
            ["lam-theo-yeu-cau"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.Ordering, InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.DragDrop),

            ["net-co-ban"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.Matching),
            ["tao-hinh"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.Matching),
            ["noi-diem"] = RuleWithTracing(InteractionTypes.DragDrop, InteractionTypes.Matching),
            ["me-cung"] = Rule(InteractionTypes.DragDrop, InteractionTypes.Ordering),
            ["kheo-tay"] = Rule(InteractionTypes.Ordering, InteractionTypes.DragDrop, InteractionTypes.SingleChoice, InteractionTypes.Matching),

            ["con-vat"] = Rule(InteractionTypes.Classification, InteractionTypes.ListenAndChoose, InteractionTypes.StoryChoice, InteractionTypes.Matching, InteractionTypes.MultiSelect, InteractionTypes.SingleChoice),
            ["cay-co"] = Rule(InteractionTypes.Classification, InteractionTypes.Ordering, InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Matching),
            ["thoi-tiet"] = Rule(InteractionTypes.Classification, InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Matching),
            ["giao-thong"] = Rule(InteractionTypes.Classification, InteractionTypes.ListenAndChoose, InteractionTypes.StoryChoice, InteractionTypes.Matching, InteractionTypes.DragDrop, InteractionTypes.SingleChoice)
        };

    public static ActivityTemplateDefinition? Find(string interactionType) =>
        Templates.FirstOrDefault(x => x.InteractionType == interactionType);

    public static string GetDisplayName(string? interactionType) =>
        interactionType == InteractionTypes.Tracing
            ? "Tô theo nét"
            : Templates.FirstOrDefault(x => x.InteractionType == interactionType)?.Name ?? "Không xác định";

    public static TopicActivityRule ForTopic(string? topicCode) =>
        topicCode is not null && TopicRules.TryGetValue(topicCode, out var rule) ? rule : Rule();

    public static bool IsAllowed(string? topicCode, string interactionType) =>
        ForTopic(topicCode).InteractionTypes.Contains(interactionType, StringComparer.OrdinalIgnoreCase);

    public static bool IsItemAllowed(LearningItem item)
    {
        var rule = ForTopic(item.Topic?.Code);
        return item.InteractionType == InteractionTypes.Tracing
            ? rule.AllowsTracing
            : rule.InteractionTypes.Contains(item.InteractionType, StringComparer.OrdinalIgnoreCase);
    }

    private static ActivityTemplateDefinition Template(
        string interactionType,
        string name,
        string description,
        string iconKey,
        string defaultInstruction,
        string defaultPrompt,
        bool requiresAudio = false,
        bool requiresImage = false) =>
        new(interactionType, name, description, iconKey, defaultInstruction, defaultPrompt, requiresAudio, requiresImage);

    private static TopicActivityRule Rule(params string[] interactionTypes) => new(interactionTypes, false);
    private static TopicActivityRule RuleWithTracing(params string[] interactionTypes) => new(interactionTypes, true);
}

public record ActivityTemplateDefinition(
    string InteractionType,
    string Name,
    string Description,
    string IconKey,
    string DefaultInstruction,
    string DefaultPrompt,
    bool RequiresAudio,
    bool RequiresImage);

public record TopicActivityRule(IReadOnlyList<string> InteractionTypes, bool AllowsTracing);
