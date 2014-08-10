#region Usings

using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using FSOManagement.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Launcher.Common.Util
{
    public static class ProfileManagerExtensions
    {
        public static IObservable<IProfile> GetProfileObservable(this IProfileManager profileManager)
        {
            if (profileManager == null || profileManager.CurrentProfileObservable == null)
            {
                return Observable.Empty<IProfile>();
            }
            else
            {
                return profileManager.CurrentProfileObservable;
            }
        }

        public static void CreateProfileBinding<TProfileProperty, TViewModel, TVmProperty>(this IProfileManager profileManager,
            Expression<Func<IProfile, TProfileProperty>> profileProperty, Func<TProfileProperty, TVmProperty> profileToVmConverter,
            TViewModel viewModel, Expression<Func<TViewModel, TVmProperty>> rightProperty, Func<TVmProperty, TProfileProperty> vmToProfileConverter)
        {
            IDisposable firstBinding = null;
            IDisposable secondBinding = null;

            profileManager.GetProfileObservable().Subscribe(profile =>
            {
                if (firstBinding != null)
                {
                    firstBinding.Dispose();
                }
                if (secondBinding != null)
                {
                    secondBinding.Dispose();
                }

                firstBinding = profile.WhenAny(profileProperty, val => profileToVmConverter(val.Value)).BindTo(viewModel, rightProperty);

                secondBinding = viewModel.WhenAny(rightProperty, val => vmToProfileConverter(val.Value)).BindTo(profile, profileProperty);
            });
        }

        public static void CreateProfileValueBinding<TProfileProperty, TViewModel, TVmProperty>(this IProfileManager profileManager,
            Expression<Func<IProfile, TProfileProperty>> profileProperty,
            TViewModel viewModel, Expression<Func<TViewModel, TVmProperty>> rightProperty)
        {
            IDisposable firstBinding = null;
            IDisposable secondBinding = null;

            profileManager.GetProfileObservable().Subscribe(profile =>
            {
                if (firstBinding != null)
                {
                    firstBinding.Dispose();
                }
                if (secondBinding != null)
                {
                    secondBinding.Dispose();
                }

                firstBinding = profile.WhenAnyValue(profileProperty).BindTo(viewModel, rightProperty);

                secondBinding = viewModel.WhenAnyValue(rightProperty).BindTo(profile, profileProperty);
            });
        }
    }
}
