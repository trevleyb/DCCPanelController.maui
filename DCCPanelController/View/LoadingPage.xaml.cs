using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCCPanelController.View;

public partial class LoadingPage : ContentPage {
    public LoadingPage()
    {
        InitializeComponent();

        Title = "Starting…";
        Content = new Grid {
            Children = {
                new ActivityIndicator { IsRunning = true, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Fill },
                new Label { Text = "Loading profile…", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Fill }
            }
        };
        NavigationPage.SetHasNavigationBar(this, false);
    }
}