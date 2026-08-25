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
                if (existingItem.SortOrder != definition.SortOrder) { existingItem.SortOrder = definition.SortOrder; changed = true; }
                if (existingItem.Title != definition.Title) { existingItem.Title = definition.Title; changed = true; }
                if (existingItem.SkillGroupId != topic.SkillGroupId) { existingItem.SkillGroupId = topic.SkillGroupId; changed = true; }
                if (existingItem.TopicId != topic.Id) { existingItem.TopicId = topic.Id; changed = true; }
                if (existingItem.Level != definition.Level) { existingItem.Level = definition.Level; changed = true; }
                if (existingItem.InteractionType != definition.InteractionType) { existingItem.InteractionType = definition.InteractionType; changed = true; }
                if (existingItem.InstructionText != definition.Instruction) { existingItem.InstructionText = definition.Instruction; changed = true; }
                if (existingItem.ContentJson != payloadJson) { existingItem.ContentJson = payloadJson; changed = true; }

                var existingQuestion = existingItem.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
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
            ("flashcard-shape-heart.svg", "/images/photos/flashcard-shape-heart.svg", "Thẻ học Hình trái tim")
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
        lessons.AddRange(BuildCoverageLessons());

        var topicOrders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        return lessons.Select(lesson =>
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
    }

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
            if (!string.IsNullOrWhiteSpace(tracingImage))
            {
                tracingPayload["imageUrl"] = tracingImage;
                tracingPayload["imageAltText"] = $"Thẻ học {sym}";
            }
            return tracingPayload.ToJsonString();
        }

        var payload = JsonNode.Parse(lesson.PayloadJson)?.AsObject() ?? new JsonObject();
        payload["schemaVersion"] = 2;
        payload["activityType"] = lesson.InteractionType;
        payload["questionAudioUrl"] ??= string.Empty;
        payload["instructionSpeechText"] = lesson.Instruction;
        payload["questionSpeechText"] = lesson.Prompt;
        payload["correctSpeechText"] = "Giỏi lắm, con đã làm đúng!";
        payload["retrySpeechText"] = "Con quan sát kỹ rồi thử lại nhé.";

        if (lesson.TopicCode == "hinh-dang" &&
            string.IsNullOrWhiteSpace(payload["imageUrl"]?.GetValue<string>()))
        {
            payload["focusVisual"] = lesson.CorrectAnswer;
        }

        var currentImage = payload["imageUrl"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentImage))
        {
            currentImage = ResolveQuestionImageUrl(lesson, payload);
            if (!string.IsNullOrWhiteSpace(currentImage))
            {
                payload["imageUrl"] = currentImage;
            }
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
        if (ObservationPhotos.TryGetValue(clean, out var url))
        {
            return url;
        }

        var letter = char.ToUpperInvariant(clean[0]);
        return letter is >= 'A' and <= 'Z'
            ? $"/images/photos/flashcard-letter-{char.ToLowerInvariant(letter)}.jpg"
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
        return string.IsNullOrWhiteSpace(numberImageUrl)
            ? ResolveLetterFlashcardUrl(symbol)
            : numberImageUrl;
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
        yield return Multi("seed-multi-vowels", "Chọn các chữ nguyên âm", "phan-biet-chu", ["A", "B", "E", "M"], ["A", "E"]);
        yield return Multi("seed-multi-even", "Chọn các số chẵn", "so-0-9", ["1", "2", "3", "4"], ["2", "4"]);
        yield return Multi("seed-multi-focus", "Tìm các hình tròn", "tap-trung", ["Hình tròn đỏ", "Hình vuông xanh", "Hình tròn vàng", "Hình tam giác"], ["Hình tròn đỏ", "Hình tròn vàng"]);
        yield return Multi("seed-multi-animals-water", "Chọn các con vật sống dưới nước", "con-vat", ["Cá", "Tôm", "Mèo", "Gà"], ["Cá", "Tôm"]);
        yield return Multi("seed-multi-fruits-red", "Chọn các quả màu đỏ", "phan-loai", ["Táo", "Dâu tây", "Cam", "Bắp cải"], ["Táo", "Dâu tây"]);
        yield return Multi("seed-multi-school-tools", "Chọn đồ dùng để học tập", "phan-loai", ["Bút", "Vở", "Bát", "Thìa"], ["Bút", "Vở"]);
    }

    private static IEnumerable<SeedLesson> BuildListenLessons()
    {
        yield return Listen("seed-listen-cat", "Nghe tiếng con mèo", "con-vat", "Con mèo kêu meo meo.", ["Con mèo", "Con chó", "Con vịt"], "Con mèo");
        yield return Listen("seed-listen-dog", "Nghe tiếng con chó", "con-vat", "Con chó kêu gâu gâu.", ["Con chó", "Con mèo", "Con gà"], "Con chó");
        yield return Listen("seed-listen-duck", "Nghe tiếng con vịt", "con-vat", "Con vịt kêu cạp cạp.", ["Con vịt", "Con chim", "Con gà"], "Con vịt");
        yield return Listen("seed-listen-letter-b", "Nghe và chọn chữ B", "kham-pha-chu", "Đây là chữ Bờ trong quả bóng.", ["A", "B", "D"], "B");
        yield return Listen("seed-listen-letter-c", "Nghe và chọn chữ C", "kham-pha-chu", "Đây là chữ Cờ trong con cá.", ["C", "E", "O"], "C");
        yield return Listen("seed-listen-rhyme", "Nghe từ có vần an", "am-van", "Từ cái bàn có vần an.", ["Bàn", "Bé", "Bò"], "Bàn");
        yield return Listen("seed-listen-rhyme-2", "Nghe từ có vần am", "am-van", "Quả cam có vần am.", ["Cam", "Cá", "Cây"], "Cam");
    }

    private static IEnumerable<SeedLesson> BuildDragLessons()
    {
        yield return Drag("seed-drag-uppercase", "Kéo chữ hoa đúng", "ghep-hoa-thuong", "Chữ hoa A", ["A", "a", "ă"], "A");
        yield return Drag("seed-drag-number", "Kéo số vào nhóm ba vật", "ghep-so-luong", "Nhóm có 3 vật", ["2", "3", "4"], "3");
        yield return Drag("seed-drag-position", "Đặt quả bóng vào trong hộp", "vi-tri", "Trong hộp", ["Quả bóng", "Cái bàn", "Đám mây"], "Quả bóng");
        yield return Drag("seed-drag-vehicle", "Đưa ô tô vào bãi đỗ", "vi-tri", "Bãi đỗ xe", ["Ô tô", "Quả táo", "Con mèo"], "Ô tô");
        yield return Drag("seed-drag-fruit", "Bỏ táo vào giỏ hoa quả", "phan-loai", "Giỏ trái cây", ["Táo", "Bút", "Cái thìa"], "Táo");
    }

    private static IEnumerable<SeedLesson> BuildMatchingLessons()
    {
        yield return Mapping("seed-match-case-1", "Nối chữ hoa với chữ thường", "ghep-hoa-thuong", InteractionTypes.Matching, [("A", "a"), ("B", "b"), ("C", "c")]);
        yield return Mapping("seed-match-case-2", "Nối chữ hoa với chữ thường 2", "ghep-hoa-thuong", InteractionTypes.Matching, [("D", "d"), ("Đ", "đ"), ("E", "e")]);
        yield return Mapping("seed-match-shape", "Nối hình với tên", "hinh-dang", InteractionTypes.Matching, [("○", "Hình tròn"), ("□", "Hình vuông"), ("△", "Hình tam giác")]);
        yield return Mapping("seed-match-vocabulary", "Nối con vật với tiếng kêu", "von-tu", InteractionTypes.Matching, [("Mèo", "Meo meo"), ("Chó", "Gâu gâu"), ("Vịt", "Cạp cạp")]);
        yield return Mapping("seed-match-food-animal", "Nối con vật với thức ăn", "con-vat", InteractionTypes.Matching, [("Thỏ", "Cà rốt"), ("Mèo", "Cá"), ("Ong", "Bông hoa")]);
    }

    private static IEnumerable<SeedLesson> BuildOrderingLessons()
    {
        yield return Ordering("seed-order-numbers", "Sắp xếp số từ bé đến lớn", "thu-tu-so", ["1", "2", "3", "4"]);
        yield return Ordering("seed-order-numbers-desc", "Sắp xếp số từ lớn về bé", "thu-tu-so", ["4", "3", "2", "1"]);
        yield return Ordering("seed-order-wash", "Các bước rửa tay đúng cách", "tu-phuc-vu", ["Làm ướt tay", "Lấy xà phòng", "Chà sạch tay", "Xả nước", "Lau khô"]);
        yield return Ordering("seed-order-seed", "Hạt lớn thành cây", "ke-chuyen", ["Gieo hạt", "Tưới nước", "Hạt nảy mầm", "Cây lớn lên"]);
        yield return Ordering("seed-order-brush-teeth", "Các bước đánh răng", "tu-phuc-vu", ["Lấy kem đánh răng", "Chải sạch răng", "Súc miệng bằng nước"]);
    }

    private static IEnumerable<SeedLesson> BuildCountingLessons()
    {
        yield return Counting("seed-count-3", "Đếm 3 quả táo đỏ", "dem-so-luong", "🍎", 3);
        yield return Counting("seed-count-5", "Đếm 5 ngôi sao vàng", "dem-so-luong", "⭐", 5);
        yield return Counting("seed-count-7", "Đếm 7 bông hoa xinh", "dem-so-luong", "🌼", 7);
        yield return Counting("seed-count-2", "Đếm 2 quả cam", "dem-so-luong", "🍊", 2);
        yield return Counting("seed-count-4", "Đếm 4 chú cá bơi", "dem-so-luong", "🐟", 4);
    }

    private static IEnumerable<SeedLesson> BuildQuantityLessons()
    {
        yield return Quantity("seed-quantity-2", "Tạo 2 quả cam", "tao-so-luong", "🍊", 2);
        yield return Quantity("seed-quantity-4", "Tạo 4 chú cá", "tao-so-luong", "🐟", 4);
        yield return Quantity("seed-quantity-5", "Tạo 5 quả táo", "tao-so-luong", "🍎", 5);
        yield return Quantity("seed-quantity-3", "Tạo 3 ngôi sao", "tao-so-luong", "⭐", 3);
        yield return Quantity("seed-quantity-6", "Tạo 6 khối vuông", "tao-so-luong", "■", 6);
    }

    private static IEnumerable<SeedLesson> BuildComparisonLessons()
    {
        yield return Comparison("seed-compare-more", "Nhóm nào nhiều hơn?", "so-sanh", "🍓", "Rổ đỏ", 5, "Rổ xanh", 3);
        yield return Comparison("seed-compare-less", "Nhóm nào ít hơn?", "so-sanh", "⭐", "Nhóm vàng", 2, "Nhóm xanh", 6, "less");
        yield return Comparison("seed-compare-equal", "Hai nhóm có bằng nhau?", "so-sanh", "●", "Nhóm A", 4, "Nhóm B", 4, "equal");
        yield return Comparison("seed-compare-animals", "Nhóm nào có nhiều cá hơn?", "so-sanh", "🐟", "Bể trái", 6, "Bể phải", 2);
    }

    private static IEnumerable<SeedLesson> BuildClassificationLessons()
    {
        yield return Mapping("seed-classify-food", "Phân loại rau củ và trái cây", "phan-loai", InteractionTypes.Classification, [("Táo", "Trái cây"), ("Cam", "Trái cây"), ("Cà rốt", "Rau củ"), ("Bắp cải", "Rau củ")]);
        yield return Mapping("seed-classify-animal", "Phân loại con vật", "con-vat", InteractionTypes.Classification, [("Cá", "Dưới nước"), ("Tôm", "Dưới nước"), ("Mèo", "Trên cạn"), ("Gà", "Trên cạn")]);
        yield return Mapping("seed-classify-weather", "Chọn đồ dùng theo thời tiết", "thoi-tiet", InteractionTypes.Classification, [("Áo mưa", "Trời mưa"), ("Ô", "Trời mưa"), ("Mũ rộng vành", "Trời nắng"), ("Kính râm", "Trời nắng")]);
        yield return Mapping("seed-classify-transport", "Phân loại phương tiện giao thông", "giao-thong", InteractionTypes.Classification, [("Ô tô", "Trên đường"), ("Xe đạp", "Trên đường"), ("Máy bay", "Trên trời"), ("Thuyền", "Dưới nước")]);
    }

    private static IEnumerable<SeedLesson> BuildStoryLessons()
    {
        yield return Story("seed-story-wash", "Câu chuyện rửa tay sạch sẽ", "tu-phuc-vu",
            "Trước khi ăn, Minh làm ướt tay, lấy xà phòng, chà sạch rồi lau khô.",
            "/images/lessons/story-wash-hands.png", "Minh làm gì trước khi ăn?", ["Rửa tay", "Đi ngủ", "Cất sách"], "Rửa tay");
        yield return Story("seed-story-crossing", "Bé qua đường an toàn", "an-toan",
            "Lan đứng trên vỉa hè cùng mẹ. Khi đèn dành cho người đi bộ bật màu xanh, hai mẹ con quan sát rồi đi trên vạch qua đường.",
            "/images/lessons/story-safe-crossing.png", "Khi nào Lan được qua đường?", ["Khi đèn người đi bộ màu xanh", "Khi xe đang chạy", "Khi đèn người đi bộ màu đỏ"], "Khi đèn người đi bộ màu xanh");
        yield return Story("seed-story-sharing", "Bạn bè biết chia sẻ", "cam-xuc",
            "Nam buồn vì quên hộp bút màu. Mai nhận ra điều đó và vui vẻ chia sẻ bút với Nam.",
            "/images/lessons/story-sharing.png", "Mai đã làm gì khi thấy Nam buồn?", ["Chia sẻ bút màu", "Cất hết bút đi", "Bỏ ra ngoài"], "Chia sẻ bút màu");
        yield return Story("seed-story-traffic", "Đội mũ bảo hiểm khi đi xe máy", "an-toan",
            "Bố đón An đi học về. An tự giác đội mũ bảo hiểm và cài quai cẩn thận trước khi lên xe.",
            "/images/lessons/visual-road-safety.png", "An làm gì trước khi lên xe máy?", ["Đội mũ bảo hiểm", "Đứng nhảy nhót", "Cởi giày"], "Đội mũ bảo hiểm");
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
            yield return Quantity($"seed-quantity-{number}", $"Tạo {number} chấm tròn", "tao-so-luong", "●", number);
        }
        foreach (var number in new[] { 0, 1, 2, 4, 6, 8, 9 })
        {
            yield return Counting($"seed-count-{number}", $"Đếm {number} chấm tròn", "dem-so-luong", "●", number);
        }

        yield return Comparison("seed-compare-more-2", "Chọn nhóm nhiều hơn 2", "so-sanh", "●", "Nhóm A", 3, "Nhóm B", 6, "more");
        yield return Comparison("seed-compare-more-3", "Chọn nhóm nhiều hơn 3", "so-sanh", "■", "Nhóm A", 8, "Nhóm B", 5, "more");
        yield return Comparison("seed-compare-less-2", "Chọn nhóm ít hơn 2", "so-sanh", "▲", "Nhóm A", 7, "Nhóm B", 4, "less");
        yield return Comparison("seed-compare-less-3", "Chọn nhóm ít hơn 3", "so-sanh", "★", "Nhóm A", 2, "Nhóm B", 5, "less");
        yield return Comparison("seed-compare-equal-2", "Hai nhóm bằng nhau 2", "so-sanh", "●", "Nhóm A", 3, "Nhóm B", 3, "equal");
        yield return Comparison("seed-compare-equal-3", "Hai nhóm bằng nhau 3", "so-sanh", "■", "Nhóm A", 6, "Nhóm B", 6, "equal");
        yield return Comparison("seed-compare-more-4", "Chọn nhóm nhiều hơn 4", "so-sanh", "◆", "Nhóm A", 9, "Nhóm B", 7, "more");

        yield return Ordering("seed-fine-fold", "Gấp giấy theo thứ tự", "kheo-tay", ["Đặt giấy ngay ngắn", "Gấp hai mép", "Miết nếp gấp"]);
        yield return Ordering("seed-fine-pencil", "Chuẩn bị bút chì", "kheo-tay", ["Chọn bút", "Cầm bằng ba ngón", "Đặt tay lên giấy"]);
        yield return Drag("seed-fine-maze-1", "Đưa ong về tổ", "me-cung", "Tổ ong", ["Ong", "Cá", "Ô tô"], "Ong");
        yield return Drag("seed-fine-maze-2", "Đưa cá về hồ", "me-cung", "Hồ nước", ["Cá", "Chim", "Xe đạp"], "Cá");
        yield return Drag("seed-fine-maze-3", "Đưa xe về gara", "me-cung", "Gara", ["Ô tô", "Táo", "Bút"], "Ô tô");
        yield return Ordering("seed-fine-beads", "Xâu hạt theo bước", "kheo-tay", ["Chọn sợi dây", "Chọn hạt", "Luồn từng hạt", "Buộc hai đầu"]);

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

        yield return Ordering("seed-logic-pattern-1", "Quy luật đỏ xanh", "quy-luat", ["Đỏ", "Xanh", "Đỏ", "Xanh"]);
        yield return Ordering("seed-logic-pattern-2", "Quy luật nhỏ lớn", "quy-luat", ["Nhỏ", "Lớn", "Nhỏ", "Lớn"]);
        yield return Ordering("seed-logic-pattern-3", "Quy luật một hai", "quy-luat", ["1", "2", "1", "2"]);
        yield return Mapping("seed-logic-classify-1", "Phân loại đồ dùng học tập", "phan-loai", InteractionTypes.Classification, [("Bút", "Học tập"), ("Vở", "Học tập"), ("Bát", "Nhà bếp"), ("Thìa", "Nhà bếp")]);
        yield return Mapping("seed-logic-classify-2", "Phân loại nơi di chuyển", "phan-loai", InteractionTypes.Classification, [("Thuyền", "Dưới nước"), ("Cá", "Dưới nước"), ("Xe", "Trên đường"), ("Xe đạp", "Trên đường")]);
        yield return Mapping("seed-logic-classify-3", "Phân loại ngày và đêm", "phan-loai", InteractionTypes.Classification, [("Mặt trời", "Ban ngày"), ("Đi học", "Ban ngày"), ("Mặt trăng", "Ban đêm"), ("Đi ngủ", "Ban đêm")]);

        yield return Ordering("seed-memory-morning", "Nhớ việc buổi sáng", "ghi-nho", ["Thức dậy", "Đánh răng", "Ăn sáng", "Đi học"]);
        yield return Multi("seed-memory-colors", "Nhớ hai màu đã thấy", "ghi-nho", ["Đỏ", "Xanh", "Vàng", "Tím"], ["Đỏ", "Vàng"]);
        yield return Mapping("seed-memory-pairs", "Nhớ cặp đồ vật", "ghi-nho", InteractionTypes.Matching, [("Bàn chải", "Kem đánh răng"), ("Bát", "Thìa"), ("Bút", "Vở")]);

        yield return Choice("seed-life-helmet", "Đội mũ bảo hiểm khi đi xe máy", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động an toàn.", "Khi ngồi trên xe máy, con cần làm gì?", ["Đội mũ bảo hiểm", "Đứng lên", "Đùa nghịch"], "Đội mũ bảo hiểm",
            imageUrl: "/images/pictograms/helmet.svg");
        yield return Choice("seed-life-stranger", "Không đi theo người lạ", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn cách xử lý an toàn.", "Người lạ rủ con đi theo, con làm gì?", ["Từ chối và gọi người thân", "Đi theo ngay", "Không nói với ai"], "Từ chối và gọi người thân",
            imageUrl: "/images/pictograms/telephone.svg");
        yield return Choice("seed-life-feeling", "Nói ra cảm xúc", "cam-xuc", InteractionTypes.SingleChoice,
            "Con chọn cách chia sẻ phù hợp.", "Khi buồn, con nên làm gì?", ["Nói với người con tin tưởng", "Đập đồ", "La hét vào bạn"], "Nói với người con tin tưởng",
            imageUrl: "/images/pictograms/speaking.svg");

        yield return Ordering("seed-fine-cut-paper", "Cắt giấy an toàn", "kheo-tay", ["Ngồi ngay ngắn", "Cầm kéo đúng tay", "Cắt theo đường", "Cất kéo"]);
        yield return Ordering("seed-fine-coloring", "Tô màu gọn gàng", "kheo-tay", ["Chọn màu", "Tô từ trong ra ngoài", "Tô kín hình", "Cất bút"]);
        yield return Drag("seed-fine-maze-4", "Đưa thỏ về vườn cà rốt", "me-cung", "Vườn cà rốt", ["Thỏ", "Cá", "Máy bay"], "Thỏ");
        yield return Drag("seed-fine-maze-5", "Đưa chim về tổ", "me-cung", "Tổ chim", ["Chim", "Xe buýt", "Quả bóng"], "Chim");

        yield return Choice("seed-logic-different-1", "Tìm vật khác nhóm 1", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không phải trái cây?", ["Táo", "Cam", "Bút chì"], "Bút chì",
            imageUrl: "/images/photos/flashcard-apple.jpg");
        yield return Choice("seed-logic-different-2", "Tìm vật khác nhóm 2", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không phải con vật?", ["Mèo", "Gà", "Cái bàn"], "Cái bàn",
            imageUrl: "/images/photos/cat.jpg");
        yield return Choice("seed-logic-different-3", "Tìm vật khác nhóm 3", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không dùng để đi lại?", ["Xe đạp", "Ô tô", "Cái bát"], "Cái bát",
            imageUrl: "/images/pictograms/bicycle.svg");
        yield return Ordering("seed-logic-pattern-4", "Quy luật tròn vuông", "quy-luat", ["Tròn", "Vuông", "Tròn", "Vuông"]);
        yield return Ordering("seed-logic-pattern-5", "Quy luật cao thấp", "quy-luat", ["Cao", "Thấp", "Cao", "Thấp"]);
        yield return Ordering("seed-logic-pattern-6", "Quy luật một một hai", "quy-luat", ["1", "1", "2", "1", "1", "2"]);
        yield return Mapping("seed-logic-classify-4", "Phân loại nóng và lạnh", "phan-loai", InteractionTypes.Classification, [("Kem", "Lạnh"), ("Nước đá", "Lạnh"), ("Canh", "Nóng"), ("Trà", "Nóng")]);
        yield return Mapping("seed-logic-classify-5", "Phân loại mềm và cứng", "phan-loai", InteractionTypes.Classification, [("Gối", "Mềm"), ("Bông", "Mềm"), ("Đá", "Cứng"), ("Gạch", "Cứng")]);

        yield return Ordering("seed-memory-school", "Nhớ thứ tự đến lớp", "ghi-nho", ["Chào cô", "Cất ba lô", "Ngồi vào chỗ", "Mở sách"]);
        yield return Ordering("seed-memory-lunch", "Nhớ thứ tự bữa ăn", "ghi-nho", ["Rửa tay", "Ngồi ngay ngắn", "Ăn cơm", "Dọn bát"]);
        yield return Multi("seed-memory-shapes", "Nhớ hai hình đã thấy", "ghi-nho", ["Hình tròn", "Hình vuông", "Hình tam giác", "Hình thoi"], ["Hình vuông", "Hình thoi"]);
        yield return Multi("seed-memory-objects", "Nhớ đồ dùng học tập", "ghi-nho", ["Bút", "Vở", "Nồi", "Chảo"], ["Bút", "Vở"]);
        yield return Mapping("seed-memory-pairs-2", "Nhớ cặp trang phục", "ghi-nho", InteractionTypes.Matching, [("Áo", "Quần"), ("Giày", "Tất"), ("Mũ", "Khăn")]);
        yield return Mapping("seed-memory-pairs-3", "Nhớ cặp nơi chốn", "ghi-nho", InteractionTypes.Matching, [("Cá", "Hồ nước"), ("Chim", "Tổ"), ("Xe", "Gara")]);

        yield return Choice("seed-life-electric", "Tránh xa ổ điện", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động an toàn.", "Khi thấy ổ điện, con cần làm gì?", ["Không chạm vào", "Cho tay vào", "Đổ nước lên"], "Không chạm vào",
            imageUrl: "/images/pictograms/electric-plug.svg");
        yield return Choice("seed-life-sharing", "Biết chia sẻ đồ chơi", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn cách cư xử thân thiện.", "Bạn muốn chơi cùng, con nên làm gì?", ["Chia sẻ và chơi cùng", "Giấu đồ chơi", "Đẩy bạn ra"], "Chia sẻ và chơi cùng",
            imageUrl: "/images/pictograms/handshake.svg");
        yield return Choice("seed-life-apology", "Biết nói lời xin lỗi", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời nói phù hợp.", "Khi vô ý làm bạn đau, con nên nói gì?", ["Mình xin lỗi bạn", "Không phải mình", "Bạn tự chịu"], "Mình xin lỗi bạn",
            imageUrl: "/images/pictograms/folded-hands.svg");

        yield return Lesson("seed-visual-count-apples", "Đếm táo trong tranh", "dem-so-luong", InteractionTypes.Counting,
            "Con quan sát tranh, chạm từng quả táo rồi chọn số đúng.", "Trong tranh có bao nhiêu quả táo đỏ?",
            new
            {
                choices = new[] { "4", "5", "6" },
                objectSymbol = "🍎",
                targetCount = 5,
                imageUrl = "/images/lessons/visual-counting-groups.png",
                audioUrl = string.Empty,
                speechText = string.Empty
            }, "5");
        yield return Choice("seed-visual-shape-triangle", "Tìm hình tam giác trong tranh", "hinh-dang", InteractionTypes.SingleChoice,
            "Con quan sát tranh và chạm vào tên hình đúng.", "Hình nào có ba cạnh?",
            ["Hình tam giác", "Hình tròn", "Hình vuông"], "Hình tam giác",
            imageUrl: "/images/lessons/visual-basic-shapes.png");
        yield return Choice("seed-visual-road-crossing", "Qua đường cùng người lớn", "an-toan", InteractionTypes.SingleChoice,
            "Con quan sát tranh và chọn hành động an toàn.", "Khi qua đường, con nên làm gì?",
            ["Đi cùng người lớn", "Tự chạy thật nhanh", "Đứng chơi giữa đường"], "Đi cùng người lớn",
            imageUrl: "/images/lessons/visual-road-safety.png");

        // Bổ sung bài học mới cho các nhóm kỹ năng
        // 1. Phân loại & Nơi ở
        yield return Mapping("seed-logic-animal-homes", "Ghép con vật và nơi ở", "phan-loai", InteractionTypes.Matching,
            [("Chim", "Tổ chim"), ("Cá", "Hồ nước"), ("Thỏ", "Vườn cà rốt"), ("Ong", "Tổ ong")]);
        yield return Mapping("seed-logic-fruits-veggies", "Phân loại rau củ và trái cây", "phan-loai", InteractionTypes.Classification,
            [("Táo", "Trái cây"), ("Cam", "Trái cây"), ("Cà rốt", "Rau củ"), ("Bắp cải", "Rau củ")]);
        yield return Mapping("seed-logic-weather-clothes", "Mặc trang phục theo thời tiết", "phan-loai", InteractionTypes.Matching,
            [("Trời nắng", "Mũ rộng vành"), ("Trời mưa", "Áo mưa"), ("Trời lạnh", "Khăn")]);

        // 2. Kỹ năng sống & Thói quen tốt
        yield return Ordering("seed-life-brush-teeth", "Quy trình đánh răng đúng cách", "kheo-tay",
            ["Lấy kem", "Chải mặt ngoài", "Chải mặt trong", "Súc miệng"]);
        yield return Ordering("seed-life-clean-table", "Gọn gàng góc học tập", "kheo-tay",
            ["Gập sách", "Cất bút", "Xếp ba lô", "Lau bàn"]);
        yield return Choice("seed-life-happy-expression", "Cảm xúc khi làm việc tốt", "cam-xuc", InteractionTypes.SingleChoice,
            "Con chọn cảm xúc phù hợp.", "Khi giúp đỡ bạn, con cảm thấy thế nào?", ["Vui vẻ", "Tức giận", "Buồn bã"], "Vui vẻ",
            imageUrl: "/images/pictograms/speaking.svg");
        yield return Choice("seed-life-help-parents", "Bé ngoan giúp đỡ việc nhà", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn việc làm phù hợp với lứa tuổi.", "Bé có thể giúp mẹ làm việc gì?", ["Gấp quần áo gọn gàng", "Tự ý bật bếp gas", "Nghịch nước bẩn"], "Gấp quần áo gọn gàng",
            imageUrl: "/images/pictograms/shirt.svg");

        // 3. Ngôn ngữ, Câu đố vần & Kể chuyện
        yield return Choice("seed-lang-rhyme-cat", "Câu đố: Chú mèo bắt chuột", "kham-pha-chu", InteractionTypes.SingleChoice,
            "Con nghe câu đố và chọn đáp án.", "Con gì mắt sáng, thích bắt chuột, kêu meo meo?", ["Con mèo", "Con cún", "Con vịt"], "Con mèo",
            imageUrl: "/images/photos/cat.jpg");
        yield return Choice("seed-lang-rhyme-rooster", "Câu đố: Tiếng gáy ban mai", "kham-pha-chu", InteractionTypes.SingleChoice,
            "Con nghe câu đố và chọn đáp án.", "Con gì gáy ò ó o gọi mọi người thức dậy?", ["Gà trống", "Con thỏ", "Con ong"], "Gà trống",
            imageUrl: "/images/photos/chicken.jpg");
        yield return Story("seed-lang-story-frog", "Chuyện chú ếch ngoan ngoãn", "giao-tiep",
            "Mỗi buổi sáng gặp người lớn, chú ếch nhỏ đều cúi đầu lễ phép khoanh tay chào hỏi.",
            "/images/pictograms/speaking.svg",
            "Chú ếch con trong truyện đã làm gì khi gặp người lớn?",
            ["Khoanh tay chào lễ phép", "Nhảy đi mất", "Không nói gì"], "Khoanh tay chào lễ phép");

        // 4. Hình dạng & Không gian
        yield return Choice("seed-shape-find-circle", "Đồ vật hình tròn quanh bé", "hinh-dang", InteractionTypes.SingleChoice,
            "Con quan sát và chọn đồ vật có hình tròn.", "Đồ vật nào dưới đây có dạng hình tròn?", ["Quả bóng", "Cái bảng đen", "Hộp quà vuông"], "Quả bóng",
            imageUrl: "/images/pictograms/ball.svg");
        yield return Choice("seed-shape-find-square", "Đồ vật hình vuông", "hinh-dang", InteractionTypes.SingleChoice,
            "Con chọn đồ vật có bốn cạnh bằng nhau.", "Đồ vật nào có dạng hình vuông?", ["Cái khăn vuông", "Quả trứng", "Bánh xe"], "Cái khăn vuông",
            imageUrl: "/images/photos/flashcard-shape-square.svg");
        yield return Choice("seed-shape-find-triangle", "Đồ vật hình tam giác", "hinh-dang", InteractionTypes.SingleChoice,
            "Con chọn đồ vật có dạng hình tam giác.", "Biển báo giao thông có dạng hình gì?", ["Hình tam giác", "Hình tròn", "Hình vuông"], "Hình tam giác",
            imageUrl: "/images/photos/flashcard-shape-triangle.svg");

        // 5. Chữ số và Toán học (Số 10-20, Thứ tự, Tách gộp, Cộng bớt)
        yield return Choice("seed-math-num-10", "Nhận biết số 10", "so-10-20", InteractionTypes.SingleChoice,
            "Con quan sát và chọn số mười.", "Đâu là số 10?", ["10", "1", "0"], "10",
            imageUrl: "/images/photos/flashcard-number-10.jpg");
        yield return Choice("seed-math-order-after-5", "Số liền sau số 5", "thu-tu-so", InteractionTypes.SingleChoice,
            "Con tìm số đứng ngay sau số năm.", "Số nào đứng liền sau số 5?", ["6", "4", "3"], "6");
        yield return Choice("seed-math-order-before-8", "Số liền trước số 8", "thu-tu-so", InteractionTypes.SingleChoice,
            "Con tìm số đứng ngay trước số tám.", "Số nào đứng liền trước số 8?", ["7", "9", "6"], "7");
        yield return Choice("seed-math-split-combine-4", "Tách gộp 4 quả táo", "tach-gop", InteractionTypes.SingleChoice,
            "Con quan sát cách tách bốn quả táo.", "Bốn quả táo có thể tách thành hai nhóm nào?", ["2 quả và 2 quả", "1 quả và 5 quả", "3 quả và 3 quả"], "2 quả và 2 quả",
            imageUrl: "/images/photos/flashcard-apple.jpg");
        yield return Choice("seed-math-add-simple", "Bé làm phép tính thêm", "cong-bot", InteractionTypes.SingleChoice,
            "Có hai quả bóng, thêm một quả bóng nữa là mấy quả bóng?", "Có 2 quả bóng, thêm 1 quả bóng là mấy quả bóng?", ["3 quả bóng", "1 quả bóng", "4 quả bóng"], "3 quả bóng",
            imageUrl: "/images/pictograms/ball.svg");
        yield return Choice("seed-math-subtract-simple", "Bé làm phép tính bớt", "cong-bot", InteractionTypes.SingleChoice,
            "Có ba quả cam, ăn mất một quả thì còn mấy quả?", "Có 3 quả cam, bớt 1 quả thì còn lại mấy quả?", ["2 quả cam", "4 quả cam", "1 quả cam"], "2 quả cam",
            imageUrl: "/images/photos/flashcard-orange.jpg");

        // 6. Tư duy logic (Quy luật, Ghép bóng, Tìm khác biệt)
        yield return Choice("seed-logic-find-different", "Tìm con vật sống dưới nước", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm con vật khác biệt với các con còn lại.", "Con vật nào sống dưới nước?", ["Con cá", "Con mèo", "Con chó"], "Con cá",
            imageUrl: "/images/photos/fish.jpg");
        yield return Choice("seed-logic-find-fly", "Tìm loài vật biết bay", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm loài vật có cánh biết bay lượn.", "Loài vật nào biết bay trên trời?", ["Con chim", "Con tôm", "Con thỏ"], "Con chim",
            imageUrl: "/images/pictograms/bird.svg");
        yield return Ordering("seed-logic-color-pattern", "Quy luật màu sắc đỏ vàng", "quy-luat",
            ["Đỏ", "Vàng", "Đỏ", "Vàng"]);
        yield return Mapping("seed-logic-match-shadow", "Ghép con vật và đặc điểm", "ghep-bong", InteractionTypes.Matching,
            [("Con mèo", "Thích bắt chuột"), ("Con chó", "Trông giữ nhà"), ("Con vịt", "Biết bơi lội")]);

        // 7. Kỹ năng sống & An toàn
        yield return Choice("seed-life-helmet", "Đội mũ bảo hiểm khi đi xe", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn trang bị an toàn khi ngồi xe máy.", "Khi ngồi trên xe máy cùng bố mẹ, bé cần đội gì?", ["Mũ bảo hiểm", "Mũ len", "Mũ bơi"], "Mũ bảo hiểm",
            imageUrl: "/images/pictograms/helmet.svg");
        yield return Choice("seed-life-traffic-light", "Tuân thủ đèn giao thông", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động đúng theo tín hiệu đèn.", "Khi đèn giao thông màu đỏ sáng lên, người đi đường phải làm gì?", ["Dừng lại", "Chạy nhanh qua", "Bấm còi"], "Dừng lại",
            imageUrl: "/images/pictograms/car.svg");
        yield return Choice("seed-life-sharp-objects", "An toàn với vật sắc nhọn", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn cách xử lý an toàn.", "Khi thấy dao kéo hoặc vật sắc nhọn, con nên làm gì?", ["Không tự ý nghịch", "Lấy ra chơi đồ hàng", "Cầm chạy nhảy"], "Không tự ý nghịch",
            imageUrl: "/images/pictograms/cooking-pot.svg");
        yield return Choice("seed-life-wash-hands", "Rửa tay sạch bằng xà phòng", "tu-phuc-vu", InteractionTypes.SingleChoice,
            "Con chọn thời điểm cần rửa tay.", "Bé nên rửa tay sạch bằng xà phòng khi nào?", ["Trước khi ăn và sau khi đi vệ sinh", "Chỉ khi bị mẹ nhắc", "Không cần rửa"], "Trước khi ăn và sau khi đi vệ sinh",
            imageUrl: "/images/pictograms/soap.svg");
        yield return Choice("seed-life-greeting-school", "Lời chào khi đến trường", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời chào lễ phép khi đến lớp.", "Khi đến trường gặp cô giáo, bé nói gì?", ["Con chào cô ạ!", "Tớ đến rồi", "Không nói gì"], "Con chào cô ạ!",
            imageUrl: "/images/pictograms/speaking.svg");
        yield return Choice("seed-life-say-thanks", "Lời cảm ơn khi nhận quà", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời nói lễ phép khi nhận quà.", "Khi được ông bà tặng quà, bé nói gì?", ["Con cảm ơn ông bà ạ!", "Cho con thêm cái nữa", "Cầm lấy rồi đi ngay"], "Con cảm ơn ông bà ạ!",
            imageUrl: "/images/pictograms/folded-hands.svg");

        // 8. Tiền tập đọc, Ngôn ngữ & Kể chuyện
        yield return Choice("seed-lang-body-parts", "Nhận biết các giác quan", "von-tu", InteractionTypes.SingleChoice,
            "Con chọn bộ phận dùng để nhìn ngắm.", "Bộ phận nào trên khuôn mặt giúp bé nhìn thấy vạn vật?", ["Đôi mắt", "Đôi tai", "Cái mũi"], "Đôi mắt",
            imageUrl: "/images/pictograms/flower.svg");
        yield return Choice("seed-lang-sound-b", "Tiếng có âm B", "am-van", InteractionTypes.SingleChoice,
            "Con lắng nghe và tìm từ bắt đầu bằng âm B.", "Từ nào dưới đây bắt đầu bằng âm B?", ["Bút chì", "Cái kéo", "Quyển vở"], "Bút chì",
            imageUrl: "/images/pictograms/pencil.svg");
        yield return Choice("seed-lang-sound-c", "Tiếng có âm C", "am-van", InteractionTypes.SingleChoice,
            "Con tìm từ bắt đầu bằng âm C.", "Từ nào dưới đây bắt đầu bằng âm C?", ["Con cá", "Quả táo", "Bông hoa"], "Con cá",
            imageUrl: "/images/photos/fish.jpg");
        yield return Story("seed-lang-story-tortoise", "Chuyện Rùa và Thỏ", "ke-chuyen",
            "Thỏ cậy mình chạy nhanh nên mải chơi, còn Rùa kiên trì từng bước và đã về đích trước.",
            "/images/photos/flashcard-rabbit.jpg",
            "Trong cuộc thi chạy, vì sao chú Rùa lại chiến thắng chú Thỏ?",
            ["Rùa kiên trì, chăm chỉ", "Rùa chạy nhanh hơn Thỏ", "Thỏ nhường cho Rùa"], "Rùa kiên trì, chăm chỉ");

        // 9. Vị trí & Kích thước trong không gian
        yield return Choice("seed-space-up-down", "Vị trí trên và dưới", "vi-tri", InteractionTypes.SingleChoice,
            "Con quan sát vị trí của chú chim và cây xanh.", "Chú chim đang đậu ở đâu?", ["Trên cành cây", "Dưới mặt đất", "Trong hồ nước"], "Trên cành cây",
            imageUrl: "/images/pictograms/bird.svg");
        yield return Choice("seed-space-size-compare", "So sánh lớn hơn và nhỏ hơn", "kich-thuoc", InteractionTypes.SingleChoice,
            "Con so sánh kích thước của hai con vật.", "Con voi như thế nào so với con chuột?", ["To lớn hơn", "Nhỏ bé hơn", "Bằng nhau"], "To lớn hơn",
            imageUrl: "/images/photos/cat.jpg");

        // 10. Khám phá thế giới
        yield return Mapping("seed-world-animals-habitat", "Môi trường sống của động vật", "con-vat", InteractionTypes.Classification,
            [("Con cá", "Dưới nước"), ("Con tôm", "Dưới nước"), ("Con mèo", "Trên cạn"), ("Con chó", "Trên cạn")]);
        yield return Mapping("seed-world-vehicles-transport", "Phương tiện và đường đi", "giao-thong", InteractionTypes.Matching,
            [("Ô tô", "Đường bộ"), ("Tàu buồm", "Đường thủy"), ("Máy bay", "Đường hàng không")]);
        yield return Choice("seed-world-plant-roots", "Bộ phận hút nước của cây", "cay-co", InteractionTypes.SingleChoice,
            "Con chọn bộ phận dưới lòng đất của cây.", "Bộ phận nào nằm dưới đất hút chất dinh dưỡng cho cây?", ["Rễ cây", "Lá cây", "Bông hoa"], "Rễ cây",
            imageUrl: "/images/pictograms/seedling.svg");
        yield return Choice("seed-world-day-night", "Bầu trời ban ngày và ban đêm", "thoi-tiet", InteractionTypes.SingleChoice,
            "Con quan sát hiện tượng tự nhiên.", "Khi ban đêm đến, bé nhìn thấy gì trên bầu trời?", ["Mặt trăng và các vì sao", "Mặt trời rực rỡ", "Cầu vồng bảy sắc"], "Mặt trăng và các vì sao",
            imageUrl: "/images/pictograms/moon.svg");
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
        Lesson(code, title, topicCode, InteractionTypes.MultiSelect, "Con chọn tất cả đáp án đúng rồi bấm Hoàn thành.", "Những đáp án nào phù hợp?", new { choices, correctCount = answers.Length, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, string.Join('|', answers.OrderBy(x => x)));

    private static SeedLesson Listen(string code, string title, string topicCode, string speechText, string[] choices, string answer) =>
        Choice(code, title, topicCode, InteractionTypes.ListenAndChoose, "Con bấm Nghe rồi chọn đáp án đúng.", "Con vừa nghe thấy gì?", choices, answer, speechText);

    private static SeedLesson Drag(string code, string title, string topicCode, string target, string[] choices, string answer) =>
        Lesson(code, title, topicCode, InteractionTypes.DragDrop, "Con chọn hoặc kéo vật đúng vào vùng đích.", $"Vật nào thuộc vùng “{target}”?", new { choices, targetLabel = target, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, answer);

    private static SeedLesson Mapping(string code, string title, string topicCode, string type, (string Left, string Right)[] mappings)
    {
        var orderedAnswer = string.Join('|', mappings.OrderBy(x => x.Left).Select(x => $"{x.Left}=>{x.Right}"));
        var payload = type == InteractionTypes.Classification
            ? JsonSerializer.Serialize(new { mappings = mappings.Select(x => new { left = x.Left, right = x.Right }), categories = mappings.Select(x => x.Right).Distinct(), imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty })
            : JsonSerializer.Serialize(new { pairs = mappings.Select(x => new { left = x.Left, right = x.Right }), imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty });
        return new(code, title, topicCode, type,
            type == InteractionTypes.Classification ? "Con đưa từng vật vào đúng nhóm màu." : "Con chọn hai mục phù hợp để tạo đường nối.",
            type == InteractionTypes.Classification ? "Mỗi vật thuộc nhóm nào?" : "Con hãy nối đủ các cặp.",
            payload, orderedAnswer, "Quan sát đặc điểm của từng mục rồi thử ghép lại.");
    }

    private static SeedLesson Ordering(string code, string title, string topicCode, string[] items) =>
        Lesson(code, title, topicCode, InteractionTypes.Ordering, "Con dùng các nút mũi tên để sắp xếp đúng thứ tự.", "Thứ tự đúng là gì?", new { items, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, string.Join('|', items));

    private static SeedLesson Counting(string code, string title, string topicCode, string symbol, int count) =>
        Lesson(code, title, topicCode, InteractionTypes.Counting, "Con chạm từng đồ vật để đếm rồi chọn số đúng.", "Có bao nhiêu đồ vật?", new { choices = new[] { Math.Max(0, count - 1).ToString(), count.ToString(), (count + 1).ToString() }, objectSymbol = symbol, targetCount = count, imageUrl = ResolveCountingFlashcardUrl(count), audioUrl = string.Empty, speechText = string.Empty }, count.ToString());

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

    private static SeedLesson Story(string code, string title, string topicCode, string speechText, string imageUrl, string prompt, string[] choices, string answer) =>
        Choice(code, title, topicCode, InteractionTypes.StoryChoice, "Con nghe câu chuyện, xem tranh rồi chọn đáp án.", prompt, choices, answer, speechText, imageUrl);

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
