   M S B A M L   `   `   `  ����T  PAvalonDock, Version=1.2.2702.0, Culture=neutral, PublicKeyToken=85a1e0ada7ec13e4'clr-namespace:AvalonDock
AvalonDock  5      \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35  �� MergedDictionaries �� Source   DockableContentTabItemStyle �� 
TargetType     AvalonDock.DockableContent �� Property �� Value �� 
TargetType J� SnapsToDevicePixels ��Name - )ManagedContentTabControlNormalBorderBrush �� CornerRadius ) %ManagedContentTabItemNormalBackground	 �� Padding
 � ColumnDefinitions   AvalonDock.AlignedImage ��Name ��Name   AvalonDock.ManagedContent   Title �� Triggers P� Property P� Value , (ManagedContentTabItemSelectedBorderBrush �� 
TargetName + 'ManagedContentTabItemSelectedBackground ]� 
Conditions �� Property �� Value ' #ManagedContentTabItemHotBorderBrush & "ManagedContentTabItemHotBackground , (ManagedContentTabItemDisabledBorderBrush + 'ManagedContentTabItemDisabledBackground   AvalonDock.DockablePane 	 ad:DockingManager /
 +ad:AvalonDockBrushes.DefaultBackgroundBrush +� FocusedElement �� ElementName � RowDefinitions 4 0ad:AvalonDockBrushes.DockablePaneTitleBackground ��Name  PaneHeaderCommandStyle % !PaneHeaderContextMenuCommandStyle �� RelativeSource �� Path 4 0ad:AvalonDockBrushes.DockablePaneTitleForeground-  'System.Windows.Input.KeyboardNavigation   TabNavigation   DirectionalNavigation   TabIndex!   AvalonDock.DockableTabPanel  Name K� IsItemsHost   AvalonDock.PaneTabPanel    TabItemStyle! �� Value" �� Binding < 8ad:AvalonDockBrushes.DockablePaneTitleBackgroundSelected < 8ad:AvalonDockBrushes.DockablePaneTitleForegroundSelected   AvalonDock.Pane# �� Angle  DocumentTabItemStyle$ � ContextMenu & "ad:ContextMenuElement.DocumentPane% F�Name 1 -ad:AvalonDockBrushes.DocumentHeaderBackground& �� Orientation 1 -ad:AvalonDockBrushes.DocumentHeaderForeground'  Name( ��Name) � Cursor* J� Opacity+ �� Stretch, �� Color 9 5ad:AvalonDockBrushes.DocumentHeaderBackgroundSelected - )ad:AvalonDockBrushes.DocumentHeaderBorder- �� 
SourceName : 6ad:AvalonDockBrushes.DocumentHeaderBackgroundMouseOver   AvalonDock.DocumentPane. � 	Resources  Key/ �� 	StaysOpen0 �� Self1 �� Opacity . *ad:AvalonDockBrushes.DocumentHeaderBorder22 ��Name!	   AvalonDock.DocumentTabPanel3 	 Name4 K� ZIndex5 :� RoutedEvent6 �� TargetProperty7 W� From	8 W� To9 g� Duration&
    AvalonDock.DockablePaneAnchorTab: j� Stretch; !� Visual   AvalonDock.AnchorStyle , (ManagedContentTabItemInvNormalBackground ) %ManagedContentTabItemInvHotBackground+   %AvalonDock.DockablePaneAnchorTabGroup   AvalonDock.OverlayWindow< �Name�� 5      F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     8x,http://schemas.microsoft.com/winfx/2006/xaml    5      adclr-namespace:AvalonDock  5        5      �� 5      $/ )/AvalonDock;component/themes/generic.xaml?�6   �� 5      $@ :/AvalonDock;component/themes/aero.normalcolor.brushes.xaml?�6   %rE  &	        7  7  7  7  7  7  7  7  '  Z    7  7  7  7   &	 z    7  7  7  '  N    7  7  '
  �+    7  7  7  7  7  7  7  7  '  d2    '  �2    �� 5      "   6-   .�5      ��  � �� 5      �� 5      "   6   .R�5      �  . .+�5      �� $ PART_DragArea��5      8��  $��1,0,1,1q�5      $��0,2,-1,0q�5      $ 0��5      8�� $	 1q�5      ._�5!   	   � $��4,0,4,0q�6   
 5"   
   �� 5#      $
��Auto �6   �� 5$      $��* �6   �� 5%   &   $
��Auto �67   .+�5'   
     ._�5(      �� $
 Icon��6   $��0��5)      $��1q�5*      $��Center=�5+      $
��Icon��5,      # z 5-      5.      �� 5/   
   $ tabItemTitle��50      $��1��51   )   $��CharacterEllipsis=�52      $��NoWrap=�62   #��z 53      $��2,0,0,0q�54      $��Center=�6"   #��� ��55      56      58   	   59       5;      P� 5<       �@��
IsSelected . . �5>   	   �� 8  $ PART_DragArea��5?   L    � ���� 5@   	   8  $ PART_DragArea��5A   K    � ���� 5B   	   # � ��5C   
   $ tabItemTitle��6P    � ���� 5D   	   $ PART_DragArea��5E   7    � ��$ 	0,1,-1,-2q�6%   67   �� 5F   	   $ PART_DragArea��5G   /    �@��Padding$ 2q�6%   6/   �� 5H   	    �@K�ZIndex$	 100��5I   %   5J   	   ]� 5K       5L   	   �� 5M   
    �@J�IsMouseOver . 6+   �� 5N   
    �@��
IsSelected .  63   ./�5P   	   �� 8  $ PART_DragArea��5Q   %    � ���� 5R   	   8  $ PART_DragArea��5S   %    � ���� 5T   	    �@K�ZIndex$ 99��6(   5U   	   P� 5V       � {� .  . �5W   	   �� 8  $ PART_DragArea��5X   %    � ���� 5Y   	   8  $ PART_DragArea��5Z   %    � ���� 5[   	   # � ��6&   $ tabItemTitle��5\   %    � ���� 5]   	   $
 Icon��6    � {� .  67   5^   	   P� 5_       5`      Z�  �@ Icon. �5a   	   �� $
 Icon��6    � y�$ 	Collapsed=�5c      5d   	   5f      5h      5i      �� 5l      "  6*   .�5m      ��  � �� 6!   C� *�� *M� *3	 +�� *3
 +++�� 5n   
    � ��$ 0,4,0,0q�6#   �� 5o       � �� 5p      �� 5q      "  6   .R�5r      �� |�. #��z��5t      ._�5v      �  6   �� $ PART_SelectedContent�� 5w   	   �� 5x   
   $
