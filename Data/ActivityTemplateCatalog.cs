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
            ["kham-pha-chu"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.ListenAndChoose, InteractionTypes.DragDrop),
            ["chu-in-hoa"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose, InteractionTypes.Matching),
            ["chu-in-thuong"] = RuleWithTracing(InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose, InteractionTypes.Matching),
            ["ghep-hoa-thuong"] = Rule(InteractionTypes.Matching, InteractionTypes.DragDrop),
            ["phan-biet-chu"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect),

            ["so-0-9"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect, InteractionTypes.ListenAndChoose, InteractionTypes.Counting),
            ["so-10-20"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.Counting, InteractionTypes.Ordering),
            ["thu-tu-so"] = Rule(InteractionTypes.Ordering, InteractionTypes.DragDrop),
            ["phan-biet-so"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect),
            ["viet-so"] = RuleWithTracing(),

            ["dem-so-luong"] = Rule(InteractionTypes.Counting, InteractionTypes.SingleChoice, InteractionTypes.ListenAndChoose),
            ["tao-so-luong"] = Rule(InteractionTypes.QuantityBuilder),
            ["ghep-so-luong"] = Rule(InteractionTypes.DragDrop, InteractionTypes.Matching),
            ["so-sanh"] = Rule(InteractionTypes.Comparison),
            ["tach-gop"] = Rule(InteractionTypes.QuantityBuilder, InteractionTypes.DragDrop),
            ["cong-bot"] = Rule(InteractionTypes.QuantityBuilder, InteractionTypes.SingleChoice),

            ["phan-loai"] = Rule(InteractionTypes.Classification),
            ["quy-luat"] = Rule(InteractionTypes.Ordering, InteractionTypes.SingleChoice),
            ["ghep-bong"] = Rule(InteractionTypes.Matching, InteractionTypes.DragDrop),
            ["tim-khac-biet"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.MultiSelect),

            ["tu-phuc-vu"] = Rule(InteractionTypes.Ordering, InteractionTypes.StoryChoice),
            ["an-toan"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Classification),
            ["cam-xuc"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.SingleChoice, InteractionTypes.Classification),
            ["giao-tiep"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.ListenAndChoose, InteractionTypes.SingleChoice),

            ["von-tu"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.Matching, InteractionTypes.Classification),
            ["nghe-hieu"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.StoryChoice),
            ["ke-chuyen"] = Rule(InteractionTypes.StoryChoice, InteractionTypes.Ordering),
            ["am-van"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.Matching, InteractionTypes.SingleChoice),

            ["hinh-dang"] = Rule(InteractionTypes.SingleChoice, InteractionTypes.Matching, InteractionTypes.Classification),
            ["vi-tri"] = Rule(InteractionTypes.DragDrop, InteractionTypes.SingleChoice, InteractionTypes.StoryChoice),
            ["kich-thuoc"] = Rule(InteractionTypes.Comparison, InteractionTypes.SingleChoice),
            ["ghep-hinh"] = Rule(InteractionTypes.DragDrop, InteractionTypes.Matching),

            ["ghi-nho"] = Rule(InteractionTypes.Matching, InteractionTypes.Ordering, InteractionTypes.MultiSelect),
            ["tap-trung"] = Rule(InteractionTypes.MultiSelect, InteractionTypes.SingleChoice),
            ["lam-theo-yeu-cau"] = Rule(InteractionTypes.ListenAndChoose, InteractionTypes.Ordering, InteractionTypes.StoryChoice),

            ["net-co-ban"] = RuleWithTracing(),
            ["noi-diem"] = RuleWithTracing(),
            ["me-cung"] = Rule(InteractionTypes.DragDrop),
            ["kheo-tay"] = Rule(InteractionTypes.Ordering, InteractionTypes.DragDrop),

            ["con-vat"] = Rule(InteractionTypes.Classification, InteractionTypes.ListenAndChoose, InteractionTypes.StoryChoice),
            ["cay-co"] = Rule(InteractionTypes.Classification, InteractionTypes.Ordering, InteractionTypes.StoryChoice),
            ["thoi-tiet"] = Rule(InteractionTypes.Classification, InteractionTypes.StoryChoice, InteractionTypes.SingleChoice),
            ["giao-thong"] = Rule(InteractionTypes.Classification, InteractionTypes.ListenAndChoose, InteractionTypes.StoryChoice)
        };

    public static ActivityTemplateDefinition? Find(string interactionType) =>
        Templates.FirstOrDefault(x => x.InteractionType == interactionType);

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
