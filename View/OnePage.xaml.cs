namespace Ashwell_Maintenance.View;

public partial class OnePage : ContentPage
{
    double pipeworkVolumeNumber;
    double totalPipeworkVolumeNumber = 0;

	public OnePage()
	{
		InitializeComponent();

        List<Int64> numbers = new List<Int64>();
        for (Int64 i = 1; i <= 88; i++)
            numbers.Add(i);

        steel1.ItemsSource = numbers;
        steel2.ItemsSource = numbers;
        steel3.ItemsSource = numbers;
        steel4.ItemsSource = numbers;
        steel5.ItemsSource = numbers;
        steel6.ItemsSource = numbers;
        steel7.ItemsSource = numbers;
        steel8.ItemsSource = numbers;
        steel9.ItemsSource = numbers;
        steel10.ItemsSource = numbers;
        steel11.ItemsSource = numbers;
        steel12.ItemsSource = numbers;
        steel13.ItemsSource = numbers;

        copper1.ItemsSource = numbers;
        copper2.ItemsSource = numbers;
        copper3.ItemsSource = numbers;
        copper4.ItemsSource = numbers;
        copper5.ItemsSource = numbers;
        copper6.ItemsSource = numbers;
        copper7.ItemsSource = numbers;

        pesdr1.ItemsSource = numbers;
        pesdr2.ItemsSource = numbers;
        pesdr3.ItemsSource = numbers;
        pesdr4.ItemsSource = numbers;
        pesdr5.ItemsSource = numbers;
        pesdr6.ItemsSource = numbers;
        pesdr7.ItemsSource = numbers;
        pesdr8.ItemsSource = numbers;
    }

    [Obsolete]
    public async void OneBack(object sender, EventArgs e)
	{
        if (OSection1.IsVisible)
        {
            OneBackBtt.IsEnabled = false;
            await Navigation.PopModalAsync();
        }
        else if (OSection2.IsVisible)
        {
            OSection2.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await OSection1.ScrollToAsync(0, 0, false);
            OSection1.IsVisible = true;
        }
        else if (OSection3.IsVisible)
        {
            OSection3.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await OSection2.ScrollToAsync(0, 0, false);
            OSection2.IsVisible = true;
        }
        else if (OSection4.IsVisible)
        {
            checkAreaA.IsChecked = false;
            checkAreaB.IsChecked = false;
            checkAreaCD.IsChecked = false;

            OSection4.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await OSection3.ScrollToAsync(0, 0, false);
            OSection3.IsVisible = true;
        }
        else if (OSection5.IsVisible)
        {
            OSection5.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await OSection4.ScrollToAsync(0, 0, false);
            OSection4.IsVisible = true;
        }
        else
        {
            OSection6.IsVisible = false;

            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                await OSection5.ScrollToAsync(0, 0, false);
            OSection5.IsVisible = true;
        }
	}