��Auto �6   �� 5y   
   $��* �6   �� 5z   
   $
��Auto �6   .+�5|   	   �� $ PART_Header��6   $��0��5}      |�.  $��1,1,1,0q�5      ���#e1e6ff$��18��5�   %   ��5~      C� *�� *M� *3	 +�� *3 +++._�5�   
   ]� ��. .+�5�      �� $ btnPinAutoHide��6   $��Right=�6*   8��  $+��%ad:DockablePane.ToggleAutoHideCommand��6q   $��15��5�   1   $��15��6<   $��Center=�6H   .��5�      �� $<��6/AvalonDock;component/resources/Images/PinAutoHide.png��6   $��13��6R   $��13��6]   $
��None=�6i   5�      �� 5�      $  PART_ShowContextMenuButton��6   $��Right=�67   8�� $(��"ad:DockablePane.ShowOptionsCommand��6�   $��15��5�   1   $��15��6<   $��Center=�6H   .��5�      �� $8��2/AvalonDock;component/resources/Images/PinMenu.png��6   $��13��6N   $��13��6Y   $
��None=�6e   5�      �� 5�      $ 	paneTitle��5�      $��0��5�      $��2,0,0,0q�5�      $��CharacterEllipsis=�6#   $��NoWrap=�6D   $��Center=�5�   -   ��5�      ��  �� *TemplatedParent+$ SelectedItem.Title���5�      C� *�� *M� *3	 +�� *3 +++5�      5�   
   �� 5�   	   $��1��6   $��1,0,1,1q�5�      ���#e1e6ff��5�   )   ��  �� *TemplatedParent+$ SelectedItem.Background�._�5�   
   �� $ PART_SelectedContent��5�   )   $ Local=�5�      $ 	Contained=�5�      $ 1��5�      ��5�   )   ��  �� *TemplatedParent+$ SelectedItem.Content�5�      5�   
   �� 5�   	   $ 	PART_Tabs��6   $��2��5�   )   $��0,2,0,0q�5�      $��23��5�   )   $��0,1,0,0q�5�      8�� 5�   
     5�   	   $ paneTabsPanel��5�   )   $��2��5�   )   $��22��5�   )   $ 2��5�   )    . $��0,0,0,2q�5�   )   8   5�   )   5�   	   5�       5�      �� 5�      $
