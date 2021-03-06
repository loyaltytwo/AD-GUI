﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Active_Directory_Management
{
	public partial class BirthdayPicker : UserControl
	{
		public BirthdayPicker()
		{
			InitializeComponent();

			for (int i = 1; i <= 31; i++)
				dayBox.Items.Add(i.ToString().PadLeft(2, '0'));

            for (int i = 1; i <= 12; i++)
                monthBox.Items.Add(i.ToString().PadLeft(2, '0'));

			for(int i = 16; i <= 90; i++)
				yearBox.Items.Add(DateTime.Today.Year - i);

			Clear();
		}

		public DateTime Value
		{
			get
			{
				try
				{
					return new DateTime(
						int.Parse(yearBox.Text),
						int.Parse(monthBox.Text),
						int.Parse(dayBox.Text));
				}
				catch
				{
					return DateTime.MinValue;
				}
			}
			set
			{
				if (value == DateTime.MinValue)
					Clear();

				else
				{
					dayBox.Text = value.Day.ToString().PadLeft(2, '0');
					monthBox.Text = value.Month.ToString().PadLeft(2, '0');
					yearBox.Text = value.Year.ToString();
				}
			}
		}

		public void Clear()
		{
			dayBox.Text = string.Empty;
			monthBox.Text = string.Empty;
			yearBox.Text = string.Empty;
		}


		override public Color ForeColor
		{
			get
			{
				return dayBox.ForeColor;
			}
			set
			{
				dayBox.ForeColor = value;
				monthBox.ForeColor = value;
				yearBox.ForeColor = value;
			}
		}

	}
}
