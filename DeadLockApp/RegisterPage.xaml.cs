namespace DeadLockApp;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
        BindingContext = new DeadLockApp.ViewModels.RegisterViewModel();
    }
}