﻿using System;

namespace SpreadsheetLight
{
    /// <summary>
    /// Simple settings for themes.
    /// </summary>
    public class SLThemeSettings
    {
        /// <summary>
        /// The theme name.
        /// </summary>
        public string ThemeName { get; set; }
        
        /// <summary>
        /// The major latin font.
        /// </summary>
        public string MajorLatinFont { get; set; }
        
        /// <summary>
        /// The minor latin font.
        /// </summary>
        public string MinorLatinFont { get; set; }

        /// <summary>
        /// Typically pure black.
        /// </summary>
        public System.Drawing.Color Dark1Color { get; set; }
        
        /// <summary>
        /// Typically pure white.
        /// </summary>
        public System.Drawing.Color Light1Color { get; set; }

        /// <summary>
        /// A dark color that still has visual contrast against light tints of the accent colors.
        /// </summary>
        public System.Drawing.Color Dark2Color { get; set; }

        /// <summary>
        /// A light color that still has visual contrast against dark tints of the accent colors.
        /// </summary>
        public System.Drawing.Color Light2Color { get; set; }

        /// <summary>
        /// Accent1 color.
        /// </summary>
        public System.Drawing.Color Accent1Color { get; set; }

        /// <summary>
        /// Accent2 color.
        /// </summary>
        public System.Drawing.Color Accent2Color { get; set; }

        /// <summary>
        /// Accent3 color.
        /// </summary>
        public System.Drawing.Color Accent3Color { get; set; }

        /// <summary>
        /// Accent4 color.
        /// </summary>
        public System.Drawing.Color Accent4Color { get; set; }

        /// <summary>
        /// Accent5 color.
        /// </summary>
        public System.Drawing.Color Accent5Color { get; set; }

        /// <summary>
        /// Accent6 color.
        /// </summary>
        public System.Drawing.Color Accent6Color { get; set; }

        /// <summary>
        /// Color of a hyperlink.
        /// </summary>
        public System.Drawing.Color Hyperlink { get; set; }

        /// <summary>
        /// Color of a followed hyperlink.
        /// </summary>
        public System.Drawing.Color FollowedHyperlinkColor { get; set; }

        /// <summary>
        /// Initialize an instance of SLThemeSettings.
        /// </summary>
        public SLThemeSettings()
        {
            this.SetTheme(SLThemeTypeValues.Office);
            this.ThemeName = "SpreadsheetLight Custom";
        }

        /// <summary>
        /// Initialize an instance of SLThemeSettings with a given theme.
        /// </summary>
        /// <param name="ThemeType"></param>
        public SLThemeSettings(SLThemeTypeValues ThemeType)
        {
            this.SetTheme(ThemeType);
        }

