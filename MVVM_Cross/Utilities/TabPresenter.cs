using MvvmCross;
using MvvmCross.Logging;
using MvvmCross.Platforms.Wpf.Presenters;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MVVM_Cross.Utilities
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MS-PL license.
    // See the LICENSE file in the project root for more information.

    public class TabPresenter
            : MvxAttributeViewPresenter, IMvxWpfViewPresenter
        {
            private IMvxWpfViewLoader _wpfViewLoader;
            protected IMvxWpfViewLoader WpfViewLoader
            {
                get
                {
                    if (_wpfViewLoader == null)
                        _wpfViewLoader = Mvx.IoCProvider.Resolve<IMvxWpfViewLoader>();
                    return _wpfViewLoader;
                }
            }

            private Dictionary<ContentControl, Stack<FrameworkElement>> _frameworkElementsDictionary;
            protected Dictionary<ContentControl, Stack<FrameworkElement>> FrameworkElementsDictionary
            {
                get
                {
                    if (_frameworkElementsDictionary == null)
                        _frameworkElementsDictionary = new Dictionary<ContentControl, Stack<FrameworkElement>>();
                    return _frameworkElementsDictionary;
                }
            }

            protected TabPresenter()
            {
            }

            public TabPresenter(ContentControl contentControl) // Accept ContentControl only for the first host view 
            {
                if (contentControl is Window window)
                    window.Closed += Window_Closed;

                FrameworkElementsDictionary.Add(contentControl, new Stack<FrameworkElement>());
            }

            public override void RegisterAttributeTypes()
            {
                AttributeTypesToActionsDictionary.Register<MvxWindowPresentationAttribute>(
                        (viewType, attribute, request) =>
                        {
                            var view = WpfViewLoader.CreateView(request);
                            return ShowWindow(view, (MvxWindowPresentationAttribute)attribute, request);
                        },
                        (viewModel, attribute) => CloseWindow(viewModel));

                AttributeTypesToActionsDictionary.Register<MvxContentPresentationAttribute>(
                        (viewType, attribute, request) =>
                        {
                            var view = WpfViewLoader.CreateView(request);
                            return ShowContentView(view, (MvxContentPresentationAttribute)attribute, request);
                        },
                        (viewModel, attribute) => CloseContentView(viewModel));
            }

            public override MvxBasePresentationAttribute CreatePresentationAttribute(Type viewModelType, Type viewType)
            {
                if (viewType.IsSubclassOf(typeof(Window)))
                {
                    return new MvxWindowPresentationAttribute();
                }
                return new MvxContentPresentationAttribute();
            }

            protected virtual Task<bool> ShowWindow(FrameworkElement element, MvxWindowPresentationAttribute attribute, MvxViewModelRequest request)
            {
                Window window;
                if (element is IMvxWindow mvxWindow)
                {
                    window = (Window)element;
                    mvxWindow.Identifier = attribute.Identifier ?? element.GetType().Name;
                }
                else if (element is Window normalWindow)
                {
                    // Accept normal Window class
                    window = normalWindow;
                }
                else
                {
                    // Wrap in window
                    window = new MvxWindow
                    {
                        Identifier = attribute.Identifier ?? element.GetType().Name
                    };
                }
                window.Closed += Window_Closed;
                FrameworkElementsDictionary.Add(window, new Stack<FrameworkElement>());

                if (!(element is Window))
                {
                    FrameworkElementsDictionary[window].Push(element);
                    window.Content = element;
                }

                if (attribute.Modal)
                    window.ShowDialog();
                else
                    window.Show();
                return Task.FromResult(true);
            }

            private void Window_Closed(object sender, EventArgs e)
            {
                var window = sender as Window;
                window.Closed -= Window_Closed;

                if (FrameworkElementsDictionary.ContainsKey(window))
                    FrameworkElementsDictionary.Remove(window);
            }

            protected virtual Task<bool> ShowContentView(FrameworkElement element, MvxContentPresentationAttribute attribute, MvxViewModelRequest request)
            {
                var contentControl = FrameworkElementsDictionary.Keys.FirstOrDefault(w => (w as MvxWindow)?.Identifier == attribute.WindowIdentifier) ?? FrameworkElementsDictionary.Keys.Last();

                if (!attribute.StackNavigation && FrameworkElementsDictionary[contentControl].Any())
                    FrameworkElementsDictionary[contentControl].Pop(); // Close previous view

                FrameworkElementsDictionary[contentControl].Push(element);
                contentControl.Content = element;
                return Task.FromResult(true);
            }

            public override async Task<bool> Close(IMvxViewModel toClose)
            {
                // toClose is window
                if (FrameworkElementsDictionary.Any(i => (i.Key as IMvxWpfView)?.ViewModel == toClose) && await CloseWindow(toClose))
                    return true;

                // toClose is content
                if (FrameworkElementsDictionary.Any(i => i.Value.Any() && (i.Value.Peek() as IMvxWpfView)?.ViewModel == toClose) && await CloseContentView(toClose))
                    return true;

                return false;
            }

            protected virtual Task<bool> CloseWindow(IMvxViewModel toClose)
            {
                var item = FrameworkElementsDictionary.FirstOrDefault(i => (i.Key as IMvxWpfView)?.ViewModel == toClose);
                var contentControl = item.Key;
                if (contentControl is Window window)
                {
                    FrameworkElementsDictionary.Remove(window);
                    window.Close();
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }

            protected virtual Task<bool> CloseContentView(IMvxViewModel toClose)
            {
                var item = FrameworkElementsDictionary.FirstOrDefault(i => i.Value.Any() && (i.Value.Peek() as IMvxWpfView)?.ViewModel == toClose);
                var contentControl = item.Key;
                var elements = item.Value;

                if (elements.Any())
                    elements.Pop(); // Pop closing view

                if (elements.Any())
                {
                    contentControl.Content = elements.Peek();
                    return Task.FromResult(true);
                }

                // Close window if no contents
                if (contentControl is Window window)
                {
                    FrameworkElementsDictionary.Remove(window);
                    window.Close();
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
}
