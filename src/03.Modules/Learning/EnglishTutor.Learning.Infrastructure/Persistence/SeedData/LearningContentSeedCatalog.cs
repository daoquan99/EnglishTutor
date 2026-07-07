namespace EnglishTutor.Learning.Infrastructure.Persistence.SeedData;

public static class LearningContentSeedCatalog
{
    public static IReadOnlyList<TopicSeed> Topics { get; } =
    [
        new("daily-life", "Daily Life", "Luyện giao tiếp trong các hoạt động thường ngày."),
        new("travel", "Travel", "Giao tiếp khi lên kế hoạch và xử lý tình huống du lịch."),
        new("food-and-dining", "Food and Dining", "Từ vựng và hội thoại về món ăn, nhà hàng và nấu nướng."),
        new("work-and-career", "Work and Career", "Giao tiếp chuyên nghiệp, phỏng vấn và phát triển nghề nghiệp."),
        new("technology", "Technology", "Trao đổi về thiết bị, phần mềm và đời sống số."),
        new("health-and-fitness", "Health and Fitness", "Mô tả sức khỏe, thói quen vận động và chăm sóc bản thân."),
        new("shopping", "Shopping", "Hỏi giá, so sánh sản phẩm, thanh toán và đổi trả."),
        new("education", "Education", "Học tập, lớp học, mục tiêu và phương pháp học."),
        new("relationships", "Relationships", "Bạn bè, gia đình, cảm xúc và giao tiếp xã hội."),
        new("entertainment", "Entertainment", "Phim ảnh, âm nhạc, sách và hoạt động giải trí.")
    ];

    public static IReadOnlyList<ModeSeed> Modes { get; } =
    [
        new("free-talk", "Free Talk", "Trò chuyện tự do theo chủ đề với phản hồi thích ứng."),
        new("role-play", "Role-play", "Đóng vai trong một tình huống giao tiếp thực tế."),
        new("shadowing", "Shadowing", "Nghe và lặp lại để cải thiện phát âm và nhịp điệu."),
        new("translate-coach", "Translate Coach", "Dịch ý tiếng Việt sang tiếng Anh với gợi ý từng bước."),
        new("quick-response", "Quick Response", "Phản xạ trả lời ngắn trong thời gian giới hạn."),
        new("listening-challenge", "Listening Challenge", "Nghe hiểu thông tin chính và chi tiết."),
        new("correction-drill", "Correction Drill", "Nhận diện và sửa lỗi trong câu tiếng Anh.")
    ];

    public static IReadOnlyList<TopicModeSeed> TopicModes { get; } = CreateTopicModes();