! True��6p   " 6   ��  �� *Self+$" SelectedItem.IsActiveContent�.a�5�   	   �� $ PART_Header��6�    6&   C� *�� *M� *3	 +�� *3 +++ � ��6�   �� 5�   	   $ 	paneTitle��6�    6&   C� *�� *M� *3	 +�� *3 +++ � ��6�   5�   	   P� 5�       �@ 
ShowHeader .  . �5�   	   �� $ PART_Header��68    � y�$ 	Collapsed=�6&   68   5�   	   P� 5�       �@ ShowTabs .  . �5�   	   �� $ 	PART_Tabs��68    � y�$ 	Collapsed=�6&   68   5�   	   P� 5�       �@ HasSingleItem . . �5�   	   �� $ 	PART_Tabs��68    � y�$ 	Collapsed=�6&   68   �� 5�   	   $ paneTabsPanel��68    � y�$ 	Collapsed=�6&   68   5�   	   �� 5�      $! AutoHide��6g   " 6   ��  �� *Self+$ SelectedItem.State�.a�5�   	   �� $ btnPinAutoHide��6+    �@�LayoutTransform 5�   
   �� 5�      $# 90Q�6   5�   
   5�   	   5�      5�      5�      �� 5�      "  6&   .�5�      ��  � ��
 ���� 6!   �� 5�       � �� 5�      �� 5�      "  6   .R�5�      �� $ PART_DragArea��5�      8��  $��	-10,0,0,0q�5�       . $ 5�      C� *�� *M� *3	 +�� *3 +++._�5�      � $��0,0,0,0q�6   
 5�   	   �� 5�   
   $��20 �6   �� 5�   
   $
��Auto �6   .+�5�   	   F� $% tabItemIntPathBackground��5�   !   ;���� -1 � � $�  � ��� ��� �@KL ��)��)� -1��)	��5�   #   C� *�� *M� *3	 +�� *3 +++F� 5�   	   $% tabItemIntPathBorder��5�   !   8�� 2���� -1 � � $�  � ��� ��� �@KL ��)��)	5�   !   �� 5�   	   $ tabItemIntBorder��5�   %   $��1��5�   %   $��0,1,1,0q�5�   %   $��
-0.5,0,0,0q�5�   %   $ 0,3,0,0��5�   %   8�� ��5�   %   C� *�� *M� *3	 +�� *3 +++._�5�   
   �� $& 
