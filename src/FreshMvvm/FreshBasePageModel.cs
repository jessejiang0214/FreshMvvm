﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FreshMvvm.Extensions;
using System.Reactive;

namespace FreshMvvm
{
    public abstract class FreshBasePageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This event is raise when a page is Popped, this might not be raise everytime a page is Popped. 
        /// Note* this might be raised multiple times. 
        /// </summary>
        public event EventHandler PageWasPopped; 

        /// <summary>
        /// This property is used by the FreshBaseContentPage and allows you to set the toolbar items on the page.
        /// </summary>
        public ObservableCollection<ToolbarItem> ToolbarItems { get; set; }

        /// <summary>
        /// The previous page model, that's automatically filled, on push
        /// </summary>
        public FreshBasePageModel PreviousPageModel { get; set; }

        /// <summary>
        /// A reference to the current page, that's automatically filled, on push
        /// </summary>
        public Page CurrentPage { get; set; }

        /// <summary>
        /// Core methods are basic built in methods for the App including Pushing, Pop and Alert
        /// </summary>
        public IPageModelCoreMethods CoreMethods { get; set; }

        /// <summary>
        /// This method is called when a page is Pop'd, it also allows for data to be returned.
        /// </summary>
        /// <param name="returnedData">This data that's returned from </param>
        public virtual void ReverseInit (object returnedData)
        {
        }

        /// <summary>
        /// This method is called when the PageModel is loaded, the initData is the data that's sent from pagemodel before
        /// </summary>
        /// <param name="initData">Data that's sent to this PageModel from the pusher</param>
        public virtual void Init (object initData)
        {            
        }

        protected void RaisePropertyChanged ([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) {
                handler (this, new PropertyChangedEventArgs (propertyName));
            }
        }

        internal void WireEvents (Page page)
        {
            page.ObserveAppearing().SubscribeWeakly(this, ViewIsAppearingStatic);
            page.ObserveDisappearing().SubscribeWeakly(this, ViewIsDisappearingStatic);
        }

        /// <summary>
        /// Is true when this model is the first of a new navigation stack
        /// </summary>
        internal bool IsModalFirstChild;

        /// <summary>
        /// Used when a page is shown modal and wants a new Navigation Stack
        /// </summary>
        public string PreviousNavigationServiceName;

        /// <summary>
        /// Used when a page is shown modal and wants a new Navigation Stack
        /// </summary>
        public string CurrentNavigationServiceName = Constants.DefaultNavigationServiceName;

        /// <summary>
        /// This means the current PageModel is shown modally and can be pop'd modally
        /// </summary>
        public bool IsModalAndHasPreviousNavigationStack()
        {
            return !string.IsNullOrWhiteSpace (PreviousNavigationServiceName) && PreviousNavigationServiceName != CurrentNavigationServiceName;
        }

        static void ViewIsDisappearingStatic(FreshBasePageModel sender, EventPattern<EventArgs> e)
        {
            sender.ViewIsDisappearing(sender, e.EventArgs);
        }

        /// <summary>
        /// This method is called when the view is disappearing. 
        /// </summary>
        protected virtual void ViewIsDisappearing (object sender, EventArgs e)
        {

        }

        static void ViewIsAppearingStatic(FreshBasePageModel sender, EventPattern<EventArgs> e)
        {
            sender.ViewIsAppearing(sender, e.EventArgs);
        }

        /// <summary>
        /// This methods is called when the View is appearing
        /// </summary>
        protected virtual void ViewIsAppearing (object sender, EventArgs e)
        {
            if (!_alreadyAttached)
                AttachPageWasPoppedEvent();
        }

        bool _alreadyAttached = false;
        /// <summary>
        /// This is used to attach the page was popped method to a NavigationPage if available
        /// </summary>
        void AttachPageWasPoppedEvent()
        {
            var navPage = (this.CurrentPage.Parent as NavigationPage);
            if (navPage != null)
            {
                _alreadyAttached = true;
                navPage.ObservePopped().SubscribeWeakly(this, HandleNavPagePoppedStatic);
            }
        }

        static void HandleNavPagePoppedStatic(FreshBasePageModel sender, EventPattern<NavigationEventArgs> e)
        {
            sender.HandleNavPagePopped(sender, e.EventArgs);
        }

        private void HandleNavPagePopped(object sender, NavigationEventArgs e)
        {
            if (e.Page == this.CurrentPage)
            {
                RaisePageWasPopped();
            }
        }

        public void RaisePageWasPopped()
        {
            if (PageWasPopped != null)
                PageWasPopped(this, EventArgs.Empty);
            
            CurrentPage.BindingContext = null;
        }
    }
}