        private void SetTheme(SLThemeTypeValues ThemeType)
        {
            switch (ThemeType)
            {
                case SLThemeTypeValues.Office:
                    this.ThemeName = SLConstants.OfficeThemeName;
                    this.MajorLatinFont = SLConstants.OfficeThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.OfficeThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.OfficeThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.OfficeThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.OfficeThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.OfficeThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.OfficeThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.OfficeThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.OfficeThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.OfficeThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.OfficeThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.OfficeThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.OfficeThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.OfficeThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Adjacency:
                    this.ThemeName = SLConstants.AdjacencyThemeName;
                    this.MajorLatinFont = SLConstants.AdjacencyThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.AdjacencyThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.AdjacencyThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.AdjacencyThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.AdjacencyThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.AdjacencyThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.AdjacencyThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.AdjacencyThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.AdjacencyThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.AdjacencyThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.AdjacencyThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.AdjacencyThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.AdjacencyThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.AdjacencyThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Angles:
                    this.ThemeName = SLConstants.AnglesThemeName;
                    this.MajorLatinFont = SLConstants.AnglesThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.AnglesThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.AnglesThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.AnglesThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.AnglesThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.AnglesThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.AnglesThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.AnglesThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.AnglesThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.AnglesThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.AnglesThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.AnglesThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.AnglesThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.AnglesThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Apex:
                    this.ThemeName = SLConstants.ApexThemeName;
                    this.MajorLatinFont = SLConstants.ApexThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ApexThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ApexThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ApexThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ApexThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ApexThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ApexThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ApexThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ApexThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ApexThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ApexThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ApexThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ApexThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ApexThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Apothecary:
                    this.ThemeName = SLConstants.ApothecaryThemeName;
                    this.MajorLatinFont = SLConstants.ApothecaryThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ApothecaryThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ApothecaryThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ApothecaryThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ApothecaryThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ApothecaryThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ApothecaryThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ApothecaryThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ApothecaryThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ApothecaryThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ApothecaryThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ApothecaryThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ApothecaryThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ApothecaryThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Aspect:
                    this.ThemeName = SLConstants.AspectThemeName;
                    this.MajorLatinFont = SLConstants.AspectThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.AspectThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.AspectThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.AspectThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.AspectThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.AspectThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.AspectThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.AspectThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.AspectThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.AspectThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.AspectThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.AspectThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.AspectThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.AspectThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Austin:
                    this.ThemeName = SLConstants.AustinThemeName;
                    this.MajorLatinFont = SLConstants.AustinThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.AustinThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.AustinThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.AustinThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.AustinThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.AustinThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.AustinThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.AustinThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.AustinThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.AustinThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.AustinThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.AustinThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.AustinThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.AustinThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.BlackTie:
                    this.ThemeName = SLConstants.BlackTieThemeName;
                    this.MajorLatinFont = SLConstants.BlackTieThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.BlackTieThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.BlackTieThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.BlackTieThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.BlackTieThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.BlackTieThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.BlackTieThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.BlackTieThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.BlackTieThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.BlackTieThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.BlackTieThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.BlackTieThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.BlackTieThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.BlackTieThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Civic:
                    this.ThemeName = SLConstants.CivicThemeName;
                    this.MajorLatinFont = SLConstants.CivicThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.CivicThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.CivicThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.CivicThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.CivicThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.CivicThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.CivicThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.CivicThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.CivicThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.CivicThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.CivicThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.CivicThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.CivicThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.CivicThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Clarity:
                    this.ThemeName = SLConstants.ClarityThemeName;
                    this.MajorLatinFont = SLConstants.ClarityThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ClarityThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ClarityThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ClarityThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ClarityThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ClarityThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ClarityThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ClarityThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ClarityThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ClarityThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ClarityThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ClarityThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ClarityThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ClarityThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Composite:
                    this.ThemeName = SLConstants.CompositeThemeName;
                    this.MajorLatinFont = SLConstants.CompositeThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.CompositeThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.CompositeThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.CompositeThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.CompositeThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.CompositeThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.CompositeThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.CompositeThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.CompositeThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.CompositeThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.CompositeThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.CompositeThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.CompositeThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.CompositeThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Concourse:
                    this.ThemeName = SLConstants.ConcourseThemeName;
                    this.MajorLatinFont = SLConstants.ConcourseThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ConcourseThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ConcourseThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ConcourseThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ConcourseThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ConcourseThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ConcourseThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ConcourseThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ConcourseThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ConcourseThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ConcourseThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ConcourseThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ConcourseThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ConcourseThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Couture:
                    this.ThemeName = SLConstants.CoutureThemeName;
                    this.MajorLatinFont = SLConstants.CoutureThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.CoutureThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.CoutureThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.CoutureThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.CoutureThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.CoutureThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.CoutureThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.CoutureThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.CoutureThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.CoutureThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.CoutureThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.CoutureThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.CoutureThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.CoutureThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Elemental:
                    this.ThemeName = SLConstants.ElementalThemeName;
                    this.MajorLatinFont = SLConstants.ElementalThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ElementalThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ElementalThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ElementalThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ElementalThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ElementalThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ElementalThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ElementalThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ElementalThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ElementalThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ElementalThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ElementalThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ElementalThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ElementalThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Equity:
                    this.ThemeName = SLConstants.EquityThemeName;
                    this.MajorLatinFont = SLConstants.EquityThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.EquityThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.EquityThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.EquityThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.EquityThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.EquityThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.EquityThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.EquityThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.EquityThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.EquityThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.EquityThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.EquityThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.EquityThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.EquityThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Essential:
                    this.ThemeName = SLConstants.EssentialThemeName;
                    this.MajorLatinFont = SLConstants.EssentialThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.EssentialThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.EssentialThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.EssentialThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.EssentialThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.EssentialThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.EssentialThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.EssentialThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.EssentialThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.EssentialThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.EssentialThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.EssentialThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.EssentialThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.EssentialThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Executive:
                    this.ThemeName = SLConstants.ExecutiveThemeName;
                    this.MajorLatinFont = SLConstants.ExecutiveThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ExecutiveThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ExecutiveThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ExecutiveThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ExecutiveThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ExecutiveThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ExecutiveThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ExecutiveThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ExecutiveThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ExecutiveThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ExecutiveThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ExecutiveThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ExecutiveThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ExecutiveThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Flow:
                    this.ThemeName = SLConstants.FlowThemeName;
                    this.MajorLatinFont = SLConstants.FlowThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.FlowThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.FlowThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.FlowThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.FlowThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.FlowThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.FlowThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.FlowThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.FlowThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.FlowThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.FlowThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.FlowThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.FlowThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.FlowThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Foundry:
                    this.ThemeName = SLConstants.FoundryThemeName;
                    this.MajorLatinFont = SLConstants.FoundryThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.FoundryThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.FoundryThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.FoundryThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.FoundryThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.FoundryThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.FoundryThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.FoundryThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.FoundryThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.FoundryThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.FoundryThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.FoundryThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.FoundryThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.FoundryThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Grid:
                    this.ThemeName = SLConstants.GridThemeName;
                    this.MajorLatinFont = SLConstants.GridThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.GridThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.GridThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.GridThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.GridThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.GridThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.GridThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.GridThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.GridThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.GridThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.GridThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.GridThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.GridThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.GridThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Hardcover:
                    this.ThemeName = SLConstants.HardcoverThemeName;
                    this.MajorLatinFont = SLConstants.HardcoverThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.HardcoverThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.HardcoverThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.HardcoverThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.HardcoverThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.HardcoverThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.HardcoverThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.HardcoverThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.HardcoverThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.HardcoverThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.HardcoverThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.HardcoverThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.HardcoverThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.HardcoverThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Horizon:
                    this.ThemeName = SLConstants.HorizonThemeName;
                    this.MajorLatinFont = SLConstants.HorizonThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.HorizonThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.HorizonThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.HorizonThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.HorizonThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.HorizonThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.HorizonThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.HorizonThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.HorizonThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.HorizonThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.HorizonThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.HorizonThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.HorizonThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.HorizonThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Median:
                    this.ThemeName = SLConstants.MedianThemeName;
                    this.MajorLatinFont = SLConstants.MedianThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.MedianThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.MedianThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.MedianThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.MedianThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.MedianThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.MedianThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.MedianThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.MedianThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.MedianThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.MedianThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.MedianThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.MedianThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.MedianThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Metro:
                    this.ThemeName = SLConstants.MetroThemeName;
                    this.MajorLatinFont = SLConstants.MetroThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.MetroThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.MetroThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.MetroThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.MetroThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.MetroThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.MetroThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.MetroThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.MetroThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.MetroThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.MetroThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.MetroThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.MetroThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.MetroThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Module:
                    this.ThemeName = SLConstants.ModuleThemeName;
                    this.MajorLatinFont = SLConstants.ModuleThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ModuleThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ModuleThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ModuleThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ModuleThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ModuleThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ModuleThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ModuleThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ModuleThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ModuleThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ModuleThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ModuleThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ModuleThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ModuleThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Newsprint:
                    this.ThemeName = SLConstants.NewsprintThemeName;
                    this.MajorLatinFont = SLConstants.NewsprintThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.NewsprintThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.NewsprintThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.NewsprintThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.NewsprintThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.NewsprintThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.NewsprintThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.NewsprintThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.NewsprintThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.NewsprintThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.NewsprintThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.NewsprintThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.NewsprintThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.NewsprintThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Opulent:
                    this.ThemeName = SLConstants.OpulentThemeName;
                    this.MajorLatinFont = SLConstants.OpulentThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.OpulentThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.OpulentThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.OpulentThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.OpulentThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.OpulentThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.OpulentThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.OpulentThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.OpulentThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.OpulentThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.OpulentThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.OpulentThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.OpulentThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.OpulentThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Oriel:
                    this.ThemeName = SLConstants.OrielThemeName;
                    this.MajorLatinFont = SLConstants.OrielThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.OrielThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.OrielThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.OrielThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.OrielThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.OrielThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.OrielThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.OrielThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.OrielThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.OrielThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.OrielThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.OrielThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.OrielThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.OrielThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Origin:
                    this.ThemeName = SLConstants.OriginThemeName;
                    this.MajorLatinFont = SLConstants.OriginThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.OriginThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.OriginThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.OriginThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.OriginThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.OriginThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.OriginThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.OriginThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.OriginThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.OriginThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.OriginThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.OriginThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.OriginThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.OriginThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Paper:
                    this.ThemeName = SLConstants.PaperThemeName;
                    this.MajorLatinFont = SLConstants.PaperThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.PaperThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.PaperThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.PaperThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.PaperThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.PaperThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.PaperThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.PaperThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.PaperThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.PaperThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.PaperThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.PaperThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.PaperThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.PaperThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Perspective:
                    this.ThemeName = SLConstants.PerspectiveThemeName;
                    this.MajorLatinFont = SLConstants.PerspectiveThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.PerspectiveThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.PerspectiveThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.PerspectiveThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.PerspectiveThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.PerspectiveThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.PerspectiveThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.PerspectiveThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.PerspectiveThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.PerspectiveThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.PerspectiveThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.PerspectiveThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.PerspectiveThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.PerspectiveThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Pushpin:
                    this.ThemeName = SLConstants.PushpinThemeName;
                    this.MajorLatinFont = SLConstants.PushpinThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.PushpinThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.PushpinThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.PushpinThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.PushpinThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.PushpinThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.PushpinThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.PushpinThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.PushpinThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.PushpinThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.PushpinThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.PushpinThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.PushpinThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.PushpinThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Slipstream:
                    this.ThemeName = SLConstants.SlipstreamThemeName;
                    this.MajorLatinFont = SLConstants.SlipstreamThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.SlipstreamThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.SlipstreamThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.SlipstreamThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.SlipstreamThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.SlipstreamThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.SlipstreamThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.SlipstreamThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.SlipstreamThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.SlipstreamThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.SlipstreamThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.SlipstreamThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.SlipstreamThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.SlipstreamThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Solstice:
                    this.ThemeName = SLConstants.SolsticeThemeName;
                    this.MajorLatinFont = SLConstants.SolsticeThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.SolsticeThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.SolsticeThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.SolsticeThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.SolsticeThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.SolsticeThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.SolsticeThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.SolsticeThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.SolsticeThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.SolsticeThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.SolsticeThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.SolsticeThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.SolsticeThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.SolsticeThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Technic:
                    this.ThemeName = SLConstants.TechnicThemeName;
                    this.MajorLatinFont = SLConstants.TechnicThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.TechnicThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.TechnicThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.TechnicThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.TechnicThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.TechnicThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.TechnicThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.TechnicThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.TechnicThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.TechnicThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.TechnicThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.TechnicThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.TechnicThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.TechnicThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Thatch:
                    this.ThemeName = SLConstants.ThatchThemeName;
                    this.MajorLatinFont = SLConstants.ThatchThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ThatchThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ThatchThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ThatchThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ThatchThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ThatchThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ThatchThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ThatchThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ThatchThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ThatchThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ThatchThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ThatchThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ThatchThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ThatchThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Trek:
                    this.ThemeName = SLConstants.TrekThemeName;
                    this.MajorLatinFont = SLConstants.TrekThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.TrekThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.TrekThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.TrekThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.TrekThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.TrekThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.TrekThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.TrekThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.TrekThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.TrekThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.TrekThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.TrekThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.TrekThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.TrekThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Urban:
                    this.ThemeName = SLConstants.UrbanThemeName;
                    this.MajorLatinFont = SLConstants.UrbanThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.UrbanThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.UrbanThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.UrbanThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.UrbanThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.UrbanThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.UrbanThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.UrbanThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.UrbanThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.UrbanThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.UrbanThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.UrbanThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.UrbanThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.UrbanThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Verve:
                    this.ThemeName = SLConstants.VerveThemeName;
                    this.MajorLatinFont = SLConstants.VerveThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.VerveThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.VerveThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.VerveThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.VerveThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.VerveThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.VerveThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.VerveThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.VerveThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.VerveThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.VerveThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.VerveThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.VerveThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.VerveThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Waveform:
                    this.ThemeName = SLConstants.WaveformThemeName;
                    this.MajorLatinFont = SLConstants.WaveformThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.WaveformThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.WaveformThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.WaveformThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.WaveformThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.WaveformThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.WaveformThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.WaveformThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.WaveformThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.WaveformThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.WaveformThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.WaveformThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.WaveformThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.WaveformThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Autumn:
                    this.ThemeName = SLConstants.AutumnThemeName;
                    this.MajorLatinFont = SLConstants.AutumnThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.AutumnThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.AutumnThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.AutumnThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.AutumnThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.AutumnThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.AutumnThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.AutumnThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.AutumnThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.AutumnThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.AutumnThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.AutumnThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.AutumnThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.AutumnThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Decatur:
                    this.ThemeName = SLConstants.DecaturThemeName;
                    this.MajorLatinFont = SLConstants.DecaturThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.DecaturThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.DecaturThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.DecaturThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.DecaturThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.DecaturThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.DecaturThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.DecaturThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.DecaturThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.DecaturThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.DecaturThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.DecaturThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.DecaturThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.DecaturThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Kilter:
                    this.ThemeName = SLConstants.KilterThemeName;
                    this.MajorLatinFont = SLConstants.KilterThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.KilterThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.KilterThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.KilterThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.KilterThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.KilterThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.KilterThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.KilterThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.KilterThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.KilterThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.KilterThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.KilterThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.KilterThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.KilterThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Macro:
                    this.ThemeName = SLConstants.MacroThemeName;
                    this.MajorLatinFont = SLConstants.MacroThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.MacroThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.MacroThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.MacroThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.MacroThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.MacroThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.MacroThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.MacroThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.MacroThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.MacroThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.MacroThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.MacroThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.MacroThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.MacroThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Mylar:
                    this.ThemeName = SLConstants.MylarThemeName;
                    this.MajorLatinFont = SLConstants.MylarThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.MylarThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.MylarThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.MylarThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.MylarThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.MylarThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.MylarThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.MylarThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.MylarThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.MylarThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.MylarThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.MylarThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.MylarThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.MylarThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Sketchbook:
                    this.ThemeName = SLConstants.SketchbookThemeName;
                    this.MajorLatinFont = SLConstants.SketchbookThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.SketchbookThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.SketchbookThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.SketchbookThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.SketchbookThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.SketchbookThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.SketchbookThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.SketchbookThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.SketchbookThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.SketchbookThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.SketchbookThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.SketchbookThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.SketchbookThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.SketchbookThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Soho:
                    this.ThemeName = SLConstants.SohoThemeName;
                    this.MajorLatinFont = SLConstants.SohoThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.SohoThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.SohoThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.SohoThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.SohoThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.SohoThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.SohoThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.SohoThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.SohoThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.SohoThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.SohoThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.SohoThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.SohoThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.SohoThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Spring:
                    this.ThemeName = SLConstants.SpringThemeName;
                    this.MajorLatinFont = SLConstants.SpringThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.SpringThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.SpringThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.SpringThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.SpringThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.SpringThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.SpringThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.SpringThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.SpringThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.SpringThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.SpringThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.SpringThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.SpringThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.SpringThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Summer:
                    this.ThemeName = SLConstants.SummerThemeName;
                    this.MajorLatinFont = SLConstants.SummerThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.SummerThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.SummerThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.SummerThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.SummerThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.SummerThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.SummerThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.SummerThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.SummerThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.SummerThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.SummerThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.SummerThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.SummerThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.SummerThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Thermal:
                    this.ThemeName = SLConstants.ThermalThemeName;
                    this.MajorLatinFont = SLConstants.ThermalThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.ThermalThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.ThermalThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.ThermalThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.ThermalThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.ThermalThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.ThermalThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.ThermalThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.ThermalThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.ThermalThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.ThermalThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.ThermalThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.ThermalThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.ThermalThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Tradeshow:
                    this.ThemeName = SLConstants.TradeshowThemeName;
                    this.MajorLatinFont = SLConstants.TradeshowThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.TradeshowThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.TradeshowThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.TradeshowThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.TradeshowThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.TradeshowThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.TradeshowThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.TradeshowThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.TradeshowThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.TradeshowThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.TradeshowThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.TradeshowThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.TradeshowThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.TradeshowThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.UrbanPop:
                    this.ThemeName = SLConstants.UrbanPopThemeName;
                    this.MajorLatinFont = SLConstants.UrbanPopThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.UrbanPopThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.UrbanPopThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.UrbanPopThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.UrbanPopThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.UrbanPopThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.UrbanPopThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.UrbanPopThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.UrbanPopThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.UrbanPopThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.UrbanPopThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.UrbanPopThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.UrbanPopThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.UrbanPopThemeFollowedHyperlinkColor);
                    break;
                case SLThemeTypeValues.Winter:
                    this.ThemeName = SLConstants.WinterThemeName;
                    this.MajorLatinFont = SLConstants.WinterThemeMajorLatinFont;
                    this.MinorLatinFont = SLConstants.WinterThemeMinorLatinFont;
                    this.Dark1Color = SLTool.ToColor(SLConstants.WinterThemeDark1Color);
                    this.Light1Color = SLTool.ToColor(SLConstants.WinterThemeLight1Color);
                    this.Dark2Color = SLTool.ToColor(SLConstants.WinterThemeDark2Color);
                    this.Light2Color = SLTool.ToColor(SLConstants.WinterThemeLight2Color);
                    this.Accent1Color = SLTool.ToColor(SLConstants.WinterThemeAccent1Color);
                    this.Accent2Color = SLTool.ToColor(SLConstants.WinterThemeAccent2Color);
                    this.Accent3Color = SLTool.ToColor(SLConstants.WinterThemeAccent3Color);
                    this.Accent4Color = SLTool.ToColor(SLConstants.WinterThemeAccent4Color);
                    this.Accent5Color = SLTool.ToColor(SLConstants.WinterThemeAccent5Color);
                    this.Accent6Color = SLTool.ToColor(SLConstants.WinterThemeAccent6Color);
                    this.Hyperlink = SLTool.ToColor(SLConstants.WinterThemeHyperlink);
                    this.FollowedHyperlinkColor = SLTool.ToColor(SLConstants.WinterThemeFollowedHyperlinkColor);
                    break;
            }
        }
    }
}
