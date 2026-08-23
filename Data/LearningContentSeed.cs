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
            ["Táo"] = "/images/photos/apple.jpg",
            ["Cam"] = "/images/photos/orange.jpg",
            ["Cà rốt"] = "/images/photos/carrot.jpg",
            ["Bắp cải"] = "/images/photos/cabbage.jpg",
            ["Mèo"] = "/images/photos/cat.jpg",
            ["Chó"] = "/images/photos/dog.jpg",
            ["Vịt"] = "/images/photos/duck.jpg",
            ["Cá"] = "/images/photos/fish.jpg",
            ["Tôm"] = "/images/photos/shrimp.jpg",
            ["Gà"] = "/images/photos/chicken.jpg"
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
                payloadJson = JsonSerializer.Serialize(new
                {
                    symbol = definition.Symbol,
                    templateId,
                    guideMode = "outline",
                    expectedStrokeCount = definition.ExpectedStrokeCount,
                    showStartPoint = false,
                    audioUrl = string.Empty
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
                        showStartPoint = false
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
            ("apple.jpg", "/images/photos/apple.jpg", "Một quả táo đỏ chụp rõ trên nền sáng"),
            ("dog.jpg", "/images/photos/dog.jpg", "Chó cứu hộ màu đen nhìn nghiêng"),
            ("fish.jpg", "/images/photos/fish.jpg", "Cá vàng màu cam trong bể nước"),
            ("orange.jpg", "/images/photos/orange.jpg", "Những quả cam chín màu vàng cam"),
            ("carrot.jpg", "/images/photos/carrot.jpg", "Các củ cà rốt tươi còn nguyên lá"),
            ("cabbage.jpg", "/images/photos/cabbage.jpg", "Một cây bắp cải nhìn rõ các lớp lá"),
            ("cat.jpg", "/images/photos/cat.jpg", "Mèo màu vàng nhìn thẳng"),
            ("duck.jpg", "/images/photos/duck.jpg", "Vịt đang đi trên mặt đất"),
            ("shrimp.jpg", "/images/photos/shrimp.jpg", "Tôm nhỏ màu đỏ trên lá xanh"),
            ("chicken.jpg", "/images/photos/chicken.jpg", "Gà mái nhìn nghiêng rõ đầu và thân")
        };
        var existingPaths = await db.MediaAssets.AsNoTracking()
            .Where(x => x.AssetType == "image")
            .Select(x => x.StoragePath)
            .ToHashSetAsync();

        foreach (var image in images.Where(x => !existingPaths.Contains(x.Item2)))
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
        new(code, title, topicCode, InteractionTypes.Tracing, "Con tô theo đường viền từ điểm bắt đầu.", $"Con hãy tô ký tự {symbol}.", "{}", string.Empty, "Bắt đầu ở điểm màu cam và tô chậm theo đường viền.", symbol, strokes);

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
        Lesson(code, title, topicCode, InteractionTypes.Counting, "Con chạm từng đồ vật để đếm rồi chọn số đúng.", "Có bao nhiêu đồ vật?", new { choices = new[] { Math.Max(0, count - 1).ToString(), count.ToString(), (count + 1).ToString() }, objectSymbol = symbol, targetCount = count, imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, count.ToString());

    private static SeedLesson Quantity(string code, string title, string topicCode, string symbol, int count) =>
        Lesson(code, title, topicCode, InteractionTypes.QuantityBuilder, "Con thêm hoặc bớt đồ vật để tạo đúng số lượng.", $"Hãy tạo {count} đồ vật.", new { objectSymbol = symbol, targetCount = count, maxItems = count + 3, targetLabel = "Số lượng đã tạo", imageUrl = string.Empty, audioUrl = string.Empty, speechText = string.Empty }, count.ToString());

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