    public static IReadOnlyList<ScenarioSeed> Scenarios { get; } =
    [
        S("daily-life", "free-talk", "Morning routine", "Describe and compare morning routines.", "Beginner", "Have a friendly English conversation about the learner's morning routine. Ask one clear question at a time and explain corrections briefly in Vietnamese."),
        S("daily-life", "quick-response", "Making weekend plans", "Respond quickly to questions about weekend activities.", "Intermediate", "Give short English prompts about weekend plans. Require a natural answer within one or two sentences, then provide concise Vietnamese-supported feedback."),
        S("travel", "role-play", "Checking in at a hotel", "Practice a complete hotel check-in conversation.", "Beginner", "Act as a hotel receptionist. Help the Vietnamese learner check in, confirm the reservation, ask about breakfast, and request room information in English."),
        S("travel", "listening-challenge", "Airport gate announcement", "Understand a gate change and boarding instructions.", "Intermediate", "Present a short airport announcement in natural English, then ask comprehension questions about flight number, gate, time, and required action."),
        S("food-and-dining", "role-play", "Ordering at a restaurant", "Order a meal and handle dietary preferences.", "Beginner", "Act as a restaurant server. Guide the learner through ordering food, asking about ingredients, and requesting the bill in English."),
        S("food-and-dining", "translate-coach", "Explaining a Vietnamese dish", "Explain ingredients and flavor in English.", "Intermediate", "Give Vietnamese ideas about a familiar dish one at a time. Coach the learner to express each idea naturally in English without translating mechanically."),
        S("work-and-career", "role-play", "Job interview", "Answer common interview questions with evidence.", "Advanced", "Act as an interviewer for a professional role. Ask behavioral questions and coach the learner to give concise STAR-style answers in English."),
        S("work-and-career", "correction-drill", "Professional email review", "Correct tone, grammar, and clarity in workplace messages.", "Intermediate", "Show one flawed workplace sentence at a time. Ask the learner to correct it, then explain the best professional wording in Vietnamese."),
        S("technology", "free-talk", "Technology in daily life", "Discuss useful and distracting technology habits.", "Intermediate", "Discuss how technology helps and distracts the learner. Ask follow-up questions that elicit reasons, examples, and comparisons."),
        S("technology", "quick-response", "Technical support", "Respond to common device and software problems.", "Intermediate", "Present brief technical-support situations. Ask the learner to describe the problem and request help in clear English."),
        S("health-and-fitness", "role-play", "Talking to a doctor", "Describe symptoms and understand basic advice.", "Intermediate", "Act as a clinician in a non-emergency consultation. Help the learner describe symptoms, duration, and severity in English. Do not provide diagnosis or medical treatment."),
        S("health-and-fitness", "free-talk", "Building healthy habits", "Discuss realistic sleep, food, and exercise habits.", "Beginner", "Have a supportive English conversation about healthy routines. Focus on frequency expressions, goals, and simple reasons."),
        S("shopping", "role-play", "Returning a product", "Explain a product problem and request a solution.", "Intermediate", "Act as a store assistant. Let the learner explain a faulty product, provide purchase details, and request a refund or exchange."),
        S("shopping", "quick-response", "Comparing products", "Compare price, quality, size, and features.", "Beginner", "Present pairs of everyday products and ask the learner to make quick comparative sentences in English."),
        S("education", "free-talk", "Effective learning methods", "Discuss study habits and evaluate their effectiveness.", "Intermediate", "Discuss the learner's study methods in English. Ask for examples and help them use language for cause, effect, and preference."),
        S("education", "translate-coach", "Asking for clarification", "Turn Vietnamese classroom needs into polite English.", "Beginner", "Provide Vietnamese classroom situations and coach the learner to ask for repetition, examples, deadlines, or clarification politely in English."),
        S("relationships", "role-play", "Resolving a misunderstanding", "Clarify intent, listen, and apologize appropriately.", "Advanced", "Act as a friend resolving a misunderstanding. Encourage calm clarification, active listening, and a sincere English apology."),
        S("relationships", "quick-response", "Keeping a conversation going", "Use follow-up questions and supportive responses.", "Beginner", "Give short social statements and ask the learner to respond naturally with interest and one relevant follow-up question."),
        S("entertainment", "free-talk", "Recommending a movie", "Describe and recommend a movie without spoilers.", "Intermediate", "Ask the learner to recommend a movie in English, covering genre, premise, strengths, and intended audience without major spoilers."),
        S("entertainment", "listening-challenge", "Podcast review", "Identify opinion, supporting reasons, and recommendation.", "Advanced", "Present a short spoken-style review in English, then ask questions about the speaker's opinion, evidence, and final recommendation.")
    ];

    public static IReadOnlyList<VocabularySeed> Vocabulary { get; } = CreateVocabulary();

    public static IReadOnlyList<PhraseSeed> Phrases { get; } = CreatePhrases();

    private static ScenarioSeed S(string topic, string mode, string name, string description, string level, string prompt) =>
        new(topic, mode, name, description, level, prompt);

    private static IReadOnlyList<TopicModeSeed> CreateTopicModes()
    {
        var map = new Dictionary<string, string[]>
        {
            ["daily-life"] = ["free-talk", "quick-response", "shadowing", "correction-drill"],
            ["travel"] = ["role-play", "listening-challenge", "quick-response", "translate-coach"],
            ["food-and-dining"] = ["role-play", "translate-coach", "shadowing", "quick-response"],
            ["work-and-career"] = ["role-play", "correction-drill", "free-talk", "listening-challenge"],
            ["technology"] = ["free-talk", "quick-response", "translate-coach", "correction-drill"],
            ["health-and-fitness"] = ["role-play", "free-talk", "listening-challenge", "translate-coach"],
            ["shopping"] = ["role-play", "quick-response", "listening-challenge", "shadowing"],
            ["education"] = ["free-talk", "translate-coach", "correction-drill", "listening-challenge"],
            ["relationships"] = ["role-play", "quick-response", "free-talk", "shadowing"],
            ["entertainment"] = ["free-talk", "listening-challenge", "shadowing", "quick-response"]
        };

        return map.SelectMany(pair => pair.Value.Select(mode => new TopicModeSeed(pair.Key, mode))).ToArray();
    }

