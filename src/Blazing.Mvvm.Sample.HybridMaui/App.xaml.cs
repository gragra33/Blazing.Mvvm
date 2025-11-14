namespace Blazing.Mvvm.Sample.HybridMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Blazing.Mvvm.Sample.HybridMaui" };
        }
    }
}
