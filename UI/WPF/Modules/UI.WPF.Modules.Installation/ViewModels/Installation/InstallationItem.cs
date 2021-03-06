﻿#region Usings

using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FSOManagement.Annotations;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public abstract class InstallationItem : ReactiveObjectBase
    {
        private bool _indeterminate;

        private bool _installationInProgress;

        private string _operationMessage;

        private double _progress;

        private InstallationResult _result;

        private string _resultMessage;

        private string _title;

        protected InstallationItem()
        {
            ProgressObservable = this.WhenAnyValue(x => x.Progress);
            IndeterminateObservable = this.WhenAnyValue(x => x.Indeterminate);
            ResultObservable = this.WhenAnyValue(x => x.Result);

            // When the result changes set the message to null, if there is a special message the implementing class will set that
            ResultObservable.Subscribe(_ => ResultMessage = null);

            ProgressObservable.Select(p => p < 0.999).BindTo(this, x => x.InstallationInProgress);

            var cancelCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.InstallationInProgress));
            cancelCommand.Subscribe(_ =>
            {
                if (CancellationTokenSource != null)
                {
                    CancellationTokenSource.Cancel();
                }
            });

            CancelCommand = cancelCommand;
        }

        public ICommand CancelCommand { get; private set; }

        [CanBeNull]
        public CancellationTokenSource CancellationTokenSource { get; protected set; }

        public IObservable<InstallationResult> ResultObservable { get; private set; }

        public InstallationResult Result
        {
            get { return _result; }
            protected set { RaiseAndSetIfPropertyChanged(ref _result, value); }
        }

        public string ResultMessage
        {
            get { return _resultMessage; }
            protected set { RaiseAndSetIfPropertyChanged(ref _resultMessage, value); }
        }

        public bool InstallationInProgress
        {
            get { return _installationInProgress; }
            private set { RaiseAndSetIfPropertyChanged(ref _installationInProgress, value); }
        }

        public string Title
        {
            get { return _title; }
            protected set { RaiseAndSetIfPropertyChanged(ref _title, value); }
        }

        public string OperationMessage
        {
            get { return _operationMessage; }
            protected set { RaiseAndSetIfPropertyChanged(ref _operationMessage, value); }
        }

        public double Progress
        {
            get { return _progress; }
            protected set { RaiseAndSetIfPropertyChanged(ref _progress, value); }
        }

        public IObservable<double> ProgressObservable { get; private set; }

        public bool Indeterminate
        {
            get { return _indeterminate; }
            protected set { RaiseAndSetIfPropertyChanged(ref _indeterminate, value); }
        }

        public IObservable<bool> IndeterminateObservable { get; private set; }

        public abstract Task Install();
    }
}