    private static IReadOnlyList<VocabularySeed> CreateVocabulary() =>
    [
        V("daily-life", "routine", "thói quen hằng ngày", "noun", "/ruːˈtiːn/", "My morning routine starts at six.", "Thói quen buổi sáng của tôi bắt đầu lúc sáu giờ."),
        V("daily-life", "commute", "đi lại giữa nhà và nơi làm việc", "verb", "/kəˈmjuːt/", "I commute by bus every weekday.", "Tôi đi làm bằng xe buýt mỗi ngày trong tuần."),
        V("daily-life", "chore", "việc nhà", "noun", "/tʃɔːr/", "Doing the laundry is my least favorite chore.", "Giặt quần áo là việc nhà tôi ít thích nhất."),
        V("daily-life", "schedule", "lịch trình", "noun", "/ˈskedʒuːl/", "My schedule is flexible this afternoon.", "Lịch trình chiều nay của tôi khá linh hoạt."),
        V("daily-life", "prepare", "chuẩn bị", "verb", "/prɪˈper/", "I prepare lunch the night before.", "Tôi chuẩn bị bữa trưa từ tối hôm trước."),
        V("daily-life", "usually", "thường xuyên", "adverb", "/ˈjuːʒuəli/", "I usually read before bed.", "Tôi thường đọc sách trước khi ngủ."),
        V("daily-life", "errand", "việc vặt cần ra ngoài giải quyết", "noun", "/ˈerənd/", "I need to run a few errands after work.", "Tôi cần giải quyết vài việc vặt sau giờ làm."),
        V("daily-life", "tidy", "dọn dẹp gọn gàng", "verb", "/ˈtaɪdi/", "Please tidy your desk before dinner.", "Hãy dọn bàn học trước bữa tối."),
        V("daily-life", "appointment", "cuộc hẹn", "noun", "/əˈpɔɪntmənt/", "I have a dentist appointment at three.", "Tôi có lịch hẹn nha sĩ lúc ba giờ."),
        V("daily-life", "relax", "thư giãn", "verb", "/rɪˈlæks/", "Music helps me relax in the evening.", "Âm nhạc giúp tôi thư giãn vào buổi tối."),

        V("travel", "destination", "điểm đến", "noun", "/ˌdestɪˈneɪʃn/", "Da Nang is our final destination.", "Đà Nẵng là điểm đến cuối cùng của chúng tôi."),
        V("travel", "itinerary", "lịch trình chuyến đi", "noun", "/aɪˈtɪnəreri/", "The itinerary includes three cities.", "Lịch trình bao gồm ba thành phố."),
        V("travel", "reservation", "đặt chỗ", "noun", "/ˌrezərˈveɪʃn/", "I made a reservation for two nights.", "Tôi đã đặt phòng trong hai đêm."),
        V("travel", "departure", "sự khởi hành", "noun", "/dɪˈpɑːrtʃər/", "Our departure is delayed by an hour.", "Chuyến khởi hành của chúng tôi bị hoãn một giờ."),
        V("travel", "luggage", "hành lý", "noun", "/ˈlʌɡɪdʒ/", "Can I leave my luggage here?", "Tôi có thể để hành lý ở đây không?"),
        V("travel", "boarding pass", "thẻ lên máy bay", "noun", "/ˈbɔːrdɪŋ pæs/", "Please show your boarding pass at the gate.", "Vui lòng xuất trình thẻ lên máy bay tại cửa."),
        V("travel", "accommodation", "chỗ ở", "noun", "/əˌkɑːməˈdeɪʃn/", "We found affordable accommodation downtown.", "Chúng tôi tìm được chỗ ở vừa túi tiền ở trung tâm."),
        V("travel", "sightseeing", "tham quan", "noun", "/ˈsaɪtsiːɪŋ/", "We spent the morning sightseeing.", "Chúng tôi dành buổi sáng để tham quan."),
        V("travel", "delay", "sự chậm trễ", "noun", "/dɪˈleɪ/", "The weather caused a long delay.", "Thời tiết gây ra sự chậm trễ kéo dài."),
        V("travel", "souvenir", "quà lưu niệm", "noun", "/ˌsuːvəˈnɪr/", "I bought a small souvenir for my sister.", "Tôi mua một món quà lưu niệm nhỏ cho em gái."),

        V("food-and-dining", "ingredient", "nguyên liệu", "noun", "/ɪnˈɡriːdiənt/", "Fresh herbs are the key ingredient.", "Rau thơm tươi là nguyên liệu chính."),
        V("food-and-dining", "appetizer", "món khai vị", "noun", "/ˈæpɪtaɪzər/", "We shared an appetizer before dinner.", "Chúng tôi dùng chung món khai vị trước bữa tối."),
        V("food-and-dining", "portion", "khẩu phần", "noun", "/ˈpɔːrʃn/", "The portions here are generous.", "Khẩu phần ở đây rất đầy đặn."),
        V("food-and-dining", "spicy", "cay", "adjective", "/ˈspaɪsi/", "Is this soup very spicy?", "Món súp này có cay lắm không?"),
        V("food-and-dining", "savory", "có vị mặn đậm đà", "adjective", "/ˈseɪvəri/", "The sauce has a rich, savory flavor.", "Nước sốt có vị mặn đậm đà."),
        V("food-and-dining", "recommend", "gợi ý", "verb", "/ˌrekəˈmend/", "What dish would you recommend?", "Bạn gợi ý món nào?"),
        V("food-and-dining", "allergy", "dị ứng", "noun", "/ˈælərdʒi/", "I have a serious peanut allergy.", "Tôi bị dị ứng đậu phộng nghiêm trọng."),
        V("food-and-dining", "receipt", "hóa đơn", "noun", "/rɪˈsiːt/", "Could I have the receipt, please?", "Cho tôi xin hóa đơn được không?"),
        V("food-and-dining", "grilled", "được nướng trên vỉ", "adjective", "/ɡrɪld/", "I ordered grilled fish with vegetables.", "Tôi gọi cá nướng kèm rau."),
        V("food-and-dining", "delicious", "ngon", "adjective", "/dɪˈlɪʃəs/", "The noodle soup was delicious.", "Món mì nước rất ngon."),

        V("work-and-career", "deadline", "hạn chót", "noun", "/ˈdedlaɪn/", "We need to meet Friday's deadline.", "Chúng ta cần kịp hạn chót thứ Sáu."),
        V("work-and-career", "colleague", "đồng nghiệp", "noun", "/ˈkɑːliːɡ/", "A colleague helped me solve the issue.", "Một đồng nghiệp đã giúp tôi giải quyết vấn đề."),
        V("work-and-career", "responsibility", "trách nhiệm", "noun", "/rɪˌspɑːnsəˈbɪləti/", "Managing the budget is my responsibility.", "Quản lý ngân sách là trách nhiệm của tôi."),
        V("work-and-career", "promotion", "sự thăng chức", "noun", "/prəˈmoʊʃn/", "She earned a promotion last month.", "Cô ấy được thăng chức tháng trước."),
        V("work-and-career", "qualification", "trình độ hoặc bằng cấp", "noun", "/ˌkwɑːlɪfɪˈkeɪʃn/", "This role requires a technical qualification.", "Vị trí này yêu cầu trình độ kỹ thuật."),
        V("work-and-career", "interview", "phỏng vấn", "noun", "/ˈɪntərvjuː/", "I have a job interview tomorrow.", "Ngày mai tôi có một buổi phỏng vấn việc làm."),
        V("work-and-career", "negotiate", "đàm phán", "verb", "/nɪˈɡoʊʃieɪt/", "We need to negotiate the project scope.", "Chúng ta cần đàm phán phạm vi dự án."),
        V("work-and-career", "feedback", "phản hồi", "noun", "/ˈfiːdbæk/", "Constructive feedback helps me improve.", "Phản hồi mang tính xây dựng giúp tôi tiến bộ."),
        V("work-and-career", "achievement", "thành tựu", "noun", "/əˈtʃiːvmənt/", "Launching the product was a major achievement.", "Ra mắt sản phẩm là một thành tựu lớn."),
        V("work-and-career", "remote", "từ xa", "adjective", "/rɪˈmoʊt/", "Our team supports remote work.", "Đội của chúng tôi hỗ trợ làm việc từ xa."),

        V("technology", "device", "thiết bị", "noun", "/dɪˈvaɪs/", "This device connects to Wi-Fi automatically.", "Thiết bị này tự động kết nối Wi-Fi."),
        V("technology", "software", "phần mềm", "noun", "/ˈsɔːftwer/", "The software needs an update.", "Phần mềm cần được cập nhật."),
        V("technology", "privacy", "quyền riêng tư", "noun", "/ˈpraɪvəsi/", "Check the app's privacy settings.", "Hãy kiểm tra cài đặt quyền riêng tư của ứng dụng."),
        V("technology", "password", "mật khẩu", "noun", "/ˈpæswɜːrd/", "Use a unique password for each account.", "Hãy dùng mật khẩu riêng cho mỗi tài khoản."),
        V("technology", "download", "tải xuống", "verb", "/ˌdaʊnˈloʊd/", "You can download the file here.", "Bạn có thể tải tệp xuống tại đây."),
        V("technology", "wireless", "không dây", "adjective", "/ˈwaɪərləs/", "These wireless headphones last all day.", "Tai nghe không dây này dùng được cả ngày."),
        V("technology", "battery", "pin", "noun", "/ˈbætəri/", "My phone battery is almost empty.", "Pin điện thoại của tôi gần hết."),
        V("technology", "connection", "kết nối", "noun", "/kəˈnekʃn/", "The internet connection is unstable.", "Kết nối internet không ổn định."),
        V("technology", "feature", "tính năng", "noun", "/ˈfiːtʃər/", "The new feature saves a lot of time.", "Tính năng mới tiết kiệm rất nhiều thời gian."),
        V("technology", "troubleshoot", "tìm và xử lý sự cố", "verb", "/ˈtrʌblʃuːt/", "The guide helps users troubleshoot common errors.", "Hướng dẫn giúp người dùng xử lý các lỗi phổ biến."),

        V("health-and-fitness", "symptom", "triệu chứng", "noun", "/ˈsɪmptəm/", "When did the symptom first appear?", "Triệu chứng xuất hiện lần đầu khi nào?"),
        V("health-and-fitness", "exercise", "tập thể dục", "noun", "/ˈeksərsaɪz/", "Regular exercise improves my mood.", "Tập thể dục thường xuyên cải thiện tâm trạng của tôi."),
        V("health-and-fitness", "balanced", "cân bằng", "adjective", "/ˈbælənst/", "A balanced diet includes many food groups.", "Chế độ ăn cân bằng bao gồm nhiều nhóm thực phẩm."),
        V("health-and-fitness", "recover", "hồi phục", "verb", "/rɪˈkʌvər/", "It took a week to recover from the flu.", "Tôi mất một tuần để hồi phục sau cúm."),
        V("health-and-fitness", "strength", "sức mạnh", "noun", "/streŋθ/", "This exercise builds core strength.", "Bài tập này tăng sức mạnh vùng trung tâm."),
        V("health-and-fitness", "stretch", "kéo giãn", "verb", "/stretʃ/", "Remember to stretch after running.", "Hãy nhớ kéo giãn sau khi chạy."),
        V("health-and-fitness", "hydrated", "đủ nước", "adjective", "/ˈhaɪdreɪtɪd/", "Drink water to stay hydrated.", "Hãy uống nước để cơ thể đủ nước."),
        V("health-and-fitness", "stamina", "sức bền", "noun", "/ˈstæmɪnə/", "Cycling can improve your stamina.", "Đạp xe có thể cải thiện sức bền."),
        V("health-and-fitness", "sore", "đau nhức", "adjective", "/sɔːr/", "My legs feel sore after the workout.", "Chân tôi đau nhức sau buổi tập."),
        V("health-and-fitness", "well-being", "sức khỏe toàn diện", "noun", "/ˌwel ˈbiːɪŋ/", "Sleep is essential for well-being.", "Giấc ngủ rất cần thiết cho sức khỏe toàn diện."),

        V("shopping", "discount", "giảm giá", "noun", "/ˈdɪskaʊnt/", "This jacket has a twenty percent discount.", "Chiếc áo khoác này được giảm hai mươi phần trăm."),
        V("shopping", "refund", "hoàn tiền", "noun", "/ˈriːfʌnd/", "I would like to request a refund.", "Tôi muốn yêu cầu hoàn tiền."),
        V("shopping", "exchange", "đổi hàng", "verb", "/ɪksˈtʃeɪndʒ/", "Can I exchange this for a larger size?", "Tôi có thể đổi sang cỡ lớn hơn không?"),
        V("shopping", "affordable", "có giá phải chăng", "adjective", "/əˈfɔːrdəbl/", "We are looking for an affordable laptop.", "Chúng tôi đang tìm một chiếc laptop giá phải chăng."),
        V("shopping", "quality", "chất lượng", "noun", "/ˈkwɑːləti/", "The quality is better than I expected.", "Chất lượng tốt hơn tôi mong đợi."),
        V("shopping", "cashier", "nhân viên thu ngân", "noun", "/kæˈʃɪr/", "The cashier gave me a receipt.", "Nhân viên thu ngân đưa tôi hóa đơn."),
        V("shopping", "warranty", "bảo hành", "noun", "/ˈwɔːrənti/", "The phone comes with a two-year warranty.", "Điện thoại có bảo hành hai năm."),
        V("shopping", "available", "có sẵn", "adjective", "/əˈveɪləbl/", "Is this color available in medium?", "Màu này có sẵn cỡ vừa không?"),
        V("shopping", "compare", "so sánh", "verb", "/kəmˈper/", "I always compare prices online.", "Tôi luôn so sánh giá trên mạng."),
        V("shopping", "purchase", "mua hàng", "noun", "/ˈpɜːrtʃəs/", "Keep the receipt as proof of purchase.", "Hãy giữ hóa đơn làm bằng chứng mua hàng."),

        V("education", "assignment", "bài tập", "noun", "/əˈsaɪnmənt/", "The assignment is due on Monday.", "Bài tập đến hạn vào thứ Hai."),
        V("education", "curriculum", "chương trình học", "noun", "/kəˈrɪkjələm/", "The curriculum combines theory and practice.", "Chương trình học kết hợp lý thuyết và thực hành."),
        V("education", "concentrate", "tập trung", "verb", "/ˈkɑːnsntreɪt/", "I concentrate better in a quiet room.", "Tôi tập trung tốt hơn trong phòng yên tĩnh."),
        V("education", "revise", "ôn tập", "verb", "/rɪˈvaɪz/", "I need to revise for the exam.", "Tôi cần ôn tập cho kỳ thi."),
        V("education", "lecture", "bài giảng", "noun", "/ˈlektʃər/", "Today's lecture was very practical.", "Bài giảng hôm nay rất thực tế."),
        V("education", "research", "nghiên cứu", "noun", "/rɪˈsɜːrtʃ/", "Her research focuses on language learning.", "Nghiên cứu của cô ấy tập trung vào việc học ngôn ngữ."),
        V("education", "grade", "điểm số", "noun", "/ɡreɪd/", "He received a high grade for the project.", "Anh ấy nhận điểm cao cho dự án."),
        V("education", "scholarship", "học bổng", "noun", "/ˈskɑːlərʃɪp/", "She applied for an international scholarship.", "Cô ấy nộp đơn xin học bổng quốc tế."),
        V("education", "clarify", "làm rõ", "verb", "/ˈklærəfaɪ/", "Could you clarify the last point?", "Thầy cô có thể làm rõ ý cuối không?"),
        V("education", "progress", "sự tiến bộ", "noun", "/ˈprɑːɡres/", "Daily practice leads to steady progress.", "Luyện tập hằng ngày tạo ra tiến bộ ổn định."),

        V("relationships", "trust", "sự tin tưởng", "noun", "/trʌst/", "Honest communication builds trust.", "Giao tiếp chân thành xây dựng sự tin tưởng."),
        V("relationships", "supportive", "biết hỗ trợ", "adjective", "/səˈpɔːrtɪv/", "My friends are very supportive.", "Bạn bè của tôi rất biết hỗ trợ."),
        V("relationships", "misunderstanding", "sự hiểu lầm", "noun", "/ˌmɪsʌndərˈstændɪŋ/", "A small misunderstanding caused the argument.", "Một hiểu lầm nhỏ đã gây ra cuộc tranh cãi."),
        V("relationships", "apologize", "xin lỗi", "verb", "/əˈpɑːlədʒaɪz/", "I should apologize for being late.", "Tôi nên xin lỗi vì đến muộn."),
        V("relationships", "respect", "sự tôn trọng", "noun", "/rɪˈspekt/", "Mutual respect is important in every relationship.", "Sự tôn trọng lẫn nhau quan trọng trong mọi mối quan hệ."),
        V("relationships", "encourage", "động viên", "verb", "/ɪnˈkɜːrɪdʒ/", "My family encourages me to try new things.", "Gia đình động viên tôi thử những điều mới."),
        V("relationships", "patient", "kiên nhẫn", "adjective", "/ˈpeɪʃnt/", "Please be patient while I explain.", "Hãy kiên nhẫn khi tôi giải thích."),
        V("relationships", "argument", "cuộc tranh cãi", "noun", "/ˈɑːrɡjumənt/", "They resolved the argument calmly.", "Họ giải quyết cuộc tranh cãi một cách bình tĩnh."),
        V("relationships", "boundary", "ranh giới", "noun", "/ˈbaʊndri/", "Healthy boundaries protect both people.", "Ranh giới lành mạnh bảo vệ cả hai người."),
        V("relationships", "appreciate", "trân trọng", "verb", "/əˈpriːʃieɪt/", "I really appreciate your help.", "Tôi thực sự trân trọng sự giúp đỡ của bạn."),

        V("entertainment", "genre", "thể loại", "noun", "/ˈʒɑːnrə/", "Science fiction is my favorite genre.", "Khoa học viễn tưởng là thể loại tôi yêu thích."),
        V("entertainment", "plot", "cốt truyện", "noun", "/plɑːt/", "The plot becomes more exciting near the end.", "Cốt truyện trở nên hấp dẫn hơn ở gần cuối."),
        V("entertainment", "performance", "màn trình diễn", "noun", "/pərˈfɔːrməns/", "Her performance was powerful and convincing.", "Màn trình diễn của cô ấy mạnh mẽ và thuyết phục."),
        V("entertainment", "audience", "khán giả", "noun", "/ˈɔːdiəns/", "The audience laughed throughout the show.", "Khán giả cười suốt buổi diễn."),
        V("entertainment", "review", "bài đánh giá", "noun", "/rɪˈvjuː/", "I read a positive review of the film.", "Tôi đọc một bài đánh giá tích cực về bộ phim."),
        V("entertainment", "episode", "tập phim", "noun", "/ˈepɪsoʊd/", "The final episode answered every question.", "Tập cuối đã giải đáp mọi câu hỏi."),
        V("entertainment", "soundtrack", "nhạc phim", "noun", "/ˈsaʊndtræk/", "The soundtrack matches the story perfectly.", "Nhạc phim rất phù hợp với câu chuyện."),
        V("entertainment", "recommendation", "lời gợi ý", "noun", "/ˌrekəmenˈdeɪʃn/", "Thanks for the book recommendation.", "Cảm ơn lời gợi ý sách của bạn."),
        V("entertainment", "engaging", "cuốn hút", "adjective", "/ɪnˈɡeɪdʒɪŋ/", "The documentary is informative and engaging.", "Bộ phim tài liệu vừa nhiều thông tin vừa cuốn hút."),
        V("entertainment", "stream", "xem hoặc nghe trực tuyến", "verb", "/striːm/", "We streamed the concert at home.", "Chúng tôi xem buổi hòa nhạc trực tuyến tại nhà.")
    ];

