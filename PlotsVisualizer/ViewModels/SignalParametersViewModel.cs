﻿using SignalProcessing;
using System;
using System.Windows;
using UILogic.Base;

namespace PlotsVisualizer.ViewModels
{
    public class SignalParametersViewModel : BindableBase
    {
        private readonly Types.Complex _mean;
        private readonly Types.Complex _meanAbs;
        private readonly Types.Complex _meanPower;
        private readonly Types.Complex _effectiveValue;
        private readonly Types.Complex _variance;
        private string _meanText;
        private string _meanAbsText;
        private string _meanPowerText;
        private string _effectiveValueText;
        private string _varianceText;

        public SignalParametersViewModel(Types.Signal signal)
        {
            Signal = signal ?? throw new ArgumentNullException(nameof(signal));
            _mean = Statistics.meanValue(signal.points);
            _meanAbs = Statistics.meanAbsValue(signal.points);
            _meanPower = Statistics.meanPower(signal.points);
            _effectiveValue = Statistics.effectiveValue(signal.points);
            _variance = Statistics.variance(signal.points);

            FormatCommand = new RelayCommand(FormatValues);

            FormatValues();
        }

        public IRaiseCanExecuteCommand FormatCommand { get; }

        public Types.Signal Signal { get; }

        public string Mean
        {
            get => _meanText;
            private set => SetProperty(ref _meanText, value);
        }

        public string MeanAbs
        {
            get => _meanAbsText;
            private set => SetProperty(ref _meanAbsText, value);
        }

        public string MeanPower
        {
            get => _meanPowerText;
            private set => SetProperty(ref _meanPowerText, value);
        }

        public string EffectiveValue
        {
            get => _effectiveValueText;
            private set => SetProperty(ref _effectiveValueText, value);
        }

        public string Variance
        {
            get => _varianceText;
            private set => SetProperty(ref _varianceText, value);
        }

        public uint FormatPrecision { get; set; } = 4;

        private void FormatValues()
        {
            if (FormatPrecision == 0 || FormatPrecision > 10)
            {
                MessageBox.Show("Precision has to be integer from 1-9");
                return;
            }

            Mean = _mean.ToString($"F{FormatPrecision}");
            MeanAbs = _meanAbs.ToString($"F{FormatPrecision}");
            MeanPower = _meanPower.ToString($"F{FormatPrecision}");
            EffectiveValue = _effectiveValue.ToString($"F{FormatPrecision}");
            Variance = _variance.ToString($"F{FormatPrecision}");
        }
    }
}