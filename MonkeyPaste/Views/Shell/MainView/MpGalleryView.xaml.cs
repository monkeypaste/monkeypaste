using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
//using static Xamarin.Essentials.Permissions;

namespace MonkeyPaste
{
    public partial class MpGalleryView : ContentPage
    {
        public MpGalleryView()
        {
            InitializeComponent();
            BindingContext = MpResolver.Resolve<MpGalleryViewModel>();
        }

        private void SelectToolBarItem_Clicked(object sender, EventArgs e)
        {
            if (!Photos.SelectedItems.Any())
            {
                DisplayAlert("No photos", "No photos selected", "OK");
                return;
            }
            var viewModel = (MpGalleryViewModel)BindingContext;
            viewModel.AddFavorites.Execute(Photos.SelectedItems.Select(x => (Photo)x).ToList());
            DisplayAlert("Added", "Selected photos has been added to favorites", "OK");
        }

    }
}
