#region Usings

using System;
using System.Collections.Generic;
using FSOManagement.Annotations;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.Services
{
    internal class UpdateProgress : IUpdateProgress
    {
        public UpdateProgress(double progress, UpdateState state)
        {
            Progress = progress;
            State = state;
        }

        private UpdateProgress()
        {
        }

        #region IUpdateProgress Members

        public double Progress { get; private set; }

        public UpdateState State { get; private set; }

        public IDictionary<Version, string> ReleaseNotes { get; private set; }

        #endregion

        [NotNull]
        public static UpdateProgress Finished([NotNull] IDictionary<Version, string> releaseNotes)
        {
            return new UpdateProgress
            {
                State = UpdateState.Finished,
                ReleaseNotes = releaseNotes
            };
        }
    }
}
