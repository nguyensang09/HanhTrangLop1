namespace HanhTrangLop1.Application.Lessons;

public static partial class Lexicon
{
    /// <summary>Từ vựng dùng chung cho nhiều nhóm bài. Nhóm nào cần thêm nhãn riêng thì thêm vào bảng của nhóm đó.</summary>
    private const string CoreTable = """
        // Con vật
        Con mèo | 🐱 | cat
        Con chó | 🐶 | dog
        Con cá | 🐟 | fish
        Con tôm | 🦐 | shrimp
        Con cua | 🦀 | crab
        Con thỏ | 🐰 | rabbit
        Con gà | 🐔 | chicken
        Gà con | 🐤 | chick
        Con vịt | 🦆 | duck
        Con chim | 🐦 | bird
        Con ong | 🐝 | bee
        Con bướm | 🦋 | butterfly
        Con voi | 🐘 | elephant
        Con hổ | 🐯 | tiger
        Con khỉ | 🐵 | monkey
        Con gấu | 🐻 | bear
        Con sóc | 🐿️ | squirrel
        Con rùa | 🐢 | turtle
        Con ếch | 🐸 | frog
        Con bò | 🐮 | cow
        Con lợn | 🐷 | pig
        Con ngựa | 🐴 | horse
        Con cừu | 🐑 | sheep
        Con hươu cao cổ | 🦒 | giraffe
        Con sư tử | 🦁 | lion
        Con cá heo | 🐬 | dolphin
        Cá voi | 🐳 | whale
        Bạch tuộc | 🐙 | octopus
        Con ốc sên | 🐌 | snail
        Con kiến | 🐜 | ant
        Bọ rùa | 🐞 | ladybug
        Con sâu | 🐛 | caterpillar
        Con chuột | 🐭 | mouse
        Con cú | 🦉 | owl
        Chim cánh cụt | 🐧 | penguin
        Con cá sấu | 🐊 | crocodile
        Con rắn | 🐍 | snake
        Khủng long | 🦕 | dinosaur
        // Quả, rau, đồ ăn
        Quả táo | 🍎 | apple
        Quả cam | 🍊 | orange
        Quả chuối | 🍌 | banana
        Quả dâu tây | 🍓 | strawberry
        Quả nho | 🍇 | grapes
        Quả dưa hấu | 🍉 | watermelon
        Quả dứa | 🍍 | pineapple
        Quả lê | 🍐 | pear
        Quả chanh | 🍋 | lemon
        Quả xoài | 🥭 | mango
        Quả dừa | 🥥 | coconut
        Quả đào | 🍑 | peach
        Cà rốt | 🥕 | carrot
        Bắp cải | 🥬 | cabbage
        Cà chua | 🍅 | tomato
        Bắp ngô | 🌽 | corn
        Củ khoai tây | 🥔 | potato
        Quả ớt | 🌶️ | chili pepper
        Cây nấm | 🍄 | mushroom
        Bánh mì | 🍞 | bread
        Bát cơm | 🍚 | bowl of rice
        Quả trứng | 🥚 | egg
        Cốc sữa | 🥛 | glass of milk
        Que kem | 🍦 | ice cream
        Cái bánh | 🍰 | cake
        Kẹo | 🍬 | candy
        Mật ong | 🍯 | honey
        Hạt dẻ | 🌰 | chestnut
        // Đồ vật
        Bút chì | ✏️ | pencil
        Bút màu | 🖍️ | crayon
        Quyển sách | 📕 | book
        Quyển vở | 📒 | notebook
        Cặp sách | 🎒 | school bag
        Cái kéo | ✂️ | scissors
        Thước kẻ | 📏 | ruler
        Quả bóng | ⚽ | ball
        Bóng bay | 🎈 | balloon
        Con diều | 🪁 | kite
        Gấu bông | 🧸 | teddy bear
        Hộp quà | 🎁 | gift box
        Cái ô | ☂️ | umbrella
        Áo mưa | 🧥 | raincoat
        Cái cốc | 🥤 | cup
        Cái thìa | 🥄 | spoon
        Cái bát | 🥣 | bowl
        Cái đĩa | 🍽️ | plate
        Cái nồi | 🍲 | pot
        Bàn chải đánh răng | 🪥 | toothbrush
        Xà phòng | 🧼 | soap
        Khăn mặt | 🧻 | towel
        Cái gối | 🛏️ | pillow
        Đồng hồ | ⏰ | clock
        Cái đèn | 💡 | lamp
        Điện thoại | 📱 | phone
        Cái chìa khóa | 🔑 | key
        Cái ghế | 🪑 | chair
        Cái kèn | 🎺 | trumpet
        Cái trống | 🥁 | drum
        Đàn ghi-ta | 🎸 | guitar
        // Quần áo
        Cái áo | 👕 | shirt
        Cái quần | 👖 | trousers
        Cái váy | 👗 | dress
        Áo ấm | 🧥 | warm coat
        Khăn quàng | 🧣 | scarf
        Găng tay | 🧤 | gloves
        Đôi tất | 🧦 | socks
        Đôi giày | 👟 | shoes
        Đôi ủng | 👢 | boots
        Mũ rộng vành | 👒 | sun hat
        Mũ len | 🧢 | cap
        Kính râm | 🕶️ | sunglasses
        Mũ bảo hiểm | ⛑️ | helmet
        // Phương tiện
        Xe đạp | 🚲 | bicycle
        Ô tô | 🚗 | car
        Xe buýt | 🚌 | bus
        Xe máy | 🛵 | motorbike
        Tàu hỏa | 🚆 | train
        Máy bay | ✈️ | airplane
        Thuyền buồm | ⛵ | sailboat
        Tàu thủy | 🚢 | ship
        Xe cứu hỏa | 🚒 | fire truck
        Xe cứu thương | 🚑 | ambulance
        Tên lửa | 🚀 | rocket
        Trực thăng | 🚁 | helicopter
        // Thiên nhiên
        Mặt trời | ☀️ | sun
        Mặt trăng | 🌙 | moon
        Ngôi sao | ⭐ | star
        Đám mây | ☁️ | cloud
        Cầu vồng | 🌈 | rainbow
        Bông hoa | 🌸 | flower
        Hoa hướng dương | 🌻 | sunflower
        Cái cây | 🌳 | tree
        Chiếc lá | 🍃 | leaf
        Hạt giống | 🫘 | seed
        Mầm cây | 🌱 | sprout
        Ngôi nhà | 🏠 | house
        Trường học | 🏫 | school
        // Nhãn của dạng so sánh
        Nhóm bên trái | 👈 | the left group
        Nhóm bên phải | 👉 | the right group
        Bên trái | 👈 | the left one
        Bên phải | 👉 | the right one
        Bằng nhau | ⚖️ | the same
        """;

    /// <summary>Tiếng mở đầu cho bài "âm đầu" của nhóm Chữ cái (nhãn hiển thị là chữ thường của từ).</summary>
    private const string LetterWordsTable = "";
}