Horizontal=�6   $��5,0,4,0q�5�   -   .+�5�      �� $ tabItemTitle��5�      $��CharacterEllipsis=�5�      $��NoWrap=�5�   -   #��z 5�      $��0,1,0,0q�5�   -   ��5      C� *�� *M� *3	 +�� *3 +++  5     $' PART_LockedIcon��6   $��2,0,0,0q�64   $y�	Collapsed=�6E   $��Center=�6\   $��Center=�6w   ._�5     �� $7��1/AvalonDock;component/resources/Images/Locked.png��6   $��6��6M   $��8��6W   $��Uniform=�6b   5     �� 5  &   $( CloseButton��6R   $��2��6-   $��8��6=   $��8��6G   $��4,0,0,0q�6g   $
) Hand��6x   $��Center=�6�   .+�5  *   �� 
���  ��$* 0Q�6?   $��8��6K   $��8��6U   F� 5  *   $��8��6/   $��8��6:   $+ Uniform=�6D   ����и	x�>���:H�� 7`
��n�H���0��η?x�>�047
�	��0��η?�>�Qn�ʡE���?� 7�;
Q��:ʡE���?Ѹ	�>���9��	Ѹ	x�>	��5  .   �� 5  2   $, 	#FF333333��6B   5
  +   5  '   5  #   5  
   5  	   5      5     P� 5      �@��
IsSelected . . �5  	   �� $ tabItemIntBorder��5  %    5  %   C� *�� *M� *3	 +�� *3 +++ � ���� 5  	   $ tabItemIntPathBackground��5  %    5  %   C� *�� *M� *3	 +�� *3 +++ � ���� 5  	   $ tabItemIntBorder��6'    6E   C� *�� *M� *3	 +�� *3 +++ � ���� 5  	   $ tabItemIntPathBorder��6"    6D   C� *�� *M� *3	 +�� *3 +++ � ��5  	   �� 5     $
! True��6d   " 6   �� $ IsActiveDocument� �� *Self+.a�5  	   �� $ tabItemTitle��60    � ��$
 Bold"�6J   5   	   ]� 5!      5"  	   �� 5#  
   $- tabItemIntBorder��6+    �@J�IsMouseOver . 6I   �� 5$  
    �@��
IsSelected .  63   ./�5&  	   �� $ tabItemIntBorder��5'  %    5(  %   C� *�� *M� *3	 +�� *3 +++ � ���� 5*  	   $ tabItemIntPathBackground��5+  %    5,  %   C� *�� *M� *3	 +�� *3 +++ � ��5.  	   �� 50     $
! True��6\   " 6   �� $ IsLocked� �� *Self+.a�51  	   �� $ PART_LockedIcon��66    � y�$ Visible=�6&   66   52  	   54     56     57     �� 5:     "  6	   .�5;     ��  � �� 6!   C� *�� *M� *3	 +�� *3
 +++�� 5<      �@�FocusVisualStyle 6'   Z� �� 5=      � �� 5>     �� 5?     "  6   . 5@     ���� 5A     DocumentsListMenu / . ��5C  	   �� 5D  
   " m�6   .�5E     ��  �@m�CommandParameter 6.   �� # Z0 $ Header��� 5F      �@m�Command$- 'ad:DocumentPane.ActivateDocumentCommand��6%   �� 5G      � �� 5H     �� 5I     " m�6   .R�5J     �� $ 	intBorder��6   $��1q�6(   #��z��6<   $ 2��6f   ._�5K     � 
 5L     �� 5M     $
��Auto �6"   $��24��6/   �� 5N     $��* �6"   .+�5P     �� $
 Icon��6!   $��2q�6/   $��0��6X   $��Center=�6h   ��6:   �� $
 Icon�6h   �� 5Q     $ intMenuTitle��6   $��5,2,20,2q�60   $��1��6^   $��Center=�6n   ��6B   �� $ Title�6n   5R     5S      5T     P� 5U      �@J�IsMouseOver . . �5V     �� $ 	intBorder��6-    � �� 5W     �� 5X     #, � ��6"   $	1 0.3Q�6j   5Z     �� 5[     $ 	intBorder��6.   # � ��6E    � ��5\     �� 5]     $
! True��6E   " 6   �� $ IsActiveDocument�.a�5^     �� $ intMenuTitle��6:    � ��$
 Bold"�6-   6:   5_     5g     5i     5j     5l  	   5m     .R�5n     �� |�.  #��z��5p     ._�5r     �  5s  	   �� 5t  
   $��20 �6   �� 5u  
   $��* �6   .+�5w  	   � $��1��6   $��0,-1,0,0q�6   .+�5x  
   �� $��5q�5y  %   $ 3��5{  %   |�.  ��5z  %   C� *�� *M� *3	 +�� *3 +++5}     �� 5~  
   $��2q�6   $ 3��5�  )   $��1q�5�  )   |�.  ��5  )   C� *�� *M� *3	 +�� *3 +++��5�  )   ��  �� *TemplatedParent+$ SelectedItem.Background�._�5�     �� $��2q�5�  )   $ Local=�5�     $ 	Contained=�5�     $ 1��5�     ��5�  )   ��  �� *TemplatedParent+$ SelectedItem.Content�5�     5�     5�  
   �� 5�  	   $ PART_Header��6   $��0��5�     |�.  $��1,1,1,0q�5�     ._�5�  
   ]� ��. .+�5�     �� $  PART_ShowContextMenuButton��6   $��Right=�66   $��15��6M   $��15��6X   8��  $2��,ad:DocumentPane.ShowDocumentsListMenuCommand��6�   .��5�     �� $2 ShowContextMenuIcon��6   $8��2/AvalonDock;component/resources/Images/PinMenu.png��6/   $��13��6k   $��13��6v   $
��None=�6�   5�     	  5�     $3 paneTabsPanel��5�  '   $4 1��5�  '   $ 2��5�  '    . $��10,2,0,0q�5�     8   5�     5�     5�  
   5�  	   5�      5�     P� 5�      �@��HasItems .  . �5�  	   ��  � y�$ Hidden=�6&   5�  	   �� 5�     $
! True��6f   " 6   �� $ IsMainDocumentPane� �� *Self+.a�5�  	   �� $ ShowContextMenuIcon��6`    � ��$; 5/AvalonDock;component/resources/Images\PinDocMenu.png��6"   6`   5�  	   :� 5�     $5 Window.Loaded��6   .[�5�  	   �� .��5�  
   �� .��5�     W� $6 Opacity�5�  )   $7 0X�5�  )   $8 1X�62   $9 	0:0:0.200D�6:   5�     5�  
   5�  	   5�     5�     5�     �� 5�     " 
 6	   .�5�     ��  � �� 5�     �� 5�     " 
 6   .R�5�     � .+�5�     �� $��PART_Border��5�     $��1,0,1,1q�5�     8��  8�� ._�5�  	   � $��3,0,3,0q�6   
 5�  
   �� 5�     $
