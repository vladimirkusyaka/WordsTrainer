using System;
using System.Collections.Generic;
using System.Text;
using WordsTrainer.Mobile.ViewModels;

namespace WordsTrainer.Mobile.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
