using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels
{
    public class ExplanationViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;

        private string _targetWord = "";
        private string _nativeTranslation = "";
        private string _explanation = "";
        private string _message = "";
        private bool _isBusy;

        public ExplanationViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
            PlayAudioCommand = new Command(async () => await PlayAudioAsync());
        }

        public string TargetWord { get => _targetWord; set => SetField(ref _targetWord, value); }
        public string NativeTranslation { get => _nativeTranslation; set => SetField(ref _nativeTranslation, value); }
        public string Explanation { get => _explanation; set => SetField(ref _explanation, value); }
        public string Message { get => _message; set => SetField(ref _message, value); }
        public bool IsBusy { get => _isBusy; set => SetField(ref _isBusy, value); }

        public string? AudioUrl { get; private set; }

        public ICommand PlayAudioCommand { get; }

        public async Task LoadAsync(Guid attemptId)
        {
            try
            {
                IsBusy = true;
                Message = "";

                var response = await _apiClient.GetExplanationAsync(attemptId);

                if (response == null)
                {
                    Message = "Объяснение не найдено.";
                    return;
                }

                TargetWord = response.TargetWord;
                NativeTranslation = response.NativeTranslation;
                Explanation = response.Explanation;
                AudioUrl = response.AudioUrl;
            }
            catch (Exception ex)
            {
                Message = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PlayAudioAsync()
        {
            Message = string.IsNullOrWhiteSpace(AudioUrl)
                ? "Аудио пока не подключено."
                : $"AudioUrl: {AudioUrl}";

            await Task.CompletedTask;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }
    }
}