��Auto �6   �� 5�     $��* �6   .+�5�  
   �� $��0��6   $��16��6!   $��16��6,   ��5�     !� 5�     $
: None=�6   ; 6'   �� $ ReferencedContent.Icon� �� *TemplatedParent+5�     �� 5�  
   $��1��5�  %   #��� ��5�  %   $��4,2,2,2q�5�  %   ��5�  %   �� $ ReferencedContent.Title� �� *TemplatedParent+5�  %   5�  
   5�  	   5�      5�     P� 5�      �@
 Anchor 5�  	    5�  
   Left=�5�     . �5�  	   �� $ PART_Border��6   8   � ��6?   �� 5�  	   $ PART_Border��6    � ��$ 1,1,1,0q�6D   5�  	   P� 5�      �@
 Anchor 5�  	    5�  
   
Bottom=�5�     . �5�  	   �� $ PART_Border��6   8   � ��6?   �� 5�  	   $ PART_Border��6    � ��$ 1,1,1,0q�6D   5�  	   P� 5�      �@J�IsMouseOver . . �5�  	   �� 8  $ PART_Border��5�  %    � ���� 5�  	   8  $ PART_Border��5�  %    � ��5�  	   ]� 5�      5�  	   �� 5�  
    �@J�IsMouseOver . 6+   �� 5�  
    �@
 Anchor 5�      5�     Top=�5�     5�     ./�5   	   �� $ PART_Border��6   8   � ��6?   5  	   ]� 5      5  	   �� 5  
    �@J�IsMouseOver . 6+   �� 5  
    �@
 Anchor 5      5     	Right=�5	     5     ./�5  	   �� $ PART_Border��6   8   � ��6?   5  	   5     5     5     �� 5     "  6	   .�5     ��  �@��Orientation$ 
Horizontal=�6"   �� 5      � ��$ 7,0,0,0q�6   5     �� 5     "  6	   .�5     ��  � ��
 ���� 6!   �� 5      � |� .  6    �� 5       � �� 5!     �� 5"     "  6   .R�5#     �  . $	* 0.9Q�6'   .+�5$     �� .+�5%  	   �� $ PART_SelectionBox��6   #��� ��6+   
