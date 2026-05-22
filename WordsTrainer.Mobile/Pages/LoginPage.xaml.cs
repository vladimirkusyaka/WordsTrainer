using System;
using System.Collections.Generic;
using System.Text;
using WordsTrainer.Mobile.ViewModels;

namespace WordsTrainer.Mobile.Pages
{
    public partial class LoginPage : ContentPage
    {
        private bool _isPasswordVisible;

        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void TogglePasswordClicked(object? sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            PasswordEntry.IsPassword = !_isPasswordVisible;

            if (sender is ImageButton button)
            {
                button.Source = _isPasswordVisible
                    ? "eye_off.svg"
                    : "eye.svg";
            }
        }
    }
}
