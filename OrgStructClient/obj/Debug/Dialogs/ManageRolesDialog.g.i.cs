﻿#pragma checksum "..\..\..\Dialogs\ManageRolesDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "060908415F3F13D8CBB60BA83D61B798377C569038198636B94F63D1CA3FD2B0"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using OrgStructClient;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace OrgStructClient {
    
    
    /// <summary>
    /// ManageRolesDialog
    /// </summary>
    public partial class ManageRolesDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\Dialogs\ManageRolesDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lsvOrgRoles;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\Dialogs\ManageRolesDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRoleNew;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\Dialogs\ManageRolesDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRoleEdit;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Dialogs\ManageRolesDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRoleDelete;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/OrgStructClient;component/dialogs/managerolesdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\ManageRolesDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.lsvOrgRoles = ((System.Windows.Controls.ListView)(target));
            return;
            case 2:
            this.btnRoleNew = ((System.Windows.Controls.Button)(target));
            
            #line 28 "..\..\..\Dialogs\ManageRolesDialog.xaml"
            this.btnRoleNew.Click += new System.Windows.RoutedEventHandler(this.btnRoleNew_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.btnRoleEdit = ((System.Windows.Controls.Button)(target));
            
            #line 29 "..\..\..\Dialogs\ManageRolesDialog.xaml"
            this.btnRoleEdit.Click += new System.Windows.RoutedEventHandler(this.btnRoleEdit_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnRoleDelete = ((System.Windows.Controls.Button)(target));
            
            #line 30 "..\..\..\Dialogs\ManageRolesDialog.xaml"
            this.btnRoleDelete.Click += new System.Windows.RoutedEventHandler(this.btnRoleDelete_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

