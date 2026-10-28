namespace HanhTrangLop1.Infrastructure;

/// <summary>
/// Lời phản hồi chuẩn sau khi chấm bài. Lời khen luôn ngắn gọn và giống nhau ở mọi bài để bé nghe xong là sang câu
/// tiếp; lời nhắc khi chưa đúng lấy theo từng câu hỏi (có hướng dẫn riêng), thiếu thì dùng câu nhắc chuẩn.
/// Chữ hiển thị, kết quả chấm và Voice phải dùng cùng các câu này để luôn khớp nhau.
/// </summary>
public static class LearningFeedback
{
    public const string CorrectVi = "Chính xác!";
    public const string CorrectEn = "Correct!";
    public const string RetryVi = "Con thử lại nhé";
    public const string RetryEn = "Let's try again.";
}
