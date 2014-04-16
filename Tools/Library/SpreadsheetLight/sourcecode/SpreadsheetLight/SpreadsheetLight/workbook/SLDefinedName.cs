using System;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SpreadsheetLight
{
    internal class SLDefinedName
    {
        internal string Text { get; set; }
        internal string Name { get; set; }
        internal string Comment { get; set; }
        internal string CustomMenu { get; set; }
        internal string Description { get; set; }
        internal string Help { get; set; }
        internal string StatusBar { get; set; }
        internal uint? LocalSheetId { get; set; }
        internal bool? Hidden { get; set; }
        internal bool? Function { get; set; }
        internal bool? VbProcedure { get; set; }
        internal bool? Xlm { get; set; }
        internal uint? FunctionGroupId { get; set; }
        internal string ShortcutKey { get; set; }
        internal bool? PublishToServer { get; set; }
        internal bool? WorkbookParameter { get; set; }

        internal SLDefinedName(string Name)
        {
            this.Text = string.Empty;
            this.Name = Name;
            this.SetAllNull();
        }

        private void SetAllNull()
        {
            this.Comment = null;
            this.CustomMenu = null;
            this.Description = null;
            this.Help = null;
            this.StatusBar = null;
            this.LocalSheetId = null;
            this.Hidden = null;
            this.Function = null;
            this.VbProcedure = null;
            this.Xlm = null;
            this.FunctionGroupId = null;
            this.ShortcutKey = null;
            this.PublishToServer = null;
            this.WorkbookParameter = null;
        }

        internal void FromDefinedName(DefinedName dn)
        {
            this.SetAllNull();
            this.Text = dn.Text ?? string.Empty;
            this.Name = dn.Name.Value;
            if (dn.Comment != null) this.Comment = dn.Comment.Value;
            if (dn.CustomMenu != null) this.CustomMenu = dn.CustomMenu.Value;
            if (dn.Description != null) this.Description = dn.Description.Value;
            if (dn.Help != null) this.Help = dn.Help.Value;
            if (dn.StatusBar != null) this.StatusBar = dn.StatusBar.Value;
            if (dn.LocalSheetId != null) this.LocalSheetId = dn.LocalSheetId.Value;
            if (dn.Hidden != null) this.Hidden = dn.Hidden.Value;
            if (dn.Function != null) this.Function = dn.Function.Value;
            if (dn.VbProcedure != null) this.VbProcedure = dn.VbProcedure.Value;
            if (dn.Xlm != null) this.Xlm = dn.Xlm.Value;
            if (dn.FunctionGroupId != null) this.FunctionGroupId = dn.FunctionGroupId.Value;
            if (dn.ShortcutKey != null) this.ShortcutKey = dn.ShortcutKey.Value;
            if (dn.PublishToServer != null) this.PublishToServer = dn.PublishToServer.Value;
            if (dn.WorkbookParameter != null) this.WorkbookParameter = dn.WorkbookParameter.Value;
        }

        internal DefinedName ToDefinedName()
        {
            DefinedName dn = new DefinedName();
            dn.Text = this.Text;
            dn.Name = this.Name;
            if (this.Comment != null) dn.Comment = this.Comment;
            if (this.CustomMenu != null) dn.CustomMenu = this.CustomMenu;
            if (this.Description != null) dn.Description = this.Description;
            if (this.Help != null) dn.Help = this.Help;
            if (this.StatusBar != null) dn.StatusBar = this.StatusBar;
            if (this.LocalSheetId != null) dn.LocalSheetId = this.LocalSheetId.Value;
            if (this.Hidden != null && this.Hidden != false) dn.Hidden = this.Hidden.Value;
            if (this.Function != null && this.Function != false) dn.Function = this.Function.Value;
            if (this.VbProcedure != null && this.VbProcedure != false) dn.VbProcedure = this.VbProcedure.Value;
            if (this.Xlm != null && this.Xlm != false) dn.Xlm = this.Xlm.Value;
            if (this.FunctionGroupId != null) dn.FunctionGroupId = this.FunctionGroupId.Value;
            if (this.ShortcutKey != null) dn.ShortcutKey = this.ShortcutKey;
            if (this.PublishToServer != null && this.PublishToServer != false) dn.PublishToServer = this.PublishToServer.Value;
            if (this.WorkbookParameter != null && this.WorkbookParameter != false) dn.WorkbookParameter = this.WorkbookParameter.Value;

            return dn;
        }

        internal SLDefinedName Clone()
        {
            SLDefinedName dn = new SLDefinedName(this.Name);
            dn.Text = this.Text;
            dn.Name = this.Name;
            dn.Comment = this.Comment;
            dn.CustomMenu = this.CustomMenu;
            dn.Description = this.Description;
            dn.Help = this.Help;
            dn.StatusBar = this.StatusBar;
            dn.LocalSheetId = this.LocalSheetId;
            dn.Hidden = this.Hidden;
            dn.Function = this.Function;
            dn.VbProcedure = this.VbProcedure;
            dn.Xlm = this.Xlm;
            dn.FunctionGroupId = this.FunctionGroupId;
            dn.ShortcutKey = this.ShortcutKey;
            dn.PublishToServer = this.PublishToServer;
            dn.WorkbookParameter = this.WorkbookParameter;

            return dn;
        }
    }
}