�������$��3q�6�   $	* 0.2Q�6�   $y�Hidden=�6�   5&  	   � 5'     $< 	panelDrag��6   .+�5(  	     $��PART_btnDockLeft��6   $��Center=�61   $
��Left=�6L   ._�5)  
   �� $2 IMG_DockLeft��6   $B��</AvalonDock;component/resources/Images/Aero/AeroDockLeft.PNG��6&   $��Uniform=�6l   $��32��6~   $��31��6�   5*  
     5+  	   $��PART_btnDockRight��6   $��Center=�62   $��Right=�6M   ._�5,  
   �� $2 IMG_DockRight��6   $C��=/AvalonDock;component/resources/Images/Aero/AeroDockRight.PNG��6'   $��Uniform=�6n   $��32��6�   $��31��6�   5-  
     5.  	   $��PART_btnDockBottom��6   $��Bottom=�63   $��Center=�6N   ._�5/  
   �� $2 IMG_DockBottom��6   $D��>/AvalonDock;component/resources/Images/Aero/AeroDockBottom.PNG��6(   $��Uniform=�6p   $��31��6�   $��32��6�   50  
     51  	   $��PART_btnDockTop��6   $	��Top=�60   $��Center=�6H   ._�52  
   �� $2 IMG_DockTop��6   $A��;/AvalonDock;component/resources/Images/Aero/AeroDockTop.PNG��6%   $��Uniform=�6j   $��31��6|   $��32��6�   53  
   54  	   �� 55     .+�56  	   � $)��#PART_gridPaneRelativeDockingOptions��6   $y�	Collapsed=�69   $	��103��6P   $	��101��6\   
 57  
   �� 58     �� 59     �� 5:      5<  
   �� 5=     �� 5>     �� 5?     .+�5A  
     $��3��6   $��3��6.   ._�5B     �� $2 IMG_DockPane��6   $B��</AvalonDock;component/resources/Images/Aero/AeroDockPane.png��6'   $��Uniform=�6m   5C     �� 5D  
   $��PART_btnDockPaneTop��6   $��1��6,   $��0��6<   �� 5E  
   $��PART_btnDockPaneRight��6   $��2��6.   $��1��6>   �� 5F  
   $��PART_btnDockPaneBottom��6   $��1��6/   $��2��6?   �� 5G  
   $��PART_btnDockPaneLeft��6   $��0��6-   $��1��6=   �� 5H  
   $��PART_btnDockPaneInto��6   $��1��6-   $��1��6=   5I  
   5J  	   5K      5L     �� 5M     $! DropBorderLeft��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5N  	   �� $ IMG_DockLeft��6l    � ��$G A/AvalonDock;component/resources/Images/Aero/AeroDockLeftHover.PNG��6"   6l   5O  	   �� 5P     $! DropBorderRight��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5Q  	   �� $ IMG_DockRight��6m    � ��$H B/AvalonDock;component/resources/Images/Aero/AeroDockRightHover.PNG��6"   6m   5R  	   �� 5S     $! DropBorderTop��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5T  	   �� $ IMG_DockTop��6k    � ��$F @/AvalonDock;component/resources/Images/Aero/AeroDockTopHover.PNG��6"   6k   5U  	   �� 5V     $! DropBorderBottom��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5W  	   �� $ IMG_DockBottom��6n    � ��$I C/AvalonDock;component/resources/Images/Aero/AeroDockBottomHover.PNG��6"   6n   5X  	   �� 5Z     $! DropPaneInto��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5[  	   �� $ IMG_DockPane��6k    � ��$F @/AvalonDock;component/resources/Images/Aero/AeroDockPaneInto.PNG��6"   6k   5\  	   �� 5^     $! DropPaneLeft��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5_  	   �� $ IMG_DockPane��6k    � ��$F @/AvalonDock;component/resources/Images/Aero/AeroDockPaneLeft.PNG��6"   6k   5`  	   �� 5a     $! DropPaneRight��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5b  	   �� $ IMG_DockPane��6l    � ��$G A/AvalonDock;component/resources/Images/Aero/AeroDockPaneRight.PNG��6"   6l   5c  	   �� 5d     $! DropPaneTop��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5e  	   �� $ IMG_DockPane��6j    � ��$E ?/AvalonDock;component/resources/Images/Aero/AeroDockPaneTop.PNG��6"   6j   5f  	   �� 5g     $! DropPaneBottom��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5h  	   �� $ IMG_DockPane��6m    � ��$H B/AvalonDock;component/resources/Images/Aero/AeroDockPaneBottom.PNG��6"   6m   5i  	   �� 5k     $
! None��6f   " 6   �� $ OverlayButtonHover� �� *Self+.a�5l  	   �� $ IMG_DockPane��6g    � ��$B </AvalonDock;component/resources/Images/Aero/AeroDockPane.PNG��6"   6g   5m  	   5q     5s     5u     5x     