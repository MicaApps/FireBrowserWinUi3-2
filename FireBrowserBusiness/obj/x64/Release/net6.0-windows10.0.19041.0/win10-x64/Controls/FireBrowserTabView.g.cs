﻿#pragma checksum "C:\Users\JPJM-\source\repos\Browser\FireBrowserWinUi3\FireBrowserBusiness\Controls\FireBrowserTabView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "381F81BDB3BD2D2B9700302149E22C528C5CF332679DDD03D29AA82BE5DEC7DB"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FireBrowserBusiness.Controls
{
    partial class FireBrowserTabView : 
        global::Microsoft.UI.Xaml.Controls.TabView, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2309")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Microsoft_UI_Xaml_FrameworkElement_Style(global::Microsoft.UI.Xaml.FrameworkElement obj, global::Microsoft.UI.Xaml.Style value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Microsoft.UI.Xaml.Style) global::Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Microsoft.UI.Xaml.Style), targetNullValue);
                }
                obj.Style = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2309")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class FireBrowserTabView_obj1_Bindings :
            global::Microsoft.UI.Xaml.Markup.IComponentConnector,
            IFireBrowserTabView_Bindings
        {
            private global::FireBrowserBusiness.Controls.FireBrowserTabView dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::System.WeakReference obj1;

            private FireBrowserTabView_obj1_BindingsTracking bindingsTracking;

            public FireBrowserTabView_obj1_Bindings()
            {
                this.bindingsTracking = new FireBrowserTabView_obj1_BindingsTracking(this);
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 1: // Controls\FireBrowserTabView.xaml line 1
                        this.obj1 = new global::System.WeakReference(global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TabView>(target));
                        this.bindingsTracking.RegisterTwoWayListener_1((this.obj1.Target as global::Microsoft.UI.Xaml.Controls.TabView));
                        break;
                    default:
                        break;
                }
            }
                        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2309")]
                        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
                        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target) 
                        {
                            return null;
                        }

            // IFireBrowserTabView_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = global::WinRT.CastExtensions.As<global::FireBrowserBusiness.Controls.FireBrowserTabView>(newDataRoot);
                    return true;
                }
                return false;
            }

            public void Activated(object obj, global::Microsoft.UI.Xaml.WindowActivatedEventArgs data)
            {
                this.Initialize();
            }

            public void Loading(global::Microsoft.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::FireBrowserBusiness.Controls.FireBrowserTabView obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_ViewModel(obj.ViewModel, phase);
                    }
                }
            }
            private void Update_ViewModel(global::FireBrowserBusiness.Controls.FireBrowserTabView.FireBrowserTabViewViewModel obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_ViewModel(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_ViewModel_Style(obj.Style, phase);
                    }
                }
            }
            private void Update_ViewModel_Style(global::Microsoft.UI.Xaml.Style obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // Controls\FireBrowserTabView.xaml line 1
                    if ((this.obj1.Target as global::Microsoft.UI.Xaml.Controls.TabView) != null)
                    {
                        XamlBindingSetters.Set_Microsoft_UI_Xaml_FrameworkElement_Style((this.obj1.Target as global::Microsoft.UI.Xaml.Controls.TabView), obj, null);
                    }
                }
            }
            private void UpdateTwoWay_1_Style()
            {
                if (this.initialized)
                {
                    if (this.dataRoot != null)
                    {
                        if (this.dataRoot.ViewModel != null)
                        {
                            this.dataRoot.ViewModel.Style = (this.obj1.Target as global::Microsoft.UI.Xaml.Controls.TabView).Style;
                        }
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2309")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class FireBrowserTabView_obj1_BindingsTracking
            {
                private global::System.WeakReference<FireBrowserTabView_obj1_Bindings> weakRefToBindingObj; 

                public FireBrowserTabView_obj1_BindingsTracking(FireBrowserTabView_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<FireBrowserTabView_obj1_Bindings>(obj);
                }

                public FireBrowserTabView_obj1_Bindings TryGetBindingObject()
                {
                    FireBrowserTabView_obj1_Bindings bindingObject = null;
                    if (weakRefToBindingObj != null)
                    {
                        weakRefToBindingObj.TryGetTarget(out bindingObject);
                        if (bindingObject == null)
                        {
                            weakRefToBindingObj = null;
                            ReleaseAllListeners();
                        }
                    }
                    return bindingObject;
                }

                public void ReleaseAllListeners()
                {
                    UpdateChildListeners_ViewModel(null);
                }

                public void PropertyChanged_ViewModel(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    FireBrowserTabView_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::FireBrowserBusiness.Controls.FireBrowserTabView.FireBrowserTabViewViewModel obj = sender as global::FireBrowserBusiness.Controls.FireBrowserTabView.FireBrowserTabViewViewModel;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                            if (obj != null)
                            {
                                bindings.Update_ViewModel_Style(obj.Style, DATA_CHANGED);
                            }
                        }
                        else
                        {
                            switch (propName)
                            {
                                case "Style":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_ViewModel_Style(obj.Style, DATA_CHANGED);
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                    }
                }
                private global::FireBrowserBusiness.Controls.FireBrowserTabView.FireBrowserTabViewViewModel cache_ViewModel = null;
                public void UpdateChildListeners_ViewModel(global::FireBrowserBusiness.Controls.FireBrowserTabView.FireBrowserTabViewViewModel obj)
                {
                    if (obj != cache_ViewModel)
                    {
                        if (cache_ViewModel != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)cache_ViewModel).PropertyChanged -= PropertyChanged_ViewModel;
                            cache_ViewModel = null;
                        }
                        if (obj != null)
                        {
                            cache_ViewModel = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_ViewModel;
                        }
                    }
                }
                public void RegisterTwoWayListener_1(global::Microsoft.UI.Xaml.Controls.TabView sourceObject)
                {
                    sourceObject.RegisterPropertyChangedCallback(global::Microsoft.UI.Xaml.FrameworkElement.StyleProperty, (sender, prop) =>
                    {
                        var bindingObj = this.TryGetBindingObject();
                        if (bindingObj != null)
                        {
                            bindingObj.UpdateTwoWay_1_Style();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2309")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2309")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // Controls\FireBrowserTabView.xaml line 1
                {                    
                    global::Microsoft.UI.Xaml.Controls.TabView element1 = (global::Microsoft.UI.Xaml.Controls.TabView)target;
                    FireBrowserTabView_obj1_Bindings bindings = new FireBrowserTabView_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            }
            return returnValue;
        }
    }
}

