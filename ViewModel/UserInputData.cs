using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ViewModel
{
	public class UserInputData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		private string startingPoint;
		public string StartingPoint
		{
			get => this.startingPoint; set
			{
				this.startingPoint = value;
				this.RaisePropertyChanged(nameof(this.StartingPoint));
			}
		}

		private string endingPoint;
		public string EndingPoint
		{
			get => this.endingPoint;
			set
			{
				this.endingPoint = value;
				this.RaisePropertyChanged(nameof(this.EndingPoint));
			}
		}


		private string additionalDistanceKm;
		public string AdditionalDistanceKm
		{
			get => this.additionalDistanceKm;
			set
			{
				this.additionalDistanceKm = value;
				this.RaisePropertyChanged(nameof(this.AdditionalDistanceKm));
			}
		}

		private string additionalTimeMin;
		public string AdditionalTimeMin
		{
			get => this.additionalTimeMin;
			set
			{
				this.additionalTimeMin = value;
				this.RaisePropertyChanged(nameof(additionalTimeMin));
			}
		}


	}
}
