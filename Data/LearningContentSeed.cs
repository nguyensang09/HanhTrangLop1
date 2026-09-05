using HanhTrangLop1.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HanhTrangLop1.Data;

public static class LearningContentSeed
{
    private static readonly IReadOnlyDictionary<string, string> ObservationPhotos =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Fruits & Vegetables
            ["Táo"] = "/images/photos/flashcard-apple.jpg",
            ["Quả táo"] = "/images/photos/flashcard-apple.jpg",
            ["Trái táo"] = "/images/photos/flashcard-apple.jpg",
            ["🍎"] = "/images/photos/flashcard-apple.jpg",
            ["Cam"] = "/images/photos/flashcard-orange.jpg",
            ["Quả cam"] = "/images/photos/flashcard-orange.jpg",
            ["Trái cam"] = "/images/photos/flashcard-orange.jpg",
            ["🍊"] = "/images/photos/flashcard-orange.jpg",
            ["Cà rốt"] = "/images/photos/flashcard-carrot.jpg",
            ["Củ cà rốt"] = "/images/photos/flashcard-carrot.jpg",
            ["🥕"] = "/images/photos/flashcard-carrot.jpg",
            ["Bắp cải"] = "/images/photos/flashcard-cabbage.jpg",
            ["Rau bắp cải"] = "/images/photos/flashcard-cabbage.jpg",
            ["Chuối"] = "/images/photos/banana.jpg",
            ["Quả chuối"] = "/images/photos/banana.jpg",
            ["Dâu"] = "/images/pictograms/strawberry.svg",
            ["Dâu tây"] = "/images/pictograms/strawberry.svg",
            ["Quả dâu tây"] = "/images/pictograms/strawberry.svg",
            ["🍓"] = "/images/pictograms/strawberry.svg",
            ["Trái cây"] = "/images/photos/flashcard-apple.jpg",
            ["Rau củ"] = "/images/photos/flashcard-carrot.jpg",

            // Animals
            ["Mèo"] = "/images/photos/cat.jpg",
            ["Con mèo"] = "/images/photos/cat.jpg",
            ["Chú mèo"] = "/images/photos/cat.jpg",
            ["🐱"] = "/images/photos/cat.jpg",
            ["Chó"] = "/images/photos/dog.jpg",
            ["Con chó"] = "/images/photos/dog.jpg",
            ["Chú chó"] = "/images/photos/dog.jpg",
            ["🐶"] = "/images/photos/dog.jpg",
            ["Vịt"] = "/images/photos/duck.jpg",
            ["Con vịt"] = "/images/photos/duck.jpg",
            ["Chú vịt"] = "/images/photos/duck.jpg",
            ["🦆"] = "/images/photos/duck.jpg",
            ["Cá"] = "/images/photos/fish.jpg",
            ["Con cá"] = "/images/photos/fish.jpg",
            ["Chú cá"] = "/images/photos/fish.jpg",
            ["🐟"] = "/images/photos/fish.jpg",
            ["Tôm"] = "/images/photos/flashcard-shrimp.jpg",
            ["Con tôm"] = "/images/photos/flashcard-shrimp.jpg",
            ["Chú tôm"] = "/images/photos/flashcard-shrimp.jpg",
            ["🦐"] = "/images/photos/flashcard-shrimp.jpg",
            ["Gà"] = "/images/photos/chicken.jpg",
            ["Con gà"] = "/images/photos/chicken.jpg",
            ["Gà con"] = "/images/photos/chicken.jpg",
            ["🐔"] = "/images/photos/chicken.jpg",
            ["Ong"] = "/images/photos/flashcard-bee.jpg",
            ["Con ong"] = "/images/photos/flashcard-bee.jpg",
            ["Chú ong"] = "/images/photos/flashcard-bee.jpg",
            ["🐝"] = "/images/photos/flashcard-bee.jpg",
            ["Bướm"] = "/images/photos/flashcard-butterfly.jpg",
            ["Con bướm"] = "/images/photos/flashcard-butterfly.jpg",
            ["Chú bướm"] = "/images/photos/flashcard-butterfly.jpg",
            ["🦋"] = "/images/photos/flashcard-butterfly.jpg",
            ["Thỏ"] = "/images/photos/flashcard-rabbit.jpg",
            ["Con thỏ"] = "/images/photos/flashcard-rabbit.jpg",
            ["Chú thỏ"] = "/images/photos/flashcard-rabbit.jpg",
            ["🐰"] = "/images/photos/flashcard-rabbit.jpg",
            ["Chim"] = "/images/pictograms/bird.svg",
            ["Con chim"] = "/images/pictograms/bird.svg",
            ["Chú chim"] = "/images/pictograms/bird.svg",
            ["Rùa biển"] = "/images/photos/flashcard-sea-turtle.jpg",
            ["Động vật biển"] = "/images/photos/flashcard-sea-animal.jpg",
            ["Côn trùng"] = "/images/photos/flashcard-insects.jpg",
            ["Dưới nước"] = "/images/photos/fish.jpg",
            ["Trên cạn"] = "/images/photos/cat.jpg",

            // Numbers
            ["0"] = "/images/photos/flashcard-number-0.svg",
            ["1"] = "/images/photos/flashcard-number-1.jpg",
            ["2"] = "/images/photos/flashcard-number-2.jpg",
            ["3"] = "/images/photos/flashcard-number-3.jpg",
            ["4"] = "/images/photos/flashcard-number-4.jpg",
            ["5"] = "/images/photos/flashcard-number-5.jpg",
            ["6"] = "/images/photos/flashcard-number-6.jpg",
            ["7"] = "/images/photos/flashcard-number-7.jpg",
            ["8"] = "/images/photos/flashcard-number-8.jpg",
            ["9"] = "/images/photos/flashcard-number-9.jpg",
            ["10"] = "/images/photos/flashcard-number-10.jpg",
            ["11"] = "/images/photos/flashcard-number-11.jpg",
            ["12"] = "/images/photos/flashcard-number-12.jpg",
            ["13"] = "/images/photos/flashcard-number-13.jpg",
            ["14"] = "/images/photos/flashcard-number-14.jpg",
            ["15"] = "/images/photos/flashcard-number-15.jpg",
            ["16"] = "/images/photos/flashcard-number-16.jpg",
            ["17"] = "/images/photos/flashcard-number-17.jpg",
            ["18"] = "/images/photos/flashcard-number-18.jpg",
            ["19"] = "/images/photos/flashcard-number-19.jpg",
            ["20"] = "/images/photos/flashcard-number-20.jpg",

            // Vietnamese Alphabet (Uppercase & Lowercase)
            ["A"] = "/images/photos/flashcard-letter-a.jpg",
            ["a"] = "/images/photos/flashcard-letter-a.jpg",
            ["Ă"] = "/images/photos/flashcard-letter-ă.svg",
            ["ă"] = "/images/photos/flashcard-letter-ă.svg",
            ["Â"] = "/images/photos/flashcard-letter-â.svg",
            ["â"] = "/images/photos/flashcard-letter-â.svg",
            ["B"] = "/images/photos/flashcard-letter-b.jpg",
            ["b"] = "/images/photos/flashcard-letter-b.jpg",
            ["C"] = "/images/photos/flashcard-letter-c.jpg",
            ["c"] = "/images/photos/flashcard-letter-c.jpg",
            ["D"] = "/images/photos/flashcard-letter-d.jpg",
            ["d"] = "/images/photos/flashcard-letter-d.jpg",
            ["Đ"] = "/images/photos/flashcard-letter-đ.svg",
            ["đ"] = "/images/photos/flashcard-letter-đ.svg",
            ["E"] = "/images/photos/flashcard-letter-e.jpg",
            ["e"] = "/images/photos/flashcard-letter-e.jpg",
            ["Ê"] = "/images/photos/flashcard-letter-ê.svg",
            ["ê"] = "/images/photos/flashcard-letter-ê.svg",
            ["G"] = "/images/photos/flashcard-letter-g.jpg",
            ["g"] = "/images/photos/flashcard-letter-g.jpg",
            ["H"] = "/images/photos/flashcard-letter-h.jpg",
            ["h"] = "/images/photos/flashcard-letter-h.jpg",
            ["I"] = "/images/photos/flashcard-letter-i.jpg",
            ["i"] = "/images/photos/flashcard-letter-i.jpg",
            ["K"] = "/images/photos/flashcard-letter-k.jpg",
            ["k"] = "/images/photos/flashcard-letter-k.jpg",
            ["L"] = "/images/photos/flashcard-letter-l.jpg",
            ["l"] = "/images/photos/flashcard-letter-l.jpg",
            ["M"] = "/images/photos/flashcard-letter-m.jpg",
            ["m"] = "/images/photos/flashcard-letter-m.jpg",
            ["N"] = "/images/photos/flashcard-letter-n.jpg",
            ["n"] = "/images/photos/flashcard-letter-n.jpg",
            ["O"] = "/images/photos/flashcard-letter-o.jpg",
            ["o"] = "/images/photos/flashcard-letter-o.jpg",
            ["Ô"] = "/images/photos/flashcard-letter-ô.svg",
            ["ô"] = "/images/photos/flashcard-letter-ô.svg",
            ["Ơ"] = "/images/photos/flashcard-letter-ơ.svg",
            ["ơ"] = "/images/photos/flashcard-letter-ơ.svg",
            ["P"] = "/images/photos/flashcard-letter-p.jpg",
            ["p"] = "/images/photos/flashcard-letter-p.jpg",
            ["Q"] = "/images/photos/flashcard-letter-q.jpg",
            ["q"] = "/images/photos/flashcard-letter-q.jpg",
            ["R"] = "/images/photos/flashcard-letter-r.jpg",
            ["r"] = "/images/photos/flashcard-letter-r.jpg",
            ["S"] = "/images/photos/flashcard-letter-s.jpg",
            ["s"] = "/images/photos/flashcard-letter-s.jpg",
            ["T"] = "/images/photos/flashcard-letter-t.jpg",
            ["t"] = "/images/photos/flashcard-letter-t.jpg",
            ["U"] = "/images/photos/flashcard-letter-u.jpg",
            ["u"] = "/images/photos/flashcard-letter-u.jpg",
            ["Ư"] = "/images/photos/flashcard-letter-ư.svg",
            ["ư"] = "/images/photos/flashcard-letter-ư.svg",
            ["V"] = "/images/photos/flashcard-letter-v.jpg",
            ["v"] = "/images/photos/flashcard-letter-v.jpg",
            ["X"] = "/images/photos/flashcard-letter-x.jpg",
            ["x"] = "/images/photos/flashcard-letter-x.jpg",
            ["Y"] = "/images/photos/flashcard-letter-y.jpg",
            ["y"] = "/images/photos/flashcard-letter-y.jpg",

            // Shapes
            ["Hình tròn"] = "/images/photos/flashcard-shape-circle.svg",
            ["○"] = "/images/photos/flashcard-shape-circle.svg",
            ["Hình vuông"] = "/images/photos/flashcard-shape-square.svg",
            ["□"] = "/images/photos/flashcard-shape-square.svg",
            ["Hình tam giác"] = "/images/photos/flashcard-shape-triangle.svg",
            ["△"] = "/images/photos/flashcard-shape-triangle.svg",
            ["Hình ngôi sao"] = "/images/photos/flashcard-shape-star.svg",
            ["⭐"] = "/images/photos/flashcard-shape-star.svg",
            ["★"] = "/images/photos/flashcard-shape-star.svg",
            ["Hình trái tim"] = "/images/photos/flashcard-shape-heart.svg",
            ["❤️"] = "/images/photos/flashcard-shape-heart.svg",
            ["Hình chữ nhật"] = "/images/photos/flashcard-shape-square.svg",
            ["Hình bầu dục"] = "/images/photos/flashcard-shape-circle.svg",
            ["Hình thoi"] = "/images/photos/flashcard-shape-square.svg",

            // Vehicles & Transport
            ["Xe đạp"] = "/images/pictograms/bicycle.svg",
            ["Ô tô"] = "/images/pictograms/car.svg",
            ["Xe ô tô"] = "/images/pictograms/car.svg",
            ["Xe buýt"] = "/images/pictograms/bus.svg",
            ["Xe buýt trường học"] = "/images/pictograms/bus.svg",
            ["Máy bay"] = "/images/pictograms/airplane.svg",
            ["Thuyền"] = "/images/pictograms/sailboat.svg",
            ["Tàu buồm"] = "/images/pictograms/sailboat.svg",
            ["Gara"] = "/images/pictograms/house.svg",
            ["Trên đường"] = "/images/pictograms/car.svg",

            // School & Daily Life Objects
            ["Bút"] = "/images/pictograms/pencil.svg",
            ["Bút chì"] = "/images/pictograms/pencil.svg",
            ["Bút màu"] = "/images/pictograms/artist-palette.svg",
            ["Vở"] = "/images/pictograms/notebook.svg",
            ["Quyển vở"] = "/images/pictograms/notebook.svg",
            ["Sách"] = "/images/pictograms/book.svg",
            ["Quyển sách"] = "/images/pictograms/book.svg",
            ["Ba lô"] = "/images/pictograms/backpack.svg",
            ["Cặp sách"] = "/images/pictograms/backpack.svg",
            ["Bát"] = "/images/pictograms/bowl.svg",
            ["Thìa"] = "/images/pictograms/spoon.svg",
            ["Nồi"] = "/images/pictograms/cooking-pot.svg",
            ["Bàn chải"] = "/images/pictograms/toothbrush.svg",
            ["Xà phòng"] = "/images/pictograms/soap.svg",
            ["Áo"] = "/images/pictograms/shirt.svg",
            ["Quần"] = "/images/pictograms/pants.svg",
            ["Giày"] = "/images/pictograms/shoe.svg",
            ["Tất"] = "/images/pictograms/socks.svg",
            ["Mũ"] = "/images/pictograms/hat.svg",
            ["Khăn"] = "/images/pictograms/scarf.svg",
            ["Áo mưa"] = "/images/pictograms/coat.svg",
            ["Ô"] = "/images/pictograms/umbrella.svg",
            ["Chiếc ô"] = "/images/pictograms/umbrella.svg",
            ["Mũ rộng vành"] = "/images/pictograms/sun-hat.svg",
            ["Kính râm"] = "/images/pictograms/sunglasses.svg",
            ["Mũ bảo hiểm"] = "/images/pictograms/helmet.svg",
            ["Quả bóng"] = "/images/pictograms/ball.svg",
            ["Đồng hồ"] = "/images/photos/flashcard-letter-đ.svg",

