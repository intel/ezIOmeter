using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ezIOmeter
{
	/// <summary>
	/// Interaction logic for NavigationDrawer.xaml
	/// </summary>
	public partial class NavigationDrawer : UserControl
	{
		public NavigationDrawer()
		{
			this.InitializeComponent();
		}

        public void SetDawerWidth(double window_width, double header_height)
        {
            double temp_width = window_width - header_height;

            // Adjust the navgiation drawer content's dimenstions.
            NavigationDrawerContent.Width = temp_width;
            NavigationDrawerContent.MinWidth = temp_width;
        }

        public void OpenDrawer()
        {
            this.Visibility = System.Windows.Visibility.Visible;
            Storyboard open_drawer = (Storyboard)this.Resources["OpenDrawer"];
            open_drawer.Begin();
        }

        public void CloseDrawer()
        {
            Storyboard close_drawer = (Storyboard)this.Resources["CloseDrawer"];
            close_drawer.Completed += new EventHandler(Close_Completed);
            close_drawer.Begin();            
        }

        private void Menu_Back_Clicked(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
        }

        private void OverlayMask_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseDrawer();
        }

        private void Close_Completed(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
	}
}