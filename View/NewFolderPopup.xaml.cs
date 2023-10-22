using CommunityToolkit.Maui.Views;

namespace Ashwell_Maintenance.View;

public partial class NewFolderPopup : Popup
{
	public NewFolderPopup()
	{
		InitializeComponent();
	}

	public void NewFolderCancel(object sender, EventArgs e)
	{
		this.Close();
	}
}