            // Environment & Emotions
            ["Mặt trời"] = "/images/pictograms/sun.svg",
            ["Trời nắng"] = "/images/pictograms/sun.svg",
            ["Ban ngày"] = "/images/pictograms/sun.svg",
            ["Mặt trăng"] = "/images/pictograms/moon.svg",
            ["Ban đêm"] = "/images/pictograms/moon.svg",
            ["Trời mưa"] = "/images/pictograms/umbrella.svg",
            ["Cây"] = "/images/pictograms/seedling.svg",
            ["Bông hoa"] = "/images/pictograms/flower.svg",
            ["🌼"] = "/images/pictograms/flower.svg",
            ["Học tập"] = "/images/pictograms/notebook.svg",
            ["Nhà bếp"] = "/images/pictograms/cooking-pot.svg",
            ["Tổ chim"] = "/images/pictograms/bird.svg",
            ["Hồ nước"] = "/images/photos/fish.jpg",
            ["Vườn cà rốt"] = "/images/photos/flashcard-carrot.jpg",
            ["Tổ ong"] = "/images/photos/flashcard-bee.jpg"
        };

    public static readonly string[] VietnameseAlphabet =
    [
        "A", "Ă", "Â", "B", "C", "D", "Đ", "E", "Ê", "G", "H", "I", "K", "L", "M",
        "N", "O", "Ô", "Ơ", "P", "Q", "R", "S", "T", "U", "Ư", "V", "X", "Y"
    ];

    public static async Task<int> SeedAsync(ApplicationDbContext db)
    {
        await SeedStoryImagesAsync(db);

        var topics = await db.Topics.AsNoTracking().ToDictionaryAsync(x => x.Code);
        var existingSeedItems = await db.LearningItems
            .Include(x => x.Questions)
            .Where(x => x.Code.StartsWith("seed-"))
            .ToDictionaryAsync(x => x.Code);
        var definitions = BuildDefinitions();
        var createdCount = 0;

        foreach (var definition in definitions)
        {
            if (!topics.TryGetValue(definition.TopicCode, out var topic))
            {
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            var payloadJson = definition.PayloadJson;

            if (existingSeedItems.TryGetValue(definition.Code, out var existingItem))
            {
                var changed = false;
                var existingQuestion = existingItem.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
                payloadJson = PreserveVoiceLinks(
                    payloadJson,
                    existingQuestion?.PayloadJson ?? existingItem.ContentJson);
                if (existingItem.SortOrder != definition.SortOrder) { existingItem.SortOrder = definition.SortOrder; changed = true; }
                if (existingItem.Title != definition.Title) { existingItem.Title = definition.Title; changed = true; }
                if (existingItem.SkillGroupId != topic.SkillGroupId) { existingItem.SkillGroupId = topic.SkillGroupId; changed = true; }
                if (existingItem.TopicId != topic.Id) { existingItem.TopicId = topic.Id; changed = true; }
                if (existingItem.Level != definition.Level) { existingItem.Level = definition.Level; changed = true; }
                if (existingItem.InteractionType != definition.InteractionType) { existingItem.InteractionType = definition.InteractionType; changed = true; }
                if (existingItem.InstructionText != definition.Instruction) { existingItem.InstructionText = definition.Instruction; changed = true; }
                if (existingItem.ContentJson != payloadJson) { existingItem.ContentJson = payloadJson; changed = true; }

                if (existingQuestion is not null)
                {
                    if (ApplyQuestionDefinition(existingQuestion, definition, payloadJson))
                    {
                        changed = true;
                    }
                }

                if (changed)
                {
                    existingItem.UpdatedAt = now;
                }
                continue;
            }

            var itemId = Guid.NewGuid();

            if (definition.InteractionType == InteractionTypes.Tracing)
            {
                var templateId = Guid.NewGuid();
                var tracingImageUrl = ResolveTracingFlashcardUrl(definition.Symbol);
                payloadJson = JsonSerializer.Serialize(new
                {
                    symbol = definition.Symbol,
                    templateId,
                    guideMode = "outline",
                    expectedStrokeCount = definition.ExpectedStrokeCount,
                    showStartPoint = true,
                    audioUrl = string.Empty,
                    imageUrl = tracingImageUrl,
                    imageAltText = string.IsNullOrWhiteSpace(tracingImageUrl)
                        ? string.Empty
                        : $"Thẻ học {definition.Symbol}"
                });
                db.TracingTemplates.Add(new TracingTemplate
                {
                    Id = templateId,
                    SymbolType = definition.TopicCode == "viet-so" ? "number" :
                        definition.TopicCode == "chu-in-thuong" ? "lowercase" :
                        definition.TopicCode == "net-co-ban" ? "stroke" : "uppercase",
                    Symbol = definition.Symbol.Length > 10 ? definition.Symbol[..10] : definition.Symbol,
                    DisplayName = definition.Title.Length > 100 ? definition.Title[..100] : definition.Title,
                    CanvasWidth = 720,
                    CanvasHeight = 720,
                    GuideJson = JsonSerializer.Serialize(new
                    {
                        guideMode = "outline",
                        expectedStrokeCount = definition.ExpectedStrokeCount,
                        showStartPoint = true
                    }),
                    CreatedAt = now
                });
            }

            var item = new LearningItem
            {
                Id = itemId,
                Code = definition.Code,
                Title = definition.Title,
                SkillGroupId = topic.SkillGroupId,
                TopicId = topic.Id,
                Level = definition.Level,
                SortOrder = definition.SortOrder,
                InteractionType = definition.InteractionType,
                EstimatedMinutes = definition.InteractionType == InteractionTypes.Tracing ? 5 : 4,
                InstructionText = definition.Instruction,
                ContentJson = payloadJson,
                Status = ContentStatus.Published,
                PublishedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            var question = new Question
            {
                Id = Guid.NewGuid(),
                LearningItemId = itemId,
                SortOrder = 1
            };
            ApplyQuestionDefinition(question, definition, payloadJson);
            item.Questions.Add(question);

            db.LearningItems.Add(item);
            createdCount++;
        }

        var nextOrderByTopicId = definitions
            .Where(x => topics.ContainsKey(x.TopicCode))
            .GroupBy(x => topics[x.TopicCode].Id)
            .ToDictionary(x => x.Key, x => x.Max(y => y.SortOrder));
        var unorderedUserItems = await db.LearningItems
            .Where(x => !x.Code.StartsWith("seed-") && x.SortOrder <= 0)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
        foreach (var userItem in unorderedUserItems)
        {
            var topicKey = userItem.TopicId ?? Guid.Empty;
            nextOrderByTopicId.TryGetValue(topicKey, out var currentOrder);
            currentOrder += 10;
            nextOrderByTopicId[topicKey] = currentOrder;
            userItem.SortOrder = currentOrder;
        }

        await db.SaveChangesAsync();
        return createdCount;
    }

    private static string PreserveVoiceLinks(string newPayloadJson, string? existingPayloadJson)
    {
        if (string.IsNullOrWhiteSpace(existingPayloadJson)) return newPayloadJson;

        try
        {
            var newPayload = JsonNode.Parse(newPayloadJson)?.AsObject() ?? new JsonObject();
            var existingPayload = JsonNode.Parse(existingPayloadJson)?.AsObject();
            if (existingPayload is null) return newPayloadJson;

            var voiceKeys = new[]
            {
                "questionAudioUrl", "questionAudioUrlEn",
                "audioUrl", "audioUrlEn",
                "correctAudioUrl", "correctAudioUrlEn",
                "retryAudioUrl", "retryAudioUrlEn",
                "optionAudio", "optionAudioEn"
            };
            foreach (var key in voiceKeys)
            {
                if (existingPayload[key] is not JsonNode existingValue || !HasVoiceValue(existingValue)) continue;
                newPayload[key] = existingValue.DeepClone();
            }

            return newPayload.ToJsonString();
        }
        catch (JsonException)
        {
            return newPayloadJson;
        }
    }

    private static bool HasVoiceValue(JsonNode value)
    {
        if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var url))
        {
            return !string.IsNullOrWhiteSpace(url);
        }

        return value is JsonObject map && map.Any(entry =>
            entry.Value is JsonValue mapValue &&
            mapValue.TryGetValue<string>(out var mapUrl) &&
            !string.IsNullOrWhiteSpace(mapUrl));
    }

    private static bool ApplyQuestionDefinition(Question question, SeedLesson definition, string payloadJson)
    {
        var changed = false;
        if (question.PromptText != definition.Prompt) { question.PromptText = definition.Prompt; changed = true; }
        if (question.QuestionType != definition.InteractionType) { question.QuestionType = definition.InteractionType; changed = true; }
        if (question.PayloadJson != payloadJson) { question.PayloadJson = payloadJson; changed = true; }

        var expectedAnswerJson = definition.InteractionType == InteractionTypes.Tracing
            ? JsonSerializer.Serialize(new { minPoints = 20, expectedStrokeCount = definition.ExpectedStrokeCount })
            : JsonSerializer.Serialize(new { value = definition.CorrectAnswer });
        if (question.CorrectAnswerJson != expectedAnswerJson) { question.CorrectAnswerJson = expectedAnswerJson; changed = true; }

        var expectedHintJson = JsonSerializer.Serialize(new { level1 = definition.Hint });
        if (question.HintJson != expectedHintJson) { question.HintJson = expectedHintJson; changed = true; }

        var expectedFeedbackJson = JsonSerializer.Serialize(new { correct = "Giỏi lắm, con làm đúng rồi!", retry = "Con thử lại nhé" });
        if (question.FeedbackJson != expectedFeedbackJson) { question.FeedbackJson = expectedFeedbackJson; changed = true; }

        return changed;
    }

    private static async Task SeedStoryImagesAsync(ApplicationDbContext db)
    {
        var images = new[]
        {
            ("story-wash-hands.png", "/images/lessons/story-wash-hands.png", "Bé rửa tay bằng xà phòng"),
            ("story-safe-crossing.png", "/images/lessons/story-safe-crossing.png", "Bé sang đường an toàn"),
            ("story-sharing.png", "/images/lessons/story-sharing.png", "Hai bạn chia sẻ bút màu"),
            ("visual-counting-groups.png", "/images/lessons/visual-counting-groups.png", "Các nhóm táo, ngôi sao và khối xếp hình để luyện đếm"),
            ("visual-basic-shapes.png", "/images/lessons/visual-basic-shapes.png", "Sáu hình dạng cơ bản nhiều màu"),
            ("visual-road-safety.png", "/images/lessons/visual-road-safety.png", "Bé đội mũ bảo hiểm và chờ qua đường cùng người lớn"),
            ("soc-nau.svg", "/images/soc-nau.svg", "Linh vật Sóc Nâu Đồng Hành"),
            ("fish.jpg", "/images/photos/fish.jpg", "Cá vàng màu cam trong bể nước"),
            ("orange.jpg", "/images/photos/orange.jpg", "Những quả cam chín màu vàng cam"),
            ("carrot.jpg", "/images/photos/carrot.jpg", "Các củ cà rốt tươi còn nguyên lá"),
            ("cabbage.jpg", "/images/photos/cabbage.jpg", "Một cây bắp cải nhìn rõ các lớp lá"),
            ("cat.jpg", "/images/photos/cat.jpg", "Mèo màu vàng nhìn thẳng"),
            ("duck.jpg", "/images/photos/duck.jpg", "Vịt đang đi trên mặt đất"),
            ("shrimp.jpg", "/images/photos/shrimp.jpg", "Tôm nhỏ màu đỏ trên lá xanh"),
            ("chicken.jpg", "/images/photos/chicken.jpg", "Gà mái nhìn nghiêng rõ đầu và thân"),
            ("flashcard-apple.jpg", "/images/photos/flashcard-apple.jpg", "Thẻ học quả táo có ảnh và chữ táo"),
            ("flashcard-orange.jpg", "/images/photos/flashcard-orange.jpg", "Thẻ học quả cam có ảnh và chữ cam"),
            ("flashcard-carrot.jpg", "/images/photos/flashcard-carrot.jpg", "Thẻ học củ cà rốt có ảnh và chữ cà rốt"),
            ("flashcard-cabbage.jpg", "/images/photos/flashcard-cabbage.jpg", "Thẻ học bắp cải có ảnh và chữ bắp cải"),
            ("flashcard-bee.jpg", "/images/photos/flashcard-bee.jpg", "Thẻ học con ong có ảnh và chữ ong"),
            ("flashcard-butterfly.jpg", "/images/photos/flashcard-butterfly.jpg", "Thẻ học con bướm có ảnh và chữ bướm"),
            ("flashcard-rabbit.jpg", "/images/photos/flashcard-rabbit.jpg", "Thẻ học con thỏ có ảnh và chữ thỏ"),
            ("flashcard-shrimp.jpg", "/images/photos/flashcard-shrimp.jpg", "Thẻ học con tôm có ảnh và chữ tôm"),
            ("flashcard-sea-turtle.jpg", "/images/photos/flashcard-sea-turtle.jpg", "Thẻ học rùa biển có ảnh và chữ rùa biển"),
            ("flashcard-sea-animal.jpg", "/images/photos/flashcard-sea-animal.jpg", "Thẻ chủ đề động vật biển"),
            ("flashcard-insects.jpg", "/images/photos/flashcard-insects.jpg", "Thẻ chủ đề côn trùng"),
            ("flashcard-shape-circle.svg", "/images/photos/flashcard-shape-circle.svg", "Thẻ học Hình tròn"),
            ("flashcard-shape-square.svg", "/images/photos/flashcard-shape-square.svg", "Thẻ học Hình vuông"),
            ("flashcard-shape-triangle.svg", "/images/photos/flashcard-shape-triangle.svg", "Thẻ học Hình tam giác"),
            ("flashcard-shape-star.svg", "/images/photos/flashcard-shape-star.svg", "Thẻ học Hình ngôi sao"),
            ("flashcard-shape-heart.svg", "/images/photos/flashcard-shape-heart.svg", "Thẻ học Hình trái tim"),
            ("story-gau-mat-ong.jpg", "/images/lessons/doc-hieu/story-gau-mat-ong.jpg", "Truyện Chú gấu và mật ong"),
            ("story-cao-chiec-khan.jpg", "/images/lessons/doc-hieu/story-cao-chiec-khan.jpg", "Truyện Cáo nhỏ và chiếc khăn"),
            ("story-chiec-o-vang.jpg", "/images/lessons/doc-hieu/story-chiec-o-vang.jpg", "Truyện Chiếc ô màu vàng"),
            ("story-soc-hat-de.jpg", "/images/lessons/doc-hieu/story-soc-hat-de.jpg", "Truyện Chú sóc và hạt dẻ"),
            ("story-chim-non-tap-bay.jpg", "/images/lessons/doc-hieu/story-chim-non-tap-bay.jpg", "Truyện Chú chim non tập bay"),
            ("story-rua-dong-suoi.jpg", "/images/lessons/doc-hieu/story-rua-dong-suoi.jpg", "Truyện Rùa nhỏ và dòng suối"),
            ("story-chiec-hop-bi-mat.jpg", "/images/lessons/doc-hieu/story-chiec-hop-bi-mat.jpg", "Truyện Chiếc hộp bí mật"),
            ("story-nhim-qua-tao.jpg", "/images/lessons/doc-hieu/story-nhim-qua-tao.jpg", "Truyện Chú nhím và quả táo"),
            ("story-tho-cu-ca-rot.jpg", "/images/lessons/doc-hieu/story-tho-cu-ca-rot.jpg", "Truyện Chú thỏ và củ cà rốt"),
            ("story-chiec-thuyen-giay.jpg", "/images/lessons/doc-hieu/story-chiec-thuyen-giay.jpg", "Truyện Chiếc thuyền giấy"),
            ("story-tho-con-ca-rot.jpg", "/images/lessons/doc-hieu/story-tho-con-ca-rot.jpg", "Truyện Thỏ con và cà rốt")
        };
        var numberImages = Enumerable.Range(0, 21)
            .Select(number => (
                number == 0 ? "flashcard-number-0.svg" : $"flashcard-number-{number}.jpg",
                number == 0 ? "/images/photos/flashcard-number-0.svg" : $"/images/photos/flashcard-number-{number}.jpg",
                $"Thẻ học số {number}"));
        var countImages = Enumerable.Range(1, 10)
            .Select(number => (
                $"flashcard-count-{number}.jpg",
                $"/images/photos/flashcard-count-{number}.jpg",
                $"Thẻ đếm số lượng {number} với nhóm đồ vật trực quan"));
        var allImages = images.Concat(numberImages).Concat(countImages);
        var existingPaths = await db.MediaAssets.AsNoTracking()
            .Where(x => x.AssetType == "image")
            .Select(x => x.StoragePath)
            .ToHashSetAsync();

        foreach (var image in allImages.Where(x => !existingPaths.Contains(x.Item2)))
        {
            db.MediaAssets.Add(new MediaAsset
            {
                Id = Guid.NewGuid(),
                AssetType = "image",
                FileName = image.Item1,
                ContentType = Path.GetExtension(image.Item1).Equals(".svg", StringComparison.OrdinalIgnoreCase)
                    ? "image/svg+xml"
                    : "image/jpeg",
                StoragePath = image.Item2,
                AltText = image.Item3,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private static IReadOnlyList<SeedLesson> BuildDefinitions()
    {
        var lessons = new List<SeedLesson>();

        // 1. Tracing & Letter Learning for all 29 Vietnamese letters
        for (var index = 0; index < VietnameseAlphabet.Length; index++)
        {
            var upper = VietnameseAlphabet[index];
            var lower = upper.ToLower(new System.Globalization.CultureInfo("vi-VN"));
            lessons.Add(Tracing($"seed-tracing-upper-{index + 1:00}", $"Tô chữ {upper} in hoa", "chu-in-hoa", upper, 2));
            lessons.Add(Tracing($"seed-tracing-lower-{index + 1:00}", $"Tô chữ {lower} in thường", "chu-in-thuong", lower, 2));
        }

        // 2. Numbers 0 to 9 Tracing and Recognition
        for (var number = 0; number <= 9; number++)
        {
            lessons.Add(Tracing($"seed-tracing-number-{number}", $"Tô số {number}", "viet-so", number.ToString(), 1));
            var choices = new[] { Math.Max(0, number - 1).ToString(), number.ToString(), Math.Min(9, number + 1).ToString() }.Distinct().ToArray();
            if (choices.Length < 2) choices = [number.ToString(), number == 0 ? "1" : "0"];
            lessons.Add(Choice(
                $"seed-recognize-number-{number}", $"Nhận biết số {number}", "so-0-9", InteractionTypes.SingleChoice,
                "Con quan sát và chọn đúng chữ số.", $"Đâu là số {number}?", choices, number.ToString(),
                imageUrl: ResolveNumberFlashcardUrl(number.ToString())));
        }

        lessons.AddRange(BuildMultiSelectLessons());
        lessons.AddRange(BuildListenLessons());
        lessons.AddRange(BuildDragLessons());
        lessons.AddRange(BuildMatchingLessons());
        lessons.AddRange(BuildOrderingLessons());
        lessons.AddRange(BuildCountingLessons());
        lessons.AddRange(BuildQuantityLessons());
        lessons.AddRange(BuildComparisonLessons());
        lessons.AddRange(BuildClassificationLessons());
        lessons.AddRange(BuildStoryLessons());
        lessons.AddRange(BuildReadingComprehensionLessons());
        lessons.AddRange(BuildCoverageLessons());
        EnsureMinimumActivityCoverage(lessons, minimumPerTopicAndActivity: 10);

        var topicOrders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var enrichedLessons = lessons.Select(lesson =>
        {
            topicOrders.TryGetValue(lesson.TopicCode, out var currentOrder);
            currentOrder += 10;
            topicOrders[lesson.TopicCode] = currentOrder;
            return lesson with
            {
                SortOrder = ResolvePedagogicalOrder(lesson.Code, currentOrder),
                PayloadJson = EnrichLessonMedia(lesson)
            };
        }).ToList();

        ValidateSeedCoverage(enrichedLessons);
        return enrichedLessons;
    }

    private static void ValidateSeedCoverage(IReadOnlyCollection<SeedLesson> lessons)
    {
        const int minimumLessonsPerGroup = 10;
        const int minimumLessonsPerActivity = 10;

        var groupsBelowMinimum = CurriculumCatalog.Groups
            .Select(group => new
            {
                group.Code,
                LessonCount = lessons.Count(lesson => group.Topics.Any(topic =>
                    string.Equals(topic.Code, lesson.TopicCode, StringComparison.OrdinalIgnoreCase)))
            })
            .Where(group => group.LessonCount < minimumLessonsPerGroup)
            .Select(group => $"{group.Code} ({group.LessonCount}/{minimumLessonsPerGroup})")
            .ToArray();

        if (groupsBelowMinimum.Length > 0)
        {
            throw new InvalidOperationException($"Kho bài học gốc chưa đủ tối thiểu {minimumLessonsPerGroup} bài/nhóm: {string.Join(", ", groupsBelowMinimum)}.");
        }

        var missingTopics = CurriculumCatalog.Groups
            .SelectMany(group => group.Topics)
            .Where(topic => !lessons.Any(lesson =>
                string.Equals(lesson.TopicCode, topic.Code, StringComparison.OrdinalIgnoreCase)))
            .Select(topic => topic.Code)
            .ToArray();

        if (missingTopics.Length > 0)
        {
            throw new InvalidOperationException($"Kho bài học gốc chưa phủ các chủ đề: {string.Join(", ", missingTopics)}.");
        }

        var missingTopicActivities = CurriculumCatalog.Groups
            .SelectMany(group => group.Topics)
            .SelectMany(topic =>
            {
                var rule = ActivityTemplateCatalog.ForTopic(topic.Code);
                var expectedTypes = rule.InteractionTypes.Concat(rule.AllowsTracing ? [InteractionTypes.Tracing] : []);
                return expectedTypes.Select(interactionType => new
                {
                    topic.Code,
                    InteractionType = interactionType,
                    LessonCount = lessons.Count(lesson =>
                        string.Equals(lesson.TopicCode, topic.Code, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(lesson.InteractionType, interactionType, StringComparison.OrdinalIgnoreCase))
                });
            })
            .Where(activity => activity.LessonCount < minimumLessonsPerActivity)
            .Select(activity => $"{activity.Code}/{activity.InteractionType} ({activity.LessonCount}/{minimumLessonsPerActivity})")
            .ToArray();

        if (missingTopicActivities.Length > 0)
        {
            throw new InvalidOperationException($"Kho bài học gốc chưa đủ tối thiểu {minimumLessonsPerActivity} bài cho từng dạng bài trong từng chủ đề: {string.Join(", ", missingTopicActivities)}.");
        }
    }

    private static void EnsureMinimumActivityCoverage(List<SeedLesson> lessons, int minimumPerTopicAndActivity)
    {
        foreach (var group in CurriculumCatalog.Groups)
        {
            foreach (var topic in group.Topics)
            {
                var rule = ActivityTemplateCatalog.ForTopic(topic.Code);
                var expectedTypes = rule.InteractionTypes
                    .Concat(rule.AllowsTracing ? [InteractionTypes.Tracing] : [])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var interactionType in expectedTypes)
                {
                    var currentCount = lessons.Count(lesson =>
                        string.Equals(lesson.TopicCode, topic.Code, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(lesson.InteractionType, interactionType, StringComparison.OrdinalIgnoreCase));

                    for (var ordinal = currentCount + 1; ordinal <= minimumPerTopicAndActivity; ordinal++)
                    {
                        lessons.Add(BuildMinimumCoverageLesson(group.Code, topic.Code, topic.Name, interactionType, ordinal));
                    }
                }
            }
        }
    }

    private static SeedLesson BuildMinimumCoverageLesson(
        string groupCode,
        string topicCode,
        string topicName,
        string interactionType,
        int ordinal)
    {
        var code = $"seed-minimum-{NormalizeSeedCode(topicCode)}-{NormalizeSeedCode(interactionType.Replace('_', '-'))}-{ordinal:00}";
        var number = ((ordinal - 1) % 10) + 1;
        var nextNumber = number == 10 ? 1 : number + 1;
        var thirdNumber = nextNumber == 10 ? 1 : nextNumber + 1;
        var profile = GetTopicCoverageProfile(topicCode, groupCode);
        var primary = profile.CorrectItems[(ordinal - 1) % profile.CorrectItems.Length];
        var secondary = profile.CorrectItems[ordinal % profile.CorrectItems.Length];
        var distractor1 = profile.Distractors[(ordinal - 1) % profile.Distractors.Length];
        var distractor2 = profile.Distractors[ordinal % profile.Distractors.Length];
        var orderingScenario = BuildOrderingScenario(topicCode, ordinal, primary, profile);
        var targetImage = ResolveSemanticImageUrl(primary, groupCode);

        return interactionType switch
        {
            InteractionTypes.SingleChoice => SemanticChoice(code, $"Nhận biết {primary}", topicCode, interactionType,
                $"Quan sát và chọn đúng {primary}.", $"Đâu là {primary}?",
                [distractor1, primary, distractor2], primary, targetImage),
            InteractionTypes.MultiSelect => SemanticMulti(code, $"Tìm {profile.Criterion}", topicCode,
                $"Chọn tất cả {profile.Criterion}.", [primary, distractor1, secondary, distractor2], [primary, secondary]),
            InteractionTypes.ListenAndChoose => SemanticChoice(code, $"Nghe để tìm {primary}", topicCode, interactionType,
                "Nghe kỹ rồi chọn đúng nội dung được nhắc đến.", $"Con vừa nghe thấy nội dung nào?",
                [distractor1, primary, distractor2], primary, targetImage, $"Con hãy chọn {primary}."),
            InteractionTypes.DragDrop => SemanticChoice(code, $"Đưa {primary} về đúng chỗ", topicCode, interactionType,
                $"Kéo {primary} vào vùng đích.", $"Vật nào cần đưa vào vùng {profile.Criterion}?",
                [distractor1, primary, distractor2], primary, string.Empty, targetLabel: profile.Criterion),
            InteractionTypes.Matching => Mapping(code, $"Ghép đúng trong bài {topicName}", topicCode, interactionType,
                [(primary, BuildSemanticPairLabel(topicCode, primary)),
                 (secondary, BuildSemanticPairLabel(topicCode, secondary)),
                 (distractor1, BuildSemanticPairLabel(topicCode, distractor1))], suppressAutoImage: true),
            InteractionTypes.Ordering => SemanticOrdering(code, orderingScenario.Title, topicCode,
                orderingScenario.Prompt, orderingScenario.Items),
            InteractionTypes.Counting => Counting(code, $"Đếm {number} {profile.CountingObject}", topicCode,
                GetCoverageSymbol(groupCode), number),
            InteractionTypes.QuantityBuilder => Quantity(code, $"Tạo nhóm {number} {profile.CountingObject}", topicCode,
                GetCoverageSymbol(groupCode), number),
            InteractionTypes.Comparison => Comparison(code, $"So sánh hai nhóm {profile.CountingObject}", topicCode,
                GetCoverageSymbol(groupCode), $"{number + 1} {profile.CountingObject}", number + 1, $"{number} {profile.CountingObject}", number),
            InteractionTypes.Classification => Mapping(code, $"Phân loại: {profile.Criterion}", topicCode, interactionType,
                [(primary, profile.Criterion), (secondary, profile.Criterion),
                 (distractor1, $"Không phải {profile.Criterion}"), (distractor2, $"Không phải {profile.Criterion}")], suppressAutoImage: true),
            InteractionTypes.StoryChoice => Story(code, $"Câu chuyện về {primary}", topicCode,
                $"Trong hoạt động {topicName.ToLowerInvariant()}, bạn nhỏ quan sát và nhận ra {primary}. Bạn gọi đúng tên là {primary}.",
                targetImage, $"Bạn nhỏ đã nhận ra nội dung nào?", [distractor1, primary, distractor2], primary),
            InteractionTypes.Tracing => Tracing(code, $"Tô {GetCoverageTracingSymbol(groupCode, ordinal)} theo nét", topicCode,
                GetCoverageTracingSymbol(groupCode, ordinal), Math.Clamp(number, 1, 4)),
            _ => throw new InvalidOperationException($"Chưa có mẫu bổ sung cho dạng bài {interactionType}.")
        };
    }

    private static SeedLesson SemanticChoice(
        string code, string title, string topicCode, string type, string instruction, string prompt,
        string[] choices, string answer, string imageUrl, string speechText = "", string targetLabel = "") =>
        Lesson(code, title, topicCode, type, instruction, prompt,
            new { choices, targetLabel, audioUrl = string.Empty, speechText, imageUrl }, answer);

    private static SeedLesson SemanticMulti(
        string code, string title, string topicCode, string prompt, string[] choices, string[] answers) =>
        Lesson(code, title, topicCode, InteractionTypes.MultiSelect,
            "Con chọn tất cả đáp án đúng rồi bấm Hoàn thành.", prompt,
            new
            {
                choices,
                correctCount = answers.Length,
                imageUrl = string.Empty,
                suppressAutoImage = true,
                audioUrl = string.Empty,
                speechText = string.Empty
            },
            string.Join('|', answers.OrderBy(x => x)));

    private static SeedLesson SemanticOrdering(
        string code, string title, string topicCode, string prompt, string[] items) =>
        Lesson(code, title, topicCode, InteractionTypes.Ordering,
            "Con sắp xếp các mục theo trình tự đúng.", prompt,
            new { items = ShuffleValuesDeterministically(items, code), imageUrl = string.Empty, suppressAutoImage = true, audioUrl = string.Empty, speechText = string.Empty },
            string.Join('|', items));

    private static string[] ShuffleValuesDeterministically(string[] values, string seed)
    {
        var result = values.ToArray();
        var hash = seed.Aggregate(17, (current, character) => unchecked(current * 31 + character));
        var random = new Random(hash);
        for (var index = result.Length - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (result[index], result[swapIndex]) = (result[swapIndex], result[index]);
        }
        if (result.SequenceEqual(values) && result.Length > 1)
        {
            (result[0], result[1]) = (result[1], result[0]);
        }
        return result;
    }

    private static OrderingScenario BuildOrderingScenario(
        string topicCode,
        int ordinal,
        string primary,
        TopicCoverageProfile profile)
    {
        var number = ((ordinal - 1) % 7) + 1;
        if (topicCode == "so-0-9" || topicCode == "thu-tu-so")
        {
            var values = Enumerable.Range(number, 4).Select(value => $"Số {value}").ToArray();
            return new($"Xếp các số từ {number} đến {number + 3}", "Sắp xếp các số từ bé đến lớn.", values);
        }
        if (topicCode == "so-10-20")
        {
            var start = 10 + ((ordinal - 1) % 8);
            var values = Enumerable.Range(start, 4).Select(value => $"Số {value}").ToArray();
            return new($"Xếp các số từ {start} đến {start + 3}", "Sắp xếp các số từ bé đến lớn.", values);
        }
        if (topicCode == "quy-luat")
        {
            var first = ordinal % 2 == 0 ? "Hình tròn" : "Hình vuông";
            var second = first == "Hình tròn" ? "Hình vuông" : "Hình tròn";
            return new($"Hoàn thành quy luật {first} – {second}", "Sắp xếp để hai hình luân phiên nhau.", [first, second, first, second]);
        }

        var scenarios = topicCode switch
        {
            "tu-phuc-vu" => new[]
            {
                Scenario("Các bước rửa tay", "Làm ướt tay", "Lấy xà phòng", "Chà sạch hai tay", "Xả nước và lau khô"),
                Scenario("Các bước đánh răng", "Lấy bàn chải", "Cho kem đánh răng", "Chải đều các mặt răng", "Súc miệng sạch"),
                Scenario("Cất đồ chơi gọn gàng", "Phân loại đồ chơi", "Cho vào đúng hộp", "Đặt hộp lên kệ", "Kiểm tra sàn nhà"),
                Scenario("Chuẩn bị ba lô", "Xem thời khóa biểu", "Lấy sách vở", "Cho đồ vào ba lô", "Kéo khóa ba lô")
            },
            "an-toan" => new[]
            {
                Scenario("Qua đường an toàn", "Dừng lại bên lề", "Quan sát hai phía", "Nắm tay người lớn", "Đi trên vạch sang đường"),
                Scenario("Khi thấy ổ điện nguy hiểm", "Không chạm vào ổ điện", "Lùi ra xa", "Báo cho người lớn", "Chờ người lớn xử lý"),
                Scenario("Khi thấy nước nóng", "Không đưa tay chạm", "Đứng cách xa", "Gọi người lớn", "Chờ nước nguội"),
                Scenario("Cất kéo an toàn", "Cầm vào phần cán", "Khép lưỡi kéo", "Đưa cán kéo về phía trước", "Cất vào hộp có nắp")
            },
            "giao-tiep" => new[]
            {
                Scenario("Chào hỏi lễ phép", "Nhìn người đối diện", "Mỉm cười", "Nói lời chào", "Lắng nghe lời đáp"),
                Scenario("Nói lời cảm ơn", "Nhận sự giúp đỡ", "Nhìn người giúp mình", "Nói lời cảm ơn", "Mỉm cười thân thiện"),
                Scenario("Nói lời xin lỗi", "Nhận ra việc chưa đúng", "Đến gần bạn", "Nói lời xin lỗi", "Cùng sửa lại việc đó"),
                Scenario("Xin phép mượn đồ", "Đến gần bạn", "Hỏi mượn lịch sự", "Chờ bạn đồng ý", "Dùng xong rồi trả lại")
            },
            "nghe-hieu" => new[]
            {
                Scenario($"Nghe và tìm {primary}", "Ngồi yên để nghe", $"Nghe câu có từ {primary}", "Ghi nhớ từ quan trọng", $"Chọn đúng {primary}")
            },
            "ke-chuyen" => new[]
            {
                Scenario($"Kể chuyện về {primary}", $"Giới thiệu {primary}", "Kể sự việc bắt đầu", "Kể điều xảy ra tiếp theo", "Nói kết thúc câu chuyện")
            },
            "doc-hieu" => new[]
            {
                Scenario($"Đọc hiểu về {primary}", "Đọc từ đầu câu chuyện", $"Tìm đoạn nói về {primary}", "Nhớ sự việc quan trọng", "Trả lời câu hỏi")
            },
            "ghi-nho" => new[]
            {
                Scenario($"Ghi nhớ {primary}", $"Quan sát kỹ {primary}", "Ghi nhớ màu và vị trí", "Che hình lại", $"Chọn lại đúng {primary}")
            },
            "lam-theo-yeu-cau" => new[]
            {
                Scenario($"Thực hiện yêu cầu với {primary}", "Nghe hết yêu cầu", $"Tìm đúng {primary}", $"Đưa {primary} vào vị trí được nói", "Kiểm tra rồi hoàn thành")
            },
            "me-cung" => new[]
            {
                Scenario("Tìm đường qua mê cung", "Tìm điểm bắt đầu", "Quan sát các lối đi", "Tránh đường cụt", "Đi đến đích")
            },
            "kheo-tay" => new[]
            {
                Scenario($"Thao tác an toàn với {primary}", $"Chuẩn bị {primary}", "Quan sát hình mẫu", "Thực hiện chậm và cẩn thận", "Cất dụng cụ đúng chỗ")
            },
            "cay-co" => new[]
            {
                Scenario("Quá trình cây lớn lên", "Gieo hạt xuống đất", "Tưới nước", "Hạt nảy mầm", "Cây lớn lên")
            },
            _ => new[]
            {
                new OrderingScenario($"Trình tự: {primary}", "Sắp xếp các bước theo trình tự hợp lý.", profile.OrderedItems)
            }
        };

        return scenarios[(ordinal - 1) % scenarios.Length];
    }

    private static OrderingScenario Scenario(string title, params string[] items) =>
        new(title, "Sắp xếp các bước theo trình tự hợp lý.", items);

    private sealed record OrderingScenario(string Title, string Prompt, string[] Items);

    private static TopicCoverageProfile GetTopicCoverageProfile(string topicCode, string groupCode) => topicCode switch
    {
        "kham-pha-chu" => Profile("các chữ cái", ["Chữ A", "Chữ B", "Chữ C", "Chữ D", "Chữ E", "Chữ G"], ["Số 1", "Số 2", "Hình tròn", "Ngôi sao"], ["Nghe tên chữ", "Quan sát chữ", "Chọn chữ đúng", "Đọc lại chữ"], "chữ cái"),
        "chu-in-hoa" => Profile("các chữ in hoa", ["A", "B", "C", "D", "E", "G"], ["a", "b", "c", "d", "e", "g"], ["Quan sát chữ mẫu", "Đặt bút đúng điểm", "Tô theo nét", "Đọc tên chữ"], "chữ"),
        "chu-in-thuong" => Profile("các chữ in thường", ["a", "b", "c", "d", "e", "g"], ["A", "B", "C", "D", "E", "G"], ["Quan sát chữ mẫu", "Nhận ra nét chữ", "Chọn chữ thường", "Đọc tên chữ"], "chữ"),
        "ghep-hoa-thuong" => Profile("các chữ cái", ["A", "B", "C", "D", "E", "G"], ["1", "2", "3", "4"], ["Chọn chữ hoa", "Tìm chữ thường", "Ghép thành cặp", "Đọc tên chữ"], "cặp chữ"),
        "phan-biet-chu" => Profile("các chữ cái", ["b", "d", "p", "q", "m", "n"], ["6", "9", "2", "5"], ["Quan sát hướng nét", "So sánh hai chữ", "Tìm điểm khác", "Chọn chữ đúng"], "chữ"),

        "so-0-9" => Profile("các số từ 0 đến 9", ["Số 0", "Số 1", "Số 2", "Số 3", "Số 4", "Số 5"], ["Chữ A", "Chữ B", "Hình tròn", "Ngôi sao"], ["Số 0", "Số 1", "Số 2", "Số 3", "Số 4"], "chấm tròn"),
        "so-10-20" => Profile("các số từ 10 đến 20", ["Số 10", "Số 11", "Số 12", "Số 13", "Số 14", "Số 15"], ["Số 2", "Số 4", "Chữ A", "Chữ B"], ["Số 10", "Số 11", "Số 12", "Số 13", "Số 14"], "chấm tròn"),
        "thu-tu-so" => Profile("các chữ số", ["Số 2", "Số 3", "Số 4", "Số 5", "Số 6", "Số 7"], ["Chữ A", "Chữ B", "Hình vuông", "Bông hoa"], ["Số 1", "Số 2", "Số 3", "Số 4", "Số 5"], "thẻ số"),
        "phan-biet-so" => Profile("các chữ số", ["Số 2", "Số 5", "Số 6", "Số 9", "Số 3", "Số 8"], ["Chữ S", "Chữ G", "Hình tròn", "Hình vuông"], ["Quan sát chữ số", "So sánh nét", "Tìm điểm khác", "Chọn số đúng"], "chữ số"),
        "viet-so" => Profile("các chữ số", ["0", "1", "2", "3", "4", "5"], ["A", "B", "C", "D"], ["Quan sát số mẫu", "Đặt bút đúng điểm", "Tô theo nét", "Đọc tên số"], "chữ số"),

        "dem-so-luong" => Profile("các nhóm đồ vật có thể đếm", ["Ba quả táo", "Bốn ngôi sao", "Hai con cá", "Năm chiếc bút", "Một quả bóng", "Sáu bông hoa"], ["Rửa tay", "Trời mưa", "Chữ A", "Màu xanh"], ["Chỉ từng đồ vật", "Đếm từ trái sang phải", "Nói số lượng", "Chọn chữ số"], "đồ vật"),
        "tao-so-luong" => Profile("các nhóm có số lượng xác định", ["Hai quả táo", "Ba ngôi sao", "Bốn con cá", "Năm chiếc bút", "Sáu bông hoa", "Một quả bóng"], ["Chữ A", "Trời nắng", "Rửa tay", "Hình vuông"], ["Đọc số mục tiêu", "Thêm từng đồ vật", "Đếm lại", "Xác nhận kết quả"], "đồ vật"),
        "ghep-so-luong" => Profile("các nhóm số lượng", ["Một quả bóng", "Hai con cá", "Ba quả táo", "Bốn ngôi sao", "Năm chiếc bút", "Sáu bông hoa"], ["Chữ A", "Chữ B", "Trời mưa", "Rửa tay"], ["Quan sát nhóm", "Đếm đồ vật", "Tìm thẻ số", "Ghép số với nhóm"], "đồ vật"),
        "so-sanh" => Profile("các nhóm đồ vật", ["Nhóm ba quả táo", "Nhóm bốn ngôi sao", "Nhóm hai con cá", "Nhóm năm chiếc bút", "Nhóm sáu bông hoa", "Nhóm một quả bóng"], ["Chữ A", "Trời nắng", "Rửa tay", "Màu đỏ"], ["Đếm nhóm thứ nhất", "Đếm nhóm thứ hai", "So sánh hai số", "Chọn nhiều hơn"], "đồ vật"),
        "tach-gop" => Profile("các nhóm đồ vật", ["Hai quả táo", "Ba ngôi sao", "Bốn con cá", "Năm chiếc bút", "Sáu bông hoa", "Một quả bóng"], ["Chữ A", "Trời mưa", "Rửa tay", "Màu vàng"], ["Quan sát nhóm ban đầu", "Tách thành hai phần", "Đếm từng phần", "Gộp và kiểm tra"], "đồ vật"),
        "cong-bot" => Profile("các nhóm đồ vật", ["Hai quả táo", "Ba ngôi sao", "Bốn con cá", "Năm chiếc bút", "Sáu bông hoa", "Một quả bóng"], ["Chữ A", "Trời nắng", "Đánh răng", "Hình tròn"], ["Đếm số ban đầu", "Thêm hoặc bớt", "Đếm lại", "Chọn kết quả"], "đồ vật"),

        "phan-loai" => Profile("các con vật", ["Con mèo", "Con chó", "Con cá", "Con vịt", "Con gà", "Con thỏ"], ["Quả táo", "Bút chì", "Xe đạp", "Cái bát"], ["Quan sát từng vật", "Tìm đặc điểm chung", "Chọn nhóm phù hợp", "Kiểm tra lại"], "thẻ hình"),
        "quy-luat" => Profile("các hình dạng", ["Hình tròn", "Hình vuông", "Hình tam giác", "Ngôi sao", "Trái tim", "Hình chữ nhật"], ["Con mèo", "Bút chì", "Quả táo", "Xe đạp"], ["Hình tròn", "Hình vuông", "Hình tròn", "Hình vuông"], "hình"),
        "ghep-bong" => Profile("các đồ vật", ["Quả táo", "Con mèo", "Con cá", "Bút chì", "Chiếc ô", "Xe đạp"], ["Số 1", "Chữ A", "Màu đỏ", "Trời nắng"], ["Quan sát đồ vật", "Quan sát đường viền", "Tìm bóng giống nhau", "Ghép đúng cặp"], "đồ vật"),
        "tim-khac-biet" => Profile("các hình dạng", ["Hình tròn", "Hình vuông", "Hình tam giác", "Ngôi sao", "Trái tim", "Hình chữ nhật"], ["Con mèo", "Quả táo", "Bút chì", "Xe đạp"], ["Quan sát toàn bộ hình", "So sánh màu sắc", "So sánh hình dạng", "Chọn hình khác"], "hình"),

        "tu-phuc-vu" => Profile("các việc bé tự làm", ["Rửa tay", "Đánh răng", "Xếp đồ chơi", "Cất giày dép", "Mặc áo", "Cất ba lô"], ["Nghịch ổ điện", "Chạy qua đường", "Thức khuya", "Vứt rác bừa bãi"], ["Chuẩn bị đồ dùng", "Thực hiện từng bước", "Cất đồ gọn gàng", "Rửa tay sạch"], "việc tốt"),
        "an-toan" => Profile("các hành động an toàn", ["Đội mũ bảo hiểm", "Đi cùng người lớn", "Tránh xa ổ điện", "Cất dao kéo", "Ngồi đúng chỗ", "Báo người lớn"], ["Nghịch ổ điện", "Tự bật bếp", "Chạy qua đường", "Chạm nước sôi"], ["Dừng lại quan sát", "Nắm tay người lớn", "Đi đúng vạch", "Sang đường an toàn"], "hành động an toàn"),
        "cam-xuc" => Profile("các cảm xúc tích cực", ["Vui vẻ", "Bình tĩnh", "Tự hào", "Yêu thương", "Hào hứng", "Thân thiện"], ["Cái bát", "Xe đạp", "Bút chì", "Quả táo"], ["Nhìn nét mặt", "Lắng nghe giọng nói", "Gọi tên cảm xúc", "Chia sẻ với người lớn"], "cảm xúc"),
        "giao-tiep" => Profile("các lời nói lịch sự", ["Con chào cô ạ", "Con cảm ơn ạ", "Con xin lỗi", "Mời bạn cùng chơi", "Bạn có cần giúp không", "Con xin phép ạ"], ["Tránh ra", "Không thích", "Đưa đây", "Im đi"], ["Nhìn người đối diện", "Lắng nghe", "Nói lời lịch sự", "Chờ bạn trả lời"], "lời nói lịch sự"),

        "von-tu" => Profile("các từ chỉ đồ vật", ["Bút chì", "Quyển sách", "Ba lô", "Chiếc ô", "Cái bát", "Đôi giày"], ["Vui vẻ", "Chạy nhanh", "Màu đỏ", "Số ba"], ["Quan sát hình", "Gọi tên đồ vật", "Nói đặc điểm", "Dùng từ trong câu"], "từ"),
        "nghe-hieu" => Profile("các đồ vật được nhắc đến", ["Quả táo", "Con mèo", "Bút chì", "Quyển sách", "Bông hoa", "Chiếc ô"], ["Số 1", "Chữ A", "Màu xanh", "Hình vuông"], ["Ngồi yên", "Nghe hết câu", "Nhớ từ quan trọng", "Chọn đáp án"], "nội dung"),
        "ke-chuyen" => Profile("các nhân vật câu chuyện", ["Bạn thỏ", "Chú mèo", "Bạn gấu", "Cô bé", "Chú cá", "Bạn ong"], ["Cái bát", "Bút chì", "Số 2", "Hình vuông"], ["Mở đầu câu chuyện", "Sự việc xảy ra", "Nhân vật giải quyết", "Kết thúc câu chuyện"], "nhân vật"),
        "am-van" => Profile("các tiếng bắt đầu bằng âm b", ["Bút", "Bát", "Bóng", "Bé", "Bò", "Bướm"], ["Cá", "Táo", "Mèo", "Ô"], ["Nghe âm đầu", "Nhắc lại âm", "Ghép với tiếng", "Đọc trọn tiếng"], "tiếng"),
        "doc-hieu" => Profile("các nhân vật trong câu chuyện", ["Bạn An", "Bạn Bình", "Chú mèo", "Bạn thỏ", "Cô giáo", "Mẹ"], ["Cái bát", "Số 3", "Hình tròn", "Màu đỏ"], ["Đọc câu chuyện", "Tìm nhân vật", "Nhớ sự việc", "Trả lời câu hỏi"], "nhân vật"),

        "hinh-dang" => Profile("các hình dạng", ["Hình tròn", "Hình vuông", "Hình tam giác", "Hình chữ nhật", "Ngôi sao", "Trái tim"], ["Con mèo", "Bút chì", "Số 2", "Chữ A"], ["Quan sát đường viền", "Đếm số cạnh", "Đếm số góc", "Gọi tên hình"], "hình"),
        "vi-tri" => Profile("các từ chỉ vị trí", ["Ở phía trên", "Ở phía dưới", "Ở bên trái", "Ở bên phải", "Ở phía trước", "Ở phía sau"], ["Màu đỏ", "Số 2", "Con mèo", "Hình tròn"], ["Quan sát vật làm mốc", "Xác định hướng", "Nói vị trí", "Đặt vật đúng chỗ"], "vị trí"),
        "kich-thuoc" => Profile("các từ chỉ kích thước", ["Lớn hơn", "Nhỏ hơn", "Dài hơn", "Ngắn hơn", "Cao hơn", "Thấp hơn"], ["Màu đỏ", "Số 2", "Con mèo", "Hình tròn"], ["Đặt hai vật cạnh nhau", "Quan sát kích thước", "So sánh", "Chọn kết quả"], "hình"),
        "ghep-hinh" => Profile("các hình dùng để ghép", ["Hình tròn", "Hình vuông", "Hình tam giác", "Hình chữ nhật", "Nửa hình tròn", "Ngôi sao"], ["Con mèo", "Số 2", "Chữ A", "Màu đỏ"], ["Quan sát hình mẫu", "Chọn mảnh ghép", "Xoay đúng hướng", "Đặt vào vị trí"], "mảnh ghép"),

        "ghi-nho" => Profile("các đồ vật cần ghi nhớ", ["Quả táo đỏ", "Con cá xanh", "Ngôi sao vàng", "Bông hoa tím", "Chiếc bút", "Quả bóng"], ["Số 2", "Chữ A", "Trời mưa", "Vui vẻ"], ["Quan sát các vật", "Nhắm mắt ghi nhớ", "Nhắc lại vị trí", "Chọn vật đúng"], "thẻ hình"),
        "tap-trung" => Profile("các hình mục tiêu", ["Ngôi sao vàng", "Quả táo đỏ", "Con cá xanh", "Bông hoa tím", "Chiếc bút", "Quả bóng"], ["Số 2", "Chữ A", "Trời mưa", "Vui vẻ"], ["Nghe yêu cầu", "Quan sát kỹ", "Bỏ qua vật gây nhiễu", "Chọn hình mục tiêu"], "hình"),
        "lam-theo-yeu-cau" => Profile("các đồ vật được yêu cầu", ["Bút chì", "Quyển sách", "Quả bóng", "Chiếc ô", "Ba lô", "Đôi giày"], ["Vui vẻ", "Màu đỏ", "Số 2", "Trời mưa"], ["Nghe yêu cầu thứ nhất", "Thực hiện yêu cầu thứ nhất", "Nghe yêu cầu thứ hai", "Hoàn thành"], "đồ vật"),

        "net-co-ban" => Profile("các nét cơ bản", ["Nét thẳng", "Nét ngang", "Nét xiên", "Nét cong", "Nét móc", "Nét khuyết"], ["Quả táo", "Con mèo", "Số 2", "Màu đỏ"], ["Quan sát nét mẫu", "Đặt bút", "Đi theo chiều mũi tên", "Dừng ở điểm cuối"], "nét"),
        "tao-hinh" => Profile("các dụng cụ tạo hình", ["Bút chì", "Bút màu", "Giấy màu", "Đất nặn", "Hồ dán", "Kéo thủ công"], ["Cái bát", "Đôi giày", "Xe đạp", "Quả táo"], ["Chọn hình mẫu", "Vẽ đường viền", "Tô màu", "Hoàn thiện bức hình"], "dụng cụ"),
        "noi-diem" => Profile("các điểm cần nối", ["Điểm 1", "Điểm 2", "Điểm 3", "Điểm 4", "Điểm 5", "Điểm 6"], ["Chữ A", "Màu đỏ", "Con mèo", "Quả táo"], ["Tìm điểm 1", "Nối đến điểm 2", "Nối đến điểm 3", "Hoàn thành đường nét"], "điểm"),
        "me-cung" => Profile("các lối đi trong mê cung", ["Lối bên trái", "Lối bên phải", "Lối đi thẳng", "Lối lên trên", "Lối xuống dưới", "Lối về đích"], ["Màu đỏ", "Số 2", "Bút chì", "Quả táo"], ["Tìm điểm bắt đầu", "Quan sát các lối", "Tránh đường cụt", "Đi đến đích"], "lối đi"),
        "kheo-tay" => Profile("các dụng cụ thủ công", ["Bút chì", "Bút màu", "Giấy màu", "Đất nặn", "Hồ dán", "Kéo thủ công"], ["Cái bát", "Đôi giày", "Xe đạp", "Quả táo"], ["Chuẩn bị dụng cụ", "Làm theo mẫu", "Thao tác cẩn thận", "Cất đồ gọn gàng"], "dụng cụ"),

        "con-vat" => Profile("các con vật", ["Con mèo", "Con chó", "Con cá", "Con vịt", "Con gà", "Con thỏ"], ["Quả táo", "Bút chì", "Xe đạp", "Cái bát"], ["Quan sát con vật", "Tìm đặc điểm", "Gọi tên", "Nói nơi sống"], "con vật"),
        "cay-co" => Profile("các cây và bộ phận của cây", ["Cây xanh", "Bông hoa", "Chiếc lá", "Quả táo", "Hạt giống", "Rễ cây"], ["Con mèo", "Bút chì", "Xe đạp", "Cái bát"], ["Gieo hạt", "Tưới nước", "Hạt nảy mầm", "Cây lớn lên"], "cây"),
        "thoi-tiet" => Profile("các hiện tượng thời tiết", ["Trời nắng", "Trời mưa", "Gió mạnh", "Đám mây", "Cầu vồng", "Trời lạnh"], ["Con mèo", "Bút chì", "Cái bát", "Xe đạp"], ["Quan sát bầu trời", "Nhận biết thời tiết", "Chọn trang phục", "Chuẩn bị ra ngoài"], "hiện tượng"),
        "giao-thong" => Profile("các phương tiện giao thông", ["Xe đạp", "Ô tô", "Xe buýt", "Máy bay", "Thuyền", "Tàu hỏa"], ["Quả táo", "Con mèo", "Bút chì", "Cái bát"], ["Quan sát phương tiện", "Nhận biết nơi di chuyển", "Gọi tên phương tiện", "Chọn cách đi an toàn"], "phương tiện"),

        _ => GetGroupCoverageProfile(groupCode)
    };

    private static TopicCoverageProfile GetGroupCoverageProfile(string groupCode) => groupCode switch
    {
        "chu-cai" => Profile("các chữ cái", ["Chữ A", "Chữ B", "Chữ C", "Chữ D"], ["Số 1", "Số 2", "Hình tròn", "Ngôi sao"], ["Quan sát", "Nhận biết", "Chọn đáp án", "Đọc lại"], "chữ"),
        "chu-so" or "so-luong-toan" => Profile("các nhóm số lượng", ["Một đồ vật", "Hai đồ vật", "Ba đồ vật", "Bốn đồ vật"], ["Chữ A", "Màu đỏ", "Trời mưa", "Vui vẻ"], ["Quan sát", "Đếm", "Chọn số", "Kiểm tra"], "đồ vật"),
        _ => Profile("các nội dung phù hợp", ["Quả táo", "Con mèo", "Bút chì", "Quyển sách"], ["Số 1", "Chữ A", "Màu đỏ", "Trời mưa"], ["Quan sát", "Suy nghĩ", "Chọn đáp án", "Kiểm tra"], "đồ vật")
    };

    private static TopicCoverageProfile Profile(
        string criterion, string[] correctItems, string[] distractors, string[] orderedItems, string countingObject) =>
        new(criterion, correctItems, distractors, orderedItems, countingObject);

    private static string BuildSemanticPairLabel(string topicCode, string value)
    {
        if (topicCode == "ghep-hoa-thuong" && value.Length == 1)
        {
            return value.ToLower(new System.Globalization.CultureInfo("vi-VN"));
        }
        if (topicCode == "chu-in-hoa" && value.Length == 1) return $"Chữ hoa {value}";
        if (topicCode == "chu-in-thuong" && value.Length == 1) return $"Chữ thường {value}";
        if (value.StartsWith("Số ", StringComparison.OrdinalIgnoreCase)) return $"Tên gọi của {value.ToLowerInvariant()}";
        if (value.StartsWith("Hình ", StringComparison.OrdinalIgnoreCase)) return $"Đồ vật có dạng {value.ToLowerInvariant()}";
        return $"Hình minh họa đúng của {value.ToLowerInvariant()}";
    }

    private static string ResolveSemanticImageUrl(string value, string groupCode)
    {
        if (TryResolveObservationPhoto(value, out var imageUrl)) return imageUrl;

        var cleanValue = value
            .Replace("Chữ hoa ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("Chữ thường ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("Chữ ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("Số ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
        if (groupCode == "chu-cai") return ResolveLetterFlashcardUrl(cleanValue);
        if (groupCode is "chu-so" or "so-luong-toan") return ResolveNumberFlashcardUrl(cleanValue);
        return string.Empty;
    }

    private sealed record TopicCoverageProfile(
        string Criterion,
        string[] CorrectItems,
        string[] Distractors,
        string[] OrderedItems,
        string CountingObject);

    private static string GetCoverageItem(string groupCode, int index)
    {
        var items = groupCode switch
        {
            "chu-cai" => new[] { "Chữ A", "Chữ B", "Chữ C", "Chữ D", "Chữ E", "Chữ G" },
            "chu-so" => new[] { "Số 1", "Số 2", "Số 3", "Số 4", "Số 5", "Số 6" },
            "so-luong-toan" => new[] { "Ba quả táo", "Bốn ngôi sao", "Hai con cá", "Năm chiếc bút", "Một quả bóng", "Sáu bông hoa" },
            "tu-duy-logic" => new[] { "Hình tròn đỏ", "Hình vuông xanh", "Hình tam giác vàng", "Mảnh ghép lớn", "Mảnh ghép nhỏ", "Chiếc bóng đúng" },
            "ky-nang-song" => new[] { "Rửa tay", "Đánh răng", "Xếp đồ chơi", "Chào hỏi", "Đội mũ bảo hiểm", "Cất giày dép" },
            "ngon-ngu" => new[] { "Quả táo", "Con mèo", "Bút chì", "Quyển sách", "Bông hoa", "Chiếc ô" },
            "hinh-dang-khong-gian" => new[] { "Hình tròn", "Hình vuông", "Hình tam giác", "Ở phía trên", "Ở phía dưới", "Ở bên trái" },
            "ghi-nho-tap-trung" => new[] { "Ngôi sao vàng", "Quả táo đỏ", "Con cá xanh", "Bông hoa tím", "Chiếc bút", "Quả bóng" },
            "van-dong-tinh" => new[] { "Bút chì", "Bút màu", "Đường nét thẳng", "Đường nét cong", "Mảnh ghép", "Chấm tròn" },
            "kham-pha" => new[] { "Con mèo", "Con cá", "Cây xanh", "Bông hoa", "Xe đạp", "Đám mây" },
            _ => new[] { "Đáp án một", "Đáp án hai", "Đáp án ba", "Đáp án bốn", "Đáp án năm", "Đáp án sáu" }
        };
        return items[Math.Abs(index) % items.Length];
    }

    private static string GetCoverageSymbol(string groupCode) => groupCode switch
    {
        "chu-cai" => "A",
        "chu-so" or "so-luong-toan" => "●",
        "hinh-dang-khong-gian" => "★",
        "kham-pha" => "🐟",
        _ => "●"
    };

    private static string GetCoverageTracingSymbol(string groupCode, int ordinal) => groupCode switch
    {
        "chu-cai" => VietnameseAlphabet[(ordinal - 1) % VietnameseAlphabet.Length],
        "chu-so" => ((ordinal - 1) % 10).ToString(),
        _ => ordinal % 2 == 0 ? "C" : "O"
    };

    private static string GetCoverageImage(string groupCode) => groupCode switch
    {
        "chu-cai" => "/images/photos/flashcard-letter-a.jpg",
        "chu-so" or "so-luong-toan" => "/images/photos/flashcard-number-3.jpg",
        "hinh-dang-khong-gian" => "/images/photos/flashcard-shape-circle.svg",
        "kham-pha" => "/images/photos/fish.jpg",
        _ => "/images/photos/flashcard-apple.jpg"
    };

    private static string EnrichLessonMedia(SeedLesson lesson)
    {
        if (lesson.InteractionType == InteractionTypes.Tracing)
        {
            var tracingPayload = JsonNode.Parse(lesson.PayloadJson)?.AsObject() ?? new JsonObject();
            var sym = !string.IsNullOrWhiteSpace(lesson.Symbol) ? lesson.Symbol : ExtractSymbolFromTitle(lesson.Title, lesson.Prompt);
            tracingPayload["symbol"] = sym;
            tracingPayload["expectedStrokeCount"] = lesson.ExpectedStrokeCount;
            tracingPayload["guideMode"] = "outline";
            var tracingImage = ResolveTracingFlashcardUrl(sym);
            tracingPayload["imageUrl"] = tracingImage;
            tracingPayload["imageAltText"] = $"Hình mẫu tô theo nét {sym}";
            return tracingPayload.ToJsonString();
        }

        var payload = JsonNode.Parse(lesson.PayloadJson)?.AsObject() ?? new JsonObject();
        payload["schemaVersion"] = 2;
        payload["activityType"] = lesson.InteractionType;
        payload["questionAudioUrl"] ??= string.Empty;
        payload["instructionSpeechText"] = lesson.Instruction;
        payload["questionSpeechText"] = lesson.Prompt;
        payload["correctSpeechText"] = "Giỏi lắm, con làm đúng rồi!";
        payload["retrySpeechText"] = "Con thử lại nhé";

        if (lesson.TopicCode == "hinh-dang" &&
            string.IsNullOrWhiteSpace(payload["imageUrl"]?.GetValue<string>()))
        {
            payload["focusVisual"] = lesson.CorrectAnswer;
        }

        var currentImage = payload["imageUrl"]?.GetValue<string>() ?? string.Empty;
        var suppressAutoImage = payload["suppressAutoImage"]?.GetValue<bool>() == true;
        if (string.IsNullOrWhiteSpace(currentImage) && !suppressAutoImage)
        {
            currentImage = ResolveQuestionImageUrl(lesson, payload);
            if (!string.IsNullOrWhiteSpace(currentImage))
            {
                payload["imageUrl"] = currentImage;
            }
        }
        if (string.IsNullOrWhiteSpace(currentImage) && lesson.InteractionType == InteractionTypes.StoryChoice)
        {
            currentImage = $"/learning-media/topic/{Uri.EscapeDataString(lesson.TopicCode)}";
            payload["imageUrl"] = currentImage;
        }

        var imageAltText = currentImage switch
        {
            "/images/lessons/visual-counting-groups.png" => "Năm quả táo đỏ cùng các nhóm đồ vật nhiều màu",
            "/images/lessons/visual-basic-shapes.png" => "Sáu hình dạng cơ bản nhiều màu",
            "/images/lessons/visual-road-safety.png" => "Bé đội mũ bảo hiểm và qua đường cùng người lớn",
            "/images/lessons/story-wash-hands.png" => "Bé rửa tay sạch bằng xà phòng",
            "/images/lessons/story-safe-crossing.png" => "Bé đi qua đường an toàn cùng người lớn",
            "/images/lessons/story-sharing.png" => "Hai bạn nhỏ vui vẻ chia sẻ bút màu",
            _ => string.Empty
        };
        if (string.IsNullOrWhiteSpace(imageAltText) &&
            TryReadNumberFlashcard(currentImage, "flashcard-count-", out var countNumber))
        {
            imageAltText = $"Thẻ đếm số lượng {countNumber} với nhóm đồ vật trực quan";
        }
        if (string.IsNullOrWhiteSpace(imageAltText) &&
            TryReadNumberFlashcard(currentImage, "flashcard-number-", out var flashcardNumber))
        {
            imageAltText = $"Thẻ học số {flashcardNumber}";
        }
        if (string.IsNullOrWhiteSpace(imageAltText))
        {
            imageAltText = ResolveQuestionImageAltText(lesson, payload, currentImage);
        }
        payload["imageAltText"] = imageAltText;

        if (payload["itemMedia"] is null && lesson.TopicCode != "doc-hieu")
        {
            var lessonValues = CollectStringValues(payload).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var itemMedia = new JsonObject();
            foreach (var photo in ObservationPhotos.Where(photo => lessonValues.Contains(photo.Key)))
            {
                itemMedia[photo.Key] = photo.Value;
            }
            if (itemMedia.Count > 0)
            {
                payload["itemMedia"] = itemMedia;
            }
        }
        return payload.ToJsonString();
    }

    private static IEnumerable<string> CollectStringValues(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            yield return text;
        }
        else if (node is JsonObject objectNode)
        {
            foreach (var child in objectNode.SelectMany(property => CollectStringValues(property.Value)))
            {
                yield return child;
            }
        }
        else if (node is JsonArray arrayNode)
        {
            foreach (var child in arrayNode.SelectMany(CollectStringValues))
            {
                yield return child;
            }
        }
    }

    private static string ResolveQuestionImageUrl(SeedLesson lesson, JsonObject payload)
    {
        foreach (var candidate in ResolveQuestionImageCandidates(lesson, payload))
        {
            if (TryResolveObservationPhoto(candidate, out var imageUrl))
            {
                return imageUrl;
            }
        }

        // Fallbacks for Letter and Number Lessons
        if (lesson.TopicCode is "kham-pha-chu" or "chu-in-hoa" or "chu-in-thuong" or "phan-biet-chu")
        {
            var letterUrl = ResolveLetterFlashcardUrl(lesson.CorrectAnswer);
            if (!string.IsNullOrWhiteSpace(letterUrl)) return letterUrl;
        }
        if (lesson.TopicCode is "so-0-9" or "so-10-20" or "viet-so" or "thu-tu-so")
        {
            var numberUrl = ResolveNumberFlashcardUrl(lesson.CorrectAnswer);
            if (!string.IsNullOrWhiteSpace(numberUrl)) return numberUrl;
        }

        return string.Empty;
    }

    private static string ResolveQuestionImageAltText(SeedLesson lesson, JsonObject payload, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        foreach (var candidate in ResolveQuestionImageCandidates(lesson, payload))
        {
            if (TryResolveObservationPhoto(candidate, out var candidateImageUrl) &&
                string.Equals(candidateImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
            {
                return $"Thẻ minh họa {candidate}";
            }
        }

        return "Hình minh họa bài tập";
    }

    private static IEnumerable<string> ResolveQuestionImageCandidates(SeedLesson lesson, JsonObject payload)
    {
        yield return lesson.CorrectAnswer;

        foreach (var key in new[] { "targetLabel", "focusVisual", "objectSymbol" })
        {
            if (payload[key] is JsonValue value && value.TryGetValue<string>(out var text))
            {
                yield return text;
            }
        }
    }

    private static bool TryResolveObservationPhoto(string text, out string imageUrl)
    {
        imageUrl = string.Empty;
        var normalized = text.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        var candidates = new[]
        {
            normalized,
            normalized.StartsWith("Con ", StringComparison.OrdinalIgnoreCase) ? normalized[4..] : normalized,
            normalized.StartsWith("Chú ", StringComparison.OrdinalIgnoreCase) ? normalized[4..] : normalized,
            normalized.StartsWith("Cái ", StringComparison.OrdinalIgnoreCase) ? normalized[4..] : normalized,
            normalized.StartsWith("Quả ", StringComparison.OrdinalIgnoreCase) ? normalized[4..] : normalized,
            normalized.StartsWith("Trái ", StringComparison.OrdinalIgnoreCase) ? normalized[5..] : normalized,
            normalized.StartsWith("Chiếc ", StringComparison.OrdinalIgnoreCase) ? normalized[6..] : normalized,
            normalized.StartsWith("Hình ", StringComparison.OrdinalIgnoreCase) ? normalized : $"Hình {normalized}"
        };

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (ObservationPhotos.TryGetValue(candidate, out var resolvedImageUrl))
            {
                imageUrl = resolvedImageUrl;
                return true;
            }
        }

        return false;
    }

    private static bool TryReadNumberFlashcard(string imageUrl, string prefix, out int number)
    {
        number = 0;
        var fileName = Path.GetFileNameWithoutExtension(imageUrl);
        return fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(fileName[prefix.Length..], out number);
    }

    public static string ResolveLetterFlashcardUrl(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return string.Empty;
        }

        var clean = symbol.Trim();
        var culture = new System.Globalization.CultureInfo("vi-VN");
        var upper = clean.ToUpper(culture);
        return VietnameseAlphabet.Contains(upper, StringComparer.OrdinalIgnoreCase)
            ? $"/learning-media/letter/{Uri.EscapeDataString(upper)}"
            : string.Empty;
    }

    public static string ResolveNumberFlashcardUrl(string symbol)
    {
        var clean = symbol.Trim();
        if (clean == "0") return "/images/photos/flashcard-number-0.svg";
        return int.TryParse(clean, out var number) && number is >= 1 and <= 20
            ? $"/images/photos/flashcard-number-{number}.jpg"
            : string.Empty;
    }

    public static string ResolveCountingFlashcardUrl(int count)
    {
        return count is >= 1 and <= 10
            ? $"/images/photos/flashcard-count-{count}.jpg"
            : string.Empty;
    }

    public static string ResolveTracingFlashcardUrl(string symbol)
    {
        var numberImageUrl = ResolveNumberFlashcardUrl(symbol);
        if (!string.IsNullOrWhiteSpace(numberImageUrl)) return numberImageUrl;
        var letterImageUrl = ResolveLetterFlashcardUrl(symbol);
        return !string.IsNullOrWhiteSpace(letterImageUrl)
            ? letterImageUrl
            : string.IsNullOrWhiteSpace(symbol)
                ? string.Empty
                : $"/learning-media/tracing?symbol={Uri.EscapeDataString(symbol.Trim())}";
    }

    private static int ResolvePedagogicalOrder(string code, int fallbackOrder)
    {
        var orderedPrefixes = new[]
        {
            "seed-tracing-upper-", "seed-tracing-lower-", "seed-tracing-number-",
            "seed-recognize-number-", "seed-letter-recognition-", "seed-quantity-", "seed-count-"
        };
        foreach (var prefix in orderedPrefixes)
        {
            if (code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(code[prefix.Length..], out var sequence))
            {
                return prefix is "seed-tracing-number-" or "seed-recognize-number-" or "seed-quantity-" or "seed-count-"
                    ? (sequence + 1) * 10
                    : sequence * 10;
            }
        }
        return 1000 + fallbackOrder;
    }

    private static IEnumerable<SeedLesson> BuildMultiSelectLessons()
    {
        // 1. Chữ cái & Âm vần
        yield return Multi("seed-multi-vowels", "Chọn các chữ nguyên âm", "phan-biet-chu", ["A", "B", "E", "M"], ["A", "E"]);
        yield return Multi("seed-multi-hat-letters", "Chọn các chữ cái có dấu mũ", "phan-biet-chu", ["Â", "A", "Ê", "E", "Ô"], ["Â", "Ê", "Ô"]);
        yield return Multi("seed-multi-horn-letters", "Chọn các chữ cái có dấu râu", "phan-biet-chu", ["Ơ", "O", "Ư", "U"], ["Ơ", "Ư"]);
        yield return Multi("seed-multi-curved-letters", "Chọn các chữ có nét cong kín", "phan-biet-chu", ["O", "Ô", "Ơ", "I", "T"], ["O", "Ô", "Ơ"]);

        // 2. Chữ số & Toán học
        yield return Multi("seed-multi-even", "Chọn các số chẵn", "so-0-9", ["1", "2", "3", "4", "6"], ["2", "4", "6"]);
        yield return Multi("seed-multi-odd", "Chọn các số lẻ", "so-0-9", ["1", "3", "4", "5", "8"], ["1", "3", "5"]);
        yield return Multi("seed-multi-greater-5", "Chọn các số lớn hơn 5", "so-0-9", ["6", "8", "2", "9", "3"], ["6", "8", "9"]);
        yield return Multi("seed-multi-less-4", "Chọn các số nhỏ hơn 4", "so-0-9", ["1", "2", "3", "5", "7"], ["1", "2", "3"]);
        yield return Multi("seed-multi-sum-5", "Chọn hai số có tổng bằng 5", "tach-gop", ["2", "3", "4", "1"], ["2", "3"]);

        // 3. Hình học & Quan sát
        yield return Multi("seed-multi-focus-circles", "Tìm tất cả các hình tròn", "hinh-dang", ["Hình tròn đỏ", "Hình vuông xanh", "Hình tròn vàng", "Hình tam giác"], ["Hình tròn đỏ", "Hình tròn vàng"]);
        yield return Multi("seed-multi-focus-triangles", "Tìm tất cả các hình tam giác", "hinh-dang", ["Hình tam giác xanh", "Hình tam giác vàng", "Hình vuông", "Hình tròn"], ["Hình tam giác xanh", "Hình tam giác vàng"]);
        yield return Multi("seed-multi-focus-stars", "Tìm các ngôi sao lấp lánh", "tap-trung", ["Ngôi sao vàng", "Ngôi sao đỏ", "Mặt trăng", "Đám mây"], ["Ngôi sao vàng", "Ngôi sao đỏ"]);

        // 4. Phân loại & Thế giới quanh bé
        yield return Multi("seed-multi-animals-water", "Chọn các con vật sống dưới nước", "con-vat", ["Cá", "Tôm", "Mèo", "Gà", "Cua"], ["Cá", "Tôm", "Cua"]);
        yield return Multi("seed-multi-animals-fly", "Chọn các loài vật biết bay", "con-vat", ["Chim", "Ong", "Bướm", "Chó", "Cá"], ["Chim", "Ong", "Bướm"]);
        yield return Multi("seed-multi-animals-4legs", "Chọn các con vật có 4 chân", "con-vat", ["Mèo", "Chó", "Thỏ", "Gà", "Vịt"], ["Mèo", "Chó", "Thỏ"]);
        yield return Multi("seed-multi-fruits-red", "Chọn các quả có màu đỏ", "phan-loai", ["Táo", "Dâu tây", "Dưa hấu", "Cam", "Bắp cải"], ["Táo", "Dâu tây", "Dưa hấu"]);
        yield return Multi("seed-multi-fruits-yellow", "Chọn các quả có màu vàng", "phan-loai", ["Chuối", "Xoài", "Táo đỏ", "Dưa hấu"], ["Chuối", "Xoài"]);
        yield return Multi("seed-multi-school-tools", "Chọn đồ dùng để học tập", "phan-loai", ["Bút", "Vở", "Cặp sách", "Bát", "Thìa"], ["Bút", "Vở", "Cặp sách"]);
        yield return Multi("seed-multi-kitchen-tools", "Chọn đồ dùng trong nhà bếp", "phan-loai", ["Nồi", "Chảo", "Bát", "Thìa", "Bút chì"], ["Nồi", "Chảo", "Bát", "Thìa"]);

        // 5. Kỹ năng sống & Thói quen tốt
        yield return Multi("seed-multi-safety-actions", "Chọn các hành động an toàn khi ở nhà", "an-toan", ["Tránh xa ổ điện", "Không nghịch dao kéo", "Tự bật bếp gas", "Nghịch nước sôi"], ["Tránh xa ổ điện", "Không nghịch dao kéo"]);
        yield return Multi("seed-multi-good-habits", "Chọn các thói quen tốt mỗi ngày", "tu-phuc-vu", ["Đánh răng mỗi sáng", "Rửa tay trước khi ăn", "Thức khuya xem điện thoại", "Vứt rác bừa bãi"], ["Đánh răng mỗi sáng", "Rửa tay trước khi ăn"]);
        yield return Multi("seed-multi-summer-clothes", "Chọn trang phục mùa hè mát mẻ", "thoi-tiet", ["Áo phông", "Quần đùi", "Mũ rộng vành", "Áo len dày", "Khăn quàng cổ"], ["Áo phông", "Quần đùi", "Mũ rộng vành"]);

        // 6. Tiền tập đọc & ngôn ngữ
        yield return Multi("seed-multi-words-sound-b", "Chọn các từ bắt đầu bằng âm B", "von-tu", ["Bút chì", "Bát", "Bông hoa", "Con cá", "Quả táo"], ["Bút chì", "Bát", "Bông hoa"]);

        // 7. Vận động tinh
        yield return Multi("seed-multi-drawing-tools", "Chọn đồ dùng để vẽ và tô màu", "kheo-tay", ["Bút chì", "Bút màu", "Quyển vở", "Bát", "Thìa"], ["Bút chì", "Bút màu", "Quyển vở"]);
    }

    private static IEnumerable<SeedLesson> BuildListenLessons()
    {
        // 1. Nhận biết tiếng kêu động vật
        yield return Listen("seed-listen-cat", "Nghe tiếng con mèo", "con-vat", "Con mèo kêu meo meo.", ["Con mèo", "Con chó", "Con vịt"], "Con mèo");
        yield return Listen("seed-listen-dog", "Nghe tiếng con chó", "con-vat", "Con chó kêu gâu gâu.", ["Con chó", "Con mèo", "Con gà"], "Con chó");
        yield return Listen("seed-listen-duck", "Nghe tiếng con vịt", "con-vat", "Con vịt kêu cạp cạp.", ["Con vịt", "Con chim", "Con gà"], "Con vịt");
        yield return Listen("seed-listen-rooster", "Nghe tiếng gà trống", "con-vat", "Con gà trống gáy ò ó o.", ["Gà trống", "Con vịt", "Con chim"], "Gà trống");
        yield return Listen("seed-listen-frog", "Nghe tiếng chú ếch", "con-vat", "Chú ếch kêu ộp ộp bên bờ ao.", ["Chú ếch", "Con mèo", "Con chó"], "Chú ếch");
        yield return Listen("seed-listen-bird", "Nghe tiếng chim hót", "con-vat", "Chú chim nhỏ hót líu lo trên cành cây.", ["Con chim", "Con vịt", "Con gà"], "Con chim");
        yield return Listen("seed-listen-bee", "Nghe tiếng ong bay", "con-vat", "Chú ong bay vo ve đi tìm mật hoa.", ["Chú ong", "Con bướm", "Con cá"], "Chú ong");
        yield return Listen("seed-listen-cow", "Nghe tiếng chú bò", "con-vat", "Chú bò kêu ụ bò trên đồng cỏ.", ["Chú bò", "Con mèo", "Con chó"], "Chú bò");

        // 2. Nhận biết âm chữ cái
        yield return Listen("seed-listen-letter-a", "Nghe và chọn chữ A", "kham-pha-chu", "Đây là chữ A trong quả táo.", ["A", "B", "C"], "A");
        yield return Listen("seed-listen-letter-b", "Nghe và chọn chữ B", "kham-pha-chu", "Đây là chữ Bờ trong quả bóng.", ["A", "B", "D"], "B");
        yield return Listen("seed-listen-letter-c", "Nghe và chọn chữ C", "kham-pha-chu", "Đây là chữ Cờ trong con cá.", ["C", "E", "O"], "C");
        yield return Listen("seed-listen-letter-d", "Nghe và chọn chữ D", "kham-pha-chu", "Đây là chữ Dờ trong quả dưa.", ["D", "Đ", "B"], "D");
        yield return Listen("seed-listen-letter-dd", "Nghe và chọn chữ Đ", "kham-pha-chu", "Đây là chữ Đờ trong chiếc đồng hồ.", ["Đ", "D", "B"], "Đ");
        yield return Listen("seed-listen-letter-e", "Nghe và chọn chữ E", "kham-pha-chu", "Đây là chữ E trong em bé.", ["E", "Ê", "A"], "E");
        yield return Listen("seed-listen-letter-ee", "Nghe và chọn chữ Ê", "kham-pha-chu", "Đây là chữ Ê trong cái ghế.", ["Ê", "E", "O"], "Ê");
        yield return Listen("seed-listen-letter-m", "Nghe và chọn chữ M", "kham-pha-chu", "Đây là chữ Mờ trong con mèo.", ["M", "N", "H"], "M");
        yield return Listen("seed-listen-letter-n", "Nghe và chọn chữ N", "kham-pha-chu", "Đây là chữ Nờ trong nụ hoa.", ["N", "M", "L"], "N");
        yield return Listen("seed-listen-letter-o", "Nghe và chọn chữ O", "kham-pha-chu", "Đây là chữ O tròn như quả trứng gà.", ["O", "Ô", "Ơ"], "O");

        // 3. Nhận biết âm vần
        yield return Listen("seed-listen-rhyme", "Nghe từ có vần an", "am-van", "Từ cái bàn có vần an.", ["Bàn", "Bé", "Bò"], "Bàn");
        yield return Listen("seed-listen-rhyme-2", "Nghe từ có vần am", "am-van", "Quả cam có vần am.", ["Cam", "Cá", "Cây"], "Cam");
        yield return Listen("seed-listen-rhyme-ap", "Nghe từ có vần ap", "am-van", "Chiếc cặp sách có vần ap.", ["Cặp", "Cá", "Cơm"], "Cặp");
        yield return Listen("seed-listen-rhyme-oc", "Nghe từ có vần oc", "am-van", "Chú con cóc có vần oc.", ["Cóc", "Cá", "Cua"], "Cóc");
        yield return Listen("seed-listen-rhyme-en", "Nghe từ có vần en", "am-van", "Ngọn đèn sáng có vần en.", ["Đèn", "Đá", "Đi"], "Đèn");
    }

    private static IEnumerable<SeedLesson> BuildDragLessons()
    {
        // 1. Ghép chữ hoa - thường
        // Bản ghi mã cũ vẫn được quản lý để sửa payload thiếu lựa chọn trên CSDL hiện hữu.
        yield return Drag("seed-drag-uppercase", "Kéo chữ hoa A đúng", "ghep-hoa-thuong", "Chữ hoa A", ["A", "a", "ă"], "A");
        yield return Drag("seed-drag-uppercase-a", "Kéo chữ hoa A đúng", "ghep-hoa-thuong", "Chữ hoa A", ["A", "a", "ă"], "A");
        yield return Drag("seed-drag-uppercase-b", "Kéo chữ hoa B đúng", "ghep-hoa-thuong", "Chữ hoa B", ["B", "b", "d"], "B");
        yield return Drag("seed-drag-uppercase-c", "Kéo chữ hoa C đúng", "ghep-hoa-thuong", "Chữ hoa C", ["C", "c", "o"], "C");
        yield return Drag("seed-drag-uppercase-d", "Kéo chữ hoa D đúng", "ghep-hoa-thuong", "Chữ hoa D", ["D", "d", "đ"], "D");
        yield return Drag("seed-drag-uppercase-m", "Kéo chữ hoa M đúng", "ghep-hoa-thuong", "Chữ hoa M", ["M", "m", "n"], "M");

        // 2. Ghép số với số lượng
        yield return Drag("seed-drag-number-2", "Kéo số vào nhóm hai quả cam", "ghep-so-luong", "Nhóm có 2 vật", ["1", "2", "3"], "2");
        yield return Drag("seed-drag-number-3", "Kéo số vào nhóm ba vật", "ghep-so-luong", "Nhóm có 3 vật", ["2", "3", "4"], "3");
        yield return Drag("seed-drag-number-5", "Kéo số vào nhóm năm ngôi sao", "ghep-so-luong", "Nhóm có 5 vật", ["4", "5", "6"], "5");
        yield return Drag("seed-drag-number-4", "Kéo số vào nhóm bốn chú cá", "ghep-so-luong", "Nhóm có 4 vật", ["3", "4", "5"], "4");

        // 3. Vị trí & Không gian
        yield return Drag("seed-drag-position-in", "Đặt quả bóng vào trong hộp", "vi-tri", "Trong hộp", ["Quả bóng", "Cái bàn", "Đám mây"], "Quả bóng");
        yield return Drag("seed-drag-position-on", "Đặt quyển sách lên trên bàn", "vi-tri", "Trên bàn", ["Quyển sách", "Chiếc ô", "Quả táo"], "Quyển sách");
        yield return Drag("seed-drag-position-under", "Đặt đôi giày dưới gầm giường", "vi-tri", "Dưới gầm giường", ["Đôi giày", "Cái mũ", "Bông hoa"], "Đôi giày");

        // 4. Phương tiện & Nơi đỗ
        yield return Drag("seed-drag-vehicle-garage", "Đưa ô tô vào bãi đỗ", "vi-tri", "Bãi đỗ xe", ["Ô tô", "Quả táo", "Con mèo"], "Ô tô");
        yield return Drag("seed-drag-vehicle-airport", "Đưa máy bay về sân bay", "giao-thong", "Sân bay", ["Máy bay", "Thuyền", "Xe đạp"], "Máy bay");
        yield return Drag("seed-drag-vehicle-port", "Đưa thuyền buồm về bến cảng", "giao-thong", "Bến cảng", ["Thuyền buồm", "Ô tô", "Xe buýt"], "Thuyền buồm");

        // 5. Phân loại đồ vật & Thức ăn
        yield return Drag("seed-drag-fruit", "Bỏ táo vào giỏ hoa quả", "phan-loai", "Giỏ trái cây", ["Táo", "Bút", "Cái thìa"], "Táo");
        yield return Drag("seed-drag-veggie", "Bỏ cà rốt vào rổ rau củ", "phan-loai", "Rổ rau củ", ["Cà rốt", "Cặp sách", "Cái bát"], "Cà rốt");
        yield return Drag("seed-drag-school-tool", "Bỏ bút chì vào hộp bút", "phan-loai", "Hộp bút", ["Bút chì", "Nồi canh", "Quả bóng"], "Bút chì");

        // 6. Con vật & Tổ ấm
        yield return Drag("seed-drag-bird-nest", "Đưa chim con về tổ", "con-vat", "Tổ chim trên cây", ["Chim non", "Cá vàng", "Con thỏ"], "Chim non");
        yield return Drag("seed-drag-fish-water", "Đưa cá vàng về hồ nước", "con-vat", "Hồ nước trong xanh", ["Cá vàng", "Mèo con", "Gà con"], "Cá vàng");
        yield return Drag("seed-drag-rabbit-burrow", "Đưa chú thỏ về hang cỏ", "con-vat", "Hang thỏ", ["Chú thỏ", "Con vịt", "Máy bay"], "Chú thỏ");
    }

    private static IEnumerable<SeedLesson> BuildMatchingLessons()
    {
        // 1. Chữ hoa - chữ thường
        yield return Mapping("seed-match-case-1", "Nối chữ hoa với chữ thường 1", "ghep-hoa-thuong", InteractionTypes.Matching, [("A", "a"), ("B", "b"), ("C", "c")]);
        yield return Mapping("seed-match-case-2", "Nối chữ hoa với chữ thường 2", "ghep-hoa-thuong", InteractionTypes.Matching, [("D", "d"), ("Đ", "đ"), ("E", "e")]);
        yield return Mapping("seed-match-case-3", "Nối chữ hoa với chữ thường 3", "ghep-hoa-thuong", InteractionTypes.Matching, [("G", "g"), ("H", "h"), ("I", "i")]);
        yield return Mapping("seed-match-case-4", "Nối chữ hoa với chữ thường 4", "ghep-hoa-thuong", InteractionTypes.Matching, [("M", "m"), ("N", "n"), ("O", "o")]);
        yield return Mapping("seed-match-case-5", "Nối chữ hoa với chữ thường 5", "ghep-hoa-thuong", InteractionTypes.Matching, [("U", "u"), ("Ư", "ư"), ("V", "v")]);

        // 2. Hình khối & Màu sắc
        yield return Mapping("seed-match-shape-1", "Nối hình cơ bản với tên gọi", "hinh-dang", InteractionTypes.Matching, [("○", "Hình tròn"), ("□", "Hình vuông"), ("△", "Hình tam giác")]);
        yield return Mapping("seed-match-shape-2", "Nối hình trang trí với tên gọi", "hinh-dang", InteractionTypes.Matching, [("⭐", "Hình ngôi sao"), ("❤️", "Hình trái tim"), ("■", "Hình vuông")]);

        // 3. Con vật & Tiếng kêu & Thức ăn
        yield return Mapping("seed-match-vocabulary", "Nối con vật với tiếng kêu", "von-tu", InteractionTypes.Matching, [("Mèo", "Meo meo"), ("Chó", "Gâu gâu"), ("Vịt", "Cạp cạp")]);
        yield return Mapping("seed-match-food-animal", "Nối con vật với thức ăn yêu thích", "con-vat", InteractionTypes.Matching, [("Thỏ", "Cà rốt"), ("Mèo", "Cá"), ("Ong", "Bông hoa")]);
        yield return Mapping("seed-match-food-animal-2", "Nối thức ăn cho các loài vật", "con-vat", InteractionTypes.Matching, [("Gà", "Hạt thóc"), ("Khỉ", "Quả chuối"), ("Bò", "Cỏ tươi")]);
        yield return Mapping("seed-match-mother-baby", "Nối mẹ và con yêu", "con-vat", InteractionTypes.Matching, [("Gà mẹ", "Gà con"), ("Mèo mẹ", "Mèo con"), ("Vịt mẹ", "Vịt con")]);
        yield return Mapping("seed-match-animal-home", "Nối con vật với ngôi nhà của mình", "con-vat", InteractionTypes.Matching, [("Chim", "Tổ cây"), ("Cá", "Hồ nước"), ("Ong", "Tổ ong")]);

        // 4. Đồ dùng ghép đôi
        yield return Mapping("seed-match-pair-tools", "Nối các cặp đồ vật quen thuộc", "phan-loai", InteractionTypes.Matching, [("Bàn chải", "Kem đánh răng"), ("Bút", "Vở"), ("Bát", "Thìa")]);
        yield return Mapping("seed-match-pair-clothes", "Nối các trang phục đi cùng nhau", "thoi-tiet", InteractionTypes.Matching, [("Áo", "Quần"), ("Giày", "Tất"), ("Mũ", "Khăn")]);

        // 5. Biển báo & Đèn giao thông
        yield return Mapping("seed-match-traffic-light", "Nối tín hiệu đèn giao thông", "an-toan", InteractionTypes.Matching, [("Đèn đỏ", "Dừng lại"), ("Đèn xanh", "Được đi"), ("Đèn vàng", "Đi chậm")]);

        // 6. Số lượng & Chữ số
        yield return Mapping("seed-match-quantity-1to3", "Nối số với số lượng chấm tròn", "ghep-so-luong", InteractionTypes.Matching, [("1", "●"), ("2", "●●"), ("3", "●●●")]);
        yield return Mapping("seed-match-quantity-4to6", "Nối số với số lượng quả táo", "ghep-so-luong", InteractionTypes.Matching, [("4", "🍎🍎🍎🍎"), ("5", "🍎🍎🍎🍎🍎"), ("6", "🍎🍎🍎🍎🍎🍎")]);
    }

    private static IEnumerable<SeedLesson> BuildOrderingLessons()
    {
        // 1. Thứ tự số học
        yield return Ordering("seed-order-numbers", "Sắp xếp số từ bé đến lớn (1-4)", "thu-tu-so", ["1", "2", "3", "4"]);
        yield return Ordering("seed-order-numbers-5to8", "Sắp xếp số từ bé đến lớn (5-8)", "thu-tu-so", ["5", "6", "7", "8"]);
        yield return Ordering("seed-order-numbers-7to10", "Sắp xếp số từ bé đến lớn (7-10)", "thu-tu-so", ["7", "8", "9", "10"]);
        yield return Ordering("seed-order-numbers-desc", "Sắp xếp số từ lớn về bé (4-1)", "thu-tu-so", ["4", "3", "2", "1"]);
        yield return Ordering("seed-order-numbers-desc-8to5", "Sắp xếp số từ lớn về bé (8-5)", "thu-tu-so", ["8", "7", "6", "5"]);
        yield return Ordering("seed-order-numbers-desc-10to7", "Sắp xếp số từ lớn về bé (10-7)", "thu-tu-so", ["10", "9", "8", "7"]);

        // 2. Vệ sinh cá nhân & Thói quen tự phục vụ
        yield return Ordering("seed-order-wash", "Các bước rửa tay đúng cách", "tu-phuc-vu", ["Làm ướt tay", "Lấy xà phòng", "Chà sạch tay", "Xả nước", "Lau khô"]);
        yield return Ordering("seed-order-brush-teeth", "Các bước đánh răng đúng cách", "tu-phuc-vu", ["Lấy kem đánh răng", "Chải mặt ngoài", "Chải mặt trong", "Súc miệng sạch"]);
        yield return Ordering("seed-order-morning-routine", "Thứ tự các việc buổi sáng", "tu-phuc-vu", ["Thức dậy", "Đánh răng", "Ăn sáng", "Đi học"]);
        yield return Ordering("seed-order-bedtime-routine", "Thứ tự các việc trước khi đi ngủ", "tu-phuc-vu", ["Đánh răng", "Mặc đồ ngủ", "Nghe đọc truyện", "Đi ngủ"]);
        yield return Ordering("seed-order-put-shoes", "Các bước xỏ giày đi học", "tu-phuc-vu", ["Xỏ chân vào giày", "Kéo gót giày", "Dán quai giày ngay ngắn"]);
        yield return Ordering("seed-order-pack-backpack", "Chuẩn bị cặp sách đến trường", "tu-phuc-vu", ["Xếp sách vở", "Đặt hộp bút", "Kéo khóa cặp", "Đeo ba lô"]);

        // 3. Vòng đời phát triển tự nhiên
        yield return Ordering("seed-order-seed", "Hạt nảy mầm lớn thành cây", "ke-chuyen", ["Gieo hạt", "Tưới nước", "Hạt nảy mầm", "Cây lớn lên"]);
        yield return Ordering("seed-order-frog", "Vòng đời của chú ếch", "ke-chuyen", ["Trứng ếch", "Nòng nọc", "Ếch con có đuôi", "Chú ếch trưởng thành"]);
        yield return Ordering("seed-order-butterfly", "Vòng đời của chú bướm xinh", "ke-chuyen", ["Trứng bướm", "Sâu bướm", "Chiếc kén", "Bướm xinh bay lượn"]);
        yield return Ordering("seed-order-chicken", "Quả trứng nở thành chú gà con", "ke-chuyen", ["Gà ấp trứng", "Trứng nứt vỏ", "Gà con chui ra", "Gà con theo mẹ"]);

        // 4. An toàn & Khéo tay
        yield return Ordering("seed-order-cross-road", "Các bước qua đường an toàn", "an-toan", ["Đứng trên vỉa hè", "Quan sát hai bên", "Chờ đèn xanh bật", "Đi cùng người lớn"]);
        yield return Ordering("seed-order-peel-banana", "Các bước bóc quả chuối ăn", "kheo-tay", ["Cầm cuống chuối", "Bóc vỏ nhẹ nhàng", "Ăn chuối", "Bỏ vỏ vào thùng rác"]);
    }

    private static IEnumerable<SeedLesson> BuildCountingLessons()
    {
        yield return Counting("seed-count-1", "Đếm 1 quả dưa hấu", "dem-so-luong", "🍉", 1);
        yield return Counting("seed-count-2", "Đếm 2 quả cam mọng nước", "dem-so-luong", "🍊", 2);
        yield return Counting("seed-count-3", "Đếm 3 quả táo đỏ", "dem-so-luong", "🍎", 3);
        yield return Counting("seed-count-4", "Đếm 4 chú cá bơi lội", "dem-so-luong", "🐟", 4);
        yield return Counting("seed-count-5", "Đếm 5 ngôi sao vàng lấp lánh", "dem-so-luong", "⭐", 5);
        yield return Counting("seed-count-6", "Đếm 6 quả dâu tây ngọt lịm", "dem-so-luong", "🍓", 6);
        yield return Counting("seed-count-7", "Đếm 7 bông hoa xinh tươi", "dem-so-luong", "🌼", 7);
        yield return Counting("seed-count-8", "Đếm 8 chú bướm rực rỡ", "dem-so-luong", "🦋", 8);
        yield return Counting("seed-count-9", "Đếm 9 quả bóng bay", "dem-so-luong", "🎈", 9);
        yield return Counting("seed-count-10", "Đếm 10 trái tim yêu thương", "dem-so-luong", "❤️", 10);
    }

    private static IEnumerable<SeedLesson> BuildQuantityLessons()
    {
        yield return Quantity("seed-quantity-1", "Tạo 1 quả dưa hấu", "tao-so-luong", "🍉", 1);
        yield return Quantity("seed-quantity-2", "Tạo 2 quả cam", "tao-so-luong", "🍊", 2);
        yield return Quantity("seed-quantity-3", "Tạo 3 ngôi sao", "tao-so-luong", "⭐", 3);
        yield return Quantity("seed-quantity-4", "Tạo 4 chú cá", "tao-so-luong", "🐟", 4);
        yield return Quantity("seed-quantity-5", "Tạo 5 quả táo đỏ", "tao-so-luong", "🍎", 5);
        yield return Quantity("seed-quantity-6", "Tạo 6 khối vuông", "tao-so-luong", "■", 6);
        yield return Quantity("seed-quantity-7", "Tạo 7 bông hoa", "tao-so-luong", "🌼", 7);
        yield return Quantity("seed-quantity-8", "Tạo 8 quả dâu tây", "tao-so-luong", "🍓", 8);
        yield return Quantity("seed-quantity-9", "Tạo 9 quả bóng", "tao-so-luong", "🎈", 9);
        yield return Quantity("seed-quantity-10", "Tạo 10 chấm tròn", "tao-so-luong", "●", 10);
    }

    private static IEnumerable<SeedLesson> BuildComparisonLessons()
    {
        yield return Comparison("seed-compare-more", "Nhóm nào có nhiều dâu tây hơn?", "so-sanh", "🍓", "Rổ đỏ", 5, "Rổ xanh", 3, "more");
        yield return Comparison("seed-compare-more-apples", "Đĩa nào có nhiều táo hơn?", "so-sanh", "🍎", "Đĩa trái", 7, "Đĩa phải", 4, "more");
        yield return Comparison("seed-compare-more-stars", "Bầu trời nào có nhiều sao hơn?", "so-sanh", "⭐", "Trời đêm A", 8, "Trời đêm B", 5, "more");
        yield return Comparison("seed-compare-less", "Nhóm nào có ít ngôi sao hơn?", "so-sanh", "⭐", "Nhóm vàng", 2, "Nhóm xanh", 6, "less");
        yield return Comparison("seed-compare-less-oranges", "Rổ nào có ít cam hơn?", "so-sanh", "🍊", "Rổ A", 3, "Rổ B", 6, "less");
        yield return Comparison("seed-compare-less-fish", "Bể nào có ít cá bơi hơn?", "so-sanh", "🐟", "Bể trái", 2, "Bể phải", 5, "less");
        yield return Comparison("seed-compare-equal", "Hai nhóm có bằng nhau không?", "so-sanh", "●", "Nhóm A", 4, "Nhóm B", 4, "equal");
        yield return Comparison("seed-compare-equal-hearts", "Hai hộp quà có số trái tim bằng nhau?", "so-sanh", "❤️", "Hộp A", 5, "Hộp B", 5, "equal");
        yield return Comparison("seed-compare-animals", "Bể nào có nhiều cá hơn?", "so-sanh", "🐟", "Bể trái", 6, "Bể phải", 2, "more");
        yield return Comparison("seed-compare-flowers", "Vườn hoa nào có nhiều hoa hơn?", "so-sanh", "🌼", "Vườn A", 8, "Vườn B", 4, "more");
    }

    private static IEnumerable<SeedLesson> BuildClassificationLessons()
    {
        // 1. Thực phẩm & Dinh dưỡng
        yield return Mapping("seed-classify-food", "Phân loại rau củ và trái cây", "phan-loai", InteractionTypes.Classification, [("Táo", "Trái cây"), ("Cam", "Trái cây"), ("Cà rốt", "Rau củ"), ("Bắp cải", "Rau củ")]);
        yield return Mapping("seed-classify-food-2", "Phân loại đồ ăn và thức uống", "phan-loai", InteractionTypes.Classification, [("Bánh mì", "Đồ ăn"), ("Cơm", "Đồ ăn"), ("Sữa tươi", "Thức uống"), ("Nước cam", "Thức uống")]);

        // 2. Động vật & Môi trường
        yield return Mapping("seed-classify-animal", "Phân loại con vật trên cạn và dưới nước", "con-vat", InteractionTypes.Classification, [("Cá", "Dưới nước"), ("Tôm", "Dưới nước"), ("Mèo", "Trên cạn"), ("Gà", "Trên cạn")]);
        yield return Mapping("seed-classify-animal-2", "Phân loại chim bay và thú nuôi", "con-vat", InteractionTypes.Classification, [("Chim sẻ", "Biết bay"), ("Bồ câu", "Biết bay"), ("Chó cưng", "Thú nuôi"), ("Mèo con", "Thú nuôi")]);

        // 3. Thời tiết & Trang phục
        yield return Mapping("seed-classify-weather", "Chọn đồ dùng theo thời tiết nắng mưa", "thoi-tiet", InteractionTypes.Classification, [("Áo mưa", "Trời mưa"), ("Ô", "Trời mưa"), ("Mũ rộng vành", "Trời nắng"), ("Kính râm", "Trời nắng")]);
        yield return Mapping("seed-classify-season", "Trang phục mùa hè và mùa đông", "thoi-tiet", InteractionTypes.Classification, [("Áo phông", "Mùa hè"), ("Quần đùi", "Mùa hè"), ("Áo khoác len", "Mùa đông"), ("Khăn quàng", "Mùa đông")]);

        // 4. Phương tiện giao thông
        yield return Mapping("seed-classify-transport", "Phân loại phương tiện giao thông đường bộ và đường khác", "giao-thong", InteractionTypes.Classification, [("Ô tô", "Trên đường"), ("Xe đạp", "Trên đường"), ("Máy bay", "Trên trời"), ("Thuyền", "Dưới nước")]);

        // 5. Đồ dùng trong gia đình
        yield return Mapping("seed-classify-house-tools", "Phân loại đồ dùng học tập và nhà bếp", "phan-loai", InteractionTypes.Classification, [("Bút chì", "Học tập"), ("Quyển vở", "Học tập"), ("Nồi canh", "Nhà bếp"), ("Cái chảo", "Nhà bếp")]);

        // 6. Tính chất & Cảm giác
        yield return Mapping("seed-classify-soft-hard", "Phân loại đồ vật mềm và cứng", "phan-loai", InteractionTypes.Classification, [("Chiếc gối", "Mềm mại"), ("Bông gòn", "Mềm mại"), ("Viên đá", "Cứng cáp"), ("Cục gạch", "Cứng cáp")]);
        yield return Mapping("seed-classify-hot-cold", "Phân loại đồ nóng và đồ lạnh", "phan-loai", InteractionTypes.Classification, [("Kem dâu", "Lạnh"), ("Viên đá lạnh", "Lạnh"), ("Bát súp nóng", "Nóng"), ("Ly trà ấm", "Nóng")]);
    }

    private static IEnumerable<SeedLesson> BuildStoryLessons()
    {
        yield return Story("seed-story-wash", "Câu chuyện rửa tay sạch sẽ", "tu-phuc-vu",
            "Trước khi ăn cơm, bạn Minh luôn nhớ làm ướt tay, xoa xà phòng chà sạch mu bàn tay và các kẽ ngón tay, sau đó rửa lại bằng nước sạch rồi lau khô.",
            "/images/lessons/story-wash-hands.png", "Minh làm gì trước khi ngồi vào bàn ăn cơm?", ["Rửa tay sạch với xà phòng", "Đi ngủ ngay", "Cất sách vở"], "Rửa tay sạch với xà phòng");
        yield return Story("seed-story-crossing", "Bé qua đường an toàn", "an-toan",
            "Lan đứng trên vỉa hè nắm chặt tay mẹ. Khi đèn tín hiệu cho người đi bộ chuyển sang màu xanh, hai mẹ con quan sát xe rồi đi trên vạch sơn trắng.",
            "/images/lessons/story-safe-crossing.png", "Khi nào bé Lan và mẹ mới bước qua đường?", ["Khi đèn người đi bộ màu xanh", "Khi xe cộ đang chạy nhanh", "Khi đèn người đi bộ màu đỏ"], "Khi đèn người đi bộ màu xanh");
        yield return Story("seed-story-sharing", "Bạn bè biết sẻ chia", "cam-xuc",
            "Trong giờ vẽ tranh, Nam buồn vì để quên hộp bút sáp ở nhà. Thấy bạn buồn, Mai liền vui vẻ chia sẻ hộp màu của mình để hai bạn cùng vẽ tranh.",
            "/images/lessons/story-sharing.png", "Bạn Mai đã làm gì khi thấy bạn Nam buồn?", ["Chia sẻ bút màu cho bạn mượn", "Cất hết bút đi", "Cười chê bạn"], "Chia sẻ bút màu cho bạn mượn");
        yield return Story("seed-story-traffic", "Đội mũ bảo hiểm khi đi xe máy", "an-toan",
            "Bố đón An tan học về. An tự giác cầm mũ bảo hiểm của mình, đội lên đầu và nhờ bố cài quai chắc chắn trước khi lên xe máy.",
            "/images/lessons/visual-road-safety.png", "An làm việc gì trước khi lên xe máy cùng bố?", ["Đội mũ bảo hiểm an toàn", "Đứng nhảy nhót đùa nghịch", "Cởi giày vứt đi"], "Đội mũ bảo hiểm an toàn");
        yield return Story("seed-story-rabbit-turtle", "Chuyện Rùa và Thỏ", "ke-chuyen",
            "Trong cuộc thi chạy, Thỏ cậy mình nhanh nhẹn nên mải đuổi bướm hái hoa và ngủ quên. Rùa tuy đi chậm nhưng kiên trì, chăm chỉ bước từng bước nên đã về đích trước.",
            "/images/photos/flashcard-rabbit.jpg", "Vì sao bạn Rùa lại chiến thắng trong cuộc thi chạy?", ["Rùa kiên trì và chăm chỉ", "Rùa chạy nhanh hơn Thỏ", "Thỏ nhường giải cho Rùa"], "Rùa kiên trì và chăm chỉ");
        yield return Story("seed-story-kind-bear", "Bác Gấu đen và hai chú Thỏ", "giao-tiep",
            "Đêm mưa gió lạnh buốt, bác Gấu đen ướt sũng đến gõ cửa xin trú nhờ. Thỏ Nâu và Thỏ Trắng đã vui vẻ mời bác Gấu vào nhà, đốt lửa sưởi ấm và mời bác ăn cà rốt ngọt.",
            "/images/photos/flashcard-carrot.jpg", "Hai chú Thỏ đã làm gì khi bác Gấu đen đến xin trú mưa?", ["Mời bác vào sưởi ấm và cho ăn", "Đóng chặt cửa đuổi bác đi", "Chạy trốn khỏi nhà"], "Mời bác vào sưởi ấm và cho ăn");
        yield return Story("seed-story-ant-grasshopper", "Kiến chăm chỉ và Ve sầu", "ke-chuyen",
            "Suốt mùa hè ấm áp, đàn Kiến chăm chỉ tìm thức ăn mang về tổ dự trữ. Chú Ve sầu chỉ mải ca hát cả ngày. Đến mùa đông lạnh giá, Kiến có đồ ăn no ấm còn Ve sầu đói run rẩy.",
            "/images/photos/flashcard-insects.jpg", "Bài học rút ra từ câu chuyện đàn Kiến và Ve sầu là gì?", ["Chăm chỉ lao động, biết lo xa", "Chỉ nên mải chơi ca hát", "Không cần giúp đỡ ai"], "Chăm chỉ lao động, biết lo xa");
    }

    private static IEnumerable<SeedLesson> BuildReadingComprehensionLessons()
    {
        // 1. Chú gấu và mật ong
        const string gauSpeech = "Một buổi sáng đẹp trời, chú gấu nâu đi dạo trong rừng và ngửi thấy mùi thơm ngọt ngào của mật ong. Chú lần theo mùi hương và tìm thấy tổ ong trên cành cây cao. Chú gấu leo lên cây, nhẹ nhàng lấy một ít mật ong vào chiếc hũ nhỏ mình mang theo. Xong rồi, chú nói: “Chỉ lấy vừa đủ thôi để không làm phiền các bạn ong nhé!” rồi leo xuống và đi về.";
        yield return ReadingQuestion("seed-story-gau-mat-ong-q1", "Chú gấu và mật ong (1/5)", gauSpeech, "gau-mat-ong", "q1", "Chú gấu đi dạo ở đâu?", ["Trong rừng", "Ở trường", "Trong thành phố"], "Trong rừng");
        yield return ReadingQuestion("seed-story-gau-mat-ong-q2", "Chú gấu và mật ong (2/5)", gauSpeech, "gau-mat-ong", "q2", "Chú gấu ngửi thấy mùi gì?", ["Mùi hoa", "Mùi mật ong", "Mùi thức ăn"], "Mùi mật ong");
        yield return ReadingQuestion("seed-story-gau-mat-ong-q3", "Chú gấu và mật ong (3/5)", gauSpeech, "gau-mat-ong", "q3", "Tổ ong ở đâu?", ["Trên cành cây", "Dưới bụi cây", "Trong hang đá"], "Trên cành cây");
        yield return ReadingQuestion("seed-story-gau-mat-ong-q4", "Chú gấu và mật ong (4/5)", gauSpeech, "gau-mat-ong", "q4", "Vì sao chú gấu chỉ lấy một ít mật ong?", ["Vì mật ong không ngon lắm", "Vì chú gấu tốt bụng và không muốn làm phiền các bạn ong", "Vì chú gấu vội nên không lấy được nhiều"], "Vì chú gấu tốt bụng và không muốn làm phiền các bạn ong");
        yield return ReadingQuestion("seed-story-gau-mat-ong-q5", "Chú gấu và mật ong (5/5)", gauSpeech, "gau-mat-ong", "q5", "Theo con, chú gấu là một bạn như thế nào?", ["Tốt bụng", "Ích kỷ", "Nóng tính"], "Tốt bụng");

        // 2. Cáo nhỏ và chiếc khăn
        const string caoSpeech = "Mùa đông đã đến. Gió thổi lạnh buốt cả khu rừng. Cáo nhỏ thấy các bạn đều có khăn quàng cổ. Cáo cũng rất muốn có một chiếc khăn thật đẹp. Một hôm, cáo nhỏ nhìn thấy một chiếc khăn màu xanh nằm trên cành cây. Cáo nghĩ chắc bạn nào đánh rơi. Cáo liền nhảy lên, lấy chiếc khăn rồi quàng vào cổ. Chiếc khăn mềm mại và ấm áp quá! Đang lúc cáo nhỏ vui vẻ chạy đi khoe thì sóc nâu hớt hải chạy đến: – Chiếc khăn đó của tớ! Tớ đánh rơi khi đi tìm hạt dẻ. Cáo nhỏ ngại ngùng nói: – Tớ không biết đó là của cậu. Tớ chỉ thấy đẹp quá nên lấy thôi. Sóc nâu mỉm cười: – Cảm ơn cậu đã nói thật. Cậu trả lại tớ nhé! Cáo nhỏ vội tháo khăn trả lại cho sóc. Sóc nâu cảm ơn cáo và rủ cáo cùng đi tìm hạt dẻ. Từ đó, cáo nhỏ hiểu rằng: “Nhặt được của rơi thì phải trả lại cho người mất.”";
        yield return ReadingQuestion("seed-story-cao-khan-q1", "Cáo nhỏ và chiếc khăn (1/6)", caoSpeech, "cao-chiec-khan", "q1", "Câu chuyện xảy ra vào mùa nào?", ["Mùa xuân", "Mùa hè", "Mùa đông"], "Mùa đông");
        yield return ReadingQuestion("seed-story-cao-khan-q2", "Cáo nhỏ và chiếc khăn (2/6)", caoSpeech, "cao-chiec-khan", "q2", "Cáo nhỏ nhìn thấy gì trên cành cây?", ["Quả táo", "Chiếc khăn", "Quả thông"], "Chiếc khăn");
        yield return ReadingQuestion("seed-story-cao-khan-q3", "Cáo nhỏ và chiếc khăn (3/6)", caoSpeech, "cao-chiec-khan", "q3", "Chiếc khăn màu gì?", ["Màu đỏ", "Màu xanh", "Màu vàng"], "Màu xanh");
        yield return ReadingQuestion("seed-story-cao-khan-q4", "Cáo nhỏ và chiếc khăn (4/6)", caoSpeech, "cao-chiec-khan", "q4", "Vì sao cáo nhỏ lấy chiếc khăn?", ["Vì cáo thấy chiếc khăn đẹp nên lấy", "Vì cáo bị lạnh nên lấy để quàng cho ấm", "Vì cáo nghĩ đó là của mình"], "Vì cáo thấy chiếc khăn đẹp nên lấy");
        yield return ReadingQuestion("seed-story-cao-khan-q5", "Cáo nhỏ và chiếc khăn (5/6)", caoSpeech, "cao-chiec-khan", "q5", "Cáo nhỏ đã làm gì khi biết chiếc khăn là của sóc nâu?", ["Trả lại khăn cho sóc nâu", "Mang khăn về và chạy đi", "Xé chiếc khăn làm đôi"], "Trả lại khăn cho sóc nâu");
        yield return ReadingQuestion("seed-story-cao-khan-q6", "Cáo nhỏ và chiếc khăn (6/6)", caoSpeech, "cao-chiec-khan", "q6", "Theo con, chúng ta cần học điều gì từ câu chuyện này?", ["Cho đi để nhận lại", "Nhặt được của rơi phải trả lại", "Kết bạn và chia sẻ"], "Nhặt được của rơi phải trả lại");

        // 3. Chiếc ô màu vàng
        const string oVangSpeech = "Sáng nay, trời có mưa. Bé Na mang theo một chiếc ô màu vàng. Trên đường đến trường, Na gặp một chú mèo nhỏ đang trú dưới gốc cây. Na dừng lại, che ô cho chú mèo rồi mới tiếp tục đến lớp.";
        yield return ReadingQuestion("seed-story-o-vang-q1", "Chiếc ô màu vàng (1/5)", oVangSpeech, "chiec-o-vang", "q1", "Chiếc ô của Na màu gì?", ["Xanh", "Vàng", "Đỏ"], "Vàng");
        yield return ReadingQuestion("seed-story-o-vang-q2", "Chiếc ô màu vàng (2/5)", oVangSpeech, "chiec-o-vang", "q2", "Na gặp con vật nào trên đường đến trường?", ["Chó", "Mèo", "Thỏ"], "Mèo");
        yield return ReadingQuestion("seed-story-o-vang-q3", "Chiếc ô màu vàng (3/5)", oVangSpeech, "chiec-o-vang", "q3", "Chú mèo đang trú ở đâu?", ["Dưới gốc cây", "Trong nhà", "Ở trường"], "Dưới gốc cây");
        yield return ReadingQuestion("seed-story-o-vang-q4", "Chiếc ô màu vàng (4/5)", oVangSpeech, "chiec-o-vang", "q4", "Vì sao Na dừng lại?", ["Vì Na muốn quên đường", "Vì Na muốn giúp chú mèo", "Vì Na muốn chơi"], "Vì Na muốn giúp chú mèo");
        yield return ReadingQuestion("seed-story-o-vang-q5", "Chiếc ô màu vàng (5/5)", oVangSpeech, "chiec-o-vang", "q5", "Con thấy Na là một bạn như thế nào?", ["Tốt bụng", "Ích kỷ", "Nóng tính"], "Tốt bụng");

        // 4. Chú sóc và hạt dẻ
        const string socDeSpeech = "Mùa thu đến, lá cây vàng đỏ khắp nơi. Chú sóc nâu chăm chỉ nhặt những hạt dẻ rơi dưới gốc cây lớn. Sóc nhặt từng hạt, bỏ vào giỏ nhỏ. Nhặt xong, sóc đào một cái hố và chôn giỏ hạt dẻ xuống đất. Sóc nghĩ: “Mùa đông đến, mình sẽ có thật nhiều hạt dẻ ngon để ăn.”";
        yield return ReadingQuestion("seed-story-soc-de-q1", "Chú sóc và hạt dẻ (1/5)", socDeSpeech, "soc-hat-de", "q1", "Câu chuyện diễn ra vào mùa nào?", ["Mùa xuân", "Mùa hè", "Mùa thu"], "Mùa thu");
        yield return ReadingQuestion("seed-story-soc-de-q2", "Chú sóc và hạt dẻ (2/5)", socDeSpeech, "soc-hat-de", "q2", "Sóc nâu chăm chỉ nhặt gì dưới gốc cây?", ["Hạt dẻ", "Táo", "Nấm"], "Hạt dẻ");
        yield return ReadingQuestion("seed-story-soc-de-q3", "Chú sóc và hạt dẻ (3/5)", socDeSpeech, "soc-hat-de", "q3", "Sau khi nhặt xong, sóc đã làm gì?", ["Bỏ vào giỏ", "Đào hố chôn giỏ hạt dẻ", "Chạy về nhà ngay"], "Đào hố chôn giỏ hạt dẻ");
        yield return ReadingQuestion("seed-story-soc-de-q4", "Chú sóc và hạt dẻ (4/5)", socDeSpeech, "soc-hat-de", "q4", "Vì sao sóc chôn hạt dẻ xuống đất?", ["Vì sóc không biết để ở đâu", "Vì sóc muốn giữ hạt dẻ cho mùa đông", "Vì sóc muốn chơi đùa với hạt dẻ"], "Vì sóc muốn giữ hạt dẻ cho mùa đông");
        yield return ReadingQuestion("seed-story-soc-de-q5", "Chú sóc và hạt dẻ (5/5)", socDeSpeech, "soc-hat-de", "q5", "Theo con, chú sóc trong câu chuyện là một bạn như thế nào?", ["Chăm chỉ", "Lười biếng", "Ích kỷ"], "Chăm chỉ");

        // 5. Chú chim non tập bay
        const string chimBaySpeech = "Sáng nay, trong tổ có ba chú chim non. Mẹ chim đã đi kiếm mồi từ sớm. Một chú chim non nhìn ra ngoài và nói: “Con muốn tập bay như các bạn!” Chú nhảy lên cành cây nhỏ, dang đôi cánh bé xíu và cố gắng bay. Nhưng chú chỉ lượn được một đoạn ngắn rồi rơi xuống cành thấp. Chú hơi buồn. Mẹ chim bay về, nhẹ nhàng nói: “Lần đầu chưa được thì không sao. Con cứ tập mỗi ngày, con sẽ bay thật giỏi!” Chú chim non mỉm cười và hứa sẽ cố gắng hơn.";
        yield return ReadingQuestion("seed-story-chim-bay-q1", "Chú chim non tập bay (1/5)", chimBaySpeech, "chim-non-tap-bay", "q1", "Trong tổ có bao nhiêu chú chim non?", ["2", "3", "4"], "3");
        yield return ReadingQuestion("seed-story-chim-bay-q2", "Chú chim non tập bay (2/5)", chimBaySpeech, "chim-non-tap-bay", "q2", "Ai đã đi kiếm mồi từ sớm?", ["Chú chim non", "Mẹ chim", "Chú sóc"], "Mẹ chim");
        yield return ReadingQuestion("seed-story-chim-bay-q3", "Chú chim non tập bay (3/5)", chimBaySpeech, "chim-non-tap-bay", "q3", "Chú chim non tập bay như thế nào?", ["Nhảy lên cành cây và dang cánh", "Nhảy xuống suối để bơi", "Chạy thật nhanh trên sân"], "Nhảy lên cành cây và dang cánh");
        yield return ReadingQuestion("seed-story-chim-bay-q4", "Chú chim non tập bay (4/5)", chimBaySpeech, "chim-non-tap-bay", "q4", "Vì sao chú chim non lúc đầu hơi buồn?", ["Vì mẹ chim chưa về", "Vì chú rơi xuống cành thấp, chưa bay được xa", "Vì chú không thích ăn giun"], "Vì chú rơi xuống cành thấp, chưa bay được xa");
        yield return ReadingQuestion("seed-story-chim-bay-q5", "Chú chim non tập bay (5/5)", chimBaySpeech, "chim-non-tap-bay", "q5", "Theo con, chú chim non là một bạn như thế nào?", ["Chăm chỉ và không bỏ cuộc", "Ích kỷ và chỉ nghĩ cho mình", "Lười biếng và hay nản chí"], "Chăm chỉ và không bỏ cuộc");

        // 6. Rùa nhỏ và dòng suối
        const string ruaSuoiSpeech = "Rùa nhỏ sống ở gần một dòng suối trong xanh. Ngày nào rùa cũng muốn sang bờ bên kia để ăn những chiếc lá non. Nhưng dòng suối khá rộng và nước chảy rất xiết, khiến rùa nhỏ không dám bơi qua. Một ngày, thỏ trắng thấy rùa ngồi buồn bên bờ suối liền hỏi: – Bạn buồn vì điều gì vậy? Rùa nhỏ kể lại nỗi sợ của mình. Thỏ trắng mỉm cười và nói: – Mình sẽ giúp bạn! Nói rồi, thỏ nhặt những cành cây và lá chuối, buộc thành một chiếc bè nhỏ. Thỏ dắt rùa lên bè và nhẹ nhàng đẩy qua suối. Rùa nhỏ cảm động nói: “Cảm ơn bạn rất nhiều! Nhờ bạn mà mình đã có bữa ăn ngon lành.” Từ đó, rùa nhỏ và thỏ trắng trở thành những người bạn tốt của nhau.";
        yield return ReadingQuestion("seed-story-rua-suoi-q1", "Rùa nhỏ và dòng suối (1/5)", ruaSuoiSpeech, "rua-dong-suoi", "q1", "Rùa nhỏ muốn làm gì?", ["Ăn lá non", "Về nhà", "Ngắm hoa"], "Ăn lá non");
        yield return ReadingQuestion("seed-story-rua-suoi-q2", "Rùa nhỏ và dòng suối (2/5)", ruaSuoiSpeech, "rua-dong-suoi", "q2", "Vì sao rùa nhỏ không dám qua suối?", ["Nước rất sâu", "Nhiều đá", "Nước chảy xiết"], "Nước chảy xiết");
        yield return ReadingQuestion("seed-story-rua-suoi-q3", "Rùa nhỏ và dòng suối (3/5)", ruaSuoiSpeech, "rua-dong-suoi", "q3", "Ai đã giúp rùa nhỏ qua suối?", ["Sóc nâu", "Thỏ trắng", "Gà con"], "Thỏ trắng");
        yield return ReadingQuestion("seed-story-rua-suoi-q4", "Rùa nhỏ và dòng suối (4/5)", ruaSuoiSpeech, "rua-dong-suoi", "q4", "Để giúp rùa nhỏ, thỏ trắng đã làm gì?", ["Nhặt cành cây và lá chuối buộc thành chiếc bè", "Bơi qua suối và cõng rùa sang bờ kia", "Gọi bạn đến rồi khiêng rùa qua suối"], "Nhặt cành cây và lá chuối buộc thành chiếc bè");
        yield return ReadingQuestion("seed-story-rua-suoi-q5", "Rùa nhỏ và dòng suối (5/5)", ruaSuoiSpeech, "rua-dong-suoi", "q5", "Theo con, rùa nhỏ và thỏ trắng là những người bạn như thế nào?", ["Quan tâm và giúp đỡ nhau", "Không thích nhau", "Hay cãi nhau"], "Quan tâm và giúp đỡ nhau");

        // 7. Chiếc hộp bí mật
        const string hopBiMatSpeech = "Hôm nay là sinh nhật của Bin. Buổi sáng, Bin nhận được một chiếc hộp nhỏ từ mẹ. Hộp được gói bằng giấy màu xanh và buộc nơ vàng rất đẹp. Bin mở hộp ra, bên trong là một quyển sách tranh mà Bin rất thích. Bin ôm mẹ và nói: “Con cảm ơn mẹ nhiều lắm! Đây là món quà tuyệt vời nhất!”";
        yield return ReadingQuestion("seed-story-hop-bi-mat-q1", "Chiếc hộp bí mật (1/5)", hopBiMatSpeech, "chiec-hop-bi-mat", "q1", "Hôm nay là ngày gì của Bin?", ["Sinh nhật", "Khai giảng", "Giáng sinh"], "Sinh nhật");
        yield return ReadingQuestion("seed-story-hop-bi-mat-q2", "Chiếc hộp bí mật (2/5)", hopBiMatSpeech, "chiec-hop-bi-mat", "q2", "Bin nhận được gì từ mẹ?", ["Quyển sách", "Gấu bông", "Ô tô đồ chơi"], "Quyển sách");
        yield return ReadingQuestion("seed-story-hop-bi-mat-q3", "Chiếc hộp bí mật (3/5)", hopBiMatSpeech, "chiec-hop-bi-mat", "q3", "Hộp quà được gói màu gì và buộc nơ màu gì?", ["Xanh - đỏ", "Xanh - vàng", "Hồng - tím"], "Xanh - vàng");
        yield return ReadingQuestion("seed-story-hop-bi-mat-q4", "Chiếc hộp bí mật (4/5)", hopBiMatSpeech, "chiec-hop-bi-mat", "q4", "Vì sao Bin nói đây là món quà tuyệt vời nhất?", ["Vì quyển sách rất đẹp", "Vì đó là món quà mẹ tặng cho Bin", "Vì Bin chỉ thích nhận quà"], "Vì đó là món quà mẹ tặng cho Bin");
        yield return ReadingQuestion("seed-story-hop-bi-mat-q5", "Chiếc hộp bí mật (5/5)", hopBiMatSpeech, "chiec-hop-bi-mat", "q5", "Con thấy Bin là một bạn như thế nào?", ["Biết ơn và lễ phép", "Ích kỷ", "Nóng tính"], "Biết ơn và lễ phép");

        // 8. Chú nhím và quả táo
        const string nhimTaoSpeech = "Buổi chiều, chú nhím nhỏ đi dạo trong vườn. Chú thấy một quả táo đỏ nằm dưới gốc cây. Chú dùng mũi nhọn lăn quả táo về phía mình. Quả táo hơi to nên chú phải nghỉ một lát rồi mới lăn được đến bụi cỏ. Chú nhím rửa sạch quả táo ở bờ suối rồi chậm rãi gặm từng miếng nhỏ. Ăn xong, chú cất phần còn lại vào hang để ăn dần. Chú nhím nghĩ: “Làm việc từ từ nhưng kiên trì thì sẽ làm được việc khó!”";
        yield return ReadingQuestion("seed-story-nhim-tao-q1", "Chú nhím và quả táo (1/5)", nhimTaoSpeech, "nhim-qua-tao", "q1", "Chú nhím đi dạo vào lúc nào?", ["Buổi sáng", "Buổi trưa", "Buổi chiều"], "Buổi chiều");
        yield return ReadingQuestion("seed-story-nhim-tao-q2", "Chú nhím và quả táo (2/5)", nhimTaoSpeech, "nhim-qua-tao", "q2", "Chú nhím thấy gì dưới gốc cây?", ["Quả táo", "Quả chuối", "Quả lê"], "Quả táo");
        yield return ReadingQuestion("seed-story-nhim-tao-q3", "Chú nhím và quả táo (3/5)", nhimTaoSpeech, "nhim-qua-tao", "q3", "Chú nhím đã làm gì với quả táo?", ["Lăn về phía mình", "Xách về nhà", "Nhảy qua quả táo"], "Lăn về phía mình");
        yield return ReadingQuestion("seed-story-nhim-tao-q4", "Chú nhím và quả táo (4/5)", nhimTaoSpeech, "nhim-qua-tao", "q4", "Vì sao chú nhím phải nghỉ một lát?", ["Vì chú mệt quá", "Vì quả táo hơi to", "Vì chú không biết lăn quả táo"], "Vì quả táo hơi to");
        yield return ReadingQuestion("seed-story-nhim-tao-q5", "Chú nhím và quả táo (5/5)", nhimTaoSpeech, "nhim-qua-tao", "q5", "Theo con, chú nhím là một bạn như thế nào?", ["Kiên trì và siêng năng", "Ích kỷ và lười biếng", "Vui tính và hay giúp đỡ"], "Kiên trì và siêng năng");

        // 9. Chú thỏ và củ cà rốt
        const string thoCuCarotSpeech = "Sáng nay, thỏ trắng ra vườn tìm cà rốt. Thỏ thấy một củ cà rốt lớn nhô lên khỏi mặt đất. Thỏ dùng hai chân sau đạp đất, dùng hai chân trước kéo mạnh. Cuối cùng, củ cà rốt cũng được nhổ lên. Thỏ cười tươi: “Củ cà rốt này thật ngọt và giòn!”";
        yield return ReadingQuestion("seed-story-tho-cu-ca-rot-q1", "Chú thỏ và củ cà rốt (1/5)", thoCuCarotSpeech, "tho-cu-ca-rot", "q1", "Thỏ ra vườn để làm gì?", ["Tìm cà rốt", "Hái hoa", "Tìm dưa hấu"], "Tìm cà rốt");
        yield return ReadingQuestion("seed-story-tho-cu-ca-rot-q2", "Chú thỏ và củ cà rốt (2/5)", thoCuCarotSpeech, "tho-cu-ca-rot", "q2", "Thỏ nhìn thấy củ cà rốt như thế nào?", ["Nhỏ", "Vừa", "Lớn"], "Lớn");
        yield return ReadingQuestion("seed-story-tho-cu-ca-rot-q3", "Chú thỏ và củ cà rốt (3/5)", thoCuCarotSpeech, "tho-cu-ca-rot", "q3", "Để nhổ được củ cà rốt, thỏ đã làm gì?", ["Chạy nhanh", "Nhảy lên", "Kéo mạnh"], "Kéo mạnh");
        yield return ReadingQuestion("seed-story-tho-cu-ca-rot-q4", "Chú thỏ và củ cà rốt (4/5)", thoCuCarotSpeech, "tho-cu-ca-rot", "q4", "Vì sao cuối cùng thỏ cũng nhổ được củ cà rốt?", ["Vì thỏ mệt quá nên nghỉ một lát", "Vì thỏ cố gắng kéo mạnh", "Vì có bạn đến giúp thỏ"], "Vì thỏ cố gắng kéo mạnh");
        yield return ReadingQuestion("seed-story-tho-cu-ca-rot-q5", "Chú thỏ và củ cà rốt (5/5)", thoCuCarotSpeech, "tho-cu-ca-rot", "q5", "Theo con, thỏ là một bạn như thế nào?", ["Chăm chỉ", "Lười biếng", "Ích kỷ"], "Chăm chỉ");

        // 10. Chiếc thuyền giấy
        const string thuyenGiaySpeech = "Buổi chiều, sau cơn mưa lớn, ở trước sân có rất nhiều vũng nước. An gấp một chiếc thuyền giấy thật xinh xắn. An thả thuyền xuống vũng nước và nhìn nó trôi theo dòng nước nhỏ. Chiếc thuyền đi qua lá cây, qua viên sỏi rồi dừng lại ở mép sân. An cười tít mắt vì chiếc thuyền đã có một chuyến đi thật thú vị!";
        yield return ReadingQuestion("seed-story-thuyen-giay-q1", "Chiếc thuyền giấy (1/5)", thuyenGiaySpeech, "chiec-thuyen-giay", "q1", "An gấp gì?", ["Máy bay giấy", "Thuyền giấy", "Hạc giấy"], "Thuyền giấy");
        yield return ReadingQuestion("seed-story-thuyen-giay-q2", "Chiếc thuyền giấy (2/5)", thuyenGiaySpeech, "chiec-thuyen-giay", "q2", "An thả thuyền ở đâu?", ["Vũng nước", "Bể cá", "Con suối"], "Vũng nước");
        yield return ReadingQuestion("seed-story-thuyen-giay-q3", "Chiếc thuyền giấy (3/5)", thuyenGiaySpeech, "chiec-thuyen-giay", "q3", "Chiếc thuyền đi qua những gì?", ["Lá cây và viên sỏi", "Khúc gỗ to", "Đám cỏ rậm"], "Lá cây và viên sỏi");
        yield return ReadingQuestion("seed-story-thuyen-giay-q4", "Chiếc thuyền giấy (4/5)", thuyenGiaySpeech, "chiec-thuyen-giay", "q4", "Vì sao An cười tít mắt?", ["Vì An được đi mua thuyền mới", "Vì chiếc thuyền có chuyến đi thật thú vị", "Vì thuyền giấy của An rất to và đẹp"], "Vì chiếc thuyền có chuyến đi thật thú vị");
        yield return ReadingQuestion("seed-story-thuyen-giay-q5", "Chiếc thuyền giấy (5/5)", thuyenGiaySpeech, "chiec-thuyen-giay", "q5", "Theo con, An là một bạn như thế nào?", ["Sáng tạo và thích khám phá", "Ích kỷ", "Nóng tính"], "Sáng tạo và thích khám phá");

        // 11. Thỏ con và cà rốt
        const string thoConCarotSpeech = "Thỏ con rất thích cà rốt. Mỗi ngày, thỏ con đều ra vườn nhổ cà rốt để ăn. Nhưng có hôm, thỏ con thấy những củ cà rốt nhỏ xíu nên buồn rầu. Thỏ con hỏi bác thỏ già: – Tại sao cà rốt của cháu lại nhỏ thế ạ? Bác thỏ cười hiền và nói: – Cháu phải nhổ cỏ, tưới nước và kiên nhẫn chờ đợi thì cà rốt mới lớn được. Thỏ con làm theo lời bác. Một thời gian sau, những củ cà rốt trong vườn to và ngọt lắm! Thỏ con vui lắm vì biết rằng kiên trì sẽ mang lại kết quả tốt đẹp.";
        yield return ReadingQuestion("seed-story-tho-con-carot-q1", "Thỏ con và cà rốt (1/5)", thoConCarotSpeech, "tho-con-ca-rot", "q1", "Thỏ con thích ăn gì?", ["Cà rốt", "Rau cải", "Táo"], "Cà rốt");
        yield return ReadingQuestion("seed-story-tho-con-carot-q2", "Thỏ con và cà rốt (2/5)", thoConCarotSpeech, "tho-con-ca-rot", "q2", "Vì sao cà rốt lúc đầu nhỏ xíu?", ["Có nhiều cỏ và chưa được chăm sóc", "Đất cứng", "Trời không mưa"], "Có nhiều cỏ và chưa được chăm sóc");
        yield return ReadingQuestion("seed-story-tho-con-carot-q3", "Thỏ con và cà rốt (3/5)", thoConCarotSpeech, "tho-con-ca-rot", "q3", "Bác thỏ già đã dặn thỏ con phải làm gì?", ["Nhổ cỏ, tưới nước và kiên nhẫn chờ đợi", "Đi chơi cùng bạn bè", "Đổi sang trồng củ khác"], "Nhổ cỏ, tưới nước và kiên nhẫn chờ đợi");
        yield return ReadingQuestion("seed-story-tho-con-carot-q4", "Thỏ con và cà rốt (4/5)", thoConCarotSpeech, "tho-con-ca-rot", "q4", "Thỏ con đã làm gì sau khi nghe lời bác thỏ?", ["Nhổ cỏ và tưới nước", "Đi chơi cùng bạn bè", "Nằm ngủ cả ngày"], "Nhổ cỏ và tưới nước");
        yield return ReadingQuestion("seed-story-tho-con-carot-q5", "Thỏ con và cà rốt (5/5)", thoConCarotSpeech, "tho-con-ca-rot", "q5", "Cuối cùng, thỏ con đã nhận được kết quả gì?", ["Những củ cà rốt to và ngọt", "Một chiếc huy chương", "Một chiếc bánh kem"], "Những củ cà rốt to và ngọt");
    }

    private static IEnumerable<SeedLesson> BuildCoverageLessons()
    {
        var basicStrokes = new[]
        {
            ("net-ngang", "Nét ngang", "─"), ("net-doc", "Nét dọc", "│"),
            ("net-xien-trai", "Nét xiên trái", "/"), ("net-xien-phai", "Nét xiên phải", "\\"),
            ("net-cong-trai", "Nét cong trái", "("), ("net-cong-phai", "Nét cong phải", ")"),
            ("net-moc-xuoi", "Nét móc xuôi", "J"), ("net-moc-nguoc", "Nét móc ngược", "L"),
            ("net-khuyet-tren", "Nét khuyết trên", "ℓ"), ("net-khuyet-duoi", "Nét khuyết dưới", "ɟ"),
            ("net-that", "Nét thắt", "∞"), ("net-vong", "Nét vòng", "○")
        };
        foreach (var (code, title, symbol) in basicStrokes)
        {
            yield return Tracing($"seed-basic-{code}", $"Tô {title.ToLowerInvariant()}", "net-co-ban", symbol, 1);
        }

        // Picture & Scene Tracing with rich object combinations (Thiên nhiên, Đồ dùng, Động vật, Hình học)
        var pictureTracingLessons = new[]
        {
            ("tranh-phong-canh", "Tô tranh phong cảnh: Mặt trời, ngôi nhà và cây thông", "phong-canh", 5),
            ("tranh-do-dung", "Tô tranh đồ dùng học tập: Balo, bút chì và số 5", "do-dung", 5),
            ("tranh-hinh-hoc", "Tô tranh ngôi sao, trái tim và hình học kỳ diệu", "hinh-hoc", 4),
            ("tranh-meo-con", "Tô tranh chú mèo con dễ thương", "meo-con", 4),
            ("tranh-ca-heo", "Tô tranh cá heo tung tăng bơi lội", "ca-heo", 4),
            ("tranh-o-che-mua", "Tô tranh chiếc ô che mưa và chú ếch con", "o-che-mua", 4),
            ("tranh-trai-tao", "Tô tranh quả táo ngọt và chú sâu nhỏ", "trai-tao", 4),
            ("tranh-tau-hoa", "Tô tranh đoàn tàu hỏa tí hon", "tau-hoa", 4),
            ("tranh-chu-tho", "Tô tranh chú thỏ trắng và củ cà rốt", "chu-tho", 4),
            ("tranh-ong-vang", "Tô tranh chú ong chăm chỉ và hoa hướng dương", "ong-vang", 4),
            ("tranh-may-bay", "Tô tranh chiếc máy bay trên bầu trời", "may-bay", 4),
            ("tranh-thuyen", "Tô tranh thuyền buồm lướt sóng biển", "thuyen", 4),
            ("tranh-cau-vong", "Tô tranh cầu vồng rực rỡ và lâu đài cổ tích", "cau-vong", 4),
            ("tranh-chu-buom", "Tô tranh cánh bướm xinh bên vườn hoa", "chu-buom", 4),
            ("tranh-khung-long", "Tô tranh chú khủng long tí hon", "khung-long", 4),
            ("tranh-ten-lua", "Tô tranh tàu vũ trụ và các vì sao", "ten-lua", 4)
        };
        foreach (var (code, title, symbol, strokes) in pictureTracingLessons)
        {
            yield return Tracing($"seed-picture-{code}", title, "tao-hinh", symbol, strokes);
        }

        // Letter Recognition for all 29 Vietnamese letters
        for (var index = 0; index < VietnameseAlphabet.Length; index++)
        {
            var letter = VietnameseAlphabet[index];
            var previous = VietnameseAlphabet[(index + VietnameseAlphabet.Length - 1) % VietnameseAlphabet.Length];
            var next = VietnameseAlphabet[(index + 1) % VietnameseAlphabet.Length];
            yield return Choice($"seed-letter-recognition-{index + 1:00}", $"Nhận biết chữ {letter}", "kham-pha-chu",
                InteractionTypes.SingleChoice, "Con nhìn mẫu rồi chọn đúng chữ cái.", $"Đâu là chữ {letter}?", [previous, letter, next], letter,
                imageUrl: ResolveLetterFlashcardUrl(letter));
        }

        // Numbers 10-20 Recognition
        for (var number = 10; number <= 20; number++)
        {
            yield return Choice($"seed-recognize-number-{number}", $"Nhận biết số {number}", "so-10-20",
                InteractionTypes.SingleChoice, "Con quan sát rồi chọn đúng số.", $"Đâu là số {number}?",
                [(number - 1).ToString(), number.ToString(), (number + 1 > 20 ? 10 : number + 1).ToString()], number.ToString(),
                imageUrl: ResolveNumberFlashcardUrl(number.ToString()));
        }

        // Quantity and Counting seeds
        foreach (var number in new[] { 0, 1, 3, 5, 7, 8, 9 })
        {
            yield return Quantity($"seed-quantity-dot-{number}", $"Tạo {number} chấm tròn", "tao-so-luong", "●", number);
        }
        foreach (var number in new[] { 0, 1, 2, 4, 6, 8, 9 })
        {
            yield return Counting($"seed-count-dot-{number}", $"Đếm {number} chấm tròn", "dem-so-luong", "●", number);
        }

        // So sánh
        yield return Comparison("seed-compare-more-2", "Chọn nhóm nhiều hơn 2", "so-sanh", "●", "Nhóm A", 3, "Nhóm B", 6, "more");
        yield return Comparison("seed-compare-more-3", "Chọn nhóm nhiều hơn 3", "so-sanh", "■", "Nhóm A", 8, "Nhóm B", 5, "more");
        yield return Comparison("seed-compare-less-2", "Chọn nhóm ít hơn 2", "so-sanh", "▲", "Nhóm A", 7, "Nhóm B", 4, "less");
        yield return Comparison("seed-compare-less-3", "Chọn nhóm ít hơn 3", "so-sanh", "★", "Nhóm A", 2, "Nhóm B", 5, "less");
        yield return Comparison("seed-compare-equal-2", "Hai nhóm bằng nhau 2", "so-sanh", "●", "Nhóm A", 3, "Nhóm B", 3, "equal");
        yield return Comparison("seed-compare-equal-3", "Hai nhóm bằng nhau 3", "so-sanh", "■", "Nhóm A", 6, "Nhóm B", 6, "equal");
        yield return Comparison("seed-compare-more-4", "Chọn nhóm nhiều hơn 4", "so-sanh", "◆", "Nhóm A", 9, "Nhóm B", 7, "more");

        // Vận động tinh, Mê cung & Khéo tay
        yield return Ordering("seed-fine-fold", "Gấp giấy theo thứ tự", "kheo-tay", ["Đặt giấy ngay ngắn", "Gấp hai mép", "Miết nếp gấp"]);
        yield return Ordering("seed-fine-pencil", "Chuẩn bị bút chì", "kheo-tay", ["Chọn bút", "Cầm bằng ba ngón", "Đặt tay lên giấy"]);
        yield return Ordering("seed-fine-beads", "Xâu hạt theo bước", "kheo-tay", ["Chọn sợi dây", "Chọn hạt", "Luồn từng hạt", "Buộc hai đầu"]);
        yield return Ordering("seed-fine-cut-paper", "Cắt giấy an toàn", "kheo-tay", ["Ngồi ngay ngắn", "Cầm kéo đúng tay", "Cắt theo đường", "Cất kéo"]);
        yield return Ordering("seed-fine-coloring", "Tô màu gọn gàng", "kheo-tay", ["Chọn màu", "Tô từ trong ra ngoài", "Tô kín hình", "Cất bút"]);
        yield return Drag("seed-fine-maze-1", "Đưa ong về tổ", "me-cung", "Tổ ong", ["Ong", "Cá", "Ô tô"], "Ong");
        yield return Drag("seed-fine-maze-2", "Đưa cá về hồ", "me-cung", "Hồ nước", ["Cá", "Chim", "Xe đạp"], "Cá");
        yield return Drag("seed-fine-maze-3", "Đưa xe về gara", "me-cung", "Gara", ["Ô tô", "Táo", "Bút"], "Ô tô");
        yield return Drag("seed-fine-maze-4", "Đưa thỏ về vườn cà rốt", "me-cung", "Vườn cà rốt", ["Thỏ", "Cá", "Máy bay"], "Thỏ");
        yield return Drag("seed-fine-maze-5", "Đưa chim về tổ", "me-cung", "Tổ chim", ["Chim", "Xe buýt", "Quả bóng"], "Chim");

        // Hình dạng cơ bản
        var shapes = new[] { "Hình tròn", "Hình vuông", "Hình tam giác", "Hình ngôi sao", "Hình trái tim", "Hình chữ nhật" };
        foreach (var shape in shapes)
        {
            var shapeImg = shape switch
            {
                "Hình tròn" => "/images/photos/flashcard-shape-circle.svg",
                "Hình vuông" => "/images/photos/flashcard-shape-square.svg",
                "Hình tam giác" => "/images/photos/flashcard-shape-triangle.svg",
                "Hình ngôi sao" => "/images/photos/flashcard-shape-star.svg",
                "Hình trái tim" => "/images/photos/flashcard-shape-heart.svg",
                _ => "/images/photos/flashcard-shape-square.svg"
            };
            yield return Choice($"seed-shape-{NormalizeSeedCode(shape)}", $"Nhận biết {shape.ToLowerInvariant()}", "hinh-dang",
                InteractionTypes.SingleChoice, "Con quan sát đặc điểm rồi chọn đúng tên hình.", "Đây là hình gì?",
                new[] { shape, "Hình tròn", "Hình vuông" }.Distinct().Append("Hình tam giác").Take(3).ToArray(), shape,
                imageUrl: shapeImg);
        }

        // Tư duy logic & Quy luật
        yield return Ordering("seed-logic-pattern-1", "Quy luật đỏ xanh", "quy-luat", ["Đỏ", "Xanh", "Đỏ", "Xanh"]);
        yield return Ordering("seed-logic-pattern-2", "Quy luật nhỏ lớn", "quy-luat", ["Nhỏ", "Lớn", "Nhỏ", "Lớn"]);
        yield return Ordering("seed-logic-pattern-3", "Quy luật một hai", "quy-luat", ["1", "2", "1", "2"]);
        yield return Ordering("seed-logic-pattern-4", "Quy luật tròn vuông", "quy-luat", ["Tròn", "Vuông", "Tròn", "Vuông"]);
        yield return Ordering("seed-logic-pattern-5", "Quy luật cao thấp", "quy-luat", ["Cao", "Thấp", "Cao", "Thấp"]);
        yield return Ordering("seed-logic-pattern-6", "Quy luật một một hai", "quy-luat", ["1", "1", "2", "1", "1", "2"]);

        // Phân loại & Tìm khác biệt
        yield return Choice("seed-logic-different-1", "Tìm vật khác nhóm (Trái cây)", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không phải trái cây?", ["Táo", "Cam", "Bút chì"], "Bút chì",
            imageUrl: "/images/photos/flashcard-apple.jpg");
        yield return Choice("seed-logic-different-2", "Tìm vật khác nhóm (Con vật)", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không phải con vật?", ["Mèo", "Gà", "Cái bàn"], "Cái bàn",
            imageUrl: "/images/photos/cat.jpg");
        yield return Choice("seed-logic-different-3", "Tìm vật khác nhóm (Phương tiện)", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không dùng để đi lại?", ["Xe đạp", "Ô tô", "Cái bát"], "Cái bát",
            imageUrl: "/images/pictograms/bicycle.svg");
        yield return Choice("seed-logic-different-water", "Tìm con vật sống dưới nước", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm con vật khác biệt với các con còn lại.", "Con vật nào sống dưới nước?", ["Con cá", "Con mèo", "Con chó"], "Con cá",
            imageUrl: "/images/photos/fish.jpg");
        yield return Choice("seed-logic-different-fly", "Tìm loài vật biết bay", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm loài vật có cánh biết bay lượn.", "Loài vật nào biết bay trên trời?", ["Con chim", "Con tôm", "Con thỏ"], "Con chim",
            imageUrl: "/images/pictograms/bird.svg");

        // Ghi nhớ & Tập trung
        yield return Ordering("seed-memory-morning", "Nhớ việc buổi sáng", "ghi-nho", ["Thức dậy", "Đánh răng", "Ăn sáng", "Đi học"]);
        yield return Ordering("seed-memory-school", "Nhớ thứ tự đến lớp", "ghi-nho", ["Chào cô", "Cất ba lô", "Ngồi vào chỗ", "Mở sách"]);
        yield return Ordering("seed-memory-lunch", "Nhớ thứ tự bữa ăn", "ghi-nho", ["Rửa tay", "Ngồi ngay ngắn", "Ăn cơm", "Dọn bát"]);
        yield return Multi("seed-memory-colors", "Nhớ hai màu đã thấy", "ghi-nho", ["Đỏ", "Xanh", "Vàng", "Tím"], ["Đỏ", "Vàng"]);
        yield return Multi("seed-memory-shapes", "Nhớ hai hình đã thấy", "ghi-nho", ["Hình tròn", "Hình vuông", "Hình tam giác", "Hình thoi"], ["Hình vuông", "Hình thoi"]);
        yield return Multi("seed-memory-objects", "Nhớ đồ dùng học tập", "ghi-nho", ["Bút", "Vở", "Nồi", "Chảo"], ["Bút", "Vở"]);
        yield return Mapping("seed-memory-pairs-1", "Nhớ cặp đồ vật", "ghi-nho", InteractionTypes.Matching, [("Bàn chải", "Kem đánh răng"), ("Bát", "Thìa"), ("Bút", "Vở")]);
        yield return Mapping("seed-memory-pairs-2", "Nhớ cặp trang phục", "ghi-nho", InteractionTypes.Matching, [("Áo", "Quần"), ("Giày", "Tất"), ("Mũ", "Khăn")]);
        yield return Mapping("seed-memory-pairs-3", "Nhớ cặp nơi chốn", "ghi-nho", InteractionTypes.Matching, [("Cá", "Hồ nước"), ("Chim", "Tổ"), ("Xe", "Gara")]);

        // Kỹ năng sống & An toàn & Cảm xúc
        yield return Choice("seed-life-helmet-safety", "Đội mũ bảo hiểm khi đi xe máy", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động an toàn.", "Khi ngồi trên xe máy, con cần làm gì?", ["Đội mũ bảo hiểm", "Đứng lên", "Đùa nghịch"], "Đội mũ bảo hiểm",
            imageUrl: "/images/pictograms/helmet.svg");
        yield return Choice("seed-life-stranger-safety", "Không đi theo người lạ", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn cách xử lý an toàn.", "Người lạ rủ con đi theo, con làm gì?", ["Từ chối và gọi người thân", "Đi theo ngay", "Không nói với ai"], "Từ chối và gọi người thân",
            imageUrl: "/images/pictograms/telephone.svg");
        yield return Choice("seed-life-electric-safety", "Tránh xa ổ cắm điện", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động an toàn.", "Khi thấy ổ điện, con cần làm gì?", ["Không chạm vào", "Cho tay vào", "Đổ nước lên"], "Không chạm vào",
            imageUrl: "/images/pictograms/electric-plug.svg");
        yield return Choice("seed-life-traffic-light-action", "Tuân thủ tín hiệu đèn giao thông", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động đúng theo tín hiệu đèn.", "Khi đèn giao thông màu đỏ sáng lên, người đi đường phải làm gì?", ["Dừng lại", "Chạy nhanh qua", "Bấm còi"], "Dừng lại",
            imageUrl: "/images/pictograms/car.svg");
        yield return Choice("seed-life-sharp-safety", "An toàn với vật sắc nhọn", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn cách xử lý an toàn.", "Khi thấy dao kéo hoặc vật sắc nhọn, con nên làm gì?", ["Không tự ý nghịch", "Lấy ra chơi đồ hàng", "Cầm chạy nhảy"], "Không tự ý nghịch",
            imageUrl: "/images/pictograms/cooking-pot.svg");
        yield return Choice("seed-life-wash-hands-routine", "Thời điểm bé cần rửa tay", "tu-phuc-vu", InteractionTypes.SingleChoice,
            "Con chọn thời điểm cần rửa tay.", "Bé nên rửa tay sạch bằng xà phòng khi nào?", ["Trước khi ăn và sau khi đi vệ sinh", "Chỉ khi bị mẹ nhắc", "Không cần rửa"], "Trước khi ăn và sau khi đi vệ sinh",
            imageUrl: "/images/pictograms/soap.svg");
        yield return Choice("seed-life-feeling-share", "Chia sẻ cảm xúc khi buồn", "cam-xuc", InteractionTypes.SingleChoice,
            "Con chọn cách chia sẻ phù hợp.", "Khi buồn, con nên làm gì?", ["Nói với người con tin tưởng", "Đập đồ", "La hét vào bạn"], "Nói với người con tin tưởng",
            imageUrl: "/images/pictograms/speaking.svg");
        yield return Choice("seed-life-happy-action", "Cảm xúc khi làm được việc tốt", "cam-xuc", InteractionTypes.SingleChoice,
            "Con chọn cảm xúc phù hợp.", "Khi giúp đỡ bạn, con cảm thấy thế nào?", ["Vui vẻ", "Tức giận", "Buồn bã"], "Vui vẻ",
            imageUrl: "/images/pictograms/speaking.svg");
        yield return Choice("seed-life-sharing-toys", "Biết chia sẻ đồ chơi cùng bạn", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn cách cư xử thân thiện.", "Bạn muốn chơi cùng, con nên làm gì?", ["Chia sẻ và chơi cùng", "Giấu đồ chơi", "Đẩy bạn ra"], "Chia sẻ và chơi cùng",
            imageUrl: "/images/pictograms/handshake.svg");
        yield return Choice("seed-life-apology-manner", "Biết nói lời xin lỗi lễ phép", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời nói phù hợp.", "Khi vô ý làm bạn đau, con nên nói gì?", ["Mình xin lỗi bạn", "Không phải mình", "Bạn tự chịu"], "Mình xin lỗi bạn",
            imageUrl: "/images/pictograms/folded-hands.svg");
        yield return Choice("seed-life-greeting-class", "Lời chào khi đến lớp học", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời chào lễ phép khi đến lớp.", "Khi đến trường gặp cô giáo, bé nói gì?", ["Con chào cô ạ!", "Tớ đến rồi", "Không nói gì"], "Con chào cô ạ!",
            imageUrl: "/images/pictograms/speaking.svg");
        yield return Choice("seed-life-thank-gift", "Lời cảm ơn khi được tặng quà", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời nói lễ phép khi nhận quà.", "Khi được ông bà tặng quà, bé nói gì?", ["Con cảm ơn ông bà ạ!", "Cho con thêm cái nữa", "Cầm lấy rồi đi ngay"], "Con cảm ơn ông bà ạ!",
            imageUrl: "/images/pictograms/folded-hands.svg");
        yield return Choice("seed-life-clean-desk", "Bé ngoan giữ sạch bàn học", "kheo-tay", InteractionTypes.SingleChoice,
            "Con chọn cách sắp xếp góc học tập.", "Sau khi học xong, bé nên làm gì?", ["Cất bút và xếp sách vở gọn gàng", "Vứt sách xuống đất", "Để nguyên rồi chạy đi chơi"], "Cất bút và xếp sách vở gọn gàng",
            imageUrl: "/images/pictograms/notebook.svg");

        // Toán học & Số lượng nâng cao
        yield return Choice("seed-math-order-after-3", "Số liền sau số 3", "thu-tu-so", InteractionTypes.SingleChoice,
            "Con tìm số đứng ngay sau số ba.", "Số nào đứng liền sau số 3?", ["4", "2", "5"], "4");
        yield return Choice("seed-math-order-after-5", "Số liền sau số 5", "thu-tu-so", InteractionTypes.SingleChoice,
            "Con tìm số đứng ngay sau số năm.", "Số nào đứng liền sau số 5?", ["6", "4", "3"], "6");
        yield return Choice("seed-math-order-before-8", "Số liền trước số 8", "thu-tu-so", InteractionTypes.SingleChoice,
            "Con tìm số đứng ngay trước số tám.", "Số nào đứng liền trước số 8?", ["7", "9", "6"], "7");
        yield return Choice("seed-math-order-between-4-6", "Số ở giữa số 4 và số 6", "thu-tu-so", InteractionTypes.SingleChoice,
            "Con tìm số đứng giữa số bốn và số sáu.", "Số nào nằm giữa 4 và 6?", ["5", "3", "7"], "5");
        yield return Choice("seed-math-diff-6-9", "Phân biệt số 6 và số 9", "phan-biet-so", InteractionTypes.SingleChoice,
            "Con quan sát số có vòng tròn ở dưới.", "Đâu là số sáu (6)?", ["6", "9", "0"], "6",
            imageUrl: "/images/photos/flashcard-number-6.jpg");
        yield return Choice("seed-math-diff-1-7", "Phân biệt số 1 và số 7", "phan-biet-so", InteractionTypes.SingleChoice,
            "Con quan sát số có nét gạch ngang trên đầu.", "Đâu là số bảy (7)?", ["7", "1", "4"], "7",
            imageUrl: "/images/photos/flashcard-number-7.jpg");
        yield return Choice("seed-math-split-combine-4", "Tách gộp 4 quả táo", "tach-gop", InteractionTypes.SingleChoice,
            "Con quan sát cách tách bốn quả táo.", "Bốn quả táo có thể tách thành hai nhóm nào?", ["2 quả và 2 quả", "1 quả và 5 quả", "3 quả và 3 quả"], "2 quả và 2 quả",
            imageUrl: "/images/photos/flashcard-apple.jpg");
        yield return Choice("seed-math-split-combine-5", "Tách gộp 5 ngôi sao", "tach-gop", InteractionTypes.SingleChoice,
            "Con quan sát cách tách năm ngôi sao.", "Năm ngôi sao gồm những nhóm nào?", ["2 ngôi sao và 3 ngôi sao", "1 ngôi sao và 6 ngôi sao", "4 ngôi sao và 4 ngôi sao"], "2 ngôi sao và 3 ngôi sao",
            imageUrl: "/images/photos/flashcard-shape-star.svg");
        yield return Choice("seed-math-add-1plus1", "Phép tính thêm: 1 + 1", "cong-bot", InteractionTypes.SingleChoice,
            "Có một chú cá, bơi thêm một chú cá nữa là mấy chú cá?", "1 chú cá thêm 1 chú cá là mấy chú cá?", ["2 chú cá", "1 chú cá", "3 chú cá"], "2 chú cá",
            imageUrl: "/images/photos/fish.jpg");
        yield return Choice("seed-math-add-simple", "Bé làm phép tính thêm: 2 + 1", "cong-bot", InteractionTypes.SingleChoice,
            "Có hai quả bóng, thêm một quả bóng nữa là mấy quả bóng?", "Có 2 quả bóng, thêm 1 quả bóng là mấy quả bóng?", ["3 quả bóng", "1 quả bóng", "4 quả bóng"], "3 quả bóng",
            imageUrl: "/images/pictograms/ball.svg");
        yield return Choice("seed-math-subtract-simple", "Bé làm phép tính bớt: 3 - 1", "cong-bot", InteractionTypes.SingleChoice,
            "Có ba quả cam, ăn mất một quả thì còn mấy quả?", "Có 3 quả cam, bớt 1 quả thì còn lại mấy quả?", ["2 quả cam", "4 quả cam", "1 quả cam"], "2 quả cam",
            imageUrl: "/images/photos/flashcard-orange.jpg");
        yield return Choice("seed-math-subtract-4minus2", "Bé làm phép tính bớt: 4 - 2", "cong-bot", InteractionTypes.SingleChoice,
            "Có bốn chiếc kẹo, cho bạn hai chiếc kẹo thì còn lại mấy chiếc?", "Có 4 chiếc kẹo, bớt 2 chiếc kẹo thì còn mấy chiếc?", ["2 chiếc kẹo", "3 chiếc kẹo", "1 chiếc kẹo"], "2 chiếc kẹo");

        // Ngôn ngữ & Âm vần & Vốn từ
        yield return Choice("seed-lang-diff-b-d", "Phân biệt chữ b và chữ d", "phan-biet-chu", InteractionTypes.SingleChoice,
            "Con quan sát chữ có nét cong bên phải.", "Đâu là chữ b trong quả bóng?", ["b", "d", "p"], "b",
            imageUrl: "/images/photos/flashcard-letter-b.jpg");
        yield return Choice("seed-lang-diff-n-m", "Phân biệt chữ n và chữ m", "phan-biet-chu", InteractionTypes.SingleChoice,
            "Con quan sát chữ có hai nét móc.", "Đâu là chữ m trong con mèo?", ["m", "n", "u"], "m",
            imageUrl: "/images/photos/flashcard-letter-m.jpg");
        yield return Choice("seed-lang-body-parts", "Nhận biết các giác quan", "von-tu", InteractionTypes.SingleChoice,
            "Con chọn bộ phận dùng để nhìn ngắm.", "Bộ phận nào trên khuôn mặt giúp bé nhìn thấy vạn vật?", ["Đôi mắt", "Đôi tai", "Cái mũi"], "Đôi mắt",
            imageUrl: "/images/pictograms/flower.svg");
        yield return Choice("seed-lang-sound-b", "Tiếng có âm B", "am-van", InteractionTypes.SingleChoice,
            "Con lắng nghe và tìm từ bắt đầu bằng âm B.", "Từ nào dưới đây bắt đầu bằng âm B?", ["Bút chì", "Cái kéo", "Quyển vở"], "Bút chì",
            imageUrl: "/images/pictograms/pencil.svg");
        yield return Choice("seed-lang-sound-c", "Tiếng có âm C", "am-van", InteractionTypes.SingleChoice,
            "Con tìm từ bắt đầu bằng âm C.", "Từ nào dưới đây bắt đầu bằng âm C?", ["Con cá", "Quả táo", "Bông hoa"], "Con cá",
            imageUrl: "/images/photos/fish.jpg");
        yield return Choice("seed-lang-sound-d", "Tiếng có âm D", "am-van", InteractionTypes.SingleChoice,
            "Con tìm từ bắt đầu bằng âm D.", "Từ nào dưới đây bắt đầu bằng âm D?", ["Quả dưa", "Con gà", "Cái bàn"], "Quả dưa");

        // Không gian & Kích thước
        yield return Choice("seed-space-up-down", "Vị trí trên và dưới", "vi-tri", InteractionTypes.SingleChoice,
            "Con quan sát vị trí của chú chim và cây xanh.", "Chú chim đang đậu ở đâu?", ["Trên cành cây", "Dưới mặt đất", "Trong hồ nước"], "Trên cành cây",
            imageUrl: "/images/pictograms/bird.svg");
        yield return Choice("seed-space-in-out", "Vị trí trong và ngoài", "vi-tri", InteractionTypes.SingleChoice,
            "Con quan sát vị trí đồ vật.", "Quả táo đang nằm ở đâu so với chiếc giỏ?", ["Trong giỏ", "Ngoài giỏ", "Dưới giỏ"], "Trong giỏ",
            imageUrl: "/images/photos/flashcard-apple.jpg");
        yield return Choice("seed-space-size-compare", "So sánh lớn hơn và nhỏ hơn", "kich-thuoc", InteractionTypes.SingleChoice,
            "Con so sánh kích thước của hai con vật.", "Con voi như thế nào so với con chuột?", ["To lớn hơn", "Nhỏ bé hơn", "Bằng nhau"], "To lớn hơn",
            imageUrl: "/images/photos/cat.jpg");
        yield return Choice("seed-space-height-compare", "So sánh cao hơn và thấp hơn", "kich-thuoc", InteractionTypes.SingleChoice,
            "Con so sánh chiều cao của hai cây.", "Cây dừa như thế nào so với bụi cỏ?", ["Cao hơn", "Thấp hơn", "Bằng nhau"], "Cao hơn",
            imageUrl: "/images/pictograms/seedling.svg");

        // Khám phá tự nhiên & Thế giới
        yield return Choice("seed-world-plant-roots", "Bộ phận hút nước của cây", "cay-co", InteractionTypes.SingleChoice,
            "Con chọn bộ phận dưới lòng đất của cây.", "Bộ phận nào nằm dưới đất hút chất dinh dưỡng cho cây?", ["Rễ cây", "Lá cây", "Bông hoa"], "Rễ cây",
            imageUrl: "/images/pictograms/seedling.svg");
        yield return Choice("seed-world-day-night", "Bầu trời ban ngày và ban đêm", "thoi-tiet", InteractionTypes.SingleChoice,
            "Con quan sát hiện tượng tự nhiên.", "Khi ban đêm đến, bé nhìn thấy gì trên bầu trời?", ["Mặt trăng và các vì sao", "Mặt trời rực rỡ", "Cầu vồng bảy sắc"], "Mặt trăng và các vì sao",
            imageUrl: "/images/pictograms/moon.svg");
        yield return Choice("seed-world-rain-umbrella", "Đồ dùng khi trời mưa", "thoi-tiet", InteractionTypes.SingleChoice,
            "Con chọn đồ dùng phù hợp khi trời mưa.", "Khi đi dưới trời mưa, bé cần mang theo vật gì?", ["Chiếc ô che mưa", "Kính râm", "Quạt tay"], "Chiếc ô che mưa",
            imageUrl: "/images/pictograms/umbrella.svg");
        yield return Choice("seed-world-animals-egg", "Loài vật đẻ trứng", "con-vat", InteractionTypes.SingleChoice,
            "Con chọn con vật sinh sản bằng cách đẻ trứng.", "Con vật nào dưới đây đẻ ra những quả trứng tròn?", ["Con gà mái", "Con chó con", "Con mèo"], "Con gà mái",
            imageUrl: "/images/photos/chicken.jpg");
        yield return Choice("seed-world-animals-milk", "Loài vật cho sữa thơm ngon", "con-vat", InteractionTypes.SingleChoice,
            "Con chọn con vật mang lại nguồn sữa cho bé.", "Con vật nào cho chúng ta nguồn sữa tươi thơm ngon?", ["Bò sữa", "Con thỏ", "Con vịt"], "Bò sữa");

        // Bổ sung các chủ đề còn trống để độ phủ chương trình luôn đạt 100%.
        yield return Mapping("seed-coverage-shadow-pairs", "Ghép đồ vật với bóng tương ứng", "ghep-bong", InteractionTypes.Matching,
            [("Quả táo", "Bóng quả táo"), ("Con cá", "Bóng con cá"), ("Chiếc ô", "Bóng chiếc ô")]);
        yield return Listen("seed-coverage-listen-request", "Nghe và chọn đồ vật được nhắc đến", "nghe-hieu",
            "Con hãy chọn chiếc bút chì để chuẩn bị học bài.", ["Bút chì", "Quả táo", "Con cá"], "Bút chì");
        yield return Drag("seed-coverage-build-house", "Chọn mảnh ghép làm mái nhà", "ghep-hinh", "Mái nhà",
            ["Hình tam giác", "Hình tròn", "Hình vuông"], "Hình tam giác");
        yield return Listen("seed-coverage-follow-request", "Nghe và làm theo yêu cầu", "lam-theo-yeu-cau",
            "Con hãy chọn quả bóng rồi chọn nút hoàn thành.", ["Quả bóng", "Quyển sách", "Chiếc ô"], "Quả bóng");
        yield return Mapping("seed-coverage-connect-dots", "Nối các điểm theo thứ tự", "noi-diem", InteractionTypes.Matching,
            [("Điểm 1", "Điểm 2"), ("Điểm 3", "Điểm 4"), ("Điểm 5", "Điểm 6")]);

        // Phủ đủ mọi dạng bài được phép trong từng nhóm kỹ năng.
        yield return Story("seed-coverage-letter-story", "Câu chuyện chữ A và quả táo", "kham-pha-chu",
            "Bạn An tìm thấy một quả táo đỏ. Chữ A là chữ đầu trong tên bạn An.", "/images/photos/flashcard-letter-a.jpg",
            "Chữ nào đứng đầu tên bạn An?", ["A", "B", "C"], "A");

        yield return Listen("seed-coverage-number-listen", "Nghe và chọn số mười hai", "so-10-20",
            "Đây là số mười hai.", ["11", "12", "13"], "12");
        yield return Counting("seed-coverage-number-count", "Đếm năm chấm và chọn chữ số", "so-0-9", "●", 5);
        yield return Drag("seed-coverage-number-order", "Đưa số 8 vào sau số 7", "thu-tu-so",
            "Vị trí sau số 7", ["6", "8", "9"], "8");
        yield return Mapping("seed-coverage-number-match", "Nối các số dễ nhầm với tên gọi", "phan-biet-so", InteractionTypes.Matching,
            [("6", "Số sáu"), ("9", "Số chín"), ("2", "Số hai")]);

        yield return Listen("seed-coverage-math-listen", "Nghe và chọn nhóm có ba đồ vật", "dem-so-luong",
            "Con hãy chọn số ba.", ["2", "3", "4"], "3");

        yield return Mapping("seed-coverage-life-classify", "Phân loại hành động an toàn", "an-toan", InteractionTypes.Classification,
            [("Đội mũ bảo hiểm", "An toàn"), ("Đi cùng người lớn", "An toàn"), ("Nghịch ổ điện", "Nguy hiểm")]);
        yield return Listen("seed-coverage-life-listen", "Nghe lời chào lịch sự", "giao-tiep",
            "Con chào cô ạ.", ["Lời chào lễ phép", "Lời từ chối", "Lời xin lỗi"], "Lời chào lễ phép");

        yield return Mapping("seed-coverage-language-classify", "Phân loại từ chỉ con vật và đồ vật", "von-tu", InteractionTypes.Classification,
            [("Con mèo", "Con vật"), ("Con cá", "Con vật"), ("Bút chì", "Đồ vật"), ("Quyển vở", "Đồ vật")]);

        yield return Mapping("seed-coverage-shape-classify", "Phân loại hình có góc và không có góc", "hinh-dang", InteractionTypes.Classification,
            [("Hình tròn", "Không có góc"), ("Hình vuông", "Có góc"), ("Hình tam giác", "Có góc")]);
        yield return Story("seed-coverage-position-story", "Chú chim ở trên cành cây", "vi-tri",
            "Chú chim nhỏ bay lên và đậu trên cành cây xanh.", "/images/pictograms/bird.svg",
            "Chú chim đang ở đâu?", ["Trên cành cây", "Dưới hồ nước", "Trong ngôi nhà"], "Trên cành cây");
        yield return Comparison("seed-coverage-size-compare", "So sánh hai nhóm hình lớn và nhỏ", "kich-thuoc", "●",
            "Nhóm hình lớn", 6, "Nhóm hình nhỏ", 3, "more");

        yield return Choice("seed-coverage-memory-choice", "Nhớ vị trí quả táo", "ghi-nho", InteractionTypes.SingleChoice,
            "Con nhớ vị trí vừa quan sát rồi chọn đáp án.", "Quả táo vừa nằm ở đâu?", ["Bên trái", "Ở giữa", "Bên phải"], "Ở giữa",
            imageUrl: "/images/photos/flashcard-apple.jpg");
        yield return Counting("seed-coverage-focus-count", "Tập trung đếm bốn ngôi sao", "tap-trung", "★", 4);
        yield return Story("seed-coverage-follow-story", "Bé làm theo hai yêu cầu", "lam-theo-yeu-cau",
            "Mẹ nhờ bé lấy quyển vở rồi đặt bút chì lên bàn. Bé lắng nghe và làm đúng cả hai việc.", "/images/pictograms/notebook.svg",
            "Bé lấy đồ vật nào trước?", ["Quyển vở", "Bút chì", "Quả bóng"], "Quyển vở");
        yield return Drag("seed-coverage-follow-drag", "Làm theo yêu cầu đưa bút vào cặp", "lam-theo-yeu-cau",
            "Trong cặp sách", ["Bút chì", "Cái bát", "Quả bóng"], "Bút chì");

        yield return Story("seed-coverage-animal-story", "Chú cá tìm đường về hồ", "con-vat",
            "Chú cá nhỏ bơi theo dòng nước trong và tìm được đường về hồ cùng các bạn.", "/images/photos/fish.jpg",
            "Chú cá sống ở đâu?", ["Trong hồ nước", "Trên cành cây", "Trong tổ ong"], "Trong hồ nước");
        yield return Ordering("seed-coverage-plant-order", "Sắp xếp quá trình cây lớn lên", "cay-co",
            ["Gieo hạt", "Tưới nước", "Hạt nảy mầm", "Cây lớn lên"]);
    }

    private static string NormalizeSeedCode(string value) => value
        .ToLowerInvariant()
        .Replace(' ', '-');

    private static SeedLesson Tracing(string code, string title, string topicCode, string symbol, int strokes)
    {
        var tracingImageUrl = ResolveTracingFlashcardUrl(symbol);
        var payload = JsonSerializer.Serialize(new
        {
            symbol,
            expectedStrokeCount = strokes,
            guideMode = "outline",
            imageUrl = tracingImageUrl,
            imageAltText = string.IsNullOrWhiteSpace(tracingImageUrl) ? "Hình minh họa" : $"Thẻ học {symbol}"
        });
        var isPicture = symbol.Contains("phong-canh") || symbol.Contains("do-dung") || symbol.Contains("meo-con") ||
                        symbol.Contains("ca-heo") || symbol.Contains("o-che-mua") || symbol.Contains("hinh-hoc") ||
                        symbol.Contains("trai-tao") || symbol.Contains("tau-hoa") || code.Contains("picture") || title.Contains("tranh", StringComparison.OrdinalIgnoreCase);
        var prompt = isPicture
            ? "Bé hãy quan sát bức tranh và tô theo các nét đứt nhé! ✨"
            : $"Bé hãy quan sát cách viết {symbol} nhé!";
        var instruction = isPicture
            ? "Bé hãy tô theo các nét đứt để hoàn thành bức tranh nhé."
            : "Bé vẽ theo đường nét đứt nhé.";
        return new(code, title, topicCode, InteractionTypes.Tracing, instruction, prompt, payload, string.Empty, "Bắt đầu ở chấm màu cam, đi theo mũi tên và tô chậm trên nét đứt.", symbol, strokes);
    }

    private static string ExtractSymbolFromTitle(string? title, string? prompt = null)
    {
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            var match = System.Text.RegularExpressions.Regex.Match(prompt, @"cách viết\s+([^\s!.,?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups[1].Value.Trim();
        }
        if (!string.IsNullOrWhiteSpace(title))
        {
            var match = System.Text.RegularExpressions.Regex.Match(title, @"(chữ số|chữ|số|nét)\s+([^\s!.,?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups[2].Value.Trim();
        }
        return "A";
    }

    private static SeedLesson Choice(string code, string title, string topicCode, string type, string instruction, string prompt, string[] choices, string answer, string speechText = "", string imageUrl = "") =>
        Lesson(code, title, topicCode, type, instruction, prompt, new { choices, targetLabel = string.Empty, audioUrl = string.Empty, speechText, imageUrl }, answer);

    private static SeedLesson Multi(string code, string title, string topicCode, string[] choices, string[] answers) =>
        Lesson(code, title, topicCode, InteractionTypes.MultiSelect, "Con chọn tất cả đáp án đúng rồi bấm Hoàn thành.", title, new { choices, correctCount = answers.Length, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, string.Join('|', answers.OrderBy(x => x)));

    private static SeedLesson Listen(string code, string title, string topicCode, string speechText, string[] choices, string answer) =>
        Choice(code, title, topicCode, InteractionTypes.ListenAndChoose, "Con bấm Nghe rồi chọn đáp án đúng.", title, choices, answer, speechText);

    private static SeedLesson Drag(string code, string title, string topicCode, string target, string[] choices, string answer) =>
        Lesson(code, title, topicCode, InteractionTypes.DragDrop, "Con chọn hoặc kéo vật đúng vào vùng đích.", title, new { choices, targetLabel = target, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, answer);

    private static SeedLesson Mapping(
        string code,
        string title,
        string topicCode,
        string type,
        (string Left, string Right)[] mappings,
        bool suppressAutoImage = false)
    {
        var orderedAnswer = string.Join('|', mappings.OrderBy(x => x.Left).Select(x => $"{x.Left}=>{x.Right}"));
        var payload = type == InteractionTypes.Classification
            ? JsonSerializer.Serialize(new { mappings = mappings.Select(x => new { left = x.Left, right = x.Right }), categories = mappings.Select(x => x.Right).Distinct(), imageUrl = string.Empty, suppressAutoImage, audioUrl = string.Empty, speechText = string.Empty })
            : JsonSerializer.Serialize(new { pairs = mappings.Select(x => new { left = x.Left, right = x.Right }), imageUrl = string.Empty, suppressAutoImage, audioUrl = string.Empty, speechText = string.Empty });
        return new(code, title, topicCode, type,
            type == InteractionTypes.Classification ? "Con đưa từng vật vào đúng nhóm màu." : "Con chọn hai mục phù hợp để tạo đường nối.",
            title,
            payload, orderedAnswer, "Quan sát đặc điểm của từng mục rồi thử ghép lại.");
    }

    private static SeedLesson Ordering(string code, string title, string topicCode, string[] items) =>
        Lesson(code, title, topicCode, InteractionTypes.Ordering, "Con dùng các nút mũi tên để sắp xếp đúng thứ tự.", title, new { items, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, string.Join('|', items));

    private static SeedLesson Counting(string code, string title, string topicCode, string symbol, int count) =>
        Lesson(code, title, topicCode, InteractionTypes.Counting, "Con chạm từng đồ vật để đếm rồi chọn số đúng.", title, new { choices = new[] { Math.Max(0, count - 1).ToString(), count.ToString(), (count + 1).ToString() }, objectSymbol = symbol, targetCount = count, imageUrl = ResolveCountingFlashcardUrl(count), audioUrl = string.Empty, speechText = string.Empty }, count.ToString());

    private static SeedLesson Quantity(string code, string title, string topicCode, string symbol, int count) =>
        Lesson(code, title, topicCode, InteractionTypes.QuantityBuilder, "Con thêm hoặc bớt đồ vật để tạo đúng số lượng.", $"Hãy tạo {count} đồ vật.", new { objectSymbol = symbol, targetCount = count, maxItems = count + 3, targetLabel = "Số lượng đã tạo", imageUrl = ResolveCountingFlashcardUrl(count), audioUrl = string.Empty, speechText = string.Empty }, count.ToString());

    private static SeedLesson Comparison(string code, string title, string topicCode, string symbol, string leftLabel, int leftCount, string rightLabel, int rightCount, string comparisonMode = "more")
    {
        var answer = comparisonMode switch
        {
            "equal" => "equal",
            "less" => leftCount < rightCount ? "left" : "right",
            _ => leftCount > rightCount ? "left" : "right"
        };
        return Lesson(code, title, topicCode, InteractionTypes.Comparison, "Con quan sát hai nhóm rồi chọn kết quả phù hợp.", title,
            new { objectSymbol = symbol, leftLabel, leftCount, rightLabel, rightCount, comparisonMode, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, answer);
    }

    private static SeedLesson Story(string code, string title, string topicCode, string speechText, string imageUrl, string prompt, string[] choices, string answer, Dictionary<string, string>? itemMedia = null)
    {
        var payload = itemMedia is { Count: > 0 }
            ? (object)new { choices, targetLabel = string.Empty, audioUrl = string.Empty, speechText, imageUrl, itemMedia }
            : new { choices, targetLabel = string.Empty, audioUrl = string.Empty, speechText, imageUrl };
        return Lesson(code, title, topicCode, InteractionTypes.StoryChoice, "Con nghe câu chuyện, xem tranh rồi chọn đáp án.", prompt, payload, answer);
    }

    private static SeedLesson ReadingQuestion(
        string code,
        string title,
        string speechText,
        string slug,
        string qKey,
        string prompt,
        string[] choices,
        string answer)
    {
        var media = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < choices.Length && i < 3; i++)
        {
            var optPath = $"/images/lessons/doc-hieu/opt-{slug}-{qKey}-opt{i + 1}.jpg";
            media[choices[i]] = optPath;
        }
        var qImg = $"/images/lessons/doc-hieu/qbox-{slug}-{qKey}.jpg";
        return Story(code, title, "doc-hieu", speechText, qImg, prompt, choices, answer, media);
    }

    private static SeedLesson Lesson(string code, string title, string topicCode, string type, string instruction, string prompt, object payload, string answer) =>
        new(code, title, topicCode, type, instruction, prompt, JsonSerializer.Serialize(payload), answer, "Con quan sát kỹ từng thông tin rồi thử lại nhé.");

    private sealed record SeedLesson(
        string Code,
        string Title,
        string TopicCode,
        string InteractionType,
        string Instruction,
        string Prompt,
        string PayloadJson,
        string CorrectAnswer,
        string Hint,
        string Symbol = "",
        int ExpectedStrokeCount = 1,
        byte Level = 1,
        int SortOrder = 0);
}
