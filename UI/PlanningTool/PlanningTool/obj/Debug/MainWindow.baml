   M S B A M L   `   `   `  ����G  CPlanningTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:PlanningToolPlanningTool  5   	   [2clr-namespace:TreeViewWithViewModelDemo.TextSearch$TreeViewWithViewModelDemo.TextSearch  5   	       PlanningTool.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:PlanningTool     5   	   Dbasic2clr-namespace:TreeViewWithViewModelDemo.TextSearch     5   	   -   5          � Title$  
MainWindow��5	   	   $	��500��6   $
��1000��6)    � 	Resources 5
      ,   &PlanningTool.BoolToVisibilityConverter   	boolToVis �� 
TargetType  GridBlockStyle �� Property �� Value �� Path �� RelativeSource �� AncestorType �� 	Converter	 �� ConverterParameter  GridEditStyle��%�  &	        &	      7   &	 �     7     5   
   6*   �� 5   
   " ��6   .�5      ��  � ��$ Center=�62   �� 5       � y� 5      �� $ 
IsSelected� �� *FindAncestor+" ��8   $	 False��5      �� 5   
   " �6   .�5      ��  � ��$ Center=�62   �� 5       � y� 5      �� $ 
IsSelected� �� *FindAncestor+" ��8   $
	 True��5!      5#      .��5$      ]� .+�5%   
   ]� $	��Top=�6   .+�5&      �� $	��Top=�6   .+�5'      o� .@�5(      m� $��_File��6   
 � DataContext$

 this��6*   .@�5)      m� -   5        $
��_New��5)      6-   m� 5*      -   5        $��_Open��5*      6.   m� 5+      -   5        $��_Save��5+      6.   m� 5,      -   5        $��Save _As��5,      6M   �� 5-      m� 5.      -   5        $��_Exit��5.      6.   5/      m� 50      $��_Edit��6   .@�51      m� -   5        $��_Create AMP Test data��51   #   6B   m� 52      -   5        $��_Remove all outputs��52   #   6@   m� 53      -	   5        $"��_Create calibration Template��53   #   6I   54      55      c� 56      .@�57      �� -
   5        $	��Run��57       � ToolTip$ Run the simulation��6-   6J   58      5;      ]� 5<      $��Stretch=�6   ��. .+�5=      �� $
��Left=�6   $
��Auto��62   $
��Left=�6?   	Address :6e   �� 5>      $��Right=�6   $��Right=�61   $��Right=�6M   .��6p   	Go to >6y   �� 5?      -   5         ��Name$ 
AddressBox��5?      $��Stretch=�6.   $��Stretch=�6L   $	��200��6r    �� MaxLines$ 1��6�    �� TextWrapping$
 Wrap=�6�   .��6�   Simulation address goes here6�   5@      5A      � 5B   
   $��Stretch=�6   $��Stretch=�6-   $	��300��6I   $	��400��6X    � ColumnDefinitions 5D      �� 5E      $
��125* �6#   �� 5F      $
��204* �6#   �� 5G      $
��Auto �6#   �� 5H      $
��657* �6#   .+�5N      �� 
�������$��1q�6(   $��2q�6<   $��2��6G   ._�5O      @   :TreeViewWithViewModelDemo.TextSearch.TextSearchDemoControl  -   5          Name$ TreeviewControl��5O   .   $
��Left=�6H   5P      �� 5Q      $��2��6    �� ResizeBehavior$ PreviousAndNext=�6%   $��2��6F   �� 5R      
�������$��1q�6(   $��2��6<   $��2q�6L   �� 5U      -   5         ��Name$ listView��5U      $��3��6R   $��Stretch=�6b   $��Stretch=�6�   $��Stretch=�6�   $��Stretch=�6�   $
��Auto��6�   $	��442��6�   $
��Bold"�6  ��6*   �� $ 
Parameters���5V      �� 5W      " ��6   .�5X      -   5        5Y       �� View 5\      �� 5]      .P�5^      ��  �� Width$	 200��6)   $��	Parameter��65    �� CellTemplate 5_      �� 5`   "   .R�5a   &   � .+�5b   *   �� ��64   �� $
 Name�5f   '   5g   #   5i      �� 5j      $	 200��6)   $��Value��65    5k      �� 5l   "   .R�5m   &   � .+�5n   *   �� #��[ 5o   "   ��5n   4   �� $ Value�5o   "   �� 5p   *   #��[ 5q       ��5p   2   �� $ Value�5q       5r   '   5s   #   5u      5v      5x      5z      5{      5|      