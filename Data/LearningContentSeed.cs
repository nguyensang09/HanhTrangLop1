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
            ["Táo"] = "/images/photos/flashcard-apple.jpg",
            ["Quả táo"] = "/images/photos/flashcard-apple.jpg",
            ["🍎"] = "/images/photos/flashcard-apple.jpg",
            ["Cam"] = "/images/photos/flashcard-orange.jpg",
            ["Quả cam"] = "/images/photos/flashcard-orange.jpg",
            ["🍊"] = "/images/photos/flashcard-orange.jpg",
            ["Cà rốt"] = "/images/photos/flashcard-carrot.jpg",
            ["Bắp cải"] = "/images/photos/flashcard-cabbage.jpg",
            ["Mèo"] = "/images/photos/cat.jpg",
            ["Con mèo"] = "/images/photos/cat.jpg",
            ["Vịt"] = "/images/photos/duck.jpg",
            ["Con vịt"] = "/images/photos/duck.jpg",
            ["Cá"] = "/images/photos/fish.jpg",
            ["Con cá"] = "/images/photos/fish.jpg",
            ["Chú cá"] = "/images/photos/fish.jpg",
            ["🐟"] = "/images/photos/fish.jpg",
            ["Tôm"] = "/images/photos/flashcard-shrimp.jpg",
            ["Con tôm"] = "/images/photos/flashcard-shrimp.jpg",
            ["Gà"] = "/images/photos/chicken.jpg",
            ["Con gà"] = "/images/photos/chicken.jpg",
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
            ["A"] = "/images/photos/flashcard-letter-a.jpg",
            ["a"] = "/images/photos/flashcard-letter-a.jpg",
            ["B"] = "/images/photos/flashcard-letter-b.jpg",
            ["b"] = "/images/photos/flashcard-letter-b.jpg",
            ["C"] = "/images/photos/flashcard-letter-c.jpg",
            ["c"] = "/images/photos/flashcard-letter-c.jpg",
            ["D"] = "/images/photos/flashcard-letter-d.jpg",
            ["d"] = "/images/photos/flashcard-letter-d.jpg",
            ["E"] = "/images/photos/flashcard-letter-e.jpg",
            ["e"] = "/images/photos/flashcard-letter-e.jpg",
            ["F"] = "/images/photos/flashcard-letter-f.jpg",
            ["f"] = "/images/photos/flashcard-letter-f.jpg",
            ["G"] = "/images/photos/flashcard-letter-g.jpg",
            ["g"] = "/images/photos/flashcard-letter-g.jpg",
            ["H"] = "/images/photos/flashcard-letter-h.jpg",
            ["h"] = "/images/photos/flashcard-letter-h.jpg",
            ["I"] = "/images/photos/flashcard-letter-i.jpg",
            ["i"] = "/images/photos/flashcard-letter-i.jpg",
            ["J"] = "/images/photos/flashcard-letter-j.jpg",
            ["j"] = "/images/photos/flashcard-letter-j.jpg",
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
            ["V"] = "/images/photos/flashcard-letter-v.jpg",
            ["v"] = "/images/photos/flashcard-letter-v.jpg",
            ["W"] = "/images/photos/flashcard-letter-w.jpg",
            ["w"] = "/images/photos/flashcard-letter-w.jpg",
            ["X"] = "/images/photos/flashcard-letter-x.jpg",
            ["x"] = "/images/photos/flashcard-letter-x.jpg",
            ["Y"] = "/images/photos/flashcard-letter-y.jpg",
            ["y"] = "/images/photos/flashcard-letter-y.jpg",
            ["Z"] = "/images/photos/flashcard-letter-z.jpg",
            ["z"] = "/images/photos/flashcard-letter-z.jpg",
            ["Ong"] = "/images/photos/flashcard-bee.jpg",
            ["Bướm"] = "/images/photos/flashcard-butterfly.jpg",
            ["Thỏ"] = "/images/photos/flashcard-rabbit.jpg",
            ["Con thỏ"] = "/images/photos/flashcard-rabbit.jpg",
            ["Rùa biển"] = "/images/photos/flashcard-sea-turtle.jpg",
            ["Động vật biển"] = "/images/photos/flashcard-sea-animal.jpg",
            ["Côn trùng"] = "/images/photos/flashcard-insects.jpg"
        };
    private static readonly string[] VietnameseAlphabet =
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
            if (existingSeedItems.TryGetValue(definition.Code, out var existingItem))
            {
                existingItem.SortOrder = definition.SortOrder;
                if (definition.InteractionType != InteractionTypes.Tracing)
                {
                    existingItem.Title = definition.Title;
                    existingItem.SkillGroupId = topic.SkillGroupId;
                    existingItem.TopicId = topic.Id;
                    existingItem.Level = definition.Level;
                    existingItem.InteractionType = definition.InteractionType;
                    existingItem.InstructionText = definition.Instruction;
                    existingItem.ContentJson = definition.PayloadJson;
                    var existingQuestion = existingItem.Questions.OrderBy(x => x.SortOrder).FirstOrDefault();
                    if (existingQuestion is not null)
                    {
                        ApplyQuestionDefinition(existingQuestion, definition, definition.PayloadJson);
                    }
                }
                continue;
            }

            var itemId = Guid.NewGuid();
            var payloadJson = definition.PayloadJson;

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
                        definition.TopicCode == "chu-in-thuong" ? "lowercase" : "uppercase",
                    Symbol = definition.Symbol,
                    DisplayName = definition.Title,
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

    private static void ApplyQuestionDefinition(Question question, SeedLesson definition, string payloadJson)
    {
        question.PromptText = definition.Prompt;
        question.QuestionType = definition.InteractionType;
        question.PayloadJson = payloadJson;
        question.CorrectAnswerJson = definition.InteractionType == InteractionTypes.Tracing
            ? JsonSerializer.Serialize(new { minPoints = 20, expectedStrokeCount = definition.ExpectedStrokeCount })
            : JsonSerializer.Serialize(new { value = definition.CorrectAnswer });
        question.HintJson = JsonSerializer.Serialize(new { level1 = definition.Hint });
        question.FeedbackJson = JsonSerializer.Serialize(new
        {
            correct = "Giỏi lắm, con đã hoàn thành đúng!",
            retry = "Chưa đúng rồi. Con quan sát kỹ và thử lại nhé."
        });
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
            ("flashcard-letter-a.jpg", "/images/photos/flashcard-letter-a.jpg", "Thẻ chữ A a với hình quả táo"),
            ("flashcard-letter-b.jpg", "/images/photos/flashcard-letter-b.jpg", "Thẻ chữ B b với hình quả bóng"),
            ("flashcard-letter-c.jpg", "/images/photos/flashcard-letter-c.jpg", "Thẻ chữ C c với hình con mèo"),
            ("flashcard-letter-d.jpg", "/images/photos/flashcard-letter-d.jpg", "Thẻ chữ D d với hình búp bê"),
            ("flashcard-letter-e.jpg", "/images/photos/flashcard-letter-e.jpg", "Thẻ chữ E e với hình quả trứng"),
            ("flashcard-letter-f.jpg", "/images/photos/flashcard-letter-f.jpg", "Thẻ chữ F f với hình quạt"),
            ("flashcard-letter-g.jpg", "/images/photos/flashcard-letter-g.jpg", "Thẻ chữ G g với hình khu vườn"),
            ("flashcard-letter-h.jpg", "/images/photos/flashcard-letter-h.jpg", "Thẻ chữ H h với hình bàn tay"),
            ("flashcard-letter-i.jpg", "/images/photos/flashcard-letter-i.jpg", "Thẻ chữ I i với hình băng"),
            ("flashcard-letter-j.jpg", "/images/photos/flashcard-letter-j.jpg", "Thẻ chữ J j với hình mứt"),
            ("flashcard-letter-k.jpg", "/images/photos/flashcard-letter-k.jpg", "Thẻ chữ K k với hình chuột túi"),
            ("flashcard-letter-l.jpg", "/images/photos/flashcard-letter-l.jpg", "Thẻ chữ L l với hình cừu"),
            ("flashcard-letter-m.jpg", "/images/photos/flashcard-letter-m.jpg", "Thẻ chữ M m với hình nấm"),
            ("flashcard-letter-n.jpg", "/images/photos/flashcard-letter-n.jpg", "Thẻ chữ N n với hình lưới"),
            ("flashcard-letter-o.jpg", "/images/photos/flashcard-letter-o.jpg", "Thẻ chữ O o với hình quả cam"),
            ("flashcard-letter-p.jpg", "/images/photos/flashcard-letter-p.jpg", "Thẻ chữ P p với hình thú cưng"),
            ("flashcard-letter-q.jpg", "/images/photos/flashcard-letter-q.jpg", "Thẻ chữ Q q với hình chăn"),
            ("flashcard-letter-r.jpg", "/images/photos/flashcard-letter-r.jpg", "Thẻ chữ R r với hình mưa"),
            ("flashcard-letter-s.jpg", "/images/photos/flashcard-letter-s.jpg", "Thẻ chữ S s với hình hoa hướng dương"),
            ("flashcard-letter-t.jpg", "/images/photos/flashcard-letter-t.jpg", "Thẻ chữ T t với hình tàu hỏa"),
            ("flashcard-letter-u.jpg", "/images/photos/flashcard-letter-u.jpg", "Thẻ chữ U u với hình quần áo lót"),
            ("flashcard-letter-v.jpg", "/images/photos/flashcard-letter-v.jpg", "Thẻ chữ V v với hình bình hoa"),
            ("flashcard-letter-w.jpg", "/images/photos/flashcard-letter-w.jpg", "Thẻ chữ W w với hình xe kéo"),
            ("flashcard-letter-x.jpg", "/images/photos/flashcard-letter-x.jpg", "Thẻ chữ X x với hình phim X-quang"),
            ("flashcard-letter-y.jpg", "/images/photos/flashcard-letter-y.jpg", "Thẻ chữ Y y với hình yo-yo"),
            ("flashcard-letter-z.jpg", "/images/photos/flashcard-letter-z.jpg", "Thẻ chữ Z z với hình ngựa vằn")
        };
        var numberImages = Enumerable.Range(1, 20)
            .Select(number => (
                $"flashcard-number-{number}.jpg",
                $"/images/photos/flashcard-number-{number}.jpg",
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
                ContentType = Path.GetExtension(image.Item1).Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                    ? "image/jpeg"
                    : "image/png",
                StoragePath = image.Item2,
                AltText = image.Item3,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private static IReadOnlyList<SeedLesson> BuildDefinitions()
    {
        var lessons = new List<SeedLesson>();

        for (var index = 0; index < VietnameseAlphabet.Length; index++)
        {
            var upper = VietnameseAlphabet[index];
            var lower = upper.ToLower(new System.Globalization.CultureInfo("vi-VN"));
            lessons.Add(Tracing($"seed-tracing-upper-{index + 1:00}", $"Tô chữ {upper} in hoa", "chu-in-hoa", upper, 2));
            lessons.Add(Tracing($"seed-tracing-lower-{index + 1:00}", $"Tô chữ {lower} in thường", "chu-in-thuong", lower, 2));
        }

        for (var number = 0; number <= 9; number++)
        {
            lessons.Add(Tracing($"seed-tracing-number-{number}", $"Tô số {number}", "viet-so", number.ToString(), 1));
            var choices = new[] { Math.Max(0, number - 1).ToString(), number.ToString(), Math.Min(9, number + 1).ToString() }.Distinct().ToArray();
            if (choices.Length < 2) choices = [number.ToString(), number == 0 ? "1" : "0"];
            lessons.Add(Choice(
                $"seed-recognize-number-{number}", $"Nhận biết số {number}", "so-0-9", InteractionTypes.SingleChoice,
                "Con quan sát và chọn đúng chữ số.", $"Đâu là số {number}?", choices, number.ToString()));
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
            return lesson.PayloadJson;
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
            "/images/lessons/story-lost-pencil.png" => "Bạn nhỏ tìm thấy chiếc bút chì bị thất lạc",
            "/images/lessons/story-rainy-day.png" => "Các bạn nhỏ giúp nhau trong ngày mưa",
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
        if (lesson.InteractionType is InteractionTypes.MultiSelect or InteractionTypes.Matching or
            InteractionTypes.Ordering or InteractionTypes.Classification or InteractionTypes.Comparison)
        {
            return string.Empty;
        }

        foreach (var candidate in ResolveQuestionImageCandidates(lesson, payload))
        {
            if (TryResolveObservationPhoto(candidate, out var imageUrl))
            {
                return imageUrl;
            }
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

        return string.Empty;
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
            normalized.StartsWith("Quả ", StringComparison.OrdinalIgnoreCase) ? normalized[4..] : normalized
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

    private static string ResolveLetterFlashcardUrl(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol) || symbol.Length != 1)
        {
            return string.Empty;
        }

        var letter = char.ToUpperInvariant(symbol[0]);
        return letter is >= 'A' and <= 'Z'
            ? $"/images/photos/flashcard-letter-{char.ToLowerInvariant(letter)}.jpg"
            : string.Empty;
    }

    private static string ResolveNumberFlashcardUrl(string symbol)
    {
        return int.TryParse(symbol, out var number) && number is >= 1 and <= 20
            ? $"/images/photos/flashcard-number-{number}.jpg"
            : string.Empty;
    }

    private static string ResolveCountingFlashcardUrl(int count)
    {
        return count is >= 1 and <= 10
            ? $"/images/photos/flashcard-count-{count}.jpg"
            : string.Empty;
    }

    private static string ResolveTracingFlashcardUrl(string symbol)
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
    }

    private static IEnumerable<SeedLesson> BuildListenLessons()
    {
        yield return Listen("seed-listen-cat", "Nghe tiếng con mèo", "con-vat", "Con mèo kêu meo meo.", ["Con mèo", "Con chó", "Con vịt"], "Con mèo");
        yield return Listen("seed-listen-letter-b", "Nghe và chọn chữ B", "kham-pha-chu", "Đây là chữ Bờ.", ["A", "B", "D"], "B");
        yield return Listen("seed-listen-rhyme", "Nghe từ có vần an", "am-van", "Từ bàn có vần an.", ["Bàn", "Bé", "Bò"], "Bàn");
    }

    private static IEnumerable<SeedLesson> BuildDragLessons()
    {
        yield return Drag("seed-drag-uppercase", "Kéo chữ hoa đúng", "ghep-hoa-thuong", "Chữ hoa", ["A", "a", "ă"], "A");
        yield return Drag("seed-drag-number", "Kéo số vào nhóm ba vật", "ghep-so-luong", "Nhóm có 3 vật", ["2", "3", "4"], "3");
        yield return Drag("seed-drag-position", "Đặt quả bóng vào trong hộp", "vi-tri", "Trong hộp", ["Quả bóng", "Cái bàn", "Đám mây"], "Quả bóng");
    }

    private static IEnumerable<SeedLesson> BuildMatchingLessons()
    {
        yield return Mapping("seed-match-case-1", "Nối chữ hoa với chữ thường", "ghep-hoa-thuong", InteractionTypes.Matching, [("A", "a"), ("B", "b"), ("C", "c")]);
        yield return Mapping("seed-match-shape", "Nối hình với tên", "hinh-dang", InteractionTypes.Matching, [("○", "Hình tròn"), ("□", "Hình vuông"), ("△", "Hình tam giác")]);
        yield return Mapping("seed-match-vocabulary", "Nối con vật với tiếng kêu", "von-tu", InteractionTypes.Matching, [("Mèo", "Meo meo"), ("Chó", "Gâu gâu"), ("Vịt", "Cạp cạp")]);
    }

    private static IEnumerable<SeedLesson> BuildOrderingLessons()
    {
        yield return Ordering("seed-order-numbers", "Sắp xếp số từ bé đến lớn", "thu-tu-so", ["1", "2", "3", "4"]);
        yield return Ordering("seed-order-wash", "Các bước rửa tay", "tu-phuc-vu", ["Làm ướt tay", "Lấy xà phòng", "Chà sạch tay", "Xả nước", "Lau khô"]);
        yield return Ordering("seed-order-seed", "Hạt lớn thành cây", "ke-chuyen", ["Gieo hạt", "Tưới nước", "Hạt nảy mầm", "Cây lớn lên"]);
    }

    private static IEnumerable<SeedLesson> BuildCountingLessons()
    {
        yield return Counting("seed-count-3", "Đếm 3 quả táo", "dem-so-luong", "🍎", 3);
        yield return Counting("seed-count-5", "Đếm 5 ngôi sao", "dem-so-luong", "⭐", 5);
        yield return Counting("seed-count-7", "Đếm 7 bông hoa", "dem-so-luong", "🌼", 7);
    }

    private static IEnumerable<SeedLesson> BuildQuantityLessons()
    {
        yield return Quantity("seed-quantity-2", "Tạo 2 quả cam", "tao-so-luong", "🍊", 2);
        yield return Quantity("seed-quantity-4", "Tạo 4 chú cá", "tao-so-luong", "🐟", 4);
        yield return Quantity("seed-quantity-6", "Tạo 6 khối vuông", "tao-so-luong", "■", 6);
    }

    private static IEnumerable<SeedLesson> BuildComparisonLessons()
    {
        yield return Comparison("seed-compare-more", "Nhóm nào nhiều hơn?", "so-sanh", "🍓", "Rổ đỏ", 5, "Rổ xanh", 3);
        yield return Comparison("seed-compare-less", "Nhóm nào ít hơn?", "so-sanh", "⭐", "Nhóm vàng", 2, "Nhóm xanh", 6, "less");
        yield return Comparison("seed-compare-equal", "Hai nhóm có bằng nhau?", "so-sanh", "●", "Nhóm A", 4, "Nhóm B", 4, "equal");
    }

    private static IEnumerable<SeedLesson> BuildClassificationLessons()
    {
        yield return Mapping("seed-classify-food", "Phân loại rau củ và trái cây", "phan-loai", InteractionTypes.Classification, [("Táo", "Trái cây"), ("Cam", "Trái cây"), ("Cà rốt", "Rau củ"), ("Bắp cải", "Rau củ")]);
        yield return Mapping("seed-classify-animal", "Phân loại con vật", "con-vat", InteractionTypes.Classification, [("Cá", "Dưới nước"), ("Tôm", "Dưới nước"), ("Mèo", "Trên cạn"), ("Gà", "Trên cạn")]);
        yield return Mapping("seed-classify-weather", "Chọn đồ dùng theo thời tiết", "thoi-tiet", InteractionTypes.Classification, [("Áo mưa", "Trời mưa"), ("Ô", "Trời mưa"), ("Mũ rộng vành", "Trời nắng"), ("Kính râm", "Trời nắng")]);
    }

    private static IEnumerable<SeedLesson> BuildStoryLessons()
    {
        yield return Story("seed-story-wash", "Câu chuyện rửa tay", "tu-phuc-vu",
            "Trước khi ăn, Minh làm ướt tay, lấy xà phòng, chà sạch rồi lau khô.",
            "/images/lessons/story-wash-hands.png", "Minh làm gì trước khi ăn?", ["Rửa tay", "Đi ngủ", "Cất sách"], "Rửa tay");
        yield return Story("seed-story-crossing", "Qua đường an toàn", "an-toan",
            "Lan đứng trên vỉa hè cùng mẹ. Khi đèn dành cho người đi bộ bật màu xanh, hai mẹ con quan sát rồi đi trên vạch qua đường.",
            "/images/lessons/story-safe-crossing.png", "Khi nào Lan được qua đường?", ["Khi đèn người đi bộ màu xanh", "Khi xe đang chạy", "Khi đèn người đi bộ màu đỏ"], "Khi đèn người đi bộ màu xanh");
        yield return Story("seed-story-sharing", "Bạn bè biết chia sẻ", "cam-xuc",
            "Nam buồn vì quên hộp bút màu. Mai nhận ra điều đó và vui vẻ chia sẻ bút với Nam.",
            "/images/lessons/story-sharing.png", "Mai đã làm gì khi thấy Nam buồn?", ["Chia sẻ bút màu", "Cất hết bút đi", "Bỏ ra ngoài"], "Chia sẻ bút màu");
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

        for (var index = 0; index < 20; index++)
        {
            var letter = VietnameseAlphabet[index];
            var previous = VietnameseAlphabet[(index + VietnameseAlphabet.Length - 1) % VietnameseAlphabet.Length];
            var next = VietnameseAlphabet[(index + 1) % VietnameseAlphabet.Length];
            yield return Choice($"seed-letter-recognition-{index + 1:00}", $"Nhận biết chữ {letter}", "kham-pha-chu",
                InteractionTypes.SingleChoice, "Con nhìn mẫu rồi chọn đúng chữ cái.", $"Đâu là chữ {letter}?", [previous, letter, next], letter);
        }

        for (var number = 10; number <= 14; number++)
        {
            yield return Choice($"seed-recognize-number-{number}", $"Nhận biết số {number}", "so-10-20",
                InteractionTypes.SingleChoice, "Con quan sát rồi chọn đúng số.", $"Đâu là số {number}?",
                [(number - 1).ToString(), number.ToString(), (number + 1).ToString()], number.ToString());
        }

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

        var shapes = new[] { "Hình tròn", "Hình vuông", "Hình tam giác", "Hình chữ nhật", "Hình bầu dục", "Hình thoi" };
        foreach (var shape in shapes)
        {
            yield return Choice($"seed-shape-{NormalizeSeedCode(shape)}", $"Nhận biết {shape.ToLowerInvariant()}", "hinh-dang",
                InteractionTypes.SingleChoice, "Con quan sát đặc điểm rồi chọn đúng tên hình.", "Đây là hình gì?",
                new[] { shape, "Hình tròn", "Hình vuông" }.Distinct().Append("Hình tam giác").Take(3).ToArray(), shape);
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

        yield return Choice("seed-life-helmet", "Đội mũ bảo hiểm", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn hành động an toàn.", "Khi ngồi trên xe máy, con cần làm gì?", ["Đội mũ bảo hiểm", "Đứng lên", "Đùa nghịch"], "Đội mũ bảo hiểm");
        yield return Choice("seed-life-stranger", "Không đi theo người lạ", "an-toan", InteractionTypes.SingleChoice,
            "Con chọn cách xử lý an toàn.", "Người lạ rủ con đi theo, con làm gì?", ["Từ chối và gọi người thân", "Đi theo ngay", "Không nói với ai"], "Từ chối và gọi người thân");
        yield return Choice("seed-life-feeling", "Nói ra cảm xúc", "cam-xuc", InteractionTypes.SingleChoice,
            "Con chọn cách chia sẻ phù hợp.", "Khi buồn, con nên làm gì?", ["Nói với người con tin tưởng", "Đập đồ", "La hét vào bạn"], "Nói với người con tin tưởng");

        yield return Ordering("seed-fine-cut-paper", "Cắt giấy an toàn", "kheo-tay", ["Ngồi ngay ngắn", "Cầm kéo đúng tay", "Cắt theo đường", "Cất kéo"]);
        yield return Ordering("seed-fine-coloring", "Tô màu gọn gàng", "kheo-tay", ["Chọn màu", "Tô từ trong ra ngoài", "Tô kín hình", "Cất bút"]);
        yield return Drag("seed-fine-maze-4", "Đưa thỏ về vườn cà rốt", "me-cung", "Vườn cà rốt", ["Thỏ", "Cá", "Máy bay"], "Thỏ");
        yield return Drag("seed-fine-maze-5", "Đưa chim về tổ", "me-cung", "Tổ chim", ["Chim", "Xe buýt", "Quả bóng"], "Chim");

        yield return Choice("seed-shape-star", "Nhận biết hình ngôi sao", "hinh-dang", InteractionTypes.SingleChoice,
            "Con quan sát rồi chọn đúng tên hình.", "Hình có năm cánh là hình gì?", ["Hình ngôi sao", "Hình tròn", "Hình vuông"], "Hình ngôi sao");
        yield return Choice("seed-shape-heart", "Nhận biết hình trái tim", "hinh-dang", InteractionTypes.SingleChoice,
            "Con quan sát rồi chọn đúng tên hình.", "Đâu là tên của hình trái tim?", ["Hình trái tim", "Hình tam giác", "Hình chữ nhật"], "Hình trái tim");

        yield return Choice("seed-logic-different-1", "Tìm vật khác nhóm 1", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không phải trái cây?", ["Táo", "Cam", "Bút chì"], "Bút chì");
        yield return Choice("seed-logic-different-2", "Tìm vật khác nhóm 2", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không phải con vật?", ["Mèo", "Gà", "Cái bàn"], "Cái bàn");
        yield return Choice("seed-logic-different-3", "Tìm vật khác nhóm 3", "tim-khac-biet", InteractionTypes.SingleChoice,
            "Con tìm một vật không cùng nhóm.", "Vật nào không dùng để đi lại?", ["Xe đạp", "Ô tô", "Cái bát"], "Cái bát");
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
            "Con chọn hành động an toàn.", "Khi thấy ổ điện, con cần làm gì?", ["Không chạm vào", "Cho tay vào", "Đổ nước lên"], "Không chạm vào");
        yield return Choice("seed-life-sharing", "Biết chia sẻ đồ chơi", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn cách cư xử thân thiện.", "Bạn muốn chơi cùng, con nên làm gì?", ["Chia sẻ và chơi cùng", "Giấu đồ chơi", "Đẩy bạn ra"], "Chia sẻ và chơi cùng");
        yield return Choice("seed-life-apology", "Biết nói lời xin lỗi", "giao-tiep", InteractionTypes.SingleChoice,
            "Con chọn lời nói phù hợp.", "Khi vô ý làm bạn đau, con nên nói gì?", ["Mình xin lỗi bạn", "Không phải mình", "Bạn tự chịu"], "Mình xin lỗi bạn");

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
    }

    private static string NormalizeSeedCode(string value) => value
        .ToLowerInvariant()
        .Replace(' ', '-');

    private static SeedLesson Tracing(string code, string title, string topicCode, string symbol, int strokes) =>
        new(code, title, topicCode, InteractionTypes.Tracing, "Bé vẽ theo đường nét đứt nhé.", $"Bé hãy quan sát cách viết {symbol} nhé!", "{}", string.Empty, "Bắt đầu ở chấm màu cam, đi theo mũi tên và tô chậm trên nét đứt.", symbol, strokes);

    private static SeedLesson Choice(string code, string title, string topicCode, string type, string instruction, string prompt, string[] choices, string answer, string speechText = "", string imageUrl = "") =>
        Lesson(code, title, topicCode, type, instruction, prompt, new { choices, targetLabel = string.Empty, audioUrl = string.Empty, speechText, imageUrl }, answer);

    private static SeedLesson Multi(string code, string title, string topicCode, string[] choices, string[] answers) =>
        Lesson(code, title, topicCode, InteractionTypes.MultiSelect, "Con chọn tất cả đáp án đúng rồi bấm Kiểm tra.", "Những đáp án nào phù hợp?", new { choices, correctCount = answers.Length, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, string.Join('|', answers.OrderBy(x => x)));

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
