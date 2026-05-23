using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Seed;

public static class VocabularySeedData
{
    public static IReadOnlyList<VocabularyItem> CreateDefaultEnglishItems(DateTime utcNow) =>
        DefaultItems.Select(item => CreateItem(item, utcNow)).ToList();

    private static VocabularyItem CreateItem(DefaultVocabularyItem item, DateTime utcNow)
    {
        var vocabularyItem = VocabularyItem.Create(
            LanguageCode.English,
            item.Word,
            phonetic: null,
            item.Level,
            item.Topic,
            item.PartOfSpeech,
            utcNow);

        vocabularyItem.AddTranslation(LanguageCode.Vietnamese, item.MeaningVi);
        var example = vocabularyItem.AddExample(item.Example, item.Level);
        example.AddTranslation(LanguageCode.Vietnamese, item.ExampleMeaningVi);

        return vocabularyItem;
    }

    private static readonly IReadOnlyList<DefaultVocabularyItem> DefaultItems =
    [
        new("hello", "xin chao", "Hello, my name is Linh.", "Xin chao, ten toi la Linh.", LanguageLevel.A1, "greeting", PartOfSpeech.Interjection),
        new("family", "gia dinh", "My family is very kind.", "Gia dinh toi rat tot bung.", LanguageLevel.A1, "people", PartOfSpeech.Noun),
        new("school", "truong hoc", "I go to school every morning.", "Toi den truong moi buoi sang.", LanguageLevel.A1, "daily life", PartOfSpeech.Noun),
        new("learn", "hoc", "I learn English after dinner.", "Toi hoc tieng Anh sau bua toi.", LanguageLevel.A1, "study", PartOfSpeech.Verb),
        new("listen", "lang nghe", "Please listen to the question.", "Lam on lang nghe cau hoi.", LanguageLevel.A1, "study", PartOfSpeech.Verb),
        new("speak", "noi", "She can speak slowly and clearly.", "Co ay co the noi cham va ro rang.", LanguageLevel.A1, "speaking", PartOfSpeech.Verb),
        new("friend", "ban be", "My friend helps me practice.", "Ban toi giup toi luyen tap.", LanguageLevel.A1, "people", PartOfSpeech.Noun),
        new("happy", "vui ve", "He is happy today.", "Hom nay anh ay vui ve.", LanguageLevel.A1, "feelings", PartOfSpeech.Adjective),
        new("quickly", "nhanh chong", "She answered quickly.", "Co ay tra loi nhanh chong.", LanguageLevel.A2, "communication", PartOfSpeech.Adverb),
        new("because", "boi vi", "I study because I want to improve.", "Toi hoc boi vi toi muon tien bo.", LanguageLevel.A2, "grammar", PartOfSpeech.Conjunction),
        new("between", "o giua", "The book is between two bags.", "Quyen sach nam giua hai cai tui.", LanguageLevel.A2, "position", PartOfSpeech.Preposition),
        new("practice", "luyen tap", "Daily practice builds confidence.", "Luyen tap hang ngay xay dung su tu tin.", LanguageLevel.A2, "study", PartOfSpeech.Noun),
        new("improve", "cai thien", "Your pronunciation will improve.", "Phat am cua ban se cai thien.", LanguageLevel.A2, "progress", PartOfSpeech.Verb),
        new("mistake", "loi sai", "A mistake can help you learn.", "Mot loi sai co the giup ban hoc.", LanguageLevel.A2, "learning", PartOfSpeech.Noun),
        new("conversation", "cuoc hoi thoai", "This conversation is about travel.", "Cuoc hoi thoai nay ve du lich.", LanguageLevel.B1, "speaking", PartOfSpeech.Noun),
        new("explain", "giai thich", "Can you explain the answer?", "Ban co the giai thich cau tra loi khong?", LanguageLevel.B1, "communication", PartOfSpeech.Verb),
        new("confident", "tu tin", "She feels confident when speaking.", "Co ay cam thay tu tin khi noi.", LanguageLevel.B1, "feelings", PartOfSpeech.Adjective),
        new("fluency", "su troi chay", "Fluency improves with regular speaking.", "Su troi chay cai thien khi noi thuong xuyen.", LanguageLevel.B1, "speaking", PartOfSpeech.Noun),
        new("accurate", "chinh xac", "Try to give an accurate answer.", "Hay co gang dua ra cau tra loi chinh xac.", LanguageLevel.B1, "assessment", PartOfSpeech.Adjective),
        new("recommendation", "loi khuyen", "The tutor gave a useful recommendation.", "Gia su dua ra mot loi khuyen huu ich.", LanguageLevel.B2, "feedback", PartOfSpeech.Noun)
    ];

    private sealed record DefaultVocabularyItem(
        string Word,
        string MeaningVi,
        string Example,
        string ExampleMeaningVi,
        LanguageLevel Level,
        string Topic,
        PartOfSpeech PartOfSpeech);
}
