using System;
using System.Collections.Generic;
using System.Text;

namespace WordsTrainer.Mobile.Services
{
    public class UiTextService
    {
        private string _languageCode = "en";

        public void SetLanguage(string? languageCode)
        {
            _languageCode = string.IsNullOrWhiteSpace(languageCode)
                ? "en"
                : languageCode.ToLowerInvariant();
        }

        public string T(string key)
        {
            return _languageCode switch
            {
                "ru" => Ru.TryGetValue(key, out var ru) ? ru : En.GetValueOrDefault(key, key),
                "de" => De.TryGetValue(key, out var de) ? de : En.GetValueOrDefault(key, key),
                _ => En.GetValueOrDefault(key, key)
            };
        }

        private static readonly Dictionary<string, string> En = new()
        {
            ["app.title"] = "WordsTrainer",
            ["app.subtitle"] = "Daily vocabulary practice",
            ["logout"] = "Logout",
            ["today"] = "Today",
            ["correct"] = "Correct",
            ["new"] = "New",
            ["learned"] = "Learned",
            ["translate.word"] = "Translate this word",
            ["dont.know"] = "I don't know this word",
            ["training.complete"] = "Training complete",
            ["correct.answer"] = "Correct!",
            ["wrong.answer"] = "Correct answer: {0}",
            ["no.words"] = "No words available for training.",
            ["session.complete"] = "Great job! You completed today's training."
        };

        private static readonly Dictionary<string, string> Ru = new()
        {
            ["app.title"] = "WordsTrainer",
            ["app.subtitle"] = "Ежедневная тренировка слов",
            ["logout"] = "Выйти",
            ["today"] = "Сегодня",
            ["correct"] = "Верно",
            ["new"] = "Новые",
            ["learned"] = "Выучено",
            ["translate.word"] = "Переведи слово",
            ["dont.know"] = "Я не знаю это слово",
            ["training.complete"] = "Тренировка завершена",
            ["correct.answer"] = "Правильно!",
            ["wrong.answer"] = "Правильный ответ: {0}",
            ["no.words"] = "Нет слов для тренировки.",
            ["session.complete"] = "Отлично! Сегодняшняя тренировка завершена."
        };

        private static readonly Dictionary<string, string> De = new()
        {
            ["app.title"] = "WordsTrainer",
            ["app.subtitle"] = "Tägliches Worttraining",
            ["logout"] = "Abmelden",
            ["today"] = "Heute",
            ["correct"] = "Richtig",
            ["new"] = "Neu",
            ["learned"] = "Gelernt",
            ["translate.word"] = "Übersetze dieses Wort",
            ["dont.know"] = "Ich kenne dieses Wort nicht",
            ["training.complete"] = "Training abgeschlossen",
            ["correct.answer"] = "Richtig!",
            ["wrong.answer"] = "Richtige Antwort: {0}",
            ["no.words"] = "Keine Wörter zum Trainieren verfügbar.",
            ["session.complete"] = "Sehr gut! Dein heutiges Training ist abgeschlossen."
        };
    }
}