    private static VocabularySeed V(string topic, string word, string definition, string partOfSpeech, string phonetic, string example, string translation) =>
        new(topic, word, definition, partOfSpeech, phonetic, example, translation);

    private static IReadOnlyList<PhraseSeed> CreatePhrases() =>
    [
        P("daily-life", "What does your typical day look like?", "Một ngày điển hình của bạn như thế nào?", "Hỏi về lịch sinh hoạt thường ngày."),
        P("daily-life", "I'm running a little late.", "Tôi đang hơi trễ một chút.", "Thông báo lịch trình bị chậm."),
        P("daily-life", "Let me check my schedule.", "Để tôi kiểm tra lịch.", "Kiểm tra trước khi xác nhận cuộc hẹn."),
        P("daily-life", "I usually take it easy in the evening.", "Tôi thường thư giãn vào buổi tối.", "Mô tả thói quen nghỉ ngơi."),
        P("daily-life", "I have a few errands to run.", "Tôi có vài việc vặt cần giải quyết.", "Nói về các việc cần làm bên ngoài."),
        P("travel", "I'd like to check in, please.", "Tôi muốn làm thủ tục nhận phòng.", "Tại quầy lễ tân khách sạn."),
        P("travel", "Could you tell me how to get there?", "Bạn có thể chỉ tôi cách đến đó không?", "Hỏi đường."),
        P("travel", "Is breakfast included?", "Bữa sáng có được bao gồm không?", "Hỏi về dịch vụ phòng."),
        P("travel", "My flight has been delayed.", "Chuyến bay của tôi đã bị hoãn.", "Thông báo vấn đề chuyến bay."),
        P("travel", "Can I leave my luggage here?", "Tôi có thể gửi hành lý ở đây không?", "Yêu cầu giữ hành lý."),
        P("food-and-dining", "Could we see the menu, please?", "Cho chúng tôi xem thực đơn được không?", "Bắt đầu gọi món."),
        P("food-and-dining", "What do you recommend?", "Bạn gợi ý món nào?", "Xin gợi ý từ nhân viên phục vụ."),
        P("food-and-dining", "I'm allergic to peanuts.", "Tôi bị dị ứng đậu phộng.", "Thông báo dị ứng thực phẩm."),
        P("food-and-dining", "Could we have the bill, please?", "Cho chúng tôi xin hóa đơn.", "Yêu cầu thanh toán."),
        P("food-and-dining", "This is absolutely delicious.", "Món này thực sự rất ngon.", "Khen món ăn."),
        P("work-and-career", "Could we move the deadline?", "Chúng ta có thể dời hạn chót không?", "Thương lượng tiến độ công việc."),
        P("work-and-career", "I'd like to share an update.", "Tôi muốn cập nhật tình hình.", "Mở đầu cập nhật công việc."),
        P("work-and-career", "Could you clarify the requirements?", "Bạn có thể làm rõ các yêu cầu không?", "Làm rõ phạm vi nhiệm vụ."),
        P("work-and-career", "I took the lead on this project.", "Tôi đã dẫn dắt dự án này.", "Mô tả kinh nghiệm phỏng vấn."),
        P("work-and-career", "Thank you for the constructive feedback.", "Cảm ơn phản hồi mang tính xây dựng.", "Phản hồi chuyên nghiệp."),
        P("technology", "Have you tried restarting the device?", "Bạn đã thử khởi động lại thiết bị chưa?", "Hỗ trợ kỹ thuật cơ bản."),
        P("technology", "The connection keeps dropping.", "Kết nối liên tục bị ngắt.", "Mô tả lỗi mạng."),
        P("technology", "How do I change the privacy settings?", "Tôi thay đổi cài đặt quyền riêng tư thế nào?", "Yêu cầu hướng dẫn ứng dụng."),
        P("technology", "This feature saves me a lot of time.", "Tính năng này giúp tôi tiết kiệm nhiều thời gian.", "Đánh giá sản phẩm công nghệ."),
        P("technology", "I can't log in to my account.", "Tôi không thể đăng nhập tài khoản.", "Báo lỗi truy cập."),
        P("health-and-fitness", "I've had this pain for two days.", "Tôi bị đau như vậy hai ngày rồi.", "Mô tả thời gian có triệu chứng."),
        P("health-and-fitness", "How often do you exercise?", "Bạn tập thể dục bao lâu một lần?", "Hỏi về tần suất vận động."),
        P("health-and-fitness", "I'm trying to eat a balanced diet.", "Tôi đang cố gắng ăn uống cân bằng.", "Nói về mục tiêu dinh dưỡng."),
        P("health-and-fitness", "Remember to stay hydrated.", "Hãy nhớ uống đủ nước.", "Nhắc nhở khi vận động."),
        P("health-and-fitness", "I need a day to recover.", "Tôi cần một ngày để hồi phục.", "Nói về nghỉ ngơi sau vận động."),
        P("shopping", "Do you have this in a larger size?", "Bạn có cỡ lớn hơn không?", "Hỏi kích cỡ sản phẩm."),
        P("shopping", "Can I try this on?", "Tôi có thể thử món này không?", "Yêu cầu thử quần áo."),
        P("shopping", "I'd like to return this item.", "Tôi muốn trả lại món hàng này.", "Bắt đầu yêu cầu đổi trả."),
        P("shopping", "Is there a discount available?", "Có chương trình giảm giá nào không?", "Hỏi ưu đãi."),
        P("shopping", "It comes with a two-year warranty.", "Sản phẩm có bảo hành hai năm.", "Mô tả chính sách bảo hành."),
        P("education", "Could you explain that again?", "Thầy cô có thể giải thích lại không?", "Yêu cầu lặp lại lời giải thích."),
        P("education", "When is the assignment due?", "Khi nào bài tập đến hạn?", "Hỏi hạn nộp bài."),
        P("education", "I'm having trouble concentrating.", "Tôi đang khó tập trung.", "Mô tả khó khăn học tập."),
        P("education", "What's the best way to revise?", "Cách ôn tập tốt nhất là gì?", "Xin lời khuyên học tập."),
        P("education", "I've made steady progress.", "Tôi đã tiến bộ đều đặn.", "Đánh giá quá trình học."),
        P("relationships", "I appreciate you telling me.", "Tôi trân trọng việc bạn đã nói với tôi.", "Phản hồi khi người khác chia sẻ."),
        P("relationships", "I think there has been a misunderstanding.", "Tôi nghĩ đã có sự hiểu lầm.", "Bắt đầu làm rõ mâu thuẫn."),
        P("relationships", "I'm sorry I hurt your feelings.", "Tôi xin lỗi vì đã làm bạn tổn thương.", "Xin lỗi chân thành."),
        P("relationships", "How have you been lately?", "Dạo này bạn thế nào?", "Hỏi thăm thân mật."),
        P("relationships", "You can always count on me.", "Bạn luôn có thể tin cậy tôi.", "Thể hiện sự hỗ trợ."),
        P("entertainment", "What kind of movies are you into?", "Bạn thích thể loại phim nào?", "Hỏi sở thích phim ảnh."),
        P("entertainment", "I highly recommend this series.", "Tôi rất khuyên bạn xem loạt phim này.", "Đưa ra lời gợi ý."),
        P("entertainment", "The plot kept me guessing.", "Cốt truyện khiến tôi liên tục phải đoán.", "Nhận xét cốt truyện."),
        P("entertainment", "The soundtrack was amazing.", "Nhạc phim rất tuyệt.", "Nhận xét âm nhạc."),
        P("entertainment", "It's worth watching more than once.", "Bộ phim đáng xem nhiều lần.", "Đánh giá tích cực.")
    ];

    private static PhraseSeed P(string topic, string phrase, string translation, string context) =>
        new(topic, phrase, translation, context);
}
