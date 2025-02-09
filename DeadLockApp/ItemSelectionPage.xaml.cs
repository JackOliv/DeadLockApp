using DeadLockApp.ViewModels;

namespace DeadLockApp
{
    [QueryProperty(nameof(PartId), "partId")]
    public partial class ItemSelectionPage : ContentPage
    {
        public int PartId { get; set; }
        public ItemSelectionPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
