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
				this.RaisePropertyChanged(nameof(this.additionalTimeMin));
			}
		}

		private bool useAggregatedPoints;
		public bool UseAggregatedPoints
		{
			get => this.useAggregatedPoints;
			set
			{
				this.useAggregatedPoints = value;
				this.RaisePropertyChanged(nameof(this.UseAggregatedPoints));
			}
		}

		private string processingTime;
		public string ProcessingTime
		{
			get => this.processingTime;
			set
			{
				this.processingTime = value;
				this.RaisePropertyChanged(nameof(this.ProcessingTime));
			}
		}

		private string referenceDistanceKm;
		public string ReferenceDistanceKm
		{
			get => this.referenceDistanceKm;
			set
			{
				this.referenceDistanceKm = value;
				this.RaisePropertyChanged(nameof(this.ReferenceDistanceKm));
			}
		}

		private string referenceTimeHMin;
		public string ReferenceTimeHMin
		{
			get => this.referenceTimeHMin;
			set
			{
				this.referenceTimeHMin = value;
				this.RaisePropertyChanged(nameof(this.ReferenceTimeHMin));
			}
		}

		private string resultDistanceKm;
		public string ResultDistanceKm
		{
			get => this.resultDistanceKm;
			set
			{
				this.resultDistanceKm = value;
				this.RaisePropertyChanged(nameof(this.ResultDistanceKm));
			}
		}

		private string resultTimeHMin;
		public string ResultTimeHMin
		{
			get => this.resultTimeHMin;
			set
			{
				this.resultTimeHMin = value;
				this.RaisePropertyChanged(nameof(this.ResultTimeHMin));
			}
		}

		private string resultAdditionalStops;
		public string ResultAdditionalStops
		{
			get => this.resultAdditionalStops;
			set
			{
				this.resultAdditionalStops = value;
				this.RaisePropertyChanged(nameof(this.ResultAdditionalStops));
			}
		}

	}
}
