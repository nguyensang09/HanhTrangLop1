using System.Globalization;
using System.Security;
using System.Text;
using HanhTrangLop1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HanhTrangLop1.Controllers;

[AllowAnonymous]
public sealed class LearningMediaController : Controller
{
    private static readonly CultureInfo VietnameseCulture = new("vi-VN");

    [HttpGet("learning-media/letter/{symbol}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult LetterCard(string symbol)
    {
        var clean = (symbol ?? string.Empty).Trim();
        var upper = clean.ToUpper(VietnameseCulture);
        if (!LearningContentSeed.VietnameseAlphabet.Contains(upper, StringComparer.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var lower = upper.ToLower(VietnameseCulture);
        var safeUpper = SecurityElement.Escape(upper) ?? string.Empty;
        var safeLower = SecurityElement.Escape(lower) ?? string.Empty;
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 520" role="img" aria-labelledby="title description">
              <title id="title">Chữ {{safeUpper}} in hoa và {{safeLower}} in thường</title>
              <desc id="description">Thẻ học chữ cái tiếng Việt, không kèm từ tiếng nước ngoài.</desc>
              <defs>
                <linearGradient id="background" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="#fff9e8"/>
                  <stop offset="1" stop-color="#e9f9ff"/>
                </linearGradient>
              </defs>
              <rect x="12" y="12" width="776" height="496" rx="42" fill="url(#background)" stroke="#35bdf4" stroke-width="8"/>
              <text x="400" y="90" text-anchor="middle" font-family="Arial, 'Be Vietnam Pro', sans-serif" font-size="34" font-weight="700" fill="#39506b">CHỮ CÁI TIẾNG VIỆT</text>
              <text x="400" y="350" text-anchor="middle" font-family="Arial, 'Be Vietnam Pro', sans-serif" font-size="250" font-weight="800" fill="#ff7a3d">{{safeUpper}} {{safeLower}}</text>
              <circle cx="105" cy="430" r="18" fill="#ffd45a"/>
              <circle cx="155" cy="430" r="18" fill="#46e6b3"/>
              <circle cx="645" cy="430" r="18" fill="#c48af2"/>
              <circle cx="695" cy="430" r="18" fill="#ef6ea8"/>
            </svg>
            """;

        return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml; charset=utf-8");
    }

    [HttpGet("learning-media/topic/{topicCode}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult TopicCard(string topicCode)
    {
        var group = CurriculumCatalog.Groups.FirstOrDefault(candidate =>
            candidate.Topics.Any(topic => string.Equals(topic.Code, topicCode, StringComparison.OrdinalIgnoreCase)));
        var topic = group?.Topics.FirstOrDefault(candidate =>
            string.Equals(candidate.Code, topicCode, StringComparison.OrdinalIgnoreCase));
        if (group is null || topic is null) return NotFound();

        var safeGroup = SecurityElement.Escape(group.Name) ?? string.Empty;
        var safeTopic = SecurityElement.Escape(topic.Name) ?? string.Empty;
        var safeIcon = SecurityElement.Escape(GetTopicSymbol(topicCode)) ?? "★";
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 520" role="img" aria-labelledby="title description">
              <title id="title">{{safeTopic}}</title>
              <desc id="description">Minh họa trung tính cho chủ đề {{safeGroup}}.</desc>
              <defs><linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#fff9e8"/><stop offset="1" stop-color="#e9f9ff"/></linearGradient></defs>
              <rect x="12" y="12" width="776" height="496" rx="42" fill="url(#bg)" stroke="#46cfa1" stroke-width="8"/>
              <text x="400" y="92" text-anchor="middle" font-family="Arial, 'Be Vietnam Pro', sans-serif" font-size="30" font-weight="700" fill="#52657a">{{safeGroup}}</text>
              <text x="400" y="300" text-anchor="middle" font-family="'Segoe UI Emoji', Arial, sans-serif" font-size="150">{{safeIcon}}</text>
              <text x="400" y="420" text-anchor="middle" font-family="Arial, 'Be Vietnam Pro', sans-serif" font-size="48" font-weight="800" fill="#174d46">{{safeTopic}}</text>
            </svg>
            """;
        return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml; charset=utf-8");
    }

    [HttpGet("learning-media/tracing")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public IActionResult TracingCard([FromQuery] string symbol)
    {
        var clean = (symbol ?? string.Empty).Trim();
        if (clean.Length is < 1 or > 40) return NotFound();

        var display = GetTracingDisplay(clean);
        var safeDisplay = SecurityElement.Escape(display) ?? string.Empty;
        var safeLabel = SecurityElement.Escape(ToDisplayLabel(clean)) ?? string.Empty;
        var svg = $$"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 520" role="img" aria-labelledby="title description">
              <title id="title">Tô theo nét {{safeLabel}}</title>
              <desc id="description">Hình mẫu nét đứt để quan sát trước khi tô.</desc>
              <rect x="12" y="12" width="776" height="496" rx="42" fill="#fffdf7" stroke="#ffad72" stroke-width="8"/>
              <text x="400" y="90" text-anchor="middle" font-family="Arial, 'Be Vietnam Pro', sans-serif" font-size="32" font-weight="700" fill="#52657a">TÔ THEO NÉT</text>
              <text x="400" y="350" text-anchor="middle" font-family="'Segoe UI Emoji', Arial, sans-serif" font-size="230" fill="#fff" stroke="#ff7a3d" stroke-width="5" stroke-dasharray="14 10">{{safeDisplay}}</text>
              <text x="400" y="455" text-anchor="middle" font-family="Arial, 'Be Vietnam Pro', sans-serif" font-size="34" font-weight="700" fill="#7b4b36">{{safeLabel}}</text>
            </svg>
            """;
        return File(Encoding.UTF8.GetBytes(svg), "image/svg+xml; charset=utf-8");
    }

    private static string GetTopicSymbol(string topicCode) => topicCode switch
    {
        "an-toan" => "🛡️", "cam-xuc" => "😊", "giao-tiep" => "💬", "tu-phuc-vu" => "🧼",
        "con-vat" => "🐾", "cay-co" => "🌱", "thoi-tiet" => "🌦️", "giao-thong" => "🚌",
        "hinh-dang" or "ghep-hinh" => "🔷", "vi-tri" => "🧭", "kich-thuoc" => "↔️",
        "ke-chuyen" or "doc-hieu" => "📖", "nghe-hieu" or "am-van" => "👂", "von-tu" => "🗣️",
        "ghi-nho" or "tap-trung" => "🧠", "lam-theo-yeu-cau" => "✅",
        "net-co-ban" or "tao-hinh" or "noi-diem" or "kheo-tay" => "✏️", "me-cung" => "🧩",
        _ => "⭐"
    };

    private static string GetTracingDisplay(string symbol) => symbol.ToLowerInvariant() switch
    {
        "ca-heo" => "🐬", "cau-vong" => "🌈", "chu-buom" => "🦋", "chu-tho" => "🐇",
        "do-dung" => "🎒", "hinh-hoc" => "⭐", "khung-long" => "🦕", "may-bay" => "✈️",
        "meo-con" => "🐈", "o-che-mua" => "☂️", "ong-vang" => "🐝", "phong-canh" => "🏡",
        "tau-hoa" => "🚂", "ten-lua" => "🚀", "thuyen" => "⛵", "trai-tao" => "🍎",
        _ => symbol
    };

    private static string ToDisplayLabel(string symbol) =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(symbol.Replace('-', ' '));
}