    [Obsolete]
    public async void ONext1(object sender, EventArgs e)
    {
        OSection1.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await OSection2.ScrollToAsync(0, 0, false);
        OSection2.IsVisible = true;
    }
    [Obsolete]
    public async void ONext2(object sender, EventArgs e)
    {
        OSection2.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await OSection3.ScrollToAsync(0, 0, false);
        OSection3.IsVisible = true;
    }
    [Obsolete]
    public async void ONext3(object sender, EventArgs e)
    {
        OSection3.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await OSection4.ScrollToAsync(0, 0, false);
        OSection4.IsVisible = true;

        if (!new[] { null, "", ".", "-", "+"}.Contains(gaugeReadableMovement.Text) && totalVolumeForTesting.Text != null && testMediumFactor.Text != null)
        {
            AreaA_Value.Text = Math.Round(double.Parse(gaugeReadableMovement.Text) * double.Parse(totalVolumeForTesting.Text) * double.Parse(testMediumFactor.Text), 3).ToString();
            if (!new[] { null, "", ".", "-", "+" }.Contains(roomVolume.Text))
                AreaB_Value.Text = Math.Round(2.8 * double.Parse(AreaA_Value.Text) / double.Parse(roomVolume.Text), 3).ToString();
            AreaCD_Value.Text = Math.Round(0.047 * double.Parse(AreaA_Value.Text), 3).ToString();
        }
        else
        {
            AreaA_Value.Text = null;
            AreaB_Value.Text = null;
            AreaCD_Value.Text = null;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }
    [Obsolete]
    public async void ONext4(object sender, EventArgs e)
    {
        OSection4.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await OSection5.ScrollToAsync(0, 0, false);
        OSection5.IsVisible = true;
    }
    [Obsolete]
    public async void ONext5(object sender, EventArgs e)
    {
        OSection5.IsVisible = false;

        if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            await OSection6.ScrollToAsync(0, 0, false);
        OSection6.IsVisible = true;
    }
    public void ONextFinish(object sender, EventArgs e)
    {

    }


    public void checkAreaA_Tap(object sender, EventArgs e)
    {
        if (checkAreaA.IsChecked)
        {
            checkAreaA.Color = Colors.Red;
            checkAreaB.IsChecked = false;
            checkAreaCD.IsChecked = false;

            AreaB.IsVisible = false;
            AreaCD.IsVisible = false;
            AreaA.IsVisible = true;

            if (AreaA_Value.Text != null)
            {
                letByDuration.Text = "2";
                stabilisationDuration.Text = "15";
                testDuration.Text = "2";

                double min = Math.Ceiling(double.Parse(AreaA_Value.Text));
                string min_string = min.ToString();

                if (min > 15)
                {
                    letByDuration.Text = min_string;
                    stabilisationDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
                else if (min > 2)
                {
                    letByDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
            }
            else
            {
                letByDuration.Text = null;
                stabilisationDuration.Text = null;
                testDuration.Text = null;
            }
        }
        else
        {
            checkAreaA.Color = Colors.White;
            AreaA.IsVisible = false;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }
    public void checkAreaB_Tap(object sender, EventArgs e)
    {
        if (checkAreaB.IsChecked)
        {
            checkAreaB.Color = Colors.Red;
            checkAreaA.IsChecked = false;
            checkAreaCD.IsChecked = false;

            AreaA.IsVisible = false;
            AreaCD.IsVisible = false;
            AreaB.IsVisible = true;

            if (AreaB_Value.Text != null)
            {
                letByDuration.Text = "2";
                stabilisationDuration.Text = "15";
                testDuration.Text = "2";

                double min = Math.Ceiling(double.Parse(AreaB_Value.Text));
                string min_string = min.ToString();

                if (min > 15)
                {
                    letByDuration.Text = min_string;
                    stabilisationDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
                else if (min > 2)
                {
                    letByDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
            }
            else
            {
                letByDuration.Text = null;
                stabilisationDuration.Text = null;
                testDuration.Text = null;
            }
        }
        else
        {
            checkAreaB.Color = Colors.White;
            AreaB.IsVisible = false;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }
    public void checkAreaCD_Tap(object sender, EventArgs e)
    {
        if (checkAreaCD.IsChecked)
        {
            checkAreaCD.Color = Colors.Red;
            checkAreaB.IsChecked = false;
            checkAreaA.IsChecked = false;

            AreaA.IsVisible = false;
            AreaB.IsVisible = false;
            AreaCD.IsVisible = true;

            if (AreaCD_Value.Text != null)
            {
                letByDuration.Text = "2";
                stabilisationDuration.Text = "15";
                testDuration.Text = "2";

                double min = Math.Ceiling(double.Parse(AreaCD_Value.Text));
                string min_string = min.ToString();

                if (min > 15)
                {
                    letByDuration.Text = min_string;
                    stabilisationDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
                else if (min > 2)
                {
                    letByDuration.Text = min_string;
                    testDuration.Text = min_string;
                }
            }
            else
            {
                letByDuration.Text = null;
                stabilisationDuration.Text = null;
                testDuration.Text = null;
            }
        }
        else
        {
            checkAreaCD.Color = Colors.White;
            AreaCD.IsVisible = false;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }


    //public void letByDuration_Completed(object sender, EventArgs e)
    //{
    //    EntryChanged(letByDuration);
    //}
    //public void stabilisationDuration_Completed(object sender, EventArgs e)
    //{
    //    EntryChanged(stabilisationDuration);
    //}
    //public void testDuration_Completed(object sender, EventArgs e)
    //{
    //    EntryChanged(testDuration);
    //}


    private void UpdateTotalPipeworkVolume(Label total, Picker quantificator, double k)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(quantificator.SelectedItem.ToString()) * k, 5);
        total.Text = pipeworkVolumeNumber.ToString();

        totalPipeworkVolumeNumber += pipeworkVolumeNumber;
        totalPipeworkVolume.Text = Math.Round(totalPipeworkVolumeNumber, 5).ToString();
        pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

        if (meterVolume.Text != null && meterVolume.Text != "0.079d2L*")
            totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 5).ToString();
    }
    private void SubtractTotalPipeworkVolume()
    {
        double subtract = Math.Round(totalPipeworkVolumeNumber - pipeworkVolumeNumber, 5);
        if (subtract == 0)
        {
            totalPipeworkVolumeNumber = 0;
            totalPipeworkVolume.Text = null;
            pipeworkFittingsIV.Text = null;
            totalVolumeForTesting.Text = null;
        }
        else
        {
            totalPipeworkVolumeNumber = subtract;
            totalPipeworkVolume.Text = subtract.ToString();
            pipeworkFittingsIV.Text = Math.Round(totalPipeworkVolumeNumber + totalPipeworkVolumeNumber * 0.1, 7).ToString();

            if (meterVolume.Text != null)
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 5).ToString();
        }
    }

    //private void EntryChanged(Entry entry)
    //{
    //    double min;
    //    double setMin = entry == stabilisationDuration ? 15 : 2;
    //    string value = entry.Text;

    //    if (!checkAreaA.IsChecked && !checkAreaB.IsChecked && !checkAreaCD.IsChecked)
    //        entry.Text = null;
    //    else if (checkAreaA.IsChecked)
    //    {
    //        if (AreaA_Value.Text != null)
    //        {
    //            min = Math.Ceiling(double.Parse(AreaA_Value.Text));

    //            if (double.Parse(entry.Text) < min)
    //            {
    //                if (min > setMin)
    //                    entry.Text = min.ToString();
    //                else
    //                    entry.Text = setMin.ToString();
    //            }
    //            else if (entry != stabilisationDuration && double.Parse(entry.Text) > double.Parse(stabilisationDuration.Text))
    //            {
    //                letByDuration.Text = value;
    //                stabilisationDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //            else
    //            {
    //                if (min > 15)
    //                    stabilisationDuration.Text = min.ToString();
    //                else
    //                    stabilisationDuration.Text = "15";

    //                letByDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //        }
    //        else
    //            entry.Text = null;
    //    }
    //    else if (checkAreaB.IsChecked)
    //    {
    //        if (AreaB_Value.Text != null)
    //        {
    //            min = Math.Ceiling(double.Parse(AreaB_Value.Text));

    //            if (double.Parse(entry.Text) < min)
    //            {
    //                if (min > setMin)
    //                    entry.Text = min.ToString();
    //                else
    //                    entry.Text = setMin.ToString();
    //            }
    //            else if (entry != stabilisationDuration && double.Parse(entry.Text) > double.Parse(stabilisationDuration.Text))
    //            {
    //                letByDuration.Text = value;
    //                stabilisationDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //            else
    //            {
    //                if (min > 15)
    //                    stabilisationDuration.Text = min.ToString();
    //                else
    //                    stabilisationDuration.Text = "15";

    //                letByDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //        }
    //        else
    //            entry.Text = null;
    //    }
    //    else
    //    {
    //        if (AreaCD_Value.Text != null)
    //        {
    //            min = Math.Ceiling(double.Parse(AreaCD_Value.Text));

    //            if (double.Parse(entry.Text) < min)
    //            {
    //                if (min > setMin)
    //                    entry.Text = min.ToString();
    //                else
    //                    entry.Text = setMin.ToString();
    //            }
    //            else if (entry != stabilisationDuration && double.Parse(entry.Text) > double.Parse(stabilisationDuration.Text))
    //            {
    //                letByDuration.Text = value;
    //                stabilisationDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //            else
    //            {
    //                if (min > 15)
    //                    stabilisationDuration.Text = min.ToString();
    //                else
    //                    stabilisationDuration.Text = "15";

    //                letByDuration.Text = value;
    //                testDuration.Text = value;
    //            }
    //        }
    //        else
    //            entry.Text = null;
    //    }
    //}


    public void meterVolumePicker_IndexChanged(object sender, EventArgs e)
    {
        if (meterVolumePicker.SelectedIndex != -1)
        {
            meterVolumePicker_delete.IsVisible = true;

            switch (meterVolumePicker.SelectedItem)
            {
                case "G4 / U6": meterVolume.Text = "0.008"; break;
                case "U16": meterVolume.Text = "0.025"; break;
                case "U25": meterVolume.Text = "0.037"; break;
                case "U40": meterVolume.Text = "0.067"; break;
                case "U65": meterVolume.Text = "0.1"; break;
                case "U100": meterVolume.Text = "0.182"; break;
                case "U160": meterVolume.Text = "0.304"; break;
                case "RD or Turbine": meterVolume.Text = "0.079d2L*"; break;
                case "Ultrasonic": meterVolume.Text = "0.0024"; break;
            }

            if (meterVolume.Text != "0.079d2L*" && pipeworkFittingsIV.Text != null)
                totalVolumeForTesting.Text = Math.Round(double.Parse(pipeworkFittingsIV.Text) + double.Parse(meterVolume.Text), 5).ToString();
        }
        else
        {
            meterVolumePicker_delete.IsVisible = false;
        }
    }
    public void meterVolumePicker_Delete(object sender, EventArgs e)
    {
        meterVolume.Text = null;
        totalVolumeForTesting.Text = null;
        meterVolumePicker.SelectedIndex = -1;
    }


    public void roomVolume_TextChanged(object sender, EventArgs e)
    {
        if (!new[] { null, "", ".", "-", "+" }.Contains(roomVolume.Text) && !new[] { null, " ", ".", "-", "+" }.Contains(gaugeReadableMovement.Text) && totalVolumeForTesting.Text != null && testMediumFactor.Text != null)
        {
            AreaB_Value.Text = Math.Round(2.8 * double.Parse(AreaA_Value.Text) / double.Parse(roomVolume.Text), 3).ToString();

            letByDuration.Text = "2";
            stabilisationDuration.Text = "15";
            testDuration.Text = "2";

            double min = Math.Ceiling(double.Parse(AreaB_Value.Text));
            string min_string = min.ToString();

            if (min > 15)
            {
                letByDuration.Text = min_string;
                stabilisationDuration.Text = min_string;
                testDuration.Text = min_string;
            }
            else if (min > 2)
            {
                letByDuration.Text = min_string;
                testDuration.Text = min_string;
            }
        }
        else
        {
            AreaB_Value.Text = null;

            letByDuration.Text = null;
            stabilisationDuration.Text = null;
            testDuration.Text = null;
        }
    }

    public void testMediumPicker_IndexChanged(object sender, EventArgs e)
    {
        if (testMediumPicker.SelectedIndex != -1)
        {
            testMediumPicker_x.IsVisible = true;
            testMediumPicker_delete.IsVisible = true;

            if (testMediumPicker.SelectedItem.ToString() == "Natural Gas")
                testMediumFactor.Text = "42";

            else
                testMediumFactor.Text = "67";
        }
        else
        {
            testMediumPicker_x.IsVisible = false;
            testMediumPicker_delete.IsVisible = false;
        }
    }
    public void testMediumPicker_Delete(object sender, EventArgs e)
    {
        testMediumFactor.Text = null;
        testMediumPicker.SelectedIndex = -1;
    }


    public void installationPicker_IndexChanged(object sender, EventArgs e)
    {
        if (installationPicker.SelectedIndex != -1)
        {
            installationPicker_x.IsVisible = true;
            installationPicker_delete.IsVisible = true;
        }
        else
        {
            installationPicker_x.IsVisible = false;
            installationPicker_delete.IsVisible = false;
        }
    }
    public void installationPicker_Delete(object sender, EventArgs e)
    {
        installationPicker.SelectedIndex = -1;
    }


    public void maximumPermittedLeakRate_IndexChanged(object sender, EventArgs e)
    {
        if (maximumPermittedLeakRate.SelectedIndex != -1)
        {
            maximumPermittedLeakRate_x.IsVisible = true;
            maximumPermittedLeakRate_delete.IsVisible = true;
        }
        else
        {
            maximumPermittedLeakRate_x.IsVisible = false;
            maximumPermittedLeakRate_delete.IsVisible = false;
        }
    }
    public void maximumPermittedLeakRate_Delete(object sender, EventArgs e)
    {
        maximumPermittedLeakRate.SelectedIndex = -1;
    }

    // steel ================================================================================================================================================================================================================

    public void steel1_IndexChanged(object sender, EventArgs e)
    {
        if (steel1.SelectedIndex != -1)
        {
            steel1_x.IsVisible = true;
            steel1_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel1Total, steel1, 0.00024);
        }
        else
        {
            steel1_x.IsVisible = false;
            steel1_delete.IsVisible = false;
            steel1Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel1_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel1.SelectedItem.ToString()) * 0.00024, 5);
        steel1.SelectedIndex = -1;
    }


    public void steel2_IndexChanged(object sender, EventArgs e)
    {
        if (steel2.SelectedIndex != -1)
        {
            steel2_x.IsVisible = true;
            steel2_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel2Total, steel2, 0.00046);
        }
        else
        {
            steel2_x.IsVisible = false;
            steel2_delete.IsVisible = false;
            steel2Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel2_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel2.SelectedItem.ToString()) * 0.00046, 5);
        steel2.SelectedIndex = -1;
    }


    public void steel3_IndexChanged(object sender, EventArgs e)
    {
        if (steel3.SelectedIndex != -1)
        {
            steel3_x.IsVisible = true;
            steel3_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel3Total, steel3, 0.00064);
        }
        else
        {
            steel3_x.IsVisible = false;
            steel3_delete.IsVisible = false;
            steel3Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel3_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel3.SelectedItem.ToString()) * 0.00064, 5);
        steel3.SelectedIndex = -1;
    }


    public void steel4_IndexChanged(object sender, EventArgs e)
    {
        if (steel4.SelectedIndex != -1)
        {
            steel4_x.IsVisible = true;
            steel4_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel4Total, steel4, 0.0011);
        }
        else
        {
            steel4_x.IsVisible = false;
            steel4_delete.IsVisible = false;
            steel4Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel4_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel4.SelectedItem.ToString()) * 0.0011, 5);
        steel4.SelectedIndex = -1;
    }


    public void steel5_IndexChanged(object sender, EventArgs e)
    {
        if (steel5.SelectedIndex != -1)
        {
            steel5_x.IsVisible = true;
            steel5_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel5Total, steel5, 0.0015);
        }
        else
        {
            steel5_x.IsVisible = false;
            steel5_delete.IsVisible = false;
            steel5Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel5_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel5.SelectedItem.ToString()) * 0.0015, 5);
        steel5.SelectedIndex = -1;
    }


    public void steel6_IndexChanged(object sender, EventArgs e)
    {
        if (steel6.SelectedIndex != -1)
        {
            steel6_x.IsVisible = true;
            steel6_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel6Total, steel6, 0.0024);
        }
        else
        {
            steel6_x.IsVisible = false;
            steel6_delete.IsVisible = false;
            steel6Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel6_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel6.SelectedItem.ToString()) * 0.0024, 5);
        steel6.SelectedIndex = -1;
    }


    public void steel7_IndexChanged(object sender, EventArgs e)
    {
        if (steel7.SelectedIndex != -1)
        {
            steel7_x.IsVisible = true;
            steel7_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel7Total, steel7, 0.0038);
        }
        else
        {
            steel7_x.IsVisible = false;
            steel7_delete.IsVisible = false;
            steel7Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel7_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel7.SelectedItem.ToString()) * 0.0038, 5);
        steel7.SelectedIndex = -1;
    }


    public void steel8_IndexChanged(object sender, EventArgs e)
    {
        if (steel8.SelectedIndex != -1)
        {
            steel8_x.IsVisible = true;
            steel8_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel8Total, steel8, 0.0054);
        }
        else
        {
            steel8_x.IsVisible = false;
            steel8_delete.IsVisible = false;
            steel8Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel8_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel8.SelectedItem.ToString()) * 0.0054, 5);
        steel8.SelectedIndex = -1;
    }


    public void steel9_IndexChanged(object sender, EventArgs e)
    {
        if (steel9.SelectedIndex != -1)
        {
            steel9_x.IsVisible = true;
            steel9_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel9Total, steel9, 0.009);
        }
        else
        {
            steel9_x.IsVisible = false;
            steel9_delete.IsVisible = false;
            steel9Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel9_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel9.SelectedItem.ToString()) * 0.009, 5);
        steel9.SelectedIndex = -1;
    }


    public void steel10_IndexChanged(object sender, EventArgs e)
    {
        if (steel10.SelectedIndex != -1)
        {
            steel10_x.IsVisible = true;
            steel10_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel10Total, steel10, 0.014);
        }
        else
        {
            steel10_x.IsVisible = false;
            steel10_delete.IsVisible = false;
            steel10Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel10_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel10.SelectedItem.ToString()) * 0.014, 5);
        steel10.SelectedIndex = -1;
    }


    public void steel11_IndexChanged(object sender, EventArgs e)
    {
        if (steel11.SelectedIndex != -1)
        {
            steel11_x.IsVisible = true;
            steel11_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel11Total, steel11, 0.02);
        }
        else
        {
            steel11_x.IsVisible = false;
            steel11_delete.IsVisible = false;
            steel11Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel11_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel11.SelectedItem.ToString()) * 0.02, 5);
        steel11.SelectedIndex = -1;
    }


    public void steel12_IndexChanged(object sender, EventArgs e)
    {
        if (steel12.SelectedIndex != -1)
        {
            steel12_x.IsVisible = true;
            steel12_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel12Total, steel12, 0.035);
        }
        else
        {
            steel12_x.IsVisible = false;
            steel12_delete.IsVisible = false;
            steel12Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel12_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel12.SelectedItem.ToString()) * 0.035, 5);
        steel12.SelectedIndex = -1;
    }


    public void steel13_IndexChanged(object sender, EventArgs e)
    {
        if (steel13.SelectedIndex != -1)
        {
            steel13_x.IsVisible = true;
            steel13_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(steel13Total, steel13, 0.053);
        }
        else
        {
            steel13_x.IsVisible = false;
            steel13_delete.IsVisible = false;
            steel13Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void steel13_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(steel13.SelectedItem.ToString()) * 0.053, 5);
        steel13.SelectedIndex = -1;
    }


    // copper ================================================================================================================================================================================================================

    public void copper1_IndexChanged(object sender, EventArgs e)
    {
        if (copper1.SelectedIndex != -1)
        {
            copper1_x.IsVisible = true;
            copper1_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper1Total, copper1, 0.00014);
        }
        else
        {
            copper1_x.IsVisible = false;
            copper1_delete.IsVisible = false;
            copper1Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper1_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper1.SelectedItem.ToString()) * 0.00014, 5);
        copper1.SelectedIndex = -1;
    }


    public void copper2_IndexChanged(object sender, EventArgs e)
    {
        if (copper2.SelectedIndex != -1)
        {
            copper2_x.IsVisible = true;
            copper2_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper2Total, copper2, 0.00032);
        }
        else
        {
            copper2_x.IsVisible = false;
            copper2_delete.IsVisible = false;
            copper2Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper2_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper2.SelectedItem.ToString()) * 0.00032, 5);
        copper2.SelectedIndex = -1;
    }


    public void copper3_IndexChanged(object sender, EventArgs e)
    {
        if (copper3.SelectedIndex != -1)
        {
            copper3_x.IsVisible = true;
            copper3_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper3Total, copper3, 0.00054);
        }
        else
        {
            copper3_x.IsVisible = false;
            copper3_delete.IsVisible = false;
            copper3Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper3_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper3.SelectedItem.ToString()) * 0.00054, 5);
        copper3.SelectedIndex = -1;
    }


    public void copper4_IndexChanged(object sender, EventArgs e)
    {
        if (copper4.SelectedIndex != -1)
        {
            copper4_x.IsVisible = true;
            copper4_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper4Total, copper4, 0.00084);
        }
        else
        {
            copper4_x.IsVisible = false;
            copper4_delete.IsVisible = false;
            copper4Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper4_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper4.SelectedItem.ToString()) * 0.00084, 5);
        copper4.SelectedIndex = -1;
    }


    public void copper5_IndexChanged(object sender, EventArgs e)
    {
        if (copper5.SelectedIndex != -1)
        {
            copper5_x.IsVisible = true;
            copper5_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper5Total, copper5, 0.0012);
        }
        else
        {
            copper5_x.IsVisible = false;
            copper5_delete.IsVisible = false;
            copper5Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper5_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper5.SelectedItem.ToString()) * 0.0012, 5);
        copper5.SelectedIndex = -1;
    }


    public void copper6_IndexChanged(object sender, EventArgs e)
    {
        if (copper6.SelectedIndex != -1)
        {
            copper6_x.IsVisible = true;
            copper6_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper6Total, copper6, 0.0021);
        }
        else
        {
            copper6_x.IsVisible = false;
            copper6_delete.IsVisible = false;
            copper6Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper6_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper6.SelectedItem.ToString()) * 0.0021, 5);
        copper6.SelectedIndex = -1;
    }


    public void copper7_IndexChanged(object sender, EventArgs e)
    {
        if (copper7.SelectedIndex != -1)
        {
            copper7_x.IsVisible = true;
            copper7_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(copper7Total, copper7, 0.0033);
        }
        else
        {
            copper7_x.IsVisible = false;
            copper7_delete.IsVisible = false;
            copper7Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void copper7_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(copper7.SelectedItem.ToString()) * 0.0033, 5);
        copper7.SelectedIndex = -1;
    }


    // PE SDR 11 ================================================================================================================================================================================================================

    public void pesdr1_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr1.SelectedIndex != -1)
        {
            pesdr1_x.IsVisible = true;
            pesdr1_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr1Total, pesdr1, 0.00019);
        }
        else
        {
            pesdr1_x.IsVisible = false;
            pesdr1_delete.IsVisible = false;
            pesdr1Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr1_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr1.SelectedItem.ToString()) * 0.00019, 5);
        pesdr1.SelectedIndex = -1;
    }


    public void pesdr2_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr2.SelectedIndex != -1)
        {
            pesdr2_x.IsVisible = true;
            pesdr2_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr2Total, pesdr2, 0.00033);
        }
        else
        {
            pesdr2_x.IsVisible = false;
            pesdr2_delete.IsVisible = false;
            pesdr2Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr2_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr2.SelectedItem.ToString()) * 0.00033, 5);
        pesdr2.SelectedIndex = -1;
    }


    public void pesdr3_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr3.SelectedIndex != -1)
        {
            pesdr3_x.IsVisible = true;
            pesdr3_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr3Total, pesdr3, 0.00053);
        }
        else
        {
            pesdr3_x.IsVisible = false;
            pesdr3_delete.IsVisible = false;
            pesdr3Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr3_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr3.SelectedItem.ToString()) * 0.00053, 5);
        pesdr3.SelectedIndex = -1;
    }


    public void pesdr4_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr4.SelectedIndex != -1)
        {
            pesdr4_x.IsVisible = true;
            pesdr4_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr4Total, pesdr4, 0.0016);
        }
        else
        {
            pesdr4_x.IsVisible = false;
            pesdr4_delete.IsVisible = false;
            pesdr4Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr4_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr4.SelectedItem.ToString()) * 0.0016, 5);
        pesdr4.SelectedIndex = -1;
    }


    public void pesdr5_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr5.SelectedIndex != -1)
        {
            pesdr5_x.IsVisible = true;
            pesdr5_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr5Total, pesdr5, 0.0021);
        }
        else
        {
            pesdr5_x.IsVisible = false;
            pesdr5_delete.IsVisible = false;
            pesdr5Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr5_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr5.SelectedItem.ToString()) * 0.0021, 5);
        pesdr5.SelectedIndex = -1;
    }


    public void pesdr6_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr6.SelectedIndex != -1)
        {
            pesdr6_x.IsVisible = true;
            pesdr6_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr6Total, pesdr6, 0.0029);
        }
        else
        {
            pesdr6_x.IsVisible = false;
            pesdr6_delete.IsVisible = false;
            pesdr6Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr6_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr6.SelectedItem.ToString()) * 0.0029, 5);
        pesdr6.SelectedIndex = -1;
    }


    public void pesdr7_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr7.SelectedIndex != -1)
        {
            pesdr7_x.IsVisible = true;
            pesdr7_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr7Total, pesdr7, 0.004);
        }
        else
        {
            pesdr7_x.IsVisible = false;
            pesdr7_delete.IsVisible = false;
            pesdr7Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr7_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr7.SelectedItem.ToString()) * 0.004, 5);
        pesdr7.SelectedIndex = -1;
    }


    public void pesdr8_IndexChanged(object sender, EventArgs e)
    {
        if (pesdr8.SelectedIndex != -1)
        {
            pesdr8_x.IsVisible = true;
            pesdr8_delete.IsVisible = true;

            UpdateTotalPipeworkVolume(pesdr8Total, pesdr8, 0.008);
        }
        else
        {
            pesdr8_x.IsVisible = false;
            pesdr8_delete.IsVisible = false;
            pesdr8Total.Text = null;

            SubtractTotalPipeworkVolume();
        }
    }
    public void pesdr8_Delete(object sender, EventArgs e)
    {
        pipeworkVolumeNumber = Math.Round(Int64.Parse(pesdr8.SelectedItem.ToString()) * 0.008, 5);
        pesdr8.SelectedIndex = -1;
    }

    // =======================================================================================================================================================================================

    public void checkAreasWithInadequateVentilationYes_CheckedChanged(object sender, EventArgs e)
    {
        if (checkAreasWithInadequateVentilationYes.IsChecked)
        {
            checkAreasWithInadequateVentilationNo.IsChecked = false;
            checkAreasWithInadequateVentilationNA.IsChecked = false;

            checkAreasWithInadequateVentilationNo.Color = Colors.White;
            checkAreasWithInadequateVentilationNA.Color = Colors.White;
            checkAreasWithInadequateVentilationYes.Color = Colors.Red;
        }
        else
            checkAreasWithInadequateVentilationYes.Color = Colors.White;
    }
    public void checkAreasWithInadequateVentilationNo_CheckedChanged(object sender, EventArgs e)
    {
        if (checkAreasWithInadequateVentilationNo.IsChecked)
        {
            checkAreasWithInadequateVentilationYes.IsChecked = false;
            checkAreasWithInadequateVentilationNA.IsChecked = false;

            checkAreasWithInadequateVentilationYes.Color = Colors.White;
            checkAreasWithInadequateVentilationNA.Color = Colors.White;
            checkAreasWithInadequateVentilationNo.Color = Colors.Red;
        }
        else
            checkAreasWithInadequateVentilationNo.Color = Colors.White;
    }
    public void checkAreasWithInadequateVentilationNA_CheckedChanged(object sender, EventArgs e)
    {
        if (checkAreasWithInadequateVentilationNA.IsChecked)
        {
            checkAreasWithInadequateVentilationNo.IsChecked = false;
            checkAreasWithInadequateVentilationYes.IsChecked = false;

            checkAreasWithInadequateVentilationNo.Color = Colors.White;
            checkAreasWithInadequateVentilationYes.Color = Colors.White;
            checkAreasWithInadequateVentilationNA.Color = Colors.Red;
        }
        else
            checkAreasWithInadequateVentilationNA.Color = Colors.White;
    }

    private async void stampAnimation(Image image)
    {
        var rotate = image.RotateTo(30, 350, Easing.Default);
        var scale = image.ScaleTo(0.85, 1000, Easing.BounceOut);
        var opacity = image.FadeTo(0.5, 1000, Easing.BounceOut);

        await Task.WhenAll(opacity, rotate, scale);

        await image.FadeTo(0.5, 2000);
        await image.FadeTo(0, 200);
    }
    private async void stampAnimationEnd(Image image)
    {
        await image.FadeTo(0, 0);
        await image.RotateTo(0, 0);
        await image.ScaleTo(1, 0);
    }
    public async void testPassedOrFailed_IndexChanged(object sender, EventArgs e)
    {
        if (testPassedOrFailed.SelectedIndex != -1)
        {
            testPassedOrFailed_x.IsVisible = true;
            testPassedOrFailed_delete.IsVisible = true;

            if (testPassedOrFailed.SelectedItem.ToString() == "PASS")
                stampAnimation(passStamp);
            else
                stampAnimation(failStamp);
        }
        else
        {
            testPassedOrFailed_x.IsVisible = false;
            testPassedOrFailed_delete.IsVisible = false;

            stampAnimationEnd(passStamp);
            stampAnimationEnd(failStamp);
        }
    }
    public void testPassedOrFailed_Delete(object sender, EventArgs e)
    {
        passStamp.CancelAnimations();
        failStamp.CancelAnimations();

        testPassedOrFailed.SelectedIndex = -1;
    }
}